   M S B A M L   `   `   `  ���� I  EBigBallsWarVII, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:BigBallsWarVIIBigBallsWarVII      BigBallsWarVII.MainWindow   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:BigBallsWarVII       � Icon$>  8pack://application:,,,/Resources/BigBallsWarVII_icon.ico�� � Title$ 
MainWindow��$	��450��$	��800��.���  � ColumnDefinitions �� �� $��6* ���  � RowDefinitions �� $
��316* ��� $
��119* �.+��� -   $��backGroundCanva��$��2��$��3��.+��� -   $	��Sky��$	��800��$	��433��$��Center=�$	��Top=�����  �� 
StartPoint$	 0,0&� �� EndPoint$	 0,1&�.���  � Color$ SkyBlue�� � Offset$	 1.0Q�� $ 	LightBlue��$	 0.3Q��� -   $��Ground��$	��800��$	��205��$��Center=�$	��Top=�
 �� Top$	 230������ $	 0,0&�$	 0,1&�	 �� Opacity$		 0.9Q�.��� $ LightSkyBlue��$ 0Q�� $ 	Goldenrod��$	 0.2Q�� $ SeaGreen��$	 0.3Q�� $ Green��$	 0.5Q��� -   $��cloud1��$��/CloundIMG.png��$
��Left=�$	��Top=�
 �� Left$	
 526��$ 22��$	��133��$	��264���� -   $��cloud2��$��/CloundIMG.png��$
��Left=�$	��Top=�$
 2��$	 -30��$	��160��$	��317���� -   $��cloud3��$��/CloundIMG.png��$
��Left=�$��Center=�$	
 394��$ 22��$	��133��$	��264���� -   $��cloud4��$��/CloundIMG.png��$
��Left=�$	��Top=�$
 2��$	 -30��$	��160��$	��317���� -   $��cloud5��$��/CloundIMG.png��$
��Left=�$	��Top=�$
 0��$ 0��$	��160��$	��317���� -	   $��enemyCastle��$��75��$	��120��$��Center=�$	��Top=�$	 157��$
 45������ $	 0,0&�$	 0,1&�.��� $ DarkRed��$	 1.0Q�� $	 Red��$	 0.0Q��� -
   $��enemyHPBarBackGround��$	��120��$��20��$	 130��$
 45��
���   � J� Opacity$	 0.5Q��� -   $��
enemyHPBar��$	��120��$��20��$	 130��$
 45��
���  ��$	 0.5Q��� -   $��enemyCastleHPText��$
 66��$	 132��$��12(�$��84�� �� TextAlignment$ Right=�$��Center=�$��Center=��� -    �Name$ enemyHPCurrent��$
�1600�� �� $� /�� �� -   $ enemyMaxHPCurrent��$
�1600������  �� Color$ White���� -    ��Name$ myCastle��$��75��$	��120��$��Center=�$	��Top=�$	 157��$	
 680������ $	 0,0&�$	 0,1&�.��� $
 Blue��$	 1.0Q�� $ DeepSkyBlue��$	 0.0Q��� -   $��myHPBarBackGround��$	��120��$��20��$	 130��$	
 635��
���   �$	 0.5Q�$
��Left=�$��Center=��� -   $��myHPBar��$	��120��$��20��$	 130��$	
 635��
����  �$	 0.5Q�$
��Left=�$��Center=��� -   $��myCastleText�� �� Right$	 658��$	 132��$��12(�$ Right=�$	
 658��$��84��$
��Left=�$��Center=��� -   $ myHPCurrent��$
�1000�� �� $� /�� �� -   $ myMaxHPCurrent��$
�1000������ $ White��A� -   $��moneyBackGround��$	��277��$	��200��
���   �$	 0.7Q�$
��Left=�$	
 557��$��Center=�$	 -90���� -   $��myBallsCount��$	��100��$��20(�$	
 605��$ 60��$
��Left=�$��Center=�$	��175������ $ 
Aquamarine��$	 1Q��� -   $��isGameOverMask��$
�� 800��$
�� 250��
���   �$	 0.3Q�$
��Left=�$ 51��$��Center=��� -   $��gameOverLabel��$��你贏了！��$	
 245��$ 87��$��75(�$
��Left=�$��Center=��� -   $��isCashEnough��$��錢不夠！��$��30(�$	
 340��$ 51��$��Arial Black)�$��Center=�$	��Top=����� $	 Red��� -   $��gameStartBox��$
��Left=�$��Center=�$	��239��$	
 220��$ 84�� �� $��0* ��� .+��� -   $��Title��$��球球大戰爭！��$��55(�$
��Left=�$	��Top=�$��1��$��10,0,0,0q��� -   $��startButton��$��開始遊戲��$��30(�$��Center=�$	��Top=�$��	0,151,0,0q�$��1���� -   $��	mainCanva��$��2��$��3��.+��� -   $��buttonBackground��$	��800��$	��120�� �� Bottom$ 0��
���   �$	 0.4Q�$��Center=�$	 302��$	��Top=��� -   $��myMoney��$��20(�$	
 650��$ 29��$
��Left=�$��Center=��� $�金錢 :�� �� -    $ currentMoney��$
�5000�� �� $�$������ $ White���� -!   $��ballStackPanel�� �� Orientation$ 
Horizontal=�$	��110��$	��780��$ 0��$ 10��$��RightToLeft=�
�������$	 0.9Q�$��Center=�$
 20��$	 307��$	��Top=�.+��  �� $	��53* ��� $	��57* �.+��� -"   $��largeBotton��$	��100��$	��100��$
��250$��$��	13,5,13,5q�$��24(�$��1,42,1,1q� ^� InitialShowDelay$	 250�� ^� ShowDuration$ 10000�� ^� BetweenShowDelay$	 200��$��2�� � ToolTip _� $9��3最菁英的球體，無可比擬的攻擊與血量��A� -#   $��previewLargeBall��$��40��$��40��
��� ���$��43,15,43,55q� J� IsHitTestVisible$ False��$��2��� .+��� -$   $��squareBotton��$	��100��$	��100��$
��$150��$��0,5,0,5q�$��24(�$��1,40,1,1q�$	 250��$ 10000��$	 200�� _� $3��-如同城牆般的正方形，但不會攻擊���� -%   $��previewSquare��$��25��$��40��
���**��$��38,15,38,55q�$ False��� .+��� -&   $��mediumBotton��$	��100��$	��100��$	��75$��$��0,5,0,5q�$��24(�$��1,40,1,1q�$	 250��$ 10000��$	 200��$��Center=� _� $3��-中等的球，體力與攻擊力都上升了��A� -'   $��previewMediumBall��$��25��$��25��
����  �$��44,30,44,55q�$ False��� .+��� -(   $��triangleBotton��$	��100��$	��100��$	��30$��$��0,5,5,5q�$��24(�$��1,40,1,1q�$	 250��$ 10000��$	 200�� _� $6��0快攻三角形，超痛攻擊但馬上就死了���� $��/Image/Triangle.png��$��20��$��20��$��42,30,43,50q�$ False��� .+��� -)   $��smallBotton��$	��100��$	��100��$	��10$��$��24(�$��0,40,0,0q�$	 250��$ 10000��$	 200��$��Center=�$��0,5,0,5q� _� $3��-基本的小球，普通的攻擊力跟血量��A� -*   $��previewSmallBall��$��15��$��15��
��� � �$��43,35,43,50q�$ False���� -+   $��smallBottonSlider��$��10��$	��100��$	 0.5Q�
���   �$	
 254��$ 20��$��Center=�$	 312��$	��Top=��� -,   $��triangleBottonSlider��$��10��$	��100��$	 0.5Q�
���   �$	
 360��$ 20��$��Center=�$	 313��$	��Top=��� --   $��mediumBottonSlider��$��10��$	��100��$	 0.5Q�
���   �$	
 467��$ 20��$��Center=�$	 313��$	��Top=��� -.   $��squareBottonSlider��$��10��$	��100��$	 0.5Q�
���   �$	
 574��$ 20��$��Center=�$	 312��$	��Top=��� -/   $��largeBottonSlider��$��10��$	��100��$	 0.5Q�
���   �$	
 687��$ 20��$��Center=�$	 313��$	��Top=�A� -0   $��moneyUpgrateBackGround��$	��187��$	��100��
���   �$	 0.7Q�$��Center=�$	��Top=�$ 10��$
 35��$' !每秒增加的金錢數量增加��$	 250��$ 10000��$	 309���� -1   $��moneyUpgrateTitleText��$��金錢升級��$
 70��$ 55��$��28(�$��Comic Sans MS)� J� RenderTransformOrigin$ 0.5,0.5&�$	��116��$��Center=�$	��Top=�$ False��$	 325������ $	 0,0&�$	 0,1&�.��� $
 Gold��$	 1.0Q�� $ Yellow��$	 0.0Q��� -2    ��Name$ UpgrateText��$��24(�$
 92��$ 25��$
��Left=�$	��Top=�$ False��$	 361������ $	 0,0&�$	 0,1&�.��� $
 Gold��$	 1.0Q�� $ Yellow��$	 0.0Q��� -3   $ howMuchUpgrateText�� � Language$ zh-tw�$�200元���� �� $ zh-tw��� �� �� -4   $��restartButton��$��重新開始��$	
 334��$	 276��$��30(�$��Center=�$	��Top=� � 	Resources    
cloud1Move �� 
TargetName �� TargetProperty W� From	 W� To  g� Duration! g� RepeatBehavior��%�  &	        �� .��W� $ cloud1��$ (Canvas.Left)� 400 -400$  0:0:40D�W� $ cloud2��$ (Canvas.Left)� 2 -400$  0:0:25D�W� $ cloud3��$ (Canvas.Left)� 900 -400$  0:1:30D�$! Forever��W� $ cloud4��$ (Canvas.Left)� 800 -400$  0:1:10D�$! Forever��W� $ cloud5��$ (Canvas.Left)� 1200 -400$  0:1:40D�$! Forever��" � Triggers" :� # :� RoutedEvent$# Window.Loaded��.[��� #��[  