//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccChargeGLPostingOverrideLookups
//
//    This class should be used for overriding collections in AutoAccChargeGLPostingOverrideLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeGLPostingOverrideLookups : AutoAccChargeGLPostingOverrideLookups
	{
		public AccChargeGLPostingOverrideLookups(AutoAccChargeGLPostingOverride parent)
			: base(parent)
		{
		}

		new AccChargeGLPostingOverride Parent => base.Parent as AccChargeGLPostingOverride;

		#region Accounts

		#region Clearing Accounts

		public virtual AccGLHeaderCollection GLRevenueClearingAccountCollection => ChargeCodeLookups.GLRevenueClearingAccounts;

		public virtual AccGLHeaderCollection GLCostClearingAccountCollection => ChargeCodeLookups.GLCostClearingAccounts;

		#endregion

		#region GL Revenue Accounts

		public AccGLHeaderCollection GLRevenueAccountCollection
		{
			get { return ChargeCodeLookups.GLRevenueAccountCollection; }
		}

		#endregion

		#region GL WIP Accounts

		public AccGLHeaderCollection GLWIPAccountCollection
		{
			get { return ChargeCodeLookups.GLWIPAccountCollection; }
		}

		#endregion

		#region GL Cost Accounts

		public AccGLHeaderCollection GLCostAccountCollection
		{
			get { return ChargeCodeLookups.GLCostAccountCollection; }
		}

		#endregion

		#region GL Accrual Accounts

		public AccGLHeaderCollection GLAccrualAccountCollection
		{
			get { return ChargeCodeLookups.GLAccrualAccountCollection; }
		}

		#endregion

		#region ConsolidationAccountingCategoryClassCollection

		public CodeDescriptionPairList ConsolidationAccountingCategoryClassCollection
		{
			get { return ChargeCodeLookups.ConsolidationAccountingCategoryClassCollection; }
		}

		#endregion

		#endregion

		#region Charge Code Lookups

		AccChargeCodeLookups ChargeCodeLookups
		{
			get { return new AccChargeCodeLookups(Parent.ChargeCode); }
		}

		#endregion

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList => Parent.JobTypeDirectionAndTransportListProvider.JobTypeList;

		public static class JobTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region DirectionList

		public CodeDescriptionPairList DirectionList => Parent.JobTypeDirectionAndTransportListProvider.DirectionList;

		#endregion

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList => Parent.JobTypeDirectionAndTransportListProvider.TransportModeList;

		public static class TransportModeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region ConsolContainerModeList

		public CodeDescriptionPairList ConsolContainerModeList
		{
			get
			{
				if (consolContainerModeList == null)
				{
					consolContainerModeList = ObjectFactory.Get<IFreightCodePairListProvider>().GetConsolModeList(ZString.Empty, ZString.Empty);
					consolContainerModeList.Insert(0, new CodeDescriptionPair(ConsolContainerModeAdditionalCodes.All, Res.GetString("a03d81ee-eb0b-47b8-a8de-fc9ba0e48eac", "Any Container Mode")));
				}
				return consolContainerModeList;
			}
		}
		CodeDescriptionPairList consolContainerModeList;

		public static class ConsolContainerModeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region MasterPaymentTypeList

		public CodeDescriptionPairList MasterPaymentTypeList
		{
			get
			{
				if (masterPaymentTypeList == null)
				{
					masterPaymentTypeList = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
					masterPaymentTypeList.Insert(0, new CodeDescriptionPair(MasterPaymentTypeAdditionalCodes.All, Res.GetString("c434b994-c6e7-4549-8d9b-f912f8858f0f", "All Payment Term")));
				}
				return masterPaymentTypeList;
			}
		}
		CodeDescriptionPairList masterPaymentTypeList;

		public static class MasterPaymentTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region HousePaymentTypeList

		public CodeDescriptionPairList HousePaymentTypeList
		{
			get
			{
				if (housePaymentTypeList == null)
				{
					housePaymentTypeList = new CodeDescriptionPairList(OLookUpEditType.PaymentType);
					housePaymentTypeList.Insert(0, new CodeDescriptionPair(HousePaymentTypeAdditionalCodes.All, Res.GetString("5245fcfb-478f-4bbf-8871-e5656e725a29", "All Payment Term")));
				}
				return housePaymentTypeList;
			}
		}
		CodeDescriptionPairList housePaymentTypeList;

		public static class HousePaymentTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion
	}
}
