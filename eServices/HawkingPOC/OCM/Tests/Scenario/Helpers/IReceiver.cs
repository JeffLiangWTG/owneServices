using System;
using System.Threading;
using System.Threading.Tasks;

namespace OcmPoc.Tests.Scenario.Helpers
{
	interface IReceiver
    {
		IReceiver Initialise();
		Task WaitForMessages(TimeSpan timeout);
    }
}
