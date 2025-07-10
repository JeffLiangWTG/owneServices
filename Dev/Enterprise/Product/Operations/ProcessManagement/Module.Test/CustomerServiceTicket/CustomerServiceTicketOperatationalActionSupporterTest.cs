using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(CustomerServiceTicketOperationalActionSupporter))]
	public class CustomerServiceTicketOperatationalActionSupporterTest : OperationalActionSupporterTest<CustomerServiceTicketOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CustomerServiceTicket; }
		}

		public override bool ShouldSupportDocuments
		{
			get { return true; }
		}
	}
}
