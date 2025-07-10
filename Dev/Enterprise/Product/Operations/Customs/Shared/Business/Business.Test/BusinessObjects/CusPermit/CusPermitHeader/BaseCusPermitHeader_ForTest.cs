using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusPermitHeader_ForTest : BaseCusPermitHeader
	{
		public BaseCusPermitHeader_ForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusPermitHeaderLookups GetNewLookups()
		{
			return new CusPermitHeaderLookups_ForTest(this);
		}

		public override PermitCountrySpecificInstruction CountrySpecificInstruction => new PermitCountrySpecificInstruction_ForTest(Factory);
	}
}
