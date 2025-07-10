using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceDefaultingConfigurationViewLookups : AutoAccCashAdvanceDefaultingConfigurationViewLookups
	{
		public AccCashAdvanceDefaultingConfigurationViewLookups(AutoAccCashAdvanceDefaultingConfigurationView parent) : base(parent)
		{
		}

		public new IJobConfiguration Parent => (IJobConfiguration)base.Parent;

		#region Charge Code FindBoxCollection

		public AccChargeCodeCollection ChargeCodeFindBoxCollection
		{
			get
			{
				if (chargeCodeFindBoxCollection == null)
				{
					chargeCodeFindBoxCollection = new AccChargeCodeCollection(Factory, GlbCompany.CurrentCompany);
					chargeCodeFindBoxCollection.FilterBusinessObjectDefaults.Add(new CargoWise.EntityFramework.FilterBusinessObjectDefault("Description", "Property", ZString.Empty));
				}
				return chargeCodeFindBoxCollection;
			}
		}
		AccChargeCodeCollection chargeCodeFindBoxCollection;

		#endregion

		#region LedgerList

		public CodeDescriptionPairList LedgerList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ZString.Empty, Res.GetString("b9ea4592-d953-41a8-8edc-e65169ad788e", "Both Accounts Receivable and Payable"));
				result.AddPair(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription);
				result.AddPair(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription);
				return result;
			}
		}

		#endregion

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList => JobConfigurationLookupsExtensions.GetJobTypeList();

		#endregion

		#region Direction List

		public CodeDescriptionPairList DirectionsList => Parent.GetDirectionList();

		#endregion

		#region Mode List

		public CodeDescriptionPairList TransportModesList => Parent.GetTransportModeList();

		#endregion

		#region Defaulting Option List

		public CodeDescriptionPairList DefaultingOptions => CashAdvanceDefaultingOption.CodesList;

		#endregion
	}
}
