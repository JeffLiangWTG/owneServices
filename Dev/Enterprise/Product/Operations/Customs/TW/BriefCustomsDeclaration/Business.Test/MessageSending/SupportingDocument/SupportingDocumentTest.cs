using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCheckType()
		{
			var errorMessage = "Please enter a Type.";
			var targetInfo = SupportingDocument.TypeInfo;
			SupportingDocument.EDoc = ZGuid.NewZGuid();
			SupportingDocument.Type = "1";
			AssertNoError(targetInfo, errorMessage);

			SupportingDocument.Type = ZString.Empty;
			AssertHasError(targetInfo, errorMessage);
		}

		public void TestCheckBillNumber()
		{
			var errorMessage = "Please enter a Bill Number.";
			var targetInfo = SupportingDocument.BillNumberInfo;
			SupportingDocument.EDoc = ZGuid.NewZGuid();
			SupportingDocument.BillNumber = "1";
			AssertNoError(targetInfo, errorMessage);

			SupportingDocument.BillNumber = ZString.Empty;
			AssertHasError(targetInfo, errorMessage);
		}

		public void TestValidateAll()
		{
			SupportingDocument.EDoc = ZGuid.NewZGuid();
			SupportingDocument.Type = ZString.Empty;
			SupportingDocument.BillNumber = ZString.Empty;
			SupportingDocument.TypeInfo.ClearAllNotifications();
			SupportingDocument.BillNumberInfo.ClearAllNotifications();
			SupportingDocument.ValidateAll();
			AssertHasErrors(SupportingDocument.TypeInfo);
			AssertHasErrors(SupportingDocument.BillNumberInfo);
		}

		SupportingDocumentCollection SupportingDocuments => supportingDocuments ??= new SupportingDocumentCollection(Factory, null, null, null);

		SupportingDocumentCollection supportingDocuments;

		SupportingDocument SupportingDocument => supportingDocument ??= SupportingDocuments.AddNew();

		SupportingDocument supportingDocument;

		protected override BusinessObject GetNewBusinessObject() => SupportingDocument;
	}
}
