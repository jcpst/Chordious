// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : ChordiousWindow
    {
        public OptionsViewModelExtended VM
        {
            get
            {
                return DataContext as OptionsViewModelExtended;
            }
            private set
            {
                DataContext = value;
            }
        }

        public OptionsWindow()
        {
            VM = new OptionsViewModelExtended();
            InitializeComponent();
        }
    }
}
