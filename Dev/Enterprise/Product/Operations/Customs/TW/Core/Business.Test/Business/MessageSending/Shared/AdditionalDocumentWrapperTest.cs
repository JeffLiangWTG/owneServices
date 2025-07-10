using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentWrapper))]
	sealed class AdditionalDocumentWrapperTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestProperties()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			var cusHead = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead.CH_JE = declaration.PK;
			cusHead.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), MessageConstants.DocumentTypes.PKL);
			doc1.DocType = "1";
			var messageSendingObject = new LicensingMessageSendingObjectForTest(header);
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.EDoc = doc1.UniqueKey;
			docLine1.DocumentNo = "11";
			docLine1.Type = "TYP";
			docLine1.Remarks = "Remarks";
			docLine1.LineNumber = 5;
			IAdditionalDocument additionalDocument = new AdditionalDocumentWrapper(docLine1, doc1);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalDocument.ID, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(additionalDocument.ImageFileFormat, NUnit.Framework.Is.EqualTo("PDF").Using(CustomComparers.TypeComparison), "ImageFileFormat");
				NUnit.Framework.Assert.That(additionalDocument.ImageFileName, NUnit.Framework.Is.EqualTo("sample.pdf").Using(CustomComparers.TypeComparison), "ImageFileName");
				NUnit.Framework.Assert.That(additionalDocument.TypeCode, NUnit.Framework.Is.EqualTo("TYP").Using(CustomComparers.TypeComparison), "TypeCode");
				NUnit.Framework.Assert.That(additionalDocument.Content, NUnit.Framework.Is.EqualTo("Remarks").Using(CustomComparers.TypeComparison), "Content");
				NUnit.Framework.Assert.That(additionalDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "SequenceNumeric");
			});

			CombineAssertions(() =>
			{
				additionalDocument = new AdditionalDocumentWrapper("id");
				NUnit.Framework.Assert.That(additionalDocument.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison), "AdditionalDocument.ID should be");
				NUnit.Framework.Assert.That(additionalDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "AdditionalDocument.SequenceNumeric should be");
				additionalDocument = new AdditionalDocumentWrapper("id1", 12);
				NUnit.Framework.Assert.That(additionalDocument.ID, NUnit.Framework.Is.EqualTo("id1").Using(CustomComparers.TypeComparison), "AdditionalDocument.ID should be");
				NUnit.Framework.Assert.That(additionalDocument.SequenceNumeric, NUnit.Framework.Is.EqualTo(12).Using(CustomComparers.TypeComparison), "AdditionalDocument.SequenceNumeric should be");
				additionalDocument = new AdditionalDocumentWrapper(ZDateTime.BrettsBirthday);
				NUnit.Framework.Assert.That(additionalDocument.SlaughterDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday.Date), "AdditionalDocument.SlaughterDateTime should be");
			});
		}
	}
}
