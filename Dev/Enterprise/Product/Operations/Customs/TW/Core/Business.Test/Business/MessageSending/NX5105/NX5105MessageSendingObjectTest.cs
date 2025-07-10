using System;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX5105MessageSendingObject))]
	sealed class NX5105MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region Message Type
		[ExpectNoExceptions]
		public void TestDeclaration_MessageType()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			var messageSendingObject1 = new NX5105MessageSendingObject(entryHeader);
			var messageSendingObject2 = new NX5105MessageSendingObject(entryHeader, false, true);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(messageSendingObject1.MessageType, NUnit.Framework.Is.EqualTo("ICD").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(messageSendingObject2.MessageType, NUnit.Framework.Is.EqualTo("CAA").Using(CustomComparers.TypeComparison));
			});
		}
		#endregion

		#region Acceptance Date Time
		[TestDate(2019, 06, 25)]
		[ExpectNoExceptions]
		public void TestDeclaration_AcceptanceDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 06, 24);
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(new ZDate(2019, 06, 24)));
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(new ZDate(2019, 06, 25)));
		}

		#endregion
		#region Authentication
		[ExpectNoExceptions]
		public void TestDeclaration_Authentication()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Authentication, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Declaration.Authentication should be");
		}

		#endregion
		#region ID
		[ExpectNoExceptions]
		public void TestDeclaration_ID()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryHeader.EntryNumber = "";
			NUnit.Framework.Assert.That(messageSendingObject.ID, NUnit.Framework.Is.EqualTo(MessageConstants.EntryNumberPlaceHolder).Using(CustomComparers.TypeComparison), "Declaration.ID should be");
			entryHeader.CH_Status = "AWO";
			entryHeader.EntryNumber = "AAG20812340001";
			NUnit.Framework.Assert.That(messageSendingObject.ID, NUnit.Framework.Is.EqualTo("AAG20812340001").Using(CustomComparers.TypeComparison), "Declaration.ID should be");
		}

		#endregion

		#region Invoice Amount
		[TestDate(2016, 01, 01)]
		[ExpectNoExceptions]
		public void TestDeclaration_InvoiceAmount()
		{
			var date = new ZDateTime(2020, 01, 01);
			var usd = Core.Constants.CurrencyCodes.UnitedStates;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OwnerRef = "B07252327";
			jobDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			jobDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = date;

			var groupInvoice = jobDeclaration.JobComInvoiceGroupHeaders[0];
			var groupCharges = groupInvoice.Charges;

			var invoice = groupInvoice.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 14096m;
			invoice.JZ_RX_NKInvoice_Currency = usd;
			invoice.JZ_IncoTerm = "FOB";
			var charges = invoice.Charges;

			var deduction = groupCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 704.8m, usd);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_EnteredUnitPrice = 14096m;

			jobDeclaration.ResumeApportionment();
			jobDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			var entryLine1 = invoiceLine1.CusEntryLine;

			CombineAssertions(() =>
			{
				AssertEquals("Balance", 0m, invoice.JZ_Calc_Balance);
				AssertEquals("Unit Price Term", "FOB", entryHeader.CH_DeclarationIncoterm);
				AssertEquals("Invoice Total", 14096m, entryHeader.CH_TotalInvoiceAmountInInvoiceCurrency);
				AssertEquals("Invoice Line Total", 14096m, entryHeader.CH_InvoiceLineTotal.Amount);
				AssertEquals("Declaration Invoice Total (16)", 13391.2m, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				AssertEquals("Freight (17)", 0m, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				AssertEquals("Insurance (18)", 0m, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				AssertEquals("Additions (19)", 0m, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				AssertEquals("Deductions (20)", 704.8m, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				AssertEquals("FOB (21)", 14096m, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				AssertEquals("Message - Declaration Invoice Total (16)", 14096m, messageSendingObject.InvoiceAmount);
			});
		}

		#endregion
		#region Total Gross Mass Measure
		[ExpectNoExceptions]
		public void TestDeclaration_TotalGrossMassMeasure()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			declaration.JE_TotalWeight = 1000m;
			declaration.JE_TotalWeightUnit = "LB";
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.TotalGrossMassMeasure, NUnit.Framework.Is.EqualTo(453.59237m).Using(CustomComparers.TypeComparison), "Declaration.TotalGrossMassMeasure should be 453.59237");
			declaration.JE_TotalWeightUnit = "KG";
			NUnit.Framework.Assert.That(messageSendingObject.TotalGrossMassMeasure, NUnit.Framework.Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Declaration.TotalGrossMassMeasure should be 1000");
		}

		#endregion
		#region Total Package Quantity
		[ExpectNoExceptions]
		public void TestDeclaration_TotalPackageQuantity()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryHeader.Declaration.JE_TotalNoOfPacks = 126;
			NUnit.Framework.Assert.That(messageSendingObject.TotalPackageQuantity, NUnit.Framework.Is.EqualTo(126).Using(CustomComparers.TypeComparison), "Declaration.TotalPackageQuantity should be");
		}

		#endregion
		#region Associated Government Procedure Code
		[ExpectNoExceptions]
		public void TestDeclaration_AssociatedGovernmentProcedureCode()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryHeader.EntryInstruction.CEI_ExamMode = ExamModeList.Codes.FactoryInspection;
			NUnit.Framework.Assert.That(messageSendingObject.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo(ExamModeList.Codes.FactoryInspection).Using(CustomComparers.TypeComparison), "Declaration.AssociatedGovernmentProcedureCode should be");
		}

		#endregion

		#region Combined Note
		[ExpectNoExceptions]
		public void TestDeclaration_CombinedNote()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.CombinedNote, NUnit.Framework.Is.EqualTo(ZString.Empty), "Declaration.CombinedNote should be");
			messageSendingObject = new NX5105MessageSendingObject(entryHeader, true);
			NUnit.Framework.Assert.That(messageSendingObject.CombinedNote, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "Declaration.CombinedNote should be");
			messageSendingObject = new NX5105MessageSendingObject(entryHeader, true, true);
			NUnit.Framework.Assert.That(messageSendingObject.CombinedNote, NUnit.Framework.Is.EqualTo(ZString.Empty), "Declaration.CombinedNote should be");
			messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.CombinedNote, NUnit.Framework.Is.EqualTo(ZString.Empty), "Declaration.CombinedNote should be");
		}
		#endregion

		#region Type Code
		[ExpectNoExceptions]
		public void TestDeclaration_TypeCode()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryHeader.EntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(messageSendingObject.TypeCode, NUnit.Framework.Is.EqualTo(Constants.DeclarationTypes.Import.B6).Using(CustomComparers.TypeComparison), "Declaration.TypeCode should be");
			entryHeader.EntryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			NUnit.Framework.Assert.That(messageSendingObject.TypeCode, NUnit.Framework.Is.EqualTo(Constants.DeclarationTypes.Import.G1).Using(CustomComparers.TypeComparison), "Declaration.TypeCode should be");
		}

		#endregion
		#region Additional Information
		[ExpectNoExceptions]
		public void TestDeclaration_AdditionalInformation()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			var duplicate1 = entryHeader.EntryInstruction.DeclarationDuplicates.AddNew();
			duplicate1.CY_Code = "3";
			duplicate1.CY_Data = "1";
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "Declaration.AdditionalInformation.CopyQuantity should be");

			var duplicate2 = entryHeader.EntryInstruction.DeclarationDuplicates.AddNew();
			duplicate2.CY_Code = "2";
			duplicate2.CY_Data = "4";
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(5).Using(CustomComparers.TypeComparison), "Declaration.AdditionalInformation.CopyQuantity should be");

			var duplicate3 = entryHeader.EntryInstruction.DeclarationDuplicates.AddNew();
			duplicate3.CY_Code = "4";
			duplicate3.CY_Data = "14";
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(19).Using(CustomComparers.TypeComparison), "Declaration.AdditionalInformation.CopyQuantity should be");

			var duplicate5 = entryHeader.EntryInstruction.DeclarationDuplicates.AddNew();
			duplicate5.CY_Code = "5";
			duplicate5.CY_Data = "1";
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.CopyQuantity, NUnit.Framework.Is.EqualTo(20).Using(CustomComparers.TypeComparison), "Declaration.AdditionalInformation.CopyQuantity should be");
		}

		#endregion
		#region Agent
		[ExpectNoExceptions]
		public void TestDeclaration_Agent()
		{
			var organizationWithAEO = Factory.NewWithValidTestData<OrgHeader>();
			organizationWithAEO.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			Factory.Save();
			var entryHeader = GetEntryHeaderWithMinimumData();
			entryHeader.Declaration.JE_CustomsProfile = "CBK1123-8";
			entryHeader.Declaration.JE_OA_DeclarantAddress = organizationWithAEO.MainAddress.PK;
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryHeader.EntryInstruction.CEI_BoxNumber = "123";
			NUnit.Framework.Assert.That(messageSendingObject.Agent.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "Declaration.Agent.ID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Agent.RoleCode, NUnit.Framework.Is.EqualTo("CB").Using(CustomComparers.TypeComparison), "Declaration.Agent.RoleCode should be");
			NUnit.Framework.Assert.That(messageSendingObject.Agent.SubBoxID, NUnit.Framework.Is.EqualTo("8").Using(CustomComparers.TypeComparison), "Declaration.Agent.SubBoxID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Agent.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison), "Declaration.Agent.LPCOAuthorizedParty.ID should be");
		}

		#endregion
		#region Border Transport Means
		[ExpectNoExceptions]
		public void TestDeclaration_BorderTransportMeans()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.BorderTransportMeans.GetType(), NUnit.Framework.Is.EqualTo(typeof(NX5105Declaration_BorderTransportMeans)));
		}

		#endregion
		#region Currency Exchange
		[ExpectNoExceptions]
		public void TestDeclaration_CurrencyExchange()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.CurrencyExchange.GetType(), NUnit.Framework.Is.EqualTo(typeof(NX5105Declaration_CurrencyExchange)));
		}

		#endregion
		#region Duty Tax Fee
		[ExpectNoExceptions]
		public void TestDeclaration_DutyTaxFee()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.DutyTaxFee.GetType(), NUnit.Framework.Is.EqualTo(typeof(NX5105Declaration_DutyTaxFee)));
		}

		#endregion
		#region Goods Shipment
		[ExpectNoExceptions]
		public void TestDeclaration_GoodsShipment()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment.GetType(), NUnit.Framework.Is.EqualTo(typeof(NX5105Declaration_GoodsShipment)));
		}

		#endregion
		#region Government Procedure Descriptions
		[TestDate(2020, 07, 20)]
		[ExpectNoExceptions]
		public void TestDeclaration_GovernmentProcedureDescriptions()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			var entryInstruction = entryHeader.EntryInstruction;
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryInstruction.TW_TradersRemarks = ZString.Replicate('Z', 256);
			var governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(1));
			entryInstruction.TW_TradersRemarks = ZString.Replicate('Z', 257);
			governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(2));
			entryInstruction.TW_TradersRemarks = ZString.Replicate('Z', 510);
			governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.ElementAt(0).Length, NUnit.Framework.Is.EqualTo(256));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.ElementAt(1).Length, NUnit.Framework.Is.EqualTo(254));
			entryInstruction.TW_TradersRemarks = ZString.Replicate('Z', 513);
			governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.ElementAt(0).Length, NUnit.Framework.Is.EqualTo(256));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.ElementAt(1).Length, NUnit.Framework.Is.EqualTo(256));
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "T1";
			orgHeader.OH_FullName = "test org";
			var poaDoc = orgHeader.RequiredDocuments.AddNew();
			poaDoc.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
			poaDoc.EQ_DocType = RefDocTypes.PowerOfAttorney;
			poaDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			poaDoc.EQ_DateReceived = new ZDateTimeOffset(2020, 7, 20);
			poaDoc.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			poaDoc.EQ_RN_NKRelatedCountry = CountryCodes.Taiwan;
			poaDoc.EQ_DocNumber = "111111";
			var poaDocAttr = poaDoc.Attributes[JobRequiredDocAttribTypeList.Codes.CustomsDistrict];
			poaDocAttr.D0_AttribValue = "A";
			entryInstruction.CEI_CustomsOffice = "AC";
			entryHeader.Declaration.JE_MessageType = "IMP";
			entryHeader.Declaration.JE_OH_Importer = orgHeader.PK;
			entryInstruction.TW_OverrideTradersRemarks = false;
			entryInstruction.TW_TradersRemarks += "\r\nTEST\r\n hello world!";
			governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.First(), NUnit.Framework.Is.EqualTo("常年(長期)委任報關核准文號：111111\r\n起：109年07月20日\r\n迄：116年07月20日\r\nTEST\r\n hello world!").Using(CustomComparers.TypeComparison));
			poaDocAttr.D0_AttribValue = "B";
			entryInstruction.TW_OverrideTradersRemarks = false;
			entryInstruction.TW_TradersRemarks += "\r\nTEST\r\n hello world!";
			governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.First(), NUnit.Framework.Is.EqualTo("\r\nTEST\r\n hello world!").Using(CustomComparers.TypeComparison));
			poaDocAttr.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			poaDocAttr.D0_AttribValue = "A";
			entryInstruction.TW_OverrideTradersRemarks = false;
			entryInstruction.TW_TradersRemarks += "\r\nTEST\r\n hello world!";
			governmentProcedureDescriptions = messageSendingObject.GovernmentProcedureDescriptions;
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(governmentProcedureDescriptions.First(), NUnit.Framework.Is.EqualTo("\r\nTEST\r\n hello world!").Using(CustomComparers.TypeComparison));
		}

		#endregion
		#region Importer
		[ExpectNoExceptions]
		public void TestDeclaration_ImporterAndMessageOwner()
		{
			SetupOrganizations();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			declaration.JE_OH_Importer = organization1.PK;
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Importer.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "Importer.ID should be");
			NUnit.Framework.Assert.That((messageSendingObject as NX5105MessageSendingObject).GetMessageOwner(), NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "MessageOwner should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.Name, NUnit.Framework.Is.EqualTo("OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Importer.Name should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), "Importer.ChineseName should be");
			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			var organization2Address = organization2.Addresses.AddNew();
			organization2Address.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "123", "TW");
			entryInstruction.CEI_OA_Warehouse2 = organization2Address.PK;
			entryInstruction.CEI_Style = "D7";
			messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Importer.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("TPCREGNO").Using(CustomComparers.TypeComparison), "Importer.PaymentOnAccountBusinessID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(vATCusCode);
			messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Importer.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
			organization1.MainAddress.CustomsCodes.Delete(pASCusCode);
			messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Importer.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "Importer.TypeCode should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.Communications.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("PHONE").Using(CustomComparers.TypeComparison), "Importer.Communications[0].ID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.Communications.ElementAt(0).TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Importer.Communications[0].TypeID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.Communications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("EMAIL").Using(CustomComparers.TypeComparison), "Importer.Communications[1].ID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.Communications.ElementAt(1).TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison), "Importer.Communications[1].TypeID should be");
			NUnit.Framework.Assert.That(messageSendingObject.Importer.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison), "Importer.LPCOAuthorizedParty.ID should be");
			NUnit.Framework.Assert.That((messageSendingObject as NX5105MessageSendingObject).GetMessageOwner(), NUnit.Framework.Is.EqualTo("PIDREGNO").Using(CustomComparers.TypeComparison), "MessageOwner should be");
		}

		OrgHeader organization1;
		OrgHeader organization2;
		OrgCusCode vATCusCode;
		OrgCusCode cCPCusCode;
		OrgCusCode pASCusCode;
		OrgCusCode pIDCusCode;
		void SetupOrganizations()
		{
			organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			var contact = organization1.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";
			var address1 = organization1.Addresses[0];
			address1.OA_RN_NKCountryCode = "TW";
			address1.OA_CompanyNameOverride = "OVERRIDEN COMPANY NAME";
			address1.OA_Language = "EN";
			address1.OA_Address1 = "ADDRESS 1";
			address1.OA_Address2 = "ADDRESS 2";
			address1.OA_Phone = "PHONE";
			address1.OA_Email = "EMAIL";
			var entranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			entranslatedAddress1.OTA_Language = "EN";
			entranslatedAddress1.OTA_Address1 = "EN OTA ADDRESS 1";
			entranslatedAddress1.OTA_Address2 = "EN OTA ADDRESS 2";
			var zhTWtranslatedAddress1 = address1.TranslatedAddresses.AddNew();
			zhTWtranslatedAddress1.OTA_Language = Core.SharedConstants.Languages.ChineseTraditional;
			zhTWtranslatedAddress1.OTA_CompanyName = "TW OVERRIDEN COMPANY NAME";
			zhTWtranslatedAddress1.OTA_Address1 = "TW OTA ADDRESS 1";
			zhTWtranslatedAddress1.OTA_Address2 = "TW OTA ADDRESS 2";
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("TPC", "TPCREGNO", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.AEO, "123465789", "TW");
			vATCusCode = organization1.CustomsCodes.AddNew();
			vATCusCode.OK_RN_NKCodeCountry = "TW";
			vATCusCode.OK_CodeType = "VAT";
			vATCusCode.OK_CustomsRegNo = "123465789";
			cCPCusCode = address1.CustomsCodes.AddNew();
			cCPCusCode.OK_RN_NKCodeCountry = "TW";
			cCPCusCode.OK_CodeType = "CCP";
			cCPCusCode.OK_CustomsRegNo = "987654321";
			pASCusCode = organization1.CustomsCodes.AddNew();
			pASCusCode.OK_RN_NKCodeCountry = "TW";
			pASCusCode.OK_CodeType = "PAS";
			pASCusCode.OK_CustomsRegNo = "PASREGNO";
			pIDCusCode = organization1.CustomsCodes.AddNew();
			pIDCusCode.OK_RN_NKCodeCountry = "TW";
			pIDCusCode.OK_CodeType = "PID";
			pIDCusCode.OK_CustomsRegNo = "PIDREGNO";
			address1.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "999", "TW");
			organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "Org2";
			organization2.OH_RL_NKClosestPort = "TW";
			Factory.Save();
		}

		#endregion
		#region Packaging
		[ExpectNoExceptions]
		public void TestDeclaration_Packaging()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(messageSendingObject.Packaging, NUnit.Framework.Is.TypeOf(typeof(NX5105Declaration_DeclarationPackaging)));
		}

		#endregion
		#region Representative Person Name
		[ExpectNoExceptions]
		public void TestDeclaration_RepresentativePersonName()
		{
			var newBroker = Factory.NewWithValidTestData<GlbStaff>();
			var brkCertificate = newBroker.Certificates.AddNew();
			brkCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BRK;
			brkCertificate.XZ_RN_NKCountryOfIssuance = "TW";
			brkCertificate.XZ_RefNumber = "1234";
			brkCertificate.XZ_ExpiryOrDueDate = DateTime.Today.AddYears(1);
			Factory.Save();
			var entryHeader = GetEntryHeaderWithMinimumData();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(entryHeader);
			entryHeader.Declaration.JE_GS_NKCusAgent = newBroker.GS_Code;
			NUnit.Framework.Assert.That(messageSendingObject.RepresentativePersonName, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison), "Declaration.RepresentativePersonName");
		}

		#endregion
		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var header = Factory.New<CusEntryHeader>();
				new NX5105MessageSendingObject(header);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var header = declaration.CustomsEntryHeaders.AddNew();
				var entryInstruction = declaration.CusEntryInstruction;
				header.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = header.MergedLines.AddNew();
				entryLine.InvoiceLines.AddNew();
				new NX5105MessageSendingObject(header);
			}

			);
		}

		#region Applications
		[ExpectNoExceptions]
		public void TestApplicationsWithoutCM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			header.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = header.MergedLines.AddNew();
			entryLine.InvoiceLines.AddNew();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(header);
			NUnit.Framework.Assert.That(messageSendingObject.Applications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IApplication>)));
		}

		[ExpectNoExceptions]
		public void TestApplicationsWithCM()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			var controllingMsgHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			var controllingMsgHeaderNotLink = entryInstruction.ControllingMessageHeaders.AddNew();
			header.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = header.MergedLines.AddNew();
			var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			INX5105Declaration messageSendingObject = new NX5105MessageSendingObject(header, true);
			NUnit.Framework.Assert.That(messageSendingObject.Applications.Any(), NUnit.Framework.Is.EqualTo(false));
			invoiceLine.InvoiceLineRelatedControllingMsgHeadersGenPivots.AddPivotFor(controllingMsgHeader);
			messageSendingObject = new NX5105MessageSendingObject(header, true);
			NUnit.Framework.Assert.That(messageSendingObject.Applications, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<IApplication>)));
			NUnit.Framework.Assert.That(messageSendingObject.Applications.Count(), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(messageSendingObject.Applications.First(), NUnit.Framework.Is.TypeOf<NX5105CMApplication>());
		}

		#endregion
		CusEntryHeader GetEntryHeaderWithMinimumData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return entryHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NX5105MessageSendingObject(GetEntryHeaderWithMinimumData());
		}

		[ExpectNoExceptions]
		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name == "MessageType")
			{
				NUnit.Framework.Assert.That((ZString)info.Value, NUnit.Framework.Is.EqualTo("ICD").Using(CustomComparers.TypeComparison));
			}
			else
			{
				base.TestBizObjectField(info);
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationIfNodeValueIsEmpty()
		{
			SetupOrganizations();
			new TestTWCreator(Factory).CreateAndSetProxyOrganization();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryInstruction.CEI_ExamMode = "";
			entryInstruction.CEI_WaiverOfExemption = false;
			entryInstruction.CEI_PrintDutyMemo = false;
			declaration.JE_SLD = "";
			declaration.JE_VesselArrivalReg = "";
			declaration.JE_SplitMark = false;
			invoiceLine.JI_Model = "";
			invoiceLine.JI_BrandName = "";
			invoiceLine.CitesPermit = "";
			invoiceLine.HighTechLicense = "";
			invoiceLine.JI_TariffAdditionalCode = "";
			entryInstruction.CEI_PackageDescription = "";
			entryInstruction.CEI_IsCoPackaged = false;
			entryInstruction.TW_TradersRemarks = "";
			declaration.JE_OH_Supplier = organization2.PK;
			INX5105Declaration message = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(message.Authentication, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.CombinedNote, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.DutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.DutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.BorderTransportMeans.Registration, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem, NUnit.Framework.Is.Not.EqualTo(default(IConsignmentItem)));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem.Split, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var goodsItem = message.GoodsShipment.GovernmentAgencyGoodsItems.FirstOrDefault();
			NUnit.Framework.Assert.That(goodsItem.Commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.BarCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.ChineseDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.FTATariffCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.DutyTaxFee.DutyRegimeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GovernmentProcedureDescriptions.Any(), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(message.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.Packaging.Combination, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Authentication", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_AssociatedGovernmentProcedureCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_CombinedNote", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyExemptionWaiverNote", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyMemoPrinted", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_ManifestSerialNumber", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:BorderTransportMeans/a:tw_Registration", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem/a:tw_Split", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:CommercialCategorizationID", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:GoodsGroupNameCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Name", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_BarCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_ChineseDescription", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_CITESImportPermitID", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_FTATariffCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_SHTCImportPermitID", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_TariffCodeExtensionCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:DutyTaxFee/a:DutyRegimeCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:ID", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_ChineseName", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_CustomsControlID", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure/a:Description", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:PackagingMaterialDescription", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:tw_Combination", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)));
			entryInstruction.CEI_ExamMode = "A";
			entryInstruction.CEI_WaiverOfExemption = true;
			entryInstruction.CEI_PrintDutyMemo = true;
			declaration.JE_SLD = "A";
			declaration.JE_VesselArrivalReg = "A";
			declaration.JE_SplitMark = true;
			invoiceLine.JI_Model = "A";
			invoiceLine.JI_BrandName = "A";
			invoiceLine.CitesPermit = "A";
			invoiceLine.HighTechLicense = "A";
			invoiceLine.JI_NDescription = "描述";
			invoiceLine.JI_TariffAdditionalCode = "A";
			entryInstruction.CEI_PackageDescription = "A";
			entryInstruction.CEI_IsCoPackaged = true;
			entryInstruction.TW_TradersRemarks = ZString.Replicate('Z', 513);
			declaration.JE_OH_Supplier = organization1.PK;
			entryInstruction.CEI_Style = "F2";
			entryInstruction.CEI_OA_Warehouse = organization1.MainAddress.PK;
			message = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(message.Authentication, NUnit.Framework.Is.EqualTo(ZString.Empty), "Authentication");
			NUnit.Framework.Assert.That(message.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "AssociatedGovernmentProcedureCode");
			NUnit.Framework.Assert.That(message.CombinedNote, NUnit.Framework.Is.EqualTo(ZString.Empty), "CombinedNote");
			NUnit.Framework.Assert.That(message.DutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyExemptionWaiverNote");
			NUnit.Framework.Assert.That(message.DutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMemoPrinted");
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.ManifestSerialNumbe");
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.BorderTransportMeans.Registration, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.BorderTransportMeans.Registration");
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem, NUnit.Framework.Is.Not.EqualTo(default(IConsignmentItem)));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem.Split, NUnit.Framework.Is.EqualTo("P").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.ConsignmentItem.Split");
			goodsItem = message.GoodsShipment.GovernmentAgencyGoodsItems.FirstOrDefault();
			NUnit.Framework.Assert.That(goodsItem.Commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.CommercialCategorizationID");
			NUnit.Framework.Assert.That(goodsItem.Commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "goodsItem.Commodity.GoodsGroupNameCode");
			NUnit.Framework.Assert.That(goodsItem.Commodity.Name, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.Name");
			NUnit.Framework.Assert.That(goodsItem.Commodity.BarCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "goodsItem.Commodity.BarCode");
			NUnit.Framework.Assert.That(goodsItem.Commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.ChineseDescription");
			NUnit.Framework.Assert.That(goodsItem.Commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.CITESImportPermitID");
			NUnit.Framework.Assert.That(goodsItem.Commodity.FTATariffCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "goodsItem.Commodity.FTATariffCode");
			NUnit.Framework.Assert.That(goodsItem.Commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.SHTCImportPermitID");
			NUnit.Framework.Assert.That(goodsItem.Commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo(ZString.Empty), "goodsItem.Commodity.TariffCodeExtensionCode");
			NUnit.Framework.Assert.That(goodsItem.Commodity.DutyTaxFee.DutyRegimeCode, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.DutyTaxFee.DutyRegimeCode");
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ID, NUnit.Framework.Is.EqualTo("123465789").Using(CustomComparers.TypeComparison), "message.GoodsShipment.Seller.ID");
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ChineseName, NUnit.Framework.Is.EqualTo("TW OVERRIDEN COMPANY NAME").Using(CustomComparers.TypeComparison), " message.GoodsShipment.Seller.ChineseName");
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo("999").Using(CustomComparers.TypeComparison), "message.GoodsShipment.Seller.CustomsControlID");
			NUnit.Framework.Assert.That(message.GovernmentProcedureDescriptions.Any(), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(message.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("A").Using(CustomComparers.TypeComparison), "message.Packaging.PackagingMaterialDescription");
			NUnit.Framework.Assert.That(message.Packaging.Combination, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison), "message.Packaging.Combination");
			CombineAssertions(() =>
			{
				xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
				xmlDocument.LoadXml(xml);
				nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
				nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Authentication", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.Authentication - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_AssociatedGovernmentProcedureCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.tw_AssociatedGovernmentProcedureCode - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_CombinedNote", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.tw_CombinedNote - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyExemptionWaiverNote", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.DutyTaxFee.tw_DutyExemptionWaiverNote - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyMemoPrinted", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.DutyTaxFee.tw_DutyMemoPrinted - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Consignment - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_ManifestSerialNumber", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Consignment.tw_ManifestSerialNumber - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:BorderTransportMeans/a:tw_Registration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Consignment.BorderTransportMeans.tw_Registration - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Consignment.ConsignmentItem - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem/a:tw_Split", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Consignment.ConsignmentItem.tw_Split - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:CommercialCategorizationID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.CommercialCategorizationID - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:GoodsGroupNameCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.GoodsGroupNameCode - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Name", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_BarCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.tw_BarCode - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_ChineseDescription", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.tw_ChineseDescription - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_CITESImportPermitID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.tw_CITESImportPermitID - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_FTATariffCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.tw_FTATariffCode - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_SHTCImportPermitID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.tw_SHTCImportPermitID - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_TariffCodeExtensionCode", nameSpace), NUnit.Framework.Is.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.tw_TariffCodeExtensionCode - should be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:DutyTaxFee/a:DutyRegimeCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.DutyRegimeCode - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Seller - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:ID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Seller.ID - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_ChineseName", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Seller.tw_ChineseName - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_CustomsControlID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GoodsShipment.Seller.tw_CustomsControlID - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GovernmentProcedure - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure/a:Description", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.GovernmentProcedure.Description - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.Packaging - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:PackagingMaterialDescription", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.Packaging.PackagingMaterialDescription - should not be [null]");
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:tw_Combination", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(XmlNode)), "Declaration.Packaging.tw_Combination - should not be [null]");
			}

			);
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
			cusNum1.CE_RN_NKCountryCode = CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			Factory.Save();
			cusHead1.CH_EntryStatus = "A";
			var action = new NX5105MessageSendingObject(cusHead1);
			var list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.ContainsCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(list.ContainsCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Update));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
			cusHead1.CH_EntryStatus = "";
			list = action.ActionList;
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.ContainsCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(list.ContainsCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Create), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Create));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Update), NUnit.Framework.Is.EqualTo(ActionCodeList.Descriptions.Update));
			NUnit.Framework.Assert.That(list.GetDescriptionFromCode(ActionCodeList.Codes.Delete), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestImporter()
		{
			SetupOrganizations();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = organization1.PK;
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Importer, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
			organization1.OH_RL_NKClosestPort = "US";
			NUnit.Framework.Assert.That(sendingObject.Importer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			declaration.JE_OH_Importer = ZGuid.Empty;
			NUnit.Framework.Assert.That(sendingObject.Importer, NUnit.Framework.Is.EqualTo(default(IPartyDetails)));
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			NUnit.Framework.Assert.That(sendingObject.Importer, NUnit.Framework.Is.Not.EqualTo(default(IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestImporterCustomsControlID()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Org1";
			org.OH_RL_NKClosestPort = "TW";
			var epzCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.EPZ, "EPZ12345", "TW");
			var cbfCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.CBF, "CBF54321", "TW");
			var ftzCustomsCode = org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.TaiwanCodeTypes.FTZ, "FTZ13579", "TW");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = "EPZ";
			importerDocumentaryAddress.CBPCode = "12345678";
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.CH_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = declaration.CusEntryInstruction.PK;
			var sendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Importer.CustomsControlID, NUnit.Framework.Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
			importerDocumentaryAddress.E2_AddressOverride = false;
			importerDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;
			sendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Importer.CustomsControlID, NUnit.Framework.Is.EqualTo("EPZ12345").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(epzCustomsCode);
			sendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Importer.CustomsControlID, NUnit.Framework.Is.EqualTo("CBF54321").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(cbfCustomsCode);
			sendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Importer.CustomsControlID, NUnit.Framework.Is.EqualTo("FTZ13579").Using(CustomComparers.TypeComparison));
			org.MainAddress.CustomsCodes.Delete(ftzCustomsCode);
			sendingObject = new NX5105MessageSendingObject(entryHeader);
			NUnit.Framework.Assert.That(sendingObject.Importer.CustomsControlID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}
	}
}
