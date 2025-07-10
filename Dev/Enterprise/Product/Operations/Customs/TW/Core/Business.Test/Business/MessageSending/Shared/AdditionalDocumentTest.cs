using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AdditionalDocumentTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var cusHead = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead.CH_JE = declaration.PK;
			cusHead.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), MessageConstants.DocumentTypes.PKL);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), MessageConstants.DocumentTypes.CAT);
			var doc3 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\compressed.jpg"), MessageConstants.DocumentTypes.CIV);
			var doc4 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif"), MessageConstants.DocumentTypes.IEP);
			var doc5 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_1page.tif"), MessageConstants.DocumentTypes.TDM);
			var doc6 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_2pages.tif"), "TSH");
			var messageSendingObject = new AdditionalDocumentMessageSendingObject(cusHead);
			messageSendingObject.ContactOffice = "A";
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.EDoc = doc1.UniqueKey;
			docLine1.LineNumber = 2;
			var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
			docLine2.EDoc = doc2.UniqueKey;
			var docLine3 = messageSendingObject.SupportingDocuments.AddNew();
			docLine3.EDoc = doc3.UniqueKey;
			var docLine4 = messageSendingObject.SupportingDocuments.AddNew();
			docLine4.EDoc = doc4.UniqueKey;
			IAdditionalDocument additionalDocument = new GoodsShipmentAdditionalDocumentWrapper(docLine1, doc1);
			docLine1.DocumentNo = "11";
			docLine1.Remarks = "XXXXXXXXXXXXXX";
			docLine1.ControllingAgency = "55";
			NUnit.Framework.Assert.That(additionalDocument.ID, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.Content, NUnit.Framework.Is.EqualTo("XXXXXXXXXXXXXX").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.ImageFileFormat, NUnit.Framework.Is.EqualTo("PDF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.ImageFileName, NUnit.Framework.Is.EqualTo("sample.pdf").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.SizeMeasure, NUnit.Framework.Is.EqualTo(28451).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo("55").Using(CustomComparers.TypeComparison));
			additionalDocument = new GoodsShipmentAdditionalDocumentWrapper(docLine1, doc2);
			NUnit.Framework.Assert.That(additionalDocument.ImageFileFormat, NUnit.Framework.Is.EqualTo("XLS").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.ImageFileName, NUnit.Framework.Is.EqualTo("Test.xls").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison));
			additionalDocument = new GoodsShipmentAdditionalDocumentWrapper(docLine1, doc3);
			NUnit.Framework.Assert.That(additionalDocument.ImageFileFormat, NUnit.Framework.Is.EqualTo("JPG").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.ImageFileName, NUnit.Framework.Is.EqualTo("compressed.jpg").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("3").Using(CustomComparers.TypeComparison));
			additionalDocument = new GoodsShipmentAdditionalDocumentWrapper(docLine1, doc4);
			NUnit.Framework.Assert.That(additionalDocument.ImageFileFormat, NUnit.Framework.Is.EqualTo("GIF").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.ImageFileName, NUnit.Framework.Is.EqualTo("small.gif").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("4").Using(CustomComparers.TypeComparison));
			additionalDocument = new GoodsShipmentAdditionalDocumentWrapper(docLine1, doc5);
			NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("5").Using(CustomComparers.TypeComparison));
			additionalDocument = new GoodsShipmentAdditionalDocumentWrapper(docLine1, doc6);
			NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("9").Using(CustomComparers.TypeComparison));
		}
	}
}
