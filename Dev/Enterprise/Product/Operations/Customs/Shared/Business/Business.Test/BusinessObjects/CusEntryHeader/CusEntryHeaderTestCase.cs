using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryHeaderTestCase : TestCaseWithFactory
	{
		public void TestDeleteAnyNewMessages_DoNotLoadMessagesDuringDelete()
		{
			var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>() as BaseJobDeclaration;
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = "RCV";
			cusEntryHeader.Messages.Add(message);
			Factory.Save();
			var expected = new Dictionary<string, int>
			{
				{ EDIMessage.Schema.TableName, 1 }
			};
			AssertEquals(1, cusEntryHeader.Messages.Count);
			cusEntryHeader.Messages.Reload(true);
			AssertDbHits(expected, Factory, true);

			var newFactory = new BusinessObjectFactory();
			var newEntry = newFactory.Load<CusEntryHeader>(cusEntryHeader.PK);
			newEntry.DeleteAnyNewMessages();
			expected[EDIMessage.Schema.TableName] = 0;
			AssertDbHits(expected, newFactory, true);
		}

		public void TestIsGuaranteeDeferredPaymentUsed()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			AssertEquals("IsGuaranteeDeferredPaymentUsed always is false for base customs", false, entryLine.IsGuaranteeDeferredPaymentUsed);
			AssertEquals("IsGuaranteeDeferredPaymentUsed always is false for base customs entry lines. So should entry header's be.", false, entryHeader.IsGuaranteeDeferredPaymentUsed);
		}

		public void TestGuaranteeProperties()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(currentCountryCode);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "NO";
			procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.No;
			procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.No;
			procedure.ZZ6_ZZZ_NKDataGrouping = currentCountryCode;
			procedure.ZZ6_Description = "NO description";

			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "YE";
			procedure2.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
			procedure2.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
			procedure2.ZZ6_ZZZ_NKDataGrouping = currentCountryCode;
			procedure2.ZZ6_Description = "Yes description";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "NO";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CombineAssertions("Not affecting guarantee", () => AssertGuaranteeProperties(entryHeader, expectedHasConsumingGuarantee: false, expectedIsGuaranteeConsumed: false, expectedHasReleasingGuarantee: false, expectedIsGuaranteeReleased: false));

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "YE";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryHeader.ResetTotalsAndCachedValues();
			CombineAssertions("Affecting guarantee", () => AssertGuaranteeProperties(entryHeader, expectedHasConsumingGuarantee: true, expectedIsGuaranteeConsumed: true, expectedHasReleasingGuarantee: true, expectedIsGuaranteeReleased: true));
		}

		void AssertGuaranteeProperties(CusEntryHeader entryHeader, bool expectedHasConsumingGuarantee, bool expectedIsGuaranteeConsumed, bool expectedHasReleasingGuarantee, bool expectedIsGuaranteeReleased)
		{
			AssertEquals("HasConsumingGuaranteeProcedure", expectedHasConsumingGuarantee, entryHeader.HasConsumingGuaranteeProcedure);
			AssertEquals("IsGuaranteeConsumed", expectedIsGuaranteeConsumed, entryHeader.IsGuaranteeConsumed);
			AssertEquals("HasReleasingGuaranteeProcedure", expectedHasReleasingGuarantee, entryHeader.HasReleasingGuaranteeProcedure);
			AssertEquals("IsGuaranteeReleased", expectedIsGuaranteeReleased, entryHeader.IsGuaranteeReleased);
		}

		AccChargeCode CreateChargeCode(string code, string description, string chargeType)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "BG~" + code + "~";
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = 0m;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			return chargeCode;
		}

		AccChargeCode ChargeCodeDisbursementDefault
		{
			get
			{
				if (fChargeCodeDisbursementDefault == null)
				{
					fChargeCodeDisbursementDefault = CreateChargeCode("DSB", "Customs Disbursements Default", Constants.ChargeType.Disbursement);
				}
				return fChargeCodeDisbursementDefault;
			}
		}
		AccChargeCode fChargeCodeDisbursementDefault;

		AccChargeCode ChargeCodeDisbursementDuty
		{
			get
			{
				if (fChargeCodeDisbursementDuty == null)
				{
					fChargeCodeDisbursementDuty = CreateChargeCode("DUT", "Customs Disbursements Duty", Constants.ChargeType.Disbursement);
				}
				return fChargeCodeDisbursementDuty;
			}
		}
		AccChargeCode fChargeCodeDisbursementDuty;

		AccChargeCode ChargeCodeDisbursementEntryFee
		{
			get
			{
				if (fChargeCodeDisbursementEntryFee == null)
				{
					fChargeCodeDisbursementEntryFee = CreateChargeCode("ENF", "Customs Disbursements Entry Fee", Constants.ChargeType.Disbursement);
				}
				return fChargeCodeDisbursementEntryFee;
			}
		}
		AccChargeCode fChargeCodeDisbursementEntryFee;

		AccChargeCode ChargeCodeDeferred
		{
			get
			{
				if (fChargeCodeDeferred == null)
				{
					fChargeCodeDeferred = CreateChargeCode("DEF", "Customs Deferred Charge (For information only)", Constants.ChargeType.Comment);
				}
				return fChargeCodeDeferred;
			}
		}
		AccChargeCode fChargeCodeDeferred;

		EntryChargeTypeList SetupNZRegistryForCustomsChargeAccountCodes()
		{
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ChargeCodeDisbursementDefault.PK.ToGuid());
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ChargeCodeDeferred.PK.ToGuid());

			EntryChargeTypeList chargeTypeList = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.NewZealand);
			EntryChargeTypeSettingCollection chargeTypeSettings = new EntryChargeTypeSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			EntryChargeTypeSetting chargeTypeSettingDuty = chargeTypeSettings.AddNew();
			chargeTypeSettingDuty.ChargeType = "DTY";
			chargeTypeSettingDuty.AC_ChargeCode = ChargeCodeDisbursementDuty.PK;

			EntryChargeTypeSetting chargeTypeSettingEntryFee = chargeTypeSettings.AddNew();
			chargeTypeSettingEntryFee.ChargeType = "ENF";
			chargeTypeSettingEntryFee.AC_ChargeCode = ChargeCodeDisbursementEntryFee.PK;

			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeTypeSettings);
			return chargeTypeList;
		}

		public void TestMultiLineInvoiceChargeCodeGeneration()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dty = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "DTY");
			var gst = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "GST");
			var enf = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "ENF");
			var efg = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "EFG");
			var alc = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "ALC ");
			helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "GST", gst.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "ENF", enf.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "EFG", efg.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "ALC", alc.PK);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
			EntryChargeTypeList chargeTypeList = SetupNZRegistryForCustomsChargeAccountCodes();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var mockEntryHeader = Factory.NewMoq<CusEntryHeader>();
			CusEntryHeader entryHeader = mockEntryHeader.Object;

			entryHeader.EntryNumber = "902100210";
			entryHeader.FillWithValidTestData();
			entryHeader.SetDeclarationForTesting(declaration);
			declaration.CustomsEntryHeaders.Add(entryHeader);
			mockEntryHeader.Protected().Setup<EntryChargeTypeList>("GetEntryChargeTypeList").Returns(chargeTypeList);

			mockEntryHeader.Setup(m => m.IsFeePaidByBroker(new ZString("ALC"), It.IsAny<ZString>(), It.IsAny<ILogger>())).Returns(true);
			mockEntryHeader.Setup(m => m.IsFeePaidByBroker(new ZString("DTY"), It.IsAny<ZString>(), It.IsAny<ILogger>())).Returns(false);
			mockEntryHeader.Setup(m => m.IsFeePaidByBroker(new ZString("ENF"), It.IsAny<ZString>(), It.IsAny<ILogger>())).Returns(true);
			mockEntryHeader.Setup(m => m.IsFeePaidByBroker(new ZString("GST"), It.IsAny<ZString>(), It.IsAny<ILogger>())).Returns(false);

			CustomsCharge[] countrySpecificCharges = new CustomsCharge[] { new CustomsCharge(null, "Codeless Charge", 4.40m, 0.40m, true, ZGuid.NewZGuid()) };
			mockEntryHeader.Setup(m => m.GetNonFeeCountrySpecificCharges()).Returns(countrySpecificCharges);

			entryHeader.Charges.AddNew("DTY", 11.11m);
			entryHeader.Charges.AddNew("GST", 11.22m);
			entryHeader.Charges.AddNew("ENF", 11.33m);
			entryHeader.Charges.AddNew("EFG", 2.11m);
			entryHeader.Charges.AddNew("ALC", 22.22m);

			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.FillWithValidTestData();
			entryLine1.CL_CH = entryHeader.PK;
			entryLine1.Fees.AddOrUpdate("DTY", 22.22m);
			entryLine1.Fees.AddOrUpdate("GST", 22.33m);
			entryLine1.Fees.AddOrUpdate("ENF", 22.11m);
			entryLine1.Fees.AddOrUpdate("EFG", 3.22m);
			entryLine1.Fees.AddOrUpdate("ALC", 33.33m);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.FillWithValidTestData();
			entryLine2.CL_CH = entryHeader.PK;
			entryLine2.Fees.AddOrUpdate("F1", 5.2m);
			entryLine2.Fees.AddOrUpdate("DTY", 33.33m);
			entryLine2.Fees.AddOrUpdate("GST", 33.11m);
			entryLine2.Fees.AddOrUpdate("ENF", 33.22m);
			entryLine2.Fees.AddOrUpdate("EFG", 1.33m);
			entryLine2.Fees.AddOrUpdate("ALC", 11.11m);

			CustomsCharge[] ratedLines = ServiceLocator.GetService<ICustomsCharges>(entryHeader).GetCustomsCharges(null);

			AssertEquals("RatedLines.Length", 5, ratedLines.Length);

			CustomsCharge charge1 = ratedLines[0];
			AssertEquals(null, charge1.GetChargeCode(Factory));
			AssertEquals("Codeless Charge", charge1.Description);
			AssertEquals(4.40m, charge1.Amount);
			AssertEquals(0.40m, charge1.GST);
			AssertEquals(true, charge1.IsPaidByBroker);

			CustomsCharge charge2 = ratedLines[1];
			AssertEquals(null, charge2.GetChargeCode(Factory));
			AssertEquals("ALAC Levy", charge2.Description);
			AssertEquals(66.66m, charge2.Amount);
			AssertEquals(0.00m, charge2.GST);
			AssertEquals(true, charge2.IsPaidByBroker);

			CustomsCharge charge3 = ratedLines[2];
			AssertEquals(ChargeCodeDisbursementDuty.PK, charge3.GetChargeCode(Factory).PK);
			AssertEquals("Duty", charge3.Description);
			AssertEquals(66.66m, charge3.Amount);
			AssertEquals(0.00m, charge3.GST);
			AssertEquals(false, charge3.IsPaidByBroker);

			CustomsCharge charge4 = ratedLines[3];
			AssertEquals(ChargeCodeDisbursementEntryFee.PK, charge4.GetChargeCode(Factory).PK);
			AssertEquals("Entry Fee", charge4.Description);
			AssertEquals(66.66m, charge4.Amount);
			AssertEquals(6.66m, charge4.GST);
			AssertEquals(true, charge4.IsPaidByBroker);

			CustomsCharge charge5 = ratedLines[4];
			AssertEquals(null, charge5.GetChargeCode(Factory));
			AssertEquals("GST", charge5.Description);
			AssertEquals(66.66m, charge5.Amount);
			AssertEquals(0.00m, charge5.GST);
			AssertEquals(false, charge5.IsPaidByBroker);
		}

		public void TestGroupInvoices()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			CusEntryHeader entryHeader2 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();

			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();

			BaseJobComInvoiceHeader invoice1 = topGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine2.PK;
			entryHeader.ResetTotalsAndCachedValues();

			ReadOnlyCollection<BaseJobComInvoiceGroupHeader> result = entryHeader.GroupInvoices;
			AssertEquals("For Entry1, there should be only one group invoice", 1, result.Count);
			AssertEquals("For Entry1, it should be TopGroup", topGroup, result[0]);

			entryHeader2.ResetTotalsAndCachedValues();
			result = entryHeader2.GroupInvoices;
			AssertEquals("For Entry2, there should be only one group invoice", 2, result.Count);

			bool topGroupFound = false;
			bool subGroupFound = false;
			foreach (BaseJobComInvoiceGroupHeader groupInvoice in result)
			{
				if (groupInvoice == topGroup)
				{
					topGroupFound = true;
				}
				else if (groupInvoice == subGroup)
				{
					subGroupFound = true;
				}
			}
			AssertEquals("TopGroup and SubGroup found", true, topGroupFound && subGroupFound);
		}

		public void TestGroupInvoicesDontHaveDuplicate()
		{
			BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();

			BaseJobComInvoiceHeader invoice1 = topGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			entryHeader.ResetTotalsAndCachedValues();

			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;

			ReadOnlyCollection<BaseJobComInvoiceGroupHeader> result = entryHeader.GroupInvoices;
			AssertEquals("For Entry1, there should be only one group invoice", 2, result.Count);
			bool topGroupFound = false;
			bool subGroupFound = false;
			foreach (BaseJobComInvoiceGroupHeader groupInvoice in result)
			{
				if (groupInvoice == topGroup)
				{
					topGroupFound = true;
				}
				else if (groupInvoice == subGroup)
				{
					subGroupFound = true;
				}
			}
			AssertEquals("TopGroup found", true, topGroupFound);
			AssertEquals("SubGroup found", true, subGroupFound);
		}

		public void TestMessageStatusDescription()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			var mockEntry = Factory.NewMoq<CusEntryHeader>();

			CusEntryHeader entry = mockEntry.Object;
			entry.CH_JE = declaration.PK;

			CodeDescriptionPairList testList = new CodeDescriptionPairList();
			testList.AddPair("AA", "AAAAA");
			testList.AddPair("BB", "BBBBB");

			var mockLookups = new Mock<CusEntryHeaderLookups>(entry);
			mockLookups.Setup(m => m.MessageStatusList).Returns(testList);
			CusEntryHeaderLookups entryLookups = mockLookups.Object;

			mockEntry.Protected().Setup<CusEntryHeaderLookups>("GetNewLookups").Returns(entryLookups);
			entry.CH_Status = "AA";
			AssertEquals("AAAAA", entry.MessageStatusDescription);

			entry.CH_Status = "BB";
			AssertEquals("BBBBB", entry.MessageStatusDescription);

			entry.CH_Status = "";
			AssertEquals("Not Sent", entry.MessageStatusDescription);
		}

		#region TestFetchStrategy
		public void TestFetchStrategy()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Entry Header FetchStrategy type", typeof(FetchStrategies.CusEntryHeaderFetchStrategy), entryHeader.FetchStrategy.GetType());
		}
		#endregion

		public void TestMessagesAreLoaded()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("Should not be loaded when object is initialised", false, entryHeader.MessagesAreLoaded);

			object objectToTriggerTheLazyLoader = entryHeader.Messages;
			AssertEquals("Should be loaded now", true, entryHeader.MessagesAreLoaded);
		}

		public void TestIProcessHandlingInfoProvider()
		{
			var entryHeader = (IProcessHandlingInfoProvider)Factory.New<CusEntryHeader>();
			AssertType<CusEntryHeaderProcessHandlingInfo>(entryHeader.ProcessHandlingInfo);
		}

		public void TestIRelatedJob()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var relatedJob = (IRelatedJob)entryHeader;
			entryHeader.CH_BGMReference = "B00001";
			entryHeader.CH_EntryStatus = "TX7";
			CombineAssertions(() =>
			{
				AssertEquals("B00001", relatedJob.JobNumber);
				AssertEquals("CusEntryHeader", relatedJob.JobDescription);
				AssertEquals("TX7", relatedJob.JobStatus);
			});
		}

		public void TestIControllerIDProvider()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var relatedJob = (IControllerIDProvider)entryHeader;
			CombineAssertions(() =>
			{
				AssertEquals(entryHeader.PK, relatedJob.BusinessObjectPK);
				AssertEquals(ControllerIDs.Customs.EntryHeader, relatedJob.ControllerID);
			});
		}

		#region TestGetNonFeeCountrySpecificChargesIsNotNull
		public void TestGetNonFeeCountrySpecificChargesIsNotNull()
		{
			CusEntryHeader header = Factory.New<CusEntryHeader>();
			AssertNotNull(header.GetNonFeeCountrySpecificCharges());
		}
		#endregion

		#region TestDefaultIsFeePaidByBroker
		public void TestDefaultIsFeePaidByBroker()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = Factory.New<CusEntryHeader>();
			header.CH_JE = declaration.PK;
			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));

			declaration.JE_PaidBy = MasterFiles.Business.Customs.PaidByCodeList.Codes.CLI;
			AssertEquals(false, header.IsFeePaidByBroker("anything", "", null));

			declaration.JE_PaidBy = MasterFiles.Business.Customs.PaidByCodeList.Codes.BRK;
			AssertEquals(true, header.IsFeePaidByBroker("anything", "", null));
		}
		#endregion

		#region TestStatusChangedToClearedSinceLoading
		public void TestStatusChangedToClearedSinceLoading()
		{
			var mock = Factory.NewMoq<CusEntryHeader>();
			CusEntryHeader entry = mock.Object;

			mock.Setup(m => m.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("StatusChangedToClearedSinceLoading", false, entry.StatusChangedToClearedSinceLoading);

			((INeedRow)entry).Row.AcceptChanges(); // pretend row is saved
			AssertEquals("StatusChangedToClearedSinceLoading", true, entry.StatusChangedToClearedSinceLoading);
		}
		#endregion

		#region TestDefaultIsStatusChangingToCleared
		public void TestDefaultIsStatusChangingToCleared()
		{
			AssertEquals(false, Factory.New<CusEntryHeader>().IsStatusChangingToCleared("", ""));
		}
		#endregion

		#region TestAddClearedCustomsLogIfRequired
		public void TestAddClearedCustomsLogIfRequiredWithInterchangeSentTime()
		{
			CheckClearedCustomsLog(true);
		}

		[NUnit.Framework.TestDate(2003, 2, 3)]
		public void TestAddClearedCustomsLogIfRequiredWithNoInterchangeSentTime()
		{
			CheckClearedCustomsLog(false);
		}

		void CheckClearedCustomsLog(bool setInterchangeTime)
		{
			var dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.FillWithValidTestData();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var entryMock = Factory.NewMoq<CusEntryHeader>();
			var entry = entryMock.Object;
			entry.FillWithValidTestData();
			dec.CustomsEntryHeaders.Add(entry);
			entry.SetDeclarationForTesting(dec);
			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);
			Factory.Save();
			AssertEquals("No Entry log created", 0, entry.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			AssertEquals("No Dec log created", 0, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			entry.CH_Status = "ZZ";
			entryMock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			if (setInterchangeTime)
			{
				mockMessage.Setup(m => m.EM_DateTimeInterchangeSent).Returns(new ZDateTime(2001, 2, 3));
			}
			message.EM_ReceiveTransmit = "RCV";
			entry.Messages.Add(message);
			Factory.Save();
			AssertEquals("One Entry log created", 1, entry.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			AssertEquals("One Dec log created", 1, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			var correctDate = new ZDateTime(2001, 2, 3);
			if (!setInterchangeTime)
			{
				correctDate = new ZDateTime(2003, 2, 3);
			}

			AssertEquals("Correct date", correctDate, entry.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code))[0].SL_EventTime);

			entry.OnSaving();
			AssertEquals("No more Entry logs created", 1, entry.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			AssertEquals("No more Dec logs created", 1, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
		}

		[NUnit.Framework.TestDate(2003, 2, 3)]
		public void TestExportDoesntAddClearedEventToLog()
		{
			var dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.FillWithValidTestData();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;

			var entryMock = Factory.NewMoq<CusEntryHeader>();
			var entry = entryMock.Object;
			entry.FillWithValidTestData();
			dec.CustomsEntryHeaders.Add(entry);
			entry.SetDeclarationForTesting(dec);
			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);
			Factory.Save();
			AssertEquals("No Entry log created", 0, entry.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			AssertEquals("No Dec log created", 0, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);

			entry.CH_Status = "ZZ";
			entryMock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			var mockMessage = Factory.NewMoq<EDIMessage>();
			var message = mockMessage.Object;
			message.EM_ReceiveTransmit = "RCV";
			entry.Messages.Add(message);
			Factory.Save();

			AssertEquals("No CLR log event should be created for exports, so that workflows are actioned for import CLR status", 0, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			AssertEquals("No CLR log event should be created for exports, so that workflows are actioned for import CLR status", 0, entry.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			AssertEquals("ECC log event created", 1, dec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code)).Length);
		}

		#endregion

		#region Warehouse Integration

		#region TestShouldCreateBondedWarehouseInwards

		public void TestShouldCreateBondedWarehouseInwards()
		{
			DummyDeclarationWithIntegrationSupport dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.SetSupportsBondedWarehousingForTesting(true);
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			BaseJobComInvoiceLine line = dec.CreateBondedLine();
			line.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("LineGoingIntoBondedWarehouse", true, dec.HasLineGoingIntoABondedWarehouse);

			var mock = Factory.NewMoq<CusEntryHeader>();
			var entry = mock.Object;
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			dec.CustomsEntryHeaders.Add(entry);
			entry.SetDeclarationForTesting(dec);
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry.ShouldCreateBondedWarehouseInwards);

			((INeedRow)entry).Row.AcceptChanges(); // pretend it is in the DB
			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", true, entry.ShouldCreateBondedWarehouseInwards);

			dec.SetSupportsBondedWarehousingForTesting(false);
			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry.ShouldCreateBondedWarehouseInwards);
			dec.SetSupportsBondedWarehousingForTesting(true);

			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Empty);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry.ShouldCreateBondedWarehouseInwards);

			line.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry.ShouldCreateBondedWarehouseInwards);

			BaseJobComInvoiceLine line2 = dec.CreateBondedLine();
			line2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", true, entry.ShouldCreateBondedWarehouseInwards);

			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(false);
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry.ShouldCreateBondedWarehouseInwards);

			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			AssertEquals("ShouldCreateBondedWarehouseInwards", true, entry.ShouldCreateBondedWarehouseInwards);

			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry.ShouldCreateBondedWarehouseInwards);

			var mock2 = Factory.NewMoq<CusEntryHeader>();
			var entry2 = mock2.Object;
			dec.CustomsEntryHeaders.Add(entry2);
			entry2.SetDeclarationForTesting(dec);
			((INeedRow)entry2).Row.AcceptChanges(); // pretend it is in the DB
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock2.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock2.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", true, entry2.ShouldCreateBondedWarehouseInwards);

			mock.Setup(x => x.ClearanceDate).Returns(ZDateTime.Now);
			mock2.Setup(x => x.ClearanceDate).Returns(ZDateTime.Empty);
			mock2.Setup(x => x.IsStatusChangingToCleared(ZString.Empty, ZString.Empty)).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseInwards", false, entry2.ShouldCreateBondedWarehouseInwards);
		}

		#endregion

		#region TestShouldFinaliseBondedWarehouseOutwards
		public void TestShouldFinaliseBondedWarehouseOutwards()
		{
			var dec = Factory.New<DummyDeclarationWithIntegrationSupport>();
			dec.SetSupportsBondedWarehousingForTesting(true);
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			var mock = Factory.NewMoq<CusEntryHeader>();
			var entry = mock.Object;
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			dec.CustomsEntryHeaders.Add(entry);
			entry.SetDeclarationForTesting(dec);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", false, entry.ShouldFinaliseBondedWarehouseOutwards);

			((INeedRow)entry).Row.AcceptChanges(); // pretend it is in the DB
			mock.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", true, entry.ShouldFinaliseBondedWarehouseOutwards);

			mock.Reset();
			dec.SetSupportsBondedWarehousingForTesting(false);
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", false, entry.ShouldFinaliseBondedWarehouseOutwards);
			dec.SetSupportsBondedWarehousingForTesting(true);

			mock.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Empty);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", false, entry.ShouldFinaliseBondedWarehouseOutwards);

			mock.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(false);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", false, entry.ShouldFinaliseBondedWarehouseOutwards);

			mock.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			AssertEquals("ShouldCreateBondedWarehouseOutwards", true, entry.ShouldFinaliseBondedWarehouseOutwards);

			mock.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ShouldCreateBondedWarehouseOutwards", false, entry.ShouldFinaliseBondedWarehouseOutwards);

			mock.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("ShouldCreateBondedWarehouseOutwards", true, entry.ShouldFinaliseBondedWarehouseOutwards);

			var mock2 = Factory.NewMoq<CusEntryHeader>();
			CusEntryHeader entry2 = mock2.Object;
			dec.CustomsEntryHeaders.Add(entry2);
			entry2.SetDeclarationForTesting(dec);
			((INeedRow)entry2).Row.AcceptChanges(); // pretend it is in the DB

			mock2.Reset();
			mock2.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock2.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", true, entry2.ShouldFinaliseBondedWarehouseOutwards);

			mock2.Reset();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Now);
			mock2.Setup(m => m.ClearanceDate).Returns(ZDateTime.Empty);
			mock2.Protected().Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>()).Returns(true);
			AssertEquals("ShouldCreateBondedWarehouseOutwards", false, entry2.ShouldFinaliseBondedWarehouseOutwards);
		}
		#endregion

		#region TestInwardsCreation

		MergedDeclarationCreator2Line<DummyDeclarationWithIntegrationSupport> SetupValidNature20ForInwardsCreation()
		{
			var creator = new MergedDeclarationCreator2Line<DummyDeclarationWithIntegrationSupport>(Factory, DeclarationApplicationCodeList.Codes.Builtin);
			BondedWarehouseAutomationEndToEndTest.CreateVirtualWarehouse(Factory, creator.Buyer.MainAddress);
			creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			creator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			creator.InvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			creator.InvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			creator.Entry1.SetDeclarationForTesting(creator.Declaration);

			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~~";
			part.RelatedOrganisations.AddOrganisationIfNotExist(creator.Buyer.PK, OrgPartRelation.RelationshipTypes.Owner);

			BaseCusClassification @class = Factory.New<BaseCusClassification>();
			@class.CC_LookupCode = "TestLookup";
			@class.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			@class.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			@class.CC_TariffNum = "2605.00.00 08";
			BaseCusClassPartPivot pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = @class.PK;
			pivot.CI_OP = part.PK;

			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);
			Factory.Save(); // so part gets in DB

			creator.InvoiceLine1.JI_PartNo = "~~~";
			creator.InvoiceLine1.JI_InvoiceQuantity = 100;
			creator.InvoiceLine1.JI_InvoiceUQ = "ML";

			creator.InvoiceLine2.JI_PartNo = "~~~";
			creator.InvoiceLine2.JI_InvoiceQuantity = 10;
			creator.InvoiceLine2.JI_InvoiceUQ = "KG";

			creator.Declaration.WarehouseDocAddress.E2_OA_Address = creator.Buyer.MainAddress.PK;
			creator.Entry1.EntryNumber = "~WhsTestEntry";

			Factory.Save(); // message sent

			return creator;
		}

		[NUnit.Framework.TestDate(2001, 1, 1, 1, 1, 1)]
		public void TestInwardsCreation()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var creator = SetupValidNature20ForInwardsCreation();
				creator.Entry1.OverrideIsStatusChangingToClearedForTesting = true;

				creator.Entry1.IsStatusChangingToClearedForTesting = true; // pretend service task has processed customs clear message for pay
				creator.Entry1.HasChanges = true;
				AssertEquals("No CLR Entry log created", 0, creator.Entry1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
				Factory.Save();

				var filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, creator.Entry1.EntryNumber + "-INW W00000001");
				filter.AddToFilter(WhsDocketSchema.WD_OH_Client, creator.Buyer.PK);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
				var receiveDocket = (BusinessObject)Factory.LoadTop1<IWhsReceive>(filter);

				AssertNotNull("Receive docket should be created", receiveDocket);
				AssertEquals("Should be finalised", false, ((ZDateTimeOffset)receiveDocket[WhsDocketSchema.WD_FinalisedDate.Name]).IsEmpty);
				AssertEquals("Should have correct arrival/clearance date", new ZDateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(3)), (ZDateTimeOffset)receiveDocket[WhsDocketSchema.WD_ArrivalDate.Name]);
				var linesFilter = new ZQuery(WhsDocketLineSchema.WE_WD, receiveDocket.PK);
				var docketLines = (BusinessObject[])Factory.Load<IWhsReceiveLine>(linesFilter);
				AssertEquals("Should only have a line for the n20", 1, docketLines.Length);
				AssertEquals("Should have correct qty", 10m, docketLines[0][WhsDocketLineSchema.WE_TransactionQuantity.Name]);
				AssertEquals("Should have correct unit qty", "KG", docketLines[0][WhsDocketLineSchema.WE_F3_NKPackType.Name]);
				AssertEquals("CLR Entry log created", 1, creator.Entry1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length);
			}
		}

		[NUnit.Framework.TestDate(2001, 1, 1, 1, 1, 1)]
		public void TestInwardsCreationWithMergedEntry()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var creator = SetupValidNature20ForInwardsCreation();
				creator.InvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				creator.InvoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				creator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.PartNumber;
				creator.Declaration.DoMerge();
				AssertEquals("headers", 1, creator.Declaration.CustomsEntryHeaders.Count);
				AssertEquals("lines", 1, creator.Declaration.CustomsEntryHeaders[0].MergedLines.Count);
				Factory.Save();

				creator.Entry1.OverrideIsStatusChangingToClearedForTesting = true;
				creator.Entry1.IsStatusChangingToClearedForTesting = true; // pretend service task has processed customs clear message for pay
				creator.Entry1.HasChanges = true;
				Factory.Save();

				var filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, creator.Entry1.EntryNumber + "-INW W00000001");
				filter.AddToFilter(WhsDocketSchema.WD_OH_Client, creator.Buyer.PK);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
				var receiveDocket = (BusinessObject)Factory.LoadTop1<IWhsReceive>(filter);

				AssertNotNull("Receive docket should be created", receiveDocket);
				AssertEquals("Should be finalised", false, ((ZDateTimeOffset)receiveDocket[WhsDocketSchema.WD_FinalisedDate.Name]).IsEmpty);
				AssertEquals("Should have correct arrival/clearance date", new ZDateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.FromHours(3)), (ZDateTimeOffset)receiveDocket[WhsDocketSchema.WD_ArrivalDate.Name]);
				var linesFilter = new ZQuery(WhsDocketLineSchema.WE_WD, receiveDocket.PK);
				linesFilter.OrderBy = WhsDocketLineSchema.WE_TransactionQuantity.Name + " ASC";
				var docketLines = (BusinessObject[])Factory.Load<IWhsReceiveLine>(linesFilter);
				AssertEquals("Should have two lines for the n20", 2, docketLines.Length);

				AssertEquals("Should have correct qty", 10m, docketLines[0][WhsDocketLineSchema.WE_TransactionQuantity.Name]);
				AssertEquals("Should have correct unit qty", "KG", docketLines[0][WhsDocketLineSchema.WE_F3_NKPackType.Name]);
				AssertEquals("Should have correct product", creator.InvoiceLine1.JI_OP, docketLines[0][WhsDocketLineSchema.WE_OP.Name]);

				AssertEquals("Should have correct qty", 100m, docketLines[1][WhsDocketLineSchema.WE_TransactionQuantity.Name]);
				AssertEquals("Should have correct unit qty", "ML", docketLines[1][WhsDocketLineSchema.WE_F3_NKPackType.Name]);
				AssertEquals("Should have correct product", creator.InvoiceLine2.JI_OP, docketLines[1][WhsDocketLineSchema.WE_OP.Name]);
			}
		}

		public void TestInwardsIsNotCreatedWithNoTransactionLines()
		{
			var creator = new MergedDeclarationCreator<DummyDeclarationWithIntegrationSupport>(Factory, DeclarationApplicationCodeList.Codes.Builtin);
			creator.Declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			creator.Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			creator.InvoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			creator.Entry1.SetDeclarationForTesting(creator.Declaration);

			TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);

			creator.InvoiceLine1.JI_PartNo = "";
			creator.InvoiceLine1.JI_InvoiceQuantity = 0;
			creator.InvoiceLine1.JI_InvoiceUQ = "";
			creator.Declaration.WarehouseDocAddress.E2_OA_Address = creator.Buyer.MainAddress.PK;
			creator.Entry1.EntryNumber = "~WhsTestEntry";

			Factory.Save(); // message sent
			creator.Entry1.OverrideIsStatusChangingToClearedForTesting = true;

			creator.Entry1.IsStatusChangingToClearedForTesting = true; // pretend service task has processed customs clear message for pay
			creator.Entry1.HasChanges = true;
			Factory.Save();

			ZQuery filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, creator.Entry1.EntryNumber);
			filter.AddToFilter(WhsDocketSchema.WD_OH_Client, creator.Buyer.PK);
			filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
			BusinessObject receiveDocket = (BusinessObject)Factory.LoadTop1<IWhsReceive>(filter);

			AssertNull("Receive docket should not be created", receiveDocket);
		}

		public void TestInwardsIsNotCreatedWhenAmendmentIsOutstanding()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				DataRegistry.Business.CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-1).ToDateTime());
				var creator = SetupValidNature20ForInwardsCreation();
				creator.Declaration.JE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
				creator.Entry1.OverrideIsStatusChangingToClearedForTesting = true;
				creator.Entry1.IsStatusChangingToClearedForTesting = true; // pretend service task has processed customs clear message for pay
				creator.Entry1.HasChanges = true;
				creator.Entry1.SetHaveAmendmentsBeenMadeAndNotYetClearedByCustomsForTesting(true);

				// Set up that should have occured on all client sites
				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				ZQuery postMasterFilter = new ZQuery(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
				GlbStaff postMasterStaff = (GlbStaff)factory2.Load(typeof(GlbStaff), postMasterFilter)[0];
				postMasterStaff.GS_EmailAddress = "test@test.edi.com.au";
				factory2.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				Factory.Save();

				AssertEquals("Single email send out notifying no update due to amendments", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

				ZQuery filter = new ZQuery(WhsDocketSchema.WD_ExternalReference, creator.Entry1.EntryNumber);
				filter.AddToFilter(WhsDocketSchema.WD_OH_Client, creator.Buyer.PK);
				filter.AddToFilter(WhsDocketSchema.WD_DocketType, "INW");
				BusinessObject receiveDocket = (BusinessObject)Factory.LoadTop1<IWhsReceive>(filter);

				AssertNull("Receive docket should not be created", receiveDocket);
			}
		}

		#endregion

		#endregion

		public void TestBrokerageCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.FrenchGuyana))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entryHeader = dec.CustomsEntryHeaders.AddNew();
				AssertEquals("FR", entryHeader.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entryHeader = dec.CustomsEntryHeaders.AddNew();
				AssertEquals("FR", entryHeader.BrokerageCountryCode);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var dec = Factory.New<BaseJobDeclaration>();
				var entryHeader = dec.CustomsEntryHeaders.AddNew();
				AssertEquals("DE", entryHeader.BrokerageCountryCode);
			}
		}

		public void TestGetNewAmendmentSnapshotManager()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertNull(entryHeader.GetNewAmendmentSnapshotManager());
		}
	}
}
