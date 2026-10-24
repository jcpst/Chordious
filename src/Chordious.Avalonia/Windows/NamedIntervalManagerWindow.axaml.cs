// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for InstrumentManagerWindow.xaml
    /// </summary>
    public partial class NamedIntervalManagerWindow : ChordiousWindow
    {
        public NamedIntervalManagerViewModel VM
        {
            get
            {

                return DataContext as NamedIntervalManagerViewModel;
            }
            private set
            {
                DataContext = value;
            }
        }

        public NamedIntervalManagerWindow() : this(null) { } // For the designer

        public NamedIntervalManagerWindow(NamedIntervalManagerViewModel vm)
        {
            VM = vm;
            InitializeComponent();
        }
    }
}
