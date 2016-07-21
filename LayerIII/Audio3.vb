Imports System.Threading
Imports SlimDX.DirectSound
Imports SlimDX.Direct3D10
Imports SlimDX.Multimedia
Imports System.Runtime.InteropServices

Public Class Audio3
    <DllImport("user32.dll", CharSet:=CharSet.Auto, ExactSpelling:=True)> _
    Private Shared Function GetDesktopWindow() As IntPtr
    End Function

    Private bufPlayDesc As SoundBufferDescription
    Private playBuf As SecondarySoundBuffer
    Private notifySize As Integer
    Private numberPlaybackNotifications As Integer = 2
    Private nextPlaybackOffset As Integer
    Private mPlaybackPosition As Integer
    Private mIsInit As Boolean = False

    Public eventDecode As AutoResetEvent = New AutoResetEvent(False)

    'Dim rawFile As IO.FileStream
    Private audioBuffer() As Byte
    Private audioWriteBufferPosition As Integer
    Private mAudioReadBufferPosition As Integer
    Private Const safeBufferFactor As Integer = 1 * 16
    Private Const bufferSize As Integer = 576 * safeBufferFactor

    Public ReadOnly Property BufferDescription() As SoundBufferDescription
        Get
            Return bufPlayDesc
        End Get
    End Property

    Public ReadOnly Property PlayBuffer() As SecondarySoundBuffer
        Get
            Return playBuf
        End Get
    End Property

    Public ReadOnly Property Buffer() As Byte()
        Get
            Return audioBuffer
        End Get
    End Property

    Public Property WriteBufferPosition() As Integer
        Get
            Return audioWriteBufferPosition
        End Get
        Set(ByVal value As Integer)
            audioWriteBufferPosition = value
        End Set
    End Property

    Public ReadOnly Property ReadBufferPosition() As Integer
        Get
            Return mAudioReadBufferPosition
        End Get
    End Property

    Public ReadOnly Property PlaybackPosition() As Integer
        Get
            Return mPlaybackPosition
        End Get
    End Property

    Public ReadOnly Property IsInit As Boolean
        Get
            Return mIsInit
        End Get
    End Property

    Public Sub Write()
        Dim lockSize As Integer

        lockSize = playBuf.CurrentWritePosition - nextPlaybackOffset
        If lockSize < 0 Then lockSize += bufPlayDesc.SizeInBytes

        ' Block align lock size so that we always read on a boundary
        lockSize -= lockSize Mod notifySize
        If lockSize = 0 Then Exit Sub

        playBuf.Write(audioBuffer, nextPlaybackOffset, LockFlags.None)
        ' TODO: CMP3Decoder should implement the ability to write to a WAV file
        'rawFile.Write(intBuffer, 0, intBuffer.Length)

        mAudioReadBufferPosition = nextPlaybackOffset
        mPlaybackPosition = playBuf.CurrentPlayPosition

        nextPlaybackOffset += audioBuffer.Length
        nextPlaybackOffset = nextPlaybackOffset Mod bufPlayDesc.SizeInBytes ' Circular buffer
    End Sub

    Public Sub InitDS(parentControlHandle As IntPtr, bufferMultiplier As Integer, decoder3 As Decoder3)
        ReDim audioBuffer((bufferSize * bufferMultiplier) - 1)
        ReDim decoder3.outBuffer(576 * 2 - 1)

        ' Define the capture format
        Dim format As WaveFormat = New WaveFormat()
        With format
            .BitsPerSample = 16
            .Channels = 2
            .FormatTag = WaveFormatTag.Pcm
            .SamplesPerSecond = 44100
            .BlockAlignment = CShort(.Channels * .BitsPerSample / 8)
            .AverageBytesPerSecond = .SamplesPerSecond * .BlockAlignment
        End With

        ' Define the size of the notification chunks
        notifySize = audioBuffer.Length
        notifySize -= notifySize Mod format.BlockAlignment

        ' Create a buffer description object
        bufPlayDesc = New SoundBufferDescription()
        With bufPlayDesc
            .Format = format
            .Flags = BufferFlags.ControlPositionNotify Or
                    BufferFlags.GetCurrentPosition2 Or
                    BufferFlags.GlobalFocus Or
                    BufferFlags.Static Or
                    BufferFlags.ControlVolume Or
                    BufferFlags.ControlPan Or
                    BufferFlags.ControlFrequency
            .SizeInBytes = notifySize * numberPlaybackNotifications
        End With

        Dim audioDev As DirectSound = New DirectSound()
        Dim windowHandle As IntPtr = GetDesktopWindow() ' Me.Handle
        audioDev.SetCooperativeLevel(windowHandle, CooperativeLevel.Priority)
        playBuf = New SecondarySoundBuffer(audioDev, bufPlayDesc)

        ' Define the notification events
        Dim np(numberPlaybackNotifications - 1) As NotificationPosition

        For i As Integer = 0 To numberPlaybackNotifications - 1
            np(i) = New NotificationPosition()
            np(i).Offset = (notifySize * i) + notifySize - 1
            np(i).Event = eventDecode
        Next
        playBuf.SetNotificationPositions(np)

        nextPlaybackOffset = 0
        playBuf.Play(0, PlayFlags.Looping)

        'If IO.File.Exists("test.raw") Then IO.File.Delete("test.raw")
        'rawFile = IO.File.Create("test.raw")

        mIsInit = True
    End Sub

    Public Sub [Stop]()
        playBuf.Stop()
    End Sub
End Class