using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbPersonDocManagerInfo))]
	sealed class GlbPersonDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New(typeof(GlbPerson));
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.New(typeof(GlbPerson));
		}
	}
}
