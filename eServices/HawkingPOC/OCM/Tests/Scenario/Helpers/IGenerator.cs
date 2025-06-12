using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OcmPoc.Tests.Scenario.Helpers
{
    interface IGenerator
    {
		IGenerator PrepareMessage(string line, int lineCount);
		Task GenerateMessages();
    }
}
