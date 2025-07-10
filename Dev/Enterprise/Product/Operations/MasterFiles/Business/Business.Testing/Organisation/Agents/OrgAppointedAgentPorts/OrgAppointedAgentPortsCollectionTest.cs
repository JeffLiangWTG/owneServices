using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAppointedAgentPortsCollection))]
	sealed class OrgAppointedAgentPortsCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgAppointedAgentPortsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgAppointedAgentPorts>();
		}
	}
}
