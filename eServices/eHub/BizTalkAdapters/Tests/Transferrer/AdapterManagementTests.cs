using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Microsoft.BizTalk.Adapter.Framework;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.Transferrer
{
	public class AdapterManagementTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TransferrerCore_AdapterManagement_GetSchema()
		{
			var target = new Mock<AdapterManagement>();
			var result = target.Object.GetSchema("uri", "ns", out var fileLocation);

			Assert.That(fileLocation, Is.EqualTo(string.Empty));
			Assert.That(result, Is.EqualTo(Result.Continue));
		}
	}
}
