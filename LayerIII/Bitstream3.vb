Public Class Bitstream3
    ' Scale factor band indices
    ' One table per sample rate. Each table contains the frequency indices
    ' for the 12 short and 21 long scalefactor bands. The short indices
    ' must be multiplied by 3 to get the actual index.
    Public g_sf_band_indices(2) As Etc3.t_sf_band_indices

    Public g_frame_header As Etc3.t_mpeg1_header
    Public g_side_info As Etc3.t_mpeg1_side_info ' < 100 words
    Public g_main_data As Etc3.t_mpeg1_main_data ' Large static data (~2500 words)

    ' Bit reservoir for main data
    Public g_main_data_vec(2 * 1024 - 1) As Integer ' Large static data
    Public g_main_data_ptr As Integer ' Pointer into the reservoir
    Public g_main_data_idx As Integer ' Index into the current byte (0-7)
    Public g_main_data_top As Integer ' Number of bytes in reservoir (0-1024)

    Public g_sampling_frequency(2) As Integer

    'Private p_mpeg1_scalefac_sizes(15, 1) As Integer
    'Private mpeg1_bitrates(2, 14) As Integer

    Private p_mpeg1_scalefac_sizes()() As Integer
    Private mpeg1_bitrates()() As Integer

    ' Bit reservoir for side info
    Private side_info_vec(32 + 4 - 1) As Integer
    Private side_info_ptr As Integer ' Pointer into the reservoir
    Private side_info_idx As Integer ' Index into the current byte (0-7)

    Private pIdx As Integer
    Private fSize As Integer
    Private fBuf() As Byte

    Private bitRateSum As Long
    Private bitRateCount As Integer

    Private decoder3 As Decoder3
    Private huffman3 As Huffman3
    Private bitstream3 As Bitstream3
    Private audio3 As Audio3

    Private v1() As Integer = {0, 0, 0, 0, 3, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4}
    Private v2() As Integer = {0, 1, 2, 3, 0, 1, 2, 3, 1, 2, 3, 1, 2, 3, 2, 3}
    Private v3() As Integer = {0, 32000, 64000, 96000, 128000, 160000, 192000, 224000, 256000, 288000, 320000, 352000, 384000, 416000, 448000}
    Private v4() As Integer = {0, 32000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000, 192000, 224000, 256000, 320000, 384000}
    Private v5() As Integer = {0, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000, 192000, 224000, 256000, 320000}

    Public Sub Init(ByVal fileName As String, ByVal decoder3 As Decoder3, ByVal huffman3 As Huffman3, ByVal bitstream3 As Bitstream3, ByVal audio3 As Audio3)
        bitRateSum = 0
        bitRateCount = 0

        Etc3.InitArray(Of Integer)(p_mpeg1_scalefac_sizes, 15, 1)
        Etc3.InitArray(Of Integer)(mpeg1_bitrates, 2, 14)

        Me.decoder3 = decoder3
        Me.huffman3 = huffman3
        Me.bitstream3 = bitstream3
        Me.audio3 = audio3

        pIdx = 0
        fBuf = System.IO.File.ReadAllBytes(fileName)
        fSize = fBuf.Length

        For i As Integer = 0 To 15
            p_mpeg1_scalefac_sizes(i)(0) = v1(i)
            p_mpeg1_scalefac_sizes(i)(1) = v2(i)
            If i < 15 Then
                mpeg1_bitrates(0)(i) = v3(i)
                mpeg1_bitrates(1)(i) = v4(i)
                mpeg1_bitrates(2)(i) = v5(i)
            End If
        Next
    End Sub

    Public ReadOnly Property BitRate() As Integer
        Get
            If bitRateCount = 0 Then Return 0
            Return CInt(bitRateSum / bitRateCount)
        End Get
    End Property

    Public Function MPG_Get_Bytes(ByVal no_of_bytes As Integer, ByRef data_vec() As Integer, ByVal off As Integer) As Etc3.STATUS
        For i As Integer = 0 To no_of_bytes - 1
            data_vec(off + i) = MPG_Byte()
            If data_vec(off + i) = Etc3.STATUS.C_MPG_EOF Then Return Etc3.STATUS.C_MPG_EOF
        Next

        Return Etc3.STATUS.sOK
    End Function

    Private ReadOnly Property MPG_Byte() As Integer
        Get
            If pIdx < fSize Then
                pIdx += 1
                Return fBuf(pIdx - 1)
            Else
                Return Etc3.STATUS.C_MPG_EOF
            End If
        End Get
    End Property

    Public Property MPG_Position() As Integer
        Get
            ' File open?
            If fSize = 0 Then Exit Property

            If pIdx >= fSize Then
                Return Etc3.STATUS.C_MPG_EOF
            Else
                Return pIdx
            End If
        End Get
        Set(ByVal value As Integer)
            pIdx = value
        End Set
    End Property

    '* Description: This function returns the current file size in bytes.
    Public Function MPG_Get_Filesize() As Integer
        Return fSize
    End Function

    ' Name: MPG_Read_Frame
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: Searches for next frame and read it into the buffer.
    '              Main data in this frame is saved for two frames since it
    '              might be needed for them also.
    ' Parameters: None
    ' Return value: OK if a frame is successfully read, ERROR otherwise.
    Public Function MPG_Read_Frame() As Etc3.STATUS
        Dim first As Boolean

        If MPG_Position() = 0 Then
            first = True
            g_main_data_top = 0
        Else
            first = False
        End If

        ' Try to find the next frame in the bitstream and decode it
        If MPG_Read_Header() <> Etc3.STATUS.sOK Then Return Etc3.STATUS.sError

        If first Then
            'ErrOut("Starting decode, Layer: %d, Rate: %6d, Sfreq: %05d", g_frame_header.layer, mpeg1_bitrates(g_frame_header.layer - 1, g_frame_header.bitrate_index), g_sampling_frequency(g_frame_header.sampling_frequency))
            decoder3.Init(bitstream3, Audio3)
        End If

        ' Get CRC word if present
        If g_frame_header.protection_bit = 0 Then ' CRC present
            If MPG_Read_CRC() <> Etc3.STATUS.sOK Then Return Etc3.STATUS.sError
        End If

        ' Get audio data
        If g_frame_header.layer = 3 Then
            ' Get side info
            MPG_Read_Audio_L3()

            ' If there's not enough main data in the bit reservoir,
            ' signal to calling function so that decoding isn't done!

            ' Get main data (scalefactors and Huffman coded frequency data)
            If MPG_Read_Main_L3() <> Etc3.STATUS.sOK Then Return Etc3.STATUS.sError
        Else
            'ErrOut("Only layer 3 (<> %d) is supported!\n", g_frame_header.layer)
            Return Etc3.STATUS.sError
        End If

        bitRateSum += mpeg1_bitrates(2)(g_frame_header.bitrate_index)
        bitRateCount += 1

        Return Etc3.STATUS.sOK
    End Function


    ' Name: MPG_Read_Header
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: Scans the bitstream for a syncword until we find it or EOF.
    '              The syncword must be byte-aligned. It then reads and parses
    '              the audio header.
    ' Parameters: None
    ' Return value: OK or ERROR if the syncword can't be found, or the header
    '               contains impossible values.
    Private Function MPG_Read_Header() As Etc3.STATUS

        ' Get the next four bytes from the bitstream
        Dim B1 As Integer = MPG_Byte()
        Dim B2 As Integer = MPG_Byte()
        Dim B3 As Integer = MPG_Byte()
        Dim B4 As Integer = MPG_Byte()

        ' If we got an End Of File condition we're done
        If (B1 = Etc3.STATUS.C_MPG_EOF) OrElse (B2 = Etc3.STATUS.C_MPG_EOF) OrElse (B3 = Etc3.STATUS.C_MPG_EOF) OrElse (B4 = Etc3.STATUS.C_MPG_EOF) Then Return Etc3.STATUS.sError

        Dim header As Integer = SHL(B1, 24) Or SHL(B2, 16) Or SHL(B3, 8) Or B4

        ' Are the high 12 bits the syncword (&Hfff)?
        If (header And &HFFF00000) <> Etc3.C_MPG_SYNC Then

            ' No, so scan the bitstream one byte at a time until we find it or EOF
            Do
                ' Shift the values one byte to the left
                B1 = B2
                B2 = B3
                B3 = B4

                ' Get one new byte from the bitstream
                B4 = MPG_Byte()

                ' If we got an End Of File condition we're done
                If B4 = Etc3.STATUS.C_MPG_EOF Then Return Etc3.STATUS.sError

                ' Make up the new header
                header = SHL(B1, 24) Or SHL(B2, 16) Or SHL(B3, 8) Or B4

                ' If it's the syncword (&Hfff00000) we're done
            Loop Until (header And &HFFF00000) = Etc3.C_MPG_SYNC
        End If

        ' If we get here we've found the sync word, and can decode the header
        ' which is in the low 20 bits of the 32-bit sync+header word.

        ' Decode the header
        With g_frame_header
            .ID = SHR(header And &H80000, 19)
            .layer = CType(SHR(header And &H60000, 17), Etc3.t_mpeg1_layer)
            .protection_bit = SHR(header And &H10000, 16)

            .bitrate_index = SHR(header And &HF000, 12)

            .sampling_frequency = SHR(header And &HC00, 10)
            .padding_bit = SHR(header And &H200, 9)
            .private_bit = SHR(header And &H100, 8)

            .mode = CType(SHR(header And &HC0, 6), Etc3.t_mpeg1_mode)
            .mode_extension = SHR(header And &H30, 4)

            .copyright = SHR(header And 8, 3)
            .original_or_copy = SHR(header And 4, 2)
            .emphasis = header And 3
        End With

        ' Check for invalid values and impossible combinations
        If g_frame_header.ID <> 1 Then
            'ErrOut("ID must be 1")
            'ErrOut("Header word is &H%08x at file pos %d", header, MPG_Position())
            Return Etc3.STATUS.sError
        End If

        If g_frame_header.bitrate_index = 0 Then
            'ErrOut("Free bitrate format NIY!\n")
            'ErrOut("Header word is &H%08x at file pos %d\n", header, MPG_Position())
            Return Etc3.STATUS.sError
            Exit Function
        End If

        If g_frame_header.bitrate_index = 15 Then
            'ErrOut("bitrate_index = 15 is invalid!\n")
            'ErrOut("Header word is &H%08x at file pos %d\n", header, MPG_Position())
            Return Etc3.STATUS.sError
            Exit Function
        End If

        If g_frame_header.sampling_frequency = 3 Then
            'ErrOut("sampling_frequency = 3 is invalid!\n")
            'ErrOut("Header word is &H%08x at file pos %d\n", header, MPG_Position())
            Return Etc3.STATUS.sError
            Exit Function
        End If

        If g_frame_header.layer = 0 Then
            'ErrOut("layer = 0 is invalid!\n")
            'ErrOut("Header word is &H%08x at file pos %d\n", header, MPG_Position())
            Return Etc3.STATUS.sError
            Exit Function
        End If
        g_frame_header.layer = CType(4 - g_frame_header.layer, Etc3.t_mpeg1_layer)

        Return Etc3.STATUS.sOK
    End Function


    ' Name: MPG_Read_CRC
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: Reads the 16 CRC bits
    ' Parameters: None
    ' Return value: OK or ERROR if CRC could not be read.
    Private Function MPG_Read_CRC() As Etc3.STATUS
        ' Get the next two bytes from the bitstream
        Dim B1 As Integer = MPG_Byte()
        Dim B2 As Integer = MPG_Byte()

        ' If we got an End Of File condition we're done
        If (B1 = Etc3.STATUS.C_MPG_EOF) OrElse (B2 = Etc3.STATUS.C_MPG_EOF) Then
            Return Etc3.STATUS.C_MPG_EOF
        Else
            Return Etc3.STATUS.sOK
        End If
    End Function

    ' Description: Reads the audio and main data from the bitstream into a private
    '              buffer. The main data is taken from this frame and up to two
    '              previous frames.
    ' Parameters: None
    ' Return value: OK or ERROR if data could not be read, or contains errors.
    Private Function MPG_Read_Audio_L3() As Etc3.STATUS
        Dim sideinfo_size As Integer
        Dim nch As Integer

        ' Number of channels (1 for mono and 2 for stereo)
        If g_frame_header.mode = Etc3.t_mpeg1_mode.mpeg1_mode_double_channel Then nch = 1 Else nch = 2

        ' Calculate header audio data size
        Dim framesize As Integer = (144 * (mpeg1_bitrates(g_frame_header.layer - 1)(g_frame_header.bitrate_index) \ g_sampling_frequency(g_frame_header.sampling_frequency))) + g_frame_header.padding_bit

        If framesize > 2000 Then
            'ErrOut("framesize = %d\n", framesize)
            Return Etc3.STATUS.sError
        End If

        ' Sideinfo is 17 bytes for one channel and 32 bytes for two
        If nch = 1 Then sideinfo_size = 17 Else sideinfo_size = 32

        ' Main data size is the rest of the frame, including ancillary data
        Dim main_data_size As Integer = framesize - sideinfo_size - 4 ' sync+header

        ' CRC is 2 bytes
        If (g_frame_header.protection_bit = 0) Then main_data_size = main_data_size - 2

        ' DBG ("framesize      =   %d\n", framesize)
        ' DBG ("sideinfo_size  =   %d\n", sideinfo_size)
        ' DBG ("main_data_size =   %d\n", main_data_size)

        ' Read the sideinfo from the bitstream into a local buffer used by the
        ' MPG_Get_Side_Bits function.
        MPG_Get_Sideinfo(sideinfo_size)
        If MPG_Position() = Etc3.STATUS.C_MPG_EOF Then Return Etc3.STATUS.sError

        ' Parse audio data

        ' Pointer to where we should start reading main data
        g_side_info.main_data_begin = MPG_Get_Side_Bits(9)

        ' Get private bits. Not used for anything.
        If g_frame_header.mode = Etc3.t_mpeg1_mode.mpeg1_mode_double_channel Then
            g_side_info.private_bits = MPG_Get_Side_Bits(5)
        Else
            g_side_info.private_bits = MPG_Get_Side_Bits(3)
        End If

        ' Get scale factor selection information
        For ch As Integer = 0 To nch - 1
            For scfsi_band As Integer = 0 To 3
                g_side_info.scfsi(ch)(scfsi_band) = MPG_Get_Side_Bits(1)
            Next
        Next

        ' Get the rest of the side information
        For gr As Integer = 0 To 1
            For ch As Integer = 0 To nch - 1
                g_side_info.part2_3_length(gr)(ch) = MPG_Get_Side_Bits(12)
                g_side_info.big_values(gr)(ch) = MPG_Get_Side_Bits(9)
                g_side_info.global_gain(gr)(ch) = MPG_Get_Side_Bits(8)
                g_side_info.scalefac_compress(gr)(ch) = MPG_Get_Side_Bits(4)

                g_side_info.win_switch_flag(gr)(ch) = MPG_Get_Side_Bits(1)

                If g_side_info.win_switch_flag(gr)(ch) = 1 Then
                    g_side_info.block_type(gr)(ch) = MPG_Get_Side_Bits(2)
                    g_side_info.mixed_block_flag(gr)(ch) = MPG_Get_Side_Bits(1)
                    For region As Integer = 0 To 1
                        g_side_info.table_select(gr)(ch)(region) = MPG_Get_Side_Bits(5)
                    Next
                    For window As Integer = 0 To 2
                        g_side_info.subblock_gain(gr)(ch)(window) = MPG_Get_Side_Bits(3)
                    Next
                    If (g_side_info.block_type(gr)(ch) = 2) AndAlso (g_side_info.mixed_block_flag(gr)(ch) = 0) Then
                        g_side_info.region0_count(gr)(ch) = 8 ' Implicit
                    Else
                        g_side_info.region0_count(gr)(ch) = 7 ' Implicit
                    End If

                    ' The standard is wrong on this!!!
                    g_side_info.region1_count(gr)(ch) = 20 - g_side_info.region0_count(gr)(ch) ' Implicit
                Else
                    For region As Integer = 0 To 2
                        g_side_info.table_select(gr)(ch)(region) = MPG_Get_Side_Bits(5)
                    Next
                    g_side_info.region0_count(gr)(ch) = MPG_Get_Side_Bits(4)
                    g_side_info.region1_count(gr)(ch) = MPG_Get_Side_Bits(3)
                    g_side_info.block_type(gr)(ch) = 0 ' Implicit
                End If

                g_side_info.preflag(gr)(ch) = MPG_Get_Side_Bits(1)
                g_side_info.scalefac_scale(gr)(ch) = MPG_Get_Side_Bits(1)
                g_side_info.count1table_select(gr)(ch) = MPG_Get_Side_Bits(1)
            Next
        Next

        Return Etc3.STATUS.sOK
    End Function

    ' Name: MPG_Read_Main_L3
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: This function reads the main data for layer 3 from the
    '              main_data bit reservoir.
    ' Parameters: None
    ' Return value: OK or ERROR if the data contains errors.
    Private Function MPG_Read_Main_L3() As Etc3.STATUS
        Dim nbits As Integer
        Dim slen1 As Integer
        Dim slen2 As Integer
        Dim part_2_start As Integer
        Dim sfb As Integer

        ' Calculate header audio data size
        Dim framesize As Integer = (144 * (mpeg1_bitrates(g_frame_header.layer - 1)(g_frame_header.bitrate_index)) \ g_sampling_frequency(g_frame_header.sampling_frequency)) + g_frame_header.padding_bit

        If framesize > 2000 Then
            'ErrOut("framesize = %d\n", framesize)
            MPG_Read_Main_L3 = Etc3.STATUS.sError
            Exit Function
        End If

        ' Sideinfo is 17 bytes for one channel and 32 bytes for two
        Dim sideinfo_size As Integer = 17
        If decoder3.nChannels = 2 - 1 Then sideinfo_size = 32

        ' Main data size is the rest of the frame, including ancillary data
        Dim main_data_size As Integer = framesize - sideinfo_size - 4 ' sync+header

        ' CRC is 2 bytes
        If (g_frame_header.protection_bit = 0) Then main_data_size = main_data_size - 2

        ' Assemble main data buffer with data from this frame and the previous
        ' two frames. main_data_begin indicates how many bytes from previous
        ' frames that should be used. This buffer is later accessed by the
        ' MPG_Get_Main_Bits function in the same way as the side info is.
        If MPG_Get_Main_Data(main_data_size, g_side_info.main_data_begin) <> Etc3.STATUS.sOK Then
            ' This could be due to not enough data in reservoir
            MPG_Read_Main_L3 = Etc3.STATUS.sError
            Exit Function
        End If

        For gr As Integer = 0 To 1
            For ch As Integer = 0 To decoder3.nChannels
                part_2_start = MPG_Get_Main_Pos()

                ' Number of bits in the bitstream for the bands
                slen1 = p_mpeg1_scalefac_sizes(g_side_info.scalefac_compress(gr)(ch))(0)
                slen2 = p_mpeg1_scalefac_sizes(g_side_info.scalefac_compress(gr)(ch))(1)

                If (g_side_info.win_switch_flag(gr)(ch) <> 0) And (g_side_info.block_type(gr)(ch) = 2) Then
                    If g_side_info.mixed_block_flag(gr)(ch) <> 0 Then
                        For sfb = 0 To 7
                            g_main_data.scalefac_l(gr)(ch)(sfb) = MPG_Get_Main_Bits(slen1)
                        Next
                        For sfb = 3 To 11
                            If sfb < 6 Then ' slen1 is for bands 3-5, slen2 for 6-11
                                nbits = slen1
                            Else
                                nbits = slen2
                            End If

                            For win As Integer = 0 To 2
                                g_main_data.scalefac_s(gr)(ch)(sfb)(win) = MPG_Get_Main_Bits(nbits)
                            Next
                        Next
                    Else
                        For sfb = 0 To 11
                            If sfb < 6 Then ' slen1 is for bands 3-5, slen2 for 6-11
                                nbits = slen1
                            Else
                                nbits = slen2
                            End If

                            For win As Integer = 0 To 2
                                g_main_data.scalefac_s(gr)(ch)(sfb)(win) = MPG_Get_Main_Bits(nbits)
                            Next
                        Next
                    End If
                Else ' block_type = 0 if winswitch = 0

                    ' Scale factor bands 0-5
                    If (g_side_info.scfsi(ch)(0) = 0) Or (gr = 0) Then
                        For sfb = 0 To 5
                            g_main_data.scalefac_l(gr)(ch)(sfb) = MPG_Get_Main_Bits(slen1)
                        Next
                    ElseIf (g_side_info.scfsi(ch)(0) = 1) And (gr = 1) Then
                        ' Copy scalefactors from granule 0 to granule 1
                        For sfb = 0 To 5
                            g_main_data.scalefac_l(1)(ch)(sfb) = g_main_data.scalefac_l(0)(ch)(sfb)
                        Next
                    End If

                    ' Scale factor bands 6-10
                    If (g_side_info.scfsi(ch)(1) = 0) Or (gr = 0) Then
                        For sfb = 6 To 10
                            g_main_data.scalefac_l(gr)(ch)(sfb) = MPG_Get_Main_Bits(slen1)
                        Next
                    ElseIf (g_side_info.scfsi(ch)(1) = 1) And (gr = 1) Then
                        ' Copy scalefactors from granule 0 to granule 1
                        For sfb = 6 To 10
                            g_main_data.scalefac_l(1)(ch)(sfb) = g_main_data.scalefac_l(0)(ch)(sfb)
                        Next
                    End If

                    ' Scale factor bands 11-15
                    If (g_side_info.scfsi(ch)(2) = 0) Or (gr = 0) Then
                        For sfb = 11 To 15
                            g_main_data.scalefac_l(gr)(ch)(sfb) = MPG_Get_Main_Bits(slen2)
                        Next
                    ElseIf (g_side_info.scfsi(ch)(2) = 1) And (gr = 1) Then
                        ' Copy scalefactors from granule 0 to granule 1
                        For sfb = 11 To 15
                            g_main_data.scalefac_l(1)(ch)(sfb) = g_main_data.scalefac_l(0)(ch)(sfb)
                        Next
                    End If

                    ' Scale factor bands 16-20
                    If (g_side_info.scfsi(ch)(3) = 0) Or (gr = 0) Then
                        For sfb = 16 To 20
                            g_main_data.scalefac_l(gr)(ch)(sfb) = MPG_Get_Main_Bits(slen2)
                        Next
                    ElseIf (g_side_info.scfsi(ch)(3) = 1) And (gr = 1) Then
                        ' Copy scalefactors from granule 0 to granule 1
                        For sfb = 16 To 20
                            g_main_data.scalefac_l(1)(ch)(sfb) = g_main_data.scalefac_l(0)(ch)(sfb)
                        Next
                    End If
                End If

                Try
                    ' Read Huffman coded data. Skip stuffing bits.
                    huffman3.MPG_Read_Huffman(part_2_start, gr, ch)
                Catch
                End Try
            Next
        Next

        ' The ancillary data is stored here, but we ignore it.

        Return Etc3.STATUS.sOK
    End Function

    ' Name: MPG_Get_Side_Bits
    ' Author: Krister Lagerström (krister@kmlager.com)
    ' Description: This function reads 'number_of_bits' bits from the local buffer
    '              which contains the side_info.
    ' Parameters: number_of_bits to read (max 16)
    ' Return value: The bits are returned in the LSB of the return value.
    Private Function MPG_Get_Side_Bits(ByVal number_of_bits As Integer) As Integer
        Try
            ' Form a word of the next four bytes
            Dim tmp As Integer = SHL(side_info_vec(side_info_ptr + 0), 24) Or SHL(side_info_vec(side_info_ptr + 1), 16) Or SHL(side_info_vec(side_info_ptr + 2), 8) Or side_info_vec(side_info_ptr + 3)

            ' Remove bits already used
            tmp = SHL(tmp, side_info_idx)

            ' Remove bits after the desired bits
            tmp = SHR(tmp, 32 - number_of_bits)

            ' Update pointers
            side_info_ptr += (side_info_idx + number_of_bits) \ 8
            side_info_idx = (side_info_idx + number_of_bits) And 7

            Return tmp
        Catch
        End Try
    End Function


    ' Description: This function reads 'number_of_bits' bits from the local buffer
    '              which contains the main_data.
    ' Parameters: number_of_bits to read (max 24)
    ' Return value: The bits are returned in the LSB of the return value.
    Public Function MPG_Get_Main_Bits(ByVal number_of_bits As Integer) As Integer
        If number_of_bits = 0 Then Return 0

        ' Form a word of the next four bytes
        Dim tmp As Integer = SHL(g_main_data_vec(g_main_data_ptr + 0), 24) Or SHL(g_main_data_vec(g_main_data_ptr + 1), 16) Or SHL(g_main_data_vec(g_main_data_ptr + 2), 8) Or g_main_data_vec(g_main_data_ptr + 3)

        ' Remove bits already used
        tmp = SHL(tmp, g_main_data_idx)

        ' Remove bits after the desired bits
        tmp = SHR(tmp, 32 - number_of_bits)

        ' Update pointers
        g_main_data_ptr = g_main_data_ptr + (g_main_data_idx + number_of_bits) \ 8
        g_main_data_idx = (g_main_data_idx + number_of_bits) And 7

        ' Done
        MPG_Get_Main_Bits = tmp
    End Function

    ' Description: This function one bit from the local buffer
    '              which contains the main_data.
    ' Parameters: None
    ' Return value: The bit is returned in the LSB of the return value.
    Public Function MPG_Get_Main_Bit() As Integer
        Dim bit As Integer = SHR(g_main_data_vec(g_main_data_ptr), 7 - g_main_data_idx) And 1

        g_main_data_ptr = g_main_data_ptr + (g_main_data_idx + 1) \ 8 ' SHR(g_main_data_idx + 1, 3)
        g_main_data_idx = (g_main_data_idx + 1) And 7

        Return bit
    End Function


    ' Description: This function sets the position of the next bit to be read from
    '              the main data bitstream.
    ' Parameters: Bit position. 0 = start, 8 = start of byte 1, etc.
    ' Return value: OK or ERROR if bit_pos is beyond the end of the main data for this
    '               frame.
    Public Function MPG_Set_Main_Pos(ByVal bit_pos As Integer) As Etc3.STATUS
        g_main_data_ptr = bit_pos \ 8
        g_main_data_idx = bit_pos And 7

        Return Etc3.STATUS.sOK
    End Function


    ' Description: This function returns the position of the next bit to be read
    '              from the main data bitstream.
    ' Parameters: None
    ' Return value: Bit position.
    Public Function MPG_Get_Main_Pos() As Integer
        Return g_main_data_ptr * 8 + g_main_data_idx
    End Function

    ' Description: Reads the sideinfo from the bitstream into a local buffer
    '              used by the MPG_Get_Side_Bits function.
    ' Parameters: TBD
    ' Return value: TBD
    Private Sub MPG_Get_Sideinfo(ByVal sideinfo_size As Integer)
        If MPG_Get_Bytes(sideinfo_size, side_info_vec, 0) <> Etc3.STATUS.sOK Then
            'ErrOut("\nCouldn't read sideinfo %d bytes at pos %d\n", sideinfo_size, MPG_Position())
            Exit Sub
        End If

        side_info_ptr = 0
        side_info_idx = 0
    End Sub


    ' Description: This function assembles the main data buffer with data from
    '              this frame and the previous two frames into a local buffer
    '              used by the MPG_Get_Main_Bits function.
    ' Parameters: main_data_begin indicates how many bytes from previous
    '             frames that should be used. main_data_size indicates the number
    '             of data bytes in this frame.
    ' Return value: None
    Private Function MPG_Get_Main_Data(ByVal main_data_size As Integer, ByVal main_data_begin As Integer) As Etc3.STATUS
        If main_data_size > 1500 Then
            'ErrOut("main_data_size = %d\n", main_data_size)
        End If

        ' Check that there's data available from previous frames if needed
        If main_data_begin > g_main_data_top Then
            ' No, there is not, so we skip decoding this frame, but we have to
            ' read the main_data bits from the bitstream in case they are needed
            ' for decoding the next frame.
            MPG_Get_Bytes(main_data_size, g_main_data_vec, g_main_data_top)

            ' Set up pointers
            g_main_data_ptr = 0
            g_main_data_idx = 0
            g_main_data_top = g_main_data_top + main_data_size

            MPG_Get_Main_Data = Etc3.STATUS.sError ' This frame cannot be decoded!
            Exit Function
        End If

        ' Copy data from previous frames
        For i As Integer = 0 To main_data_begin - 1
            g_main_data_vec(i) = g_main_data_vec(g_main_data_top - main_data_begin + i)
        Next

        Dim start_pos As Integer = MPG_Position()

        ' Read the main_data from file
        MPG_Get_Bytes(main_data_size, g_main_data_vec, main_data_begin)

        ' Set up pointers
        g_main_data_ptr = 0
        g_main_data_idx = 0
        g_main_data_top = main_data_begin + main_data_size

        ' Done
        Return Etc3.STATUS.sOK
    End Function

    Public Function SHR(ByVal value As Integer, ByVal shift As Integer) As Integer
        If value >= 0 Then
            Return (value >> shift)
        ElseIf shift > 2 Then
            Return (value >> shift) - (&HC0000000 >> (shift - 2))
        Else
            Return (value >> shift) - &HC0000000
        End If
    End Function

    Public Function SHL(ByVal value As Integer, ByVal shift As Integer) As Integer
        Return (value << shift)
    End Function
End Class