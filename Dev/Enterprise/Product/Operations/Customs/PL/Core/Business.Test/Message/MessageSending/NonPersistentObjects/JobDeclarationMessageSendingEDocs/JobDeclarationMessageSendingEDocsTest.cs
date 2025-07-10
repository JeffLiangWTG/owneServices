using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(JobDeclarationMessageSendingEDocs))]
sealed class JobDeclarationMessageSendingEDocsTest : NonPersistentBusinessObjectTestCase
{
	public void TestSetEDocNote()
	{
		var storageDocs = new CodeDescriptionPairList();
		var docGuid = ZGuid.NewZGuid();
		storageDocs.AddPair(docGuid, "code", "description");
		var documentNotes = new Dictionary<ZGuid, ZString>() { { docGuid, "Note" } };

		var collection = new JobDeclarationMessageSendingEDocsCollection(Factory, () => storageDocs, sendingObjectParent, () => documentNotes);
		var edoc = collection.AddNew();
		CombineAssertions(() =>
		{
			edoc.DocumentDescription = "ABC";
			edoc.EDoc = ZGuid.NewZGuid();
			AssertEquals("Empty", ZString.Empty, edoc.DocumentDescription);
			edoc.EDoc = docGuid;
			AssertEquals("Not Empty", "Note", edoc.DocumentDescription);
		});
	}

	public void TestSupportingDocument()
	{
		CombineAssertions(() =>
		{
			var eDoc = GetJobDeclarationMessageSendingEDocs();
			AssertEquals("Max Length", 50, eDoc.SupportingDocumentInfo.MaxLength);
			AssertEquals("Caption", "Supporting Document", DataBoundResourceStrings.GetDataForProperty(eDoc.SupportingDocumentInfo).Caption);
			Assert("Not readonly", !eDoc.SupportingDocumentInfo.ReadOnly);
			eDoc.AdditionalInformation = "NotEmpty";
			Assert("Readonly", eDoc.SupportingDocumentInfo.ReadOnly);
		});
	}

	public void TestAdditionalInformation()
	{
		CombineAssertions(() =>
		{
			var eDoc = GetJobDeclarationMessageSendingEDocs();
			AssertEquals("Max Length", 50, eDoc.AdditionalInformationInfo.MaxLength);
			AssertEquals("Caption", "Additional Information", DataBoundResourceStrings.GetDataForProperty(eDoc.AdditionalInformationInfo).Caption);
			Assert("Not readonly", !eDoc.AdditionalInformationInfo.ReadOnly);
			eDoc.SupportingDocument = "NotEmpty";
			Assert("Readonly", eDoc.AdditionalInformationInfo.ReadOnly);
		});
	}

	public void TestEDoc_Caption()
	{
		AssertEquals("Document", DataBoundResourceStrings.GetDataForProperty(GetJobDeclarationMessageSendingEDocs().EDocInfo).Caption);
	}

