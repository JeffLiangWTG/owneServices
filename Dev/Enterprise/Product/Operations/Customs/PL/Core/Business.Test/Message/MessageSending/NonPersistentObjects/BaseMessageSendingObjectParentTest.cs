using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(BaseMessageSendingObjectParent))]
sealed class BaseMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestHasEDocs()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		CombineAssertions(() =>
		{
			Assert("No EDocs", !sendingObj.HasEDocs);
			sendingObj.EDocs.AddNew();
			Assert("Has EDocs", sendingObj.HasEDocs);
		});
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestCreateNewEDocsCollection()
	{
		IeDoc GetEDoc(string fileName, string documentType)
		{
			var fullFileName = Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\{fileName}");
			return ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(fullFileName, documentType);
		}

		var doc1 = GetEDoc("sample.pdf", Core.Constants.FileFormats.PDF);
		GetEDoc("Test.xls", Core.Constants.FileFormats.XLS);
		GetEDoc("small.jpg", Core.Constants.FileFormats.JPG);

		doc1.DocType = "ABC";
		var reqDoc = declaration.DocsAndCartage.RequiredDocuments.AddNew("ABC");
		reqDoc.EQ_DocumentNotes = "Note1";

		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		var storageDocs = sendingObj.EDocs.StorageDocs.ToArray();
		AssertEquals("GUID match", doc1.UniqueKey, storageDocs.Single().PK);
		AssertEquals("Note match", "Note1", sendingObj.EDocs.DocumentNotes[doc1.UniqueKey]);
	}

	public void TestCustomsOffice_DefaultValue()
	{
		declaration.JE_CustomsOffice = "A";
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		AssertEquals("A", sendingObj.CustomsOffice);
	}

	public void TestCaptions()
	{
		var objectType = GetExpectedBusinessObjectType();
		AssertEquals("Customs Office", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.CustomsOffice)).Caption);
		AssertEquals("Purpose of Sending", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.PurposeOfSending)).Caption);
		AssertEquals("Procedure", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.Procedure)).Caption);
		AssertEquals("MRN number", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.MrnNumber)).Caption);
		AssertEquals("Ref number", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.RefNumber)).Caption);
		AssertEquals("Comments", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.Comments)).Caption);
		AssertEquals("Enquiry Information Code", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.EnquiryInformationCode)).Caption);
		AssertEquals("Office of Exit Actual", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.OfficeOfExitActual)).Caption);
		AssertEquals("Exit Date", DataBoundResourceStrings.GetDataForProperty(objectType, nameof(CustomsDeclarationMessageSendingObjectParent.ExitDate)).Caption);
	}

	public void TestSendButtonEnabled_EDocs()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		CombineAssertions(() =>
		{
			AssertEquals("SendButtonEnabled should be false bacause there are no eDocs", false, sendingObj.SendButtonEnabled);

			sendingObj.EDocs.AddNew();
			AssertEquals("SendButtonEnabled should be true bacause there are eDocs", true, sendingObj.SendButtonEnabled);
		});
	}

	public void TestValidationWasResetOnActionUpdate()
	{
		var sendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
		var sendingObj = sendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>().First();

		var bizObjValidationMessageErrorsWasUpdated = false;
		var additionalWarningsInfoWasUpdated = false;

		sendingObjectParent.BizObjValidationMessageErrorsInfo.ValueChanged += (s, a) => bizObjValidationMessageErrorsWasUpdated = true;
		sendingObjectParent.AdditionalWarningsInfo.ValueChanged += (s, a) => additionalWarningsInfoWasUpdated = true;
		sendingObj.Action = "Test";
		CombineAssertions(() =>
		{
			Assert(nameof(sendingObjectParent.BizObjValidationMessageErrors), bizObjValidationMessageErrorsWasUpdated);
			Assert(nameof(sendingObjectParent.AdditionalWarnings), additionalWarningsInfoWasUpdated);
		});
	}

	public void TestGetNewValidation() => AssertType<BaseMessageSendingObjectParentValidation>(GetJobDeclarationMessageSendingObjectParent().Validation);

	public void TestParentDeclaration() => AssertType<JobDeclaration>(GetJobDeclarationMessageSendingObjectParent().ParentDeclaration);

	public void TestObjectsToSend()
	{
		var sendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
		var sendingObj1 = (BaseMessageSendingObject)sendingObjectParent.SendingObjectsCollection.First();
		sendingObj1.ShouldSend = ZBool.False;
		CombineAssertions(() =>
		{
			AssertEquals("0 ObjectsToSend", 0, sendingObjectParent.ObjectsToSend.Count());

			sendingObj1.ShouldSend = ZBool.True;
			AssertEquals("1 ObjectsToSend", 1, sendingObjectParent.ObjectsToSend.Count());
		});
	}

	public void TestObjectsToSendType()
	{
		var sendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
		var sendingObj = sendingObjectParent.SendingObjectsCollection.First();

		AssertType<BaseMessageSendingObject>(sendingObj);
	}

	public void TestAllowSendWithError()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		sendingObj.AllowSendWithError = ZBool.False;
		CombineAssertions(() =>
		{
			AssertEquals(ZBool.False, sendingObj.AllowSendWithError);

			sendingObj.AllowSendWithError = ZBool.True;
			AssertEquals(ZBool.True, sendingObj.AllowSendWithError);
		});
	}

	public void TestFallbackSystem()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		sendingObj.FallbackSystem = ZBool.False;
		CombineAssertions(() =>
		{
			AssertEquals(ZBool.False, sendingObj.FallbackSystem);

			sendingObj.FallbackSystem = ZBool.True;
			AssertEquals(ZBool.True, sendingObj.FallbackSystem);
		});
	}

	public void TestIsExport()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		CombineAssertions(() =>
		{
			sendingObj.ParentDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", true, sendingObj.IsExport);

			sendingObj.ParentDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", false, sendingObj.IsExport);
		});
	}

	public void TestSendButtonEnabled()
	{
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		CombineAssertions(() =>
		{
			sendingObj.AllowSendWithError = false;
			AssertEquals("SendButtonEnabled should be false bacause AllowSendWithError is false and errors exist", false, sendingObj.SendButtonEnabled);

			sendingObj.AllowSendWithError = true;
			AssertEquals("SendButtonEnabled should be true bacause AllowSendWithError is true", true, sendingObj.SendButtonEnabled);

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CustomsEntryHeaders.AddNew();
			sendingObj = new BaseMessageSendingObjectParent(jobDeclaration);
			using (sendingObj.GetValidationSuspender())
			{
				sendingObj.ObjectsToSend.ForEach(x =>
				{
					x.SuspendValidation();
					x.Header.SuspendValidation();
					x.Header.Declaration.SuspendValidation();
				});
				AssertEquals("SendButtonEnabled should be true bacause there are no message errors", true, sendingObj.SendButtonEnabled);
			}
		});
	}

	public void TestOfficeOfExitActual() => CombineAssertions(() =>
	{
		var stringTest = "AB";
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		AssertEquals("Default value", ZString.Empty, sendingObj.OfficeOfExitActual);

		var exitControlHeader = Factory.New<CusExitControlHeader>();
		exitControlHeader.CusExitDetails.AddNew().CED_CustomsOffice = stringTest;
		exitControlHeader.CEH_ParentID = declaration.PK;
		AssertEquals("sync with exitControlHeader", stringTest, sendingObj.OfficeOfExitActual);
	});

	public void TestExitDate() => CombineAssertions(() =>
	{
		var timeTest = ZDateTime.BrettsBirthday;
		var sendingObj = GetJobDeclarationMessageSendingObjectParent();
		AssertEquals("Default value", ZDateTime.Empty, sendingObj.ExitDate);

		var exitControlHeader = Factory.New<CusExitControlHeader>();
		exitControlHeader.CusExitDetails.AddNew().CED_ExitDate = timeTest;
		exitControlHeader.CEH_ParentID = declaration.PK;
		AssertEquals("sync with exitControlHeader", timeTest, sendingObj.ExitDate);
	});

	protected override BusinessObject GetNewBusinessObject() => new BaseMessageSendingObjectParent(declaration);

	BaseMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent() => (BaseMessageSendingObjectParent)GetNewBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
}
