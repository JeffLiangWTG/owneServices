using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupActiveBusinessObjectCollection))]
	sealed class GlbGroupActiveBusinessObjectCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbGroupActiveBusinessObjectCollection>
	{
		public void TestStaffGroupOnly()
		{
			AssertEquals(true, Factory.ExistsInDatabase(GlbGroupSchema.Constants.TableName, new ZQuery(GlbGroupSchema.GG_Type, GlbGroupTypeList.Codes.Organisation)));
			var group3 = new GlbGroupActiveBusinessObjectCollection(Factory, true);
			var group4 = new GlbGroupActiveBusinessObjectCollection(Factory, false);
			AssertEquals(false, group3.OfType<GlbGroup>().Any(x => x.GG_Type == GlbGroupTypeList.Codes.Organisation));
			AssertEquals(true, group4.OfType<GlbGroup>().Any(x => x.GG_Type == GlbGroupTypeList.Codes.Organisation));
		}
	}
}
