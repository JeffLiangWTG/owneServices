using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX101GovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestManufacturers()
		{
			var localProcessorAddress = header.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			localProcessorAddress.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			localProcessorAddress.IDCode = "12345678";
			localProcessorAddress.CompanyName = "LPA Company Name";
			localProcessorAddress.E2_Address1 = "1500 HAPPY RD";
			localProcessorAddress.E2_Address2 = "ORANGE DISTRICT";
			localProcessorAddress.AdditionalAddressInformation = "5F";
			localProcessorAddress.E2_RN_NKCountryCode = "TW";
			localProcessorAddress.E2_City = "APPLE CITY";
			localProcessorAddress.E2_Postcode = "12345";
			localProcessorAddress.E2_State = "TPE";
			localProcessorAddress.E2_Email = "user@wtg.com";
			localProcessorAddress.E2_Phone = "13925568211";
			localProcessorAddress.E2_Fax = "+88621234567";

			var lpaLocalAddress = localProcessorAddress.LocalAddress;
			lpaLocalAddress.CompanyName = "本地加工公司";
			lpaLocalAddress.E2_Address1 = "臺北加工出口區園東街6號";
			lpaLocalAddress.E2_Address2 = string.Empty;
			lpaLocalAddress.AdditionalAddressInformation = "5樓";
			lpaLocalAddress.E2_RN_NKCountryCode = "TW";
			lpaLocalAddress.E2_City = "臺北巿";
			lpaLocalAddress.E2_Postcode = "90093";
			lpaLocalAddress.E2_State = "TPE";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			var manufacturer1 = invoiceLine.ManufacturerDocAddress;
			manufacturer1.E2_AddressOverride = true;
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			var manufacturer2 = invoiceLine.ManufacturerDocAddress;
			manufacturer2.E2_AddressOverride = true;
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;
			var manufacturer3 = invoiceLine.ManufacturerDocAddress;
			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			manufacturer1.IDCode = "12345678";
			manufacturer1.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			manufacturer1.CompanyName = "LPA Company Name";

			manufacturer2.IDCode = "12345678";
			manufacturer2.IDCodeType = OrgCusCode.CodeTypes.VATCode;
			manufacturer2.LocalAddress.CompanyName = "本地加工公司";

			var orgHeader = new TestTWCreator(Factory).CreateOrganization();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "00695437", Core.Constants.CountryCodes.Taiwan);
			manufacturer3.OrganisationPK = orgHeader.PK;

			var manufacturers = governmentAgencyGoodsItem.Manufacturers.ToArray();
			NUnit.Framework.Assert.That(manufacturers.Length, NUnit.Framework.Is.EqualTo(2), "Manufacturers.Length");
			AssertManufacturer("Local Processor", manufacturers[0], "12345678", "LPA Company Name", "本地加工公司", "Y", "58", new AddressWrapper("1500 HAPPY RD ORANGE DISTRICT 5F APPLE CITY 12345 TAIWAN", "90093臺北巿臺北加工出口區園東街6號5樓"), new CommunicationWrapper[] { new CommunicationWrapper("13925568211", "TE"), new CommunicationWrapper("user@wtg.com", "MA"), new CommunicationWrapper("+88621234567", "FX") });
			AssertManufacturer("InvoiceLine Manufacturer", manufacturers[1], "00695437", "HAPPY CO., LTD.", "綠晃科技股份有限公司", "N", "53", new AddressWrapper("1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN", "90093臺北巿臺北加工出口區園東街6號"), new CommunicationWrapper[] { new CommunicationWrapper("13925568211", "TE"), new CommunicationWrapper("123@456.com", "MA"), new CommunicationWrapper("13925579322", "FX") });

			localProcessorAddress.E2_Address1 = "";
			lpaLocalAddress.E2_Address1 = "";
			var address = orgHeader.MainAddress;
			var zhTWtranslatedAddress = address.TranslatedAddresses.FirstOrDefault(c => c.OTA_Language == Core.SharedConstants.Languages.ChineseTraditional);
			address.OA_Address1 = "";
			zhTWtranslatedAddress.OTA_Address1 = "";

			manufacturers = governmentAgencyGoodsItem.Manufacturers.ToArray();
			NUnit.Framework.Assert.That(manufacturers.Length, NUnit.Framework.Is.EqualTo(2), "Manufacturers.Length");
			AssertManufacturer("Local Processor", manufacturers[0], "12345678", "LPA Company Name", "本地加工公司", "Y", "58", new AddressWrapper("", ""), new CommunicationWrapper[] { new CommunicationWrapper("13925568211", "TE"), new CommunicationWrapper("user@wtg.com", "MA"), new CommunicationWrapper("+88621234567", "FX") });
			AssertManufacturer("InvoiceLine Manufacturer", manufacturers[1], "00695437", "HAPPY CO., LTD.", "綠晃科技股份有限公司", "N", "53", new AddressWrapper("", ""), new CommunicationWrapper[] { new CommunicationWrapper("13925568211", "TE"), new CommunicationWrapper("123@456.com", "MA"), new CommunicationWrapper("13925579322", "FX") });
		}

		[ExpectNoExceptions]
		void AssertManufacturer(string message, IPartyDetails manufacturer, string id, string name, string chineseName, string mainManufacturer, string typeCode, IAddress address, IEnumerable<ICommunication> communications)
		{
			CombineAssertions(message, () =>
			{
				NUnit.Framework.Assert.That(manufacturer.ID, NUnit.Framework.Is.EqualTo(id).Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(manufacturer.Name, NUnit.Framework.Is.EqualTo(name).Using(CustomComparers.TypeComparison), "Name");
				NUnit.Framework.Assert.That(manufacturer.ChineseName, NUnit.Framework.Is.EqualTo(chineseName).Using(CustomComparers.TypeComparison), "ChineseName");
				NUnit.Framework.Assert.That(manufacturer.MainManufacturer, NUnit.Framework.Is.EqualTo(mainManufacturer).Using(CustomComparers.TypeComparison), "MainManufacturer");
				NUnit.Framework.Assert.That(manufacturer.TypeCode, NUnit.Framework.Is.EqualTo(typeCode).Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(manufacturer.Address.Line, NUnit.Framework.Is.EqualTo(address.Line), "Address.Line");
				NUnit.Framework.Assert.That(manufacturer.Address.ChineseLine, NUnit.Framework.Is.EqualTo(address.ChineseLine), "Address.ChineseLine");
				var actualCommunications = manufacturer.Communications.ToArray();
				var expectedCommunications = communications.ToArray();
				NUnit.Framework.Assert.That(actualCommunications.Length, NUnit.Framework.Is.EqualTo(expectedCommunications.Length), "Communications.Length");
				for (var i = 0; i < actualCommunications.Length; i++)
				{
					NUnit.Framework.Assert.That(actualCommunications[i].ID, NUnit.Framework.Is.EqualTo(expectedCommunications[i].ID), "Communication.ID");
					NUnit.Framework.Assert.That(actualCommunications[i].TypeID, NUnit.Framework.Is.EqualTo(expectedCommunications[i].TypeID), "Communication.TypeID");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestOrigins()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Austria;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Belgium;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Belgium;
			invoiceLine.InvoiceLineLinkControllingMsgHeaders[0].IsLinkedCMHeader = true;

			header.TW1_CertificateType = CertificateTypeList.Codes.Code1;
			NUnit.Framework.Assert.That(governmentAgencyGoodsItem.Origins, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IOrigin>)), "Only output when CertificateType is '08', '16', '17' - should be [null]");

			var certificateTypes = new[] { CertificateTypeList.Codes.Code8, CertificateTypeList.Codes.Code16, CertificateTypeList.Codes.Code17 };
			foreach (var certificateType in certificateTypes)
			{
				header.TW1_CertificateType = certificateType;
				var origins = governmentAgencyGoodsItem.Origins.ToArray();
				NUnit.Framework.Assert.That(origins.Length, NUnit.Framework.Is.EqualTo(2), "Origins.Length, no duplicate country codes");
				NUnit.Framework.Assert.That(origins[0].CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Austria).Using(CustomComparers.TypeComparison), "Origins[0].CountryCode");
				NUnit.Framework.Assert.That(origins[1].CountryCode, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Belgium).Using(CustomComparers.TypeComparison), "Origins[1].CountryCode");
			}
		}

		[ExpectNoExceptions]
		public void TestPreviousDocuments()
		{
			var previousDocumentNumber = header.PreviousDocumentNumbers.AddNew();
			previousDocumentNumber.CSI_ReferenceNumber = "AW 10085S1431";
			previousDocumentNumber = header.PreviousDocumentNumbers.AddNew();
			previousDocumentNumber.CSI_ReferenceNumber = "AW 10085S1432";
			var previousDocuments = governmentAgencyGoodsItem.PreviousDocuments.ToArray();
			NUnit.Framework.Assert.That(previousDocuments.Length, NUnit.Framework.Is.EqualTo(2), "previousDocuments.Length");
			NUnit.Framework.Assert.That(previousDocuments[0].ID, NUnit.Framework.Is.EqualTo("AW 10085S1431").Using(CustomComparers.TypeComparison), "previousDocuments[0].ID");
			NUnit.Framework.Assert.That(previousDocuments[1].ID, NUnit.Framework.Is.EqualTo("AW 10085S1432").Using(CustomComparers.TypeComparison), "previousDocuments[1].ID");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			governmentAgencyGoodsItem = new NX101GovernmentAgencyGoodsItem(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		NX101GovernmentAgencyGoodsItem governmentAgencyGoodsItem;
	}
}
