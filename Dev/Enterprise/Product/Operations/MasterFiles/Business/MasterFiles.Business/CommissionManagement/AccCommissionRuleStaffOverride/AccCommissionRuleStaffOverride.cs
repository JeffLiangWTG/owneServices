using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleStaffOverride : AutoAccCommissionRuleStaffOverride, ICommissionRuleRatesProvider
	{
		public AccCommissionRuleStaffOverride(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[ChildEditable]
		public AccCommissionRuleRateCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new AccCommissionRuleRateCollection(this);
					RegisterEditableChildObject(rates);
				}

				return rates;
			}
		}
		AccCommissionRuleRateCollection rates;

		#endregion

		#region Delete

		public override void Delete()
		{
			Rates.DeleteAll();

			base.Delete();
		}

		#endregion

		#region ICommissionRuleRatesProvider Members

		ZString ICommissionRuleRatesProvider.CommissionBasis
		{
			get { return CRO_CommissionBasis; }
			set { CRO_CommissionBasis = value; }
		}

		ZString ICommissionRuleRatesProvider.CommissionTriggerType
		{
			get { return CRO_CommissionTriggerType; }
			set { CRO_CommissionTriggerType = value; }
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (CRO_ACM_ParentRule.IsEmpty)
			{
				CRO_ACM_ParentRule = Factory.NewWithValidTestData<AccGroupCommissionRule>().PK;
			}

			if (CRO_CommissionTriggerType.IsEmpty)
			{
				CRO_CommissionTriggerType = CommissionTriggerTypes.Codes.FirstArInvoice;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
