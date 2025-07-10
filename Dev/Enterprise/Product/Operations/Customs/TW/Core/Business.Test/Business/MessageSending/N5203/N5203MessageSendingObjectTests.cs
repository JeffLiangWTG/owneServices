using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5203MessageSendingObject))]
	sealed class N5203MessageSendingObjectTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return new N5203MessageSendingObject(entryHeader);
		}

		[TestDate(2023, 11, 5, 11, 20, 1)]
		[ExpectNoExceptions]
		public void TestAcceptanceDateTime()
		{
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 11, 19);
			NUnit.Framework.Assert.That(provider.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2023, 11, 19)).Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			NUnit.Framework.Assert.That(provider.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2023, 11, 5)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			entryHeader.EntryNumber = "";
			NUnit.Framework.Assert.That(provider.ID, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison));
			entryHeader.EntryNumber = "120-123456A";
			entryHeader.CH_Status = "AWO";
			NUnit.Framework.Assert.That(provider.ID, NUnit.Framework.Is.EqualTo(entryHeader.CusEntryNumber.CE_EntryNum));
		}

		[ExpectNoExceptions]
		public void TestInvoiceAmount()
		{
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_InvoiceAmount = 2536m;
			NUnit.Framework.Assert.That(provider.InvoiceAmount, NUnit.Framework.Is.EqualTo(2536m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalGrossMassMeasure()
		{
			declaration.JE_TotalWeight = 6000m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Grams;
			NUnit.Framework.Assert.That(provider.TotalGrossMassMeasure, NUnit.Framework.Is.EqualTo(6m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalPackageQuantity()
		{
			declaration.JE_TotalNoOfPacks = 1230;
			NUnit.Framework.Assert.That(provider.TotalPackageQuantity, NUnit.Framework.Is.EqualTo(declaration.JE_TotalNoOfPacks));
		}

		[ExpectNoExceptions]
		public void TestAssociatedGovernmentProcedureCode()
		{
			entryInstruction.CEI_ExamMode = ZString.Empty;
			NUnit.Framework.Assert.That(provider.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstruction.CEI_ExamMode = ExamModeList.Codes.FactoryInspection;
			NUnit.Framework.Assert.That(provider.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_ExamMode));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(provider.TypeCode, NUnit.Framework.Is.EqualTo(entryInstruction.CEI_Style));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			entryInstruction.AttachedDocumentNumbersAsString = "X1,X2,X3";
			var additionalDocumentList = provider.AdditionalDocuments.ToList();
			NUnit.Framework.Assert.That(additionalDocumentList.Count, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(additionalDocumentList.Any(i => i.ID == entryInstruction.TW_AttachedDoc1), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(additionalDocumentList.Any(i => i.ID == entryInstruction.TW_AttachedDoc2), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(additionalDocumentList.Any(i => i.ID == entryInstruction.TW_AttachedDoc3), NUnit.Framework.Is.True);
			entryInstruction.AttachedDocumentNumbersAsString = "X1,X2";
			additionalDocumentList = provider.AdditionalDocuments.ToList();
			NUnit.Framework.Assert.That(additionalDocumentList.Count, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformation()
		{
			var declarationDuplicate1 = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate1.CY_Code = "3";
			declarationDuplicate1.Copy = 1;
			var additionalInformation = provider.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));

			var declarationDuplicate2 = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate2.CY_Code = "4";
			declarationDuplicate2.Copy = 2;
			additionalInformation = provider.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(3).Using(CustomComparers.TypeComparison));

			var declarationDuplicate3 = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate3.CY_Code = "5";
			declarationDuplicate3.Copy = 3;
			additionalInformation = provider.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(6).Using(CustomComparers.TypeComparison));

			var declarationDuplicate4 = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate4.CY_Code = "6";
			declarationDuplicate4.Copy = 4;
			additionalInformation = provider.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(10).Using(CustomComparers.TypeComparison));

			var declarationDuplicate5 = entryInstruction.DeclarationDuplicates.AddNew();
			declarationDuplicate5.CY_Code = "7";
			declarationDuplicate5.Copy = 5;
			additionalInformation = provider.AdditionalInformation;
			NUnit.Framework.Assert.That(additionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(15).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAgent()
		{
			var org = testHelper.CreateOrganization();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			NUnit.Framework.Assert.That(provider.Agent.GetType(), NUnit.Framework.Is.EqualTo(typeof(Agent)));
			NUnit.Framework.Assert.That(provider.Agent.Name, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(provider.Agent.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(provider.Agent.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBorderTransportMeans()
		{
			NUnit.Framework.Assert.That(provider.BorderTransportMeans.GetType(), NUnit.Framework.Is.EqualTo(typeof(TransportMeans)));
		}

		[ExpectNoExceptions]
		public void TestCurrencyExchange()
		{
			NUnit.Framework.Assert.That(provider.CurrencyExchange.GetType(), NUnit.Framework.Is.EqualTo(typeof(CurrencyExchange)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee()
		{
			NUnit.Framework.Assert.That(provider.DutyTaxFee.GetType(), NUnit.Framework.Is.EqualTo(typeof(DutyTaxFee)));
		}

		[ExpectNoExceptions]
		public void TestGoodsShipment()
		{
			NUnit.Framework.Assert.That(provider.GoodsShipment.GetType(), NUnit.Framework.Is.EqualTo(typeof(GoodsShipment)));
		}

		[TestDate(2020, 07, 20)]
		[ExpectNoExceptions]
		public void TestGovernmentProcedureDescriptions()
		{
			declaration.JE_OH_Supplier = testHelper.CreateOrganizationForSupplier().PK;
			var supplier = declaration.Supplier;
			NUnit.Framework.Assert.That(supplier, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.MasterFiles.Business.OrgHeader)));
			supplier.RequiredDocuments.RemoveAndDeleteAll();
			entryInstruction.TW_TradersRemarks = ZString.Replicate('X', 256);
			var governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo(entryInstruction.TW_TradersRemarks));
			entryInstruction.TW_TradersRemarks = ZString.Replicate('X', 257);
			governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo(entryInstruction.TW_TradersRemarks.Substring(0, 256)));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[1], NUnit.Framework.Is.EqualTo("X").Using(CustomComparers.TypeComparison));
			entryInstruction.TW_TradersRemarks = ZString.Replicate('X', 513);
			governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo(entryInstruction.TW_TradersRemarks.Substring(0, 256)));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[1], NUnit.Framework.Is.EqualTo(entryInstruction.TW_TradersRemarks.Substring(256, 256)));
			entryInstruction.TW_TradersRemarks = "AAA\r\nBBB";
			governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo("AAA\r\nBBB").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_CustomsOffice = "AC";
			var poaDoc = supplier.RequiredDocuments.AddNew();
			poaDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			poaDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			poaDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 20);
			poaDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			poaDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			poaDoc.EQ_DocNumber = "111111";
			var poaDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaDocAttr.D0_AttribValue = "A";
			entryInstruction.TW_OverrideTradersRemarks = false;
			entryInstruction.TW_TradersRemarks += System.Environment.NewLine + "TEST\r\n hello world!";
			governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：111111\r\n起：109年07月20日\r\n迄：116年07月20日\r\nTEST\r\n hello world!").Using(CustomComparers.TypeComparison));
			poaDocAttr.D0_AttribValue = "B";
			entryInstruction.TW_OverrideTradersRemarks = false;
			entryInstruction.TW_TradersRemarks += System.Environment.NewLine + "TEST\r\n hello world!";
			governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo("\r\nTEST\r\n hello world!").Using(CustomComparers.TypeComparison));
			poaDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			poaDocAttr.D0_AttribValue = "A";
			entryInstruction.TW_OverrideTradersRemarks = false;
			entryInstruction.TW_TradersRemarks += System.Environment.NewLine + "TEST\r\n hello world!";
			governmentProcedureDescriptions = provider.GovernmentProcedureDescriptions.ToList();
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions[0], NUnit.Framework.Is.EqualTo("\r\nTEST\r\n hello world!").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPackaging()
		{
			NUnit.Framework.Assert.That(provider.Packaging.GetType(), NUnit.Framework.Is.EqualTo(typeof(Packaging)));
		}

		[ExpectNoExceptions]
		public void TestRepresentativePersonName()
		{
			testHelper.CreateRefVessel();
			var borker = Factory.NewWithValidTestData<GlbStaff>();
			var brkCertificate = borker.Certificates.AddNew();
			brkCertificate.XZ_Type = CertificateTypePairList.Codes.BR1;
			brkCertificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Taiwan;
			brkCertificate.XZ_RefNumber = "1234";
			declaration.JE_GS_NKCusAgent = borker.GS_Code;
			NUnit.Framework.Assert.That(provider.RepresentativePersonName, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetMessageOwner()
		{
			declaration.JE_OH_Supplier = testHelper.CreateOrganizationForSupplier().PK;
			var supplier = declaration.Supplier;
			NUnit.Framework.Assert.That(supplier, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.MasterFiles.Business.OrgHeader)));
			Factory.Save();
			NUnit.Framework.Assert.That(provider.GetMessageOwner(), NUnit.Framework.Is.EqualTo("6666666").Using(CustomComparers.TypeComparison));
		}

		public void TestSerializeToMessageString()
		{
			var messageSendingObject = provider;
			messageSendingObject.Action = "9";
			var messageString = messageSendingObject.SerializeToMessageString();
			AssertXMLContains("<FunctionCode>9</FunctionCode>", messageString);
			AssertXMLContains("<Declaration xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"urn:wco:datamodel:TW:N5203:R-00-05\">", messageString);
		}

		[ExpectNoExceptions]
		public void TestActionList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var cusHead1 = Factory.NewWithValidTestData<CusEntryHeader>();
			cusHead1.CH_JE = declaration.PK;
			cusHead1.CH_CEI_Instruction = entryInstruction.PK;
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = cusHead1.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = SharedJobMessageTypeList.Codes.Import;
			cusNum1.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			Factory.Save();
			cusHead1.CH_EntryStatus = "A";
			var action = new N5203MessageSendingObject(cusHead1);
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Update));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
			cusHead1.CH_EntryStatus = "";
			list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Update));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			entryInstruction = entryHeader.EntryInstruction;
			declaration = entryHeader.Declaration;
			testHelper.CreateAndSetProxyOrganization();
			invoiceLine = testHelper.CreateInvoiceLineForN5203(entryHeader);
			provider = new N5203MessageSendingObject(entryHeader);
		}

		TestTWCreator testHelper;
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		N5203MessageSendingObject provider;
		CusEntryHeader entryHeader;
		JobComInvoiceLine invoiceLine;
	}
}
