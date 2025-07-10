using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	public sealed class AdditionalReferenceDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBizObjectProvider()
		{
			var dataObject = new AdditionalReference
			{
				Type = new EntryType { Code = "CON" },
				ReferenceNumber = "111",
				IssueDate = new ZDateTime(2012, 1, 1)
			};

			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_EntryType = "CON";
			cusEntryNumber1.CE_EntryNum = "111";
			cusEntryNumber1.CE_IssueDate = new ZDateTime(2012, 2, 1);

			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_EntryType = "XXX";
			cusEntryNumber2.CE_EntryNum = "222";
			cusEntryNumber1.CE_IssueDate = new ZDateTime(2012, 2, 2);

			var reader = new AdditionalReferenceDataObjectReader(dataObject, logger, Factory, dataObj => cusEntryNumber2);
			reader.ReadIntoBusinessObject();

			AssertEquals(new ZDateTime(2012, 1, 1), cusEntryNumber2.CE_IssueDate);
		}

		public void TestBasicAdditionalReferenceLevelFieldMappings()
		{
			var cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());
			var cusEntryNumberCollectionEnumerable = new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection);
			var additionalReferenceDataObject = SetupAdditionalReference();
			var reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, cusEntryNumberCollectionEnumerable);
			var additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertContents(additionalReferenceBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			#endregion

			var newLogger = new TestErrorLogger();
			cusEntryNumberCollection.Add(additionalReferenceBO);
			reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, newLogger, Factory, cusEntryNumberCollectionEnumerable);
			additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertContents(additionalReferenceBO);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), newLogger.Logs);
			});

			#endregion
		}

		public void TestSameReferenceTypeDoesNotCreateDuplicateAsTheCurrentUniqueIndexWillNotAllowThis()
		{
			var cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());
			var cusEntryNumberCollectionEnumerable = new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection);
			var additionalReferenceDataObject = SetupAdditionalReference();
			var reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, cusEntryNumberCollectionEnumerable);
			var additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertContents(additionalReferenceBO);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			#endregion

			var newLogger = new TestErrorLogger();
			cusEntryNumberCollection.Add(additionalReferenceBO);
			additionalReferenceDataObject.ReferenceNumber = "121212221";
			reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, newLogger, Factory, cusEntryNumberCollectionEnumerable);
			var additionalReferenceBOReferenceUpdated = reader.ReadIntoBusinessObject();

			AssertEquals("additionalReferenceBOReferenceUpdated", additionalReferenceBO, additionalReferenceBOReferenceUpdated);

			#region Check Contents of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("additionalReferenceBOReferenceUpdated.CE_EntryNum", "121212221", additionalReferenceBOReferenceUpdated.CE_EntryNum);
				AssertMultilineASCIIEquals("newLogger.Logs", @"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), newLogger.Logs);
			});

			#endregion
		}

		public void TestPopulateBusinessObject_SetIssueCountryAsGermanForSZB()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				void AssertPopulateAdditionalReference(string type, string expectedCountryCode)
				{
					var dataObject = new AdditionalReference
					{
						Type = new EntryType { Code = type },
						ReferenceNumber = "0001",
						IssueDate = new ZDateTime(2012, 1, 1)
					};

					var reader = new AdditionalReferenceDataObjectReader(dataObject, logger, Factory, dataObj => null);
					var cusEntryNumber = reader.ReadIntoBusinessObject();

					CombineAssertions(() =>
					{
						AssertEquals("CE_IssueDate", new ZDateTime(2012, 1, 1), cusEntryNumber.CE_IssueDate);
						AssertEquals("CE_EntryNum", "0001", cusEntryNumber.CE_EntryNum);
						AssertEquals("CE_RN_NKCountryCode", expectedCountryCode, cusEntryNumber.CE_RN_NKCountryCode);
					});
				}

				AssertPopulateAdditionalReference(GermanyAdditionalReferenceNumberTypes.Codes.SZBNumber, Core.Constants.CountryCodes.Germany);
				AssertPopulateAdditionalReference(CusEntryNumberTypes.HongKong.ImportLicense, Core.Constants.CountryCodes.UnitedKingdom);
			}
		}

		public void TestSZBCusEntryNumbersAreNotUpdatedWhenDataObjectIsOutdated()
		{
			var cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());

			var additionalReferenceDataObject = SetupAdditionalReference();
			additionalReferenceDataObject.ReferenceNumber = "CE00001";
			additionalReferenceDataObject.IssueDate = new ZDateTime(2023, 02, 28);
			additionalReferenceDataObject.Type = new EntryType { Code = "SZB", Description = "SZB Number" };

			logger.ClearLogs();
			var reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection));
			var additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "OTH", additionalReferenceBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", additionalReferenceBO.CE_EntryType);
				AssertEquals("CE_EntryNum", "CE00001", additionalReferenceBO.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2023, 02, 28), additionalReferenceBO.CE_IssueDate);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Germany, additionalReferenceBO.CE_RN_NKCountryCode);

				AssertMultilineASCIIEquals(@"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());

			additionalReferenceDataObject = SetupAdditionalReference();
			additionalReferenceDataObject.ReferenceNumber = "WithoutIssueDate01";
			additionalReferenceDataObject.IssueDate = null;
			additionalReferenceDataObject.Type = new EntryType { Code = "SZB", Description = "SZB Number" };

			logger.ClearLogs();
			reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection));
			additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "OTH", additionalReferenceBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", additionalReferenceBO.CE_EntryType);
				AssertEquals("CE_EntryNum", "WithoutIssueDate01", additionalReferenceBO.CE_EntryNum);
				AssertEquals("CE_IssueDate", ZDateTime.Empty, additionalReferenceBO.CE_IssueDate);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Germany, additionalReferenceBO.CE_RN_NKCountryCode);

				AssertMultilineASCIIEquals(@"
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());

			var cusEntryNumber = cusEntryNumberCollection.AddNew();
			cusEntryNumber.CE_EntryNum = "CE00001";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01);
			cusEntryNumber.CE_EntryType = "SZB";

			additionalReferenceDataObject = SetupAdditionalReference();
			additionalReferenceDataObject.ReferenceNumber = "UPDATE01";
			additionalReferenceDataObject.IssueDate = new ZDateTime(2023, 02, 28);
			additionalReferenceDataObject.Type = new EntryType { Code = "SZB", Description = "SZB Number" };

			logger.ClearLogs();
			reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection));
			additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "OTH", additionalReferenceBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", additionalReferenceBO.CE_EntryType);
				AssertEquals("CE_EntryNum - Update", "UPDATE01", additionalReferenceBO.CE_EntryNum);
				AssertEquals("CE_IssueDate - Update", new ZDateTime(2023, 02, 28), additionalReferenceBO.CE_IssueDate);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Germany, additionalReferenceBO.CE_RN_NKCountryCode);

				AssertMultilineASCIIEquals(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());

			cusEntryNumber = cusEntryNumberCollection.AddNew();
			cusEntryNumber.CE_EntryNum = "CE00002";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01);
			cusEntryNumber.CE_EntryType = "SZB";

			additionalReferenceDataObject = SetupAdditionalReference();
			additionalReferenceDataObject.ReferenceNumber = "UPDATE02";
			additionalReferenceDataObject.IssueDate = new ZDateTime(2022, 12, 30);
			additionalReferenceDataObject.Type = new EntryType { Code = "SZB", Description = "SZB Number" };

			logger.ClearLogs();
			reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection));
			additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "OTH", additionalReferenceBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", additionalReferenceBO.CE_EntryType);
				AssertEquals("CE_EntryNum - Not Update (DateObject is Outdated)", "CE00002", additionalReferenceBO.CE_EntryNum);
				AssertEquals("CE_IssueDate - Not Update (DateObject is Outdated)", new ZDateTime(2023, 01, 01), additionalReferenceBO.CE_IssueDate);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Germany, additionalReferenceBO.CE_RN_NKCountryCode);

				AssertMultilineASCIIEquals(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});

			cusEntryNumberCollection = new CusEntryNumAdditionalReferenceCollection(Factory.New<CommonShipment>());

			cusEntryNumber = cusEntryNumberCollection.AddNew();
			cusEntryNumber.CE_EntryNum = "CE00002";
			cusEntryNumber.CE_IssueDate = new ZDateTime(2023, 01, 01);
			cusEntryNumber.CE_EntryType = "SZB";

			additionalReferenceDataObject = SetupAdditionalReference();
			additionalReferenceDataObject.ReferenceNumber = "WithoutIssueDate02";
			additionalReferenceDataObject.IssueDate = null;
			additionalReferenceDataObject.Type = new EntryType { Code = "SZB", Description = "SZB Number" };

			logger.ClearLogs();
			reader = new AdditionalReferenceDataObjectReader(additionalReferenceDataObject, logger, Factory, new TypedEnumerable<CusEntryNumber>(cusEntryNumberCollection));
			additionalReferenceBO = reader.ReadIntoBusinessObject();

			AssertNotNull(additionalReferenceBO);
			CombineAssertions(() =>
			{
				AssertEquals("CE_Category", "OTH", additionalReferenceBO.CE_Category);
				AssertEquals("CE_EntryType", "SZB", additionalReferenceBO.CE_EntryType);
				AssertEquals("CE_EntryNum - Update (Without IssueDate)", "WithoutIssueDate02", additionalReferenceBO.CE_EntryNum);
				AssertEquals("CE_IssueDate - Not Update (IssueDate is Null), But Others are updated", new ZDateTime(2023, 01, 01), additionalReferenceBO.CE_IssueDate);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Germany, additionalReferenceBO.CE_RN_NKCountryCode);

				AssertMultilineASCIIEquals(@"
Information - Successfully loaded matching CusEntryNumber.
Information - Populating CusEntryNumber...
".Trim(), logger.Logs);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;

		public static AdditionalReference SetupAdditionalReference()
		{
			var additionalReferenceDataObject = new AdditionalReference();

			additionalReferenceDataObject.ContextInformation = "INFORMER";
			additionalReferenceDataObject.IssueDate = new ZDateTime(2011, 3, 3);
			additionalReferenceDataObject.ReferenceNumber = "CE00001";
			additionalReferenceDataObject.Type = new EntryType { Code = "AMS", Description = "AMS Number" };

			return additionalReferenceDataObject;
		}

		public static void AssertContents(CusEntryNumber additionalReferenceBO)
		{
			AssertEquals("additionalReferenceBO.CE_Category", "OTH", additionalReferenceBO.CE_Category);
			AssertEquals("additionalReferenceBO.CE_EntryLineReference", "INFORMER", additionalReferenceBO.CE_EntryLineReference);
			AssertEquals("additionalReferenceBO.CE_EntryNum", "CE00001", additionalReferenceBO.CE_EntryNum);
			AssertEquals("additionalReferenceBO.CE_EntryType", "AMS", additionalReferenceBO.CE_EntryType);
			AssertEquals("additionalReferenceBO.CE_IssueDate", new ZDateTime(2011, 3, 3), additionalReferenceBO.CE_IssueDate);
		}

		#endregion
	}
}
