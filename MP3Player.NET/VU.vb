﻿Imports System.Threading
Imports System.Threading.Tasks

Public Class VU
    Private renderThread As Thread

    Private mMin As Integer = 0
    Private mMax As Integer = 100
    Private mValue As Integer = 0
    Private foreBrushColor As SolidBrush

    Private Sub VU_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        AddHandler Me.ForeColorChanged, Sub()
                                            If foreBrushColor IsNot Nothing Then foreBrushColor.Dispose()
                                            Try
                                                foreBrushColor = New SolidBrush(Me.ForeColor)
                                            Catch ex As Exception
                                                MsgBox(ex.Message)
                                            End Try
                                        End Sub

        foreBrushColor = New SolidBrush(Me.ForeColor)

        Task.Run(Sub()
                     Do
                         Thread.Sleep(33)
                         Me.Invalidate()
                     Loop
                 End Sub)
    End Sub

    Public Property Min As Integer
        Get
            Return mMin
        End Get
        Set(value As Integer)
            mMin = value
        End Set
    End Property

    Public Property Max As Integer
        Get
            Return mMax
        End Get
        Set(value As Integer)
            mMax = value
        End Set
    End Property

    Public Property Value As Integer
        Get
            Return mValue
        End Get
        Set(value As Integer)
            mValue = value
        End Set
    End Property

    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
        'MyBase.OnPaintBackground(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g As Graphics = e.Graphics
        Dim r As Rectangle = Me.DisplayRectangle

        g.Clear(Me.BackColor)

        Dim w As Single = (mValue - mMin) / (mMax - mMin) * r.Width
        g.FillRectangle(foreBrushColor, r.X, r.Y, w, r.Height)

        For x As Integer = r.X + 1 To r.Width Step 2
            g.DrawLine(Pens.Black, r.X + x, r.Y, r.X + x, r.Height)
        Next
    End Sub
End Class
