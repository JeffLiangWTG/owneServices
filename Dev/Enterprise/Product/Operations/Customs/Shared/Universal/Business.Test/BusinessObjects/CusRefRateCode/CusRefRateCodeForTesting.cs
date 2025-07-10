using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal.Testing
{
	public class CusRefRateCodeForTesting : CusRefRateCode
	{
		public CusRefRateCodeForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CR7_RateType = "OTH";
			Factory.Save();
		}
	}
}
