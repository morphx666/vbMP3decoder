Imports System.Collections.Generic
Imports vbMP3decoder
Imports vbMP3decoder.FFT

Public Class FFTSupport
    Public Property FFTSize As FFTSizeConstants = FFTSizeConstants.FFTs2048
    Public Property FFTHistSize As Integer = 4

    Private fftSize2 As Integer
    Private fftL() As ComplexDouble
    Private fftR() As ComplexDouble
    Private fftWindowSum As Double
    Private fftWavDstIndex As Integer
    Private ffWavSrcBufIndex As Integer
    Private fftWindowValues() As Double

    Private fftLHist(FFTHistSize - 1)() As Double
    Private fftRHist(FFTHistSize - 1)() As Double
    Private fftWavDstBufL(FFTSize - 1) As Double
    Private fftWavDstBufR(FFTSize - 1) As Double

    Public Sub New()
        Setup()
    End Sub

    Public Sub Setup()
        fftSize2 = FFTSize / 2

        ReDim fftLHist(FFTHistSize - 1)
        ReDim fftRHist(FFTHistSize - 1)
        ReDim fftWavDstBufL(FFTSize - 1)
        ReDim fftWavDstBufR(FFTSize - 1)

        For i As Integer = 0 To FFTHistSize - 1
            ReDim fftLHist(i)(fftSize2 - 1)
            ReDim fftRHist(i)(fftSize2 - 1)
        Next

        ReDim fftL(FFTSize - 1)
        ReDim fftR(FFTSize - 1)
        For i As Integer = 0 To FFTSize - 1
            fftL(i) = New ComplexDouble()
            fftR(i) = New ComplexDouble()
        Next
        fftWindowValues = FFT.GetWindowValues(FFTSize, FFTWindowConstants.Hanning)
        fftWindowSum = FFT.GetWindowSum(FFTSize, FFTWindowConstants.Hanning)
        fftWavDstIndex = 0
        ffWavSrcBufIndex = 0
    End Sub

    Public Sub RenderFFT(g As Graphics, r As Rectangle, colorLeft As Pen, colorRight As Pen, Optional drawVerticalGrid As Boolean = True)
        RunFFT()

        Dim s As Size
        Dim penOffset As Integer = colorLeft.Width / 2
        Dim lastPL As Point = New Point(r.X + penOffset, r.Bottom)
        Dim lastPR As Point = lastPL
        Dim newDivX As Integer

        If drawVerticalGrid Then
            Using p As New Pen(Color.FromArgb(128, Color.DarkSlateGray))
                For x As Integer = 0 To fftSize2 - 1 Step fftSize2 / 5
                    For x1 As Integer = 0 To 10
                        newDivX = r.X + x + Math.Min(Math.Log10(x1 + 1) / Math.Log10(fftSize2 - 1) * r.Width, r.Width) + s.Width
                        g.DrawLine(p, newDivX, r.Y, newDivX, r.Bottom)
                    Next
                Next
            End Using
        End If

        Dim lastW As Integer = FFT2Pts(fftSize2 - 1, r, 1).Width + colorLeft.Width
        For x As Integer = 0 To fftSize2 - 1
            s = FFT2Pts(x, r, 1)
            newDivX = r.X + x / fftSize2 * (r.Width - lastW) + s.Width + penOffset
            g.DrawLine(colorLeft, lastPL.X, lastPL.Y, newDivX, r.Bottom - s.Height - colorLeft.Width)
            lastPL = New Point(newDivX, r.Bottom - s.Height)

            s = FFT2Pts(x, r, 2)
            g.DrawLine(colorRight, lastPR.X, lastPR.Y, newDivX, r.Bottom - s.Height - colorLeft.Width)
            lastPR = New Point(newDivX, r.Bottom - s.Height)
        Next

        g.DrawRectangle(Pens.DimGray, r)
        Using sb As New SolidBrush(Color.FromArgb(128, 33, 33, 33))
            g.FillRectangle(sb, r)
        End Using
    End Sub

    Public Sub RenderFilledFFT(g As Graphics, r As Rectangle, colorLeft As Pen, colorRight As Pen, Optional drawVerticalGrid As Boolean = True)
        RunFFT()

        Dim s As Size
        Dim penOffset As Integer = colorLeft.Width / 2
        Dim lastPL As Point = New Point(r.X + penOffset, r.Bottom)
        Dim lastPR As Point = lastPL
        Dim newDivX As Integer

        If drawVerticalGrid Then
            Using p As New Pen(Color.FromArgb(128, Color.DarkSlateGray))
                For x As Integer = 0 To fftSize2 - 1 Step fftSize2 / 5
                    For x1 As Integer = 0 To 10
                        newDivX = r.X + x + Math.Min(Math.Log10(x1 + 1) / Math.Log10(fftSize2 - 1) * r.Width, r.Width) + s.Width
                        g.DrawLine(p, newDivX, r.Y, newDivX, r.Bottom)
                    Next
                Next
            End Using
        End If

        Dim ptsL As New List(Of Point)
        Dim ptsR As New List(Of Point)
        Dim lastW As Integer = FFT2Pts(fftSize2 - 1, r, 1).Width + colorLeft.Width
        For x As Integer = 0 To fftSize2 - 1
            s = FFT2Pts(x, r, 1)
            newDivX = r.X + x / fftSize2 * (r.Width - lastW) + s.Width + penOffset
            ptsL.Add(New Point(lastPL.X, lastPL.Y))
            lastPL = New Point(newDivX, r.Bottom - s.Height)

            s = FFT2Pts(x, r, 2)
            ptsR.Add(New Point(lastPR.X, lastPR.Y))
            lastPR = New Point(newDivX, r.Bottom - s.Height)
        Next
        ptsL.Add(New Point(lastPL.X, r.Bottom))
        g.FillPolygon(New SolidBrush(colorLeft.Color), ptsL.ToArray())
        ptsR.Add(New Point(lastPR.X, r.Bottom))
        g.FillPolygon(New SolidBrush(colorRight.Color), ptsR.ToArray())

        g.DrawRectangle(Pens.DimGray, r)
        Using sb As New SolidBrush(Color.FromArgb(128, 33, 33, 33))
            g.FillRectangle(sb, r)
        End Using
    End Sub

    Public Sub RunFFT()
        If fftWavDstIndex = 0 Then FourierTransform(FFTSize, fftWavDstBufL, fftL, fftWavDstBufR, fftR, False)

        For i As Integer = 0 To FFTHistSize - 2
            For j As Integer = 0 To fftSize2 - 1
                fftLHist(i)(j) = fftLHist(i + 1)(j)
                fftRHist(i)(j) = fftRHist(i + 1)(j)
            Next
        Next

        For i As Integer = 0 To fftSize2 - 1
            fftLHist(FFTHistSize - 1)(i) = fftL(i).Power()
            fftRHist(FFTHistSize - 1)(i) = fftR(i).Power()
        Next
    End Sub

    Public Sub FillFFTBuffer(bufL() As Byte, bufR() As Byte, vol As Integer)
        Do
            Do
                If ffWavSrcBufIndex >= bufL.Length Then
                    If fftWavDstIndex >= FFTSize Then fftWavDstIndex = 0
                    ffWavSrcBufIndex = 0
                    Exit Do
                ElseIf fftWavDstIndex >= FFTSize Then
                    fftWavDstIndex = 0
                    Exit Do
                End If

                fftWavDstBufL(fftWavDstIndex) = bufL(ffWavSrcBufIndex) * fftWindowValues(fftWavDstIndex) * vol / 100
                fftWavDstBufR(fftWavDstIndex) = bufR(ffWavSrcBufIndex) * fftWindowValues(fftWavDstIndex) * vol / 100

                fftWavDstIndex += 1
                ffWavSrcBufIndex += 1
            Loop
        Loop Until fftWavDstIndex = 0 OrElse ffWavSrcBufIndex = 0
    End Sub

    Public Sub FillFFTBuffer(bufL() As Integer, bufR() As Integer, vol As Integer)
        Do
            Do
                If ffWavSrcBufIndex >= bufL.Length Then
                    If fftWavDstIndex >= FFTSize Then fftWavDstIndex = 0
                    ffWavSrcBufIndex = 0
                    Exit Do
                ElseIf fftWavDstIndex >= FFTSize Then
                    fftWavDstIndex = 0
                    Exit Do
                End If

                fftWavDstBufL(fftWavDstIndex) = bufL(ffWavSrcBufIndex) * fftWindowValues(fftWavDstIndex) * vol / 100
                fftWavDstBufR(fftWavDstIndex) = bufR(ffWavSrcBufIndex) * fftWindowValues(fftWavDstIndex) * vol / 100

                fftWavDstIndex += 1
                ffWavSrcBufIndex += 1
            Loop
        Loop Until fftWavDstIndex = 0 OrElse ffWavSrcBufIndex = 0
    End Sub

    Private Function FFT2Pts(x As Integer, r As Rectangle, channel As Integer) As Size
        Dim v As Double

        v = (FFTAvg(x, channel) / fftWindowSum * 2) / 20

        ' Amplitude
        ' v = Math.Min((0.3 * Math.Sqrt(v) / FFTSize) * r.Height, r.Height)
        ' dB
        v = Math.Min(Math.Log10(v + 1) / 10 * r.Height, r.Height)
        x = Math.Min(Math.Log10(x + 1) / Math.Log10(fftSize2 - 1) * r.Width, r.Width)

        Return New Size(x, v)
    End Function

    Private Function FFTAvg(x As Integer, channel As Integer) As Double
        Dim v As Double
        For i As Integer = 0 To FFTHistSize - 1
            If channel = 1 Then
                v += fftLHist(i)(x)
            Else
                v += fftRHist(i)(x)
            End If
        Next
        Return v / FFTHistSize
    End Function
End Class