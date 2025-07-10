using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccTaxOverrideGroupChargeCodePivot : AutoAccTaxOverrideGroupChargeCodePivot
	{
		public AccTaxOverrideGroupChargeCodePivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			ACP_AC_ChargeCode = Factory.NewWithValidTestData<AccChargeCode>().PK;
			ACP_AX_TaxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>().PK;
		}
#endif

	}
}