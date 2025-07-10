using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BIRDDataImporterTest : TestCaseWithFactory
	{
		public void TestDoNotForENSHeaderHasMessage()
		{
			var jobdeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobdeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = jobdeclaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = jobdeclaration.PK;
			ensEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			ensEntry.Object.Messages.AddNew(typeof(MQEDIMessage));
			jobdeclaration.CustomsEntryHeaders.Add(ensEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(ensEntry.Object.MergedLines.AddNew());

			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZASKF USA                            1536 GENESIS RD                            ZBCROSSVILLE                    TN38555                                         ZZ7501000000010                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(jobdeclaration, generator, notifications);

			Assert(notifications.HasErrors());
			AssertEquals(notifications[0].Message, jobdeclaration.GetReasonForNotAbleToUpdate());
		}

		public void TestImportDataWhenCRLHeaderHasBeenAccept()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			seEntry.Messages.AddNew(typeof(MQEDIMessage));
			Factory.Save();

			AssertEquals("prerequisite only has one invoice line", 1, declaration.InvoiceLines.Count);
			Assert(seEntry.IsClearedEntry);

			string messageText = "AA3461ABC8888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         ABC 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 1234567891                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         40002DE00001000000000000000                    000000005060267                  50 1234567891                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         8950100000012813                                                                90                      0                       0000001281300000102500          ZZ3461000000010                                                                 ";
			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			var declaration2 = Factory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Import successful", 2, declaration2.InvoiceLines.Count);
			AssertEquals("Entry header should not replace", seEntry.PK, declaration2.ActiveEntryHeaders.CargoReleaseEntry.PK);
		}

		public void TestDoNotSetJE_MessageTypeFromOrgUNLOCO()
		{
			DeclarationTestHelper.SetEntryFilerCode("SV9");

			var foreignBasedImporter = Factory.NewWithValidTestData<OrgHeader>();
			foreignBasedImporter.OH_Code = "89045328";
			foreignBasedImporter.OH_RL_NKClosestPort = "AUSYD";

			var ein = foreignBasedImporter.CustomsCodes.AddNew();
			ein.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			ein.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			ein.OK_CustomsRegNo = "91-013199000";

			Factory.Save();

			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZASKF USA                            1536 GENESIS RD                            ZBCROSSVILLE                    TN38555                                         ZZ7501000000010                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(JobMessageTypeList.Codes.ImportByExternalBroker, declaration.JE_MessageType);
		}

		public void TestRegistryIndicatesACEDeclarationShouldBeCreated()
		{
			string messageText = "AA3461ABC8888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         ABC 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 1234567891                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         40002DE00001000000000000000                    000000005060267                  50 1234567891                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         8950100000012813                                                                90                      0                       0000001281300000102500          ZZ3461000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var notifications = new NotificationCollection();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = "TRF";
			declaration.US_EntryFilerCode = "ABC";
			Assert(declaration.IsACE);

			AssertNoExceptionThrown(delegate
			{ new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications); });

			AssertNotContains(BIRDDeclarationDataAdapter.MergeUnsuccessful, (ZString)notifications.ToUniqueMessageListString());
			Assert(!declaration.IsACE);
		}

		public void TestCreateUltimateConsigneeWhenPossible()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZASKF USA                            1536 GENESIS RD                            ZBCROSSVILLE                    TN38555                                         ZZ7501000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			OrgCusCode[] cusCodes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");

			AssertEquals(1, cusCodes.Length);

			OrgHeader organisation = cusCodes[0].Header;

			AssertEquals("Name", "SKF USA", organisation.OH_FullName);
			AssertEquals("Address1", "1536 GENESIS RD", organisation.MainAddress.OA_Address1);
			AssertEquals("City", "CROSSVILLE", organisation.MainAddress.OA_City);
			AssertEquals("State", "TN", organisation.MainAddress.OA_State);
			AssertEquals("PostCode", "38555", organisation.MainAddress.OA_PostCode);
			AssertEquals("PortCode", "US", organisation.MainAddress.OA_RL_NKRelatedPortCode);
			AssertEquals("EIN number assigned", "91-013199000", organisation.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.USACodeTypes.EmployerIdentificationNumber));
			AssertEquals(JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			Assert(!declaration.US_NoDutyCalc);
			declaration.UnlockImportEntryNumberAllocationMutex();
		}

		public void TestMessageTypeForExternalJobs()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			string messageText = "AA7501ABC8888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         ABC 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZZ7501000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			OrgHeader importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			Factory.Save();

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(JobMessageTypeList.Codes.ImportByExternalBroker, declaration.JE_MessageType);
			Assert(declaration.US_NoDutyCalc);
		}

		public void TestJE_MergeByIsConsideredAndBIRDIsProcessedCorrectlyForCRL()
		{
			//two lines for 3461 transactions
			string messageText = "AA3461ABC8888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         ABC 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 1234567891                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         40002DE00001000000000000000                    000000005060267                  50 1234567891                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         8950100000012813                                                                90                      0                       0000001281300000102500          ZZ3461000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			OrgHeader importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			importer.MiscServ.OM_IMMergeCustomsInvoiceLinesBy = "TRF";
			Factory.Save();

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = "TRF";//should be overridden by import process to NON
			declaration.US_EntryFilerCode = "ABC";
			Factory.Save();

			AssertNoExceptionThrown(delegate
			{ new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications); });

			var entry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(entry);
			AssertEquals(2, entry.MergedLines.Count);
			AssertEquals(2, declaration.InvoiceLines.Count);
			AssertContains("Should have warned about no orgs with the entry filer code", string.Format(BIRDDeclarationDataAdapter.NoOrganisationWithSpecifiedEntryFilerCode, "ABC"), notifications.ToUniqueMessageListString());
			declaration.UnlockImportEntryNumberAllocationMutex();
		}

		public void TestSetExternalBroker()
		{
			string messageText = "AA3461ABC8888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         ABC 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         40002DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         8950100000012813                                                                90                      0                       0000001281300000102500          ZZ3461000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			OrgHeader broker = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			broker.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "ABC");
			Factory.Save();

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals(broker.PK, declaration.JE_OH_ExternalBroker);
			AssertNotContains("Should have warned about no orgs with the entry filer code", string.Format(BIRDDeclarationDataAdapter.NoOrganisationWithSpecifiedEntryFilerCode, "ABC"), notifications.ToUniqueMessageListString());
			AssertNotContains("Should have warned about multiple orgs with the same entry filer code", string.Format(BIRDDeclarationDataAdapter.MultipleOrganisationsWithSpecifiedEntryFilerCode, "ABC"), notifications.ToUniqueMessageListString());
		}

		public void TestWhenMultipleExternalBrokersExistForOneEntryFilerCode()
		{
			string messageText = "AA3461ABC8888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         ABC 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001PK         AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         40002DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         8950100000012813                                                                90                      0                       0000001281300000102500          ZZ3461000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			OrgHeader broker1 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			broker1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "ABC");

			OrgHeader broker2 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199010");
			broker2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "ABC");

			OrgHeader broker3 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "92-013199010");
			broker3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EntryFilerCode, "123");

			Factory.Save();

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_ExternalBroker = broker3.PK;

			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertContains("Should have warned about multiple orgs with the same entry filer code", string.Format(BIRDDeclarationDataAdapter.MultipleOrganisationsWithSpecifiedEntryFilerCode, "ABC"), notifications.ToUniqueMessageListString());
			AssertEquals("declaration retains the original value", broker3.PK, declaration.JE_OH_ExternalBroker);
		}

		public void TestCannotMergeWhenSomeoneElseIsAllocatingEntryNumber()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5         01891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001CTNS       AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZZ7501000000010                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_BRDRefNo = "B00001001";
			declaration.JE_TotalNoOfPacksPackType = "";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declarationInDiffFactory = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals(true, declarationInDiffFactory.LockImportEntryNumberAllocationMutex);

			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("Job Type", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("Bond Type", "", declaration.US_BondType);
			AssertEquals("IOR", ZGuid.Empty, declaration.IOROrgPK);
			AssertEquals("Ultimate Consignee", ZGuid.Empty, declaration.JE_OH_Importer);
			AssertEquals("EntryFilerCode", "XJ5", declaration.US_EntryFilerCode);
			AssertEquals("EntryNumber", "", declaration.ImportEntryNumber);
			AssertEquals("Entry Type", "", declaration.US_EntryType);
			AssertEquals("Surety Code", "", declaration.US_SuretyCode);
			AssertEquals("Destination State", "", declaration.US_DestinationState);
			AssertEquals("Importing vessel name", "", declaration.JE_VesselName);
			AssertEquals("Trasport Mode", "", declaration.JE_TransportMode);
			AssertEquals("Container mode", "", declaration.JE_ContainerMode);
			AssertEquals("Port of Unlading", "", declaration.US_SchDArrival);
			AssertEquals("Date of importation", ZDateTime.Empty, declaration.JE_DateOfArrival);
			AssertEquals("Voyage Flight Number", "", declaration.JE_VoyageFlightNo);
			AssertEquals("Date of arrival", ZDateTime.Empty, declaration.US_EntryDate);

			AssertEquals("Number of bills", 0, declaration.Bills.Count);
			AssertEquals("Master bill number", "", declaration.JE_MasterBill);
			AssertEquals("House Bill Number", "", declaration.JE_HouseBill);
			AssertMultilineASCIIEquals("Notification", BIRDDeclarationDataAdapter.CannotMergeImportEntryNumberAllocationInProgress(declaration.GetImportEntryNumberAllocationMutexLockInfo(), declaration.JE_DeclarationReference) + @"
There is no customs entry created even after merge was attempted and the system cannot proceed any more.", notifications.ToMessageListString());
			AssertEquals(0, declaration.JE_TotalNoOfPacks);
			AssertEquals("", declaration.JE_TotalNoOfPacksPackType);
			AssertEquals(0, declaration.ActiveEntryHeaders.Count);
			declarationInDiffFactory.UnlockImportEntryNumberAllocationMutex();

			notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("Job Type", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("Bond Type", BondTypeList.Codes.ContinuousBond, declaration.US_BondType);
			AssertEquals("IOR", importer.PK, declaration.IOROrgPK);
			AssertEquals("Ultimate Consignee", importer.PK, declaration.JE_OH_Importer);
			AssertEquals("EntryFilerCode", "XJ5", declaration.US_EntryFilerCode);
			AssertEquals("EntryNumber", "", declaration.ImportEntryNumber);
			AssertEquals("Entry Type", "01", declaration.US_EntryType);
			AssertEquals("Surety Code", "891", declaration.US_SuretyCode);
			AssertEquals("Destination State", "DC", declaration.US_DestinationState);
			AssertEquals("Importing vessel name", "ADMIRALENGRACHT", declaration.JE_VesselName);
			AssertEquals("Trasport Mode", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Container mode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			AssertEquals("Port of Unlading", "2809", declaration.US_SchDArrival);
			AssertEquals("Date of importation", new ZDate(2009, 06, 11), declaration.JE_DateOfArrival);
			AssertEquals("Voyage Flight Number", "56", declaration.JE_VoyageFlightNo);
			AssertEquals("Date of arrival", new ZDate(2009, 06, 11), declaration.US_EntryDate);

			AssertEquals("Number of bills", 2, declaration.Bills.Count);
			AssertEquals("Master bill number", "OB7890234I", declaration.JE_MasterBill);
			AssertEquals("House Bill Number", "HB342789", declaration.JE_HouseBill);
			AssertEquals("Master bill and house bill linked", declaration.PrimaryMasterBill.PK, declaration.PrimaryHouseBill.CU_CU_ParentBill);
			AssertEquals("Quantity", 1m, declaration.PrimaryHouseBill.CU_NoOfPacks);
			AssertEquals("UQ was too long", "??", declaration.PrimaryHouseBill.CU_PackType);
			AssertEquals("Master bill number issuer code", "AAAA", declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC);
			AssertEquals("House bill number issuer code", "APLU", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			Assert(notifications.ToMessageListString().Contains(string.Format(BIRDDeclarationDataAdapter.ManifestUQTooLong, "CTNS")));
			AssertEquals(1, declaration.JE_TotalNoOfPacks);
			AssertEquals("??", declaration.JE_TotalNoOfPacksPackType);

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Load();
			AssertEquals("BIRD transaction stored as a message", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			AssertEquals("BRD Ref number stored", "B00001001", declaration.US_BRDRefNo);
			AssertEquals(true, declaration.US_FixRecon);
			declaration.UnlockImportEntryNumberAllocationMutex();
		}

		public void TestUpdateDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001929201891  DC 20     ADMIRALENGRACHT     102809061109B00003357            56   061109         22            OB7890234I  HB342789                00000001CTNS       AAAAAPLU   30                                  0               2061709             AAAA808 40001DE00001000000000000000                    000000005060267                  50 9801007000                      X                                DE061109N   51                                                                              60                                        ECPEFCIA113MAN                        62          50100012813                                                         708802200015           000000000100NO                               0000002500  8950100000012813                                                                90                      0                       0000001281300000102500          ZZ7501000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			OrgHeader importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");
			Factory.Save();

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "";
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("Job Type", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("Bond Type", BondTypeList.Codes.ContinuousBond, declaration.US_BondType);
			AssertEquals("IOR", importer.PK, declaration.IOROrgPK);
			AssertEquals("Ultimate Consignee", importer.PK, declaration.JE_OH_Importer);
			AssertEquals("EntryFilerCode", "XJ5", declaration.US_EntryFilerCode);
			AssertEquals("EntryNumber", "60019292", declaration.ImportEntryNumber);
			AssertEquals("Entry Type", "01", declaration.US_EntryType);
			AssertEquals("Surety Code", "891", declaration.US_SuretyCode);
			AssertEquals("Destination State", "DC", declaration.US_DestinationState);
			AssertEquals("Importing vessel name", "ADMIRALENGRACHT", declaration.JE_VesselName);
			AssertEquals("Trasport Mode", TransportTypeList.Codes.Sea, declaration.JE_TransportMode);
			AssertEquals("Container mode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			AssertEquals("Port of Unlading", "2809", declaration.US_SchDArrival);
			AssertEquals("Date of importation", new ZDate(2009, 06, 11), declaration.JE_DateOfArrival);
			AssertEquals("Voyage Flight Number", "56", declaration.JE_VoyageFlightNo);
			AssertEquals("Date of arrival", new ZDate(2009, 06, 11), declaration.US_EntryDate);

			AssertEquals("Number of bills", 2, declaration.Bills.Count);
			AssertEquals("Master bill number", "OB7890234I", declaration.JE_MasterBill);
			AssertEquals("House Bill Number", "HB342789", declaration.JE_HouseBill);
			AssertEquals("Master bill and house bill linked", declaration.PrimaryMasterBill.PK, declaration.PrimaryHouseBill.CU_CU_ParentBill);
			AssertEquals("Quantity", 1m, declaration.PrimaryHouseBill.CU_NoOfPacks);
			AssertEquals("UQ was too long", "??", declaration.PrimaryHouseBill.CU_PackType);
			AssertEquals("Master bill number issuer code", "AAAA", declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC);
			AssertEquals("House bill number issuer code", "APLU", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			Assert(notifications.ToMessageListString().Contains(string.Format(BIRDDeclarationDataAdapter.ManifestUQTooLong, "CTNS")));
			AssertEquals(1, declaration.JE_TotalNoOfPacks);
			AssertEquals("??", declaration.JE_TotalNoOfPacksPackType);

			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Load();
			AssertEquals("BIRD transaction stored as a message", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);
			AssertEquals("BRD Ref number stored", "B00001001", declaration.US_BRDRefNo);
			AssertEquals(true, declaration.US_FixRecon);
			declaration.UnlockImportEntryNumberAllocationMutex();
		}

		public void TestNoWarningWhenThereIsOnlyOneExportDate()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001690001891  DC 20     APL EMERALD         102809                           V123W               22            OBL30                               00000001PK         AAAA       30                                  0               1                   AAAA    40001CH00000050000000009000                    000000005060267                  50 19021940000000032000            KG                               CH120908N  X51                                                                              60                                        XYBEREQU6LON                          62          49900001050                                                         40002CH00000024000000000050                    000000002460267                  50 1902194000          000000000500KG                               CH120908N  V51                                                                              60                                        XYBEREQU6LON                          40003FR00000013000000000050                    000000001360267                  50 0712311000          000000000500KG                               CH120908N  V51                                                                              60                                        XYBEREQU6LON                          40004IT00000013000000000050                    000000001360267                  50 2002908020          000000000500KG                               CH120908N  V51                                                                              60                                        XYBEREQU6LON                          8949900000002500                                                                9000000032000           0                       0000000250000000005000          ZZ7501000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			OrgHeader importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals("PreCondition:ExportDate is updated", new ZDateTime(2008, 12, 9), declaration.JE_ExportDate);

			//try to import again. JobDeclaration.JE_ExportDate should be cleared before import starts
			notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertNotContains("in this BIRD transaction and Enterprise has only one place to store this value.", notifications.ToUniqueMessageListString());
			AssertEquals(1, declaration.Invoices.Count);
		}

		public void TestNoWarningWhenThereIsOnlyOneLoadingPort()
		{
			var messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001690001891  DC 20     APL EMERALD         102809                           V123W               22            OBL30                               00000001PK         AAAA       30                                  0               1                   AAAA    40001CH00000050000000009000                    000000005060267                  50 19021940000000032000            KG                               CH120908N  X51                                                                              60                                        XYBEREQU6LON                          62          49900001050                                                         40002CH00000024000000000050                    000000002460267                  50 1902194000          000000000500KG                               CH120908N  V51                                                                              60                                        XYBEREQU6LON                          40003FR00000013000000000050                    000000001360267                  50 0712311000          000000000500KG                               CH120908N  V51                                                                              60                                        XYBEREQU6LON                          40004IT00000013000000000050                    000000001360267                  50 2002908020          000000000500KG                               CH120908N  V51                                                                              60                                        XYBEREQU6LON                          8949900000002500                                                                9000000032000           0                       0000000250000000005000          ZZ7501000000010                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals(@"US_SchDLoading on declaration level should be empty, because each line has its own US_SchDLoading.
Data will be set on line level", ZString.Empty, declaration.US_SchDLoading);

			//try to import again.
			notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertNotContains("No warnings expected, because line has place to store US_SchDLoading", "in this BIRD transaction and Enterprise has only one place to store this value.", notifications.ToUniqueMessageListString());
			AssertEquals(1, declaration.Invoices.Count);
		}

		public void TestMultipleInvoices()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A300191-01319900091-013199000                 8         XJ5 7000922601891  WA 20     23                  103001061509B00151416            56   061509W138     22            UIRE978     WICC09050295            00000001PK         YMLUUSNW   30                                  0               1                   YMLU808 40001GB00000300000000000000                    000000016912498                  50 4418902500          000000150000M3                               CA061509N   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100003750                                                         62          49900006300                                                         40002GB00000095000000000000                    000000005312498          INV001  50 4418902500          000000150000M3                               CA061509N   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100001188                                                         62          49900001995                                                         40003GB00000050000000000000                    000000002812498                  50 4418902500          000000150000M3                               CA061509Y   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100000625                                                         62          49900001050                                                         40003GB00000050000000000000                    000000002812498          INV002  50 4418902500          000000150000M3                               CA061509Y   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100000625                                                         62          49900001050                                                         895010000000556349900000009345                                                  90                      0                       0000001490800000044500          ZZ7501000000010                                                                 ";
			#region 80-byte view
			//B018888XJ5EI                                               31559                
			//10A300191-01319900091-013199000                 8         XJ5 7000922601891  WA 
			//20     23                  103001061509B00151416            56   061509W138     
			//22            UIRE978     WICC09050295            00000001PK         YMLUUSNW   
			//30                                  0               1                   YMLU808 
			//40001GB00000300000000000000                    000000016912498                  
			//50 4418902500          000000150000M3                               CA061509N   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100003750                                                         
			//62          49900006300                                                         
			//40002GB00000095000000000000                    000000005312498          INV001  
			//50 4418902500          000000150000M3                               CA061509N   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100001188                                                         
			//62          49900001995                                                         
			//40003GB00000050000000000000                    000000002812498                 
			//50 4418902500          000000150000M3                               CA061509Y   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100000625                                                         
			//62          49900001050                                                         
			//40003GB00000050000000000000                    000000002812498          INV002  
			//50 4418902500          000000150000M3                               CA061509Y   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100000625                                                         
			//62          49900001050                                                         
			//895010000000556349900000009345                                                  
			//90                      0                       0000001490800000044500          
			//Y  8888XJ5EI00027
			#endregion
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("two invoices created", 2, declaration.Invoices.Count);

			JobComInvoiceHeader inv001 = null, inv002 = null;

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				if (invoice.JZ_InvoiceDisplaySequence == 1)
				{
					inv001 = invoice;
				}
				else if (invoice.JZ_InvoiceDisplaySequence == 2)
				{
					inv002 = invoice;
				}
			}

			AssertNotNull("Invoice with seq. 001 exists", inv001);
			AssertEquals("Two invoice lines should exist", 2, inv001.JobComInvoiceLines.Count);

			AssertNotNull("Invoice with seq. 002 exists", inv002);
			AssertEquals("Two invoice lines should exist", 2, inv002.JobComInvoiceLines.Count);

			AssertNotEquals("Invoice Amount should have been filled in", 0m, inv001.JZ_InvoiceAmount);
			AssertNotEquals("Invoice Amount should have been filled in", 0m, inv002.JZ_InvoiceAmount);

			AssertEquals("Invoice Currency should be USD", "USD", inv001.Invoice_Currency.RX_Code);
			AssertEquals("Invoice Currency should be USD", "USD", inv002.Invoice_Currency.RX_Code);

			AssertEquals("Incoterm should have been filled in", TermsOfDeliveryList.Codes.FOB, inv001.JZ_IncoTerm);
			AssertEquals("Incoterm should have been filled in", TermsOfDeliveryList.Codes.FOB, inv002.JZ_IncoTerm);

			//the same manufacturer address is set to invoice/invoice line's JI_OA_ManufacturerAddress
			//manufacturer is created while importing and next attempt to match MID should find a new org in the factory (not saved to db yet)

			AssertEquals(inv001.JobComInvoiceLines[0].JI_OA_ManufacturerAddress, inv002.JobComInvoiceLines[0].JI_OA_ManufacturerAddress);

			AssertEquals("INV001", inv001.JZ_InvoiceNumber);
			AssertEquals("INV002", inv002.JZ_InvoiceNumber);
		}

		public void TestWhenAllThe40RecordsHaveInvoiceDelimiter()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A300191-01319900091-013199000                 8         XJ5 7000922601891  WA 20     23                  103001061509B00151416            56   061509W138     22            UIRE978     WICC09050295            00000001PK         YMLUUSNW   30                                  0               1                   YMLU808 40001GB00000300000000000000                    000000016912498          INV001  50 4418902500          000000150000M3                               CA061509N   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100003750                                                         62          49900006300                                                         40002GB00000095000000000000                    000000005312498          INV001  50 4418902500          000000150000M3                               CA061509N   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100001188                                                         62          49900001995                                                         40003GB00000050000000000000                    000000002812498          INV002  50 4418902500          000000150000M3                               CA061509Y   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100000625                                                         62          49900001050                                                         40003GB00000050000000000000                    000000002812498          INV002  50 4418902500          000000150000M3                               CA061509Y   51                                                                              52                                    0100003785001Y00000001590                 60                                        XOJOHMAT130BRA                        62          50100000625                                                         62          49900001050                                                         895010000000556349900000009345                                                  90                      0                       0000001490800000044500          ZZ7501000000010                                                                 ";
			#region 80-byte view
			//B018888XJ5EI                                               31559                
			//10A300191-01319900091-013199000                 8         XJ5 7000922601891  WA 
			//20     23                  103001061509B00151416            56   061509W138     
			//22            UIRE978     WICC09050295            00000001PK         YMLUUSNW   
			//30                                  0               1                   YMLU808 
			//40001GB00000300000000000000                    000000016912498          INV001    
			//50 4418902500          000000150000M3                               CA061509N   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100003750                                                         
			//62          49900006300                                                         
			//40002GB00000095000000000000                    000000005312498          INV001  
			//50 4418902500          000000150000M3                               CA061509N   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100001188                                                         
			//62          49900001995                                                         
			//40003GB00000050000000000000                    000000002812498         INV002  
			//50 4418902500          000000150000M3                               CA061509Y   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100000625                                                         
			//62          49900001050                                                         
			//40003GB00000050000000000000                    000000002812498          INV002  
			//50 4418902500          000000150000M3                               CA061509Y   
			//51                                                                              
			//52                                    0100003785001Y00000001590                 
			//60                                        XOJOHMAT130BRA                        
			//62          50100000625                                                         
			//62          49900001050                                                         
			//895010000000556349900000009345                                                  
			//90                      0                       0000001490800000044500          
			//Y  8888XJ5EI00027
			#endregion
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("two invoices created", 2, declaration.Invoices.Count);

			JobComInvoiceHeader inv001 = null, inv002 = null;

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				if (invoice.JZ_InvoiceDisplaySequence == 1)
				{
					inv001 = invoice;
				}
				else if (invoice.JZ_InvoiceDisplaySequence == 2)
				{
					inv002 = invoice;
				}
			}

			AssertNotNull(inv001);
			AssertNotNull(inv002);
		}

		public void TestWithSecondaryLines()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001539901891  DC 20     APL EMERALD         102809                           V123W               22            OBL71                               00000001PK         AAAA       30                                  0               1                   AAAA    40001AU00014902000000009000                    000000005060267                  50 9101118010          000005960000NO                               AU120408NAU 51                                                                              60                                        XYBEREQU6LON                          709101118020           000005960000NO                               0000601690AU809101118030           000005960000NO                               0000790840AU819101118040           000005960000NO                               0000500612AU90                      0                                  00003383342          ZZ7501000000010                                                                 ";
			#region 80-byte view
			//B018888XJ5EI                                               6004246              
			//10A888891-01319900091-013199000                 8         XJ5 6001539901891  DC 
			//20     APL EMERALD         102809                           V123W               
			//22            OBL71                               00000001PK         AAAA       
			//30                                  0               1                   AAAA    
			//40001AU00014902000000009000                    000000005060267                  
			//50 9101118010          000005960000NO                               AU120408NAU 
			//51                                                                              
			//60                                        XYBEREQU6LON                          
			//709101118020           000005960000NO                               0000601690AU
			//809101118030           000005960000NO                               0000790840AU
			//819101118040           000005960000NO                               0000500612AU
			//90                      0                                  00003383342          
			//Y  8888XJ5EI00012
			#endregion
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(4, declaration.InvoiceLines.Count);
			AssertEquals("Three secondary lines", 3, declaration.InvoiceLines[0].SecondaryTariffLines.Count());
		}

		public void TestSetCottonFeeExemptShouldNotBeSetForXLines()
		{
			string messageText = "AA7501XJ53901B00156235                  20131021235313                          10A110123-13063440023-13063440023-130634400     8061609   XJ5 7004680601891  PA 20     TITANIC             111101      B00156235            08111081111B815 001 22            MASTER081111                        00000100CS         APLU       30                                  01              2062609             APLU    40001AU00000100000000001000                    000001000060267                  50 62052020310000197000000000010000DOZ000000100000KG                AU072211N  X60                                                                              62          50100001250                                                         62          49900002100                                                         40002AU00000050000000000500                    000000500060267                  50 6205202031          000000010000DOZ000000060000KG                AU072211N  V51                  340                        999999999                        60                                                                              40003AU00000050000000000500                    000000500060267                  50 6215200000          000000010000DOZ000000010000KG                AU072211N  V51                  659                                                         60                                                                              895010000000125049900000002500                                                  9000000197000           0                       0000000375000000010000          ZACRAIG IMPORTER INC.                BLDG.E.THIRD FLOOR                         ZBCONSHOHOCKEN                  IL60046                                         ZI001USD00001000000100000000000000000 00000000000                               ZCMAEU6666666                                                                   ZZ7501000000023                                                                 ";

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("3 invoice lines", 3, declaration.InvoiceLines.Count);

			var xLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.IsSetXLine);
			AssertEquals("Should not be indicated on X lines", ZString.Empty, xLine.US_CottonFeeExempt);
		}

		public void TestSetCottonFeeExemptForLessThanThreshold()
		{
			string messageText = "AA7501SV93901B00160916                  20131104151842                          10A391091-01319900091-013199000                 8         SV9 7003363201891  AL 20     ADMIRALENGRACHT     111101111213B00160916            478  111213         22            342897324                           00000000CT         APLU       30                                  01              2112113             APLU    40001CN00000050000000000000                              57071                  50 62069000400000033500000000050000DOZ000000032500KG                CN110413N   51                  840                                                         60                                                                              62          50100000625                                                         62          49900001732                                                         40002CN00000050000000000000                              57071                  50 62069000400000033500000000050000DOZ000000100000KG                CN110413N   51                  840                        999999999                        60                                                                              62          50100000625                                                         62          49900001732                                                         894990000000346450100000001250                                                  9000000067000           0                       0000000471400000010000          ZAABC EXPORTS USA                    16 MAIN STREET                             ZBCHICAGO                       IL60611    UNIT 1                               ZI001USD00001000000100000000000000000 00000000000                               ZZ7501000000021                                                                 ";

			//AA7501SV93901B00160916                  20131104151842                          
			//10A391091-01319900091-013199000                 8         SV9 7003363201891  AL 
			//20     ADMIRALENGRACHT     111101111213B00160916            478  111213         
			//22            342897324                           00000000CT         APLU       
			//30                                  01              2112113             APLU    
			//40001CN00000050000000000000                              57071                  
			//50 62069000400000033500000000050000DOZ000000032500KG                CN110413N   
			//51                  840                                                         
			//60                                                                              
			//62          50100000625                                                         
			//62          49900001732                                                         
			//40002CN00000050000000000000                              57071                  
			//50 62069000400000033500000000050000DOZ000000100000KG                CN110413N   
			//51                  840                        999999999                        
			//60                                                                              
			//62          50100000625                                                         
			//62          49900001732                                                         
			//894990000000346450100000001250                                                  
			//9000000067000           0                       0000000471400000010000          
			//ZAABC EXPORTS USA                    16 MAIN STREET                             
			//ZBCHICAGO                       IL60611    UNIT 1                               
			//ZI001USD00001000000100000000000000000 00000000000                               
			//ZZ7501000000021

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("2 invoice line", 2, declaration.InvoiceLines.Count);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var entryLine1 = entry.MergedLines.FindByLineNumber(1);
			var entryLine2 = entry.MergedLines.FindByLineNumber(2);

			AssertEquals("first line not exempt. Cotton exempt code does not exist", YesNoDefaultList.Codes.No, entryLine1.RandomLine.US_CottonFeeExempt);
			AssertEquals("Second line should be exempt. 99999999 exists", YesNoDefaultList.Codes.Yes, entryLine2.RandomLine.US_CottonFeeExempt);
		}

		[NUnit.Framework.TestDate(2013, 1, 1)]
		public void TestSetCottonFeeExempt()
		{
			string messageText = "AA7501XJ58888B00003262                  20131023185040                          10A888813-26220360013-262203600                 8         XJ5 6001851801891  DC 20     APL EMERALD         102809      B00003262            V123W               22            UIOYWER978                          00000001PK         APLU       30                                  01              1                   APLU    40001TW00000100000000001800                    000000004960267                  50 62069000400000067000000000083300DOZ000000180000KG                TW120908N   51                  840                                                         60                                        XYBEREQU6LON                          62          49900003464                                                         62620690004005600000370                                                         40002TW00000001550000001800                    000000000160267                  50 62069000400000001039000000083300DOZ000000180000KG                TW120908N   51                  840                        999999999                        60                                        XYBEREQU6LON                          62          49900000054                                                         40003TW00000000110000001800                              60267                  50 62069000400000000074000000083300DOZ000000180000KG                TW120908N   51                  840                                                         60                                        XYBEREQU6LON                          62620690004005600000370                                                         62          49900000004                                                         894990000000352205600000000740                                                  9000000068113           0                       0000000426200000010166          ZASKYLINE SOUTH INC                  1125 NORTHMEADOW PARKWAY                   ZBGEORGIA                       GA30076    SUITE 100 ROSWELL                    ZI001USD00001016600100000000000000000 00000000000                               ZZ7501000000026                                                                 ";

			#region 80-byte view

			//AA7501XJ58888B00003262                  20131023185040                          
			//10A888813-26220360013-262203600                 8         XJ5 6001851801891  DC 
			//20     APL EMERALD         102809      B00003262            V123W               
			//22            UIOYWER978                          00000001PK         APLU       
			//30                                  01              1                   APLU    
			//40001TW00000100000000001800                    000000004960267                  
			//50 62069000400000067000000000083300DOZ000000180000KG                TW120908N   
			//51                  840                                                         
			//60                                        XYBEREQU6LON                          
			//62          49900003464                                                         
			//62620690004005600000370                                                         
			//40002TW00000001550000001800                    000000000160267                  
			//50 62069000400000001039000000083300DOZ000000180000KG                TW120908N   
			//51                  840                        999999999                        
			//60                                        XYBEREQU6LON                          
			//62          49900000054                                                         
			//40003TW00000000110000001800                              60267                  
			//50 62069000400000000074000000083300DOZ000000180000KG                TW120908N   
			//51                  840                                                         
			//60                                        XYBEREQU6LON                          
			//62620690004005600000370                                                         
			//62          49900000004                                                         
			//894990000000352205600000000740                                                  
			//9000000068113           0                       0000000426200000010166          
			//ZASKYLINE SOUTH INC                  1125 NORTHMEADOW PARKWAY                   
			//ZBGEORGIA                       GA30076    SUITE 100 ROSWELL                    
			//ZI001USD00001016600100000000000000000 00000000000                               
			//ZZ7501000000026

			#endregion
			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("3 invoice lines", 3, declaration.InvoiceLines.Count);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine1 = entry.MergedLines.FindByLineNumber(1);
			var entryLine2 = entry.MergedLines.FindByLineNumber(2);
			var entryLine3 = entry.MergedLines.FindByLineNumber(3);

			AssertEquals("Should be indicated to N", YesNoDefaultList.Codes.No, entryLine1.RandomLine.US_CottonFeeExempt);
			AssertNotEquals(0m, entryLine1.CottonAmount);

			AssertEquals("Should be indicated to Y", YesNoDefaultList.Codes.Yes, entryLine2.RandomLine.US_CottonFeeExempt);
			AssertEquals(0m, entryLine2.CottonAmount);
			AssertEquals("9999999 in message should not be set as it is redundant", ZString.Empty, entryLine2.RandomLine.US_CottonCertificateNo);

			AssertEquals("Should be indicated to N", YesNoDefaultList.Codes.No, entryLine3.RandomLine.US_CottonFeeExempt);
			AssertNotEquals(0m, entryLine3.CottonAmount);
		}

		[NUnit.Framework.TestDate(2013, 1, 1)]
		public void TestImportParentAndSecondaryLineWhereSecondaryLineIsApplicable()
		{
			string messageText = "AA7501XJ58888B00005394                  20131023193625                          10A888812-12345670012-12345670012-123456700     8         XJ5 6003225301891  AK 20     ADMIRALENGRACHT     111101020212B00005394            2349 020212         22            43290                               00000001PK         APLU       30                                  01              2021412             APLU    40001TH00000070000000000100                    000000010060267                  50 61042200100000111300000000010000DOZ000000050000KG                AU013012N   51                  335                                                         60                                        THLIATHA191NAK                        62          50100000875                                                         62          49900002425                                                         62610220002005600000417                                                         706102200020           000000010000DOZ000000050000KG                            89501000000008754990000000250005600000000417                                    9000000111300           0                       0000000379200000007000          ZAABC EXPORTS USA                    1645 MAIN STREET                           ZBCHICAGO                       IL60611    UNIT 1                               ZI001USD00000700000100000000000000000 00000000000                               ZZ7501000000017                                                                 ";

			#region 80-byte view

			//AA7501XJ58888B00005394                  20131023193625                          
			//10A888812-12345670012-12345670012-123456700     8         XJ5 6003225301891  AK 
			//20     ADMIRALENGRACHT     111101020212B00005394            2349 020212         
			//22            43290                               00000001PK         APLU       
			//30                                  01              2021412             APLU    
			//40001TH00000070000000000100                    000000010060267                  
			//50 61042200100000111300000000010000DOZ000000050000KG                AU013012N   
			//51                  335                                                         
			//60                                        THLIATHA191NAK                        
			//62          50100000875                                                         
			//62          49900002425                                                         
			//62610220002005600000417                                                         
			//706102200020           000000010000DOZ000000050000KG                            
			//89501000000008754990000000250005600000000417                                    
			//9000000111300           0                       0000000379200000007000          
			//ZAABC EXPORTS USA                    1645 MAIN STREET                           
			//ZBCHICAGO                       IL60611    UNIT 1                               
			//ZI001USD00000700000100000000000000000 00000000000                               
			//ZZ7501000000017

			#endregion

			var generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			var declaration = Factory.New<JobDeclaration>();
			var notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("2 invoice lines", 2, declaration.InvoiceLines.Count);

			var parentLine = declaration.InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault(x => x.ParentTariffLine == null);
			var secondaryLine = parentLine.SecondaryTariffLines.ElementAt(0);

			AssertEquals("parent line should not be indicated as cotton fee is not applicable", ZString.Empty, parentLine.US_CottonFeeExempt);
			AssertEquals("cotton fee in 62 record comes from secondary line therefore not exempt", YesNoDefaultList.Codes.No, secondaryLine.US_CottonFeeExempt);
		}

		//test for adding lines
		//test for removing lines

		public void TestInformalFeeDeletedWhenTheSecondImportChangesToFormalEntry()
		{
			//informal entry type
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001767611891  DC 20     APL EMERALD         102809                           V123W               22            OBL66                               00000001PK         AAAA       30                                  0 P             1                   AAAA    3431100000200                                                                   40001HK00000030000000009000                    000000005060267                  50 74199930000000009000            KG                               AU120908    51                                                                              521                                                                             60                                        KNBEREQU6LON                          8931100000000200                                                                9000000009000           0                       0000000020000000003000          ZZ7501000000010                                                                 ";
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			NotificationCollection notifications = new NotificationCollection();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("PreCondition", EntryTypeList.Codes.InformalFreeDutiable, declaration.US_EntryType);
			AssertEquals("Informal fee is persisted OK", 2m, declaration.ActiveEntryHeaders.EntrySummaryEntry.InformalFee);

			messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 6001539901891  DC 20     APL EMERALD         102809                           V123W               22            OBL71                               00000001PK         AAAA       30                                  0               1                   AAAA    40001AU00014902000000009000                    000000005060267                  50 9101118010          000005960000NO                               AU120408NAU 51                                                                              60                                        XYBEREQU6LON                          709101118020           000005960000NO                               0000601690AU809101118030           000005960000NO                               0000790840AU819101118040           000005960000NO                               0000500612AU90                      0                                  00003383342          ZZ7501000000010                                                                 ";

			//AA7501XJ58888B00001001                  200901011212120100  6004772             
			//10A888891-01319900091-013199000                 8         XJ5 6001539901891  DC 
			//20     APL EMERALD         102809                           V123W               
			//22            OBL71                               00000001PK         AAAA       
			//30                                  0               1                   AAAA    
			//40001AU00014902000000009000                    000000005060267                  
			//50 9101118010          000005960000NO                               AU120408NAU 
			//51                                                                              
			//60                                        XYBEREQU6LON                          
			//709101118020           000005960000NO                               0000601690AU
			//809101118030           000005960000NO                               0000790840AU
			//819101118040           000005960000NO                               0000500612AU
			//90                      0                                  00003383342          
			//ZZ7501000000010                                                                 

			generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);
			AssertEquals("PreCondition", EntryTypeList.Codes.ConsumptionFreeDutiable, declaration.US_EntryType);
			AssertEquals("Informal Fee deleted", 0m, declaration.ActiveEntryHeaders.EntrySummaryEntry.InformalFee);
			AssertEquals("CusEntryLine.CL_CustomsValue should have been refreshed by merging again", 1490200m, declaration.ActiveEntryHeaders.EntrySummaryEntry.MergedLines[0].CL_CustomsValue);
		}

		public void TestPersistFDAUQsCorrectly()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10R888891-01319900091-013199000                 8         XJ5 7002182502891  IL 20     23                  103001081709B00152494            56   081709I299     22            8IDFS                               00000001PK         APLU       30                                  01              2082709             APLU    40001SV00000000000000000000                    000000020057020                  50 99150490  0000150900000000100000KG                               SV081709YP+ 51                                                                              60                                        GBBOOMED295LON                        62          50100001250                                                         700406100800           000000100000KG                               0000010000  OI        TEST                                                                  FD0100104BGT02   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    FD020000000100PK  0000004000PL  0000002000CT  0000003000BX  0000025000BV        FD03                      TEST                                                  FD040000000100KG  TOMAS TANK6309991234                                          FD05ADA08172009                                                                 FD05APA3001                                                                     FD05ATA1200                                                                     FD05CSHSV                                                                       FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTM                                                                        FD05SA1350 W 63RD STREET                                                        FD05SACWILLOWBROOK                                                              FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNTOMAS CO                                                                 FD05SCZ60527                                                                    FD05SEMNONE                                                                     FD05SFNTOMAS CO                                                                 FD05SFTI                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT56                                                                       FD05BOLAPLU8IDFS                                                                8950100000001250                                                                9000000150900           0                       0000000125000000010000          ZZ7501000000010                                                                 ";
			#region 80-byte version
			//B018888XJ5EI                                               35039                
			//10R888891-01319900091-013199000                 8         XJ5 7002182502891  IL 
			//20     23                  103001081709B00152494            56   081709I299     
			//22            8IDFS                               00000001PK         APLU       
			//30                                  01              2082709             APLU    
			//40001SV00000000000000000000                    000000020057020                  
			//50 99150490  0000150900000000100000KG                               SV081709YP+ 
			//51                                                                              
			//60                                        GBBOOMED295LON                        
			//62          50100001250                                                         
			//700406100800           000000100000KG                               0000010000  
			//OI        TEST                                                                  
			//FD0100104BGT02   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    
			//FD020000000100PK  0000004000PL  0000002000CT  0000003000BX  0000025000BV        
			//FD03                      TEST                                                  
			//FD040000000100KG  TOMAS TANK6309991234                                          
			//FD05ADA08172009                                                                 
			//FD05APA3001                                                                     
			//FD05ATA1200                                                                     
			//FD05CSHSV                                                                       
			//FD05OFTI                                                                        
			//FD05PFR12345678901                                                              
			//FD05PFTM                                                                        
			//FD05SA1350 W 63RD STREET                                                        
			//FD05SACWILLOWBROOK                                                              
			//FD05SASIL                                                                       
			//FD05SCCUS                                                                       
			//FD05SCNTOMAS CO                                                                 
			//FD05SCZ60527                                                                    
			//FD05SEMNONE                                                                     
			//FD05SFNTOMAS CO                                                                 
			//FD05SFTI                                                                        
			//FD05SFX0000000000                                                               
			//FD05SPN5555555555                                                               
			//FD05VFT56                                                                       
			//FD05BOLAPLU8IDFS                                                                
			//8950100000001250                                                                
			//9000000150900           0                       0000000125000000010000          
			//Y  8888XJ5EI00037000000150900

			#endregion

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("There should be 1 invoice line", 1, declaration.InvoiceLines.Count);
			AssertEquals("There should be 2 merged lines", 2, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("Sup Tariff", "99150490", invoiceLine.US_SupTariff);
			AssertEquals("Classification Tariff", "0406100800", invoiceLine.JI_Tariff);

			AssertEquals("There should be one FDA line", 1, invoiceLine.FDAs.Count);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
		}

		public void TestSupplementaryTariffs()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 <E#PLCH>01891  WA 20     23                  118888      B00001045            131  032210X117     22            559832203CW 46464646546             00000012KG         MAEUAMAW   30                                  01              1                   MAEU    40001LS00000020000000000100                    000000009057035                  50 9817005000                      X                                LS031810Y   51                                                                              60                                        AUABCEXP72ALE                         62          50100006500                                                         707102390050           000000007000CAR                              0000050000  40002CN00000001000000000420                    000000000957035                  50 41041950800000000330000000001000KG                               CN031810Y   51                                                                              60                                        AUABCEXP72ALE                         62          50100000670                                                         62          49900001124                                                         703402112000 0000032500000000015000KG                               0000005000  804503902000           000000001000KG                               0000000100  815601210090 0000000540000000001500KG                               0000000150  895010000000717049900000002500                                                  9000000033370           0                       0000000967000000057350          ZZ7501000000010                                                                 ";

			#region 80-byte version

			//AA7501XJ5    B00001045                  20100325190301                          
			//10A888891-01319900091-013199000                 8         XJ5 <E#PLCH>01891  WA 
			//20     23                  118888      B00001045            131  032210X117     
			//22            559832203CW 46464646546             00000012KG         MAEUAMAW   
			//30                                  01              1                   MAEU    
			//40001LS00000020000000000100                    000000009057035                  
			//50 9817005000                      X                                LS031810Y   
			//51                                                                              
			//60                                        AUABCEXP72ALE                         
			//62          50100006500                                                         
			//707102390050           000000007000CAR                              0000050000  
			//40002CN00000001000000000420                    000000000957035                  
			//50 41041950800000000330000000001000KG                               CN031810Y   
			//51                                                                              
			//60                                        AUABCEXP72ALE                         
			//62          50100000670                                                         
			//62          49900001124                                                         
			//703402112000 0000032500000000015000KG                               0000005000  
			//804503902000           000000001000KG                               0000000100  
			//815601210090 0000000540000000001500KG                               0000000150  
			//895010000000717049900000002500                                                  
			//9000000033370           0                       0000000967000000057350          
			//ZAABC EXPORTS USA                    16 MAIN STREET                             
			//ZBCHICAGO                       IL60611    UNIT 1                               
			//ZI001USD00005735000100000000000000000 00000000000                               
			//ZCAKCU9012171                                                                   
			//ZZ7501000000025

			#endregion

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("There should be 5 invoice lines - first line with Sup Tariff", 5, declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("SupTariff", "9817005000", invoiceLine.US_SupTariff);
			AssertEquals("Sup Qty UQ", "X", invoiceLine.US_SupUQ1);
			AssertEquals("Tariff", "7102390050", invoiceLine.JI_Tariff);
			AssertEquals("Line Price should be summarized", 52000m, invoiceLine.JI_LinePrice);

			AssertEquals("There should be 6 merged Lines", 6, declaration.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals("There is Sup Line", true, declaration.ActiveEntryHeaders[0].MergedLines[0].US_SupLine);
			AssertEquals("There is Sup Line with tariff 9817005000", "9817005000", declaration.ActiveEntryHeaders[0].MergedLines[0].ImportTariff.UE_Tariff);
			AssertEquals("There is child line with tariff 7102390050", "7102390050", declaration.ActiveEntryHeaders[0].MergedLines[1].ImportTariff.UE_Tariff);

			invoiceLine = declaration.InvoiceLines[1];
			AssertEquals("SupTariff", "", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "4104195080", invoiceLine.JI_Tariff);
			AssertEquals("Line Price", 100m, invoiceLine.JI_LinePrice);
			AssertEquals("4104195080", declaration.ActiveEntryHeaders[0].MergedLines[2].ImportTariff.UE_Tariff);

			invoiceLine = declaration.InvoiceLines[2];
			AssertEquals("SupTariff", "", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "3402112000", invoiceLine.JI_Tariff);
			AssertEquals("Line Price", 5000m, invoiceLine.JI_LinePrice);
			AssertEquals("3402112000", declaration.ActiveEntryHeaders[0].MergedLines[3].ImportTariff.UE_Tariff);

			invoiceLine = declaration.InvoiceLines[3];
			AssertEquals("SupTariff", "", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "4503902000", invoiceLine.JI_Tariff);
			AssertEquals("Line Price", 100m, invoiceLine.JI_LinePrice);
			AssertEquals("4503902000", declaration.ActiveEntryHeaders[0].MergedLines[4].ImportTariff.UE_Tariff);

			invoiceLine = declaration.InvoiceLines[4];
			AssertEquals("SupTariff", "", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "5601210090", invoiceLine.JI_Tariff);
			AssertEquals("Line Price", 150m, invoiceLine.JI_LinePrice);
			AssertEquals("5601210090", declaration.ActiveEntryHeaders[0].MergedLines[5].ImportTariff.UE_Tariff);
		}

		public void TestWatchRepairTariffs()
		{
			string messageText = "AA7501XJ58888B00001344                  20100414201601                          10A888813-262203600                             8      E  XJ5         01891     20     APL EMERALD         103901      B00001344            498  102907         22            88745132                            00000000           APLU       30                                  11              1                   APLU    40001AU00000034060000000100                    000000050060267                  42ECPEFCIA113MAN 23456            00010008                                      43      D OTH ARTL EXP REPAIR/ALT WA                                            43      D WATCH,BAT POW,MECH DISP AU                                            43      D WATCH CASE, BAT POW, AU, A                                            43      D WATCH STRAP, BAND OR BRACE                                            43      D WATCH BATTERY,BAT POW, AU,                                            50 9802004040                                                         101007N   51                                                                              60                                        ECPEFCIA113MAN                        709102111010 0000032704000000100000NO                               0000005280  809802004040                                                        0000002619  819102111020           000000100000NO                                           819802004040                                                        0000001345  819102111030           000000100000NO                                           819802004040                                                        0000000204  819102111040           000000100000NO                                           9000000032704                                              00000012854          ZI001USD00000340600100000000000000000 00000000000                               ZZ7501000000023                                                                 ";

			#region 80-byte version

			//AA7501XJ58888B00001344                  20100414201601                          
			//10A888813-262203600                             8      E  XJ5         01891     
			//20     APL EMERALD         103901      B00001344            498  102907         
			//22            88745132                            00000000           APLU       
			//30                                  11              1                   APLU    
			//40001AU00000034060000000100                    000000050060267                  
			//42ECPEFCIA113MAN 23456            00010008                                      
			//43      D OTH ARTL EXP REPAIR/ALT WA                                            
			//43      D WATCH,BAT POW,MECH DISP AU                                            
			//43      D WATCH CASE, BAT POW, AU, A                                            
			//43      D WATCH STRAP, BAND OR BRACE                                            
			//43      D WATCH BATTERY,BAT POW, AU,                                            
			//50 9802004040                                                         101007N   
			//51                                                                              
			//60                                        ECPEFCIA113MAN                        
			//709102111010 0000032704000000100000NO                               0000005280  
			//809802004040                                                        0000002619  
			//819102111020           000000100000NO                                           
			//819802004040                                                        0000001345  
			//819102111030           000000100000NO                                           
			//819802004040                                                        0000000204  
			//819102111040           000000100000NO                                           
			//9000000032704                                              00000012854          
			//ZI001USD00000340600100000000000000000 00000000000                               
			//ZZ7501000000023                                                                

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals("There should be 4 invoice lines", 4, declaration.InvoiceLines.Count);
			AssertEquals("There should be 8 merged Lines", 8, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("SupTariff", "9802004040", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "9102111010", invoiceLine.JI_Tariff);
			AssertEquals("US/Orig Value", 3406.00m, invoiceLine.US_98GoodsValue);
			AssertEquals("Line Price", 5280m, invoiceLine.JI_LinePrice);
			AssertEquals("CE", ZString.Empty, invoiceLine.US_UC_NKCountryOfExport);
			AssertEquals("CO should not be updated from mid. It should just be updated from C/O in ENS40", "AU", invoiceLine.US_UC_NKCountryOfOrigin);

			invoiceLine = declaration.InvoiceLines[1];
			AssertEquals("SupTariff", "9802004040", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "9102111020", invoiceLine.JI_Tariff);
			AssertEquals("US/Orig Value", 2619.00m, invoiceLine.US_98GoodsValue);
			AssertEquals("Line Price should be empty", 0m, invoiceLine.JI_LinePrice);
			AssertEquals(1000m, invoiceLine.JI_CustomsQuantity);

			invoiceLine = declaration.InvoiceLines[2];
			AssertEquals("SupTariff", "9802004040", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "9102111030", invoiceLine.JI_Tariff);
			AssertEquals("US/Orig Value", 1345.00m, invoiceLine.US_98GoodsValue);
			AssertEquals("Invoice Price", 0m, invoiceLine.JI_LinePrice);
			AssertEquals(1000m, invoiceLine.JI_CustomsQuantity);

			invoiceLine = declaration.InvoiceLines[3];
			AssertEquals("SupTariff", "9802004040", invoiceLine.US_SupTariff);
			AssertEquals("Tariff", "9102111040", invoiceLine.JI_Tariff);
			AssertEquals("US/Orig Value", 204.00m, invoiceLine.US_98GoodsValue);
			AssertEquals("Line Price should be empty", 0m, invoiceLine.JI_LinePrice);
			AssertEquals(1000m, invoiceLine.JI_CustomsQuantity);
			declaration.UnlockImportEntryNumberAllocationMutex();
		}

		public void TestUpdateToCargoRelease()
		{
			string messageText = "AA3461J58888B00001001                  200901011212120100  6004772              H1A8888XJ5 7001870691-0131990004007140981                   AA  390101891       H2I317    91-013199000 150  0000004058B00152390                                 HA            00155546514                         00000002PK                    H5001GB9018908000GBBOOMED295LON             0000004058                          OI        WHAT                                                                  FD0100176E--AX   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    FD020000010000PCS                                                               FD030000000500                                                                  FD04              TOMAS TANK6309991234                                          FD05ADA07142009                                                                 FD05APA3901                                                                     FD05ATA1200                                                                     FD05CFR8784800                                                                  FD05CSHGB                                                                       FD05DEV1417592                                                                  FD05LSTB117902                                                                  FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTG                                                                        FD05SA1350 W 63RD STREET                                                        FD05SACWILLOWBROOK                                                              FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNTOMAS CO                                                                 FD05SCZ60527                                                                    FD05SEMNONE                                                                     FD05SFNTOMAS CO                                                                 FD05SFTF                                                                        FD05SFX0000000000                                                               FD05SPN5555555555                                                               FD05VFT150                                                                      FD05AWB00155546514                                                              ZZ7501000000010                                                                 ";
			#region 80-byte version
			//B018888XJ5HI                                               32956                
			//H1A8888XJ5 7001870691-0131990004007140981                   AA  390101891       
			//H2I317    91-013199000 150  0000004058B00152390                                 
			//HA            00155546514                         00000002PK                    
			//H5001GB9018908000GBBOOMED295LON             0000004058                          
			//OI        WHAT                                                                  
			//FD0100176E--AX   GBSLNFLJKDFSR90               GBBOOMED295LON GBBOOMED295LON    
			//FD020000010000PCS                                                               
			//FD030000000500                                                                  
			//FD04              TOMAS TANK6309991234                                          
			//FD05ADA07142009                                                                 
			//FD05APA3901                                                                     
			//FD05ATA1200                                                                     
			//FD05CFR8784800                                                                  
			//FD05CSHGB                                                                       
			//FD05DEV1417592                                                                  
			//FD05LSTB117902                                                                  
			//FD05OFTI                                                                        
			//FD05PFR12345678901                                                              
			//FD05PFTG                                                                        
			//FD05SA1350 W 63RD STREET                                                        
			//FD05SACWILLOWBROOK                                                              
			//FD05SASIL                                                                       
			//FD05SCCUS                                                                       
			//FD05SCNTOMAS CO                                                                 
			//FD05SCZ60527                                                                    
			//FD05SEMNONE                                                                     
			//FD05SFNTOMAS CO                                                                 
			//FD05SFTF                                                                        
			//FD05SFX0000000000                                                               
			//FD05SPN5555555555                                                               
			//FD05VFT150                                                                      
			//FD05AWB00155546514                                                              
			//Y  8888XJ5HI00032
			#endregion
			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(true, declaration.US_EnableCRL);
			AssertEquals(TransportTypeList.Codes.Air, declaration.JE_TransportMode);

			AssertEquals(1, declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLine.US_FDAIndicator);
			AssertEquals(1, invoiceLine.FDAs.Count);

			FDA fdaLine = invoiceLine.FDAs[0];
			AssertEquals("FDA line number", (ZShort)1, fdaLine.US_FDALineNo);
			AssertEquals("FDA commercial desc", "WHAT", fdaLine.US_FDACommercialDesc);
			AssertEquals("Product Code", "76E--AX", fdaLine.US_FDAProductCode);
			AssertEquals("Country of production", "GB", fdaLine.US_UC_NKFDAProduction);
			AssertEquals("FDA value", 500m, fdaLine.US_FDAValue);
			AssertEquals("Unit Quantity", 100m, fdaLine.US_FDAQty1);
			AssertEquals("Unit", "PCS", fdaLine.US_FDAMeasure1);
		}

		public void TestUpdateToBorderCargoRelease()
		{
			string messageText = "AA3461XJ58888B00001001                  200901011212120100  6004772             01A8888XJ5700066282191-013199000889191-01319900002190901 AAGC                   0M            OB9384                              00000010PK         AAGC       02OM4906000000XOAGRPRO5001TOR             0000675000                            ZZ7501000000010                                                                 ";

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(true, declaration.US_EnableCRL);
			AssertEquals(TransportTypeList.Codes.Rail, declaration.JE_TransportMode);

			AssertEquals(1, declaration.InvoiceLines.Count);
		}

		public void TestProcessOGABlocks()
		{
			string messageText = "AA7501XJ58888B00001001                  200901011212120100  6004772             10A888891-01319900091-013199000                 8         XJ5 7002194001891  AK 20     23                  102809082509B00152513            56   082509H572     22            IF7JISDF                            00000001PK         APLU       30                                  01              2090409             APLU    40001AU00000100000000000000                    000000005060267                  50 9027205050          000000520000NO                               AU082509YAU OA  FD0                                                                         OI        DESC1                                                                 FC0102 001                 ABC                           J90394-1               FC02000000000050                                                                OI        2                                                                     FC0102 002                 DEF                           J90394-2               FC02000000000020                                                                51                                                                              60                                        ZABHPBIL1001RAN                       62          50100001250                                                         8950100000001250                                                                90                      0                       0000000125000000010000          ZZ7501000000010                                                                 ";
			//B018888XJ5EI                                               35101                
			//10A888891-01319900091-013199000                 8         XJ5 7002194001891  AK 
			//20     23                  102809082509B00152513            56   082509H572     
			//22            IF7JISDF                            00000001PK         APLU       
			//30                                  01              2090409             APLU    
			//40001AU00000100000000000000                    000000005060267                  
			//50 9027205050          000000520000NO                               AU082509YAU 
			//OA  FD0                                                                         
			//OI        DESC1                                                                 
			//FC0102 001                 ABC                           J90394-1               
			//FC02000000000050                                                                
			//OI        2                                                                     
			//FC0102 002                 DEF                           J90394-2               
			//FC02000000000020                                                                
			//51                                                                              
			//60                                        ZABHPBIL1001RAN                       
			//62          50100001250                                                         
			//8950100000001250                                                                
			//90                      0                       0000000125000000010000          
			//Y  8888XJ5EI00018    

			ABIInputBlockControlGenerator<BRDAA, BRDZZ> generator = new ABIInputBlockControlGenerator<BRDAA, BRDZZ>();
			generator.Deserialise(messageText);

			NotificationCollection notifications = new NotificationCollection();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			new BIRDDeclarationDataAdapter().DoImport(declaration, generator, notifications);

			AssertEquals(1, declaration.InvoiceLines.Count);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			AssertEquals("FDA is disclaimed", OGAIndicatorList.Codes.Disclaimed, invoiceLine.US_FDAIndicator);
		}

		OrgHeader CreateOrganisation(ZString codeType, ZString number)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();

			result.CustomsCodes.AddNew(codeType, number, GlbCompany.CurrentCompany.Country);

			return result;
		}
	}
}
