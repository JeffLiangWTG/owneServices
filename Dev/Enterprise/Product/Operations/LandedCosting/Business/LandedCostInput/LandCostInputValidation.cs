//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLandCostInputValidation
//
//    This class should be used for overriding validation in AutoLandCostInputValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LandCostInputValidation : AutoLandCostInputValidation
	{
		public LandCostInputValidation(AutoLandCostInput parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLinkedObjectUniqueCode();
			ValidateLCGroupString();
		}

		#region Validation for calculated properties

		public void ValidateLinkedObjectUniqueCode()
		{
			ValidateCalculatedProperty(CostInput.LinkedObjectUniqueCodeInfo);
		}

		protected virtual void CheckLinkedObjectUniqueCode()
		{
			MandatoryValidation.CheckEntered(CostInput.LinkedObjectUniqueCodeInfo);
			ListValidation.ErrorIfInvalidCode(CostInput.LinkedObjectUniqueCodeInfo, CostInput.Lookups.ParentAssociableList);
			if (CostInput.Parent != null && new List<IUltimateDistributee>(CostInput.Parent.UltimateDistributees).Count == 0)
			{
				CostInput.LinkedObjectUniqueCodeInfo.AddWarning(Res.GetString("cdfa0aff-806e-4e0e-a016-055a608f5cde", "There are no lines associated with this level. Without no lines, this cost will not be distributed anywhere."));
			}
		}

		public void ValidateLCGroupString()
		{
			ValidateCalculatedProperty(CostInput.LCGroupStringInfo);
		}

		protected virtual void CheckLCGroupString()
		{
			if (CostInput.LCGroupString.IsEmpty || CostInput.LCGroupString == "0")
			{
				CostInput.LCGroupStringInfo.AddError(Res.GetString("cbcae18d-4193-4a15-87da-464d76e3e61d", "You have to enter a LC Group."));
			}
			ListValidation.ErrorIfInvalidCode(CostInput.LCGroupStringInfo, CostInput.Lookups.LandCostGroupList);

			if (Parent.Lookups.LandCostGroupList.Count == 0)
			{
				OrgHeader proxyOrganisation = (OrgHeader)Parent.Factory.Load(typeof(OrgHeader), GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				CostInput.LCGroupStringInfo.AddWarning(Res.GetString("e3decd6b-4a43-4e77-a0d7-4780cdb53876", @"There is no setting for Landed Costing Preferences for the current company and the importer.
You can define the default setting by selecting an organization coded {0} -> Consignee -> Landed Costing Preference.Or if you want a client-specific setting, please press F3 in Importer field and go to the organization -> Consignee -> Landed Costing Preference.", proxyOrganisation.OH_Code));
			}
		}

		#endregion

		#region Overrides

		protected LandCostInput CostInput
		{
			get { return (LandCostInput)base.Parent; }
		}

		protected override void CheckLI_DistributeCostBy()
		{
			base.CheckLI_DistributeCostBy();
			if (CostInput.Parent != null && !(CostInput.Parent is IUltimateDistributee))
			{
				MandatoryValidation.CheckEntered(Parent.LI_DistributeCostByInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.LI_DistributeCostByInfo, Parent.Lookups.DistributeCostBy);
		}

		protected override void CheckLI_AC_ChargeCode()
		{
			base.CheckLI_AC_ChargeCode();
			ListValidation.ErrorIfInvalidPK(Parent.LI_AC_ChargeCodeInfo, Parent.Lookups.ChargeCodes);
		}

		protected override void CheckLI_CostAmount()
		{
			base.CheckLI_CostAmount();
			if (Parent.LI_CostAmount == 0)
			{
				Parent.LI_CostAmountInfo.AddError(Res.GetString("85778517-0502-4ec4-a7ea-8a6aea744cc1", "Amount should be other than Zero. If this cost adds to Total Cost, please enter as a positive amount. If this cost reduces Total Cost, enter as a negative amount."));
			}
		}

		protected override void CheckLI_RX_NKCostCurrency()
		{
			base.CheckLI_RX_NKCostCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.LI_RX_NKCostCurrencyInfo, Parent.Lookups.CostCurrencies);
			ValidateLI_ServiceExRate();
		}

		protected override void CheckLI_ServiceExRate()
		{
			base.CheckLI_ServiceExRate();
			CompareValidation.CheckNumberGreaterThanZero(Parent.LI_ServiceExRateInfo);

			if (Parent.LI_RX_NKCostCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && Parent.LI_ServiceExRate != 1)
			{
				Parent.LI_ServiceExRateInfo.AddError(Res.GetString("bac10449-1ea2-41b0-b69a-5010615abd6f", "Cost is in the local currency and Ex rate should be 1."));
			}
		}

		#endregion
	}
}
