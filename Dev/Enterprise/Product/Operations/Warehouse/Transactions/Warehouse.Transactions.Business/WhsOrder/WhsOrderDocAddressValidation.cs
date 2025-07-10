using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderDocAddressValidation : WhsDocketDocAddressValidation
	{
		public WhsOrderDocAddressValidation(JobDocAddress addressToValidate)
			: base(addressToValidate)
		{
		}

		#region Validation

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			if (!Parent.OrganisationPKInfo.HasErrors())
			{
				switch (Parent.E2_AddressType)
				{
					case DocAddressTypes.Codes.ConsigneeAddress:
						ValidateConsignee();
						break;
					case TransportCoConstants.AddressTypeCode:
						ValidateTransportCo();
						break;
				}
			}
		}

		void ValidateConsignee()
		{
			var order = (WhsOrder)Docket;
			if (order != null && !order.ConsigneeDocAddress.IsValidAddress)
			{
				Parent.OrganisationPKInfo.AddError(ConsigneeNotEntered);
			}
		}

		protected virtual void ValidateTransportCo()
		{
			var order = (WhsOrder)Docket;
			if (order != null && !order.TransportCoDocAddress.IsValidAddress && !Globals.IsWeb)
			{
				Parent.OrganisationPKInfo.AddWarning(TransportCompanyNotEntered);
			}
			if (order.WD_WP.IsValid && order.TranportCoPKHasChanges && order.IsInDatabase)
			{
				var orderStatus = order.WarehouseOrderStatus;
				if (orderStatus != WhsOrderStatus.Codes.ReadyToPack && orderStatus != WhsOrderStatus.Codes.Departed && !order.IsOrderAssignedToLoad && order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU())
				{
					Parent.OrganisationPKInfo.AddError(TransportCompanyCanNotBeChangedIfAnyPackagesInConsolidationLocationOrOnConsolidationHU);
				}
			}
		}

		#endregion

		#region Implementation

		protected ZString ConsigneeNotEntered => Res.GetString("b4887e38-c168-4861-946f-24e8a8faaac0", "The Consignee has no address entered");

		protected ZString TransportCompanyNotEntered => Res.GetString("97cec964-1dd8-4ba5-9d71-c8c134473ba9", "The Transport Company has no address entered");

		protected ZString TransportCompanyCanNotBeChangedIfAnyPackagesInConsolidationLocationOrOnConsolidationHU => Res.GetString("817ae91b-17a7-474b-9fbd-55b67c1ba1a4", "Cannot change the Transport Company when any packages are in a Packing Consolidation Location or on a Handling Unit.");

		#endregion
	}
}
