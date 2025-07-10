using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class EntryNumberDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBizObjectProvider()
		{
			var dataObject = new EntryNumber
			{
				Type = new EntryType { Code = "AAA" },
				Number = "111"
			};

			var shipment = Factory.New<CommonShipment>();

			var cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_ParentID = shipment.PK;
			cusEntryNumber.CE_ParentTable = shipment.TableName;
			cusEntryNumber.CE_EntryType = "BBB";
			cusEntryNumber.CE_EntryNum = "222";

			var reader = new EntryNumberDataObjectReader(dataObject, logger, Factory, shipment, data => cusEntryNumber);
			AssertEquals(cusEntryNumber, reader.ReadIntoBusinessObject());
			AssertEquals("BBB", cusEntryNumber.CE_EntryType);
			AssertEquals("222", cusEntryNumber.CE_EntryNum);

			cusEntryNumber.CE_EntryIsSystemGenerated = false;

			reader = new EntryNumberDataObjectReader(dataObject, logger, Factory, shipment, data => cusEntryNumber);
			AssertEquals(cusEntryNumber, reader.ReadIntoBusinessObject());
			AssertEquals("AAA", cusEntryNumber.CE_EntryType);
			AssertEquals("111", cusEntryNumber.CE_EntryNum);
		}

		public void TestSystemGeneratedCusEntryNumbersAreNotUpdatedFromIncomingSystemGeneratedData()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);
			entryNumberBO.CE_EntryIsSystemGenerated = true;

			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.EntryIsSystemGenerated = true;
			entryNumberDataObject.EntryLineReference = "SOMETHING";

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			entryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(entryNumberBO);

			CombineAssertions("The system generated Entry Number should not have been changed", delegate
			{
				AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", true, entryNumberBO.CE_EntryIsSystemGenerated);
				AssertEquals("entryNumberBO.CE_EntryLineReference", "REFERENCE", entryNumberBO.CE_EntryLineReference);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestNonSystemGeneratedCusEntryNumberIsUpdatedFromSystemGeneratedCusEntryNumber()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);

			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.EntryIsSystemGenerated = true;

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);

			CombineAssertions(delegate
			{
				AssertContentsWithReadOnly(updatedEntryNumberBO, shipmentBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestSZBCusEntryNumbersAreNotUpdatedWhenDataObjectIsOutdated()
		{
			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.EntryIsSystemGenerated = false;
			entryNumberDataObject.IssueDate = new ZDateTime(2022, 11, 11, 11, 11, 11);
			entryNumberDataObject.Type = new EntryType { Code = "SZB", Description = "SZB Number" };
			entryNumberDataObject.Number = "CE00001";

			var consolBO1 = Factory.New<CommonConsol>();

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, consolBO1.CusEntryNumsForAllCountries, consolBO1);
			var updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "CUS", updatedEntryNumberBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", updatedEntryNumberBO.CE_EntryType);
				AssertEquals("CE_EntryNum", "CE00001", updatedEntryNumberBO.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2022, 11, 11, 11, 11, 11), updatedEntryNumberBO.CE_IssueDate);

				AssertMultilineASCIIEquals(@"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			var consolBO2 = Factory.New<CommonConsol>();

			entryNumberDataObject.Number = "WithoutIssueDate01";
			entryNumberDataObject.IssueDate = null;

			logger.ClearLogs();
			reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, consolBO2.CusEntryNumsForAllCountries, consolBO2);
			updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "CUS", updatedEntryNumberBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", updatedEntryNumberBO.CE_EntryType);
				AssertEquals("CE_EntryNum", "WithoutIssueDate01", updatedEntryNumberBO.CE_EntryNum);
				AssertEquals("CE_IssueDate", ZDateTime.Empty, updatedEntryNumberBO.CE_IssueDate);

				AssertMultilineASCIIEquals(@"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			var consolBO3 = Factory.New<CommonConsol>();

			entryNumberDataObject.Number = "UPDATE01";
			entryNumberDataObject.IssueDate = new ZDateTime(2022, 10, 10, 10, 10, 10);

			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(consolBO3);
			entryNumberBO.CE_IssueDate = new ZDateTime(2022, 11, 11, 11, 11, 11);
			entryNumberBO.CE_EntryNum = "CE00001";
			entryNumberBO.CE_Category = "CUS";
			entryNumberBO.CE_EntryType = "SZB";

			logger.ClearLogs();
			reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, consolBO3.CusEntryNumsForAllCountries, consolBO3);
			updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "CUS", updatedEntryNumberBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", updatedEntryNumberBO.CE_EntryType);
				AssertEquals("CE_EntryNum - Not Update (DateObject is Outdated) ", "CE00001", updatedEntryNumberBO.CE_EntryNum);
				AssertEquals("CE_IssueDate - Not Update (DateObject is Outdated)", new ZDateTime(2022, 11, 11, 11, 11, 11), updatedEntryNumberBO.CE_IssueDate);

				AssertMultilineASCIIEquals(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			var consolBO4 = Factory.New<CommonConsol>();

			entryNumberDataObject.Number = "UPDATE02";
			entryNumberDataObject.IssueDate = new ZDateTime(2022, 11, 12, 00, 00, 00);

			entryNumberBO.CE_ParentID = consolBO4.PK;
			entryNumberBO.CE_IssueDate = new ZDateTime(2022, 11, 11, 11, 11, 11);
			entryNumberBO.CE_EntryNum = "CE00001";

			logger.ClearLogs();
			reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, consolBO4.CusEntryNumsForAllCountries, consolBO4);
			updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "CUS", updatedEntryNumberBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", updatedEntryNumberBO.CE_EntryType);
				AssertEquals("CE_EntryNum - Update", "UPDATE02", updatedEntryNumberBO.CE_EntryNum);
				AssertEquals("CE_IssueDate - Update", new ZDateTime(2022, 11, 12, 00, 00, 00), updatedEntryNumberBO.CE_IssueDate);

				AssertMultilineASCIIEquals(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			var consolBO5 = Factory.New<CommonConsol>();

			entryNumberDataObject.Number = "WithoutIssueDate02";
			entryNumberDataObject.IssueDate = null;

			entryNumberBO.CE_ParentID = consolBO5.PK;
			entryNumberBO.CE_IssueDate = new ZDateTime(2022, 10, 10, 10, 10, 10);
			entryNumberBO.CE_EntryNum = "CE00001";

			logger.ClearLogs();
			reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, consolBO5.CusEntryNumsForAllCountries, consolBO5);
			updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "CUS", updatedEntryNumberBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", updatedEntryNumberBO.CE_EntryType);
				AssertEquals("CE_EntryNum - Update (Without IssueDate)", "WithoutIssueDate02", updatedEntryNumberBO.CE_EntryNum);
				AssertEquals("CE_IssueDate - Not Update (IssueDate is Null), But Others are updated", new ZDateTime(2022, 10, 10, 10, 10, 10), updatedEntryNumberBO.CE_IssueDate);

				AssertMultilineASCIIEquals(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestSystemGeneratedCusEntryNumbersAreNotUpdatedFromIncomingNonSystemGeneratedData()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);
			entryNumberBO.CE_EntryIsSystemGenerated = true;

			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.EntryLineReference = "SOMETHING";

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			entryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(entryNumberBO);

			#region Check Contents of Business Object

			CombineAssertions("The system generated Entry Number should not have been changed", delegate
			{
				AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", true, entryNumberBO.CE_EntryIsSystemGenerated);
				AssertEquals("entryNumberBO.CE_EntryLineReference", "REFERENCE", entryNumberBO.CE_EntryLineReference);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			#endregion
		}

		public void TestUpdateNonSystemCusEntryNumberWithIncomingNonSystemCusEntryNumber()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);

			var entryNumberDataObject = SetupEntryNumberDataObject();
			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var updatedEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(updatedEntryNumberBO);

			CombineAssertions(delegate
			{
				AssertContentsWithReadOnly(updatedEntryNumberBO, shipmentBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestIncomingSystemGeneratedCusEntryNumbersIsAddedAsNonSystemIfThereIsNoMatchToExistingEntryNumber()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.EntryIsSystemGenerated = true;

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var entryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(entryNumberBO);

			CombineAssertions(delegate
			{
				AssertContents(entryNumberBO, shipmentBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...".Trim(), logger.Logs);
			});
		}

		public void TestWillNotMatchIfCountryCodeIsDifferent()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);

			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.CountryOfIssue = new Country { Code = "AU", Name = "Australia" };

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var newEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(newEntryNumberBO);

			CombineAssertions(delegate
			{
				AssertEquals("newEntryNumberBO.CE_RN_NKCountryCode", "AU", newEntryNumberBO.CE_RN_NKCountryCode);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestWillNotMatchIfEntryTypeIsDifferent()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberBO = EntryNumberDataObjectWriterTest.SetupCusEntryNumber(shipmentBO);

			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.Type = new EntryType { Code = "BKB", Description = "Booking Booked" };

			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var newEntryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(newEntryNumberBO);

			CombineAssertions(delegate
			{
				AssertEquals("newEntryNumberBO.CE_EntryType", "BKB", newEntryNumberBO.CE_EntryType);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestBasicCusEntryNumberLevelFieldMappings()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var entryNumberDataObject = SetupEntryNumberDataObject();
			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var entryNumberBO = reader.ReadIntoBusinessObject();

			AssertNotNull(entryNumberBO);

			CombineAssertions(delegate
			{
				AssertContents(entryNumberBO, shipmentBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		public void TestNotPopulateWhenNumberIsEmpty()
		{
			var shipmentBO = Factory.New<CommonShipment>();
			var logMessage = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Warning - CusEntryNumber was not populated because Entry Type is empty or Entry Number is empty with non-exempt Entry Type.
".Trim();

			var entryNumberDataObject = SetupEntryNumberDataObject();
			entryNumberDataObject.Number = ZString.Empty;
			entryNumberDataObject.Type = new EntryType { Code = ZString.Empty, Description = "Completion Entry No." };
			entryNumberDataObject.IssueDate = ZDateTime.Empty;
			entryNumberDataObject.ExpiryDate = ZDateTime.Empty;
			var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
			var newEntryNumberBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertBusinessObjectIsNotSaved(newEntryNumberBO);
			AssertMultilineASCIIEquals("logger.Logs", logMessage, logger.Logs);

			void AssertBusinessObjectIsNotSaved(BusinessObject businessObject)
			{
				Assert($"Precondition: {businessObject.GetType().Name} should be deleted", businessObject.IsDeleted);
				var businessObject2 = new BusinessObjectFactory().Load(businessObject.TablePrefix, businessObject.PK);
				AssertNull($"{businessObject.GetType().Name} should not be saved", businessObject2);
			}
		}

		public void TestPopulateWhenNumberIsEmptyAndExempt()
		{
			using(GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var shipmentBO = Factory.New<CommonShipment>();
				var entryNumberDataObject = SetupEntryNumberDataObject();
				entryNumberDataObject.Number = ZString.Empty;

				entryNumberDataObject.CountryOfIssue = new Country { Code = "AU", Name = "Australia" };
				entryNumberDataObject.Number = ZString.Empty;
				entryNumberDataObject.Type = new EntryType { Code = CMRExportExemptionCodes.EXDD.Code, Description = "Military goods. Owned by Australian Government" };
				var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
				var newEntryNumberBO = reader.ReadIntoBusinessObject();

				var logMessage = @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Warning - Attempted to insert 4 characters into Field [CE_EntryType] which has a maximum length of 3 characters. Field was truncated.
"
				.Trim();
				Factory.SaveForTesting();
				AssertBusinessObjectIsSaved(newEntryNumberBO);

				CombineAssertions(delegate
				{
					AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", false, newEntryNumberBO.CE_EntryIsSystemGenerated);
					AssertEquals("entryNumberBO.CE_Category", "CUS", newEntryNumberBO.CE_Category);
					AssertEquals("entryNumberBO.CE_EntryLineReference", "REFERENCE", newEntryNumberBO.CE_EntryLineReference);
					AssertEquals("entryNumberBO.CE_EntryNum", "", newEntryNumberBO.CE_EntryNum);
					AssertEquals("entryNumberBO.CE_EntryStatus", "FAL", newEntryNumberBO.CE_EntryStatus);
					AssertEquals("entryNumberBO.CE_EntryType", "EXD", newEntryNumberBO.CE_EntryType);
					AssertEquals("entryNumberBO.CE_ExpiryDate", new ZDateTime(2011, 2, 2),newEntryNumberBO.CE_ExpiryDate);
					AssertEquals("entryNumberBO.CE_IssueDate", new ZDateTime(2011, 3, 3), newEntryNumberBO.CE_IssueDate);
					AssertEquals("entryNumberBO.CE_RN_NKCountryCode", "AU", newEntryNumberBO.CE_RN_NKCountryCode);
					AssertEquals("entryNumberBO.CE_ParentTable", shipmentBO.TableName, newEntryNumberBO.CE_ParentTable);
					AssertEquals("entryNumberBO.CE_ParentID", shipmentBO.PK, newEntryNumberBO.CE_ParentID);
					AssertMultilineASCIIEquals("logger.Logs", logMessage, logger.Logs);
				});
			}

			void AssertBusinessObjectIsSaved(BusinessObject businessObject)
			{
				Assert($"Precondition: {businessObject.GetType().Name} should NOT be deleted", !businessObject.IsDeleted);
				var businessObject2 = new BusinessObjectFactory().Load(businessObject.TablePrefix, businessObject.PK);
				AssertNotNull($"{businessObject.GetType().Name} should be saved", businessObject2);
			}
		}

		public void TestPopulateWhenExistingNumberIsEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Belgium))
			{
				var shipmentBO = Factory.New<CommonShipment>();
				var cusEntryNum = shipmentBO.CusEntryNumbers.AddNew();
				cusEntryNum.CE_EntryType = "IMP";
				cusEntryNum.CE_EntryNum = ZString.Empty;
				cusEntryNum.CE_ParentID = shipmentBO.PK;
				cusEntryNum.CE_ParentTable = shipmentBO.TableName;
				cusEntryNum.CE_EntryIsSystemGenerated = false;

				var logMessage = @"Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...";
				Factory.SaveForTesting();

				var entryNumberDataObject = SetupEntryNumberDataObject();
				entryNumberDataObject.CountryOfIssue = new Country { Code = "BE", Name = "Belgium" };
				entryNumberDataObject.Number = "696969";
				entryNumberDataObject.Type = new EntryType { Code = "IMP", Description = "Import entry" };
				entryNumberDataObject.IssueDate = ZDateTime.Empty;
				entryNumberDataObject.ExpiryDate = ZDateTime.Empty;
				var reader = new EntryNumberDataObjectReader(entryNumberDataObject, logger, Factory, shipmentBO.CusEntryNumbersForAllCountries, shipmentBO);
				var newEntryNumberBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var newFactory = new BusinessObjectFactory();
				var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipmentBO.PK);
				AssertEquals("Precondition: Existing CusEntryNum should be updated", cusEntryNum.PK, newEntryNumberBO.PK);
				AssertEquals("696969", shipmentInNewFactory.CustomsEntryNumber);
				AssertEquals("IMP", shipmentInNewFactory.CustomsEntryNumberType);
				AssertEquals(1, shipmentInNewFactory.CusEntryNumbers.Count);
				AssertMultilineASCIIEquals("logger.Logs", logMessage, logger.Logs);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		public static EntryNumber SetupEntryNumberDataObject()
		{
			var entryNumberDataObject = new EntryNumber();

			entryNumberDataObject.CountryOfIssue = new Country { Code = "NZ", Name = "New Zealand" };
			entryNumberDataObject.EntryIsSystemGenerated = false;
			entryNumberDataObject.EntryLineReference = "REFERENCE";
			entryNumberDataObject.EntryStatus = new EntryStatus { Code = "FAL", Description = "Fail" };
			entryNumberDataObject.ExpiryDate = new ZDateTime(2011, 2, 2);
			entryNumberDataObject.IssueDate = new ZDateTime(2011, 3, 3);
			entryNumberDataObject.Number = "CE00001";
			entryNumberDataObject.Type = new EntryType { Code = "COM", Description = "Completion Entry No." };

			return entryNumberDataObject;
		}

		public static void AssertContents(CusEntryNumber entryNumberBO, BusinessObject entryNumberParent)
		{
			AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", false, entryNumberBO.CE_EntryIsSystemGenerated);
			AssertCommonContents(entryNumberBO, entryNumberParent);
		}

		public static void AssertContentsWithReadOnly(CusEntryNumber entryNumberBO, BusinessObject entryNumberParent)
		{
			AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", true, entryNumberBO.CE_EntryIsSystemGenerated);
			AssertCommonContents(entryNumberBO, entryNumberParent);
		}

		public static void AssertCommonContents(CusEntryNumber entryNumberBO, BusinessObject entryNumberParent)
		{
			AssertEquals("entryNumberBO.CE_Category", "CUS", entryNumberBO.CE_Category);
			AssertEquals("entryNumberBO.CE_EntryLineReference", "REFERENCE", entryNumberBO.CE_EntryLineReference);
			AssertEquals("entryNumberBO.CE_EntryNum", "CE00001", entryNumberBO.CE_EntryNum);
			AssertEquals("entryNumberBO.CE_EntryStatus", "FAL", entryNumberBO.CE_EntryStatus);
			AssertEquals("entryNumberBO.CE_EntryType", "COM", entryNumberBO.CE_EntryType);
			AssertEquals("entryNumberBO.CE_ExpiryDate", new ZDateTime(2011, 2, 2), entryNumberBO.CE_ExpiryDate);
			AssertEquals("entryNumberBO.CE_IssueDate", new ZDateTime(2011, 3, 3), entryNumberBO.CE_IssueDate);
			AssertEquals("entryNumberBO.CE_RN_NKCountryCode", "NZ", entryNumberBO.CE_RN_NKCountryCode);
			AssertEquals("entryNumberBO.CE_ParentTable", entryNumberParent.TableName, entryNumberBO.CE_ParentTable);
			AssertEquals("entryNumberBO.CE_ParentID", entryNumberParent.PK, entryNumberBO.CE_ParentID);
		}

		#endregion
	}
}
