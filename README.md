# WinUI Bug
## GridView SelectionMode toggling with bound List implementing ISelectionInfo

It seems that when the List bound to the GridView implements ISelectionInfo, selecting within the SelectionChanged handler of the GridView does not work anymore.
