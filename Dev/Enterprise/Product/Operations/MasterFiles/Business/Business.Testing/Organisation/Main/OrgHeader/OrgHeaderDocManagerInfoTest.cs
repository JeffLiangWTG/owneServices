using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderDocManagerInfo))]
	sealed class OrgHeaderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New(typeof(OrgHeader));
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New(typeof(OrgHeader));
		}
	}
}
