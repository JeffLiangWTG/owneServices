using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer
{
	public class AgencyShipmentStatusUpdatedLogProcessor : ShipmentStatusUpdatedLogProcessor
	{
		protected override bool IsEventAndParentTableMatchProcessor(Event @event, string logParentTableCode)
		{
			return logParentTableCode != ViewQuotedBookingSchema.Constants.Prefix
				&& base.IsEventAndParentTableMatchProcessor(@event, logParentTableCode);
		}

		protected override bool Enable()
		{
			return AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && base.Enable();
		}

		protected override BusinessObject GetLogParent(IQueuedLog log)
		{
			return log.Factory.Load(JobShipmentSchema.Constants.Prefix, log.SJ_ParentID);
		}

		protected override bool ShouldProcess(BusinessObject logParent)
		{
			return logParent is AgencyShipment agencyShipment && agencyShipment.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == CusEntryNumLookups.HIR);
		}

		protected override ITopLevelDataObjectWriter GetShipmentStatusDataObjectWriterCore(DataWritingManager writeManager, BusinessObject logParent,
			Event @event, string dataContextDocumentName, string rejectionReason, bool shouldPopulateTransportLegCollection)
		{
			if (logParent is AgencyBooking)
			{
				return new AgencyBookingStatusDataObjectWriter(writeManager)
				{
					Event = @event,
					DataContextDocumentName = dataContextDocumentName,
					ReasonForRejectionNoteText = rejectionReason,
				};
			}

			return new BillOfLadingStatusDataObjectWriter(writeManager)
			{
				Event = @event,
				DataContextDocumentName = dataContextDocumentName,
				ReasonForRejectionNoteText = rejectionReason,
			};
		}
	}
}
