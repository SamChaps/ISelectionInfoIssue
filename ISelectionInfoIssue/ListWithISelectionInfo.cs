using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.UI.Xaml.Data;

namespace ISelectionInfoIssue
{
    public class ListWithISelectionInfo : List<string>, INotifyCollectionChanged, IItemsRangeInfo, ISelectionInfo
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private ItemIndexRangeList selection = new ItemIndexRangeList();

        public void SelectRange(ItemIndexRange range)
        {
            selection.Add(range);
        }

        public void DeselectRange(ItemIndexRange range)
        {
            selection.Subtract(range);
        }

        public bool IsSelected(int index)
        {
            foreach (ItemIndexRange range in selection)
            {
                if (index >= range.FirstIndex && index <= range.LastIndex) return true;
            }
            return false;
        }

        public IReadOnlyList<ItemIndexRange> GetSelectedRanges()
        {
            return selection.ToList();
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
        }

        public void Dispose()
        {
        }
    }
}
