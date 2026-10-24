// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia.Controls;

using Chordious.Core.ViewModel;

namespace Chordious.Avalonia
{
    public partial class ScaleFinderWindow : ChordiousWindow
    {
        public ScaleFinderViewModel VM
        {
            get
            {
                return DataContext as ScaleFinderViewModel;
            }
            private set
            {
                DataContext = value;
            }
        }

        public ScaleFinderWindow() : this(null) { } // For the designer

        public ScaleFinderWindow(ScaleFinderViewModel vm)
        {
            VM = vm;

            InitializeComponent();

            // Keep the view model's selection in sync with the list
            ResultsListView.SelectionChanged += ResultsListView_SelectionChanged;

            _ = new DiagramListInteraction(ResultsListView, () => VM.SelectedResults, (pressedArgs, image, od) => IntegrationUtils.DiagramToDragDrop(pressedArgs, od));

            Closing += ScaleFinderWindow_Closing;
        }

        private void ScaleFinderWindow_Closing(object sender, WindowClosingEventArgs e)
        {
            VM.CancelSearch.Execute(null);
            e.Cancel = false;
        }

        private void ResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (object item in e.AddedItems)
            {
                ObservableDiagram od = item as ObservableDiagram;
                VM.SelectedResults.Add(od);
            }

            foreach (object item in e.RemovedItems)
            {
                ObservableDiagram od = item as ObservableDiagram;
                VM.SelectedResults.Remove(od);
            }
        }
    }
}
