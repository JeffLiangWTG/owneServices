using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CFSReceiveDataObjectReader : ShipmentDataObjectReader<CFSReceive>
	{
		public CFSReceiveDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.CFSReceive;

		protected override IMatchingBusinessEntityFinder<CFSReceive> GetCombinedReferenceMatcher() => null;

		protected override CFSReceive GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override void PopulateBusinessObject(CFSReceive targetBO)
		{
			var booking = targetBO;
			var builder = new ZStringBuilder();
			if (booking.JSB_LoadMode != SupplierBookingLoadModeList.Codes.CFS)
			{
				builder.AppendLine(Res.GetString("1b3f9d44-23d8-4c5d-a77a-f89197ba560d", "Supplier Booking must have load mode = CFS."));
			}
			if (booking.CFSAddress == null)
			{
				builder.AppendLine(Res.GetString("5e53e008-d06f-47c6-82f1-2c8fce37abe8", "Supplier Booking must have CFS Address."));
			}
			if (booking.JSB_Status != SupplierBookingStatusList.Codes.APP)
			{
				builder.AppendLine(Res.GetString("15a5e100-52f3-4fe0-a4d1-ace9a6da2958", "CFS receipt can only be performed on Approved Supplier Booking."));
			}

			if (!builder.IsEmpty)
			{
				throw new DataObjectReadFailureException(builder.ToString());
			}

			new CFSReceiveLineDataObjectCollectionReader(dataObject.PackingLineCollection, booking, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
		}
	}
}
