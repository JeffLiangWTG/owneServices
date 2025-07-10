using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderDocManagerInfo))]
	sealed class CusOutturnHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<CusOutturnHeader>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New<CusOutturnHeader>();
		}
	}
}
