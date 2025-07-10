using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgServiceLevel))]
	sealed class OrgServiceLevelTest : EnterpriseBusinessObjectTestCase
	{
		public void TestServiceLevel()
		{
			RefServiceLevel refServiceLevel1 = Factory.New<RefServiceLevel>();
			refServiceLevel1.RS_Code = "X11";
			refServiceLevel1.RS_Description = "XWindow";

			RefServiceLevel refServiceLevel2 = Factory.New<RefServiceLevel>();
			refServiceLevel2.RS_Code = "X12";
			refServiceLevel2.RS_Description = "Whatever";

			Factory.Save();

			OrgServiceLevel orgServiceLevel = Factory.New<OrgServiceLevel>();

			orgServiceLevel.PM_RS_NKSrvLvl = refServiceLevel1.RS_Code;
			AssertEquals(refServiceLevel1.PK, orgServiceLevel.ServiceLevel.PK);

			orgServiceLevel.PM_RS = refServiceLevel2.PK;
			AssertEquals(refServiceLevel2.PK, orgServiceLevel.ServiceLevel.PK);
		}
	}
}
