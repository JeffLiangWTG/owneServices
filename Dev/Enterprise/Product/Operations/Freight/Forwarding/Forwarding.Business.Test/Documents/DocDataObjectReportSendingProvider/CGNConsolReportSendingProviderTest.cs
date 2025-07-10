using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CGNConsolReportSendingProviderTest : DocDataObjectReportSendingProviderTest
	{
		protected override ZString ExpectedMessageSenderFullName => "Enterprise.Freight.Forwarding.Documents.DocDataObjects.Common.ExportNotificationMessageSender";

		protected override ZString ExpectedModuleIdentifier => ModuleIDs.JobConsol.Name;

		protected override DocDataObjectReportSendingProvider GetSendingProvider()
		{
			return new CGNConsolReportSendingProvider(Factory);
		}

		protected override BusinessObject GetValidBusinessObjectForSending()
		{
			var testHelper = new ExportNotificationMessageTestHelper(Factory);
			return testHelper.CreateConsol();
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			oldAllowToSend = PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.Value;
			PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Netherlands);
		}

		protected override void TearDown()
		{
			PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldAllowToSend);
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);

			base.TearDown();
		}

		ZString oldCountryCode;
		bool oldAllowToSend;

		#endregion
	}
}
