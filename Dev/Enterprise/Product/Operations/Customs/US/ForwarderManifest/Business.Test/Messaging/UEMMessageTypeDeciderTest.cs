using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			AssertEquals("GetTypeForBinding", typeof(UEMEDIMessage), uemMessageTypeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForLoad()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.USExportManifest;
			message.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			var row = ((INeedRow)message).Row;
			AssertEquals("GetTypeForLoad", typeof(UEMEDIMessage), uemMessageTypeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("GetTypeForNew", typeof(UEMEDIMessage), uemMessageTypeDecider.GetTypeForNew());
		}

		protected override void SetUp()
		{
			base.SetUp();
			uemMessageTypeDecider = new UEMMessageTypeDecider();
		}

		UEMMessageTypeDecider uemMessageTypeDecider;
	}
}
