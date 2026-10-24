// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for InstrumentEditorWindow.xaml
    /// </summary>
    public partial class InstrumentEditorWindow : ChordiousWindow
    {
        public InstrumentEditorWindow()
        {
            InitializeComponent();
            Loaded += InstrumentEditorWindow_Loaded;
        }

        private void InstrumentEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InstrumentEditorViewModel vm && vm.IsNew)
            {
                IntegrationUtils.EnableAutoSelectOnFirstLoad(NameTextBox);
            }
        }
    }
}
