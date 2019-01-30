Public Class FormMain
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
    Friend WithEvents TextBoxFileName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents ButtonBrowse As System.Windows.Forms.Button
    Friend WithEvents TrackBarPos As System.Windows.Forms.TrackBar
    Friend WithEvents ButtonPlay As System.Windows.Forms.Button
    Friend WithEvents ButtonPause As System.Windows.Forms.Button
    Friend WithEvents ButtonStop As System.Windows.Forms.Button
    Friend WithEvents TrackBarVol As System.Windows.Forms.TrackBar
    Friend WithEvents TrackBarRate As System.Windows.Forms.TrackBar
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents LabelBitRate As System.Windows.Forms.Label
    Friend WithEvents LabelDuration As System.Windows.Forms.Label
    Friend WithEvents VuL As MP3Player.VU
    Friend WithEvents VuR As MP3Player.VU
    Friend WithEvents FftRendererMain As FFTRenderer
    Friend WithEvents ButtonClose As System.Windows.Forms.Button
    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim ComplexDouble1 As vbMP3decoder.FFT.ComplexDouble = New vbMP3decoder.FFT.ComplexDouble()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormMain))
        Me.TextBoxFileName = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.ButtonBrowse = New System.Windows.Forms.Button()
        Me.ButtonClose = New System.Windows.Forms.Button()
        Me.TrackBarPos = New System.Windows.Forms.TrackBar()
        Me.TrackBarVol = New System.Windows.Forms.TrackBar()
        Me.TrackBarRate = New System.Windows.Forms.TrackBar()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.LabelBitRate = New System.Windows.Forms.Label()
        Me.LabelDuration = New System.Windows.Forms.Label()
        Me.ButtonStop = New System.Windows.Forms.Button()
        Me.ButtonPause = New System.Windows.Forms.Button()
        Me.ButtonPlay = New System.Windows.Forms.Button()
        Me.VuR = New MP3Player.VU()
        Me.VuL = New MP3Player.VU()
        Me.FftRendererMain = New MP3Player.FFTRenderer()
        CType(Me.TrackBarPos, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TrackBarVol, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.TrackBarRate, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'TextBoxFileName
        '
        Me.TextBoxFileName.Location = New System.Drawing.Point(12, 25)
        Me.TextBoxFileName.Name = "TextBoxFileName"
        Me.TextBoxFileName.ReadOnly = True
        Me.TextBoxFileName.Size = New System.Drawing.Size(292, 22)
        Me.TextBoxFileName.TabIndex = 0
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
        'ButtonBrowse
        '
        Me.ButtonBrowse.Location = New System.Drawing.Point(310, 25)
        Me.ButtonBrowse.Name = "ButtonBrowse"
        Me.ButtonBrowse.Size = New System.Drawing.Size(30, 21)
        Me.ButtonBrowse.TabIndex = 2
        Me.ButtonBrowse.Text = "..."
        Me.ButtonBrowse.UseVisualStyleBackColor = True
        '
        'ButtonClose
        '
        Me.ButtonClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ButtonClose.Location = New System.Drawing.Point(261, 307)
        Me.ButtonClose.Name = "ButtonClose"
        Me.ButtonClose.Size = New System.Drawing.Size(79, 29)
        Me.ButtonClose.TabIndex = 3
        Me.ButtonClose.Text = "Close"
        Me.ButtonClose.UseVisualStyleBackColor = True
        '
        'TrackBarPos
        '
        Me.TrackBarPos.Location = New System.Drawing.Point(12, 87)
        Me.TrackBarPos.Maximum = 100
        Me.TrackBarPos.Name = "TrackBarPos"
        Me.TrackBarPos.Size = New System.Drawing.Size(328, 45)
        Me.TrackBarPos.TabIndex = 4
        Me.TrackBarPos.TickFrequency = 5
        '
        'TrackBarVol
        '
        Me.TrackBarVol.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TrackBarVol.AutoSize = False
        Me.TrackBarVol.Location = New System.Drawing.Point(80, 233)
        Me.TrackBarVol.Maximum = 100
        Me.TrackBarVol.Name = "TrackBarVol"
        Me.TrackBarVol.Size = New System.Drawing.Size(260, 31)
        Me.TrackBarVol.TabIndex = 8
        Me.TrackBarVol.TickFrequency = 5
        Me.TrackBarVol.Value = 100
        '
        'TrackBarRate
        '
        Me.TrackBarRate.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TrackBarRate.AutoSize = False
        Me.TrackBarRate.Location = New System.Drawing.Point(80, 270)
        Me.TrackBarRate.Maximum = 200
        Me.TrackBarRate.Name = "TrackBarRate"
        Me.TrackBarRate.Size = New System.Drawing.Size(260, 31)
        Me.TrackBarRate.TabIndex = 9
        Me.TrackBarRate.TickFrequency = 10
        Me.TrackBarRate.Value = 100
        '
        'Label2
        '
        Me.Label2.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(17, 242)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(45, 13)
        Me.Label2.TabIndex = 10
        Me.Label2.Text = "Volume"
        '
        'Label3
        '
        Me.Label3.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(17, 279)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(30, 13)
        Me.Label3.TabIndex = 11
        Me.Label3.Text = "Rate"
        '
        'LabelBitRate
        '
        Me.LabelBitRate.AutoSize = True
        Me.LabelBitRate.Location = New System.Drawing.Point(239, 49)
        Me.LabelBitRate.Name = "LabelBitRate"
        Me.LabelBitRate.Size = New System.Drawing.Size(67, 13)
        Me.LabelBitRate.TabIndex = 12
        Me.LabelBitRate.Text = "BitRate: n/a"
        Me.LabelBitRate.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'LabelDuration
        '
        Me.LabelDuration.AutoSize = True
        Me.LabelDuration.Location = New System.Drawing.Point(12, 49)
        Me.LabelDuration.Name = "LabelDuration"
        Me.LabelDuration.Size = New System.Drawing.Size(76, 13)
        Me.LabelDuration.TabIndex = 13
        Me.LabelDuration.Text = "Duration: n/a"
        '
        'ButtonStop
        '
        Me.ButtonStop.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.ButtonStop.Image = Global.MP3Player.My.Resources.Resources.btnStop
        Me.ButtonStop.Location = New System.Drawing.Point(80, 125)
        Me.ButtonStop.Name = "ButtonStop"
        Me.ButtonStop.Size = New System.Drawing.Size(28, 26)
        Me.ButtonStop.TabIndex = 7
        Me.ButtonStop.UseVisualStyleBackColor = True
        '
        'ButtonPause
        '
        Me.ButtonPause.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.ButtonPause.Image = Global.MP3Player.My.Resources.Resources.btnPause
        Me.ButtonPause.Location = New System.Drawing.Point(46, 125)
        Me.ButtonPause.Name = "ButtonPause"
        Me.ButtonPause.Size = New System.Drawing.Size(28, 26)
        Me.ButtonPause.TabIndex = 6
        Me.ButtonPause.UseVisualStyleBackColor = True
        '
        'ButtonPlay
        '
        Me.ButtonPlay.FlatStyle = System.Windows.Forms.FlatStyle.Popup
        Me.ButtonPlay.Image = Global.MP3Player.My.Resources.Resources.btnPlay
        Me.ButtonPlay.Location = New System.Drawing.Point(12, 125)
        Me.ButtonPlay.Name = "ButtonPlay"
        Me.ButtonPlay.Size = New System.Drawing.Size(28, 26)
        Me.ButtonPlay.TabIndex = 5
        Me.ButtonPlay.UseVisualStyleBackColor = True
        '
        'VuR
        '
        Me.VuR.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.VuR.BackColor = System.Drawing.Color.FromArgb(CType(CType(66, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(66, Byte), Integer))
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
        Me.VuL.BackColor = System.Drawing.Color.FromArgb(CType(CType(66, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.VuL.ForeColor = System.Drawing.Color.LightSkyBlue
        Me.VuL.Location = New System.Drawing.Point(114, 125)
        Me.VuL.Max = 100
        Me.VuL.Min = 0
        Me.VuL.Name = "VuL"
        Me.VuL.Size = New System.Drawing.Size(226, 10)
        Me.VuL.TabIndex = 14
        Me.VuL.Value = 0
        '
        'FftRendererMain
        '
        Me.FftRendererMain.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.FftRendererMain.BackColor = System.Drawing.Color.FromArgb(CType(CType(66, Byte), Integer), CType(CType(66, Byte), Integer), CType(CType(66, Byte), Integer))
        Me.FftRendererMain.ForeColor = System.Drawing.Color.LightSkyBlue
        Me.FftRendererMain.Location = New System.Drawing.Point(12, 157)
        Me.FftRendererMain.Name = "FftRendererMain"
        Me.FftRendererMain.Size = New System.Drawing.Size(328, 70)
        Me.FftRendererMain.TabIndex = 16
        '
        'FormMain
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 15)
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(352, 348)
        Me.Controls.Add(Me.FftRendererMain)
        Me.Controls.Add(Me.VuR)
        Me.Controls.Add(Me.VuL)
        Me.Controls.Add(Me.LabelDuration)
        Me.Controls.Add(Me.LabelBitRate)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.TrackBarRate)
        Me.Controls.Add(Me.TrackBarVol)
        Me.Controls.Add(Me.ButtonStop)
        Me.Controls.Add(Me.ButtonPause)
        Me.Controls.Add(Me.ButtonPlay)
        Me.Controls.Add(Me.TrackBarPos)
        Me.Controls.Add(Me.ButtonClose)
        Me.Controls.Add(Me.ButtonBrowse)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.TextBoxFileName)
        Me.Cursor = System.Windows.Forms.Cursors.Default
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Location = New System.Drawing.Point(253, 415)
        Me.MaximizeBox = False
        Me.Name = "FormMain"
        Me.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MP3Player.NET"
        CType(Me.TrackBarPos, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TrackBarVol, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.TrackBarRate, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
#End Region

#Region "Upgrade Support "
    Private Shared m_vb6FormDefInstance As FormMain
    Private Shared m_InitializingDefInstance As Boolean
    Public Shared Property DefInstance() As FormMain
        Get
            If m_vb6FormDefInstance Is Nothing OrElse m_vb6FormDefInstance.IsDisposed Then
                m_InitializingDefInstance = True
                m_vb6FormDefInstance = New FormMain()
                m_InitializingDefInstance = False
            End If
            DefInstance = m_vb6FormDefInstance
        End Get
        Set(ByVal value As FormMain)
            m_vb6FormDefInstance = value
        End Set
    End Property
#End Region

    Private Structure Peak
        Public Left As Double
        Public Right As Double
    End Structure

    Private Const nAvg As Integer = 4
    Private ReadOnly vuAL(nAvg) As Integer
    Private ReadOnly vuAR(nAvg) As Integer

    Private mp3 As vbMP3decoder.CMP3Decoder

    Private Sub ButtonClose_Click(sender As Object, e As EventArgs) Handles ButtonClose.Click
        Me.Close()
    End Sub

    Private Sub ButtonBrowse_Click(sender As Object, e As EventArgs) Handles ButtonBrowse.Click
        Dim ofd As New OpenFileDialog()
        With ofd
            .CheckFileExists = True
            .DefaultExt = "mp3"
            .Filter = "MP3 Files (*.mp3)|*.mp3"
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                ButtonPlay.Enabled = True
                ButtonPause.Enabled = True
                ButtonStop.Enabled = True
                TrackBarVol.Enabled = True
                TrackBarRate.Enabled = True
                TrackBarPos.Enabled = True

                If mp3 Is Nothing Then
                    mp3 = New vbMP3decoder.CMP3Decoder(Me)
                    AddHandler mp3.PositionChanged, Sub() If mp3.Position >= 0 Then TrackBarPos.Value = mp3.Position / mp3.FileLength * 100
                    AddHandler mp3.DecodingProgress, Sub(progress As Single) Me.Text = progress.ToString("0") + "%"
                    AddHandler mp3.NewBuffer, Sub()
                                                  Dim p As Peak = CalculateRMS(mp3.NormalizedBuffer)
                                                  VuL.Value = p.Left * mp3.Volume
                                                  VuR.Value = p.Right * mp3.Volume
                                                  FftRendererMain.FillAudioBuffer(mp3.NormalizedBufferLeft,
                                                                                  mp3.NormalizedBufferRight,
                                                                                  mp3.Volume)
                                              End Sub
                Else
                    mp3.Stop()
                End If
                mp3.FileName = .FileName

                TextBoxFileName.Text = IO.Path.GetFileNameWithoutExtension(.FileName)
                LabelBitRate.Text = String.Format("BitRate: {0} Kbps", CInt(mp3.BitRate / 1000).ToString())

                Dim duration As TimeSpan = mp3.PosToFormattedTime(mp3.FileLength)
                LabelDuration.Text = String.Format("Duration: {0:00}:{1:00}", duration.Minutes, duration.Seconds)

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
                    .Right = If(mp3.Channels = 2, .Right + (buf(i + 1) / 30) ^ 2, .Left)
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

    Private Sub FormMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If mp3 IsNot Nothing Then
            mp3.Stop()
            mp3.Dispose()
        End If
    End Sub

    Private Sub TrackBarPos_Scroll(sender As Object, e As EventArgs) Handles TrackBarPos.Scroll
        mp3.Position = TrackBarPos.Value / 100 * mp3.FileLength
    End Sub

    Private Sub ButtonPlay_Click(ByVal sender As Object, e As EventArgs) Handles ButtonPlay.Click
        Select Case mp3.State
            Case vbMP3decoder.CMP3Decoder.DecoderStateConstants.Paused
                mp3.Pause()
            Case vbMP3decoder.CMP3Decoder.DecoderStateConstants.Playing
            Case vbMP3decoder.CMP3Decoder.DecoderStateConstants.Idle
                mp3.Play()
        End Select
    End Sub

    Private Sub ButtonPause_Click(sender As Object, e As EventArgs) Handles ButtonPause.Click
        mp3.Pause()
    End Sub

    Private Sub ButtonStop_Click(sender As Object, e As EventArgs) Handles ButtonStop.Click
        mp3.Stop()
    End Sub

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles Me.Load
        ButtonPlay.Enabled = False
        ButtonPause.Enabled = False
        ButtonStop.Enabled = False
        TrackBarRate.Enabled = False
        TrackBarVol.Enabled = False
        TrackBarPos.Enabled = False
    End Sub

    Private Sub TrackBarVol_Scroll(sender As Object, e As EventArgs) Handles TrackBarVol.Scroll
        SetRateAndVolume()
    End Sub

    Private Sub TrackBarRate_MouseDown(sender As Object, e As MouseEventArgs) Handles TrackBarRate.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Right Then
            TrackBarRate.Value = 100
            SetRateAndVolume()
        End If
    End Sub

    Private Sub TrackBarRate_Scroll(sender As Object, e As EventArgs) Handles TrackBarRate.Scroll
        SetRateAndVolume()
    End Sub

    Private Sub SetRateAndVolume()
        mp3.Volume = TrackBarVol.Value
        mp3.Rate = Math.Max(TrackBarRate.Value, 1)
    End Sub
End Class