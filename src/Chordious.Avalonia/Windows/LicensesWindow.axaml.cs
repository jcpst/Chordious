// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for LicensesWindow.xaml
    /// </summary>
    public partial class LicensesWindow : ChordiousWindow
    {
        public LicensesWindow()
        {
            InitializeComponent();
            LicensesTab.SelectedIndex = 0; // Fixes focus bug when check for updates is disabled
        }
    }
}
