// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChordiousWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModelExtended();

            Opened += MainWindow_Opened;
        }

        private void MainWindow_Opened(object sender, EventArgs e)
        {
            try
            {
                ((MainViewModelExtended)DataContext).OnLoaded();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }
    }
}
