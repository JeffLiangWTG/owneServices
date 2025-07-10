using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105ManufacturerWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(manufacturer.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(manufacturer.Name, NUnit.Framework.Is.EqualTo("X21XXXX3344").Using(CustomComparers.TypeComparison));
			var manufacturerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerOrgOrgAddress = manufacturerOrg.Addresses.AddNew();
			manufacturerOrgOrgAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			manufacturerOrgOrgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			manufacturerOrgOrgAddress.OA_IsActive = true;
			manufacturerOrgOrgAddress.OA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			manufacturerOrgOrgAddress.OA_CompanyNameOverride = "公司名稱";
			var englishAddress = manufacturerOrgOrgAddress.TranslatedAddresses.AddNew();
			englishAddress.OTA_Language = Core.SharedConstants.Languages.English;
			englishAddress.Address1 = "test1";
			englishAddress.Address2 = "test2";
			englishAddress.OTA_CompanyName = "ota company name";

			manufacturerAddress.E2_OA_Address = manufacturerOrgOrgAddress.PK;
			manufacturer = new NX5105ManufacturerWrapper(manufacturerAddress, invoiceLine);
			NUnit.Framework.Assert.That(manufacturer.Name, NUnit.Framework.Is.EqualTo("ota company name").Using(CustomComparers.TypeComparison));

			manufacturerAddress.E2_AddressOverride = true;
			manufacturerAddress.E2_CompanyName = "Override company name";
			manufacturer = new NX5105ManufacturerWrapper(manufacturerAddress, invoiceLine);
			NUnit.Framework.Assert.That(manufacturer.Name, NUnit.Framework.Is.EqualTo("Override company name").Using(CustomComparers.TypeComparison));

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			manufacturer = new NX5105ManufacturerWrapper(manufacturerAddress, invoiceLine);
			CombineAssertions("Should be empty for NX401", () =>
			{
				NUnit.Framework.Assert.That(invoiceLine.IsForCMHeaderMessageTypeNX401, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Precondition");
				NUnit.Framework.Assert.That(manufacturer.Name, NUnit.Framework.Is.EqualTo(ZString.Empty), "Name");
			});
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(manufacturer.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestAddress()
		{
			NUnit.Framework.Assert.That(manufacturer.Address.Line, NUnit.Framework.Is.EqualTo("ADDRESS 1 ADDRESS 2 TAIWAN").Using(CustomComparers.TypeComparison), "manufacturer.Address.Line should be");

			manufacturerAddress.E2_AddressOverride = true;
			manufacturerAddress.Address1 = "MANUFACTURER ADDRESS1";
			manufacturerAddress.Address2 = "MANUFACTURER ADDRESS2";
			manufacturerAddress.TPCCode = "22334455";
			manufacturerAddress.AEOCode = "987654321";
			manufacturerAddress.IDCodeType = "PAS";
			manufacturerAddress.IDCode = "12345678";
			manufacturerAddress.E2_Phone = "0222221234";
			manufacturerAddress.E2_Email = "paul@pllink.com";
			manufacturer = new NX5105ManufacturerWrapper(manufacturerAddress, invoiceLine);
			NUnit.Framework.Assert.That(manufacturer.Address.Line, NUnit.Framework.Is.EqualTo("MANUFACTURER ADDRESS1 MANUFACTURER ADDRESS2 TAIWAN").Using(CustomComparers.TypeComparison), "manufacturer.Address.Line should be");

			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMessageHeaders = entryInstruction.ControllingMessageHeaders;
			invoiceLine.JI_CEI = entryInstruction.PK;
			var controllingMessageHeader = controllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			manufacturer = new NX5105ManufacturerWrapper(manufacturerAddress, invoiceLine);
			CombineAssertions("Address should be null for NX401", () =>
			{
				NUnit.Framework.Assert.That(invoiceLine.IsForCMHeaderMessageTypeNX401, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Precondition");
				NUnit.Framework.Assert.That(manufacturer.Address, NUnit.Framework.Is.EqualTo(default(IAddress)), "Address - should be [null]");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			manufacturerAddress = invoiceLine.ManufacturerDocAddress;
			var assress = new TestTWCreator(Factory).CreateOrganizationForManufacturer().MainAddress;
			assress.OA_Address1 = "ADDRESS 1";
			assress.OA_Address2 = "ADDRESS 2";
			manufacturerAddress.E2_OA_Address = assress.PK;
			manufacturer = new NX5105ManufacturerWrapper(manufacturerAddress, invoiceLine);
		}

		IPartyDetails manufacturer;
		TWJobDocAddress manufacturerAddress;
		JobComInvoiceLine invoiceLine;
		JobDeclaration declaration;
	}
}
