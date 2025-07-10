using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX101MessageSendingObject))]
	sealed class NX101MessageSendingObjectTest : LicensingMessageSendingObjectTest<NX101MessageSendingObject>
	{
		[ExpectNoExceptions]
		public void TestObjectTypes()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageSendingObject.Validation, NUnit.Framework.Is.TypeOf<NX101MessageSendingObjectValidation>());
				NUnit.Framework.Assert.That(messageSendingObject.Consignment, NUnit.Framework.Is.TypeOf<NX101Consignment>());
				NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf<NX101GoodsShipment>());
				foreach (var governmentProcedure in messageSendingObject.GovernmentProcedure)
				{
					NUnit.Framework.Assert.That(governmentProcedure, NUnit.Framework.Is.TypeOf<NX101GovernmentProcedure>());
				}
				NUnit.Framework.Assert.That(messageSendingObject.Packaging, NUnit.Framework.Is.TypeOf<NX101Packaging>());
				NUnit.Framework.Assert.That(messageSendingObject.PreviousDocument, NUnit.Framework.Is.TypeOf<NX101PreviousDocument>());
				NUnit.Framework.Assert.That(messageSendingObject.Application, NUnit.Framework.Is.TypeOf<NX101Application>());
				NUnit.Framework.Assert.That(messageSendingObject.COImporter, NUnit.Framework.Is.TypeOf<NX101PartyDetailsWrapper>());
			});
		}

		[ExpectNoExceptions]
		protected override void TestFunctionCode()
		{
			NUnit.Framework.Assert.That(messageSendingObject.FunctionCode, NUnit.Framework.Is.EqualTo(NX101ActionCodeList.Codes._17).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCOImporter()
		{
			var importerDocumentaryAddress = messageSendingObject.Header.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_Address1 = "1500 HAPPY RD";
			importerDocumentaryAddress.E2_Address2 = "ORANGE DISTRICT";
			var importerTranslatedDocumentaryAddress = importerDocumentaryAddress.LocalAddress;
			importerTranslatedDocumentaryAddress.E2_AddressType = "ITA";
			importerTranslatedDocumentaryAddress.E2_ParentTableCode = "TW1";
			importerTranslatedDocumentaryAddress.CompanyName = "TW Address";
			importerTranslatedDocumentaryAddress.E2_Address1 = "臺北加工出口區園東街6號";
			importerTranslatedDocumentaryAddress.E2_Address2 = string.Empty;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageSendingObject.COImporter.ChineseName, NUnit.Framework.Is.EqualTo(importerTranslatedDocumentaryAddress.CompanyName), "COImporter.ChineseName is not expected");
				NUnit.Framework.Assert.That(messageSendingObject.COImporter.Address.Line, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT TAIWAN").Using(CustomComparers.TypeComparison), "COImporter.Address.Line");
				NUnit.Framework.Assert.That(messageSendingObject.COImporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("台灣臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "COImporter.Address.ChineseLine");
			});

			importerDocumentaryAddress.E2_Address1 = "";
			importerTranslatedDocumentaryAddress.E2_Address1 = "";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageSendingObject.COImporter.ChineseName, NUnit.Framework.Is.EqualTo(importerTranslatedDocumentaryAddress.CompanyName), "COImporter.ChineseName is not expected");
				NUnit.Framework.Assert.That(messageSendingObject.COImporter.Address.Line, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "COImporter.Address.Line");
				NUnit.Framework.Assert.That(messageSendingObject.COImporter.Address.ChineseLine, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "COImporter.Address.ChineseLine");
			});
		}

		public void TestSerializeToMessageString()
		{
			messageSendingObject.Action = NX101ActionCodeList.Codes._9;
			var messageString = messageSendingObject.SerializeToMessageString();
			CombineAssertions(() =>
			{
				AssertXMLContains("<FunctionCode>9</FunctionCode>", messageString);
				AssertXMLContains("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><Declaration xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"urn:wco:datamodel:TW:NX101:R-01-00 NX101.xsd\" xmlns=\"urn:wco:datamodel:TW:NX101:R-01-00\">", messageString);
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			NUnit.Framework.Assert.That(new NX101MessageSendingObject(header).Action, NUnit.Framework.Is.EqualTo(NX101ActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return new NX101MessageSendingObject(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			header.ImporterDocumentaryAddress.OrganisationPK = org.PK;
			messageSendingObject = new NX101MessageSendingObject(header);
			messageSendingObject.Action = NX101ActionCodeList.Codes._17;
		}

		protected override NX101MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX101MessageSendingObject(header);

		CusTWControllingMessageHeader header;
		NX101MessageSendingObject messageSendingObject;
	}
}
