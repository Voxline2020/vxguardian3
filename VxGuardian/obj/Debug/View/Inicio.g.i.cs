﻿#pragma checksum "..\..\..\View\Inicio.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "62B95BD560BF9784F88C53CBF2D484128BC0B2C85475F1BD5742D1528003076A"
//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using VxGuardian.View;


namespace VxGuardian.View {
    
    
    /// <summary>
    /// Inicio
    /// </summary>
    public partial class Inicio : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\View\Inicio.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RBtnVoxCMS;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\View\Inicio.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RBtnLocal;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\View\Inicio.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RBtnFTP;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\View\Inicio.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton RBtnPivote;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\View\Inicio.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSiguiente;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/VxGuardian;component/view/inicio.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\Inicio.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.RBtnVoxCMS = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 2:
            this.RBtnLocal = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 3:
            this.RBtnFTP = ((System.Windows.Controls.RadioButton)(target));
            
            #line 14 "..\..\..\View\Inicio.xaml"
            this.RBtnFTP.Checked += new System.Windows.RoutedEventHandler(this.RBtnFTP_Checked);
            
            #line default
            #line hidden
            return;
            case 4:
            this.RBtnPivote = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 5:
            this.BtnSiguiente = ((System.Windows.Controls.Button)(target));
            
            #line 16 "..\..\..\View\Inicio.xaml"
            this.BtnSiguiente.Click += new System.Windows.RoutedEventHandler(this.BtnSiguiente_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

