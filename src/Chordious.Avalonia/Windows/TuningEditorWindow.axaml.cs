// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for TuningEditorWindow.xaml
    /// </summary>
    public partial class TuningEditorWindow : ChordiousWindow
    {
        public TuningEditorWindow()
        {
            InitializeComponent();
            Loaded += TuningEditorWindow_Loaded;
        }

        private void TuningEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TuningEditorViewModel vm && vm.IsNew)
            {
                IntegrationUtils.EnableAutoSelectOnFirstLoad(NameTextBox);
            }
        }
    }
}
