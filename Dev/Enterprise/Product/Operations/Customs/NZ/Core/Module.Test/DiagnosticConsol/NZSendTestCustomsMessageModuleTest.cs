using Enterprise.Customs.Module.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Testing
{
	[TestedType(typeof(NZSendTestCustomsMessageModule))]
	sealed class NZSendTestCustomsMessageModuleTest : SendDiagnosticMessageModuleTest<NZSendTestCustomsMessageModule, NZSendTestCustomsMessageController>
	{
	}
}
