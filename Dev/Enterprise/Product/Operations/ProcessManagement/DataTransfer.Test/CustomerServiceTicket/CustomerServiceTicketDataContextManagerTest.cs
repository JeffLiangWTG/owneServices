using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	[TestedType(typeof(CustomerServiceTicketDataContextManager))]
	class CustomerServiceTicketDataContextManagerTest : ActivityDataContextManagerTestCase<CustomerServiceTicketDataContextManager, WorkRequest>
	{
		protected override DataContextType ExpectedDataContextType => DataContextType.CustomerServiceTicket;
		protected override Type ExpectedDataObjectWriterType => typeof(CustomerServiceTicketDataObjectWriter);
	}
}
