   M S B A M L   `   `   `  ���� H  DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=nullV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  K GDank Player V3.0, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Dank_Player_V3._0Dank_Player_V3._0 "   Dank_Player_V3._0.MainWindow   HControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro  R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Z VFontAwesome.WPF, Version=4.7.0.37774, Culture=neutral, PublicKeyToken=0758b07a11a4f466-fa$http://schemas.fontawesome.io/icons/  +localclr-namespace:Dank_Player_V3._0    � Title$  Dank Player v3.0��(   "MahApps.Metro.Controls.MetroWindow   TitleAlignment$ Center=�$
��1920��$
��1080�� � 
ResizeMode$ NoResize=� � WindowStyle$
 None=� � WindowState$ 	Maximized=�   ShowTitleBar .     IgnoreTaskbarOnMaximize . .��� ���	#FF252525.+�� .+�r� -    r�Name$ 
mainPlayer��$y�Hidden=�$��2���� -    ��Name$ 	debugText��$
��Left=�$	��Top=�$��	20,20,0,0q�
�������� -   	 �Name$	 btnMenu��$��Right=�$��Center=����	#33FFFFFF$��20��$	��100��.+��� $��0,33,0,0q�$��Center=�$	��‹��$��20(�$��Arial)�
�������� -   $!	 backgroundSelectorContainer��$��100,100,600,100q�
 J� Opacity$
 0Q�{�.  #   MahApps.Metro.Controls.Flyout  -     Name$ 
flyoutMenu��   Position$ Right=�   IsOpen .  $	��500�����	#BF252525 K� ZIndex$ 10��.���  � RowDefinitions �� $��2* ��� $��8* �.+�� $��0��.+��� -   $��txtCurrTime��$��Center=�$��0,0,100,100q�$	��100��$

 0.75Q�$��50��
������� �� TextAlignment$ Center=�$��30(�0:00�� $��Center=�$��	0,0,0,100q�$��50��$

 0.75Q�$��50��
�������$ Center=�$��30(�/�� -   $��
txtMaxTime��$��Center=�$��100,0,0,100q�$	��100��$��50��$

 0.75Q�
�������$ Center=�$��30(�0:00�� -   $��playerSlider��$��	50,0,50,0q�$

 0.75Q�$��Center=�   Horizontal_Slider#���   �� -	   $��volumeSlider��$��50,75,275,0q�$

 0.75Q�$��Center=�  Horizontal_Slider2#���   � Maximum$	 100Q� � Value$ 50Q�� $��1��.+�"  FontAwesome.WPF.ImageAwesome  -
     Name$ tracksLoadingIndicator��$y�Hidden=�   Icon$ CircleOutlineNotch=�   Spin .    SpinDuration$ 4Q�   
Foreground
 �����$

 0.75Q�$��100, 375, 100, 375q��� -    ��Name$ btnNewPlaylist��$��100, 350, 100, 350q�$��New playlist��$y�Visible=����	#FF2F2F2F$

 0.75Q��� -    ��Name$ lstTrackList��$y�Hidden=�$

 0.75Q�$ 10������ .R��  � ColumnDefinitions �� $��9* ��� $��1* �.+��� $��0��
�������$
��Wrap=�$��10,0,0,0q�$
��Left=�$��Center=����� *title+�� $��1��
�������$
��Wrap=�$��Center=�$��Center=����� *
duration+�� -   $ txtMainTitle��$ 5��$��Center=�$)��#Start by creating a new playlist >>��$��
0, 0, 0, 0q�$��Center=�$��60(�$��Segoe UI Light)�$
��Bold"�
������� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     
BlurRadius$ 10Q�   Opacity$	 0.6Q�r� -   $ mediaBackground�� r� Source$B <E:\Users\Gebruiker\Downloads\AbstractRotatingCubesVidevo.mov?�$ -5�� � 	Resources   SliderRepeatButton  �� 
TargetType! �� Property" �� Value# �� 
TargetType  SliderRepeatButton1$ J� SnapsToDevicePixels  SliderThumb% ��Name 
 Slider& ]�Name' ]� DecreaseRepeatButton( ]� Thumb) n�Name* ]� IncreaseRepeatButton  Slider2+ �� Triggers, P� Property- P� Value��%�  &	       &	 �     &	 �    &	 D    7  7  7  &	 �    7  7  7  &	  �    7  &	 �    7  �� "  ��.��� ! �@J�SnapsToDevicePixels" . �� ! �@�OverridesDefaultStyle" . �� ! � ��" .  �� ! � |�" .  �� ! � ��" �� "# ��.R��� $��0q�
�������
�������$��10��$	
 0.5Q��� "  ��.��� ! �@J�SnapsToDevicePixels" . �� ! �@�OverridesDefaultStyle" . �� ! � ��" �� "# ��.R��� $ . 
�������$��1q�
�������$��10��$
 1Q��� "  n�.��� ! �@J�SnapsToDevicePixels" . �� ! �@�OverridesDefaultStyle" . �� ! � ��" �� "# n�.R��� $% thumbEllipse��$��24��$��6��
��������� "# ��.R��  �� $
��Auto ��� $
��Auto �#��z���� $
��Auto �.+�]� $& 
PART_Track��$��1��' �� 8��  ( n� $) playerThumb��-   8�� * �� 8�� �� "# ��.R��  �� $
��Auto ��� $
��Auto �#��z���� $
��Auto �.+�]� $& 
PART_Track��$��1��' �� 8��  ( n� $) playerThumb��-   8�� * �� 8�� �� "  ��.��� ! � |�" .  �� ! �@J�SnapsToDevicePixels" . �� ! �@�OverridesDefaultStyle" . + P� , �@��Orientation$- 
Horizontal=�. ��� ! � ��$" 21���� ! � ��$	" 104���� ! � ��8"   �� "  ��.��� ! � |�" .  �� ! �@J�SnapsToDevicePixels" . �� ! �@�OverridesDefaultStyle" . + P� , �@��Orientation$- 
Horizontal=�. ��� ! � ��$" 21���� ! � ��$	" 104���� ! � ��8"   