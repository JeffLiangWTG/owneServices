using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatingDocManagerInfo))]
	public class RatingDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.NewWithValidTestData<ClientRate>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.NewWithValidTestData<ClientRate>();
		}
	}
}
