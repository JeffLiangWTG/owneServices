//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradeProspectValidation
//
//    This class should be used for overriding validation in AutoOrgTradeProspectValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeProspectValidation : AutoOrgTradeProspectValidation
	{
		public OrgTradeProspectValidation(AutoOrgTradeProspect parent) : base(parent)
		{
		}

		new OrgTradeProspect Parent
		{
			get { return (OrgTradeProspect)base.Parent; }
		}

		public override void ValidateAll()
		{
			if (!Parent.TradeDetail.ReadOnly) // Header.Clients is a readonly collection and should not be validated
			{
				base.ValidateAll();
			}
		}

		#region PAP_RC_NKContainer

		protected override void CheckPAP_RC_NKContainer()
		{
			base.CheckPAP_RC_NKContainer();
			ListValidation.ErrorIfInvalidCode(Parent.PAP_RC_NKContainerInfo);
		}

		#endregion

		#region PAP_RecurrenceType

		protected override void CheckPAP_RecurrenceType()
		{
			base.CheckPAP_RecurrenceType();
			MandatoryValidation.CheckEntered(Parent.PAP_RecurrenceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PAP_RecurrenceTypeInfo);
		}

		#endregion

		#region PAP_IncoTradeTerm

		protected override void CheckPAP_IncoTradeTerm()
		{
			base.CheckPAP_IncoTradeTerm();
			ListValidation.ErrorIfInvalidCode(Parent.PAP_IncoTradeTermInfo);
			IncotermValidation.Instance.WarningIfExpired(Parent.PAP_IncoTradeTermInfo);
		}

		#endregion

		#region PAP_RH_NKCommodityCode

		protected override void CheckPAP_RH_NKCommodityCode()
		{
			base.CheckPAP_RH_NKCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.PAP_RH_NKCommodityCodeInfo);
		}

		#endregion

		#region PAP_RS_NKServiceLevel

		protected override void CheckPAP_RS_NKServiceLevel()
		{
			base.CheckPAP_RS_NKServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.PAP_RS_NKServiceLevelInfo);
		}

		#endregion

		#region PAP_Density

		protected override void CheckPAP_Density()
		{
			base.CheckPAP_Density();
			ListValidation.ErrorIfInvalidCode(Parent.PAP_DensityInfo);
		}

		#endregion

		#region PAP_OH_Competitor

		protected override void CheckPAP_OH_Competitor()
		{
			base.CheckPAP_OH_Competitor();
			ListValidation.ErrorIfInvalidPK(Parent.PAP_OH_CompetitorInfo);
		}

		#endregion

		#region PAP_OH_ControllingAgent

		protected override void CheckPAP_OH_ControllingAgent()
		{
			base.CheckPAP_OH_ControllingAgent();
			ListValidation.ErrorIfInvalidPK(Parent.PAP_OH_ControllingAgentInfo);
		}

		#endregion

		#region PAP_OH_ServiceProvider

		protected override void CheckPAP_OH_ServiceProvider()
		{
			base.CheckPAP_OH_ServiceProvider();
			ListValidation.ErrorIfInvalidPK(Parent.PAP_OH_ServiceProviderInfo);
		}

		#endregion

		#region PAP_PeriodOfActivity

		protected override void CheckPAP_PeriodOfActivity()
		{
			base.CheckPAP_PeriodOfActivity();
			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.PAP_PeriodOfActivityInfo, Parent.Lookups.PeriodOfActivityTypes, Parent.Lookups.ActivePeriodOfActivityTypes);

			if (Parent.IsPeriodOfActivityOverridden)
			{
				MandatoryValidation.CheckEntered(Parent.PAP_PeriodOfActivityInfo);
			}
		}

		#endregion

		#region PAP_IndustryVertical

		protected override void CheckPAP_IndustryVertical()
		{
			base.CheckPAP_IndustryVertical();
			ListValidation.ErrorIfInvalidCodeButJustWarnIfExistingInactiveCode(Parent.PAP_IndustryVerticalInfo, Parent.Lookups.IndustryVerticalTypes, Parent.Lookups.ActiveIndustryVerticalTypes);

			if (Parent.IsIndustryVerticalOverridden)
			{
				MandatoryValidation.CheckEntered(Parent.PAP_IndustryVerticalInfo);
			}
		}

		#endregion

		protected override void CheckPAP_ExpectedTradeStartDate()
		{
			base.CheckPAP_ExpectedTradeStartDate();
			if (Parent.TradeDetail.IsCommitted)
			{
				MandatoryValidation.CheckEntered(Parent.PAP_ExpectedTradeStartDateInfo);
			}
		}
	}
}
