using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public class ContainerBatchSummary : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string RequiredDeliveryDate = "RequiredDeliveryDate";
			public const string ConfirmedDeliveryDate = "ConfirmedDeliveryDate";
			public const string ActualDeliveryDate = "ActualDeliveryDate";
			public const string EstimatedDehireDate = "EstimatedDehireDate";
			public const string EmptyPickup = "EmptyPickup";
			public const string ActualDehireDate = "ActualDehireDate";
		}

		#endregion

		public ContainerBatchSummary()
		{
			CreateSummaryLines();
		}

		public ContainerBatchSummary(TrackingContainerStandaloneCollection containers)
			: base(containers.Factory)
		{
			CreateAndCalculateSummaryLines(containers);
		}

		public ContainerSummaryRowCollection SummaryLines
		{
			get { return summaryLines; }
		}

		ContainerSummaryRowCollection summaryLines;

		Dictionary<ZString, ContainerSummaryRow> CreateSummaryLines()
		{
			ContainerStatusList containerStatuses = new ContainerStatusList();
			Dictionary<ZString, ContainerSummaryRow> indexedRows = new Dictionary<ZString, ContainerSummaryRow>();
			summaryLines = new ContainerSummaryRowCollection(Factory);

			foreach (ICodeDescription codeDesc in containerStatuses)
			{
				if (!indexedRows.ContainsKey(codeDesc.Code))
				{
					ContainerSummaryRow summaryRow = new ContainerSummaryRow(codeDesc);
					indexedRows.Add(codeDesc.Code, summaryRow);
					summaryLines.Add(summaryRow);
				}
			}

			return indexedRows;
		}

		void CreateAndCalculateSummaryLines(TrackingContainerStandaloneCollection containers)
		{
			Dictionary<ZString, ContainerSummaryRow> indexedRows = CreateSummaryLines();

			foreach (TrackingContainer container in containers)
			{
				ContainerSummaryRow row;

				if (indexedRows.TryGetValue(container.Status, out row) && row != null)
				{
					row.Containers++;
					row.Shipments += container.Shipments.Count;
					row.Orders += container.Orders.Count;
					row.Packages += container.Packs;
				}
			}
		}
	}
}
