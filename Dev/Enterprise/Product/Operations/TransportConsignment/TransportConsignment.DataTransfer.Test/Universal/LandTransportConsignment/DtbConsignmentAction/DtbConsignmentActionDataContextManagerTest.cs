using System.Linq;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbConsignmentActionDataContextManager))]
	public class DtbConsignmentActionDataContextManagerTest : DataContextManagerTestCase<DtbConsignmentActionDataContextManager, DtbConsignmentAction>
	{
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LandTransportConsignmentAction, new DtbConsignmentActionDataContextManager().DataContextType);
			AssertEquals("UniversalDataContextManager attrbitue must be applied on DtbConsignmentAction.", DataContextType.LandTransportConsignmentAction, Factory.New<DtbConsignmentAction>().GetUniversalDataContextManager().DataContextType);
			AssertEquals("", new DtbConsignmentActionDataContextManager().DataContextKey);
			AssertNull(new DtbConsignmentActionDataContextManager().DefaultOutputDirectory);
			AssertEquals(0, new DtbConsignmentActionDataContextManager().EventContextValues.Count());
		}

		#endregion
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("Action must not return a job number.", true);
		}
	}
}
