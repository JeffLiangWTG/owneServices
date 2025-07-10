using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateCommodityDefaultingRuleValidation : AutoOrgRateCommodityDefaultingRuleValidation
	{
		public OrgRateCommodityDefaultingRuleValidation(AutoOrgRateCommodityDefaultingRule parent) : base(parent)
		{
		}

		protected override void CheckORC_ContainerMode()
		{
			base.CheckORC_ContainerMode();

			ListValidation.ErrorIfInvalidCode(Parent.ORC_ContainerModeInfo);
		}

		protected override void CheckORC_Direction()
		{
			base.CheckORC_Direction();

			ListValidation.ErrorIfInvalidCode(Parent.ORC_DirectionInfo);
		}

		protected override void CheckORC_TransportMode()
		{
			base.CheckORC_TransportMode();

			ListValidation.ErrorIfInvalidCode(Parent.ORC_TransportModeInfo);
		}

		protected override void CheckORC_RS_NKServiceLevel()
		{
			base.CheckORC_RS_NKServiceLevel();

			ListValidation.ErrorIfInvalidCode(Parent.ORC_RS_NKServiceLevelInfo);
		}

		protected override void CheckORC_RH_NKCommodityCode()
		{
			base.CheckORC_RH_NKCommodityCode();

			MandatoryValidation.CheckEntered(Parent.ORC_RH_NKCommodityCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ORC_RH_NKCommodityCodeInfo);
		}

		protected override void CheckORC_Destination()
		{
			base.CheckORC_Destination();

			ListValidation.ErrorIfInvalidCode(Parent.ORC_DestinationInfo);
		}

		protected override void CheckORC_Origin()
		{
			base.CheckORC_Origin();

			ListValidation.ErrorIfInvalidCode(Parent.ORC_OriginInfo);
		}

		protected override void CheckORC_OH()
		{
			base.CheckORC_OH();

			MandatoryValidation.CheckEntered(Parent.ORC_OHInfo);
		}
	}
}
