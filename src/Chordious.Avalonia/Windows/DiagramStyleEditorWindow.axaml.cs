// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for DiagramStyleEditorWindow.xaml
    /// </summary>
    public partial class DiagramStyleEditorWindow : ChordiousWindow
    {
        public DiagramStyleEditorWindow()
        {
            InitializeComponent();

            // Clicking anywhere commits any in-progress edit (bindings update on LostFocus)
            RootPanel.AddHandler(PointerPressedEvent, FocusOn_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void FocusOn_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if ((e.Source as Visual)?.FindAncestorOfType<ComboBox>(includeSelf: true) is null)
            {
                RootPanel.Focus();
            }
        }
    }
}
