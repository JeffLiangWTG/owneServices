using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	internal static class DeduplicationFilterControlUtils
	{
		internal static void CreateColumnStyle(this ZFilterStripControl zFilterStripControl, ResourceStringData captionData, string columnName, int width, bool mandatory = false, bool visible = true)
		{
			var columnStyle = new ZTextBoxColumnStyleInfo(columnName, width);
			columnStyle.IsVisible = visible;
			columnStyle.IsMandatory = mandatory;
			columnStyle.CaptionResourceString = captionData;
			zFilterStripControl.Grid.ColumnStyles.Add(columnStyle);
		}

		internal static void PerformSearch(ZFilterGrid grid, BusinessObjectCollection collection, ZQuery filterQuery, int maxNumberOfRecordsToShowInDisplayGrids, int maximumNumberForMDMDuplicatesGrid, Action<ZString, int, bool> updateNumberLoadedMessage)
		{
			var gridCollection = (IBusinessObjectCollection)collection;

			try
			{
				grid.SuspendLayout();

				try
				{
					using (gridCollection.SuspendListChanged())
					{
						collection.SwapFactoryAndRemoveAll(new BusinessObjectFactory());
						LoadCollection(collection, gridCollection, filterQuery, maxNumberOfRecordsToShowInDisplayGrids, maximumNumberForMDMDuplicatesGrid, updateNumberLoadedMessage);
					}
				}
				catch (SqlException ex) when (ex.Number == 258)
				{
					ExceptionReporter.Instance.ReportException("", ex);
				}
			}
			finally
			{
				grid.ForcePreFetch();
				grid.ResumeLayout();
			}
		}

		#region Implementation

		static void LoadCollection(BusinessObjectCollection collection, IBusinessObjectCollection gridCollection, ZQuery filterQuery, int maxNumberOfRecordsToShowInDisplayGrids, int maximumNumberForMDMDuplicatesGrid, Action<ZString, int, bool> updateNumberLoadedMessage)
		{
			filterQuery.MaximumRows = maxNumberOfRecordsToShowInDisplayGrids;
			PropertyDescriptor sortProperty = null;
			var sortDirection = ListSortDirection.Ascending;
			if (gridCollection.IsSorted && gridCollection.SortProperty is ZCustomPropertyDescriptor)
			{
				sortProperty = gridCollection.SortProperty;
				sortDirection = gridCollection.SortDirection;
				gridCollection.RemoveSort();
			}

			collection.Load(filterQuery);
			var showCount = collection.Count;
			var message = "";

			if (collection.Count > 0)
			{
				if (collection.Count == maxNumberOfRecordsToShowInDisplayGrids)
				{
					RemoveExtraItems(collection, gridCollection, maximumNumberForMDMDuplicatesGrid);
					message = Res.GetString("97590366-28DC-4A00-9FE9-D8A30636D850",
						"Found at least {0} records.\r\nOnly showing the top {1} records.",
						showCount, collection.Count);
				}
				else if (collection.Count > maximumNumberForMDMDuplicatesGrid)
				{
					RemoveExtraItems(collection, gridCollection, maximumNumberForMDMDuplicatesGrid);
					message = Res.GetString("BDE1CEB3-76F7-4682-A794-C760C518B65F",
						"Found {0} records.\r\nOnly showing the top {1} records.",
						showCount, collection.Count);
				}
			}
			else
			{
				message = Res.GetString("9CB47941-807A-424F-877A-CED32C8C3C75", "Found no records.");
			}

			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.ShowInformation(message);
			}

			updateNumberLoadedMessage(null, showCount, false);

			if (sortProperty != null)
			{
				gridCollection.ApplySort(sortProperty, sortDirection);
			}
		}

		static void RemoveExtraItems(BusinessObjectCollection collection, IBusinessObjectCollection gridCollection, int maximumNumberForMDMDuplicatesGrid)
		{
			var extraItemsCount = collection.Count - maximumNumberForMDMDuplicatesGrid;
			if (extraItemsCount > 0)
			{
				lock (gridCollection.SyncRoot)
				{
					for (int index = 0, tail = collection.Count - 1; index < extraItemsCount; index++, tail--)
					{
						gridCollection.RemoveAt(tail);
					}
				}
			}
		}

		#endregion
	}
}
