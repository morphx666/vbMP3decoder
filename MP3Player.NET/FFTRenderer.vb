﻿Imports System.Threading
Imports System.Threading.Tasks

Public Class FFTRenderer
    Private fftBuffer() As vbMP3decoder.FFT.ComplexDouble = {New vbMP3decoder.FFT.ComplexDouble(0, 0)}
    Private foreBrushColor As SolidBrush
    Private forePenColor As Pen
    Private fft As New FFTSupport()

    Private Sub FFTRenderer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        fft.Setup()

        AddHandler Me.ForeColorChanged, Sub()
                                            If foreBrushColor IsNot Nothing Then foreBrushColor.Dispose()
                                            If forePenColor IsNot Nothing Then forePenColor.Dispose()
                                            Try
                                                foreBrushColor = New SolidBrush(Me.ForeColor)
                                                forePenColor = New Pen(Me.ForeColor)
                                            Catch ex As Exception
                                                MsgBox(ex.Message)
                                            End Try
                                        End Sub

        foreBrushColor = New SolidBrush(Me.ForeColor)
        forePenColor = New Pen(Me.ForeColor)

        Task.Run(Sub()
                     Do
                         Thread.Sleep(33)
                         Me.Invalidate()
                     Loop
                 End Sub)
    End Sub

    Public Sub FillAudioBuffer(bufL() As Byte, bufR() As Byte)
        fft.FillFFTBuffer(bufL, bufR)
    End Sub

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        MyBase.OnPaintBackground(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.DisplayRectangle

        g.Clear(Me.BackColor)

        fft.RenderFFT(g, Me.DisplayRectangle, forePenColor, Pens.Transparent)
    End Sub
End Class