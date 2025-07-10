//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradeDetailValidation
//
//    This class should be used for overriding validation in AutoOrgTradeDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeDetailValidation : AutoOrgTradeDetailValidation
	{
		public OrgTradeDetailValidation(AutoOrgTradeDetail parent) : base(parent)
		{
		}

		new OrgTradeDetail Parent
		{
			get { return (OrgTradeDetail)base.Parent; }
		}

		public override void ValidateAll()
		{
			if (!Parent.ReadOnly) // Header.Clients is a readonly collection and should not be validated
			{
				base.ValidateAll();
			}
		}

		#region PA_TradeMode

		protected override void CheckPA_TradeMode()
		{
			base.CheckPA_TradeMode();

			if (!Parent.IsActual && !Parent.PA_TradeModeInfo.ReadOnly)
			{
				if (Parent.SalesProduct != null && Parent.SalesProduct.ModeIsMandatory)
				{
					MandatoryValidation.CheckEntered(Parent.PA_TradeModeInfo);
				}
				ListValidation.ErrorIfInvalidCode(Parent.PA_TradeModeInfo);
			}
		}

		#endregion

		#region PA_TradeType

		protected override void CheckPA_TradeType()
		{
			base.CheckPA_TradeType();

			if (!Parent.IsActual && !Parent.PA_TradeTypeInfo.ReadOnly)
			{
				if (Parent.SalesProduct != null && Parent.SalesProduct.TypeIsMandatory)
				{
					MandatoryValidation.CheckEntered(Parent.PA_TradeTypeInfo);
				}
				ListValidation.ErrorIfInvalidCode(Parent.PA_TradeTypeInfo);
			}
		}

		#endregion

		#region Prospect Period Start / End

		public void ValidateProspectPeriodStart() => ValidateCalculatedProperty(Parent.ProspectPeriodStartInfo);

		protected void CheckProspectPeriodStart()
		{
			if (Parent.ProspectPeriodEndType == OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter)
			{
				MandatoryValidation.CheckEntered(Parent.ProspectPeriodStartInfo);
				if (Parent.ProspectPeriodEnd < Parent.ProspectPeriodStart)
				{
					Parent.ProspectPeriodStartInfo.AddError(Res.GetString("9215763f-176d-44fd-9fff-282722209d46", "The Start Period must be earlier than the End Period"));
				}
			}
		}

		public void ValidateProspectPeriodEnd() => ValidateCalculatedProperty(Parent.ProspectPeriodEndInfo);

		protected void CheckProspectPeriodEnd()
		{
			if (Parent.ProspectPeriodEndType == OrgTradeProspectPeriodEndTypeList.Codes.ManuallyEnter)
			{
				MandatoryValidation.CheckEntered(Parent.ProspectPeriodEndInfo);
				if (Parent.ProspectPeriodEnd < Parent.ProspectPeriodStart)
				{
					Parent.ProspectPeriodEndInfo.AddError(Res.GetString("9f0ec345-ae76-4682-bd54-5aa1d612606e", "The End Period must be later than the Start Period"));
				}
			}
		}

		#endregion
	}
}
