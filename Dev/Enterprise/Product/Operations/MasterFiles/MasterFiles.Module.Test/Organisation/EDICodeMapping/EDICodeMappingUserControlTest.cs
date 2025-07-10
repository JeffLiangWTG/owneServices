using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class EDICodeMappingUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNewZFilterStrip()
		{
			using (var filterControl = new EDICodeMappingUserControlForTest(
				new OrgPatternMatchOverrideCollectionForOrgs(Factory, new ZQuery()),
				new EDICodeMappingFilterBusinessObject()))
			using (var filterStrip = filterControl.NewZFilterStripForTest())
			{
				AssertType<EDICodeMappingFilterStrip>(filterStrip);
			}
		}
	}
}
