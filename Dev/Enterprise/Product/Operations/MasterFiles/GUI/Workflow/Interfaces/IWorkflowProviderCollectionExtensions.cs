using System;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	public static class IWorkflowItemsControlExtensions
	{
		public static IDisposable SetupSortOnRebuild(this IWorkflowItemsControl control, bool saveOrder)
		{
			IDisposable collectionRebuildHandlerDispose = null;

			void SortAndAttachCollectionRebuiltEventHandler()
			{
				collectionRebuildHandlerDispose?.Dispose();

				if (control.TasksGrid?.List is IWorkflowProviderCollection collection)
				{
					Sort(control, saveOrder);
					var collectionRebuiltEventHandler = new EventHandler((s2, e2) => Sort(control, saveOrder));
					collection.OnRebuild += collectionRebuiltEventHandler;
					collectionRebuildHandlerDispose = new DisposableAction(() => collection.OnRebuild -= collectionRebuiltEventHandler);
				}
			}

			SortAndAttachCollectionRebuiltEventHandler();

			var bindingContextChangedhandler = new EventHandler<ZGrid.ListManagerListChangedEventArgs>((s, e) => SortAndAttachCollectionRebuiltEventHandler());
			control.TasksGrid.ListManagerListChanged += bindingContextChangedhandler;
			return new DisposableAction(() =>
			{
				collectionRebuildHandlerDispose?.Dispose();
				control.TasksGrid.ListManagerListChanged -= bindingContextChangedhandler;
			});
		}

		static void Sort(IWorkflowItemsControl control, bool saveOrder)
		{
			if (control.TasksGrid?.List is IWorkflowProviderCollection collection)
			{
				if (saveOrder && collection.IsSorted)
				{
					var sortProp = control.TasksGrid.List.SortProperty;
					var sortDir = control.TasksGrid.List.SortDirection;
					collection?.Sort(collection.GetDefaultOrderComparer());
					control.TasksGrid.List.ApplySort(sortProp, sortDir);
				}
				else
				{
					collection?.Sort(collection.GetDefaultOrderComparer());
				}
			}
		}
	}
}
