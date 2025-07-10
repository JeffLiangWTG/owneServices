using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401MessageSendingObjectAdditionalMessageErrorCollector))]
	sealed class NX401MessageSendingObjectAdditionalMessageErrorCollectorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestValidateConsignee()
		{
			const string localCompanyNameMessageError = "Consignee: You have not entered a Consignee Local Company Name.";
			const string localAddressMessageError = "Consignee: You have not entered a Consignee Local Address.";
			const string notEnteredConsigneeMessageError = "Consignee: You have not entered a Consignee.";

			var consigneeWithoutLocalAddress = Factory.New<OrgHeader>();
			var consigneeWithEmptyValue = Factory.New<OrgHeader>();
			var localAddressEmpty = consigneeWithEmptyValue.MainAddress.TranslatedAddresses.AddNew();
			localAddressEmpty.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			localAddressEmpty.CompanyName = ZString.Empty;
			localAddressEmpty.Address1 = ZString.Empty;

			var consigneeFull = Factory.New<OrgHeader>();
			var localAddressFull = consigneeFull.MainAddress.TranslatedAddresses.AddNew();
			localAddressFull.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			localAddressFull.CompanyName = "綠晃科技股份有限公司";
			localAddressFull.Address1 = "遠東路88號";

			CombineAssertions("Import", () =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_OH_Consignee = consigneeWithoutLocalAddress.PK;
				NUnit.Framework.Assert.That(collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "consignee without local address");
				NUnit.Framework.Assert.That(collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "consignee without local address");

				declaration.JE_OH_Consignee = consigneeWithEmptyValue.PK;
				NUnit.Framework.Assert.That(collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "consignee with empty values local address");
				NUnit.Framework.Assert.That(collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "consignee with empty values local address");

				declaration.JE_OH_Consignee = consigneeFull.PK;
				NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "consignee with local address");
				NUnit.Framework.Assert.That(!collector.Contains(localAddressMessageError), NUnit.Framework.Is.True, "consignee with local address");
			});

			CombineAssertions("Export", () =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

				declaration.JE_OH_Consignee = ZGuid.Empty;
				NUnit.Framework.Assert.That(collector.Contains(notEnteredConsigneeMessageError), NUnit.Framework.Is.True, "not entered a consignee");

				declaration.JE_OH_Consignee = consigneeFull.PK;
				NUnit.Framework.Assert.That(!collector.Contains(notEnteredConsigneeMessageError), NUnit.Framework.Is.True, "entered a consignee");
			});
		}

		[ExpectNoExceptions]
		public void TestValidateDeclarant()
		{
			var localCompanyNameMessageError = "Declarant: You have not entered a Declarant Local Company Name.";

			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "not entered a Declarant");

			var declarantOrg = Factory.New<OrgHeader>();
			var declarantAddress = declarantOrg.Addresses.AddNew();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			NUnit.Framework.Assert.That(collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "not entered a Declarant Local Company Name.");

			var declarantLocalAddress = declarantAddress.TranslatedAddresses.AddNew();
			declarantLocalAddress.Language = Core.SharedConstants.Languages.ChineseTraditional;
			declarantLocalAddress.CompanyName = "TestCompany";
			NUnit.Framework.Assert.That(!collector.Contains(localCompanyNameMessageError), NUnit.Framework.Is.True, "entered a Declarant Local Company Name.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			sendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, ControllingMessageTypeList.Codes.NX401);
			collector = new NX401MessageSendingObjectAdditionalMessageErrorCollector(sendingObjectParent);
		}

		JobDeclaration declaration;
		LicensingMessageSendingObjectParent sendingObjectParent;
		NX401MessageSendingObjectAdditionalMessageErrorCollector collector;
	}
}
