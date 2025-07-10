using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.PortHubs.Business
{
	public class PortHubSelectionValidation : AutoPortHubSelectionValidation
	{
		public PortHubSelectionValidation(AutoPortHubSelection parent) : base(parent)
		{
		}

		string DepotCarrierBookingAgentErrorMessage
		{
			get
			{
				return Res.GetString("FF7D1F3D-EA70-4957-9F72-88DB2FFAC766", "Destination Depot or Carrier Booking Agent");
			}
		}

		protected override void CheckTY_Direction()
		{
			base.CheckTY_Direction();

			MandatoryValidation.CheckEntered(Parent.TY_DirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TY_DirectionInfo, Parent.Lookups.DirectionList);
		}

		protected override void CheckTY_UndgClass()
		{
			base.CheckTY_UndgClass();

			ListValidation.ErrorIfInvalidCode(Parent.TY_UndgClassInfo, Parent.Lookups.DGClassList);
		}

		protected override void CheckTY_RS_NKServiceLevel()
		{
			base.CheckTY_RS_NKServiceLevel();

			ListValidation.ErrorIfInvalidCode(Parent.TY_RS_NKServiceLevelInfo, Parent.Lookups.ServiceLevels);
		}

		protected override void CheckTY_RatingFreightMode()
		{
			base.CheckTY_RatingFreightMode();

			MandatoryValidation.CheckEntered(Parent.TY_RatingFreightModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TY_RatingFreightModeInfo, Parent.Lookups.FreightModeList);
		}

		protected override void CheckTY_PackMode()
		{
			base.CheckTY_PackMode();
			ListValidation.ErrorIfInvalidCode(Parent.TY_PackModeInfo, Parent.Lookups.PackModeList);
		}

		protected override void CheckTY_OA_DispatchDepotAddress()
		{
			base.CheckTY_OA_DispatchDepotAddress();

			CheckIsDepot(Parent.DispatchDepot, Parent.TY_OA_DispatchDepotAddressInfo);
		}

		protected override void CheckTY_OA_DepotAddress()
		{
			base.CheckTY_OA_DepotAddress();

			if (Parent.TY_OH_CarrierBookingAgent.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TY_OA_DepotAddressInfo, DepotCarrierBookingAgentErrorMessage);
			}

			CheckIsDepot(Parent.Depot, Parent.TY_OA_DepotAddressInfo);
		}

		void CheckIsDepot(OrgHeader depot, ZPropertyInfo propertyInfo)
		{
			if (depot != null && !depot.IsDepot)
			{
				propertyInfo.AddError(Res.GetString("300e7c96-1dc9-486d-aab8-f6926fc83160", "The organization you select is not a valid Depot."));
			}
		}

		protected override void CheckTY_F3_NKPackType()
		{
			base.CheckTY_F3_NKPackType();
			ListValidation.ErrorIfInvalidCode(Parent.TY_F3_NKPackTypeInfo, Parent.Lookups.PackTypes);
		}

		public void ValidateDispatchDepotPK()
		{
			ValidateCalculatedProperty(Parent.DispatchDepotPKInfo);
		}

		public void ValidateDepotPK()
		{
			ValidateCalculatedProperty(Parent.DepotPKInfo);
		}

		protected virtual void CheckDispatchDepotPK()
		{
			TypeValidation.CheckValidGuid(Parent.DispatchDepotPKInfo);
		}

		protected virtual void CheckDepotPK()
		{
			if (Parent.TY_OH_CarrierBookingAgent.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.DepotPKInfo, DepotCarrierBookingAgentErrorMessage);
			}

			TypeValidation.CheckValidGuid(Parent.DepotPKInfo);
		}

		protected override void CheckTY_RL_NKOriginPort()
		{
			base.CheckTY_RL_NKOriginPort();

			if (!string.IsNullOrEmpty(Parent.TY_RL_NKOriginPort))
			{
				ListValidation.ErrorIfInvalidCode(Parent.TY_RL_NKOriginPortInfo);
			}
		}

		protected override void CheckTY_RL_NKDestinationPort()
		{
			base.CheckTY_RL_NKDestinationPort();

			if (!string.IsNullOrEmpty(Parent.TY_RL_NKDestinationPort))
			{
				ListValidation.ErrorIfInvalidCode(Parent.TY_RL_NKDestinationPortInfo);
			}
		}

		protected override void CheckTY_OH_CarrierBookingAgent()
		{
			base.CheckTY_OH_CarrierBookingAgent();

			if (Parent.TY_OA_DepotAddress.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TY_OH_CarrierBookingAgentInfo, DepotCarrierBookingAgentErrorMessage);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDepotPK();
			ValidateDispatchDepotPK();
		}

		protected new PortHubSelection Parent
		{
			get { return (PortHubSelection)base.Parent; }
		}
	}
}
