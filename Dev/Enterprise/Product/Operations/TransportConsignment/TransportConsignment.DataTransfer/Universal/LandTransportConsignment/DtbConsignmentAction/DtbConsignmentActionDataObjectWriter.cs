using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentActionDataObjectWriter : DataObjectWriter<DtbConsignmentAction, Confirmation>
	{
		readonly Dictionary<ZGuid, ZInt> packageLinksDictionary;

		internal DtbConsignmentActionDataObjectWriter(IDataWritingManager manager, Dictionary<ZGuid, ZInt> packageLinksDictionary) : base(manager)
		{
			this.packageLinksDictionary = packageLinksDictionary;
		}

		protected override Confirmation PopulateDataObject(DtbConsignmentAction consignmentAction)
		{
			var confirmation = new Confirmation();
			PopulateData(consignmentAction, confirmation);

			return confirmation;
		}

		void PopulateData(DtbConsignmentAction consignmentAction, Confirmation confirmation)
		{
			confirmation.ActualDate = consignmentAction.LTA_ActualTime.ToLocalZDateTime();
			confirmation.DateDescription = consignmentAction.LTA_ActionType;
			confirmation.SlotDate = consignmentAction.LTA_Slot.ToLocalZDateTime();
			confirmation.RequiredToDate = consignmentAction.RequiredTo;
			confirmation.ReceivedBy = consignmentAction.LTA_SignedBy;
			confirmation.Reference = consignmentAction.ReferenceNumber;
			confirmation.RequiredFromDate = consignmentAction.RequiredFrom;
			confirmation.EstimatedDate = consignmentAction.Estimated;

			confirmation.SetWriterStrategy(writeManager.WriterStrategy);
			confirmation.SetPackingLinkCollection(() => consignmentAction.PackageDivots.Select(divot => new PackingLink()
			{
				PackingLineLink = packageLinksDictionary[divot.LTP_KP_Package],
				PackedQuantity = new ZDecimal(divot.LTP_PackageQuantity),
				IsContainer = divot.Package.IsContainer,
			}).ToList());
		}
	}
}
