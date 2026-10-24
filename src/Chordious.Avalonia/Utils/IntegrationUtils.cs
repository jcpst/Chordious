// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

using SkiaSharp;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    public class IntegrationUtils
    {
        public static AppViewModel AppVM => AppViewModel.Instance;

        public static string TempPath => _tempPath ??= Path.Combine(Path.GetTempPath(), "Chordious");
        private static string _tempPath;

        public static string GetTempPathAndCreateIfNecessary()
        {
            string tempPath = TempPath;

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            return tempPath;
        }

        private static TopLevel ActiveTopLevel => TopLevel.GetTopLevel(DialogHost.ActiveWindow);

        #region Clipboard

        public static void TextToClipboard(string text)
        {
            IClipboard clipboard = ActiveTopLevel?.Clipboard ?? throw new NotSupportedException();
            DialogHost.WaitFor(clipboard.SetTextAsync(text));
        }

        public static void DiagramToClipboard(ObservableDiagram diagram, float scaleFactor)
        {
            if (diagram is null)
            {
                throw new ArgumentNullException(nameof(diagram));
            }

            IClipboard clipboard = ActiveTopLevel?.Clipboard ?? throw new NotSupportedException();

            DataTransfer data = new DataTransfer();
            data.Add(CreateImageItem(diagram, scaleFactor));

            DialogHost.WaitFor(clipboard.SetDataAsync(data));
            DialogHost.WaitFor(clipboard.FlushAsync());
        }

        private static DataTransferItem CreateImageItem(ObservableDiagram diagram, float scaleFactor)
        {
            using SKBitmap skBitmap = ImageUtils.SvgTextToSKBitmap(diagram.SvgText, diagram.TotalWidth, diagram.TotalHeight, scaleFactor, Background.None);

            DataTransferItem item = new DataTransferItem();

            Bitmap bitmap = ImageUtils.SKBitmapToBitmap(skBitmap);
            item.SetBitmap(bitmap);

            // As PNG temp file, so the image can be pasted / dropped as a file
            if (GetEnhancedCopy())
            {
                string tempFile = ImageUtils.BitmapToPngTempFile(skBitmap, diagram.Title);
                IStorageFile storageFile = DialogHost.WaitFor(ActiveTopLevel.StorageProvider.TryGetFileFromPathAsync(tempFile));
                if (storageFile is not null)
                {
                    item.SetFile(storageFile);
                }
            }

            return item;
        }

        #endregion

        #region Drag and Drop

        private static readonly DataFormat<ObservableDiagramLibraryNode> LibraryNodeFormat = DataFormat.CreateInProcessFormat<ObservableDiagramLibraryNode>("Chordious.DiagramLibraryNode");

        private static readonly DataFormat<string> UseSelectedDiagramsFormat = DataFormat.CreateInProcessFormat<string>("Chordious.UseSelectedDiagrams");

        public static async void DiagramToDragDrop(PointerPressedEventArgs pressedArgs, ObservableDiagram diagram)
        {
            try
            {
                if (pressedArgs is null)
                {
                    throw new ArgumentNullException(nameof(pressedArgs));
                }

                if (diagram is null)
                {
                    throw new ArgumentNullException(nameof(diagram));
                }

                DataTransfer data = new DataTransfer();
                data.Add(CreateImageItem(diagram, 1.0f));

                await DragDrop.DoDragDropAsync(pressedArgs, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        public static async void DiagramLibraryNodeToDragDrop(PointerPressedEventArgs pressedArgs, ObservableDiagramLibraryNode diagramLibraryNode, bool useSelectedDiagrams)
        {
            try
            {
                if (pressedArgs is null)
                {
                    throw new ArgumentNullException(nameof(pressedArgs));
                }

                if (diagramLibraryNode is null)
                {
                    throw new ArgumentNullException(nameof(diagramLibraryNode));
                }

                DataTransfer data = new DataTransfer();

                DataTransferItem item = (useSelectedDiagrams && diagramLibraryNode.SelectedDiagrams.Count > 0) ? CreateImageItem(diagramLibraryNode.SelectedDiagrams[0], 1.0f) : new DataTransferItem();
                item.Set(LibraryNodeFormat, diagramLibraryNode);
                item.Set(UseSelectedDiagramsFormat, useSelectedDiagrams.ToString());
                data.Add(item);

                await DragDrop.DoDragDropAsync(pressedArgs, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        public static void DragDropToDiagramLibraryNode(IDataTransfer data, ObservableDiagramLibraryNode destinationNode, DiagramLibraryNodeDragDropAction dragDropAction)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.TryGetValue(LibraryNodeFormat) is ObservableDiagramLibraryNode sourceNode)
            {
                bool useSelectedDiagrams = bool.TryParse(data.TryGetValue(UseSelectedDiagramsFormat), out bool result) && result;

                if (sourceNode == destinationNode)
                {
                    if (dragDropAction == DiagramLibraryNodeDragDropAction.Copy)
                    {
                        if (useSelectedDiagrams)
                        {
                            // Cloning diagrams within the same collection
                            if (sourceNode.CloneSelected.CanExecute(null))
                            {
                                sourceNode.CloneSelected.Execute(null);
                            }
                        }
                    }
                }
                else
                {
                    if (dragDropAction == DiagramLibraryNodeDragDropAction.Copy)
                    {
                        if (useSelectedDiagrams)
                        {
                            // Copy selected diagrams into the destination
                            if (sourceNode.CopySelected.CanExecute(destinationNode?.Name))
                            {
                                sourceNode.CopySelected.Execute(destinationNode?.Name);
                            }
                        }
                        else
                        {
                            // Copy all diagrams into the destination
                            if (sourceNode.CopyNode.CanExecute(null))
                            {
                                sourceNode.CopyNode.Execute(destinationNode?.Name);
                            }
                        }
                    }
                    else if (dragDropAction == DiagramLibraryNodeDragDropAction.Move)
                    {
                        if (useSelectedDiagrams)
                        {
                            // Move all selected diagrams into the destination
                            if (sourceNode.MoveSelected.CanExecute(destinationNode?.Name))
                            {
                                sourceNode.MoveSelected.Execute(destinationNode?.Name);
                            }
                        }
                        else
                        {
                            // Move all diagrams into the destination (merge)
                            if (sourceNode.MergeNode.CanExecute(null))
                            {
                                sourceNode.MergeNode.Execute(destinationNode?.Name);
                            }
                        }
                    }
                }
            }
        }

        public static DiagramLibraryNodeDragDropAction GetDropAction(KeyModifiers modifiers)
        {
            // Ctrl on Windows/Linux, Option on macOS
            if (modifiers.HasFlag(KeyModifiers.Control) || modifiers.HasFlag(KeyModifiers.Alt))
            {
                return DiagramLibraryNodeDragDropAction.Copy;
            }

            return DiagramLibraryNodeDragDropAction.Move;
        }

        #endregion

        public static void EnableAutoSelectOnFirstLoad(TextBox textBox)
        {
            textBox.TextChanged += AutoSelectOnFirstLoad_TextChanged;
        }

        private static void AutoSelectOnFirstLoad_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!textBox.IsReadOnly)
                {
                    textBox.SelectAll();
                }
                textBox.TextChanged -= AutoSelectOnFirstLoad_TextChanged;
            }
        }

        #region Settings

        public static bool GetEnhancedCopy()
        {
            if (bool.TryParse(AppVM.GetSetting("integration.enhancedcopy"), out bool result))
            {
                return result;
            }

            return false;
        }

        #endregion

        #region Licenses

        public static IEnumerable<ObservableLicense> GetThirdPartyLicenses()
        {
            yield return new ObservableLicense("Avalonia", "Copyright © .NET Foundation and Contributors", "MIT License", MitLicense);
            yield return new ObservableLicense("Svg.Skia", "Copyright © Wiesław Šoltés", "MIT License", MitLicense);
            yield return new ObservableLicense("SkiaSharp", "Copyright © .NET Foundation and Contributors", "MIT License", MitLicense);
        }

        private static readonly string MitLicense = string.Join(Environment.NewLine + Environment.NewLine, new[] {
            @"Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:",
            @"The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.",
            @"THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.",
        });

        #endregion
    }

    public enum DiagramLibraryNodeDragDropAction
    {
        None,
        Copy,
        Move,
    }
}
