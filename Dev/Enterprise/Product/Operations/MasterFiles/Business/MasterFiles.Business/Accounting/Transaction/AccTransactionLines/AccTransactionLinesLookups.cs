//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTransactionLinesLookups
//
//    This class should be used for overriding collections in AutoAccTransactionLinesLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTransactionLinesLookups : AutoAccTransactionLinesLookups
	{
		public AccTransactionLinesLookups(AutoAccTransactionLines parent)
			: base(parent)
		{
		}

		#region TaxRates

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public override AccTaxRateCollection TaxRates
		{
			get { return Factory.GetCachedValue("Accounting" + GlbCompany.CurrentCompany.PK.ToStringKey() + nameof(AccTaxRateCollection) + "Active", () => new VATAccTaxRateCollection(Factory, new ZQuery(AccTaxRateSchema.AT_IsActive, true))); }
		}

		#endregion

		public override GlbBranchCollection Branches => AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory);

		public override GlbBranchCollection TaxBranches => AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory);

		public override AccInvMsgCollection VATClasses
		{
			get { return new AccInvMsgCollection(Factory, Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty); }
		}

		public AccGLHeaderCollection BSHGLHeaders
		{
			get
			{
				var glAccounts = new AccGLHeaderCollection(Factory);
				glAccounts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Account Type", "Property", (ZString)Core.Constants.AccountType.BalanceSheetAccount));
				return glAccounts;
			}
		}

		#region PlacesOfSupply

		public ReadOnlyCodeDescriptionPairList PlacesOfSupply => PlaceOfSupplyListProvider.GetPlaceOfSupplyList(Parent.Company);

		public ReadOnlyCodeDescriptionPairList PlaceOfSupplyTypes => PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(Parent.Company);

		#endregion

		public ReadOnlyCodeDescriptionPairList SupplyTypes => AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.GetValueWithoutFallback(Parent.Company?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();

		new AccTransactionLines Parent
		{
			get { return (AccTransactionLines)base.Parent; }
		}
	}
}
