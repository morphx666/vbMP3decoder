Public Class frmMain
    Inherits System.Windows.Forms.Form

#Region "Windows Form Designer generated code "
    Public Sub New()
        MyBase.New()
        If m_vb6FormDefInstance Is Nothing Then
            If m_InitializingDefInstance Then
                m_vb6FormDefInstance = Me
            Else
                Try
                    'For the start-up form, the first instance created is the default instance.
                    If System.Reflection.Assembly.GetExecutingAssembly.EntryPoint.DeclaringType Is Me.GetType Then
                        m_vb6FormDefInstance = Me
                    End If
                Catch
                End Try
            End If
        End If
        'This is required by the Windows Form Designer.
        InitializeComponent()
    End Sub
    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal Disposing As Boolean)
        If Disposing Then
            If Not components Is Nothing Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(Disposing)
    End Sub
    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Friend WithEvents txtFileName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents tbPos As System.Windows.Forms.TrackBar
    Friend WithEvents btnPlay As System.Windows.Forms.Button
    Friend WithEvents btnPause As System.Windows.Forms.Button
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents tbVol As System.Windows.Forms.TrackBar
    Friend WithEvents tbRate As System.Windows.Forms.TrackBar
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblBitRate As System.Windows.Forms.Label
    Friend WithEvents lblDuration As System.Windows.Forms.Label
    Friend WithEvents VuL As MP3Player.VU
    Friend WithEvents VuR As MP3Player.VU
    Friend WithEvents DxvuMeterNETGDI1 As NDXVUMeterNET.DXVUMeterNETGDI
    Friend WithEvents btnClose As System.Windows.Forms.Button
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.txtFileName = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.tbPos = New System.Windows.Forms.TrackBar()
        Me.tbVol = New System.Windows.Forms.TrackBar()
        Me.tbRate = New System.Windows.Forms.TrackBar()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.lblBitRate = New System.Windows.Forms.Label()
        Me.lblDuration = New System.Windows.Forms.Label()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.btnPause = New System.Windows.Forms.Button()
        Me.btnPlay = New System.Windows.Forms.Button()
        Me.DxvuMeterNETGDI1 = New NDXVUMeterNET.DXVUMeterNETGDI()
        Me.VuR = New MP3Player.VU()
        Me.VuL = New MP3Player.VU()
        CType(Me.tbPos, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbVol, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.tbRate, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtFileName
        '
        Me.txtFileName.Location = New System.Drawing.Point(12, 25)
        Me.txtFileName.Name = "txtFileName"
        Me.txtFileName.ReadOnly = True
        Me.txtFileName.Size = New System.Drawing.Size(292, 22)
        Me.txtFileName.TabIndex = 0
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(50, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "MP3 File"
        '
        'btnBrowse
        '
        Me.btnBrowse.Location = New System.Drawing.Point(310, 25)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(30, 21)
        Me.btnBrowse.TabIndex = 2
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(261, 307)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(79, 29)
        Me.btnClose.TabIndex = 3
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'tbPos
        '
        Me.tbPos.Location = New System.Drawing.Point(12, 87)
        Me.tbPos.Maximum = 100
        Me.tbPos.Name = "tbPos"
        Me.tbPos.Size = New System.Drawing.Size(328, 45)
        Me.tbPos.TabIndex = 4
        Me.tbPos.TickFrequency = 5
        '
        'tbVol
        '
        Me.tbVol.AutoSize = False
        Me.tbVol.Location = New System.Drawing.Point(80, 233)
        Me.tbVol.Maximum = 100
        Me.tbVol.Name = "tbVol"
        Me.tbVol.Size = New System.Drawing.Size(260, 31)
        Me.tbVol.TabIndex = 8
        Me.tbVol.TickFrequency = 5
        Me.tbVol.Value = 100
        '
        'tbRate
        '
        Me.tbRate.AutoSize = False
        Me.tbRate.Location = New System.Drawing.Point(80, 270)
        Me.tbRate.Maximum = 200
        Me.tbRate.Name = "tbRate"
        Me.tbRate.Size = New System.Drawing.Size(260, 31)
        Me.tbRate.TabIndex = 9
        Me.tbRate.TickFrequency = 10
        Me.tbRate.Value = 100
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(17, 242)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(46, 13)
        Me.Label2.TabIndex = 10
        Me.Label2.Text = "Volume"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(17, 279)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(30, 13)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "Rate"
        '
        'lblBitRate
        '
        Me.lblBitRate.AutoSize = True
        Me.lblBitRate.Location = New System.Drawing.Point(239, 49)
        Me.lblBitRate.Name = "lblBitRate"
        Me.lblBitRate.Size = New System.Drawing.Size(67, 13)
        Me.lblBitRate.TabIndex = 12
        Me.lblBitRate.Text = "BitRate: n/a"
        Me.lblBitRate.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'lblDuration
        '
        Me.lblDuration.AutoSize = True
        Me.lblDuration.Location = New System.Drawing.Point(12, 49)
        Me.lblDuration.Name = "lblDuration"
        Me.lblDuration.Size = New System.Drawing.Size(76, 13)
        Me.lblDuration.TabIndex = 13
        Me.lblDuration.Text = "Duration: n/a"
        '
        'btnStop
        '
        Me.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.btnStop.Image = Global.MP3Player.My.Resources.Resources.btnStop
        Me.btnStop.Location = New System.Drawing.Point(80, 125)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(28, 26)
        Me.btnStop.TabIndex = 7
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'btnPause
        '
        Me.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.btnPause.Image = Global.MP3Player.My.Resources.Resources.btnPause
        Me.btnPause.Location = New System.Drawing.Point(46, 125)
        Me.btnPause.Name = "btnPause"
        Me.btnPause.Size = New System.Drawing.Size(28, 26)
        Me.btnPause.TabIndex = 6
        Me.btnPause.UseVisualStyleBackColor = True
        '
        'btnPlay
        '
        Me.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.btnPlay.Image = Global.MP3Player.My.Resources.Resources.btnPlay
        Me.btnPlay.Location = New System.Drawing.Point(12, 125)
        Me.btnPlay.Name = "btnPlay"
        Me.btnPlay.Size = New System.Drawing.Size(28, 26)
        Me.btnPlay.TabIndex = 5
        Me.btnPlay.UseVisualStyleBackColor = True
        '
        'DxvuMeterNETGDI1
        '
        Me.DxvuMeterNETGDI1.BackColor = System.Drawing.Color.Black
        Me.DxvuMeterNETGDI1.BitDepth = CType(16, Short)
        Me.DxvuMeterNETGDI1.Channels = CType(2, Short)
        Me.DxvuMeterNETGDI1.EnableRendering = True
        Me.DxvuMeterNETGDI1.FFTDetectDTMF = False
        Me.DxvuMeterNETGDI1.FFTHighPrecisionMode = False
        Me.DxvuMeterNETGDI1.FFTHistorySize = 4
        Me.DxvuMeterNETGDI1.FFTHoldMaxPeaks = False
        Me.DxvuMeterNETGDI1.FFTHoldMinPeaks = False
        Me.DxvuMeterNETGDI1.FFTLineChannelMode = NDXVUMeterNET.DXVUMeterNETGDI.FFTLineChannelModeConstants.Normal
        Me.DxvuMeterNETGDI1.FFTNormalized = True
        Me.DxvuMeterNETGDI1.FFTPeaksDecayDelay = 10
        Me.DxvuMeterNETGDI1.FFTPeaksDecaySpeed = 20
        Me.DxvuMeterNETGDI1.FFTPlotNoiseReduction = 0
        Me.DxvuMeterNETGDI1.FFTRenderScales = NDXVUMeterNET.DXVUMeterNETGDI.FFTRenderScalesConstants.None
        Me.DxvuMeterNETGDI1.FFTScaleFont = New System.Drawing.Font("Tahoma", 8.0!)
        Me.DxvuMeterNETGDI1.FFTShowDecay = False
        Me.DxvuMeterNETGDI1.FFTShowMinMaxRange = False
        Me.DxvuMeterNETGDI1.FFTSize = NDXVUMeterNET.DXVUMeterNETGDI.FFTSizeConstants.FFTs2048
        Me.DxvuMeterNETGDI1.FFTSmoothing = 0
        Me.DxvuMeterNETGDI1.FFTStyle = NDXVUMeterNET.DXVUMeterNETGDI.FFTStyleConstants.Bars
        Me.DxvuMeterNETGDI1.FFTWindow = NDXVUMeterNET.DXVUMeterNETGDI.FFTWindowConstants.None
        Me.DxvuMeterNETGDI1.FFTXMax = 18000
        Me.DxvuMeterNETGDI1.FFTXMin = 0
        Me.DxvuMeterNETGDI1.FFTXScale = NDXVUMeterNET.DXVUMeterNETGDI.FFTXScaleConstants.Logarithmic
        Me.DxvuMeterNETGDI1.FFTXZoom = True
        Me.DxvuMeterNETGDI1.FFTXZoomWindowPos = 0
        Me.DxvuMeterNETGDI1.FFTYScale = NDXVUMeterNET.DXVUMeterNETGDI.FFTYScaleConstants.dB
        Me.DxvuMeterNETGDI1.FFTYScaleMultiplier = 1.0R
        Me.DxvuMeterNETGDI1.Frequency = 44100
        Me.DxvuMeterNETGDI1.GreenOff = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.DxvuMeterNETGDI1.GreenOn = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.DxvuMeterNETGDI1.InternalGain = 1.0R
        Me.DxvuMeterNETGDI1.LeftChannelMute = False
        Me.DxvuMeterNETGDI1.LinesThickness = 1
        Me.DxvuMeterNETGDI1.Location = New System.Drawing.Point(12, 157)
        Me.DxvuMeterNETGDI1.Name = "DxvuMeterNETGDI1"
        Me.DxvuMeterNETGDI1.NoiseFilter = 0
        Me.DxvuMeterNETGDI1.NumVUs = CType(32, Short)
        Me.DxvuMeterNETGDI1.Orientation = NDXVUMeterNET.DXVUMeterNETGDI.OrientationConstants.Horizontal
        Me.DxvuMeterNETGDI1.Overlap = 0.0R
        Me.DxvuMeterNETGDI1.PlaybackPosition = CType(0, Long)
        Me.DxvuMeterNETGDI1.PlaybackTime = ""
        Me.DxvuMeterNETGDI1.PlaybackVolume = CType(0, Short)
        Me.DxvuMeterNETGDI1.RedOff = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.DxvuMeterNETGDI1.RedOn = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.DxvuMeterNETGDI1.RightChannelMute = False
        Me.DxvuMeterNETGDI1.Size = New System.Drawing.Size(328, 70)
        Me.DxvuMeterNETGDI1.Style = NDXVUMeterNET.DXVUMeterNETGDI.StyleConstants.FFT
        Me.DxvuMeterNETGDI1.TabIndex = 16
        Me.DxvuMeterNETGDI1.Text = "DxvuMeterNETGDI1"
        Me.DxvuMeterNETGDI1.YellowOff = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.DxvuMeterNETGDI1.YellowOn = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'VuR
        '
        Me.VuR.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.VuR.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.VuR.ForeColor = System.Drawing.Color.LightSkyBlue
        Me.VuR.Location = New System.Drawing.Point(114, 141)
        Me.VuR.Max = 100
        Me.VuR.Min = 0
        Me.VuR.Name = "VuR"
        Me.VuR.Size = New System.Drawing.Size(226, 10)
        Me.VuR.TabIndex = 15
        Me.VuR.Value = 0
        '
        'VuL
        '
        Me.VuL.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.VuL.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.VuL.ForeColor = System.Drawing.Color.LightSkyBlue
        Me.VuL.Location = New System.Drawing.Point(114, 125)
        Me.VuL.Max = 100
        Me.VuL.Min = 0
        Me.VuL.Name = "VuL"
        Me.VuL.Size = New System.Drawing.Size(226, 10)
        Me.VuL.TabIndex = 14
        Me.VuL.Value = 0
        '
        'frmMain
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 15)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(352, 348)
        Me.Controls.Add(Me.DxvuMeterNETGDI1)
        Me.Controls.Add(Me.VuR)
        Me.Controls.Add(Me.VuL)
        Me.Controls.Add(Me.lblDuration)
        Me.Controls.Add(Me.lblBitRate)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.tbRate)
        Me.Controls.Add(Me.tbVol)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.btnPause)
        Me.Controls.Add(Me.btnPlay)
        Me.Controls.Add(Me.tbPos)
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtFileName)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(253, 415)
        Me.MaximizeBox = False
        Me.Name = "frmMain"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MP3Player.NET"
        CType(Me.tbPos, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbVol, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.tbRate, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

#Region "Upgrade Support "
    Private Shared m_vb6FormDefInstance As frmMain
    Private Shared m_InitializingDefInstance As Boolean
    Public Shared Property DefInstance() As frmMain
        Get
            If m_vb6FormDefInstance Is Nothing OrElse m_vb6FormDefInstance.IsDisposed Then
                m_InitializingDefInstance = True
                m_vb6FormDefInstance = New frmMain()
                m_InitializingDefInstance = False
            End If
            DefInstance = m_vb6FormDefInstance
        End Get
        Set(ByVal value As frmMain)
            m_vb6FormDefInstance = value
        End Set
    End Property
#End Region

    Private Structure Peak
        Public Left As Double
        Public Right As Double
    End Structure

    Private Const nAvg As Integer = 4
    Private vuAL(nAvg) As Integer
    Private vuAR(nAvg) As Integer

    Private mp3 As vbMP3decoder.CMP3Decoder

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        Dim ofd As Windows.Forms.OpenFileDialog = New Windows.Forms.OpenFileDialog
        With ofd
            .CheckFileExists = True
            .DefaultExt = "mp3"
            .Filter = "MP3 Files (*.mp3)|*.mp3"
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                btnPlay.Enabled = True
                btnPause.Enabled = True
                btnStop.Enabled = True
                tbVol.Enabled = True
                tbRate.Enabled = True
                tbPos.Enabled = True

                If mp3 Is Nothing Then
                    mp3 = New vbMP3decoder.CMP3Decoder(Me)
                    AddHandler mp3.PositionChanged, Sub() If mp3.Position >= 0 Then tbPos.Value = mp3.Position / mp3.FileLength * 100
                    AddHandler mp3.DecodingProgress, Sub(progress As Single) Me.Text = progress.ToString("0") + "%"
                    AddHandler mp3.NewBuffer, Sub()
                                                  Dim p = CalculateRMS(mp3.NormalizedBuffer)
                                                  VuL.Value = p.Left * mp3.Volume
                                                  VuR.Value = p.Right * mp3.Volume
                                              End Sub
                Else
                    mp3.Stop()
                End If
                mp3.FileName = .FileName

                txtFileName.Text = IO.Path.GetFileNameWithoutExtension(.FileName)
                lblBitRate.Text = String.Format("BitRate: {0} Kbps", CInt(mp3.BitRate / 1000).ToString())

                Dim duration As TimeSpan = mp3.PosToFormattedTime(mp3.FileLength)
                lblDuration.Text = String.Format("Duration: {0:00}:{1:00}", duration.Minutes, duration.Seconds)

                mp3.Play()
            End If
        End With
    End Sub

    Private Function CalculateRMS(buf() As Integer) As Peak
        Dim p As Peak

        Dim nStep As Integer = mp3.Channels * 128

        If buf IsNot Nothing Then
            With p
                ' Calculate Average
                For i = 0 To buf.Length - mp3.Channels Step nStep
                    .Left = .Left + (buf(i) / 30) ^ 2
                    If mp3.Channels = 2 Then
                        .Right = .Right + (buf(i + 1) / 30) ^ 2
                    Else
                        .Right = .Left
                    End If
                Next

                .Left = .Left / (buf.Length / nStep)
                .Right = .Right / (buf.Length / nStep)

                ' Apply compensation
                vuAL(0) = CInt(.Left)
                vuAR(0) = CInt(.Right)
                For i As Integer = nAvg To 1 Step -1
                    vuAL(i) = vuAL(i - 1)
                    vuAR(i) = vuAR(i - 1)
                    .Left = .Left + vuAL(i)
                    .Right = .Right + vuAR(i)
                Next i

                ' Calculate Power
                .Left = 0.8 * (Math.Sqrt(.Left) / 128) / nAvg : If .Left > 1 Then .Left = 1
                .Right = 0.8 * (Math.Sqrt(.Right) / 128) / nAvg : If .Right > 1 Then .Right = 1
            End With
        End If
        Return p
    End Function

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If mp3 IsNot Nothing Then
            mp3.Stop()
            mp3.Dispose()
        End If
    End Sub

    Private Sub tbPos_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbPos.Scroll
        mp3.Position = tbPos.Value / 100 * mp3.FileLength
    End Sub

    Private Sub cmdPLay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPlay.Click
        Select Case mp3.State
            Case vbMP3decoder.CMP3Decoder.DecoderStateConstants.Paused
                mp3.Pause()
            Case vbMP3decoder.CMP3Decoder.DecoderStateConstants.Playing
            Case vbMP3decoder.CMP3Decoder.DecoderStateConstants.Idle
                mp3.Play()
        End Select
    End Sub

    Private Sub btnPause_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPause.Click
        mp3.Pause()
    End Sub

    Private Sub btnStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStop.Click
        mp3.Stop()
    End Sub

    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        btnPlay.Enabled = False
        btnPause.Enabled = False
        btnStop.Enabled = False
        tbRate.Enabled = False
        tbVol.Enabled = False
        tbPos.Enabled = False
    End Sub

    Private Sub tbVol_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbVol.Scroll
        SetRateAndVolume()
    End Sub

    Private Sub tbRate_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles tbRate.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Right Then
            tbRate.Value = 100
            SetRateAndVolume()
        End If
    End Sub

    Private Sub tbRate_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbRate.Scroll
        SetRateAndVolume()
    End Sub

    Private Sub SetRateAndVolume()
        mp3.Volume = tbVol.Value
        mp3.Rate = Math.Max(tbRate.Value, 1)
    End Sub

    Private Sub DxvuMeterNETGDI1_ControlIsReady() Handles DxvuMeterNETGDI1.ControlIsReady
        DxvuMeterNETGDI1.LicenseControl("BE5BE0B4B2AB76CBF3B36ED0041AC0ED", "884433")

        DxvuMeterNETGDI1.Frequency = 44100
        DxvuMeterNETGDI1.Channels = 1
        DxvuMeterNETGDI1.BitDepth = 16

        DxvuMeterNETGDI1.GreenOn = VuL.ForeColor
        DxvuMeterNETGDI1.BackColor = VuL.BackColor

        DxvuMeterNETGDI1.FFTStyle = NDXVUMeterNET.DXVUMeterNETGDI.FFTStyleConstants.Filled
        DxvuMeterNETGDI1.FFTSize = NDXVUMeterNET.DXVUMeterNETGDI.FFTSizeConstants.FFTs1024
        DxvuMeterNETGDI1.FFTYScale = NDXVUMeterNET.DXVUMeterNETGDI.FFTYScaleConstants.PSDTimeInt
        DxvuMeterNETGDI1.FFTSmoothing = 4

        DxvuMeterNETGDI1.StartMonitoring()
    End Sub
End Class