using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	class JobDeclarationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHint()
		{
			#region Setup Tariffs and ADD/CVD

			ZString tariffCode1 = "9010100010";
			USCTariffTest.CreateTariff(Factory, tariffCode1);
			ZString tariffCode2 = "8010010010";
			USCTariffTest.CreateTariff(Factory, tariffCode2);
			ZString tariffCode3 = "7010000010";
			USCTariffTest.CreateTariff(Factory, tariffCode3);
			ZString tariffCode4 = "6010000010";
			USCTariffTest.CreateTariff(Factory, tariffCode4);

			var acCase1 = Factory.New<USCACCase>();
			acCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase1.U5_CaseNumber = "AA0101";
			acCase1.U5_ISOCountryCode = Core.Constants.CountryCodes.Australia;
			acCase1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff1 = acCase1.CaseTariffs.AddNew();
			caTariff1.U9_TariffNumber = tariffCode1.Left(4);

			var acCase2 = Factory.New<USCACCase>();
			acCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase2.U5_CaseNumber = "AA0102";
			acCase2.U5_ISOCountryCode = Core.Constants.CountryCodes.China;
			acCase2.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff2 = acCase2.CaseTariffs.AddNew();
			caTariff2.U9_TariffNumber = tariffCode2.Left(5);

			var acCase3 = Factory.New<USCACCase>();
			acCase3.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase3.U5_CaseNumber = "AA0103";
			acCase3.U5_ISOCountryCode = Core.Constants.CountryCodes.NewZealand;
			acCase3.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff3 = acCase3.CaseTariffs.AddNew();
			caTariff3.U9_TariffNumber = tariffCode3.Left(6);

			var acCase4 = Factory.New<USCACCase>();
			acCase4.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			acCase4.U5_CaseNumber = "AA0104";
			acCase4.U5_ISOCountryCode = Core.Constants.CountryCodes.Japan;
			acCase4.U5_CaseStatusDate = ZDateTime.BrettsBirthday;
			var caTariff4 = acCase4.CaseTariffs.AddNew();
			caTariff4.U9_TariffNumber = tariffCode4.Left(7);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.JE_TotalNoOfPacksPackType = "";

			var invoice1 = declaration.Invoices.AddNew();
			PopulateInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), Core.Constants.CountryCodes.Australia, tariffCode1);
			PopulateInvoiceLine(invoice1.JobComInvoiceLines.AddNew(), Core.Constants.CountryCodes.China, tariffCode2);

			var invoice2 = declaration.Invoices.AddNew();
			PopulateInvoiceLine(invoice2.JobComInvoiceLines.AddNew(), Core.Constants.CountryCodes.NewZealand, tariffCode3);
			PopulateInvoiceLine(invoice2.JobComInvoiceLines.AddNew(), Core.Constants.CountryCodes.Japan, tariffCode4);

			var deliveryOrder1 = declaration.DeliveryOrderHeaders.AddNew();
			var deliveryOrder1Line1 = deliveryOrder1.DeliveryOrderLines.AddNew();
			deliveryOrder1Line1.US_GoodsDescription = "A";
			var deliveryOrder1Line2 = deliveryOrder1.DeliveryOrderLines.AddNew();
			deliveryOrder1Line2.US_GoodsDescription = "A";
			var deliveryOrder2 = declaration.DeliveryOrderHeaders.AddNew();
			var deliveryOrder2Line1 = deliveryOrder2.DeliveryOrderLines.AddNew();
			deliveryOrder2Line1.US_GoodsDescription = "A";
			var deliveryOrder2Line2 = deliveryOrder2.DeliveryOrderLines.AddNew();
			deliveryOrder2Line2.US_GoodsDescription = "A";

			var accChgCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			accChgCode1.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			accChgCode1.AC_Code = "TST";
			accChgCode1.AC_GC = GlbCompany.CurrentCompany.PK;

			var accChgCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			accChgCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			accChgCode2.AC_Code = "TS2";
			accChgCode2.AC_GC = GlbCompany.CurrentCompany.PK;

			var accChgCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			accChgCode3.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			accChgCode3.AC_Code = "TS3";
			accChgCode3.AC_GC = GlbCompany.CurrentCompany.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_ParentTableCode = "JE";

			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = jobHeader.PK;
			charge1.JR_AC = accChgCode1.PK;
			charge1.JR_LocalSellAmt = 110m;
			charge1.JR_OSSellAmt = 110m;

			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = jobHeader.PK;
			charge2.JR_AC = accChgCode2.PK;
			charge2.JR_LocalSellAmt = 150m;
			charge2.JR_OSSellAmt = 150m;

			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_JH = jobHeader.PK;
			charge3.JR_AC = accChgCode3.PK;
			charge3.JR_LocalSellAmt = 80m;
			charge3.JR_OSSellAmt = 80m;

			var accTransHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransHeader.AH_JH = jobHeader.PK;
			accTransHeader.AH_Ledger = "AR";
			accTransHeader.AH_TransactionCategory = "DBT";
			accTransHeader.AH_OutstandingAmount = 110m;
			accTransHeader.AH_InvoiceAmount = 110m;
			accTransHeader.AH_TransactionType = TransactionTypes.Invoice;

			var accTransLine = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine.AL_AH = accTransHeader.PK;
			accTransLine.AL_LineAmount = 110m;
			accTransLine.AL_OSAmount = 110m;
			accTransLine.AL_AC = accChgCode1.PK;
			accTransLine.AL_JH = jobHeader.PK;
			accTransLine.AL_RevRecognitionType = "CUS";
			charge1.JR_AL_ARLine = accTransLine.PK;

			var accTransLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine2.AL_AH = accTransHeader.PK;
			accTransLine2.AL_LineAmount = 150m;
			accTransLine2.AL_OSAmount = 150m;
			accTransLine2.AL_AC = accChgCode2.PK;
			accTransLine2.AL_JH = jobHeader.PK;
			accTransLine2.AL_RevRecognitionType = "CUS";
			charge2.JR_AL_ARLine = accTransLine2.PK;

			var accTransLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			accTransLine3.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			accTransLine3.AL_AH = accTransHeader.PK;
			accTransLine3.AL_LineAmount = 80m;
			accTransLine3.AL_OSAmount = 80m;
			accTransLine3.AL_AC = accChgCode3.PK;
			accTransLine3.AL_JH = jobHeader.PK;
			accTransLine3.AL_RevRecognitionType = "CUS";
			charge3.JR_AL_ARLine = accTransLine3.PK;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			AddChargeTypeRegistry(chargeTypeSettings, accChgCode1.PK, Core.Constants.USCustoms.FeeCodes.Duty);
			AddChargeTypeRegistry(chargeTypeSettings, accChgCode2.PK, Core.Constants.USCustoms.FeeCodes.Avocado);
			AddChargeTypeRegistry(chargeTypeSettings, accChgCode3.PK, Core.Constants.USCustoms.FeeCodes.HMF);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationReload = newFactory.Load<JobDeclaration>(declaration.PK);
			declarationReload.LoadChildEditableObjects();

			bool foundInvoiceHeaders = false;
			bool foundInvoiceLines = false;
			bool foundCusAddInfos = false;
			foreach (var tableSelect in newFactory.TableSelects)
			{
				switch (tableSelect.TableName)
				{
					case CusAddInfoSchema.Constants.TableName:
						AssertEquals(2, tableSelect.Value);
						foundCusAddInfos = true;
						break;
					case JobComInvoiceHeaderSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundInvoiceHeaders = true;
						break;
					case JobComInvoiceLineSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundInvoiceLines = true;
						break;
				}
			}
			Assert("Found Invoice Headers", foundInvoiceHeaders);
			Assert("Found Invoice Lines", foundInvoiceLines);
			Assert("Found CusAddInfos", foundCusAddInfos);

			AssertEquals(2, declarationReload.Invoices.Count);
			AssertEquals(4, declarationReload.InvoiceLines.Count);
			foreach (JobComInvoiceLine invoiceLine in declarationReload.InvoiceLines)
			{
				AssertEquals(2, invoiceLine.FDAs.Count);
				AssertEquals(2, invoiceLine.DOTs.Count);
			}
			AssertEquals(2, declarationReload.DeliveryOrderHeaders.Count);
			foreach (DeliveryOrderHeader deliveryOrder in declarationReload.DeliveryOrderHeaders)
			{
				AssertEquals(2, deliveryOrder.DeliveryOrderLines.Count);
			}
			AssertEquals(340m, declarationReload.TotalBilledAmount);
			AssertEquals(110m, declarationReload.TotalInvoicedAmount);
			AssertEquals(110m, declarationReload.TotalOutstandingAmount);

			foundInvoiceHeaders = false;
			foundInvoiceLines = false;
			foundCusAddInfos = false;
			var foundJobHeader = false;
			var foundAccTransactionLines = false;
			var foundAccTransactionHeader = false;
			foreach (var tableSelect in newFactory.TableSelects)
			{
				switch (tableSelect.TableName)
				{
					case CusAddInfoSchema.Constants.TableName:
						AssertEquals(2, tableSelect.Value);
						foundCusAddInfos = true;
						break;
					case JobComInvoiceHeaderSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundInvoiceHeaders = true;
						break;
					case JobComInvoiceLineSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundInvoiceLines = true;
						break;
					case JobHeaderSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundJobHeader = true;
						break;
					case AccTransactionLinesSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundAccTransactionLines = true;
						break;
					case AccTransactionHeaderSchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundAccTransactionHeader = true;
						break;
				}
			}
			Assert("Found Invoice Headers", foundInvoiceHeaders);
			Assert("Found Invoice Lines", foundInvoiceLines);
			Assert("Found CusAddInfos", foundCusAddInfos);
			Assert("Found JobHeader", foundJobHeader);
			Assert("Found AccTransactionLines", foundAccTransactionLines);
			Assert("Found AccTransactionHeader", foundAccTransactionHeader);

			newFactory = new BusinessObjectFactory();
			declarationReload = newFactory.Load<JobDeclaration>(declaration.PK);
			declarationReload.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var foundUSCCountry = false;
			var foundUSCACCase = false;
			foreach (var tableSelect in newFactory.TableSelects)
			{
				switch (tableSelect.TableName)
				{
					case USCCountrySchema.Constants.TableName:
						AssertEquals(1, tableSelect.Value);
						foundUSCCountry = true;
						break;
					case USCACCaseSchema.Constants.TableName:
						foundUSCACCase = true;
						break;
				}
			}
			Assert("Found USCCountr", foundUSCCountry);
			Assert("Not Found USCACCase", !foundUSCACCase);
		}

		void PopulateInvoiceLine(JobComInvoiceLine invoiceLine, ZString countryOfOrigin, ZString tariffNumber)
		{
			invoiceLine.US_UC_NKCountryOfOrigin = countryOfOrigin;
			invoiceLine.JI_Tariff = tariffNumber;

			var invoiceLineFda1 = invoiceLine.FDAs.AddNew();
			invoiceLineFda1.US_SFR = "1";
			var invoiceLineFda2 = invoiceLine.FDAs.AddNew();
			invoiceLineFda2.US_SFR = "2";
			var invoiceLineDot1 = invoiceLine.DOTs.AddNew();
			invoiceLineDot1.US_DOTBoxNo = "1";
			var invoiceLineDot2 = invoiceLine.DOTs.AddNew();
			invoiceLineDot2.US_DOTBoxNo = "2";
		}

		void AddChargeTypeRegistry(EntryChargeTypeSettingCollection list, ZGuid chargeCodePK, ZString chargeType)
		{
			EntryChargeTypeSetting chargeTypeSetting = list.AddNew();
			chargeTypeSetting.ChargeType = chargeType;
			chargeTypeSetting.AC_ChargeCode = chargeCodePK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list);
		}
	}
}
