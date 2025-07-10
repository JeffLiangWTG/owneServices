using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal class AgencyShipmentInvoicingSupporter : CommonShipmentInvoicingSupporter
	{
		public AgencyShipmentInvoicingSupporter(AgencyShipment parent)
			: base(parent)
		{
		}

		public override OrgHeader Consignor
		{
			get
			{
				var address = GetConsignorPickupAddressForAccounting(Shipment);
				return address != null ? address.Organisation : null;
			}
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			if (AgencyRegistry.Instance.DefaultCreditorFromPrincipal.Value
					&& defaultCreditorSetting.ChargeCode != null
					&& defaultCreditorSetting.ChargeCode.AC_ChargeOtherGroups == ChargeOtherGroupsList.Codes.Principal)
			{
				return Shipment.Principal;
			}

			return null;
		}

		public override ZString HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		public override ZString MasterBillNumber
		{
			get { return Shipment.JS_HouseBill; }
		}

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return true;
		}

		public override void PostedStateChanged()
		{
			Shipment.JS_OH_DeliveryAgentInfo.RefreshBinding();
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return ObjectFactory.Get<IAccounting>().ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(Shipment); }
		}

		public static JobDocAddress GetConsignorPickupAddressForAccounting(AgencyShipment shipment)
		{
			return shipment.BookingPartyDocumentaryAddress.IsValidAddress
				? shipment.BookingPartyDocumentaryAddress
				: shipment.ConsignorDocumentaryAddress;
		}

		protected override JobSailing Sailing
		{
			get { return Shipment.Sailing; }
		}

		public override bool IsDomestic
		{
			get { return Shipment.IsDomestic(); }
		}

		#region Implementation

		public new AgencyShipment Shipment
		{
			get { return (AgencyShipment)base.Shipment; }
		}

		#endregion // Implementation
	}
}


