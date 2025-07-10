using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	using System.Linq;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
	using Enterprise.UniversalDataBuss.DataObjects.Core;

	partial class DeclarationDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestAdditionalMapping()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "NZWLG";
			declaration.JE_GoodsLocatedAt = GoodsLocatedAtListForSeaImport.Codes.DES;
			declaration.JE_LocationOfGoods = string.Empty;

			var deliveryPartyOrg = Factory.New<OrgHeader>();
			deliveryPartyOrg.OH_Code = "DELIVERTO";
			declaration.JE_OH_NotifyParty = deliveryPartyOrg.PK;

			AssertEquals("Goods Location is Final Destination", "NZWLG", declaration.JE_Cal_GoodsLocation);
			AssertEquals("LocationOfGoods is empty", string.Empty, declaration.JE_LocationOfGoods);

			Factory.SaveForTesting();

			var declarationData = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			var organizationCollection = declarationData.OrganizationAddressCollection;
			AssertEquals("Delivery Notification Party", "DELIVERTO", organizationCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.DeliveryNotificationParty).OrganizationCode);
			AssertEquals("Should export JE_Cal_GoodsLocation.", "NZWLG", declarationData.LocationAtClearance.Code);
		}
	}
}
