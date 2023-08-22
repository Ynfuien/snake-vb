Option Strict On
Option Explicit On

Public Class MainForm
    'Configuration variables
    Private Const _gridSize = 32
    Private Const _snakeSize = 5
    Private Const _scale = 20
    Private Const _size = _gridSize * _scale + _gridSize - 1
    Private Const _tickTime = 100

    'Game variables
    Private rng As Random = New Random()
    Private gBorder As Border
    Private gSnake As Snake
    Private gBerry As Berry
    Private direction As Snake.Direction
    Private newDirection As Snake.Direction
    Private gameOver As Boolean = False

    'Colors
    Private _colorBackground As Color = Color.FromArgb(36, 36, 36)
    Private _colorSnakeHead As Color = Color.FromArgb(12, 69, 114)
    Private _colorSnakeBody As Color = Color.FromArgb(17, 89, 163)
    Private _colorBerry As Color = Color.FromArgb(255, 85, 85)
    Private _colorBorder As Color = Color.FromArgb(85, 85, 85)
    Private _colorGameover As Color = Color.FromArgb(255, 85, 85)
    Private _colorScore As Color = Color.FromArgb(255, 255, 85)
    Private _colorScoreNumber As Color = Color.FromArgb(255, 170, 0)

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Setup()
    End Sub

    Private Sub Setup()
        Canvas.Width = _size
        Canvas.Height = _size

        CenterToScreen()

        gBorder = New Border(_gridSize)
        gSnake = New Snake(_snakeSize)
        gBerry = New Berry(gSnake, rng)
        direction = Snake.Direction.Right
        newDirection = direction

        Canvas.BackColor = _colorBackground

        GameTimer.Interval = _tickTime
        GameTimer.Enabled = True
        GameTimer.Start()
    End Sub


    Private Sub Canvas_Paint(sender As Object, e As PaintEventArgs) Handles Canvas.Paint
        Dim g = e.Graphics

        'Game over screen
        If gameOver Then
            Dim scale As Integer = CInt(_scale * 1.2)
            Dim score As Integer = gSnake.GetSize() - _snakeSize

            Dim font As Font = New Font("Arial", scale)
            RenderText(g, "Game over!", font, _colorGameover, CInt(_size / 2), CInt(_size / 2 - (scale * 2.3)))
            RenderText(g, $"Score: {score}", font, _colorScoreNumber, CInt(_size / 2), CInt(_size / 2 - scale))
            RenderText(g, $"Score: {Strings.StrDup(score.ToString().Length * 2, " ")}", font, _colorScore, CInt(_size / 2), CInt(_size / 2 - scale))

            gBorder.Render(g, _colorBorder)
            Return
        End If

        'Render everything
        gSnake.Render(g, _colorSnakeHead, _colorSnakeBody)
        gBerry.Render(g, _colorBerry)
        gBorder.Render(g, _colorBorder)
    End Sub

    'Renders centered text
    Private Sub RenderText(g As Graphics, text As String, font As Font, color As Color, x As Integer, y As Integer)
        Dim textSize As Size = TextRenderer.MeasureText(text, font)
        g.DrawString(text, font, New SolidBrush(color), New Point(CInt(x - (textSize.Width / 2)), CInt(y - (textSize.Height / 2))))
    End Sub

    Private Sub GameTimer_Tick(sender As Object, e As EventArgs) Handles GameTimer.Tick
        direction = newDirection

        'Move snake And check if it actually moved
        If Not gSnake.Move(direction, gBorder) Then
            'Game over
            gameOver = True
            Canvas.Refresh()
            GameTimer.Stop()
            Return
        End If

        'Check if snake got the berry
        If gSnake.ContainsBerry(gBerry) Then
            gBerry = New Berry(gSnake, rng)
            gSnake.Grow()
        End If

        'Render everything to user
        Canvas.Refresh()
    End Sub

    Private Sub MainForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If (gameOver) Then Return

        Dim key = e.KeyCode
        Select Case key

            Case Keys.Up, Keys.W
                If (direction = Snake.Direction.Down) Then Return
                newDirection = Snake.Direction.Up
            Case Keys.Down, Keys.S
                If (direction = Snake.Direction.Up) Then Return
                newDirection = Snake.Direction.Down
            Case Keys.Left, Keys.A
                If (direction = Snake.Direction.Right) Then Return
                newDirection = Snake.Direction.Left
            Case Keys.Right, Keys.D
                If (direction = Snake.Direction.Left) Then Return
                newDirection = Snake.Direction.Right
        End Select

    End Sub

    Class Pixel
        Public Property x As Integer
        Public Property y As Integer

        Public Sub New(xp As Integer, yp As Integer)
            x = If(xp > _gridSize, _gridSize, xp)
            y = If(yp > _gridSize, _gridSize, yp)
        End Sub

        Public Sub Render(g As Graphics, color As Color)
            Dim Brush = New SolidBrush(color)
            g.FillRectangle(Brush, x * _scale + x, y * _scale + y, _scale, _scale)
        End Sub

        Public Function Equals(Pixel As Pixel) As Boolean
            Return Pixel.x = x And Pixel.y = y
        End Function
    End Class


    Class Border
        Private Property borderPixels As List(Of Pixel) = New List(Of Pixel)

        Public Sub New(size As Integer)
            For i = 0 To size
                ' Border in width
                borderPixels.Add(New Pixel(i, 0))
                borderPixels.Add(New Pixel(i, size - 1))


                ' Border in height
                If i = 0 Or i = size - 1 Then Continue For
                borderPixels.Add(New Pixel(0, i))
                borderPixels.Add(New Pixel(size - 1, i))
            Next
        End Sub

        Public Sub Render(g As Graphics, color As Color)
            For Each p In borderPixels
                p.Render(g, color)
            Next
        End Sub


        Public Function Contains(pixel As Pixel) As Boolean

            For Each p In borderPixels
                If p.Equals(pixel) Then Return True
            Next

            Return False
        End Function

    End Class

    Class Snake

        Private Property bodyPixels As List(Of Pixel) = New List(Of Pixel)
        Private Property headPixel As Pixel

        Public Sub New(size As Integer)

            headPixel = New Pixel(CInt(_gridSize / 2 + (size / 2)), CInt(_gridSize / 2 - 1))
            For i = size - 1 To 1 Step -1
                bodyPixels.Add(New Pixel(headPixel.x - i, headPixel.y))
            Next
        End Sub


        Public Sub Render(g As Graphics, headColor As Color, bodyColor As Color)
            headPixel.Render(g, headColor)
            For Each p In bodyPixels
                p.Render(g, bodyColor)
            Next
        End Sub


        Public Function Move(dir As Direction, border As Border) As Boolean
            Dim x = headPixel.x
            Dim y = headPixel.y

            If dir = Direction.Up Then
                y -= 1
            ElseIf dir = Direction.Right Then
                x += 1
            ElseIf dir = Direction.Down Then
                y += 1
            ElseIf dir = Direction.Left Then
                x -= 1
            End If

            Dim newHead = New Pixel(x, y)
            If (Contains(newHead)) Then Return False
            If (border.Contains(newHead)) Then Return False

            bodyPixels.Add(headPixel)
            bodyPixels.RemoveAt(0)
            headPixel = newHead
            Return True
        End Function

        Public Sub Grow()
            Dim newBody = New Pixel(bodyPixels(0).x, bodyPixels(0).y)
            bodyPixels.Insert(0, newBody)
        End Sub

        Public Function GetSize() As Integer
            Return bodyPixels.Count + 1
        End Function

        Public Function Contains(pixel As Pixel) As Boolean
            If (headPixel.Equals(pixel)) Then Return True
            For Each p In bodyPixels
                If (p.Equals(pixel)) Then Return True
            Next

            Return False
        End Function

        'Separate method for checking for berry,
        'because only head pixel can move onto berry position
        Public Function ContainsBerry(berry As Berry) As Boolean
            Return headPixel.Equals(berry.position)
        End Function

        Public Enum Direction
            Up
            Down
            Left
            Right
        End Enum


    End Class

    Class Berry
        Public Property position As Pixel

        Public Sub New(snake As Snake, rng As Random)
            Do
                position = New Pixel(rng.Next(1, _gridSize - 1), rng.Next(1, _gridSize - 1))
            Loop While snake.Contains(position)
        End Sub

        Public Sub Render(g As Graphics, color As Color)

            position.Render(g, color)
        End Sub
    End Class

End Class
