using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business
{
	public class EDIMessageContentFilterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEDIMessageContentFilterTypes()
		{
			var bizo = Factory.New<EDIMessageContentFilter>();
			AssertEquals(new EDIMessageContentFilterTypes().Count, bizo.Lookups.FilterTypes.Count);
			AssertEquals(typeof(EDIMessageContentFilterTypes), bizo.Lookups.FilterTypes.GetType());
		}
	}
}
