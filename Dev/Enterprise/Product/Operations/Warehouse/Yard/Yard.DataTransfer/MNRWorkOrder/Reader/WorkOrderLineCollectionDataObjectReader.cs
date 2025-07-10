using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Yard.Business;
using MNRWorkOrderLine = Enterprise.Warehouse.Yard.Business.MNRWorkOrderLine;
using UniversalMNRWorkOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.MNRWorkOrderLine;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class WorkOrderLineCollectionDataObjectReader : DataObjectCollectionReader<UniversalMNRWorkOrderLine, MNRWorkOrderLine>
	{
		public WorkOrderLineCollectionDataObjectReader(DataObjectList<UniversalMNRWorkOrderLine> workOrderLines, MNRWorkOrderHeader workOrderHeader, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(workOrderLines)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.workOrderHeader = Argument.NotNull(workOrderHeader, nameof(workOrderHeader));
		}

		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly MNRWorkOrderHeader workOrderHeader;

		protected override MNRWorkOrderLine[] BusinessObjects => workOrderHeader.WorkOrderLines.Cast<MNRWorkOrderLine>().ToArray();

		protected override void AddToCollection(MNRWorkOrderLine workOrderLine)
		{
			if (workOrderLine.WorkOrderHeader == null)
			{
				workOrderHeader.WorkOrderLines.Add(workOrderLine);
			}
		}

		protected override void RemoveFromCollection(MNRWorkOrderLine workOrderLine) => workOrderHeader.WorkOrderLines.Delete(workOrderLine);

		protected override MNRWorkOrderLine FindMatchingBusinessObject(UniversalMNRWorkOrderLine dataObject)
		{
			return null;
		}

		protected override MNRWorkOrderLine ReadIntoBusinessObject(UniversalMNRWorkOrderLine dataObject, MNRWorkOrderLine businessObject)
		{
			return new WorkOrderLineDataObjectReader(dataObject, workOrderHeader, logger, factory).ReadIntoBusinessObject();
		}
	}
}
