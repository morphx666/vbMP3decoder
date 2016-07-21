Imports System.Threading
Imports System.ComponentModel

Public Class CMP3Decoder
    Implements IDisposable

    Public Enum DecoderStateConstants
        Idle = 0
        Playing = 1
        Paused = 2
    End Enum

    Public Event PositionChanged()
    Public Event StateChanged()
    Public Event NewBuffer()
    Public Event DecodingProgress(progress As Single)

    Private mFileName As String
    Private mOwner As Windows.Forms.Control
    Private mState As DecoderStateConstants = DecoderStateConstants.Idle
    Private mRate As Integer = 100
    Private mVolume As Integer = 100
    Private mBufferMultiplier As Integer = 1
    Private mNormalizedBuffer() As Integer

    Private decodeThread As Thread
    Private decoderIsBusy As Boolean = False
    Private abortDecoder As Boolean = False
    Private isDirty As Boolean

    Private posOffset As Integer

    Private audio3 As Audio3 = New Audio3()
    Private bitstream3 As Bitstream3 = New Bitstream3()
    Private etc3 As Etc3 = New Etc3()
    Private huffman3 As Huffman3 = New Huffman3()
    Private decoder3 As Decoder3 = New Decoder3()

    Private mDecodeBuffer As List(Of Integer) = New List(Of Integer)

    Private Enum EventConstants
        PositionChanged
        StateChanged
        NewBuffer
        DecodingProgress
    End Enum

    Private Delegate Sub SafeRaiseEventDel(ByRef args() As Object)
    Private Sub SafeRaiseEventProc(ByRef args() As Object)
        Select Case CType(args(0), EventConstants)
            Case EventConstants.PositionChanged
                RaiseEvent PositionChanged()
            Case EventConstants.StateChanged
                RaiseEvent StateChanged()
            Case EventConstants.NewBuffer
                RaiseEvent NewBuffer()
            Case EventConstants.DecodingProgress
                RaiseEvent DecodingProgress(CSng(Position / FileLength * 100))
        End Select
    End Sub

    Public Sub New(ownerForm As Windows.Forms.Control)
        mOwner = ownerForm
    End Sub

    Public Property FileName As String
        Get
            Return mFileName
        End Get
        Set(ByVal value As String)
            If mState = DecoderStateConstants.Playing Then Me.Stop()
            mFileName = value
            ScanFile()
        End Set
    End Property

    Public Property Volume As Integer
        Get
            Return mVolume
        End Get
        Set(ByVal value As Integer)
            mVolume = value
            SetRateAndVolume()
        End Set
    End Property

    Public Property Rate As Integer
        Get
            Return mRate
        End Get
        Set(ByVal value As Integer)
            mRate = value
            SetRateAndVolume()
        End Set
    End Property

    Public ReadOnly Property Buffer As Byte()
        Get
            Return audio3.Buffer
        End Get
    End Property

    Public ReadOnly Property NormalizedBuffer As Integer()
        Get
            Return mNormalizedBuffer
        End Get
    End Property

    Public ReadOnly Property Channels As Short
        Get
            Return audio3.PlayBuffer.Format.Channels
        End Get
    End Property

    Public ReadOnly Property BitDepth As Short
        Get
            Return audio3.PlayBuffer.Format.BitsPerSample
        End Get
    End Property

    Public Property BufferMultiplier As Integer
        Get
            Return mBufferMultiplier
        End Get
        Set(ByVal value As Integer)
            mBufferMultiplier = value
        End Set
    End Property

    Public ReadOnly Property Frequency As Integer
        Get
            Return audio3.PlayBuffer.Format.SamplesPerSecond
        End Get
    End Property

    Private Function NormalizeBuffer(source() As Byte) As Integer()
        'Try
        Dim f As SlimDX.Multimedia.WaveFormat = audio3.BufferDescription.Format
        Dim dataSize As Integer = f.BitsPerSample \ 8
        Dim bb(source.Length \ dataSize - 1) As Integer
        Dim cycleStep As Short = CShort(If(f.BitsPerSample = 8, f.Channels, f.Channels * 2))
        Dim rtChOffset As Integer = If(f.Channels = 1, 0, dataSize)
        Dim tmpB(dataSize - 1) As Byte
        Dim bbStep As Integer = 0

        For i As Integer = 0 To source.Length - rtChOffset - 1 Step cycleStep
            Select Case f.BitsPerSample
                Case 8
                    bb(bbStep) += ((128 - source(i)) * 256)

                    If f.Channels = 2 Then
                        bb(bbStep + 1) = (128 - source(i + rtChOffset)) * 256
                    End If
                Case 16
                    Array.Copy(source, i, tmpB, 0, dataSize)
                    bb(bbStep) = System.BitConverter.ToInt16(tmpB, 0)

                    If f.Channels = 2 Then
                        Array.Copy(source, i + rtChOffset, tmpB, 0, dataSize)
                        bb(bbStep + 1) = System.BitConverter.ToInt16(tmpB, 0)
                    End If
            End Select

            bbStep += f.Channels
        Next i

        Return bb
        'Catch e As System.Exception
        '    LogMessage(ErrorConstants.NormalizeBufferEx, e.StackTrace, e.Message)
        'End Try

        'Return Nothing
    End Function

    Private Sub SetRateAndVolume()
        If audio3.PlayBuffer IsNot Nothing Then
            audio3.PlayBuffer.Volume = CInt(-10000 * (100 - mVolume) / 100)
            audio3.PlayBuffer.Frequency = CInt(audio3.BufferDescription.Format.SamplesPerSecond * (mRate / 100))
        End If
    End Sub

    Public ReadOnly Property State() As DecoderStateConstants
        Get
            Return mState
        End Get
    End Property

    Public ReadOnly Property DecodeBuffer As Integer()
        Get
            Return mDecodeBuffer.ToArray()
        End Get
    End Property

    Public Sub Play()
        SafeStop()
        If IO.File.Exists(mFileName) Then
            InitDecoder(False)
            decodeThread = New Thread(AddressOf DecodeMP3)
            decodeThread.SetApartmentState(ApartmentState.MTA)
            decodeThread.Priority = ThreadPriority.AboveNormal
            decodeThread.Name = "xfxMP3Decoder"
            decodeThread.Start()

            SetRateAndVolume()

            ChangeState(DecoderStateConstants.Playing)
        End If
    End Sub

    Private Sub Decode()
        SafeStop()
        If IO.File.Exists(mFileName) Then
            decoderIsBusy = False
            InitDecoder(True)
            mDecodeBuffer.Clear()

            decodeThread = New Thread(Sub()
                                          DoDecode(False)
                                          bitstream3.MPG_Position = posOffset
                                      End Sub
            )

            decodeThread.SetApartmentState(ApartmentState.MTA)
            decodeThread.Priority = ThreadPriority.AboveNormal
            decodeThread.Name = "xfxMP3Decoder"
            decodeThread.Start()
        End If
    End Sub

    Private Sub InitDecoder(skipAudioInit As Boolean)
        FirstTimeInit()
        bitstream3.Init(mFileName, decoder3, huffman3, bitstream3, audio3)
        If Not skipAudioInit Then audio3.InitDS(mOwner.Handle, mBufferMultiplier, decoder3)
        etc3.Init(decoder3, bitstream3, audio3, huffman3)
        huffman3.Init(bitstream3)

        abortDecoder = False
    End Sub

    Private Sub SafeStop()
        If decoderIsBusy Then
            [Stop]()
            Wait4Decoder()
            decodeThread = Nothing
        End If
    End Sub

    Private Sub Wait4Decoder()
        Do While decoderIsBusy
            Windows.Forms.Application.DoEvents()
        Loop
    End Sub

    Public Sub [Stop]()
        If decoderIsBusy Then
            If mState = DecoderStateConstants.Paused Then Pause()
            abortDecoder = True
            Wait4Decoder()
            audio3.PlayBuffer.Stop()
            Position = 0
        End If

        mOwner.Invoke(New SafeRaiseEventDel(AddressOf SafeRaiseEventProc), New Object() {New Object() {EventConstants.PositionChanged}})
        ChangeState(DecoderStateConstants.Idle)
    End Sub

    Public Sub Pause()
        If decoderIsBusy Then
            If mState = DecoderStateConstants.Paused Then
                audio3.PlayBuffer.Play(0, SlimDX.DirectSound.PlayFlags.Looping)

                ChangeState(DecoderStateConstants.Playing)
            ElseIf mState = DecoderStateConstants.Playing Then
                audio3.PlayBuffer.Stop()

                ChangeState(DecoderStateConstants.Paused)
            End If
        End If
    End Sub

    Public ReadOnly Property FileLength() As Integer
        Get
            Return bitstream3.MPG_Get_Filesize
        End Get
    End Property

    Public Property Position() As Integer
        Get
            Return bitstream3.MPG_Position - posOffset
        End Get
        Set(ByVal value As Integer)
            isDirty = True
            Array.Clear(audio3.Buffer, 0, audio3.Buffer.Length)
            audio3.WriteBufferPosition = 0
            bitstream3.MPG_Position = value + posOffset
            audio3.eventDecode.Set()
        End Set
    End Property

    Public ReadOnly Property ReadBufferPosition() As Double
        Get
            Return audio3.ReadBufferPosition / audio3.BufferDescription.SizeInBytes
        End Get
    End Property

    Public ReadOnly Property PlaybackPosition() As Double
        Get
            Return audio3.PlaybackPosition / audio3.BufferDescription.SizeInBytes
        End Get
    End Property

    Public ReadOnly Property BitRate() As Integer
        Get
            Return bitstream3.BitRate
        End Get
    End Property

    Public ReadOnly Property PosToTime(position As Integer) As Double
        Get
            Return (position * 8) / (BitRate / 1000) / 1000
        End Get
    End Property

    Public ReadOnly Property PosToFormattedTime(position As Integer) As TimeSpan
        Get
            Return TimeSpan.FromSeconds(PosToTime(position))
        End Get
    End Property

    Public ReadOnly Property Duration() As Double
        Get
            Return PosToTime(FileLength)
        End Get
    End Property

    Private Sub FirstTimeInit()
        Static Ready As Boolean
        If Ready Then Exit Sub
        Ready = True

        bitstream3.g_side_info.Initialize()
        bitstream3.g_main_data.Initialize()
    End Sub

    Private Sub ChangeState(ByVal NewState As DecoderStateConstants)
        mState = NewState
        mOwner.Invoke(New SafeRaiseEventDel(AddressOf SafeRaiseEventProc), New Object() {New Object() {EventConstants.StateChanged}})
    End Sub

    Private Sub ScanFile()
        SafeStop()
        If IO.File.Exists(mFileName) Then
            InitDecoder(True)

            If bitstream3.MPG_Position() <> etc3.STATUS.C_MPG_EOF AndAlso abortDecoder <> True Then
                Do
                    If bitstream3.MPG_Read_Frame() = etc3.STATUS.sOK Then
                        Try
                            decoder3.MPG_Decode_L3()
                        Catch ex As Exception
                            Debug.WriteLine("ScanFile: " + ex.Message)
                        End Try

                        posOffset = bitstream3.MPG_Position
                        Exit Do
                    End If
                Loop
            End If
        End If
    End Sub

    <MTAThread()>
    Private Sub DecodeMP3()
        decoderIsBusy = True

        Do While bitstream3.MPG_Position() <> etc3.STATUS.C_MPG_EOF AndAlso
                 decodeThread.ThreadState = ThreadState.Running AndAlso
                 abortDecoder <> True

            DoDecode(True)
        Loop

        audio3.Stop()
        decoderIsBusy = False

        Me.Stop()
    End Sub

    Private Sub DoDecode(playAudio As Boolean)
        Dim isEOF As Boolean = False

        Do
            Do While abortDecoder <> True
                If bitstream3.MPG_Read_Frame() = etc3.STATUS.sOK Then
                    Try
                        decoder3.MPG_Decode_L3()
                    Catch ex As Exception
                        Debug.WriteLine("DecodeMP3: " + ex.Message)

                        isDirty = True
                        Exit Do
                    End Try

                    If playAudio Then
                        audio3.WriteBufferPosition += decoder3.outBuffer.Length * 2 * 2
                        If audio3.WriteBufferPosition >= audio3.Buffer.Length Then
                            audio3.WriteBufferPosition = 0
                            Exit Do
                        End If
                    Else
                        Exit Do
                    End If
                ElseIf bitstream3.MPG_Position() = etc3.STATUS.C_MPG_EOF Then
                    isEOF = True
                    Exit Do
                End If
            Loop

            If playAudio Then audio3.eventDecode.WaitOne(Timeout.Infinite, False)
            If abortDecoder Then Exit Sub

            If isDirty Then
                isDirty = False
            Else
                If playAudio Then
                    audio3.Write()
                Else
                    mDecodeBuffer.AddRange(decoder3.outBuffer)
                    mOwner.Invoke(New SafeRaiseEventDel(AddressOf SafeRaiseEventProc), New Object() {New Object() {EventConstants.DecodingProgress}})
                End If
            End If

            If playAudio Then
                If Me.ReadBufferPosition = 0 Then
                    mNormalizedBuffer = NormalizeBuffer(audio3.Buffer)
                    mOwner.Invoke(New SafeRaiseEventDel(AddressOf SafeRaiseEventProc), New Object() {New Object() {EventConstants.NewBuffer}})
                End If
                mOwner.Invoke(New SafeRaiseEventDel(AddressOf SafeRaiseEventProc), New Object() {New Object() {EventConstants.PositionChanged}})
            ElseIf isEOF Then
                Exit Do
            End If
        Loop While Not playAudio
    End Sub

    Public ReadOnly Property WriteCursor() As Double
        Get
            Return audio3.PlayBuffer.CurrentWritePosition / audio3.BufferDescription.SizeInBytes
        End Get
    End Property

    Public ReadOnly Property ReadCursor() As Double
        Get
            Return audio3.PlayBuffer.CurrentPlayPosition / audio3.BufferDescription.SizeInBytes
        End Get
    End Property

#Region " IDisposable Support "
    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free unmanaged resources when explicitly called
            End If

            ' TODO: free shared unmanaged resources
        End If
        [Stop]()
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