	public void TestSupportingDocumentList()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var eDocs = GetJobDeclarationMessageSendingEDocs();
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
		const string importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
		helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Export type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, exportCodeType, "ExportCode1", "ExportDesc1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, exportCodeType, "ExportCode2", "ExportDesc2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Import type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, importCodeType, "ImportCode1", "ImportDesc1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, importCodeType, "ImportCode2", "ImportDesc2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var list = eDocs.SupportingDocumentList;
			list.Load();
			Assert("Export Contains Code1", list.Select(x => x.ZZD_Code).Contains("ExportCode1"));
			Assert("Export Code2 description", list.Find(x => x.ZZD_Code == "ExportCode2").First().ZZD_Description == "ExportDesc2");
			AssertEquals("Export Is cached", list, eDocs.SupportingDocumentList);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			list = eDocs.SupportingDocumentList;
			list.Load();
			Assert("Import Contains Code1", list.Select(x => x.ZZD_Code).Contains("ImportCode1"));
			Assert("Import Code2 description", list.Find(x => x.ZZD_Code == "ImportCode2").First().ZZD_Description == "ImportDesc2");
			AssertEquals("Import Is cached", list, eDocs.SupportingDocumentList);
		});
	}

	public void TestAdditionalInformationList()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var eDocs = GetJobDeclarationMessageSendingEDocs();
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string exportCodeType = UniversalReferenceConstants.RefCusCodeListType.Codes.AdditionalInformationCodes;
		const string importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
		helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Export type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, exportCodeType, "ExportCode1", "ExportDesc1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, exportCodeType, "ExportCode2", "ExportDesc2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Import type");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, importCodeType, "ImportCode1", "ImportDesc1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, importCodeType, "ImportCode2", "ImportDesc2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		CombineAssertions(() =>
		{
			var list = eDocs.AdditionalInformationList;
			list.Load();
			Assert("Export Contains Code1", list.Select(x => x.ZZD_Code).Contains("ExportCode1"));
			Assert("Export Code2 description", list.Find(x => x.ZZD_Code == "ExportCode2").First().ZZD_Description == "ExportDesc2");
			AssertEquals("Export Is cached", list, eDocs.AdditionalInformationList);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			list = eDocs.AdditionalInformationList;
			list.Load();
			Assert("Import Contains Code1", list.Select(x => x.ZZD_Code).Contains("ImportCode1"));
			Assert("Import Code2 description", list.Find(x => x.ZZD_Code == "ImportCode2").First().ZZD_Description == "ImportDesc2");
			AssertEquals("Import Is cached", list, eDocs.AdditionalInformationList);
		});
	}

	public void TestStorageDocs()
	{
		var codeDescriptionList = new CodeDescriptionPairList();
		var guid1 = new ZGuid("1A65A960-1159-44FC-A863-F3981E47F63C");
		var guid2 = new ZGuid("A81BD267-0C04-4500-855B-CB3A419C4702");
		codeDescriptionList.AddPair(guid1, "abc.pdf", "abc.pdf");
		codeDescriptionList.AddPair(guid2, "cbd.pdf", "cbd.pdf");
		var testCollection = new JobDeclarationMessageSendingEDocsCollection(Factory, () => codeDescriptionList, sendingObjectParent,
			() => new Dictionary<ZGuid, ZString>());
		CombineAssertions(() =>
		{
			var item = testCollection.AddNew();
			item.EDoc = guid1;
			AssertEquals("Only 1 item in StorageDocs", 2, item.StorageDocs.Count);
			var item2 = testCollection.AddNew();
			AssertEquals("2 items in StorageDocs (one with selected Document)", 1, item2.StorageDocs.Count);
		});
	}

	public void TestFilename()
	{
		var storageDocs = new CodeDescriptionPairList();
		var docGuid = ZGuid.NewZGuid();
		storageDocs.AddPair(docGuid, "Filename.something", "description");
		var docGuid2 = ZGuid.NewZGuid();
		storageDocs.AddPair(docGuid2, "Declaration - D00-Filename2.something2", "description2");

		var documentNotes = new Dictionary<ZGuid, ZString>() { { docGuid, "Note" }, { docGuid2, "Note2" } };

		var collection = new JobDeclarationMessageSendingEDocsCollection(Factory, () => storageDocs, sendingObjectParent, () => documentNotes);
		var edoc = collection.AddNew();
		CombineAssertions(() =>
		{
			edoc.EDoc = ZGuid.NewZGuid();
			AssertEquals("Empty", ZString.Empty, edoc.Filename);

			edoc.EDoc = (ZGuid)collection.StorageDocs.ToArray()[0].PK;
			AssertEquals("PK from dbo.storageDocs", "Filename.something", edoc.Filename);

			edoc.EDoc = docGuid;
			AssertEquals("Not Empty", "Filename.something", edoc.Filename);

			edoc.EDoc = docGuid2;
			AssertEquals("Split FileName result", "Filename2.something2", edoc.Filename);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => GetJobDeclarationMessageSendingEDocs();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		sendingObjectParent = new CustomsDeclarationMessageSendingObjectParent(declaration);
	}

	JobDeclarationMessageSendingEDocs GetJobDeclarationMessageSendingEDocs() => sendingObjectParent.EDocs.AddNew();

	CustomsDeclarationMessageSendingObjectParent sendingObjectParent;
	JobDeclaration declaration;
}
