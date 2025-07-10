using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	partial class JobDeclarationDataObjectReaderTest
	{
		public void TestCusEntryHeaderCH_AddInfoMapping()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var expectedEntryHeaderAddInfo = string.Format("{0}=CA*{1}=CLR", USAddInfoSchema.Constants.US_DestinationState.Substring(3), USAddInfoSchema.Constants.US_CWOStatus.Substring(3));
				var entryHeaderAddInfo = "THIS!@#3=HELLO*" + expectedEntryHeaderAddInfo;
				var entryHeaderDataObject = SetupEntryHeader("EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), AddInfoCollectionCreator.CreateCollection(entryHeaderAddInfo));
				var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();
				AssertEquals("entryHeaderBO.CH_AddInfo", expectedEntryHeaderAddInfo, entryHeaderBO.CH_AddInfo);
			}
		}

		public void TestCusEntryHeaderCusAddInfoAndCusCodeData()
		{
			var usDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<US.IJobDeclaration>();
			usDeclaration.JE_MasterBill = "USMWB123";
			usDeclaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			var entryHeader = usDeclaration.CustomsEntryHeaders.AddNew();
			var entryHeaderCusCodeDataTypeSupporter = (ICusCodeDataTypeSupporter)entryHeader;
			var entryHeaderPSCReasonCodes = "PRC";
			var pscReasonCodesH01 = "H01";
			Type type = null;
			Assert(entryHeaderCusCodeDataTypeSupporter.GetCusCodeDataTypes().TryGetValue(entryHeaderPSCReasonCodes, out type));
			entryHeader.Delete();
			var euDeclaration = (BaseJobDeclaration)Factory.BOFactory.New<GB.IJobDeclaration>();
			euDeclaration.JE_MasterBill = "EUMWB123";
			euDeclaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			entryHeader = euDeclaration.CustomsEntryHeaders.AddNew();
			var entryHeaderCusAddInfoTypeSupporter = (ICusAddInfoTypeSupporter)entryHeader;
			Assert(entryHeaderCusAddInfoTypeSupporter.GetCusAddInfoTypes().TryGetValue(Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, out type));
			var maritimeUCNThatIsHeldAddInfo = MaritimeUcnThatIsHeldSchema.Constants.NW_UCN.Substring(3) + "=12";
			entryHeader.Delete();
			var maritimeUCNThatIsHeldDataObject = new UniversalCustoms.AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(maritimeUCNThatIsHeldAddInfo)
			};
			var pscReasonCodesDataObject = new UniversalCustoms.CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = entryHeaderPSCReasonCodes },
				SubType = new CodeDescriptionPair35Char() { Code = pscReasonCodesH01 },
				Reference = "AW1234"
			};
			var entryHeaderDataObject = new UniversalCustoms.EntryHeader()
			{
				Type = new EntryType() { Code = "EXP" },
				AddInfoGroupCollection = new List<UniversalCustoms.AddInfoGroup>(new[] { maritimeUCNThatIsHeldDataObject }),
				CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>(new[] { pscReasonCodesDataObject })
			};
			var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates), usDeclaration, ZGuid.Empty).ReadIntoBusinessObject();
			AssertNotNull(entryHeaderBO);
			CombineAssertions(delegate
			{
				var cusAddInfoBOs = LoadCusAddInfo(entryHeaderBO.TablePrefix, entryHeaderBO.PK);
				AssertEquals(0, cusAddInfoBOs.Length);
				var cusCodeDataBOs = LoadCusCodeData(entryHeaderBO.TablePrefix, entryHeaderBO.PK);
				AssertEquals(1, cusCodeDataBOs.Length);
				var pscReasonCodesBO = cusCodeDataBOs[0];
				AssertCusCodeDataContents(pscReasonCodesBO, entryHeaderBO.TablePrefix, entryHeaderBO.PK, entryHeaderPSCReasonCodes, pscReasonCodesH01, "AW1234", ZBool.False, ZShort.Zero);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryHeader found, creating new CusEntryHeader.
Information - Populating CusEntryHeader...
".Trim(), logger.Logs);
			});

			logger.ClearLogs();
			entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom), euDeclaration, ZGuid.Empty).ReadIntoBusinessObject();
			AssertNotNull(entryHeaderBO);
			CombineAssertions(delegate
			{
				var cusAddInfoBOs = LoadCusAddInfo(entryHeaderBO.TablePrefix, entryHeaderBO.PK);
				AssertEquals(1, cusAddInfoBOs.Length);
				var maritimeUCNThatIsHeldBO = cusAddInfoBOs[0];
				AssertCusAddInfoContents(maritimeUCNThatIsHeldBO, entryHeaderBO.TablePrefix, entryHeaderBO.PK, Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld, partialAddInfoData: maritimeUCNThatIsHeldAddInfo);
				var cusCodeDataBOs = LoadCusCodeData(entryHeaderBO.TablePrefix, entryHeaderBO.PK);
				AssertEquals(0, cusCodeDataBOs.Length);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryHeader found, creating new CusEntryHeader.
Information - Populating CusEntryHeader...
Warning - Customs Reference was not processed as there is no support for Table with code 'CH'.
".Trim(), logger.Logs);
			});
		}

		public void TestCusEntryHeaderFieldMappings()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeaderDataObject = SetupEntryHeader("EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);

			var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();
			AssertCusEntryHeaderContents(entryHeaderBO, declaration.PK, "EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), "", ZGuid.Empty);
			AssertEquals("entryHeaderBO.AllEntryLines.Count", 0, entryHeaderBO.AllEntryLines.Count);
			AssertEquals("entryHeaderBO.Charges.Count", 0, entryHeaderBO.Charges.Count);
			var childCusEntryNumBOs = LoadCusEntryNum(entryHeaderBO.TableName, entryHeaderBO.PK);
			AssertEquals("Child CusEntryNum", 0, childCusEntryNumBOs.Length);
			var childCusCodeDataBOs = LoadCusCodeData(entryHeaderBO.TablePrefix, entryHeaderBO.PK);
			AssertEquals("Child CusCusCodeData", 0, childCusCodeDataBOs.Length);
			var childCusAddInfoBOs = LoadCusAddInfo(entryHeaderBO.TablePrefix, entryHeaderBO.PK);
			AssertEquals("Child CusAddInfo", 0, childCusAddInfoBOs.Length);
			var childCusEntryHeaderBOs = LoadCusEntryHeaderFromPrimeEntryPK(entryHeaderBO.PK);
			AssertEquals("childCusEntryHeaderBOs.Length", 0, childCusEntryHeaderBOs.Length);

			AssertNotEquals("PreCondition", 1000.10m, entryHeaderBO.CH_TotalPaid);
			entryHeaderBO.CH_TotalPaid = 1000.10m;

			entryHeaderDataObject.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber2(), SetupEntryNumber() });
			entryHeaderDataObject.EntryLineCollection = new List<UniversalCustoms.EntryLine>(new[] { SetupEntryLine2(), SetupEntryLine() });
			entryHeaderDataObject.EntryHeaderChargeCollection = new List<UniversalCustoms.EntryHeaderCharge>(new[] { SetupEntryHeaderCharge2(), SetupEntryHeaderCharge() });
			entryHeaderDataObject.RelatedEntryHeaderCollection = new List<UniversalCustoms.EntryHeader>(new[]
				{
					SetupEntryHeader("IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5))
				});

			var entryHeaderChargeBOThatShouldBeMatched = entryHeaderBO.Charges.AddNew("WCH", 150m);
			var entryHeaderChargeBOThatShouldBeDeleted = entryHeaderBO.Charges.AddNew("GCH", 850m);

			var entryNumberBOThatShouldBeMatched = CreateCusEntryNumber(entryHeaderBO.TableName, entryHeaderBO.PK, CurrentCompanyHelper.TargetCountryCode, "W89", "W986548", ZBool.True);
			var entryNumberBOThatShouldBeDeleted = CreateCusEntryNumber(entryHeaderBO.TableName, entryHeaderBO.PK, CurrentCompanyHelper.TargetCountryCode, "!34", "ZBD234", ZBool.True);

			var entryLineBOThatShouldBeDeleted = entryHeaderBO.AllEntryLines.AddNew();
			entryLineBOThatShouldBeDeleted.CL_LineNumber = 1;

			Factory.SaveForTesting();

			logger.ClearLogs();
			var entryHeaderBO2 = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("Should have matched", entryHeaderBO, entryHeaderBO2);
				AssertCusEntryHeaderContents(entryHeaderBO2, declaration.PK, "EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), "", ZGuid.Empty);
				entryHeaderBO2.AllEntryLines.Load();
				AssertEquals("entryHeaderBO2.AllEntryLines.Count", 2, entryHeaderBO2.AllEntryLines.Count);
				AssertEquals("Should have been deleted", true, entryLineBOThatShouldBeDeleted.IsDeleted);
				var entryLineBO1 = entryHeaderBO2.AllEntryLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 2);
				var entryLineBO2 = entryHeaderBO2.AllEntryLines.OfType<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber != 2);
				AssertCusEntryLineContents2(entryLineBO1, entryHeaderBO2.PK);
				AssertCusEntryLineContents(entryLineBO2, entryHeaderBO2.PK);

				entryHeaderBO2.Charges.Load();
				AssertEquals("entryHeaderBO2.Charges.Count", 2, entryHeaderBO2.Charges.Count);
				var entryHeaderChargeBO1 = entryHeaderBO2.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == "WCH");
				var entryHeaderChargeBO2 = entryHeaderBO2.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType != "WCH");
				AssertEquals("Should have been deleted", true, entryHeaderChargeBOThatShouldBeDeleted.IsDeleted);
				AssertEquals("Should have not been deleted", false, entryHeaderChargeBOThatShouldBeMatched.IsDeleted);
				AssertEquals("Should have matched", entryHeaderChargeBO1, entryHeaderChargeBOThatShouldBeMatched);
				AssertCusEntryHeaderChargeContents2(entryHeaderChargeBO1, entryHeaderBO2.PK);
				AssertCusEntryHeaderChargeContents(entryHeaderChargeBO2, entryHeaderBO2.PK);

				childCusEntryNumBOs = LoadCusEntryNum(entryHeaderBO2.TableName, entryHeaderBO2.PK);
				AssertEquals("Child CusEntryNum", 2, childCusEntryNumBOs.Length);
				AssertEquals("Should have been deleted", true, entryNumberBOThatShouldBeDeleted.IsDeleted);
				var entryNumberBO1 = childCusEntryNumBOs.FirstOrDefault(x => x.CE_EntryType == "W89" && x.CE_RN_NKCountryCode == CurrentCompanyHelper.TargetCountryCode);
				var entryNumberBO2 = childCusEntryNumBOs.FirstOrDefault(x => x.CE_EntryType != "W89" && x.CE_RN_NKCountryCode == CurrentCompanyHelper.TargetCountryCode);
				AssertEquals(entryNumberBO1, entryNumberBOThatShouldBeMatched);
				AssertCusEntryNumberContents2(entryNumberBO1, entryHeaderBO2.TableName, entryHeaderBO2.PK, CurrentCompanyHelper.TargetCountryCode);
				AssertCusEntryNumberContents(entryNumberBO2, entryHeaderBO2.TableName, entryHeaderBO2.PK, CurrentCompanyHelper.TargetCountryCode);

				childCusEntryHeaderBOs = LoadCusEntryHeaderFromPrimeEntryPK(entryHeaderBO2.PK);
				AssertEquals("childCusEntryHeaderBOs.Length", 1, childCusEntryHeaderBOs.Length);
				AssertCusEntryHeaderContents(childCusEntryHeaderBOs[0], declaration.PK, "IM$", "MS1", "ES1", "BG89756", 869.54m, new ZDateTime(2012, 4, 4), new ZDateTime(2012, 4, 5), "", entryHeaderBO2.PK);

				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusEntryHeader.
