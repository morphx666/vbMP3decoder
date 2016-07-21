Public Class Huffman3
    Public g_huffman_table() As Integer
    'Public g_huffman_main(33, 2) As Integer
    Public g_huffman_main()() As Integer
    Private sfreq As Integer

    Private bitstream3 As Bitstream3

    Public Sub New()
        Etc3.InitArray(Of Integer)(g_huffman_main, 33, 2)
    End Sub

    Public Sub Init(ByVal bitstream3 As Bitstream3)
        Me.bitstream3 = bitstream3

        sfreq = bitstream3.g_frame_header.sampling_frequency
    End Sub

    ' Description: This function is called by MPG_Read_Main_L3 to read the Huffman coded data from the bitstream.
    ' Parameters: None
    ' Return value: None. The data is stored in g_main_data.is(ch,gr,freqline).
    Public Sub MPG_Read_Huffman(ByVal part_2_start As Integer, ByVal gr As Integer, ByVal ch As Integer)
        'Try
        Dim V, X, Y, W As Integer
        Dim bit_pos_end, table_num, is_pos As Integer
        Dim region_1_start, region_2_start As Integer

        ' Check that there is any data to decode. If not, zero the array.
        If bitstream3.g_side_info.part2_3_length(gr)(ch) = 0 Then
            For is_pos = 0 To 575
                bitstream3.g_main_data.isx(gr)(ch)(is_pos) = 0.0
            Next
            Exit Sub
        End If

        ' Calculate bit_pos_end which is the index of the last bit for this part.
        bit_pos_end = part_2_start + bitstream3.g_side_info.part2_3_length(gr)(ch) - 1

        ' Determine region boundaries
        If (bitstream3.g_side_info.win_switch_flag(gr)(ch) = 1) AndAlso (bitstream3.g_side_info.block_type(gr)(ch) = 2) Then
            region_1_start = 36 ' sfb(9/3)*3=36
            region_2_start = 576 ' No Region2 for short block case.
        Else
            region_1_start = bitstream3.g_sf_band_indices(sfreq).L(bitstream3.g_side_info.region0_count(gr)(ch) + 1)
            region_2_start = bitstream3.g_sf_band_indices(sfreq).L(bitstream3.g_side_info.region0_count(gr)(ch) + bitstream3.g_side_info.region1_count(gr)(ch) + 2)
        End If

        ' Read big_values using tables according to region_x_start
        For is_pos = 0 To bitstream3.g_side_info.big_values(gr)(ch) * 2 - 1
            If is_pos < region_1_start Then
                table_num = bitstream3.g_side_info.table_select(gr)(ch)(0)
            ElseIf is_pos < region_2_start Then
                table_num = bitstream3.g_side_info.table_select(gr)(ch)(1)
            Else
                table_num = bitstream3.g_side_info.table_select(gr)(ch)(2)
            End If

            ' Get next Huffman coded words
            MPG_Huffman_Decode(table_num, X, Y, V, W)

            If is_pos > 575 Then Exit For
            ' In the big_values area there are two freq lines per Huffman word
            bitstream3.g_main_data.isx(gr)(ch)(is_pos) = X
            is_pos += 1
            bitstream3.g_main_data.isx(gr)(ch)(is_pos) = Y
        Next

        ' Read small values until is_pos = 576 or we run out of huffman data
        table_num = bitstream3.g_side_info.count1table_select(gr)(ch) + 32
        For is_pos = bitstream3.g_side_info.big_values(gr)(ch) * 2 To 572
            If bitstream3.MPG_Get_Main_Pos() > bit_pos_end Then Exit For

            ' Get next Huffman coded words
            MPG_Huffman_Decode(table_num, X, Y, V, W)

            bitstream3.g_main_data.isx(gr)(ch)(is_pos + 0) = V
            bitstream3.g_main_data.isx(gr)(ch)(is_pos + 1) = W
            bitstream3.g_main_data.isx(gr)(ch)(is_pos + 2) = X
            bitstream3.g_main_data.isx(gr)(ch)(is_pos + 3) = Y
            is_pos += 3

            'g_main_data.isx(gr, ch, is_pos) = V
            'is_pos += 1
            'If (is_pos >= 576) Then Exit For

            'g_main_data.isx(gr, ch, is_pos) = W
            'is_pos += 1
            'If (is_pos >= 576) Then Exit For

            'g_main_data.isx(gr, ch, is_pos) = x
            'is_pos += 1
            'If (is_pos >= 576) Then Exit For

            'g_main_data.isx(gr, ch, is_pos) = Y
        Next

        ' Check that we didn't read past the end of this section
        If bitstream3.MPG_Get_Main_Pos() > (bit_pos_end + 1) Then
            ' Remove last words read
            is_pos -= 4
        End If

        ' Setup count1 which is the index of the first sample in the rzero reg.
        bitstream3.g_side_info.count1(gr)(ch) = is_pos

        ' Zero out the last part if necessary
        For is_pos = is_pos To 575 '  is_pos comes from last for-loop
            bitstream3.g_main_data.isx(gr)(ch)(is_pos) = 0.0
        Next

        ' Set the bitpos to point to the next part to read
        bitstream3.MPG_Set_Main_Pos(bit_pos_end + 1)
        'Catch
        'End Try
    End Sub


    ' Description: This function reads and decodes the next Huffman code word from the main_data bit reservoir.
    ' Parameters: Huffman table number and four pointers for the return values.
    ' Return value: Two (x, y) or four (x, y, v, w) decoded Huffman words.
    Private Function MPG_Huffman_Decode(ByVal table_num As Integer, ByRef X As Integer, ByRef Y As Integer, ByRef V As Integer, ByRef W As Integer) As Etc3.STATUS
        Dim Point As Integer = 0
        Dim bitsleft As Integer = 32
        Dim errorx As Boolean

        ' Check for empty tables
        If g_huffman_main(table_num)(1) = 0 Then
            X = 0
            Y = 0
            V = 0
            W = 0
            MPG_Huffman_Decode = Etc3.STATUS.sOK
            Exit Function
        End If

        Dim treelen As Integer = g_huffman_main(table_num)(1) ' treelen
        Dim linbits As Integer = g_huffman_main(table_num)(2) ' linbits
        Dim htptr As Integer = g_huffman_main(table_num)(0)

        ' Start reading the Huffman code word, bit by bit
        Do
            ' Check if we've matched a code word
            If (g_huffman_table(htptr + Point) And &HFFFF0000) = 0 Then
                errorx = False
                X = bitstream3.SHR(g_huffman_table(htptr + Point), 4) And 15
                Y = g_huffman_table(htptr + Point) And 15
                Exit Do
            End If

            If bitstream3.MPG_Get_Main_Bit() <> 0 Then ' Go right in tree
                Do While ((g_huffman_table(htptr + Point) And &HFFS) >= 250)
                    Point += g_huffman_table(htptr + Point) And 255
                Loop
                Point += CShort(g_huffman_table(htptr + Point) And 255)
            Else ' Go left in tree
                Do While (bitstream3.SHR(g_huffman_table(htptr + Point), 16) >= 250)
                    Point += bitstream3.SHR(g_huffman_table(htptr + Point), 16)
                Loop
                Point += bitstream3.SHR(g_huffman_table(htptr + Point), 16)
            End If

            bitsleft -= 1
        Loop While ((bitsleft > 0) AndAlso (Point < treelen))

        ' Check for error.
        If errorx Then
            'ErrOut("Illegal Huff code in data. bleft = %d, point = %d. tab = %d.", bitsleft, Point, table_num)
            X = 0
            Y = 0
        End If

        ' Process sign encodings for quadruples tables.
        If table_num > 31 Then
            V = bitstream3.SHR(Y, 3) And 1
            W = bitstream3.SHR(Y, 2) And 1
            X = bitstream3.SHR(Y, 1) And 1
            Y = Y And 1

            If (bitstream3.MPG_Get_Main_Bit() = 1) Then
                If V > 0 Then V = -V
                If W > 0 Then W = -W
                If X > 0 Then X = -X
                If Y > 0 Then Y = -Y
            End If
        Else
            ' Get linbits
            If (linbits > 0) And (X = 15) Then X += bitstream3.MPG_Get_Main_Bits(linbits)

            ' Get sign bit
            If X > 0 AndAlso (bitstream3.MPG_Get_Main_Bit() = 1) Then X = -X

            ' Get linbits
            If (linbits > 0) And (Y = 15) Then Y += bitstream3.MPG_Get_Main_Bits(linbits)

            ' Get sign bit
            If Y > 0 AndAlso (bitstream3.MPG_Get_Main_Bit() = 1) Then Y = -Y
        End If

        ' Done
        If errorx Then Return Etc3.STATUS.sError
    End Function
End Class