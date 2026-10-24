// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Chordious.Avalonia
{
    /// <summary>
    /// Interaction logic for TextPromptWindow.xaml
    /// </summary>
    public partial class TextPromptWindow : ChordiousWindow
    {
        public TextPromptWindow()
        {
            InitializeComponent();
            IntegrationUtils.EnableAutoSelectOnFirstLoad(InputTextBox);
        }
    }
}
