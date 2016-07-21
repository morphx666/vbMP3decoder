Public Class Etc3
    Public Enum STATUS
        sOK = 0
        sError = 1
        C_MPG_EOF = &HFFFFFFFF
    End Enum

    Public Const C_MPG_SYNC As Integer = &HFFF00000
    Public Const C_INV_SQRT_2 As Double = 0.70710678118654757 ' 0.707106781186548
    Public Const PI12 As Double = Math.PI / 12
    Public Const PI36 As Double = Math.PI / 36
    Public Const PI64 As Double = Math.PI / 64
    Public Const kHz As Integer = 1000
    Public Const kbit_s As Integer = 1000

    ' Layer number
    Public Enum t_mpeg1_layer
        mpeg1_layer_reserved = 0
        mpeg1_layer_3 = 1
        mpeg1_layer_2 = 2
        mpeg1_layer_1 = 3
    End Enum

    ' Modes
    Public Enum t_mpeg1_mode
        mpeg1_mode_stereo = 0
        mpeg1_mode_joint_stereo
        mpeg1_mode_dual_channel
        mpeg1_mode_double_channel
    End Enum

    ' MPEG1 Layer 1-3 frame header
    Public Structure t_mpeg1_header
        Dim ID As Integer ' 1 bit ''                       1 bit
        Dim layer As t_mpeg1_layer ' 2 bits                 2 bits
        Dim protection_bit As Integer ' 1 bit
        Dim bitrate_index As Integer ' 4 bits
        Dim sampling_frequency As Integer ' 2 bits
        Dim padding_bit As Integer ' 1 bit
        Dim private_bit As Integer ' 1 bit
        Dim mode As t_mpeg1_mode ' 2 bits
        Dim mode_extension As Integer ' 2 bits
        Dim copyright As Integer ' 1 bit
        Dim original_or_copy As Integer ' 1 bit
        Dim emphasis As Integer ' 2 bits
    End Structure

    ' MPEG1 Layer 3 Side Information
    ' (2,2) means (gr,ch)
    Public Structure t_mpeg1_side_info
        Dim main_data_begin As Integer ' 9 bits
        Dim private_bits As Integer ' 3 bits in mono, 5 in stereo
        Dim scfsi()() As Integer  ' 1 bit
        Dim part2_3_length()() As Integer  ' 12 bits
        Dim big_values()() As Integer  ' 9 bits
        Dim global_gain()() As Integer  ' 8 bits
        Dim scalefac_compress()() As Integer  ' 4 bits
        Dim win_switch_flag()() As Integer  ' 1 bit
        Dim block_type()() As Integer  ' 2 bits
        Dim mixed_block_flag()() As Integer  ' 1 bit
        Dim table_select()()() As Integer   ' 5 bits
        Dim subblock_gain()()() As Integer   '3 bits
        Dim region0_count()() As Integer  ' 4 bits
        Dim region1_count()() As Integer  ' 3 bits
        Dim preflag()() As Integer  ' 1 bit
        Dim scalefac_scale()() As Integer  ' 1 bit
        Dim count1table_select()() As Integer  ' 1 bit
        Dim count1()() As Integer  ' Not in file, calc. by huff.dec.!

        Public Sub Initialize()
            'ReDim scfsi(1, 3)
            'ReDim part2_3_length(1, 1)
            'ReDim big_values(1, 1)
            'ReDim global_gain(1, 1)
            'ReDim scalefac_compress(1, 1)
            'ReDim win_switch_flag(1, 1)
            'ReDim block_type(1, 1)
            'ReDim mixed_block_flag(1, 1)
            'ReDim table_select(1, 1, 2)
            'ReDim subblock_gain(1, 1, 2)
            'ReDim region0_count(1, 1)
            'ReDim region1_count(1, 1)
            'ReDim preflag(1, 1)
            'ReDim scalefac_scale(1, 1)
            'ReDim count1table_select(1, 1)
            'ReDim count1(1, 1)

            Etc3.InitArray(Of Integer)(scfsi, 1, 3)
            Etc3.InitArray(Of Integer)(part2_3_length, 1, 1)
            Etc3.InitArray(Of Integer)(big_values, 1, 1)
            Etc3.InitArray(Of Integer)(global_gain, 1, 1)
            Etc3.InitArray(Of Integer)(scalefac_compress, 1, 1)
            Etc3.InitArray(Of Integer)(win_switch_flag, 1, 1)
            Etc3.InitArray(Of Integer)(block_type, 1, 1)
            Etc3.InitArray(Of Integer)(mixed_block_flag, 1, 1)
            Etc3.InitArray(Of Integer)(table_select, 1, 1, 2)
            Etc3.InitArray(Of Integer)(subblock_gain, 1, 1, 2)
            Etc3.InitArray(Of Integer)(region0_count, 1, 1)
            Etc3.InitArray(Of Integer)(region1_count, 1, 1)
            Etc3.InitArray(Of Integer)(preflag, 1, 1)
            Etc3.InitArray(Of Integer)(scalefac_scale, 1, 1)
            Etc3.InitArray(Of Integer)(count1table_select, 1, 1)
            Etc3.InitArray(Of Integer)(count1, 1, 1)

        End Sub
    End Structure

    ' MPEG1 Layer 3 Main Data
    Public Structure t_mpeg1_main_data
        Dim scalefac_l()()() As Integer   ' 0-4 bits
        Dim scalefac_s()()()() As Double    ' 0-4 bits
        Dim isx()()() As Double   ' Huffman coded freq. lines

        Public Sub Initialize()
            'ReDim scalefac_l(1, 1, 20)
            'ReDim scalefac_s(1, 1, 11, 2)
            'ReDim isx(1, 1, 575)

            Etc3.InitArray(Of Integer)(scalefac_l, 1, 1, 20)
            Etc3.InitArray(Of Double)(scalefac_s, 1, 1, 11, 2)
            Etc3.InitArray(Of Double)(isx, 1, 1, 575)
        End Sub
    End Structure

    ' Scale factor band indices, for long and short windows
    Public Structure t_sf_band_indices
        Dim L() As Integer
        Dim S() As Integer

        Public Sub Initialize()
            ReDim L(22)
            ReDim S(13)
        End Sub
    End Structure

    Public Sub ErrOut(ByRef s As String, ByVal ParamArray SS() As Object)
        System.Diagnostics.Debug.WriteLine(s)
    End Sub

    Private decoder3 As Decoder3
    Private bitstream3 As Bitstream3
    Private audio3 As Audio3
    Private huffman3 As Huffman3

    Public Sub Init(ByVal decoder3 As Decoder3, ByVal bitstream3 As Bitstream3, ByVal audio3 As Audio3, ByVal huffman3 As Huffman3)
        Me.decoder3 = decoder3
        Me.bitstream3 = bitstream3
        Me.audio3 = audio3
        Me.huffman3 = huffman3

        decoder3.Init(bitstream3, audio3)

        Dim i As Integer
        ReDim huffman3.g_huffman_table(0)
        Dim K, J, L As Integer
        Dim t() As String = My.Resources.huffman_table.Split(vbCrLf.Substring(0, 1).ToCharArray)
        Dim tIdx As Integer = 0

        For i = 0 To bitstream3.g_sf_band_indices.Length - 1
            bitstream3.g_sf_band_indices(i).Initialize()
        Next

        bitstream3.g_sampling_frequency(0) = 44100
        bitstream3.g_sampling_frequency(1) = 48000
        bitstream3.g_sampling_frequency(2) = 32000

        For i = 0 To 33
            If t(tIdx).Length > 1 Then
                huffman3.g_huffman_main(i)(0) = L
                huffman3.g_huffman_main(i)(1) = CInt(t(tIdx).Split(" "c)(0))
                huffman3.g_huffman_main(i)(2) = CInt(t(tIdx).Split(" "c)(1))
                L = L + huffman3.g_huffman_main(i)(1)
                For J = 1 To huffman3.g_huffman_main(i)(1)
                    If K > UBound(huffman3.g_huffman_table) Then ReDim Preserve huffman3.g_huffman_table(K)
                    tIdx += 1
                    huffman3.g_huffman_table(K) = CInt(Trim(t(tIdx)))
                    K += 1
                Next
                tIdx += 1
            End If
            tIdx += 1

            If i <= 22 Then
                bitstream3.g_sf_band_indices(0).L(i) = CInt(Choose(i + 1, 0, 4, 8, 12, 16, 20, 24, 30, 36, 44, 52, 62, 74, 90, 110, 134, 162, 196, 238, 288, 342, 418, 576))
                bitstream3.g_sf_band_indices(1).L(i) = CInt(Choose(i + 1, 0, 4, 8, 12, 16, 20, 24, 30, 36, 42, 50, 60, 72, 88, 106, 128, 156, 190, 230, 276, 330, 384, 576))
                bitstream3.g_sf_band_indices(2).L(i) = CInt(Choose(i + 1, 0, 4, 8, 12, 16, 20, 24, 30, 36, 44, 54, 66, 82, 102, 126, 156, 194, 240, 296, 364, 448, 550, 576))
                If i <= 13 Then
                    bitstream3.g_sf_band_indices(0).S(i) = CInt(Choose(i + 1, 0, 4, 8, 12, 16, 22, 30, 40, 52, 66, 84, 106, 136, 192))
                    bitstream3.g_sf_band_indices(1).S(i) = CInt(Choose(i + 1, 0, 4, 8, 12, 16, 22, 28, 38, 50, 64, 80, 100, 126, 192))
                    bitstream3.g_sf_band_indices(2).S(i) = CInt(Choose(i + 1, 0, 4, 8, 12, 16, 22, 30, 42, 58, 78, 104, 138, 180, 192))
                End If
            End If
        Next
    End Sub

    Public Shared Sub InitArray(Of T)(ByRef array()() As T, ByVal b1 As Integer, ByVal b2 As Integer)
        For i1 As Integer = 0 To b1
            ReDim Preserve array(i1)
            For i2 As Integer = 0 To b2
                ReDim Preserve array(i1)(i2)
            Next
        Next
    End Sub

    Public Shared Sub InitArray(Of T)(ByRef array()()() As T, ByVal b1 As Integer, ByVal b2 As Integer, ByVal b3 As Integer)
        For i1 As Integer = 0 To b1
            ReDim Preserve array(i1)
            For i2 As Integer = 0 To b2
                ReDim Preserve array(i1)(i2)
                For i3 As Integer = 0 To b3
                    ReDim Preserve array(i1)(i2)(i3)
                Next
            Next
        Next
    End Sub

    Public Shared Sub InitArray(Of T)(ByRef array()()()() As T, ByVal b1 As Integer, ByVal b2 As Integer, ByVal b3 As Integer, ByVal b4 As Integer)
        For i1 As Integer = 0 To b1
            ReDim Preserve array(i1)
            For i2 As Integer = 0 To b2
                ReDim Preserve array(i1)(i2)
                For i3 As Integer = 0 To b3
                    ReDim Preserve array(i1)(i2)(i3)
                    For i4 As Integer = 0 To b4
                        ReDim Preserve array(i1)(i2)(i3)(i4)
                    Next
                Next
            Next
        Next
    End Sub
End Class