using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CINShipmentReportSendingProviderTest : DocDataObjectReportSendingProviderTest
	{
		protected override ZString ExpectedMessageSenderFullName => "Enterprise.Freight.Forwarding.Documents.DocDataObjects.Common.ExportNotificationMessageSender";

		protected override ZString ExpectedModuleIdentifier => ModuleIDs.JobShipment.Name;

		protected override DocDataObjectReportSendingProvider GetSendingProvider()
		{
			return new CINShipmentReportSendingProvider(Factory);
		}

		protected override BusinessObject GetValidBusinessObjectForSending()
		{
			var testHelper = new ExportNotificationMessageTestHelper(Factory);
			return testHelper.CreateShipment();
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			oldAllowToSend = PortMessagingRegistry.Instance.AllowToSendExportNotification.Value;
			PortMessagingRegistry.Instance.AllowToSendExportNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.France);
		}

		protected override void TearDown()
		{
			PortMessagingRegistry.Instance.AllowToSendExportNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldAllowToSend);
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);

			base.TearDown();
		}

		ZString oldCountryCode;
		bool oldAllowToSend;

		#endregion
	}
}
