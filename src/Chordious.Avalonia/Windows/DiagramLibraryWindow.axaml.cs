// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    public partial class DiagramLibraryWindow : ChordiousWindow
    {
        public DiagramLibraryViewModel VM
        {
            get
            {
                return _vm ??= DataContext as DiagramLibraryViewModel;
            }
        }
        private DiagramLibraryViewModel _vm;

        private PointerPressedEventArgs _nodePressedArgs;
        private Point _nodePressedPosition;

        public DiagramLibraryWindow()
        {
            InitializeComponent();

            // Keep the view model's selection in sync with the list
            DiagramsListView.SelectionChanged += DiagramsListView_SelectionChanged;

            // Dragging selected diagrams
            _ = new DiagramListInteraction(DiagramsListView, () => (ICollection<ObservableDiagram>)VM.SelectedNode?.SelectedDiagrams ?? Array.Empty<ObservableDiagram>(), (pressedArgs, image, od) => IntegrationUtils.DiagramLibraryNodeToDragDrop(pressedArgs, VM.SelectedNode, true));

            // Dragging whole collections
            NodesListView.AddHandler(PointerPressedEvent, NodesListView_PointerPressed, RoutingStrategies.Tunnel, true);
            NodesListView.AddHandler(PointerMovedEvent, NodesListView_PointerMoved, RoutingStrategies.Bubble, true);

            NodesListView.AddHandler(DragDrop.DropEvent, NodesListView_Drop);
            DiagramsListView.AddHandler(DragDrop.DropEvent, DiagramsListView_Drop);
        }

        private void DiagramsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ObservableDiagramLibraryNode selectedNode = VM.SelectedNode;

            if (selectedNode is not null)
            {
                foreach (object item in e.AddedItems)
                {
                    ObservableDiagram od = item as ObservableDiagram;
                    selectedNode.SelectedDiagrams.Add(od);
                }

                foreach (object item in e.RemovedItems)
                {
                    ObservableDiagram od = item as ObservableDiagram;
                    selectedNode.SelectedDiagrams.Remove(od);
                }
            }
        }

        private static ObservableDiagramLibraryNode GetNode(RoutedEventArgs e)
        {
            return (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true)?.DataContext as ObservableDiagramLibraryNode;
        }

        private void NodesListView_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            _nodePressedArgs = GetNode(e) is not null && e.GetCurrentPoint(NodesListView).Properties.IsLeftButtonPressed ? e : null;
            _nodePressedPosition = e.GetPosition(NodesListView);
        }

        private void NodesListView_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_nodePressedArgs is null)
            {
                return;
            }

            if (!e.GetCurrentPoint(NodesListView).Properties.IsLeftButtonPressed)
            {
                _nodePressedArgs = null;
                return;
            }

            Point delta = e.GetPosition(NodesListView) - _nodePressedPosition;
            if (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4)
            {
                PointerPressedEventArgs pressedArgs = _nodePressedArgs;
                _nodePressedArgs = null;

                if (GetNode(pressedArgs) is ObservableDiagramLibraryNode sourceNode)
                {
                    IntegrationUtils.DiagramLibraryNodeToDragDrop(pressedArgs, sourceNode, false);
                }
            }
        }

        private void DiagramsListView_Drop(object sender, DragEventArgs e)
        {
            if (VM.NodeIsSelected)
            {
                IntegrationUtils.DragDropToDiagramLibraryNode(e.DataTransfer, VM.SelectedNode, IntegrationUtils.GetDropAction(e.KeyModifiers));
                e.Handled = true;
            }
        }

        private void NodesListView_Drop(object sender, DragEventArgs e)
        {
            // Dropping onto a collection targets it, dropping onto empty space creates a new one
            IntegrationUtils.DragDropToDiagramLibraryNode(e.DataTransfer, GetNode(e), IntegrationUtils.GetDropAction(e.KeyModifiers));
            e.Handled = true;
        }
    }
}
