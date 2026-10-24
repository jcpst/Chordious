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
    /// <summary>
    /// Shared pointer handling for the lists of diagram images (finder results, library):
    /// - keeps a multi-selection intact when pressing an already-selected diagram, so it can be dragged
    /// - starts a drag once the pointer moves far enough with the left button held
    /// </summary>
    public class DiagramListInteraction
    {
        private const double DragThreshold = 4.0;

        private readonly ListBox _listBox;
        private readonly Func<ICollection<ObservableDiagram>> _getSelectedDiagrams;
        private readonly Action<PointerPressedEventArgs, Image, ObservableDiagram> _startDrag;

        private PointerPressedEventArgs _pressedArgs;
        private Point _pressedPosition;
        private Image _pressedImage;
        private Image _lastClicked;

        public DiagramListInteraction(ListBox listBox, Func<ICollection<ObservableDiagram>> getSelectedDiagrams, Action<PointerPressedEventArgs, Image, ObservableDiagram> startDrag)
        {
            _listBox = listBox ?? throw new ArgumentNullException(nameof(listBox));
            _getSelectedDiagrams = getSelectedDiagrams ?? throw new ArgumentNullException(nameof(getSelectedDiagrams));
            _startDrag = startDrag ?? throw new ArgumentNullException(nameof(startDrag));

            _listBox.AddHandler(InputElement.PointerPressedEvent, ListBox_PointerPressed, RoutingStrategies.Tunnel, true);
            _listBox.AddHandler(InputElement.PointerMovedEvent, ListBox_PointerMoved, RoutingStrategies.Bubble, true);
            _listBox.AddHandler(InputElement.PointerReleasedEvent, ListBox_PointerReleased, RoutingStrategies.Bubble, true);
        }

        public static Image GetDiagramImage(RoutedEventArgs e, out ObservableDiagram diagram)
        {
            Image image = (e.Source as Visual)?.FindAncestorOfType<Image>(includeSelf: true);
            diagram = image?.DataContext as ObservableDiagram;
            return diagram is not null ? image : null;
        }

        private void ListBox_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            Image image = GetDiagramImage(e, out ObservableDiagram od);
            if (image is null || !e.GetCurrentPoint(image).Properties.IsLeftButtonPressed)
            {
                return;
            }

            _pressedArgs = e;
            _pressedPosition = e.GetPosition(image);
            _pressedImage = image;

            // Don't collapse a multi-selection on the first click of an already-selected diagram
            if (e.ClickCount == 1 && e.KeyModifiers == KeyModifiers.None && _getSelectedDiagrams().Contains(od) && image != _lastClicked)
            {
                _lastClicked = image;
                e.Handled = true;
            }
            else
            {
                _lastClicked = null;
            }
        }

        private void ListBox_PointerMoved(object sender, PointerEventArgs e)
        {
            if (_pressedArgs is null || _pressedImage is null)
            {
                return;
            }

            if (!e.GetCurrentPoint(_pressedImage).Properties.IsLeftButtonPressed)
            {
                _pressedArgs = null;
                return;
            }

            Point delta = e.GetPosition(_pressedImage) - _pressedPosition;
            if (Math.Abs(delta.X) > DragThreshold || Math.Abs(delta.Y) > DragThreshold)
            {
                PointerPressedEventArgs pressedArgs = _pressedArgs;
                _pressedArgs = null;

                if (_pressedImage.DataContext is ObservableDiagram od)
                {
                    _startDrag(pressedArgs, _pressedImage, od);
                }
            }
        }

        private void ListBox_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _pressedArgs = null;
        }
    }
}
