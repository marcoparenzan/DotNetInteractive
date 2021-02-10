﻿using Microsoft.DotNet.Interactive;
using System.Windows;

namespace DemoNotebook.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(Dispatcher);
        }
    }
}
