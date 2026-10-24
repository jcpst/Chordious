// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    public partial class DiagramEditorWindow : ChordiousWindow
    {
        public DiagramEditorViewModel VM
        {
            get
            {
                return _vm ??= DataContext as DiagramEditorViewModel;
            }
        }
        private DiagramEditorViewModel _vm;

        private PointerPressedEventArgs _pressedArgs;
        private Point _pressedPosition;

        public DiagramEditorWindow()
        {
            InitializeComponent();

            // Any click (left to edit, right for the context menu) targets the element under the cursor
            DiagramImage.AddHandler(PointerPressedEvent, DiagramImage_PointerPressed, RoutingStrategies.Tunnel, true);
            DiagramImage.AddHandler(PointerMovedEvent, DiagramImage_PointerMoved, RoutingStrategies.Bubble, true);
        }

        private void DiagramImage_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Point p = e.GetPosition(DiagramImage);
            UpdateCursorPosition(p);

            if (e.GetCurrentPoint(DiagramImage).Properties.IsLeftButtonPressed)
            {
                _pressedArgs = e;
                _pressedPosition = p;
            }
        }

        private bool UpdateCursorPosition(Point p)
        {
            ObservableDiagram od = VM?.ObservableDiagram;
            if (od is not null)
            {
                od.CursorX = p.X;
                od.CursorY = p.Y;
                return od.ValidCommandsAtCursor;
            }
            return false;
        }

        private void DiagramImage_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_pressedArgs is null)
            {
                return;
            }

            if (!e.GetCurrentPoint(DiagramImage).Properties.IsLeftButtonPressed)
            {
                _pressedArgs = null;
                return;
            }

            Point delta = e.GetPosition(DiagramImage) - _pressedPosition;
            if (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
            {
                PointerPressedEventArgs pressedArgs = _pressedArgs;
                _pressedArgs = null;
                IntegrationUtils.DiagramToDragDrop(pressedArgs, VM.ObservableDiagram);
            }
        }
    }
}
