Imports System.Windows.Media.Animation
Imports System.Windows.Threading

Class MainWindow
    Public defPixel = 4
    Public Screen = 1
    Public Rnd As New Random()
    Public easEnemyS0
    Public easEnemyS1
    Public medEnemyS0
    Public medEnemyS1
    Public harEnemyS0
    Public harEnemyS1
    Public playChar
    Public isDefaultState As Boolean = True
    Public shotExists As Boolean = False
    Public shotsLeft As Integer = fireRate
    Public centre As Integer
    Public locReached As Integer = -2 '0 for reached left, 1 reached down on left, 2 reached right, 3 reached down on right
    Public reachedBottom As Boolean = False
    Public lazors As New List(Of Rectangle)
    Public elazors As New List(Of Rectangle)
    Public laserSpawntime As Integer = 0
    Public index As Integer = 0
    Public timer As DispatcherTimer
    Public easyEnemies As New List(Of Image)
    Public mediumEnemies As New List(Of Image)
    Public hardEnemies As New List(Of Image)
    Public allEnemies As New List(Of Image)
    Public starAnims As New List(Of DoubleAnimation)
    Public elapsedTime As Integer
    Public score As Integer = 0
    Public hiscore As Integer = 0
    Public life = 3
    Public isImmune As Boolean = False
    Public lastDeathTime As Integer = 0

    'Variable

    Public fireRate As Integer = 6
    Public GAME_SPEED As Double = 0.5
    Public FIRE_SPEED As Integer = 5
    Public STOP_KEY() As Key = {Key.Escape}
    Public LEFT_KEY() As Key = {Key.Left, Key.A}
    Public RIGHT_KEY() As Key = {Key.Right, Key.D}
    Public SHOOT_KEY() As Key = {Key.Up, Key.W, Key.Space}
    'Background Animations

    'Functions

    Public Function opacityAnim(fromVal As Single, toVal As Single, duration As Duration, Optional isLooped As Boolean = False)
        Dim Anim As New DoubleAnimation(fromVal, toVal, duration)
        If isLooped Then
            Anim.RepeatBehavior = RepeatBehavior.Forever
            Anim.AutoReverse = True
        End If
        Return Anim
    End Function

    Public Function getLocation(obj As Object) As Point
        Return New Point(Canvas.GetLeft(obj), Canvas.GetTop(obj))
    End Function


    ''Keypress handling


    Sub OnLoad(sender As Object, e As EventArgs)
        InstalizeBitMaps()
        Canvas.SetLeft(grdCluster, 0)
        Canvas.SetTop(grdCluster, 0)
        For Each enemy As Image In grdCluster.Children
            allEnemies.Add(enemy)
        Next
        timer = New DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal,
                                    Sub()
                                        elapsedTime += 10
                                        If life = 3 Then
                                            imgL1.Opacity = 1
                                            imgL2.Opacity = 1
                                            imgL3.Opacity = 1
                                        ElseIf life = 2 Then
                                            imgL3.Opacity = 0
                                        ElseIf life = 1 Then
                                            imgL2.Opacity = 0
                                        ElseIf life = 0 Then
                                            imgL1.Opacity = 0
                                        End If
                                        If isImmune = True Then
                                            Dim sineEase As SineEase = New SineEase()
                                            sineEase.EasingMode = EasingMode.EaseInOut
                                            Dim Anim As New DoubleAnimation(1.0, 0.5, TimeSpan.FromSeconds(0.5))
                                            Anim.RepeatBehavior = RepeatBehavior.Forever
                                            Anim.AutoReverse = True
                                            Anim.EasingFunction = sineEase
                                            imgPlayer.BeginAnimation(Control.OpacityProperty, Anim)
                                        Else
                                            imgPlayer.BeginAnimation(Control.OpacityProperty, Nothing)
                                        End If
                                        If elapsedTime - lastDeathTime = 2000 Then
                                            isImmune = False
                                        End If
                                        If elapsedTime Mod 100 * (5 * GAME_SPEED) = 0 Then
                                            Dim random As New Random(elapsedTime)
                                            Dim tempEnemy As Image
                                            Do
                                                tempEnemy = allEnemies(random.Next(0, allEnemies.Count))
                                            Loop Until tempEnemy.Opacity <> 0
                                            EnemLaser(tempEnemy)
                                        End If
                                        If elapsedTime Mod 100 * (1 / GAME_SPEED) = 0 Then
                                            moveEnemies()
                                            If isDefaultState = True Then
                                                For Each enemy As Image In easyEnemies
                                                    enemy.Source = easEnemyS1
                                                Next
                                                For Each enemy As Image In mediumEnemies
                                                    enemy.Source = medEnemyS1
                                                Next
                                                For Each enemy As Image In hardEnemies
                                                    enemy.Source = harEnemyS1
                                                Next
                                                isDefaultState = False
                                            Else
                                                For Each enemy As Image In easyEnemies
                                                    enemy.Source = easEnemyS0
                                                Next
                                                For Each enemy As Image In mediumEnemies
                                                    enemy.Source = medEnemyS0
                                                Next
                                                For Each enemy As Image In hardEnemies
                                                    enemy.Source = harEnemyS0
                                                Next
                                                isDefaultState = True
                                            End If
                                        End If
                                        If shotExists = True Then
                                            If shotsLeft > 0 Then
                                                If fireRate > 1 Then
                                                    If (elapsedTime - laserSpawntime) Mod 40 = 0 Or (fireRate = shotsLeft) Then
                                                        shotsLeft -= 1
                                                        CreateLaser()
                                                    End If
                                                Else
                                                    shotsLeft -= 1
                                                    CreateLaser()
                                                End If
                                            End If
                                        End If
                                        If lazors.Count <> 0 Then
                                            For Each rtangle As Rectangle In lazors
                                                rtangle.SetValue(Canvas.TopProperty, (rtangle.GetValue(Canvas.TopProperty) - FIRE_SPEED))
                                            Next
                                        End If
                                        If elazors.Count <> 0 Then
                                            For Each rtangle As Rectangle In elazors
                                                rtangle.SetValue(Canvas.TopProperty, (rtangle.GetValue(Canvas.TopProperty) + 1))
                                            Next
                                        End If
                                        CollisionDetect()
                                        detectEnemies()
                                        If locReached = -1 Then
                                            endGame()
                                        End If
                                    End Sub, Dispatcher)

        For Each grid As Grid In ParentGrid.Children
            Debug.Print(grid.Name)
            If grid.Name = "Start" Or grid.Name = "Stars" Then
                grid.Opacity = 1
            Else
                grid.Opacity = 0
            End If
        Next
    End Sub

    Sub StartGame(sender As Object, e As KeyEventArgs)
        If Screen = 1 Then
            Start.BeginAnimation(Grid.OpacityProperty, opacityAnim(1.0, 0, TimeSpan.FromSeconds(0.5)))
            Instructions.BeginAnimation(Grid.OpacityProperty, opacityAnim(0, 1, TimeSpan.FromSeconds(0.5)))
            Screen = 2
            timer.Start()
            resetAll()
        ElseIf Screen = 2 Then
            Instructions.BeginAnimation(Grid.OpacityProperty, opacityAnim(1.0, 0, TimeSpan.FromSeconds(0.5)))
            GameScreen.BeginAnimation(Grid.OpacityProperty, opacityAnim(0, 1, TimeSpan.FromSeconds(0.5)))
            Screen = 3
            locReached = 1
        End If
        If Screen = 3 Then
            If RIGHT_KEY.Contains(e.Key) Then
                If imgPlayer.GetValue(Canvas.LeftProperty) < Limiters.ActualWidth - imgPlayer.ActualWidth * 1.4 Then
                    imgPlayer.SetValue(Canvas.LeftProperty, (imgPlayer.GetValue(Canvas.LeftProperty) + defPixel))
                End If
            ElseIf LEFT_KEY.Contains(e.Key) Then
                If imgPlayer.GetValue(Canvas.LeftProperty) > 0 Then
                    imgPlayer.SetValue(Canvas.LeftProperty, (imgPlayer.GetValue(Canvas.LeftProperty) - defPixel))
                End If
            ElseIf SHOOT_KEY.Contains(e.Key) Then
                If Not shotExists Then
                    laserSpawntime = elapsedTime
                    shotExists = True
                End If
            ElseIf STOP_KEY.Contains(e.Key) Then
                timer.Stop()
                PauseOverlay.BeginAnimation(Grid.OpacityProperty, opacityAnim(0, 1, TimeSpan.FromSeconds(0.5)))
                Screen = 4
            End If
        ElseIf Screen = 4 Then
            If STOP_KEY.Contains(e.Key) Then
                Screen = 3
                PauseOverlay.BeginAnimation(Grid.OpacityProperty, opacityAnim(1, 0, TimeSpan.FromSeconds(0.5)))
                timer.Start()
            End If
        End If
        If Screen = 5 Then
            GameScreen.BeginAnimation(Grid.OpacityProperty, opacityAnim(1.0, 0, TimeSpan.FromSeconds(0.5)))
            GameOver.BeginAnimation(Grid.OpacityProperty, opacityAnim(1.0, 0, TimeSpan.FromSeconds(0.5)))
            Start.BeginAnimation(Grid.OpacityProperty, opacityAnim(0, 1, TimeSpan.FromSeconds(0.5)))
            Screen = 1
        End If
    End Sub

    ''Subroutines

    Sub InstalizeBitMaps() 'Creates player and enemies
        Dim pixels(128) As UInteger
        Dim colWhite As UInteger = Convert.ToUInt32("FFFFFFFF", 16)
        Dim colTrans As UInteger = Convert.ToUInt32("00FFFFFF", 16)
        Dim colBlack As UInteger = Convert.ToUInt32("FF161616", 16)
        Dim colRed As UInteger = Convert.ToUInt32("FFFF4D4D", 16)
        Dim colGreen As UInteger = Convert.ToUInt32("FF08FF00", 16)
        Dim EasS1() As Integer = {2, 8, 14, 18, 24, 25, 26, 27, 28, 29, 30, 34, 35, 37, 38, 39, 41, 42, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 57, 58, 59, 60, 61, 62, 63, 65, 66, 68, 74, 76, 80, 81, 83, 84}
        Dim EasS0() As Integer = {2, 8, 11, 14, 18, 21, 22, 24, 25, 26, 27, 28, 29, 30, 32, 33, 34, 35, 37, 38, 39, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 56, 57, 58, 59, 60, 61, 62, 63, 64, 68, 74, 78, 86}
        Dim HarS0() As Integer = {3, 4, 10, 11, 12, 13, 17, 18, 19, 20, 21, 22, 24, 25, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 42, 45, 49, 51, 52, 54, 56, 58, 61, 63}
        Dim HarS1() As Integer = {3, 4, 10, 11, 12, 13, 17, 18, 19, 20, 21, 22, 24, 25, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 41, 43, 44, 46, 48, 55, 57, 62}
        Dim MedS0() As Integer = {4, 5, 6, 7, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 41, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 62, 63, 64, 67, 68, 69, 73, 74, 77, 78, 81, 82, 86, 87, 92, 93}
        Dim MedS1() As Integer = {4, 5, 6, 7, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 41, 42, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 63, 64, 67, 68, 74, 75, 77, 78, 80, 81, 84, 85, 94, 95}
        Dim player() As Integer = {3, 9, 10, 11, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34}
        Dim writeBitmap = Function(w As UShort, h As UShort, colormap() As Integer, color As UInteger)
                              Dim newBitmap = New WriteableBitmap(w, h, w, h, PixelFormats.Bgra32, Nothing)
                              Array.Resize(pixels, w * h)
                              For i = 0 To pixels.Length - 1
                                  If colormap.Contains(i) Then
                                      pixels(i) = color
                                  Else
                                      pixels(i) = colTrans
                                  End If
                              Next
                              newBitmap.WritePixels(New Int32Rect(0, 0, w, h), pixels, w * 4, 0)
                              Return newBitmap
                          End Function
        'Player
        playChar = writeBitmap(7, 4, player, colWhite)
        imgPlayer.Source = playChar
        playChar = writeBitmap(7, 4, player, colGreen)
        imgL1.Source = playChar
        imgL2.Source = playChar
        imgL3.Source = playChar
        'Easy Enemy
        easEnemyS0 = writeBitmap(11, 8, EasS0, colWhite)
        easEnemyS1 = writeBitmap(11, 8, EasS1, colWhite)
        easEnemy.Source = easEnemyS0
        'Med Enemy
        medEnemyS0 = writeBitmap(12, 8, MedS0, colWhite)
        medEnemyS1 = writeBitmap(12, 8, MedS1, colWhite)
        medEnemy.Source = medEnemyS0
        'Hard Enemy
        harEnemyS0 = writeBitmap(8, 8, HarS0, colWhite)
        harEnemyS1 = writeBitmap(8, 8, HarS1, colWhite)
        harEnemy.Source = harEnemyS0
    End Sub

    Sub moveEnemies() ' Dictate enemy movement
        Dim moveTiles As Integer = defPixel * 3
        If locReached = 0 Then
            If (grdCluster.GetValue(TopProperty) + moveTiles) < Border.ActualHeight - grdCluster.ActualHeight Then
                Canvas.SetTop(grdCluster, grdCluster.GetValue(TopProperty) + moveTiles)
                locReached = 1
            Else
                reachedBottom = True
                locReached = -1
            End If
        ElseIf locReached = 1 Then
            If (grdCluster.GetValue(LeftProperty) + moveTiles) < Border.ActualWidth - grdCluster.ActualWidth - defPixel Then
                Canvas.SetLeft(grdCluster, grdCluster.GetValue(LeftProperty) + moveTiles)
            Else
                locReached = 2
            End If
        ElseIf locReached = 2 Then
            If (grdCluster.GetValue(TopProperty) + moveTiles) < Border.ActualHeight - grdCluster.ActualHeight Then
                Canvas.SetTop(grdCluster, grdCluster.GetValue(TopProperty) + moveTiles)
                locReached = 3
            Else
                reachedBottom = True
                locReached = -1
            End If
        ElseIf locReached = 3 Then
            If (grdCluster.GetValue(LeftProperty) - moveTiles) > 0 Then
                Canvas.SetLeft(grdCluster, grdCluster.GetValue(LeftProperty) - moveTiles)
            Else
                locReached = 0
            End If
        End If
    End Sub

    Sub CreateLaser() ' Generates player laser
        Dim Laser As New Rectangle()
        Laser.Name = $"Lazor{index}"
        Laser.Height = 3 * defPixel
        Laser.Width = defPixel
        Grid.SetRowSpan(Laser, 1)
        Grid.SetColumnSpan(Laser, 1)
        Laser.Fill = New SolidColorBrush(Colors.White)
        Laser.StrokeThickness = 1
        Laser.Opacity = 1.0
        Limiters.Children.Add(Laser)
        lazors.Add(Laser)
        centre = imgPlayer.GetValue(Canvas.LeftProperty)
        Canvas.SetLeft(Laser, centre + (imgPlayer.ActualWidth - Laser.Width) / 2)
        Canvas.SetTop(Laser, Limiters.ActualHeight - Laser.Height)
    End Sub

    Sub EnemLaser(enem As Image)
        Dim enemLaser As New Rectangle()
        enemLaser.Height = 3 * defPixel
        enemLaser.Width = defPixel / 2
        Grid.SetRowSpan(enemLaser, 1)
        Grid.SetColumnSpan(enemLaser, 1)
        enemLaser.Fill = New SolidColorBrush(Colors.Green)
        enemLaser.StrokeThickness = 1
        enemLaser.Opacity = 1.0
        Limiters.Children.Add(enemLaser)
        elazors.Add(enemLaser)
        centre = enem.GetValue(Canvas.LeftProperty)
        Canvas.SetLeft(enemLaser, (centre + (enem.ActualWidth - enemLaser.Width) / 2) + getLocation(grdCluster).X)
        Canvas.SetTop(enemLaser, Canvas.GetTop(enem) + getLocation(grdCluster).Y)
    End Sub

    Public Sub CollisionDetect() ' detect player laser collision
        Dim dump As New List(Of Rectangle)
        If lazors.Count <> 0 Then
            For Each laser As Rectangle In lazors
                If laser.GetValue(Canvas.TopProperty) > 0 Then
                    For Each enem As Image In grdCluster.Children
                        If ((getLocation(enem).X + getLocation(grdCluster).X) < (getLocation(laser).X + laser.Width)) And
                            ((getLocation(enem).X + enem.Width + getLocation(grdCluster).X) > getLocation(laser).X) And
                            ((getLocation(enem).Y + getLocation(grdCluster).Y) < (getLocation(laser).Y + laser.Height)) And
                            ((getLocation(enem).Y + enem.Height + getLocation(grdCluster).Y) > getLocation(laser).Y) And
                            enem.Opacity <> 0 Then

                            dump.Add(laser)
                            enem.Opacity = 0
                            pointsAssign(enem)
                        End If
                    Next
                Else
                    dump.Add(laser)
                End If
            Next
        End If
        If elazors.Count <> 0 Then
            For Each laser As Rectangle In elazors
                If Canvas.GetTop(laser) < Border.Height Then
                    If ((getLocation(imgPlayer).X) < (getLocation(laser).X + laser.Width)) And
                            ((getLocation(imgPlayer).X + imgPlayer.Width) > getLocation(laser).X) And
                            (Canvas.GetTop(imgPlayer)) < (Canvas.GetBottom(laser)) And isImmune <> True Then
                        dump.Add(laser)
                        life -= 1
                        If life = 0 Then
                            endGame()
                        Else
                            lastDeathTime = elapsedTime
                            imgPlayer.Opacity = 0
                            Canvas.SetLeft(imgPlayer, 0)
                            isImmune = True
                        End If
                    End If
                End If
            Next
        End If
        For Each removal As Rectangle In dump
            lazors.Remove(removal)
            Limiters.Children.Remove(removal)
        Next
        If lazors.Count = 0 Then
            shotExists = False
            shotsLeft = fireRate
        End If
    End Sub


    Public Sub pointsAssign(enem As Image) ' Assigns points
        If enem.Name.Contains("easEnemy") Then
            score += 10
        ElseIf enem.Name.Contains("medEnemy") Then
            score += 20
        ElseIf enem.Name.Contains("harEnemy") Then
            score += 30
        End If
        Dim scoreString As String = score.ToString()
        If scoreString.Length <4 Then
            Do
                scoreString= String.Concat("0", scoreString)
            Loop Until scoreString.Length = 4
        End If
        lblScore.Content = $"<{scoreString}>"
    End Sub

    Private Sub Star_Loaded(sender As Object, e As RoutedEventArgs) ' Star animations
        Dim sineEase As SineEase = New SineEase()
        sineEase.EasingMode = EasingMode.EaseInOut
        Dim Anim As New DoubleAnimation(1.0, Rnd.Next(2, 7) / 10, TimeSpan.FromSeconds(Rnd.Next(5, 10) / 10))
        Anim.RepeatBehavior = RepeatBehavior.Forever
        Anim.AutoReverse = True
        Anim.EasingFunction = sineEase
        sender.BeginAnimation(Control.OpacityProperty, Anim)
        starAnims.Add(Anim)
    End Sub

    Private Sub enemyAnim(sender As Object, e As RoutedEventArgs)
        If sender.Name.Contains("easEnemy") Then
            easyEnemies.Add(sender)
        ElseIf sender.Name.contains("medEnemy") Then
            mediumEnemies.Add(sender)
        ElseIf sender.Name.Contains("harEnemy") Then
            hardEnemies.Add(sender)
        End If
    End Sub

    Public Sub detectEnemies() ' Detect level finished
        Dim enemKilled As Integer = 0
        For Each enem As Image In grdCluster.Children
            If enem.Opacity = 0 Then
                enemKilled += 1
            End If
        Next
        If enemKilled = grdCluster.Children.Count Then
            Canvas.SetLeft(grdCluster, 0)
            Canvas.SetTop(grdCluster, 0)
            For Each enem As Image In grdCluster.Children
                enem.Opacity = 1
            Next
            locReached = 1
        End If
    End Sub

    Public Sub endGame() ' End of game handling
        timer.Stop()
        If score > hiscore Then
            Dim scoreString As String = score.ToString()
            If scoreString.Length < 4 Then
                Do
                    scoreString = String.Concat("0", scoreString)
                Loop Until scoreString.Length = 4
            End If
            lblHiscore.Content = $"<{scoreString}>"
        End If
        GameOver.Opacity = 1
        Screen = 5
    End Sub

    Public Sub resetAll() ' Reset Enemy
        For Each enem As Image In grdCluster.Children
            enem.Opacity = 1
        Next
        locReached = -2
        Canvas.SetLeft(grdCluster, 0)
        Canvas.SetTop(grdCluster, 0)
        lblScore.Content = "<0000>"
        life = 3
        Canvas.SetLeft(imgPlayer, 245.5)
    End Sub

End Class
