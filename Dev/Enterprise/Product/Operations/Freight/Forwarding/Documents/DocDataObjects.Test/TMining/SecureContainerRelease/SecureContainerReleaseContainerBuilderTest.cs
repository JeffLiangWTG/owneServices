using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using TMiningConstants = Enterprise.Freight.Forwarding.Business.TMiningConstants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class SecureContainerReleaseContainerBuilderTest : TestCaseWithFactory
	{
		public void TestCurrentStatus()
		{
			CombineAssertions("This test should ensure the value of containers' CurrentStatus are correct and all test cases are creating correct test data.", () =>
			{
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.Assigned, SecureContainerRelease.FormModeTransfer);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.Accepted, SecureContainerRelease.FormModeTransfer);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.TransferRejected, SecureContainerRelease.FormModeTransfer);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.RevokeRejected, SecureContainerRelease.FormModeRevoke);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.TransferSent, SecureContainerRelease.FormModeTransfer);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.RevokeSent, SecureContainerRelease.FormModeRevoke);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.Revoked, SecureContainerRelease.FormModeRevoke);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse, SecureContainerRelease.FormModeTransfer);
				AssertCurrentStatus(TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse, SecureContainerRelease.FormModeRevoke);
			});

			void AssertCurrentStatus(ZString status, ZString formMode)
			{
				var container = CreateContainer();
				SecureContainerReleaseContainerEventTestHelper.AddLogForContainer(container, status);

				var builder = new SecureContainerReleaseContainerBuilder();
				var data = builder.Build(container, formMode);

				AssertEquals(formMode, data.FormMode);
				AssertEquals(status, data.CurrentStatus);
			}
		}

		#region Implementation

		ForwardingContainer CreateContainer()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "CONT1111111";
			container.JC_IsNonOperativeReefer = false;

			return container;
		}

		#endregion
	}
}
