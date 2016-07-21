Public Class DXVUMeterNETGDI
    ''' <summary>
    ''' Defines the recording formats supported by DXVUMeterNETGDI
    ''' </summary>
    Public Enum RecordingFormatConstants
        ''' <summary>Uncompressed audio format</summary>
        WAV
        ''' <summary>Compressed MPEG-2 Audio Layer 3 audio format using the LAME encoder</summary>
        ''' <remarks>DXVUMeterNETGDI does not include the Lame_enc.dll so you should make sure that this file exists on one these locations:
        ''' <list type="bullet">
        ''' <item><description>In the same directory as DXVUMeterNETGDI.dll</description></item>
        ''' <item><description>In a directory that is included in the PATH variable</description></item>
        ''' <item><description>In the %windir%\system32 directory</description></item>
        ''' </list>
        ''' </remarks>
        MP3
    End Enum

    ''' <summary>Defines the style used by DXVUMeterNETGDI to render the monitored audio</summary>
    ''' <seealso cref="Style"></seealso>
    ''' <seealso cref="DXVUMeterNETGDI.EnableRendering"></seealso>
    Public Enum StyleConstants
        ''' <summary>Display the Digital VUs using leds</summary>
        DigitalVU
        ''' <summary>Display the Oscilloscope (waveform)</summary>
        Oscilloscope
        ''' <summary>Display the Audio in the Frequency domain analyzer</summary>
        FFT
        ''' <summary>Allows the use of GDI+ to render a custom display</summary>
        UserPaintGDI
    End Enum

    ''' <summary>Defines the monitoring state of the control</summary>
    Public Enum MonitoringStateConstants
        ''' <summary>The control is not monitoring</summary>
        Idle = 0
        ''' <summary>The control is monitoring</summary>
        Monitoring = 1
    End Enum

    ''' <summary>Defines the recording state of the control</summary>
    Public Enum RecordingStateConstants
        ''' <summary>The control is not recording</summary>
        Idle = 0
        ''' <summary>The control is recording</summary>
        Recording = 1
    End Enum

    ''' <summary>Defines the playback state of the control</summary>
    Public Enum PlaybackStateConstants
        ''' <summary>The control is not playing</summary>
        Idle = 0
        ''' <summary>The control is playing</summary>
        Playing = 1
        ''' <summary>The control is paused</summary>
        Paused = 2
    End Enum

    ''' <summary>Defines the Spectrum style</summary>
    Public Enum FFTStyleConstants
        ''' <summary>The FFT will be rendered using lines</summary>
        Line = 0
        ''' <summary>The FFT will be rendered using a filled polygon</summary>
        Filled = 1
        ''' <summary>The FFT will be rendered using bars</summary>
        Bars = 2
        ''' <summary>The FFT will be rendered using filled bars</summary>
        FilledBars = 3
        ''' <summary>The FFT will be rendered using bars in the form of leds</summary>
        LedBars = 4
        ''' <summary>The FFT will be rendered as an spectrum</summary>
        Spectrum = 5
        ''' <summary>The textbook definition of the transfer function of a linear time-interval discrete-time filter is defined by Y(z)/X(z), where Y(z) denotes the z transform of the filter output signal y(n) (left channel), and X(z) denotes the z transform of the filter input signal x(n) (right channel).
        ''' Note that due to the complexity of this analysis, when in this mode, DXUMeterNETGDI will consume a lot of CPU cycles.
        ''' For more information you may want to review these resources:
        ''' <list type="bullet">
        ''' <item><description><a href="http://www.mathworks.com/help/toolbox/signal/tfestimate.html">http://www.mathworks.com/help/toolbox/signal/tfestimate.html</a></description></item>
        ''' <item><description><a href="http://www.dsprelated.com/dspbooks/filters/Transfer_Function_Analysis.html">http://www.dsprelated.com/dspbooks/filters/Transfer_Function_Analysis.html</a></description></item>
        ''' </list>
        ''' </summary>
        TransferFunction = 6
    End Enum

    ''' <summary></summary>
    Public Enum FFTLineChannelModeConstants
        ''' <summary>For stereo signals, the left channel is displayed at the top and the right channel at the bottom. For mono signals, just the left channel is displayed</summary>
        Normal = 0
        ''' <summary>Both channels are displayed on the middle, but the left channel appears over the right channel</summary>
        LeftOverRight = 1
        ''' <summary>Both channels are displayed on the middle, but the right channel appears over the left channel</summary>
        RightOverLeft = 2
        ''' <summary>In this mode, DXVUMeterNETGDI will display the phase between the left and right channels</summary>
        Phase = 3
    End Enum

    ''' <summary>Defines how the Y scale (level) of the FFT will be rendered</summary>
    Public Enum FFTYScaleConstants
        ''' <summary>Magnitude is calculated as √(2•FFT)</summary>
        Magnitude = 0
        ''' <summary>Amplitude is calculated as 2√(2•FFT)</summary>
        Amplitude = 1
        ''' <summary>dB is calculated as 10•log10(2•FFT)</summary>
        dB = 2
        ''' <summary>PSDTimeInt is calculated as 2•index•(2•FFT)/n</summary>
        PSDTimeInt = 3
    End Enum

    ''' <summary>Defines how the X scale (time) of the FFT will be rendered</summary>
    Public Enum FFTXScaleConstants
        ''' <summary>Linear scale</summary>
        Normal = 0
        ''' <summary>Logarithmic scale</summary>
        Logarithmic = 1
    End Enum

    ''' <summary>Defines the size of the Fast Fourier Transform</summary>
    Public Enum FFTSizeConstants
        ''' <summary>32 bands</summary>
        FFTs32 = 32
        ''' <summary>64 bands</summary>
        FFTs64 = 64
        ''' <summary>128 bands</summary>
        FFTs128 = 128
        ''' <summary>256 bands</summary>
        FFTs256 = 256
        ''' <summary>512 bands</summary>
        FFTs512 = 512
        ''' <summary>1024 bands</summary>
        FFTs1024 = 1024
        ''' <summary>2048 bands</summary>
        FFTs2048 = 2048
        ''' <summary>4096 bands</summary>
        FFTs4096 = 4096
        ''' <summary>8192 bands</summary>
        FFTs8192 = 8192
        ''' <summary>16384 bands</summary>
        FFTs16384 = 16384
        ''' <summary>32768 bands</summary>
        FFTs32768 = 32768
    End Enum

    ''' <summary>Defines the type of window to apply to the Fast Fourier Transform</summary>
    ''' <remarks>Windows provide means of refining the transform to enhance or reduce various aspects of the waveform in order to better interpret the signal response.
    ''' Refer to the <see href="http://mathworld.wolfram.com/ApodizationFunction.html">Wolfram Research web site</see> for further information.</remarks>
    Public Enum FFTWindowConstants
        ''' <summary>No windowing is applied</summary>
        None = 0
        ''' <summary>Apply a basic triangle window</summary>
        Triangle = 1
        ''' <summary>Apply the Julius von Hann window</summary>
        Hanning = 2
        ''' <summary>Apply the Richard Hamming window</summary>
        Hamming = 3
        ''' <summary>Apply the Welch window</summary>
        Welch = 4
        ''' <summary>Apply the Gaussian window</summary>
        Gaussian = 5
        ''' <summary>Apply the Blackman window</summary>
        Blackman = 6
        ''' <summary>Apply the Parzen window</summary>
        Parzen = 7
        ''' <summary>Apply the Bartlett window</summary>
        Bartlett = 8
        ''' <summary>Apply the Cones window</summary>
        Connes = 9
        ''' <summary>Apply the KaiserBessel window</summary>
        KaiserBessel = 10
        ''' <summary>Apply the BlackmanHarris window</summary>
        BlackmanHarris = 11
        ''' <summary>Apply the Nuttall window</summary>
        Nuttall = 12
        ''' <summary>Apply the BlackmanNuttall window</summary>
        BlackmanNuttall = 13
        ''' <summary>Apply the FlatTop window</summary>
        FlatTop = 14
    End Enum

    ''' <summary>Controls which, if any, of the FFT scales are displayed.</summary>
    Public Enum FFTRenderScalesConstants
        ''' <summary>Both, the frequency and power scales will be displayed.</summary>
        Both = 0
        ''' <summary>Only the power scale will be displayed.</summary>
        Vertical = 1
        ''' <summary>Only the frequency scale will be displayed.</summary>
        Horizontal = 2
        ''' <summary>Scales will not be rendered.</summary>
        None = 3
    End Enum

    ''' <summary>Defines the error code reported by the Error event</summary>
    Public Enum ErrorConstants
        ''' <summary>This exception indicates a problem related to the cursor display under the <see cref="DXVUMeterNETGDI.StyleConstants">FFT</see> display mode</summary>
        CursorPosEx
        ''' <summary>This exception indicates that a problem has occurred while trying to initialize the color cache for the leds in the <see cref="DXVUMeterNETGDI.StyleConstants">DigitalVU</see> mode</summary>
        DefineColorsEx
        ''' <summary>This exception indicates that a problem occurred while DXVUMeterNETGDI tried to initialize DirectSound to start playing a WAV file</summary>
        StartPlayingEx
        ''' <summary>This exception indicates that a problem occurred while DXVUMeterNETGDI tried to release all the resources used by DirectSound while playing a WAV file</summary>
        StopPlayingEx
        ''' <summary>This exception indicates that a problem occurred while DXVUMeterNETGDI tried to initialize DirectSound and Direct3D to start monitoring audio</summary>
        StartMonitoringEx
        ''' <summary>This exception indicates that a problem occurred while DXVUMeterNETGDI tried to release the resources used by DirectSound and Direct3D</summary>
        StopMonitoringEx
        ''' <summary></summary>
        ResumeIdleRenderEx
        ''' <summary></summary>
        ReadCaptureDataEx
        ''' <summary></summary>
        NormalizeBufferEx
        ''' <summary></summary>
        InitFFTEx
        ''' <summary></summary>
        RenderFFTScaleEx
        ''' <summary></summary>
        RenderFFTLinesEx
        ''' <summary></summary>
        RenderFFTBarsEx
        ''' <summary></summary>
        RenderOSCEx
        ''' <summary></summary>
        CalculateRMSEx
        ''' <summary></summary>
        RenderDigitalVUsEx
        ''' <summary></summary>
        InternalEx
        ''' <summary></summary>
        ResetLedsDataEx
        ''' <summary></summary>
        RendererResetEx
        ''' <summary></summary>
        MP3EncoderEx
        ''' <summary></summary>
        StyleChangeEx
    End Enum

    ''' <summary>Defines the properties of the Digital VU display</summary>
    Public Enum OrientationConstants
        ''' <summary>The DigitalVU will appear vertical and each channel will appear in two vertical columns.</summary>
        Vertical = 0
        ''' <summary>The DigitalVU will appear horizontal and each channel will appear in two horizontal rows.</summary>
        Horizontal = 1
    End Enum

    ''' <summary>Represents all the possible DTMF codes that can be detected</summary>
    Public Enum DTMFToneConstants
        ''' <summary>Digit 1</summary>
        DTMF01 = 0 * 7 + 0
        ''' <summary>Digit 2</summary>
        DTMF02 = 0 * 7 + 1
        ''' <summary>Digit 3</summary>
        DTMF03 = 0 * 7 + 2
        ''' <summary>A symbol</summary>
        DTMFA = 0 * 7 + 3
        ''' <summary>Busy Signal</summary>
        DTMFBusyTone = 4 * 7 + 4

        ''' <summary>Digit 4</summary>
        DTMF04 = 1 * 7 + 0
        ''' <summary>Digit 5</summary>
        DTMF05 = 1 * 7 + 1
        ''' <summary>Digit 6</summary>
        DTMF06 = 1 * 7 + 2
        ''' <summary>B symbol</summary>
        DTMFB = 1 * 7 + 3
        ''' <summary>Ringback Tone (US)</summary>
        DTMFRingbackTone = 5 * 7 + 5

        ''' <summary>Digit 7</summary>
        DTMF07 = 2 * 7 + 0
        ''' <summary>Digit 8</summary>
        DTMF08 = 2 * 7 + 1
        ''' <summary>Digit 9</summary>
        DTMF09 = 2 * 7 + 2
        ''' <summary>C symbol</summary>
        DTMFC = 2 * 7 + 3
        ''' <summary>Dial Tone</summary>
        DTMFDialTone = 6 * 7 + 6

        ''' <summary>* symbol</summary>
        DTMFStar = 3 * 7 + 0
        ''' <summary>Digit 0</summary>
        DTMF00 = 3 * 7 + 1
        ''' <summary># symbol</summary>
        DTMFPound = 3 * 7 + 2
        ''' <summary>D symbol</summary>
        DTMFD = 3 * 7 + 3

        ''' <summary>Invalid (for internal use)</summary>
        DTMFInvalid = -1
    End Enum
End Class
