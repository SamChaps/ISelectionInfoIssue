using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ISelectionInfoIssue
{
    enum SelectionState
    {
        ChangeInitiated,
        ChangingToMultiple,
        ChangingToSingle,
        None
    }

    public sealed partial class GridViewWithISelectionInfoList : UserControl
    {

        private SelectionState selectionState { get; set; }
        private int currentSelectedIndex {get;set;}

        private int lastSelectedIndex { get; set; }

        private List<ItemIndexRange> savedSelectedRanges { get; set; }

        public ListWithISelectionInfo TheListWithISelectionInfo { get; set; }

        public GridViewWithISelectionInfoList()
        {
            this.InitializeComponent();

            TheListWithISelectionInfo = new ListWithISelectionInfo { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6" };

            selectionState = SelectionState.None;
            currentSelectedIndex = -1;
            lastSelectedIndex = -1;
            savedSelectedRanges = new List<ItemIndexRange>();

            Loaded += GridViewWithISelectionInfoList_Loaded;
        }

        private void GridViewWithISelectionInfoList_Loaded(object sender, RoutedEventArgs e)
        {
            TheGridView.SelectedIndex = 0;
        }

        private void TheGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRanges = TheGridView.SelectedRanges;

            int index = selectedRanges.Count > 0 ? selectedRanges[0].FirstIndex : -1;
            // Keep track of the current selected index and the previous one for selection mode toggling/resetting
            if (index >= 0 && index != currentSelectedIndex)
            {
                lastSelectedIndex = currentSelectedIndex;
                currentSelectedIndex = index;
            }

            // Get the total number of selected items
            uint numberOfSelectedItems = 0;
            foreach (var range in selectedRanges)
            {
                numberOfSelectedItems += range.Length;
            }

            switch (selectionState)
            {
                case SelectionState.ChangeInitiated:
                    {
                        bool isSingleSelection = TheGridView.SelectionMode == ListViewSelectionMode.Single;
                        selectionState = isSingleSelection ? SelectionState.ChangingToSingle : SelectionState.ChangingToMultiple;
                        break;
                    }
                case SelectionState.ChangingToMultiple:
                    {
                        // Only set the selection state to None when the selection mode change has been completed
                        if (numberOfSelectedItems > 1)
                        {
                            selectionState = SelectionState.None;
                        }
                        break;
                    }
                case SelectionState.ChangingToSingle:
                    {
                        // Only set the selection state to None when the selection mode change has been completed
                        if (numberOfSelectedItems == 1)
                        {
                            selectionState = SelectionState.None;
                        }
                        break;
                    }
                case SelectionState.None:
                    {
                        if (numberOfSelectedItems < 2)
                        {
                            if (TheGridView.SelectionMode != ListViewSelectionMode.Single)
                            {
                                selectionState = SelectionState.ChangeInitiated;
                                SaveSelectedRanges();
                                TheGridView.SelectionMode = ListViewSelectionMode.Single;
                                SelectSavedRanges(true);
                            }
                        }
                        break;
                    }

                default:
                    break;
            }
        }

        private void ItemCheckbox_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Do not propagate the event to the GridViewItem
            e.Handled = true;
        }

        private void ItemCheckbox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var checkbox = sender as Border;

            selectionState = SelectionState.ChangeInitiated;
            SaveSelectedRanges();
            TheGridView.SelectionMode = ListViewSelectionMode.Multiple;
            SelectSavedRanges(false);

            var grid = VisualTreeHelper.GetParent(checkbox) as Grid;
            var checkedItem = grid.DataContext as string;


            int index = TheListWithISelectionInfo.IndexOf(checkedItem);
            TheGridView.SelectRange(new ItemIndexRange(index, 1));

            checkbox.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void SaveSelectedRanges()
        {
            var selectedRanges = TheGridView.SelectedRanges;
            savedSelectedRanges.Clear();
            foreach (var range in selectedRanges)
            {
                savedSelectedRanges.Add(new ItemIndexRange(range.FirstIndex, range.Length));
            }
        }

        private void SelectSavedRanges(bool isSingleMode)
        {
            // Reselect previous selection due to selection reset when toggling selection mode
            foreach (var range in savedSelectedRanges)
            {
                if (isSingleMode)
                {
                    TheGridView.SelectedIndex = range.FirstIndex;
                }
                else
                {
                    TheGridView.SelectRange(range);
                }
            }
        }

        private void FilmstripItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var checkbox = grid.FindName("ItemCheckbox") as Border;
            var content = grid.DataContext as string;

            if (TheGridView.SelectionMode == ListViewSelectionMode.Single && content != (TheGridView.SelectedItem as string))
            {
                checkbox.Visibility = Visibility.Visible;
            }
            else
            {
                checkbox.Visibility = Visibility.Collapsed;
            }
        }

        private void FilmstripItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var checkbox = grid.FindName("ItemCheckbox") as Border;

            var hoveredItem = grid.DataContext as string;
            var selectedItem = TheGridView.SelectedItem as string;

            if (hoveredItem != selectedItem && TheGridView.SelectionMode == ListViewSelectionMode.Single)
            {
                checkbox.Visibility = Visibility.Visible;
            }
        }

        private void FilmstripItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var item = sender as Grid;
            var checkbox = item.FindName("ItemCheckbox") as Border;
            checkbox.Visibility = Visibility.Collapsed;
        }
    }
}
