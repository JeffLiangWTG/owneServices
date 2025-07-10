using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCaptionsAndDescriptions()
		{
			CombineAssertions(() =>
			{
				BusinessObjectCaptionTestHelper.AssertCaptions(SupportingDocument.TypeInfo, "Type");
				BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(SupportingDocument.LineNumberInfo, "Document Line Number", "Document Line No.", "Doc. Line No.", string.Empty);
			});
		}

		public void TesteDoc()
		{
			ZGuid pK1 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK1.ToGuid(), "CODE", "DESCRIPTION");
			var targetInfo = SupportingDocument.EDocInfo;
			SupportingDocument.EDoc = ZGuid.Invalid;
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			SupportingDocument.EDoc = ZGuid.Empty;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			SupportingDocument.EDoc = pK1;
			AssertNoErrors(targetInfo);
			SupportingDocuments.StorageDocs.AddPair(pK1.ToGuid(), "CODE", "DESCRIPTION");
			SupportingDocument duplicateDocument = SupportingDocuments.AddNew();
			duplicateDocument.EDoc = pK1;
			var duplicateInfo = duplicateDocument.EDocInfo;
			AssertHasError(duplicateInfo, SupportingDocument.Constants.OnlyOneCanBeSent);
			ZGuid pK2 = ZGuid.NewZGuid();
			SupportingDocuments.StorageDocs.AddPair(pK2.ToGuid(), "CODE", "DESCRIPTION");
			duplicateDocument.EDoc = pK2;
			AssertNoErrors(duplicateInfo);
		}

		public void TestDocumentNo()
		{
			var targetInfo = SupportingDocument.DocumentNoInfo;
			SupportingDocument.DocumentNo = "\u884c\u653f\u9662\u8fb2\u696d\u59d4\u54e1\u6703";
			AssertHasError(targetInfo, SupportingDocument.Constants.OnlyAllowAlphanumeric);
			SupportingDocument.DocumentNo = "doc1";
			AssertNoErrors(targetInfo);
		}

		public void TestRemarks()
		{
			SupportingDocument.Remarks = "\u884c\u653f\u9662\u8fb2\u696d\u59d4\u54e1\u6703";
			AssertNoErrors(SupportingDocument.RemarksInfo);
		}

		public void TestControllingAgency()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWControllingAgency, "ControllingAgencies");
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWControllingAgency, "ZL", "BABA THE BUILDER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var targetInfo = SupportingDocument.ControllingAgencyInfo;
			SupportingDocument.ControllingAgency = "ZZ";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			SupportingDocument.ControllingAgency = "ZL";
			AssertNoErrors(targetInfo);
		}

		[ExpectNoExceptions]
		public void TestControllingAgencyList()
		{
			NUnit.Framework.Assert.That(SupportingDocument.ControllingAgencyList.Count, NUnit.Framework.Is.EqualTo(0));
		}

		sealed class SupportingDocumentForTest : SupportingDocument
		{
			public SupportingDocumentForTest(BusinessObjectFactory facotry) : base(facotry)
			{
			}

			public int? OverrideDocumentSize
			{
				get;
				set;
			}

			protected override long GetDocumentSizeCore() => OverrideDocumentSize ?? base.GetDocumentSizeCore();
		}

		public void TestSizeValidation()
		{
			var documents = new SupportingDocumentCollection(Factory);
			var document1 = new SupportingDocumentForTest(Factory);
			var document2 = new SupportingDocumentForTest(Factory);
			documents.Add(document1);
			documents.Add(document2);
			document1.OverrideDocumentSize = 20 * 1024 * 1024;
			document2.OverrideDocumentSize = 180 * 1024 * 1024;
			document1.EDoc = ZGuid.NewZGuid();
			document2.EDoc = ZGuid.NewZGuid();
			AssertHasError(document1.EDocInfo, SupportingDocument.Constants.MaxFileSize10MB);
			AssertHasError(document1.EDocInfo, SupportingDocument.Constants.MaxTotalFileSize200MB);
			AssertHasError(document2.EDocInfo, SupportingDocument.Constants.MaxFileSize10MB);
			AssertHasError(document2.EDocInfo, SupportingDocument.Constants.MaxTotalFileSize200MB);
			document1.OverrideDocumentSize = 9 * 1024 * 1024;
			document1.EDoc = ZGuid.NewZGuid();
			document2.EDoc = ZGuid.NewZGuid();
			AssertNoError(document1.EDocInfo, SupportingDocument.Constants.MaxFileSize10MB);
			AssertNoError(document1.EDocInfo, SupportingDocument.Constants.MaxTotalFileSize200MB);
			AssertHasError(document2.EDocInfo, SupportingDocument.Constants.MaxFileSize10MB);
			AssertNoError(document2.EDocInfo, SupportingDocument.Constants.MaxTotalFileSize200MB);
		}

		public void TestValidateAll()
		{
			SupportingDocument.EDoc = ZGuid.NewZGuid();
			SupportingDocument.DocumentNo = "?";
			SupportingDocument.ControllingAgency = "12";
			SupportingDocument.LineNumber = -1;
			SupportingDocument.EDocInfo.ClearAllNotifications();
			SupportingDocument.DocumentNoInfo.ClearAllNotifications();
			SupportingDocument.ControllingAgencyInfo.ClearAllNotifications();
			SupportingDocument.LineNumberInfo.ClearAllNotifications();
			SupportingDocument.ValidateAll();
			AssertHasErrors(SupportingDocument.EDocInfo);
			AssertHasErrors(SupportingDocument.DocumentNoInfo);
			AssertHasMessageErrors(SupportingDocument.ControllingAgencyInfo);
			AssertHasMessageErrors(SupportingDocument.LineNumberInfo);
		}

		#region Implementation

		SupportingDocumentCollection SupportingDocuments => supportingDocuments ??= new SupportingDocumentCollection(Factory);

		SupportingDocumentCollection supportingDocuments;

		SupportingDocument SupportingDocument => supportingDocument ??= SupportingDocuments.AddNew();

		SupportingDocument supportingDocument;

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return SupportingDocument;
		}

		#endregion
	}
}
