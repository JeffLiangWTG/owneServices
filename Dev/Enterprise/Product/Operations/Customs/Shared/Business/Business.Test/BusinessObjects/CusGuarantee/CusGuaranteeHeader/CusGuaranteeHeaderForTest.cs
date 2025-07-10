using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGuaranteeHeaderForTest : BaseCusGuaranteeHeader
	{
		public CusGuaranteeHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool SupportsAdditionalCustomsReferencesCore => true;
	}
}
