Public Class Decoder3
    Private p_synth_dtbl(511) As Double

    Private p_pretab(20) As Integer
    Private v_vec(1, 1023) As Double ' 2 -- one for each channel
    'Private store(1, 31, 17) As Double ' stored samples used in hybrid synthesis
    Private store()()() As Double
    Private CS(7) As Double
    Private CA(7) As Double
    Private CI(7) As Double ' Used in antialiasing

    ' Speed it up!
    Public nChannels As Short
    Private re(575) As Double
    Private sfreq As Integer
    Private rawout(35) As Double
    'Private g_synth_n_win(63, 31) As Double
    Private g_synth_n_win()() As Double
    Private u_vec(511) As Double

    Public outBuffer(576 * 2 - 1) As Integer

    Private bitstream3 As Bitstream3
    Private audio3 As Audio3

    Public Sub New()
        Etc3.InitArray(Of Double)(g_synth_n_win, 63, 31)
        Etc3.InitArray(Of Double)(store, 1, 31, 17)
    End Sub

    ' This function is used to reinit the decoder before playing a new song, or when seeking
    ' inside the current song.
    Public Sub Init(ByVal bitstream3 As Bitstream3, ByVal audio3 As Audio3)
        Me.bitstream3 = bitstream3
        Me.audio3 = audio3

        bitstream3.g_main_data_top = 0 ' Clear bit reservoir

        ' Setup the v_vec intermediate vector
        'Array.Clear(store, 0, store.Length)

        ' The rest only needs to be done once.
        Static ready As Boolean
        If ready Then Exit Sub
        ready = True

        Dim i As Integer
        Dim c As Double

        For i = 0 To 20
            p_pretab(i) = CInt(Choose(i + 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3, 3, 3, 2))
        Next

        For i = 0 To 7
            CI(i) = CDbl(Choose(i + 1, -0.6, -0.535, -0.33, -0.185, -0.095, -0.041, -0.0142, -0.0037))
            c = Math.Sqrt(1.0 + CI(i) ^ 2)
            CS(i) = 1.0 / c
            CA(i) = CI(i) / c
        Next

        Dim st() As Double = Array.ConvertAll(My.Resources.synth_table.Split(vbCrLf.Substring(0, 1).ToCharArray), New Converter(Of String, Double)(AddressOf Str2Sng))
        For i = 0 To 511
            p_synth_dtbl(i) = st(i)
        Next

        ' Number of channels (1 for mono and 2 for stereo)
        If bitstream3.g_frame_header.mode = Etc3.t_mpeg1_mode.mpeg1_mode_double_channel Then
            nChannels = 1 - 1
        Else
            nChannels = 2 - 1
        End If

        ' Setup sampling frequency index
        sfreq = bitstream3.g_frame_header.sampling_frequency
    End Sub

    Private Function Str2Sng(ByVal s As String) As Double
        Return CDbl(Val(s))
    End Function

    ' Description: This function decodes a layer 3 bitstream into audio samples.
    ' Parameters: Outdata vector.
    ' Return value: OK or ERROR if the frame contains errors.
    Function MPG_Decode_L3() As Etc3.STATUS
        Dim offset As Integer
        Dim ch As Integer
        For gr As Integer = 0 To 1
            For ch = 0 To nChannels
                ' Requantize samples
                MPG_L3_Requantize(gr, ch)

                ' Reorder short blocks
                MPG_L3_Reorder(gr, ch)
            Next

            ' Stereo processing
            MPG_L3_Stereo(gr)

            For ch = 0 To nChannels
                ' Antialias
                MPG_L3_Antialias(gr, ch)

                ' Hybrid synthesis (IMDCT ,  windowing ,  overlap add)
                MPG_L3_Hybrid_Synthesis(gr, ch)

                ' Frequency inversion
                MPG_L3_Frequency_Inversion(gr, ch)

                ' Polyphase subband synthesis
                MPG_L3_Subband_Synthesis(gr, ch)
            Next

            If audio3.IsInit Then
                offset = 2 * (outBuffer.Length * gr) + audio3.WriteBufferPosition
                For i As Integer = 0 To outBuffer.Length - 1
                    Array.Copy(System.BitConverter.GetBytes(outBuffer(i)), 0, audio3.Buffer, 2 * i + offset, 2)
                Next
            End If
        Next

        Return Etc3.STATUS.sOK
    End Function

    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_L3_Requantize(ByVal gr As Integer, ByVal ch As Integer)
        Dim sfb As Integer = 0 ' scalefac band index
        Dim next_sfb As Integer ' frequency of next sfb
        Dim win_len As Integer
        Dim i As Integer = 0

        ' Determine type of block to process
        If (bitstream3.g_side_info.win_switch_flag(gr)(ch) = 1) AndAlso (bitstream3.g_side_info.block_type(gr)(ch) = 2) Then ' Short blocks

            ' Check if the first two subbands
            ' * (=2*18 samples = 8 long or 3 short sfb's) uses long blocks
            If bitstream3.g_side_info.mixed_block_flag(gr)(ch) <> 0 Then ' 2 longbl. sb  first

                ' First process the 2 long block subbands at the start
                next_sfb = bitstream3.g_sf_band_indices(sfreq).L(sfb + 1)
                For i = 0 To 35
                    If i = next_sfb Then
                        sfb += 1
                        next_sfb = bitstream3.g_sf_band_indices(sfreq).L(sfb + 1)
                    End If
                    MPG_Requantize_Process_Long(gr, ch, i, sfb)
                Next

                ' And next the remaining ,  non-zero ,  bands which uses short blocks
                sfb = 3
                next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)

                i = 36
                Do While (i < bitstream3.g_side_info.count1(gr)(ch))
                    ' Check if we're into the next scalefac band
                    If i = next_sfb Then ' Yes
                        sfb = sfb + 1
                        next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                        win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)
                    End If

                    For win As Integer = 0 To 2
                        For j As Integer = 0 To win_len - 1
                            MPG_Requantize_Process_Short(gr, ch, i, sfb, win)
                            i += 1
                        Next
                    Next
                Loop
            Else ' Only short blocks
                next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)

                Do While (i < bitstream3.g_side_info.count1(gr)(ch))
                    ' Check if we're into the next scalefac band
                    If i = next_sfb Then ' Yes
                        sfb += 1
                        next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                        win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)
                    End If

                    For win As Integer = 0 To 2
                        For j As Integer = 0 To win_len - 1
                            If sfb > 11 Then sfb = 11
                            MPG_Requantize_Process_Short(gr, ch, i, sfb, win)
                            i += 1
                        Next
                    Next
                Loop
            End If
        Else ' Only long blocks
            next_sfb = bitstream3.g_sf_band_indices(sfreq).L(sfb + 1)

            For i = 0 To bitstream3.g_side_info.count1(gr)(ch) - 1
                If i = next_sfb Then
                    If sfb < 20 Then sfb += 1
                    next_sfb = bitstream3.g_sf_band_indices(sfreq).L(sfb + 1)
                End If
                MPG_Requantize_Process_Long(gr, ch, i, sfb)
            Next
        End If
    End Sub

    ' Description: This function is used to requantize a sample in a subband
    '              that uses long blocks.
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_Requantize_Process_Long(ByVal gr As Integer, ByVal ch As Integer, ByVal is_pos As Integer, ByVal sfb As Integer)
        Dim sf_mult, tmp As Double

        If bitstream3.g_side_info.scalefac_scale(gr)(ch) <> 0 Then sf_mult = 1 Else sf_mult = 0.5
        Dim pf_x_pt As Double = bitstream3.g_side_info.preflag(gr)(ch) * p_pretab(sfb)

        If bitstream3.g_main_data.isx(gr)(ch)(is_pos) < 0.0 Then
            tmp = -(-bitstream3.g_main_data.isx(gr)(ch)(is_pos)) ^ (4 / 3)
        Else
            tmp = bitstream3.g_main_data.isx(gr)(ch)(is_pos) ^ (4 / 3)
        End If

        bitstream3.g_main_data.isx(gr)(ch)(is_pos) = 2 ^ -(sf_mult * (bitstream3.g_main_data.scalefac_l(gr)(ch)(sfb) + pf_x_pt)) *
                                                     2 ^ (0.25 * (bitstream3.g_side_info.global_gain(gr)(ch) - 210)) *
                                                     tmp
    End Sub

    '*****************************************************************************
    '
    ' Name: MPG_Requantize_Process_Short
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: This function is used to requantize a sample in a subband
    '              that uses short blocks.
    ' Parameters: TBD
    ' Return value: TBD
    '
    '****************************************************************************
    Private Sub MPG_Requantize_Process_Short(ByVal gr As Integer, ByVal ch As Integer, ByVal is_pos As Integer, ByVal sfb As Integer, ByVal win As Integer)
        Dim tmp, sf_mult As Double

        If bitstream3.g_side_info.scalefac_scale(gr)(ch) <> 0 Then sf_mult = 1 Else sf_mult = 0.5

        If bitstream3.g_main_data.isx(gr)(ch)(is_pos) < 0.0 Then
            tmp = -(-bitstream3.g_main_data.isx(gr)(ch)(is_pos)) ^ (4 / 3)
        Else
            tmp = bitstream3.g_main_data.isx(gr)(ch)(is_pos) ^ (4 / 3)
        End If

        bitstream3.g_main_data.isx(gr)(ch)(is_pos) = 2 ^ -(sf_mult * bitstream3.g_main_data.scalefac_s(gr)(ch)(sfb)(win)) *
                                                     2 ^ (0.25 * (bitstream3.g_side_info.global_gain(gr)(ch) - 210.0 - 8.0 * bitstream3.g_side_info.subblock_gain(gr)(ch)(win))) *
                                                     tmp
    End Sub

    '*****************************************************************************
    '
    ' Name: MPG_L3_Reorder
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    '
    '****************************************************************************
    Private Sub MPG_L3_Reorder(ByVal gr As Integer, ByVal ch As Integer)
        Dim win_len, next_sfb, j, sfb As Integer

        ' Only reorder short blocks
        If (bitstream3.g_side_info.win_switch_flag(gr)(ch) = 1) AndAlso (bitstream3.g_side_info.block_type(gr)(ch) = 2) Then ' Short blocks
            ' Check if the first two subbands
            ' (=2*18 samples = 8 long or 3 short sfb's) uses long blocks
            If bitstream3.g_side_info.mixed_block_flag(gr)(ch) <> 0 Then ' 2 longbl. sb  first
                ' Don't touch the first 36 samples
                ' Reorder the remaining ,  non-zero ,  bands which uses short blocks
                sfb = 3
                next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)
            Else ' Only short blocks
                sfb = 0
                next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)
            End If

            Dim i As Integer = 0
            If sfb <> 0 Then i = 36
            Do While (i < 576)
                ' Check if we're into the next scalefac band
                If i = next_sfb Then ' Yes
                    ' Copy reordered data back to the original vector
                    For j = 0 To 3 * win_len - 1
                        bitstream3.g_main_data.isx(gr)(ch)(3 * bitstream3.g_sf_band_indices(sfreq).S(sfb) + j) = re(j)
                    Next

                    ' Check if this band is above the rzero region ,  if so we're done
                    If i >= bitstream3.g_side_info.count1(gr)(ch) Then Exit Sub

                    sfb += 1
                    next_sfb = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) * 3
                    win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)
                End If
                ' Do the actual reordering
                For win As Integer = 0 To 2
                    For j = 0 To win_len - 1
                        re(j * 3 + win) = bitstream3.g_main_data.isx(gr)(ch)(i)
                        i += 1
                    Next
                Next
            Loop

            ' Copy reordered data of the last band back to the original vector
            For j = 0 To 3 * win_len - 1
                bitstream3.g_main_data.isx(gr)(ch)(3 * bitstream3.g_sf_band_indices(sfreq).S(12) + j) = re(j)
            Next
        Else ' Only long blocks
            ' No reorder necessary ,  do nothing!
            Exit Sub
        End If
    End Sub

    '*****************************************************************************
    '
    ' Name: MPG_L3_Stereo
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    '
    '****************************************************************************
    Private Sub MPG_L3_Stereo(ByVal gr As Integer)
        ' Do nothing if joint stereo is not enabled
        If (bitstream3.g_frame_header.mode <> Etc3.t_mpeg1_mode.mpeg1_mode_joint_stereo) OrElse (bitstream3.g_frame_header.mode_extension = 0) Then Exit Sub

        ' Do Middle/Side ("normal") stereo processing
        If (bitstream3.g_frame_header.mode_extension And 2) = 2 Then
            Dim max_pos As Integer

            ' Determine how many frequency lines to transform
            If bitstream3.g_side_info.count1(gr)(0) > bitstream3.g_side_info.count1(gr)(1) Then
                max_pos = bitstream3.g_side_info.count1(gr)(0)
            Else
                max_pos = bitstream3.g_side_info.count1(gr)(1)
            End If

            ' Do the actual processing
            For i As Integer = 0 To max_pos - 1
                bitstream3.g_main_data.isx(gr)(0)(i) = (bitstream3.g_main_data.isx(gr)(0)(i) + bitstream3.g_main_data.isx(gr)(1)(i)) * Etc3.C_INV_SQRT_2
                bitstream3.g_main_data.isx(gr)(1)(i) = (bitstream3.g_main_data.isx(gr)(0)(i) - bitstream3.g_main_data.isx(gr)(1)(i)) * Etc3.C_INV_SQRT_2
            Next

        End If

        ' Do intensity stereo processing
        If (bitstream3.g_frame_header.mode_extension And 1) = 1 Then
            Dim sfb As Integer ' scalefac band index

            ' The first band that is intensity stereo encoded is the first band
            ' scale factor band on or above the count1 frequency line.
            ' N.B.: Intensity stereo coding is only done for the higher subbands ,
            ' but the logic is still included to process lower subbands.

            ' Determine type of block to process
            If (bitstream3.g_side_info.win_switch_flag(gr)(0) = 1) AndAlso (bitstream3.g_side_info.block_type(gr)(0) = 2) Then ' Short blocks

                ' Check if the first two subbands
                ' (=2*18 samples = 8 long or 3 short sfb's) uses long blocks
                If bitstream3.g_side_info.mixed_block_flag(gr)(0) <> 0 Then ' 2 longbl. sb  first

                    ' First process the 8 sfb's at the start
                    For sfb = 0 To 7
                        ' Is this scale factor band above count1 for the right channel?
                        If bitstream3.g_sf_band_indices(sfreq).L(sfb) >= bitstream3.g_side_info.count1(gr)(1) Then MPG_Stereo_Process_Intensity_Long(gr, sfb)
                    Next

                    ' And next the remaining bands which uses short blocks
                    For sfb = 3 To 11
                        ' Is this scale factor band above count1 for the right channel?
                        If bitstream3.g_sf_band_indices(sfreq).S(sfb) * 3 >= bitstream3.g_side_info.count1(gr)(1) Then
                            ' Perform the intensity stereo processing
                            MPG_Stereo_Process_Intensity_Short(gr, sfb)
                        End If
                    Next
                Else ' Only short blocks
                    For sfb = 0 To 11
                        ' Is this scale factor band above count1 for the right channel?
                        If (bitstream3.g_sf_band_indices(sfreq).S(sfb) * 3) >= bitstream3.g_side_info.count1(gr)(1) Then
                            ' Perform the intensity stereo processing
                            MPG_Stereo_Process_Intensity_Short(gr, sfb)
                        End If
                    Next
                End If
            Else ' Only long blocks
                For sfb = 0 To 20
                    ' Is this scale factor band above count1 for the right channel?
                    If bitstream3.g_sf_band_indices(sfreq).L(sfb) >= bitstream3.g_side_info.count1(gr)(1) Then
                        ' Perform the intensity stereo processing
                        MPG_Stereo_Process_Intensity_Long(gr, sfb)
                    End If
                Next
            End If
        End If
    End Sub

    '*****************************************************************************
    '
    ' Name: MPG_Stereo_Process_Intensity_Long
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: This function is used to perform intensity stereo processing
    '              for an entire subband that uses long blocks.
    ' Parameters: TBD
    ' Return value: TBD
    '
    '****************************************************************************
    Private Sub MPG_Stereo_Process_Intensity_Long(ByVal gr As Integer, ByVal sfb As Integer)
        Static init As Boolean
        Static is_ratios(5) As Double
        Dim i As Integer
        Dim is_pos As Integer

        ' First-time init
        If init = False Then
            init = True
            For i = 0 To 5
                is_ratios(i) = Math.Tan((i * Math.PI) / 12.0)
            Next
        End If

        ' Check that ((is_pos(sfb)=scalefac) <> 7) => no intensity stereo
        is_pos = bitstream3.g_main_data.scalefac_l(gr)(0)(sfb)
        If is_pos <> 7 Then
            Dim sfreq As Integer
            Dim sfb_start, sfb_stop As Integer
            Dim is_ratio_l, is_ratio_r As Double

            ' Setup sampling frequency index
            sfreq = bitstream3.g_frame_header.sampling_frequency

            sfb_start = bitstream3.g_sf_band_indices(sfreq).L(sfb)
            sfb_stop = bitstream3.g_sf_band_indices(sfreq).L(sfb + 1)

            ' tan((6*PI)/12 = PI/2) needs special treatment!
            If is_pos = 6 Then
                is_ratio_l = 1.0
                is_ratio_r = 0.0
            Else
                is_ratio_l = is_ratios(is_pos) / (1.0 + is_ratios(is_pos))
                is_ratio_r = 1.0 / (1.0 + is_ratios(is_pos))
            End If

            ' Now decode all samples in this scale factor band
            For i = sfb_start To sfb_stop
                bitstream3.g_main_data.isx(gr)(0)(i) = is_ratio_l * bitstream3.g_main_data.isx(gr)(0)(i)
                bitstream3.g_main_data.isx(gr)(1)(i) = is_ratio_r * bitstream3.g_main_data.isx(gr)(1)(i)
            Next
        End If
    End Sub

    '*****************************************************************************
    '
    ' Name: MPG_Stereo_Process_Intensity_Short
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: This function is used to perform intensity stereo processing
    '              for an entire subband that uses short blocks.
    ' Parameters: TBD
    ' Return value: TBD
    '
    '****************************************************************************
    Private Sub MPG_Stereo_Process_Intensity_Short(ByVal gr As Integer, ByVal sfb As Integer)
        Dim left_Renamed, right_Renamed As Double
        Dim is_ratio_l, is_ratio_r As Double
        Dim sfb_start, sfb_stop As Integer
        Dim is_pos As Integer
        Dim is_ratios(5) As Integer
        Dim win, win_len As Integer

        ' The window length
        win_len = bitstream3.g_sf_band_indices(sfreq).S(sfb + 1) - bitstream3.g_sf_band_indices(sfreq).S(sfb)

        ' The three windows within the band has different scalefactors
        For win = 0 To 2

            ' Check that ((is_pos(sfb)=scalefac) <> 7) => no intensity stereo
            is_pos = CInt(bitstream3.g_main_data.scalefac_s(gr)(0)(sfb)(win))
            If is_pos <> 7 Then

                sfb_start = bitstream3.g_sf_band_indices(sfreq).S(sfb) * 3 + win_len * win
                sfb_stop = sfb_start + win_len

                ' XFX: Isn't this code kind of... useless?
                ' tan((6*PI)/12 = PI/2) needs special treatment!
                If is_pos = 6 Then
                    is_ratio_l = 1.0
                    is_ratio_r = 0.0
                Else
                    is_ratio_l = is_ratios(is_pos) / (1.0 + is_ratios(is_pos))
                    is_ratio_r = 1.0 / (1.0 + is_ratios(is_pos))
                End If

                ' Now decode all samples in this scale factor band
                For i As Integer = sfb_start To sfb_stop - 1
                    is_ratio_l = bitstream3.g_main_data.isx(gr)(0)(i)
                    left_Renamed = is_ratio_l
                    is_ratio_r = bitstream3.g_main_data.isx(gr)(1)(i)
                    right_Renamed = is_ratio_r
                    bitstream3.g_main_data.isx(gr)(0)(i) = left_Renamed
                    bitstream3.g_main_data.isx(gr)(1)(i) = right_Renamed
                Next

                For i As Integer = sfb_start To sfb_stop - 1
                    bitstream3.g_main_data.isx(gr)(0)(i) = bitstream3.g_main_data.isx(gr)(0)(i)
                    bitstream3.g_main_data.isx(gr)(1)(i) = bitstream3.g_main_data.isx(gr)(0)(i)
                Next
            End If
        Next

    End Sub

    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_L3_Antialias(ByVal gr As Integer, ByVal ch As Integer)
        Dim ui, sblim, li As Integer
        Dim ub, lb As Double

        ' No antialiasing is done for short blocks
        If (bitstream3.g_side_info.win_switch_flag(gr)(ch) = 1) AndAlso (bitstream3.g_side_info.block_type(gr)(ch) = 2) AndAlso (bitstream3.g_side_info.mixed_block_flag(gr)(ch) = 0) Then Exit Sub

        ' Setup the limit for how many subbands to transform
        If (bitstream3.g_side_info.win_switch_flag(gr)(ch) = 1) AndAlso (bitstream3.g_side_info.block_type(gr)(ch) = 2) AndAlso (bitstream3.g_side_info.mixed_block_flag(gr)(ch) = 1) Then
            sblim = 2
        Else
            sblim = 32
        End If

        ' Do the actual antialiasing
        For sb As Integer = 1 To sblim - 1 ' subband of 18 samples
            For i As Integer = 0 To 7
                li = 18 * sb - 1 - i
                ui = 18 * sb + i
                lb = bitstream3.g_main_data.isx(gr)(ch)(li) * CS(i) - bitstream3.g_main_data.isx(gr)(ch)(ui) * CA(i)
                ub = bitstream3.g_main_data.isx(gr)(ch)(ui) * CS(i) + bitstream3.g_main_data.isx(gr)(ch)(li) * CA(i)
                bitstream3.g_main_data.isx(gr)(ch)(li) = lb
                bitstream3.g_main_data.isx(gr)(ch)(ui) = ub
            Next
        Next
    End Sub

    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_L3_Hybrid_Synthesis(ByVal gr As Integer, ByVal ch As Integer)
        Dim bt As Integer = 0

        ' Loop through all 32 subbands
        For sb As Integer = 0 To 31
            ' Determine blocktype for this subband
            If Not ((sb < 2) AndAlso (bitstream3.g_side_info.win_switch_flag(gr)(ch) = 1) AndAlso (bitstream3.g_side_info.mixed_block_flag(gr)(ch) = 1)) Then
                ' NOT??? Long blocks in first 2 subbands
                bt = bitstream3.g_side_info.block_type(gr)(ch)
            End If

            ' Do the inverse modified discrete cosine transform (DCT) and windowing
            MPG_IMDCT_Win(gr, ch, sb * 18, rawout, bt)

            ' Overlapp add with stored vector into main_data vector
            For i As Integer = 0 To 17
                bitstream3.g_main_data.isx(gr)(ch)(sb * 18 + i) = rawout(i) + store(ch)(sb)(i)
                store(ch)(sb)(i) = rawout(i + 18)
            Next
        Next
    End Sub

    '*****************************************************************************
    '
    ' Name: MPG_IMDCT_Win
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: Does inverse modified DCT and windowing.
    ' Parameters: TBD
    ' Return value: TBD
    '
    '****************************************************************************
    Private Sub MPG_IMDCT_Win(ByVal i1 As Integer, ByVal i2 As Integer, ByVal i3 As Integer, ByRef out() As Double, ByVal block_type As Integer)
        'Static g_imdct_win(3, 35) As Double
        Static g_imdct_win()() As Double
        Dim n, i, m, p As Integer
        Dim sum As Double
        Static init As Boolean
        'Static mCache36(36, 36 \ 2) As Double
        'Static mCache12(12, 12 \ 2) As Double
        Static mCache36()() As Double
        Static mCache12()() As Double

        ' Setup the four (one for each block type) window vectors
        If init = False Then
            init = True

            Etc3.InitArray(Of Double)(g_imdct_win, 3, 35)
            Etc3.InitArray(Of Double)(mCache36, 36, 36 \ 2)
            Etc3.InitArray(Of Double)(mCache12, 12, 12 \ 2)

            For i = 0 To 35
                g_imdct_win(0)(i) = Math.Sin(Etc3.PI36 * (i + 0.5))
                g_imdct_win(1)(i) = 0.0
                g_imdct_win(2)(i) = 0.0
                g_imdct_win(3)(i) = Math.Sin(Etc3.PI36 * (i + 0.5))

                If i <= 29 Then
                    g_imdct_win(1)(i) = Math.Sin(Etc3.PI12 * (i + 0.5 - 18.0))
                    If i <= 23 Then
                        g_imdct_win(1)(i) = 1.0
                        If i <= 17 Then
                            g_imdct_win(1)(i) = Math.Sin(Etc3.PI36 * (i + 0.5))
                            g_imdct_win(3)(i) = 1.0
                            If i <= 11 Then
                                g_imdct_win(2)(i) = Math.Sin(Etc3.PI12 * (i + 0.5))
                                g_imdct_win(3)(i) = Math.Sin(Etc3.PI12 * (i + 0.5 - 6.0))
                                If i <= 5 Then
                                    g_imdct_win(3)(i) = 0.0
                                End If
                            End If
                        End If
                    End If
                End If
            Next

            ' this code alone saves 50% of CPU usage!
            n = 12
            For p = 0 To n - 1
                For m = 0 To n \ 2 - 1
                    mCache12(p)(m) = Math.Cos(Math.PI / (2 * n) * (2 * p + 1 + (n \ 2)) * (2 * m + 1))
                Next
            Next
            n = 36
            For p = 0 To n - 1
                For m = 0 To n \ 2 - 1
                    mCache36(p)(m) = Math.Cos(Math.PI / (2 * n) * (2 * p + 1 + (n \ 2)) * (2 * m + 1))
                Next
            Next
        End If

        Array.Clear(out, 0, 36)
        If block_type = 2 Then ' 3 short blocks
            n = 12
            For i = 0 To 2
                For p = 0 To n - 1
                    sum = 0.0
                    For m = 0 To (n \ 2) - 1
                        sum += bitstream3.g_main_data.isx(i1)(i2)(i3 + (i + 3 * m)) * mCache12(p)(m)
                    Next
                    out(6 * i + p + 6) += sum * g_imdct_win(block_type)(p)
                Next
            Next
        Else ' block_type <> 2
            n = 36
            For p = 0 To n - 1
                sum = 0.0
                For m = 0 To (n \ 2) - 1
                    sum += bitstream3.g_main_data.isx(i1)(i2)(i3 + m) * mCache36(p)(m)
                Next
                out(p) = sum * g_imdct_win(block_type)(p)
            Next
        End If
    End Sub

    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_L3_Frequency_Inversion(ByVal gr As Integer, ByVal ch As Integer)
        For sb As Integer = 1 To 31 Step 2
            For i As Integer = 1 To 17 Step 2
                bitstream3.g_main_data.isx(gr)(ch)(sb * 18 + i) = -bitstream3.g_main_data.isx(gr)(ch)(sb * 18 + i)
            Next
        Next
    End Sub

    ' Description: TBD
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_L3_Subband_Synthesis(ByVal gr As Integer, ByVal ch As Integer)
        Dim sum As Double
        Dim sample As Integer
        Dim i, j As Integer
        Static init As Boolean

        ' Setup the n_win windowing vector and the v_vec intermediate vector
        If init = False Then
            init = True

            For i = 0 To 63
                For j = 0 To 31
                    g_synth_n_win(i)(j) = Math.Cos((16 + i) * (2 * j + 1) * Etc3.PI64)
                Next
            Next

            ' Setup the v_vec intermediate vector
            Array.Clear(v_vec, 0, v_vec.Length)
        End If

        ' Loop through the 18 samples in each of the 32 subbands
        For sampleSubband As Integer = 0 To 17
            ' Shift up the V vector
            Array.Copy(v_vec, 0 + 1024 * ch, v_vec, 64 + 1024 * ch, 1024 - 64)

            ' Matrix multiply the input data vector with the n_win(,) matrix
            For i = 0 To 63
                v_vec(ch, i) = 0

                ' Copy the next 32 time samples to a temp vector
                sum = 0
                For j = 0 To 31
                    v_vec(ch, i) += g_synth_n_win(i)(j) * bitstream3.g_main_data.isx(gr)(ch)(j * 18 + sampleSubband)

                    If i < 8 Then
                        ' Build the U vector
                        u_vec(i * 64 + j) = v_vec(ch, i * 128 + j)
                        u_vec(i * 64 + j + 32) = v_vec(ch, i * 128 + j + 96)
                    ElseIf i > 31 Then
                        ' Calculate 32 samples and store them in the outdata vector
                        If j < 16 Then
                            sum += u_vec(j * 32 + i - 32) * p_synth_dtbl(j * 32 + i - 32)
                        ElseIf j = 16 Then
                            ' sum now contains time sample 32*ss+i. Convert to 16-bit signed int
                            sample = CInt(Math.Max(Math.Min(sum * 32767, 32767), -32768))

                            ' This function must be called for channel 0 first
                            If nChannels = 1 - 1 Then
                                ' Duplicate samples for both channels
                                outBuffer((32 * sampleSubband + i - 32) * 2) = sample
                                outBuffer((32 * sampleSubband + i - 32) * 2 + 1) = sample
                            Else
                                outBuffer((32 * sampleSubband + i - 32) * 2 + ch) = sample
                            End If
                        End If
                    End If
                Next
            Next

            ' Calculate 32 samples and store them in the outdata vector
            'For i = 0 To 31
            '    sum = 0.0
            '    For j = 0 To 15
            '        sum += u_vec(j * 32 + i) * p_synth_dtbl(j * 32 + i)
            '    Next

            '    ' sum now contains time sample 32*ss+i. Convert to 16-bit signed int
            '    sample = CInt(Math.Max(Math.Min(sum * 32767, 32767), -32768))

            '    ' This function must be called for channel 0 first
            '    If nChannels = 1 - 1 Then
            '        ' Duplicate samples for both channels
            '        outBuffer((32 * sampleSubband + i) * 2) = sample
            '        outBuffer((32 * sampleSubband + i) * 2 + 1) = sample
            '    Else
            '        outBuffer((32 * sampleSubband + i) * 2 + ch) = sample
            '    End If
            'Next
        Next
    End Sub
End Class