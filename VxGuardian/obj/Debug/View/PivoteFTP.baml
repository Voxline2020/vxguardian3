   M S B A M L   `   `   `  ����E  AVxGuardian, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:VxGuardian.ViewVxGuardian.View  5   	       VxGuardian.View.PivoteFTP   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:VxGuardian.View     5   	   -   5          � Title$  
Pivote FTP��5	   	   $	��490��6   $	��394��6)    � WindowStartupLocation$ CenterScreen=�65    � Topmost .  � 
ResizeMode$ NoResize=�6i    � WindowStyle$
 None=�6�   .��5
      � $��20q�6	    � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5      �� 5      �� 5      $
��0.6* �6   .+�5      �� $��0��6   $��0��6   $
��Left=�6)   $��Center=�6D   $
��Bold"�6_   $��
Pivote FTP��6q   �� 5   
   -   5         ��Name$ 	BtnConfig��5      $��0��6$   $��1��61   $��Right=�6A   $��Center=�6]   $
��Auto��6x   $
��Auto��6�   $
��Bold"�6�   .��6�   Config6�   �� 5      $��1��6   $��0��6    �� Orientation$ Vertical=�6.   .+�5       �� .��6   Codigo Pivote6   �� 5!      -   5        	 ��Name$	 	TxtCodigo��5!   x   $��5q�6   .��6�   	10010016�   5"      �� 5$      $��2��6   $��0��6   $ Vertical=�6.   .+�5%      �� .��6   Raiz6   �� 5&      -   5        $	 
TxtRaizSel��5&      $��5q�6   .��6/   /62   5'      �� 5(      -   5        $ SelRaiz��5(      $��2��6   $��1��6+   $��Center=�6;   $��Bottom=�6X   $��0q�6s   $	��100��6~   .��6�   Seleccionar6�   �� 5*      $��3��6   $��0��6   $ Vertical=�6.   .+�5+      �� .��6   Tiempo de Chequeo (mins)6(   �� 5,      -   5        $	 
TxtChequeo��5,      $��5q�6   $��30��6/   $
��Left=�6�   .��6�   16�   5-      �� 5/      $��	0,10,0,10q�6   $��4��6#   $��2��60   6F   �� 51      $ Vertical=�6   $��5��6(   $��0��65   .+�52      �� .��6   
Servidor6   �� 53      -   5        $	 TxtServidor��53      $��5q�6   .��6�   https://voxcms.cl6�   54      �� 56      $ Vertical=�6   $��6��6(   $��0��65   .+�57      �� .��6   Codigo Maestro6   G� 58      -   5        
 G�Name$
 TxtCodigoMaestro��58      $��5q�6   6�   59      �� 5;      $��7��6   $��2��6   $	��Top=�63   $��Center=�6K   $
��Auto��6h   .+�5<      �� -	   5        $ 
BtnIniciar��5<      $��Center=�6#   $��Center=�6@   $��10q�6[   $��40,10q�6g   $
��Auto��6w   $
��Auto��6�   $
��Bold"�6�   .��6�   	Iniciar6�   �� 5=      -
   5        $ 	BtnCerrar��5=      $��Center=�6"   $��Center=�6?   $��10q�6Z   $��10q�6f   $
��Auto��6s   $
��Auto��6�   $
��Bold"�6�   
���  ��
�������.��6�   Cerrar6�   5>      5?      5@      