Information - Populating CusEntryHeader...
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching CusEntryHeaderCharges.
Information - Populating CusEntryHeaderCharges...
Information - No matching CusEntryHeaderCharges found, creating new CusEntryHeaderCharges.
Information - Populating CusEntryHeaderCharges...
Information - No matching CusEntryLine found, creating new CusEntryLine.
Information - Populating CusEntryLine...
Information - No matching CusEntryLine found, creating new CusEntryLine.
Information - Populating CusEntryLine...
Information - No matching CusEntryHeader found, creating new CusEntryHeader.
Information - Populating CusEntryHeader...
".Trim(), logger.Logs);
			});

			if (ErrorReporter.LastKeyReported == string.Format("{0} does not support B7_Type 'WEN'", typeof(CusEntryHeader).FullName))
			{
				ErrorReporter.Clear();
			}

			#endregion
		}

		public void TestCusEntryHeaderFieldMappingsFromEntryNumber()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_MessageType = "EX$";
			cusEntryHeader.CH_Status = "MS2";
			cusEntryHeader.CH_EntryStatus = "ES3";
			cusEntryHeader.CH_BGMReference = "BG32423";
			cusEntryHeader.EntryNumber = "W986548";

			Factory.SaveForTesting();

			var entryHeaderDataObject1 = SetupEntryHeader("EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);
			entryHeaderDataObject1.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { SetupEntryNumber2() });

			var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject1, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();
			AssertEquals("Should be same because of Entry Number matched.", cusEntryHeader.PK, entryHeaderBO.PK);

			var duplicateEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			duplicateEntryHeader.CH_MessageType = "EX$";
			duplicateEntryHeader.CH_BGMReference = "BG32423";
			duplicateEntryHeader.EntryNumber = "W986548";
			Factory.SaveForTesting();

			entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject1, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();
			AssertNull("Shoun't match to any existing entry headers.", entryHeaderBO);
			AssertContains("Cannot import when there are multiple entry headers matched.", logger.Logs);
		}

		public void TestCusEntryHeaderMatchOnBGMReference()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_MessageType = "IMP";
			cusEntryHeader.CH_Status = "MS2";
			cusEntryHeader.CH_EntryStatus = "ES3";
			cusEntryHeader.CH_BGMReference = "MATCH";
			cusEntryHeader.EntryNumber = "DONTMATCH";

			Factory.SaveForTesting();

			//set up a data object that has the same BGM reference and a different entry number
			var entryHeaderDataObject1 = SetupEntryHeader("IMP", "MS2", "ES3", "MATCH", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);
			var entryNumber = SetupEntryNumber(new EntryType() { Code = "IMP", Description = "W89 DESC" }, "W986548", ZBool.False, new EntryStatus() { Code = "02", Description = "NotClear" }, new ZDateTime(2017, 7, 7));
			entryHeaderDataObject1.EntryNumberCollection = new List<UniversalCustoms.EntryNumber>(new[] { entryNumber });

			var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject1, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();

			//entry number is not found so system should match on BGM reference
			AssertEquals("Should be same because of BGM match.", cusEntryHeader.PK, entryHeaderBO.PK);
			//entry number should be updated
			AssertEquals("Entry Number should be updated", "W986548", entryHeaderBO.EntryNumber);
		}

		public void TestCusEntryHeaderMatchOnBGMReferenceWithDeclarationReference()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "DECREFERENCE";

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_MessageType = "IMP";
			cusEntryHeader.CH_Status = "MS2";
			cusEntryHeader.CH_EntryStatus = "ES3";
			cusEntryHeader.CH_BGMReference = "DECREFERENCE/MATCH";
			cusEntryHeader.EntryNumber = "DONTMATCH";

			Factory.SaveForTesting();

			//set up a data object that has the same BGM reference and a different entry number
			var entryHeaderDataObject1 = SetupEntryHeader("IMP", "MS2", "ES3", "MATCH", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);

			var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject1, logger, CurrentCompanyHelper, declaration, ZGuid.Empty).ReadIntoBusinessObject();

			//should only be one entry because entry is a match
			AssertEquals("Entry found should be the existing entry.", cusEntryHeader.PK, entryHeaderBO.PK);
			AssertEquals("At least one value on the entry should be updated to prove the match happened", 1034.43m, declaration.ActiveEntryHeaders[0].CH_TotalPaid);
		}

		public void TestCusEntryLineAndJobComInvoiceLineFetchHints()
		{
			var newFactory = new UniversalObjectFactory();
			var declaration = newFactory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_MessageType = "EX$";
			cusEntryHeader.CH_Status = "MS2";
			cusEntryHeader.CH_EntryStatus = "ES3";
			cusEntryHeader.CH_BGMReference = "BG32423";
			cusEntryHeader.EntryNumber = "W986548";

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;
			cusEntryLine.Fees.AddNew();

			var cusEntryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine2.JI_CL = cusEntryLine2.PK;
			cusEntryLine2.Fees.AddNew();

			newFactory.SaveForTesting();

			var declaration2 = Factory.Load<BaseJobDeclaration>(declaration.PK);
			var entryHeaderDataObject1 = SetupEntryHeader("EX$", "MS2", "ES3", "BG32423", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);

			var expectedCounts = new Dictionary<string, int> {
				{ "JobComInvHeaderCharge", 1 },
				{ "CusUnderbondDec", 2 },
				{ "CusContainerInvoiceLinePivot", 1 },
				{ "StmNote", 1 },
				{ "UNDGDataItem", 1 },
				{ "StmUniversalCopy", 2 },
				{ "CusEntryLineFee", 1 }
			};

			var entryHeaderBO = new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject1, logger, CurrentCompanyHelper, declaration2, ZGuid.Empty).ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				foreach (var tableName in expectedCounts.Keys)
				{
					AssertEquals(tableName, expectedCounts[tableName], Factory.BOFactory.GetTableHitCount(tableName));
				}
			});
		}

		public void TestCusEntryPayInfoNotPopulated_Default()
		{
			var universalEntryHeader = new UniversalCustoms.EntryHeader
			{
				PaymentInformationCollection = new List<UniversalCustoms.EntryHeaderPaymentInformation>
				{
					new UniversalCustoms.EntryHeaderPaymentInformation
					{
						IncomingPaymentResponseNumber = "123",
						TransactionType = new CodeDescriptionPair { Code = "TT" }
					}
				}
			};

			var declaration = Factory.New<BaseJobDeclaration>();
			var dataObjectReader = new CustomsEntryHeaderDataObjectReader(universalEntryHeader, logger, CurrentCompanyHelper, declaration, ZGuid.Empty);
			var entryHeaderBO = dataObjectReader.ReadIntoBusinessObject();
			AssertEquals("EntryPayInfo Items Count", 0, entryHeaderBO.EntryPayInfos.Count);
		}

		public void TestCusEntryPayInfoPopulated_WhenFlagIsActive()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_DeclarationReference = "DECREFERENCE";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "IMP";
			entryHeader.CH_Status = "MS2";
			entryHeader.CH_EntryStatus = "ES3";
			entryHeader.CH_BGMReference = "DECREFERENCE/MATCH";

			var entryPayInfo1 = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo1.C9_TransactionType = "TT";
			entryPayInfo1.C9_IncomingPayResponseNo = "123";

			var entryPayInfo2 = entryHeader.EntryPayInfos.AddNew();
			entryPayInfo2.C9_TransactionType = "DL";
			entryPayInfo2.C9_IncomingPayResponseNo = "DEL";
			Factory.SaveForTesting();

			var universalEntryHeader = SetupEntryHeader("IMP", "MS2", "ES3", "MATCH", 1034.43m, new ZDateTime(2012, 3, 4), new ZDateTime(2012, 3, 5), null);
			universalEntryHeader.PaymentInformationCollection = new List<UniversalCustoms.EntryHeaderPaymentInformation>
			{
				new UniversalCustoms.EntryHeaderPaymentInformation
				{
					IncomingPaymentResponseNumber = "123",
					TransactionType = new CodeDescriptionPair { Code = "TT" }
				},
				new UniversalCustoms.EntryHeaderPaymentInformation
				{
					IncomingPaymentResponseNumber = "456",
					TransactionType = new CodeDescriptionPair { Code = "ZZ" }
				},
			};

			var dataObjectReader = new CustomsEntryHeaderDataObjectReader_ForEntryPayInfoTest(universalEntryHeader, logger, CurrentCompanyHelper, declaration, ZGuid.Empty);
			var entryHeaderBO = dataObjectReader.ReadIntoBusinessObject();
			CombineAssertions("CusEntryHeader EntryPayInfo Collection", () =>
			{
				AssertEquals("Contains matched [TT]-[123] EntryPayInfo?", true, entryHeaderBO.EntryPayInfos.Any(x => x.C9_TransactionType == "TT"));
				AssertEquals("Contains new [ZZ]-[456] EntryPayInfo?", true, entryHeaderBO.EntryPayInfos.Any(x => x.C9_TransactionType == "ZZ"));
				AssertEquals("Contains unmatched [DL]-[DEL] EntryPayInfo?", false, entryHeaderBO.EntryPayInfos.Any(x => x.C9_TransactionType == "DL"));
				AssertEquals("Is unmatched [DL]-[DEL] EntryPayInfo deleted?", true, entryPayInfo2.IsDeleted);
			});
		}

		sealed class CustomsEntryHeaderDataObjectReader_ForEntryPayInfoTest : CustomsEntryHeaderDataObjectReader
		{
			public CustomsEntryHeaderDataObjectReader_ForEntryPayInfoTest(
				UniversalCustoms.EntryHeader entryHeaderDataObject,
				IXmlImportLogger logger,
				UniversalDataObjectReaderHelper helper,
				BaseJobDeclaration declaration,
				ZGuid primeEntryPK,
				List<ZString> matchingKeys = null)
				: base(entryHeaderDataObject, logger, helper, declaration, primeEntryPK, matchingKeys)
			{
			}

			protected override bool ShouldPopulatePaymentInformationData => true;
		}
	}
}
