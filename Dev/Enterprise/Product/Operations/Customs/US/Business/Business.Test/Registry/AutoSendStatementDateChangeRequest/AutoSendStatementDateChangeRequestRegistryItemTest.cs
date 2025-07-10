using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutoSendStatementDateChangeRequestRegistryItem))]
	sealed class AutoSendStatementDateChangeRequestRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<AutoSendStatementDateChangeRequest>
	{
		public void TestRegistryOptions()
		{
			AssertEquals("Options", RegistryOptions.Default, new AutoSendStatementDateChangeRequestRegistryItem("DUMMY", (NoResString)"DUMMY", (NoResString)"DUMMY", (NoResString)"DUMMY").Options);
		}

		protected override StronglyTypedRegistryItem<AutoSendStatementDateChangeRequest, AutoSendStatementDateChangeRequest> GetNewRegistryItem()
		{
			return new AutoSendStatementDateChangeRequestRegistryItem("", null, null, null);
		}

		protected override AutoSendStatementDateChangeRequest ValidValue
		{
			get
			{
				AutoSendStatementDateChangeRequest autoSendStatementDateChangeRequest = new AutoSendStatementDateChangeRequest();
				autoSendStatementDateChangeRequest.OverrideAllOrByOrganisation = "ALL";
				return autoSendStatementDateChangeRequest;
			}
		}
	}
}
