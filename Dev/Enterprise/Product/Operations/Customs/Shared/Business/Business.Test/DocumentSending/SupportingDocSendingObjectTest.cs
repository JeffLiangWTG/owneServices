using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(SupportingDocSendingObject))]
	sealed class SupportingDocSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SupportingDocSendingObject(declaration);
		}

		public void TestDocument()
		{
			var sendingObject = GetNewBusinessObject() as SupportingDocSendingObject;
			AssertNull(sendingObject.Document);
			AssertEquals(0m, sendingObject.EDocFileSizeInMB);

			sendingObject.EDoc = eDocOnDeclaration.UniqueKey;
			AssertEquals(eDocOnDeclaration.UniqueKey, sendingObject.Document.UniqueKey);
			AssertEquals(new ZDecimal(0.00000095367431640625), sendingObject.EDocFileSizeInMB);
		}

		public void TestNew()
		{
			var sendingObject = SupportingDocSendingObject.New(declaration);
			AssertType<JobDeclarationSupportingDocSendingObject>(sendingObject);
			Assert("ShouldSend", sendingObject.ShouldSend);
		}

		public void TestAvailableEDocs()
		{
			var sendingObject = GetNewBusinessObject() as SupportingDocSendingObject;
			AssertEquals(1, sendingObject.AvailableEDocs.Count);
		}

		public void TestShouldCheckFileNameInEdocField()
		{
			var sendingObject = GetNewBusinessObject() as SupportingDocSendingObject;
			AssertEquals(false, sendingObject.ShouldCheckFileNameInEdocField);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<BaseJobDeclaration>();
			eDocOnDeclaration = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
		}

		BaseJobDeclaration declaration;
		IeDoc eDocOnDeclaration;
	}
}
