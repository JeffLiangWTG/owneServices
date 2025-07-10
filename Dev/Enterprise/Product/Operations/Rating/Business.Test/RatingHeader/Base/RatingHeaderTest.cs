using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingHeaderTest : RatingTestCase
	{
		public void TestDuplicateEntries()
		{
			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AE", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);

			rateLine.OverrideChargeDescription = true;
			rateLine.TL_RateDesc = "Rate Description Override";
			rateLine.TL_RateDescLocal = "Rate Description Local Override";

			clientRate.DuplicateEntries(new[] { rateEntry });

			var newRateEntry = clientRate.AllEntries.Single(entry => entry.PK != rateEntry.PK);
			var newRateLine = (RateLine)newRateEntry.RateLines.Single();

			CombineAssertions
			(
				"WHEN duplicate THEN OverrideChargeDescription, RateDesc and RateDescLocal should be copied",
				() =>
				{
					AssertEquals("OverrideChargeDescription", true, newRateLine.OverrideChargeDescription);
					AssertEquals("RateDesc", "Rate Description Override", newRateLine.TL_RateDesc);
					AssertEquals("RateDescLocal", "Rate Description Local Override", newRateLine.TL_RateDescLocal);
				}
			);
		}

		public void TestProgressReporterValidation()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());

			var globalAIRRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AE", "");
			var globalLCLRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "BE", "");
			var globalSORRateEntry = costing.AddRateEntry(RatingConstants.RateCategory.SOR, Constants.RateMode.ALL, "CI", "");

			var progressReporterValidation = (IProgressReporterValidation)costing;
			var logs = new StringBuilder();
			using (progressReporterValidation.ReportProgress((message) => logs.Append(message)))
			{
				costing.RunPreSaveValidation();
			}

			var logsAsString = logs.ToString();

			CombineAssertions("Should show progress on 3 records i.e. AIR, LCL, SOR", () =>
			{
				AssertContains("0 of 3 records", logsAsString);
				AssertContains("1 of 3 records", logsAsString);
				AssertContains("2 of 3 records", logsAsString);
			});
		}

		#region TestTL_TIIsCorrectAfterAcceptQuote

		public void TestTL_TIIsCorrectAfterAcceptQuote()
		{
			var orgHeader = Helper.NewOrgHeader();
			var quote = Factory.New<Quote>();
			quote.TH_OH = orgHeader.PK;
			var quoteEntry = quote.AddRateEntry("FCL");
			quoteEntry.RateLines.RemoveAndDeleteAll();
			var quoteRateLine = quoteEntry.AddRateLine("FRT", FlatCalculator.Code);

			Factory.Save();

			ClientRate clientRate;
			quote.TryAcceptQuote(out clientRate);
			var clientRateEntry = clientRate.AllEntriesCollection[0] as RateEntry;

			AssertEquals(1, clientRateEntry.RateLines.Count);
			AssertNotEquals(quoteRateLine.TL_TI, clientRateEntry.RateLines[0].TL_TI);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			var ratingHeader = Factory.New<RatingHeader>();
			ratingHeader.TH_QuoteNumber = "TH12002";
			AssertEquals("JobNumber should be TH12002", "TH12002", ratingHeader.JobNumber);
		}

		#endregion

		#region End Date

		[TestDate(2005, 10, 15)]
		public void TestQuoteEndDate()
		{
			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 1);
			var testQuote = Factory.New<Quote>();
			AssertEquals(new ZDateTime(2005, 11, 15), testQuote.TH_QuoteEndDate);

			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, -1);
			testQuote = Factory.New<Quote>();
			AssertEquals(new ZDateTime(2005, 11, 30), testQuote.TH_QuoteEndDate);

			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, 0);
			testQuote = Factory.New<Quote>();
			AssertEquals(new ZDateTime(2005, 10, 31), testQuote.TH_QuoteEndDate);

			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, int.MaxValue - 10);
			testQuote = Factory.New<Quote>();
			Assert(testQuote.TH_QuoteEndDate.IsEmpty);

			Env.Registry.Rating.QuoteValidityPeriod = new QuoteValidityRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, -(int.MaxValue - 10));
			testQuote = Factory.New<Quote>();
			Assert(testQuote.TH_QuoteEndDate.IsEmpty);
		}

		#endregion

		#region CFX

		public void TestCFXReadOnlyOnNewRate()
		{
			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = NewClient.PK;

			AssertEquals("Import CFX AIR Read Only", true, testRate.TH_AirCFXInfo.ReadOnly);
			AssertEquals("Import CFX SEA Read Only", true, testRate.TH_SeaCFXInfo.ReadOnly);
			AssertEquals("Export CFX AIR Read Only", true, testRate.TH_ExportAirCFXInfo.ReadOnly);
			AssertEquals("Export CFX SEA Read Only", true, testRate.TH_ExportSeaCFXInfo.ReadOnly);
		}

		public void TestCFXReadOnlyOnExistingRate()
		{
			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = NewClient.PK;
			Factory.Save();

			var savedRate = Factory.Load<ClientRate>(testRate.PK);

			AssertEquals("Import CFX AIR Read Only", true, savedRate.TH_AirCFXInfo.ReadOnly);
			AssertEquals("Import CFX SEA Read Only", true, savedRate.TH_SeaCFXInfo.ReadOnly);
			AssertEquals("Export CFX AIR Read Only", true, savedRate.TH_ExportAirCFXInfo.ReadOnly);
			AssertEquals("Export CFX SEA Read Only", true, savedRate.TH_ExportSeaCFXInfo.ReadOnly);
		}

		public void TestCFXAEditableOnQuote()
		{
			var testQuote = Factory.New<Quote>();
			testQuote.TH_OH = NewClient.PK;

			AssertEquals("Import CFX AIR Read Only", false, testQuote.TH_AirCFXInfo.ReadOnly);
			AssertEquals("Import CFX SEA Read Only", false, testQuote.TH_SeaCFXInfo.ReadOnly);
			AssertEquals("Export CFX AIR Read Only", false, testQuote.TH_ExportAirCFXInfo.ReadOnly);
			AssertEquals("Export CFX SEA Read Only", false, testQuote.TH_ExportSeaCFXInfo.ReadOnly);
		}

		#endregion

		#region Document Functionality

		public void TestBuildPrintTask()
		{
			Env.Registry.Rating.SetQuoteTermsAndConditionsPages(new Image[] { SystemIcons.Exclamation.ToBitmap(), null, null, null, null, null, null, null, null, null });

			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			SelectPages(quote,
				"Cover Page",
				"Forwarding Standard Pricing Page",
				"Acceptance Page",
				"Contact Details",
				"Published Agents",
				"Trailing Page 1"
			);

			quote.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 100m);

			string docManagerCode = ((IDocManagerSupport)quote).DocManagerInfo.DocManagerCode;

			var query = new ZQuery(RefDocTypeSchema.RT_DocType, "QUO");
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Constants.ReferenceTypes.ClientSupplierRelationship);

			var docTypeForQuote = Factory.LoadTop1<RefDocType>(query);

			SetupStmEDocs(quote, docManagerCode);

			var command = Factory.LoadTop1<DocumentCommand>(new DocumentZQuery(BusinessContext.Quotation, "Quotation Pack"));
			command.Parent = quote;

			Factory.Save();

			const string expectedCommand1 = @"
[Documents]
Quotation Pack|Forwarding Standard Pricing Page
[eDocs]
";

			const string expectedCommand2 = @"
[Documents]
Quotation Pack|Forwarding Standard Pricing Page
[eDocs]
Quotation
";

			const string expectedTask1 = @"
Cover Page
Forwarding Standard Pricing Page
Acceptance Page
Contacts Page
Recommended Agents Page
Terms And Conditions
";

			const string expectedTask2 = @"
Cover Page
Forwarding Standard Pricing Page
Acceptance Page
Contacts Page
Recommended Agents Page
Terms And Conditions
A Quote Test eDoc
";

			AssertMultilineASCIIEquals("precondition:", expectedCommand1, RenderCommand(command));

			var supporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			var task = supporter.BuildPrintTask(command);

			AssertMultilineASCIIEquals("Standard 6 documents exist NO eDoc", expectedTask1, RenderDocumentNames(task));

			// Now add the eDoc
			StmMenuEDocs eDoc = command.AddEDoc(docTypeForQuote);
			Factory.Save();

			AssertMultilineASCIIEquals("precondition:", expectedCommand2, RenderCommand(command));

			task = supporter.BuildPrintTask(command);
			AssertMultilineASCIIEquals("Standard 6 documents exist PLUS eDoc", expectedTask2, RenderDocumentNames(task));

			// As if the print button was clicked
			quote = new BusinessObjectFactory().Load<Quote>(quote.PK);
			supporter = (RatingHeaderDocumentSupporter)((IDocumentSupportable)quote).DocumentSupporter;
			task = supporter.BuildPrintTask(null);
			AssertMultilineASCIIEquals("Standard 6 documents exist PLUS eDoc", expectedTask2, RenderDocumentNames(task));
		}

		public void TestDocManagerCode()
		{
			IDocManagerSupport header = Helper.NewClientRate(null);
			AssertEquals("CRT", header.DocManagerInfo.DocManagerCode);

			header = Helper.NewGlobalClientRate(null);
			AssertEquals("GCL", header.DocManagerInfo.DocManagerCode);

			header = Helper.NewCompanyTariff();
			AssertEquals("CTF", header.DocManagerInfo.DocManagerCode);

			header = Helper.NewGlobalTariff();
			AssertEquals("GCT", header.DocManagerInfo.DocManagerCode);

			header = Helper.NewQuote(null);
			AssertEquals("QUO", header.DocManagerInfo.DocManagerCode);

			header = Helper.NewCosting(null);
			AssertEquals("CST", header.DocManagerInfo.DocManagerCode);

			header = Helper.NewGlobalCosting(null);
			AssertEquals("GCO", header.DocManagerInfo.DocManagerCode);
		}

		void SetupStmEDocs(RatingHeader header, ZString docManagerCode)
		{
			var sMFields = ZString.Join(", ", new ZString[] { StorageMainSchema.Constants.PK, StorageMainSchema.Constants.SM_ParentFK, StorageMainSchema.Constants.SM_Type, StorageMainSchema.Constants.SM_DB });
			var sDFields = ZString.Join(", ", new ZString[] { StorageDocsSchema.Constants.PK, StorageDocsSchema.Constants.SC_SM, StorageDocsSchema.Constants.SC_DocType, StorageDocsSchema.Constants.SC_Desc, StorageDocsSchema.Constants.SC_ImageData, StorageDocsSchema.Constants.SC_Date, StorageDocsSchema.Constants.SC_SystemCreateTimeUtc, StorageDocsSchema.Constants.SC_SystemLastEditTimeUtc });

			var sMGuid = ZGuid.NewZGuid().ToString();

			var sqlText = "INSERT INTO " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " (" + sMFields + ") "
				+ " VALUES ('" + sMGuid + "', '" + header.PK + "', '" + docManagerCode + "', 1)";
			Db.Connection.ExecuteNonQuery(sqlText);       // Need to create StorageMain records for the purpose of testing Quote eDocs

			sqlText = string.Format("INSERT INTO {0}_SD001..", Db.DatabaseName.Trim()) + StorageDocsSchema.Constants.TableName + " (" + sDFields + ") "
				+ " VALUES (NEWID(), '" + sMGuid + "', 'QUO', 'A Quote Test eDoc', convert(varbinary(1000), N'1234567890123456890'), getdate(), getdate(), getdate())";
			Db.Connection.ExecuteNonQuery(sqlText);       // Need to create StorageDocs records for the purpose of testing Quote eDocs
		}

		public void TestBusinessContext()
		{
			var resultSet = new Dictionary<Type, BusinessContext>();

			resultSet.Add(typeof(ClientRate), BusinessContext.Rating);
			resultSet.Add(typeof(CompanyTariff), BusinessContext.Rating);
			resultSet.Add(typeof(GlobalTariff), BusinessContext.Rating);
			resultSet.Add(typeof(Costing), BusinessContext.Rating);
			resultSet.Add(typeof(IntercompanyTariff), BusinessContext.Rating);
			resultSet.Add(typeof(Quote), BusinessContext.Quotation);

			var assembly = typeof(RatingHeader).Assembly;
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes.Where(RatingHeaderFilter))
			{
				var businessContexts = type.GetCustomAttributes(typeof(BusinessContextAttribute), false);
				Assert(type.Name + " should have BusinessContext Attribute", businessContexts.Length == 1);
				var header = (RatingHeader)Factory.New(type);
				var documentSupporter = ((IDocumentSupportable)header).DocumentSupporter;
				AssertEquals(type.Name + " should have correct BusinessContext", resultSet[type], documentSupporter.BusinessContext);
			}
		}

		public void TestDataContexts()
		{
			var expected = new DataContext[] { DataContext.GenericFreightJob, DataContext.Quotation, DataContext.Rating, DataContext.ShippingRating, DataContext.ShippingDetentionRating, DataContext.CFSRating, DataContext.WarehouseRating, DataContext.TransportRating };
			AssertContainsExactElementsInAnyOrder("UniqueDataContexts", expected, RatingHeaderDocumentSupporter.UniqueDataContexts);

			RatingHeader header = Helper.NewCompanyTariff();
			var supporter = ((IDocumentSupportable)header).DocumentSupporter;

			var supportedDataContextsIncludingRetardedEntries = supporter.CommaSeparatedListOfSupportedDataContexts.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
			var supportedDataContexts = Array.FindAll(supportedDataContextsIncludingRetardedEntries, (s) => !s.StartsWith("."));

			AssertContainsExactElementsInAnyOrder("Supported DataContexts",
				Array.ConvertAll(expected, (c) => c.ToString()),
				supportedDataContexts);
		}

		#endregion

		#region Lazy Collections

		public void TestIfEachEntryCategoryIsInLazyCollectionArray()
		{
			var rate = Helper.NewClientRate(NewClient);
			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				AssertCollectionContains(ZString.Format("{0} is missing", category), category, rate.EntryCollections.Keys);
				AssertEquals(category + "RateEntryCollection", rate.EntryCollections[category].LazyLoadingCollection.GetType().Name);
			}
		}

		#endregion

		#region Delete

		[SnailTest, StressTest]
		public void TestDelete_DbHits()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			for (int i = 0; i < 10; i++)
			{
				var rateEntry = costing.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 1m, QuantityUnit.KG, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));
				rateEntry.TI_ContractNumber = $"CONTRACT-{i}";
				rateEntry.AddCMBRateLine("CAF", QuantityUnit.KG, ("-50", 5m), ("+50", 10m), ("+150", 15m));
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedCosting = newFactory.Load<Costing>(costing.PK);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ RatingHeaderSchema.Constants.TableName, 1 },
				{ RateEntrySchema.Constants.TableName, 1 },
			};
			newFactory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				loadedCosting.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.LoadAndSortForGUI();
			}

			expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocumentDeliverySchema.Constants.TableName, 1 },
				{ JobDocumentExclusionSchema.Constants.TableName, 1 },
				{ StmDocDataOverrideSchema.Constants.TableName, 2 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ StmUniversalCopySchema.Constants.TableName, 1 },
				{ ViewRelatedActivityPivotSchema.Constants.TableName, 1 },
			};
			newFactory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				loadedCosting.Delete();
				newFactory.Save();
			}
		}

		public void TestDelete()
		{
			var rate = Helper.NewClientRate(NewClient);

			var entry1 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry1.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			entry1.TI_RateEndDate = ZDate.Today.AddMonths(3);
			var entry2 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USSFO");
			entry2.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			entry2.TI_RateEndDate = ZDate.Today;
			Factory.Save();

			AssertEquals(2, rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);

			rate.Delete();
			Factory.Save();
			Assert(rate.IsDeleted);
		}

		public void TestDeleteRateEntriesInBatches()
		{
			var clientEntry = Helper.NewClientRate(NewClient).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			var costEntry = Helper.NewCosting(TransportProvider1).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			var companyTariffEntry = Helper.NewCompanyTariff().AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			companyTariffEntry.Factory.Save();
			Factory.Save();

			var tariff = Helper.NewGlobalTariff();

			for (int i = 0; i < 4; i++)
			{
				var entry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
				entry.TI_RateStartDate = ZDate.Today.AddDays(i);
				entry.TI_RateEndDate = ZDate.Today.AddDays(i);
			}

			tariff.Discounts.RemoveAndDeleteAll();
			tariff.WorkflowItems.RemoveAndDeleteAll();
			tariff.Factory.Save();

			CombineAssertions("Pre-conditions: Entries should be saved in DB.", () =>
			{
				AssertEntries(clientEntry.TI_TH, new[] { clientEntry });
				AssertEntries(costEntry.TI_TH, new[] { costEntry });
				AssertEntries(companyTariffEntry.TI_TH, new[] { companyTariffEntry });
				AssertEntries(tariff.PK, tariff.AllEntries.ToArray());
			});

			var pk = tariff.PK;
			var newFactory = new BusinessObjectFactory();
			var tariffLoadedInNewFactory = (CompanyTariff)newFactory.Load<RatingHeader>(pk);
			tariffLoadedInNewFactory.Discounts.RemoveAndDeleteAll();

			var testConnection = ((IDbConnected)newFactory).Connection;
			using (testConnection.TrackExecutedCommands())
			{
				tariffLoadedInNewFactory.NumOfRateEntriesToDeletePerBatch = 3;

				tariffLoadedInNewFactory.Delete();
				AssertContainsExactElementsInAnyOrder(new string[] {
					$@"DELETE TOP(3)
FROM dbo.RateEntry
WHERE TI_TH = @PK
Params
@PK: '{pk.ToGuid()}'",
					$@"DELETE TOP(3)
FROM dbo.RateEntry
WHERE TI_TH = @PK
Params
@PK: '{pk.ToGuid()}'"
				}, testConnection.ExecutedCommands.Select(cmdText => cmdText.Trim()).Where(cmdText => cmdText.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)));
			}

			CombineAssertions("After tariff delete, only related tariff entries should be deleted from DB, not other rate entries.", () =>
			{
				AssertEntries(clientEntry.TI_TH, new[] { clientEntry });
				AssertEntries(costEntry.TI_TH, new[] { costEntry });
				AssertEntries(companyTariffEntry.TI_TH, new[] { companyTariffEntry });
				AssertEntries(tariff.PK, Array.Empty<RateEntry>());
			});

			void AssertEntries(ZGuid ratingHeaderPK, RateEntry[] expectedRateEntriesInDB)
			{
				Factory.ClearQueryCache();

				var zQuery = new ZDBOnlyQuery(typeof(RateEntry));
				zQuery.AddToFilter(RateEntrySchema.TI_TH, ratingHeaderPK);
				var result = Factory.Load<RateEntry>(zQuery);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<RateEntry>.PKOnlyComparer, expectedRateEntriesInDB, result);
			}
		}

		public void TestRelatedEntitiesAreDeletedWithoutLoadingWhenDeletingHeader()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var ports = new[] { "AUSYD", "NZAKL", "ZAJNB" };
			var entryPks = new List<ZGuid>();
			var linePKs = new List<ZGuid>();

			foreach (var port in ports)
			{
				var airEntry = rate.AddRateEntry("AIR", "LSE", "USLAX", port);
				airEntry.RateLines.RemoveAndDeleteAll();
				var airLine = airEntry.AddRateLine("FRT", FlatCalculator.Code);
				airLine.GetCalculator<FlatCalculator>().BaseRate = 100;
				airLine.TL_Condition = RateLineConditions.UserDefined;
				airLine.TL_ConditionalExpression = "MOD=FSA";

				var dstEntry = rate.AddRateEntry("DST", "ALL", "USLAX", port);
				dstEntry.RateLines.RemoveAndDeleteAll();
				var dstLine = dstEntry.AddRateLine("FRT", FlatCalculator.Code);
				dstLine.GetCalculator<FlatCalculator>().BaseRate = 200;
				dstLine.TL_Condition = RateLineConditions.UserDefined;
				dstLine.TL_ConditionalExpression = "MOD=FSA";

				entryPks.AddRange(new[] { airEntry.PK, dstEntry.PK });
				linePKs.AddRange(new[] { airLine.PK, dstLine.PK });
			}

			Factory.Save();

			var lineItemPKs = Factory.Load<RateLineItem>(new ZQuery(RateLineItemsSchema.TM_TL, linePKs))
				.Select(x => x.PK)
				.ToList();

			var stmNotePKs = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, linePKs).AddToFilter(StmNoteSchema.ST_Table, "RateLines"))
				.Select(x => x.PK)
				.ToList();

			Assert(lineItemPKs.Count > 0);

			var newFactory = new BusinessObjectFactory();

			var rateInNewFactory = newFactory.Load<ClientRate>(rate.PK);

			rateInNewFactory.Delete();
			newFactory.Save();

			AssertNoRelatedEntityIsLoaded(newFactory);
			AssertRelatedEntitiesAreDeleted(newFactory, entryPks, linePKs, lineItemPKs, stmNotePKs);
		}

		public void TestDeleteDuplicatedRateEntryCanSaveAfterwards()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry1 = rate.AddRateEntry("AIR", "LSE", "AU", "US");
			Factory.Save();
			AssertEquals(false, rateEntry1.HasRowErrors);

			var rateEntry2 = rate.AddRateEntry("AIR", "LSE", "AU", "US");
			rate.RunPreSaveValidation();
			AssertEquals(true, rateEntry1.HasRowErrors);
			AssertEquals(true, rateEntry2.HasRowErrors);

			rateEntry2.Delete();
			rate.RunPreSaveValidation();
			Factory.Save();
			AssertEquals(false, rateEntry1.HasRowErrors);
			AssertEquals(1, rate.AllEntries.Count());
		}

		public void TestCanDelete_GlobalClientRate()
		{
			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var securitySettingForDelete = Env.Security.GlobalClientRatesDeleteFromAnyCompany;

			AssertCanDeleteGlobalRate(globalClientRate, securitySettingForDelete);
		}

		public void TestCanDelete_GlobalCosting()
		{
			var serviceProvider = Helper.NewOrgHeader();
			var globalCosting = Helper.NewGlobalCosting(serviceProvider);
			var securitySettingForDelete = Env.Security.GlobalCostingRatesDeleteFromAnyCompany;

			AssertCanDeleteGlobalRate(globalCosting, securitySettingForDelete);
		}

		public void TestCanDelete_GlobalTariff()
		{
			var globalTariff = Factory.New<GlobalTariff>();
			var securitySettingForDelete = Env.Security.GlobalTariffRatesDeleteFromAnyCompany;

			AssertCanDeleteGlobalRate(globalTariff, securitySettingForDelete);
		}

		void AssertCanDeleteGlobalRate(RatingHeader ratingHeader, SecurityCheckpoint securitySettingForDelete)
		{
			var isAllowed = securitySettingForDelete.IsAllowed;

			try
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.GC_Code = "NC1";
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				company1.Branches.Add(branch1);

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.GC_Code = "NC2";
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				company2.Branches.Add(branch2);

				AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out _, out _, true, false, "FRT", "FRT");

				var entry1 = ratingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
				Factory.Save();

				securitySettingForDelete.IsAllowed = false;
				Assert("Pre-condition", !securitySettingForDelete.IsAllowed);

				var description = ratingHeader.HumanReadableName;
				Assert("Pre-condition", ratingHeader.IsGlobal());

				RateEntry entry2;
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					entry2 = ratingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "GB", "");
					Factory.Save();

					Assert(!ratingHeader.CanDelete);
					AssertEquals($"This {description} can't be deleted as it contains Rate Entries Published by other Company(s) (EDI).", ratingHeader.ReasonForNotAbleToDelete);
				}

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					Factory.ClearCachedValue<ZString[]>($"OtherPublisherCompanyCodes.{ratingHeader.PK}");
					Assert(!ratingHeader.CanDelete);
					AssertEquals($"This {description} can't be deleted as it contains Rate Entries Published by other Company(s) (EDI, NC1).", ratingHeader.ReasonForNotAbleToDelete);
				}

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					entry2.Delete();
					Factory.Save();
				}

				Factory.ClearCachedValue<ZString[]>($"OtherPublisherCompanyCodes.{ratingHeader.PK}");
				Assert("Should be able to delete from EDI company as the NC1 rate entry has been deleted", ratingHeader.CanDelete);
				Assert(ratingHeader.ReasonForNotAbleToDelete.IsEmpty);

				entry1.Delete();
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					Factory.ClearCachedValue<ZString[]>($"OtherPublisherCompanyCodes.{ratingHeader.PK}");
					Assert("Should be able to delete from NC2 company as there are no rate entries left", ratingHeader.CanDelete);
					Assert(ratingHeader.ReasonForNotAbleToDelete.IsEmpty);
				}

				var entry3 = ratingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					securitySettingForDelete.IsAllowed = true;
					Assert("Pre-condition", securitySettingForDelete.IsAllowed);

					var newFactory = new BusinessObjectFactory();
					var reloadedRatingHeader = newFactory.Load<RatingHeader>(ratingHeader.PK);

					Assert("Security setting allows deleting despite the published rate in another company", reloadedRatingHeader.CanDelete);
					Assert(reloadedRatingHeader.ReasonForNotAbleToDelete.IsEmpty);
				}
			}
			finally
			{
				securitySettingForDelete.IsAllowed = isAllowed;
			}
		}

		public void TestCanDelete_LocalRatingHeaders()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var companyTariff = Factory.New<CompanyTariff>();
			Factory.Save();

			var localRatingHeaders = new RatingHeader[] { clientRate, companyTariff, costing };
			foreach (var ratingHeader in localRatingHeaders)
			{
				Factory.ClearCachedValue<ZString[]>($"OtherPublisherCompanyCodes.{ratingHeader.PK}");

				AssertEquals(ratingHeader.HumanReadableName, false, ratingHeader.IsGlobal());
				AssertEquals(ratingHeader.HumanReadableName, true, ratingHeader.CanDelete);
				AssertEquals(ratingHeader.HumanReadableName, true, ratingHeader.ReasonForNotAbleToDelete.IsEmpty);
			}
		}

		#endregion

		#region Properties

		#region IsAutoLogged

		public void TestIsAutoLogged()
		{
			var testHeader = Factory.New<RatingHeader>();
			Assert("IsAutoLogged", testHeader.IsAutoLogged);
		}

		#endregion IsAutoLogged

		#region ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges

		public void TestShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges()
		{
			var testHeader = Factory.New<RatingHeader>();
			testHeader.CreateAutoLogIfOnlyChildrenChanged = true;
			Assert("Should update audit fields if only children have changes", ((IUpdateAuditFields)testHeader).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);

			testHeader.CreateAutoLogIfOnlyChildrenChanged = false;
			Assert("Should not update audit fields if only children have changes", !((IUpdateAuditFields)testHeader).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
		}

		#endregion

		#region TH_ClientCode

		public void TestTH_ClientCode()
		{
			var rate = Helper.NewClientRate(NewClient);
			AssertEquals("Rate with Organisation should return the Client's Organisation Code", NewClient.OH_Code, rate.TH_ClientCode);

			rate.TH_OH = ZGuid.Empty;
			AssertEquals("Rate without organisation should return empty string", ZString.Empty, rate.TH_ClientCode);
		}

		#endregion

		public void TestInvalidateZonesWhenChangingTheClient()
		{
			var client1 = Helper.NewOrgHeader();
			var client2 = Helper.NewOrgHeader();
			var provider1 = Helper.CreateRateTransportZoneSet(client1, CountryCodes.Australia, zoneNames: new ZString[] { "Z1", "Z2", "Z3" });
			var provider2 = Helper.CreateRateTransportZoneSet(client2, CountryCodes.Australia, zoneNames: new ZString[] { "G1", "G2" });

			var rate = Helper.NewClientRate(client1);
			var entry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			var line = entry.AddRateLine("OCART", "CTZ", QuantityUnit.KG);

			Factory.Save();
			var collection = new CartageZoneCollection(line);
			collection.Load();

			AssertEquals(4, collection.Count);
			AssertEquals(collection[0].ZonePK, ZGuid.Empty);
			AssertEquals(collection[1].ZonePK, provider1.Zones[0].PK);
			AssertEquals(collection[2].ZonePK, provider1.Zones[1].PK);
			AssertEquals(collection[3].ZonePK, provider1.Zones[2].PK);

			rate.TH_OH = client2.PK;
			Factory.Save();
			collection.Load();

			AssertEquals(3, collection.Count);
			AssertEquals(collection[0].ZonePK, ZGuid.Empty);
			AssertEquals(collection[1].ZonePK, provider2.Zones[0].PK);
			AssertEquals(collection[2].ZonePK, provider2.Zones[1].PK);
		}

		public void TestConstraint_TH_RateType()
		{
			foreach (var field in typeof(RatingConstants.RatingHeaderTypes).GetFields())
			{
				CheckConstraints((string)(field.GetRawConstantValue()));
			}

			CheckConstraints(ZString.Empty);
		}

		void CheckConstraints(ZString rateType)
		{
			var header = Factory.NewWithValidTestData<RatingHeader>();
			header.TH_RateType = rateType;
			header.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			if (header.IsTariff())
			{
				header.TH_GlobalRateLevel = 1;
				header.TH_OH = ZGuid.Empty;
			}

			if (rateType == RatingConstants.RatingHeaderTypes.IntercompanyTariff)
			{
				header.TH_GC = ZGuid.Empty;
			}

			header.TH_QuoteDate = ZDate.Today;

			if (rateType.IsEmpty || header.IsWiseCostRate())
			{
				AssertExceptionThrown<ZSaveException>("Constraint_TH_RateType should not allow this type: " + rateType, () => Factory.Save());
			}
			else
			{
				AssertNoExceptionThrown(rateType, () => Factory.Save());
			}
		}

		#endregion

		#region TypeDecider

		public void TestTypeDecider()
		{
			var assembly = typeof(RatingHeader).Assembly;
			var assemblyTypes = assembly.GetTypes();
			var headerTypes = new Dictionary<ZGuid, Type>();
			foreach (var type in assemblyTypes.Where(RatingHeaderFilter))
			{
				var header = (RatingHeader)Factory.New(type);
				if (!header.IsTariff())
				{
					header.TH_OH = Helper.NewOrgHeader().PK;
				}
				headerTypes.Add(header.PK, type);
			}
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			foreach (var headerPK in headerTypes.Keys)
			{
				var header = factory2.Load<RatingHeader>(headerPK);
				var type1 = headerTypes[headerPK];
				AssertEquals(type1.Name + " should be set in RatingHeaderTypeDecider.FromString", type1, header.GetType());
			}
		}

		#endregion

		#region DocManagerInfo

		public void TestDocManagerInfo()
		{
			var assembly = typeof(RatingHeader).Assembly;
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes.Where(RatingHeaderFilter).Where(x => x != typeof(IntercompanyTariff)))
			{
				var header = (RatingHeader)Factory.New(type);
				var info = ((IDocManagerSupport)header).DocManagerInfo;
				Assert(type.Name + " should be set in IDocManagerSupport.DocManagerInfo", !info.DocManagerCode.IsEmpty);
			}
		}

		public void TestDocManagerInfo_CorrectCode()
		{
			var ratingHeader = Factory.New<RatingHeader>();
			var defaultCompanyPk = ratingHeader.TH_GC;
			IDocManagerSupport docManagerSupport = ratingHeader;

			var docManagerCodes = new List<(string headerType, bool isGlobal, string expectedCode)>
			{
				(RatingConstants.RatingHeaderTypes.ClientRate, false, "CRT"),
				(RatingConstants.RatingHeaderTypes.Tariff, false, "CTF"),
				(RatingConstants.RatingHeaderTypes.Quote, false, "QUO"),
				(RatingConstants.RatingHeaderTypes.Costing, false, "CST"),
				(RatingConstants.RatingHeaderTypes.ClientRate, true, "GCL"),
				(RatingConstants.RatingHeaderTypes.Tariff, true, "GCT"),
				(RatingConstants.RatingHeaderTypes.Costing, true, "GCO")
			};

			foreach (var docManagerCode in docManagerCodes)
			{
				ratingHeader.TH_GC = docManagerCode.isGlobal ? ZGuid.Empty : defaultCompanyPk;
				ratingHeader.TH_RateType = docManagerCode.headerType;
				AssertEquals("DocManagerCode for RateType " + docManagerCode.headerType, docManagerCode.expectedCode, docManagerSupport.DocManagerInfo.DocManagerCode);
			}
		}

		#endregion

		#region RelatedTypeCodes

		public void TestRelatedTypeCodes()
		{
			ZString clientRate = RatingConstants.RatingHeaderTypes.ClientRate;
			ZString tariff = RatingConstants.RatingHeaderTypes.Tariff;
			ZString quote = RatingConstants.RatingHeaderTypes.Quote;
			ZString costing = RatingConstants.RatingHeaderTypes.Costing;
			ZString intercompanyTariff = RatingConstants.RatingHeaderTypes.IntercompanyTariff;

			var resultSet = new Dictionary<Type, ZString[]>();
			resultSet.Add(typeof(ClientRate), new[] { clientRate, tariff, quote, costing, intercompanyTariff });
			resultSet.Add(typeof(CompanyTariff), new[] { clientRate, tariff, quote, costing, intercompanyTariff });
			resultSet.Add(typeof(GlobalTariff), new[] { clientRate, tariff, quote, costing, intercompanyTariff });
			resultSet.Add(typeof(Quote), new[] { clientRate, tariff, quote, costing, intercompanyTariff });
			resultSet.Add(typeof(Costing), new[] { clientRate, tariff, quote, costing, intercompanyTariff });
			resultSet.Add(typeof(IntercompanyTariff), new[] { clientRate, tariff, quote, costing, intercompanyTariff });

			var assembly = typeof(RatingHeader).Assembly;
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes.Where(RatingHeaderFilter))
			{
				var expectedCodes = resultSet[type];
				AssertEquals(type.Name, IndexOf(expectedCodes, 0), clientRate);
				AssertEquals(type.Name, IndexOf(expectedCodes, 1), tariff);
				AssertEquals(type.Name, IndexOf(expectedCodes, 2), quote);
				AssertEquals(type.Name, IndexOf(expectedCodes, 3), costing);
				AssertEquals(type.Name, IndexOf(expectedCodes, 4), intercompanyTariff);
			}

			ZString IndexOf(ZString[] array, int index) => array.Length > index ? array[index] : ZString.Empty;
		}

		#endregion

		#region Human Readable Name

		public void TestHumanReadableName()
		{
			var resultSet = new Dictionary<Type, string>();
			resultSet.Add(typeof(ClientRate), "Client Rate");
			resultSet.Add(typeof(CompanyTariff), "Company Tariff");
			resultSet.Add(typeof(GlobalTariff), "Global Tariff");
			resultSet.Add(typeof(Quote), "Quotation");
			resultSet.Add(typeof(Costing), "Costing");
			resultSet.Add(typeof(IntercompanyTariff), "Intercompany Tariff");

			var assembly = typeof(RatingHeader).Assembly;
			var assemblyTypes = assembly.GetTypes();
			foreach (var type in assemblyTypes.Where(RatingHeaderFilter))
			{
				var expectedHumanReadableName = resultSet[type];
				var header = (RatingHeader)Factory.New(type);
				AssertEquals(type.Name, expectedHumanReadableName, header.HumanReadableName);
			}
		}

		public void TestHumanReadableName_CostingWithGroupRates_ReturnCostingName()
		{
			var supplier = Helper.NewOrgHeader();
			var groupSupplier1 = Helper.NewOrgHeader();
			Factory.Save();

			groupSupplier1.RelatedManagementSubsidiaryRelations.AddOrganisation(supplier);
			Factory.Save();

			var costing = Helper.NewCosting(groupSupplier1);

			AssertEquals("Should have correct header", "Costing", costing.HumanReadableName);
		}

		public void TestHumanReadableName_GlobalCosting()
		{
			var globalCosting = Helper.NewGlobalCosting(Helper.NewOrgHeader());
			AssertEquals("Should have correct header", "Global Costing", globalCosting.HumanReadableName);
		}

		public void TestHumanReadableName_GlobalClientRate()
		{
			var globalRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			AssertEquals("Should have correct header", "Global Client Rate", globalRate.HumanReadableName);
		}

		public void TestHumanReadableName_GlobalTariff()
		{
			var globalTariff = Helper.NewGlobalTariff();
			AssertEquals("Should have correct header", "Global Tariff", globalTariff.HumanReadableName);
		}

		public void TestHumanReadableShortcutName_ClientRate()
		{
			var client = Helper.NewOrgHeader();
			client.OH_Code = "AAASYD";
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = client.PK;

			AssertEquals("Client Rate AAASYD", clientRate.HumanReadableShortcutName);

			client = Helper.NewOrgHeader();
			client.OH_Code = "BBBMEL";
			clientRate.TH_OH = client.PK;

			AssertEquals("Client Rate BBBMEL", clientRate.HumanReadableShortcutName);
		}

		public void TestHumanReadableShortcutName_CompanyTariffs()
		{
			var baseCompanyTariff = Helper.NewCompanyTariff();
			baseCompanyTariff.Factory.Save();

			AssertEquals("Pre-condition", new ZByte(1), baseCompanyTariff.TH_GlobalRateLevel);
			Assert("Company tariffs should have a description defaulted", !baseCompanyTariff.TH_GlobalRateDescription.IsEmpty);
			AssertEquals(baseCompanyTariff.TH_GlobalRateDescription, baseCompanyTariff.HumanReadableShortcutName);

			baseCompanyTariff.TH_GlobalRateDescription = "Overriden Description";

			AssertEquals("Overriden Description", baseCompanyTariff.HumanReadableShortcutName);

			var companyTariffLevel2 = Helper.NewCompanyTariff();
			companyTariffLevel2.Factory.Save();

			AssertEquals("Pre-condition", new ZByte(2), companyTariffLevel2.TH_GlobalRateLevel);
			AssertEquals("Expected description", "Company Tariff Level 2", companyTariffLevel2.TH_GlobalRateDescription);
			AssertEquals(companyTariffLevel2.TH_GlobalRateDescription, companyTariffLevel2.HumanReadableShortcutName);

			companyTariffLevel2.TH_GlobalRateDescription = ZString.Empty;

			AssertEquals("If the description is empty, fall back to default", "Company Tariff 2", companyTariffLevel2.HumanReadableShortcutName);

			companyTariffLevel2.TH_GlobalRateDescription = "Base Company Tariff";

			AssertEquals("We're not going to validate against stupidity", "Base Company Tariff", companyTariffLevel2.HumanReadableShortcutName);
		}

		public void TestHumanReadableShortcutName_Costings()
		{
			var costing = Factory.New<Costing>();
			Assert("Pre-condition", costing.TH_OH.IsEmpty);
			Assert("Standard rate should default a description", !costing.TH_GlobalRateDescription.IsEmpty);

			AssertEquals(costing.TH_GlobalRateDescription, costing.HumanReadableShortcutName);

			costing.TH_GlobalRateDescription = ZString.Empty;
			Assert("Standard Costing should have a description", costing.TH_GlobalRateDescriptionInfo.HasErrors());

			var serviceProviderOrg = Helper.NewOrgHeader();
			serviceProviderOrg.OH_Code = "HELPER";

			costing.TH_OH = serviceProviderOrg.PK;

			AssertEquals("Costing HELPER", costing.HumanReadableShortcutName);
		}

		public void TestHumanReadableShortcutName_Quotation()
		{
			var quote = Factory.New<Quote>();
			quote.TH_OH = Helper.NewOrgHeader().PK;
			Factory.Save();

			Assert("Pre-condition", !quote.TH_QuoteNumber.IsEmpty);
			AssertEquals("Pre-condition", "Quotation", Env.Registry.Rating.QuoteTitleText);
			AssertEquals("Quotation " + quote.TH_QuoteNumber, quote.HumanReadableShortcutName);

			var copiedQuote = quote.CopyIncludingChildren() as Quote;
			Factory.Save();

			AssertNotNull(copiedQuote);
			AssertEquals("Quotation " + copiedQuote.TH_QuoteNumber, copiedQuote.HumanReadableShortcutName);

			var newQuote = Factory.New<Quote>();
			newQuote.TH_OH = Helper.NewOrgHeader().PK;

			AssertEquals("Quotation " + newQuote.TH_QuoteNumber, newQuote.HumanReadableShortcutName);
		}

		#endregion

		#region Print Origin / Destination Charges

		public void TestPrintInheritedOriginDestinationCharges()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			Assert(!rate.TH_PrintInheritedOriginChargesInfo.ReadOnly);
			Assert(!rate.TH_PrintInheritedDestinationChargesInfo.ReadOnly);

			rate.TH_PrintRateLevelOriginCharges = false;
			Assert(!rate.TH_PrintInheritedOriginCharges);
			Assert(rate.TH_PrintInheritedOriginChargesInfo.ReadOnly);
			rate.TH_PrintRateLevelOriginCharges = true;
			Assert(rate.TH_PrintInheritedOriginCharges);
			Assert(!rate.TH_PrintInheritedOriginChargesInfo.ReadOnly);

			rate.TH_PrintRateLevelDestinationCharges = false;
			Assert(!rate.TH_PrintInheritedDestinationCharges);
			Assert(rate.TH_PrintInheritedDestinationChargesInfo.ReadOnly);
			rate.TH_PrintRateLevelDestinationCharges = true;
			Assert(rate.TH_PrintInheritedDestinationCharges);
			Assert(!rate.TH_PrintInheritedDestinationChargesInfo.ReadOnly);
		}

		#endregion

		#region Show Costing And/Or Company Tariff And/Or Client Rates

		public void TestShowCostingCompanyTariffClientRatesDefaults()
		{
			RatingDataRegistry.Instance.ShowCostingsDuringRating.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			RatingHeader header = Helper.NewQuote(Helper.NewOrgHeader());
			Assert(header.ShowClientRates);
			Assert(!header.ShowClientRatesInfo.ReadOnly);
			Assert(header.ShowCompanyTariff);
			Assert(!header.ShowCompanyTariffInfo.ReadOnly);
			Assert(!header.ShowCosting);
			Assert(!header.ShowCostingInfo.ReadOnly);

			RatingDataRegistry.Instance.ShowCostingsDuringRating.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			header = Helper.NewQuote(Helper.NewOrgHeader());
			Assert(header.ShowClientRates);
			Assert(!header.ShowClientRatesInfo.ReadOnly);
			Assert(header.ShowCompanyTariff);
			Assert(!header.ShowCompanyTariffInfo.ReadOnly);
			Assert(header.ShowCosting);
			Assert(!header.ShowCostingInfo.ReadOnly);

			header = Helper.NewClientRate(Helper.NewOrgHeader());
			Assert(!header.ShowClientRates);
			Assert(header.ShowClientRatesInfo.ReadOnly);
			Assert(header.ShowCompanyTariff);
			Assert(!header.ShowCompanyTariffInfo.ReadOnly);
			Assert(header.ShowCosting);
			Assert(!header.ShowCostingInfo.ReadOnly);

			header = Helper.NewCompanyTariff();
			Assert(!header.ShowClientRates);
			Assert(header.ShowClientRatesInfo.ReadOnly);
			Assert(!header.ShowCompanyTariff);
			Assert(header.ShowCompanyTariffInfo.ReadOnly);
			Assert(header.ShowCosting);
			Assert(!header.ShowCostingInfo.ReadOnly);

			header = Helper.NewCosting(Helper.NewOrgHeader());
			Assert(!header.ShowClientRates);
			Assert(header.ShowClientRatesInfo.ReadOnly);
			Assert(!header.ShowCompanyTariff);
			Assert(header.ShowCompanyTariffInfo.ReadOnly);
			Assert(header.ShowCosting);
			Assert(!header.ShowCostingInfo.ReadOnly);

			header = Helper.NewCosting(null);
			header.TH_OH = ZGuid.Empty;
			Assert(!header.ShowClientRates);
			Assert(header.ShowClientRatesInfo.ReadOnly);
			Assert(!header.ShowCompanyTariff);
			Assert(header.ShowCompanyTariffInfo.ReadOnly);
			Assert(!header.ShowCosting);
			Assert(header.ShowCostingInfo.ReadOnly);
		}

		#endregion

		#region TestChangingCompanyUpdatesStaffAssignments

		public void TestChangingCompanyUpdatesStaffAssignments()
		{
			var salesRep1 = Factory.New<GlbStaff>();
			salesRep1.GS_LoginName = "SALESREP1";
			salesRep1.GS_Code = "SP1";

			var salesRep2 = Factory.New<GlbStaff>();
			salesRep2.GS_LoginName = "SALESREP2";
			salesRep2.GS_Code = "SP2";

			var testOrg = Helper.NewOrgHeader();

			var salesRepAssignmentCurrentCompany = testOrg.StaffAssignments.AddNew();
			salesRepAssignmentCurrentCompany.O8_GC = GlbCompany.CurrentCompany.PK;
			salesRepAssignmentCurrentCompany.O8_GS_NKPersonResponsible = salesRep1.GS_Code;
			salesRepAssignmentCurrentCompany.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			var otherCompany = Factory.New<GlbCompany>();
			Factory.Save();

			var otherBranch = otherCompany.Branches.AddNew();

			var salesRepAssignmentOtherCompany = testOrg.StaffAssignments.AddNew();
			salesRepAssignmentOtherCompany.O8_GC = otherCompany.PK;
			salesRepAssignmentOtherCompany.O8_GS_NKPersonResponsible = salesRep2.GS_Code;
			salesRepAssignmentOtherCompany.O8_Role = StaffAssignmentRoles.Codes.SalesRep;

			Factory.Save();

			var testQuote = Helper.NewQuote(testOrg);
			AssertEquals("SalesRepresentative should be SalesRep1", salesRep1.GS_Code, testQuote.HeaderStaffAssignments.OverallSalesRep);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				testQuote.TH_GC = otherCompany.PK;
				AssertEquals("SalesRepresentative should be SalesRep2", salesRep2.GS_Code, testQuote.HeaderStaffAssignments.OverallSalesRep);
			}
		}

		#endregion

		#region Clone

		[TestDate(2015, 08, 07)]
		public void TestCopyIncludingChildren()
		{
			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)entry.RateLines[0].Calculator).PerUnit = 5M;

			var expiredEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "AUBNE");
			expiredEntry.TI_RateStartDate = new ZDate(2015, 01, 01);
			expiredEntry.TI_RateEndDate = new ZDate(2015, 08, 01);

			expiredEntry.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)expiredEntry.RateLines[0].Calculator).PerUnit = 4M;

			var newRate = rate.CopyIncludingChildren();
			AssertEquals("Should not copy expired rate", 1, newRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			RateEntry newEntry = newRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR)[0];
			AssertEquals(1, newEntry.RateLines.Count);
			AssertEquals(1, newEntry.RateLines[0].RateLineItems.Count);
			AssertEquals(5m, ((UnitCalculator)newEntry.RateLines[0].Calculator).PerUnit);
		}

		[ExpectNoExceptions]
		public void TestNoAdditionalRateLineItemAddedToOriginal()
		{
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX");
			var rateLine = entry.AddRateLine("ODOC", PercentageCalculator.Code);
			rateLine.InitializeCalculator();

			var applyToItem = ((PercentageCalculator)rateLine.Calculator).AddApplyToItem(CalculatorConstants.Text.FreightCharges);
			((PercentageCalculator)rateLine.Calculator).Percent = 10m;
			var percentageItem = ((PercentageCalculator)rateLine.Calculator).FindRateLineItem(CalculatorConstants.Type.PER);
			foreach (var item in rateLine.RateLineItems.ToArray())
			{
				if (item != applyToItem && item != percentageItem)
				{
					rateLine.RateLineItems.RemoveAndDelete(item);
				}
			}
			Factory.Save();

			var loadedRate = new BusinessObjectFactory().Load<ClientRate>(rate.PK);
			RateLine loadedRateLine = loadedRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0].RateLines[0];
			AssertEquals("Pre-condidtion: 2 items in original RateLine", 2, loadedRateLine.RateLineItems.Count);

			var newRate = loadedRate.CopyIncludingChildren();
			AssertEquals(1, newRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);

			RateEntry newEntry = newRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];
			AssertEquals(1, newEntry.RateLines.Count);
			AssertEquals("Copied RateLine has 2 items", 2, newEntry.RateLines[0].RateLineItems.Count);
			AssertEquals("No extra items added to original RateLine", 2, rateLine.RateLineItems.Count);
		}

		#endregion

		#region Universal Copy

		public void TestUniversalCopyIgnoreElement()
		{
			var ratingHeader = Factory.New<RatingHeader>();
			var componentType = ratingHeader.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("TH_OneTimeQuote", "TH_OneTimeQuote", attribute.ElementNames);
		}

		#endregion

		#region TestHasSelectedRateLineChangedHandlers

		public void TestHasSelectedRateLineChangedHandlers()
		{
			var rate = Helper.NewClientRate(NewClient);
			Assert("No handlers attached", !rate.HasSelectedRateLineChangedHandlers);

			rate.SelectedRateLineChanged += rate_SelectedRateLineChanged;
			Assert("There is a handler", rate.HasSelectedRateLineChangedHandlers);

			rate.SelectedRateLineChanged -= rate_SelectedRateLineChanged;
			Assert("No handlers again", !rate.HasSelectedRateLineChangedHandlers);
		}

		void rate_SelectedRateLineChanged(RateEntry sender, int index)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ICancellable

		public void TestICancellable()
		{
			var header = Factory.New<RatingHeader>();
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(header.GetType()));
			AssertEquals("CanCancel()", "Record can't be marked as Inactive. It does not support Inactivating/Activating", header.CanCancel());
			AssertEquals("CanReactivate()", "Record can't be marked as Active. It does not support Inactivating/Activating", header.CanReactivate());
		}

		#endregion

		#region Saving

		public void TestFactorySaving_ShouldNotUpdateAcceptedDate()
		{
			var factory = new BusinessObjectFactory();
			var costing = factory.New<Costing>();
			factory.Save();

			costing.Logs.AddNew(Events.DataExport, "McLaren", ZDateTimeOffset.Now);
			factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			costing = anotherFactory.Load<Costing>(costing.PK);

			AssertEquals(
				@"Expected to not be updated. Updating a property on every factory save may call conflicts for one CW1 instance when the same object is open and saved in another CW1 instance.
It is crucial if the business objects is being updated in a batch process when there is no human to resolve conflicts - in this way the save will fail.
As an example, this happened to TACT import.",
				ZDateTime.Empty,
				costing.TH_Accepted
			);
		}

		#endregion

		#region OnPreSaveValidation

		public void TestNoExtraParentCollectionsAfterValidation()
		{
			var header = Helper.NewClientRate(NewClient);
			var entry1 = header.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "AUMEL");
			var entry2 = header.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "AUMEL");
			AssertEquals("Entry has one parent collection", 1, ((IBusinessObjectInternals)entry1).ParentCollections.Length);
			AssertEquals("Entry has one parent collection", 1, ((IBusinessObjectInternals)entry2).ParentCollections.Length);

			header.RunPreSaveValidation();
			AssertEquals("Entry should have row errors", true, entry1.RowErrors.Count() > 0);
			AssertEquals("Entry should have row errors", true, entry2.RowErrors.Count() > 0);
			AssertEquals("Header should have error", true, header.HasErrors);
			AssertEquals("Entry still has one parent collection", 1, ((IBusinessObjectInternals)entry1).ParentCollections.Length);
			AssertEquals("Entry still has one parent collection", 1, ((IBusinessObjectInternals)entry2).ParentCollections.Length);
		}

		#endregion

		#region TestLoadRatingHeaderFromDifferentType

		public void TestLoadRatingHeaderFromDifferentType()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			Factory.Save();

			var loadClientRateFromclientRate = Factory.Load<ClientRate>(clientRate.PK);
			AssertEquals("SAL", loadClientRateFromclientRate.TH_RateType);

			var loadClientRateFromQuote = Factory.Load<ClientRate>(quote.PK);
			AssertEquals("QTE", loadClientRateFromQuote.TH_RateType);
		}

		#endregion

		public void TestClientRateIsNotMistakenForCompanyTariff()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			tariff1.TH_GlobalRateLevel = new ZByte(1);
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("Pre-condition", new ZByte(1), tariff1.TH_GlobalRateLevel);

			var tariff2 = Factory.New<CompanyTariff>();
			tariff2.TH_GlobalRateLevel = new ZByte(2);
			tariff2.TH_RateType = RatingConstants.RatingHeaderTypes.Costing;

			var message = "Should not be able to save with company tariff level set on this invalid rating header";
			AssertExceptionThrown<ZSaveException>(message, () => Factory.Save());

			tariff2.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;

			AssertNoExceptionThrown("No longer violates Constraint_TH_GlobalRateLevel", () => Factory.Save());
		}

		#region Global Rating Header

		public void TestGlobalTariffEntry_IsNotLoadedByRateEntryCollectionByDefaultUserFilter()
		{
			var companyTariff = Factory.New<CompanyTariff>();
			var globalTariff = Factory.New<GlobalTariff>();

			AssertIsNotLoadedByRateEntryCollectionByDefaultUserFilter(companyTariff, globalTariff);
		}

		public void TestGlobalClientRateEntry_IsNotLoadedByRateEntryCollectionByDefaultUserFilter()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			var globalClientRate = Helper.NewGlobalClientRate(client);

			AssertIsNotLoadedByRateEntryCollectionByDefaultUserFilter(clientRate, globalClientRate);
		}

		public void TestGlobalCostingEntry_IsNotLoadedByRateEntryCollectionByDefaultUserFilter()
		{
			var localCosting = Helper.NewCosting(null);
			var globalCosting = Helper.NewGlobalCosting(null);

			AssertIsNotLoadedByRateEntryCollectionByDefaultUserFilter(localCosting, globalCosting);
		}

		void AssertIsNotLoadedByRateEntryCollectionByDefaultUserFilter(RatingHeader localRatingHeader, RatingHeader globalRatingHeader)
		{
			AssertEquals("Pre-condition", globalRatingHeader.PK, localRatingHeader.GlobalRatingHeader.PK);

			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Factory.Save();

			var globalAIRRateEntry = globalRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AE", "");
			var globalLCLRateEntry = globalRatingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "BE", "");
			var globalSORRateEntry = globalRatingHeader.AddRateEntry(RatingConstants.RateCategory.SOR, Constants.RateMode.ALL, "CI", "");

			var localAIRRateEntry1 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "DE", "");
			var localAIRRateEntry2 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "DE", "AU");
			var localLCLRateEntry1 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "EE", "");
			var localLCLRateEntry2 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "EE", "AU");
			var localSORRateEntry1 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.SOR, Constants.RateMode.ALL, "FI", "");
			var localSORRateEntry2 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.SOR, Constants.RateMode.ALL, "FI", "AU");

			Factory.Save();

			var airCollection = localRatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			var lclCollection = localRatingHeader.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection;
			var sorCollection = localRatingHeader.EntryCollections[RatingConstants.RateCategory.SOR].LazyLoadingCollection;
			airCollection.LoadAndSortForGUI();
			lclCollection.LoadAndSortForGUI();
			sorCollection.LoadAndSortForGUI();

			var message = "Global Rates should not be included with Local Rates with DefaultUserFilter";
			AssertContainsExactElementsInAnyOrder(message, new[] { localAIRRateEntry1.PK, localAIRRateEntry2.PK }, airCollection.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(message, new[] { localLCLRateEntry1.PK, localLCLRateEntry2.PK }, lclCollection.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(message, new[] { localSORRateEntry1.PK, localSORRateEntry2.PK }, sorCollection.Select(x => x.PK));

			var query = new ZQuery(RateEntrySchema.TI_TH, new[] { localRatingHeader.PK, globalRatingHeader.PK });
			var filter = new RateEntryFilterStripBusinessObjectForTest(query);

			lclCollection.SetUserFilter(filter);
			lclCollection.Load();

			message = "Global Rate should be included when the default filter is overriden";
			AssertContainsExactElementsInAnyOrder(message, new[] { globalLCLRateEntry.PK, localLCLRateEntry1.PK, localLCLRateEntry2.PK }, lclCollection.Select(x => x.PK));
		}

		public void TestGlobalTariffEntry_IsNotIncludedWithLocalRatesForAutoRatingResults()
		{
			var localTariff = Helper.NewCompanyTariff();
			AssertIsNotIncludedWithLocalRatesForAutoRatingResults(localTariff);
		}

		public void TestGlobalClientRateEntry_IsNotIncludedWithLocalRatesForAutoRatingResults()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);
			AssertIsNotIncludedWithLocalRatesForAutoRatingResults(clientRate);
		}

		public void TestGlobalCostingEntry_IsNotIncludedWithLocalRatesForAutoRatingResults()
		{
			var localCosting = Helper.NewCosting(null);
			AssertIsNotIncludedWithLocalRatesForAutoRatingResults(localCosting);
		}

		void AssertIsNotIncludedWithLocalRatesForAutoRatingResults(RatingHeader localRatingHeader)
		{
			var localEntry = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AU", "");
			Factory.Save();

			var globalCosting = new RateCreator(localRatingHeader).LoadOrCreateGlobalRatingHeader(Factory);
			var globalEntry = globalCosting.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "");

			var localResults = localRatingHeader.LoadRateEntriesForAutoRater(new ZQuery());
			var globalResults = globalCosting.LoadRateEntriesForAutoRater(new ZQuery());

			var thePointOfThisTest = "Global Rates should appear on the GUI as part of the Local Rates collection but should NEVER be included in rating results to avoid duplication";
			AssertContainsExactElementsInAnyOrder(thePointOfThisTest, new[] { globalEntry }, globalResults);
			AssertContainsExactElementsInAnyOrder(thePointOfThisTest, new[] { localEntry }, localResults);
		}

		public void TestGlobalRatingHeaderKey()
		{
			var client1 = Helper.NewOrgHeader();
			var client2 = Helper.NewOrgHeader();
			var localClientRate = Helper.NewClientRate(client1);

			var expected = $"GlobalRate{localClientRate.PK}.{client1.PK}";

			AssertEquals(expected, localClientRate.GlobalRatingHeaderKey);

			localClientRate.TH_OH = client2.PK;
			expected = $"GlobalRate{localClientRate.PK}.{client2.PK}";

			AssertEquals("Changing the related OrgHeader should change the key", expected, localClientRate.GlobalRatingHeaderKey);
		}

		public void TestCanPublishRates_ClientRates()
		{
			var isAllowed = Env.Security.GlobalClientRates.IsAllowed;
			try
			{
				var client = Helper.NewOrgHeader();
				var clientRate = Helper.NewClientRate(client);
				Env.Security.GlobalClientRates.IsAllowed = false;

				Assert("Does not have permission to publish", !clientRate.SupportsRateEntryPublish());

				Env.Security.GlobalClientRates.IsAllowed = true;

				Assert(clientRate.SupportsRateEntryPublish());

				var globalClientRate = Helper.NewGlobalClientRate(client);
				Assert("Global Client Rate is already published", !globalClientRate.SupportsRateEntryPublish());
			}
			finally
			{
				Env.Security.GlobalClientRates.IsAllowed = isAllowed;
			}
		}

		public void TestCanPublishRates_Costing()
		{
			var isAllowed = Env.Security.GlobalCostingRates.IsAllowed;
			try
			{
				var costing = Helper.NewCosting(Helper.CreateCreditor());
				Env.Security.GlobalCostingRates.IsAllowed = false;

				Assert("Does not have permission to publish", !costing.SupportsRateEntryPublish());

				Env.Security.GlobalCostingRates.IsAllowed = true;

				Assert(costing.SupportsRateEntryPublish());

				var globalCosting = Helper.NewGlobalCosting(Helper.CreateCreditor());
				Assert("Global Costing is already published", !globalCosting.SupportsRateEntryPublish());
			}
			finally
			{
				Env.Security.GlobalCostingRates.IsAllowed = isAllowed;
			}
		}

		public void TestCanPublishRates_Tariffs()
		{
			var isAllowed = Env.Security.GlobalTariffRates.IsAllowed;
			try
			{
				var companyTariff = Helper.NewCompanyTariff();
				companyTariff.Factory.Save();
				Env.Security.GlobalTariffRates.IsAllowed = false;

				Assert("Does not have permission to publish", !companyTariff.SupportsRateEntryPublish());

				Env.Security.GlobalTariffRates.IsAllowed = true;

				Assert(companyTariff.SupportsRateEntryPublish());

				var additionalCompanyTariff = Helper.NewCompanyTariff();

				Assert("Additional Company Tariffs should not be able to publish rate entries", !additionalCompanyTariff.SupportsRateEntryPublish());

				var globalTariff = Helper.NewGlobalTariff();
				Assert("Global Tariffs are already published", !globalTariff.SupportsRateEntryPublish());
			}
			finally
			{
				Env.Security.GlobalTariffRates.IsAllowed = isAllowed;
			}
		}

		public void TestCanPublishRates_Quotes()
		{
			var client = Helper.NewOrgHeader();
			var quote = Helper.NewQuote(client);

			Assert("Should never be able to publish quote entries directly", !quote.SupportsRateEntryPublish());
		}

		#endregion

		public void TestRunPreSaveValidation_WhenEntriesDeleted_OnlyLoadedEntriesValidated()
		{
			var costingToSave = Helper.NewCosting(null);
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "", "AUSYD", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "", "NZAKL", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, "", "AUSYD", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var costing = newFactory.Load<Costing>(costingToSave.PK);
			var orgRateList = costing.ORGRateEntriesForBinding;

			AssertEquals("PRE: Origin rates are loaded", 2, orgRateList.Count);
			AssertEquals("PRE: no other rates loaded", 2, costing.FetchAllEntriesFromLocalCache().Length);

			orgRateList[0].Delete();
			((IBusiness)costing).RunPreSaveValidationFetch(true);
			costing.RunPreSaveValidation();

			AssertEquals("origin rates remaining", 1, orgRateList.Count);
			AssertEquals("no other rates were loaded for validation", 1, costing.FetchAllEntriesFromLocalCache().Length);
		}

		public void TestRunPreSaveValidation_ValidateOnlyLoadedRatesIsSetToYes_ValidateOnlyInMemoryRates()
		{
			string rateToString(RateEntry r)
			{
				var rateLine = r.RateLines.Cast<RateLine>().First();
				return FormattableString.Invariant(
					$"{r.TI_OriginLRC}|{r.TI_DestinationLRC}|{r.TI_RateCategory}|{rateLine.ChargeCode.AC_Code}|{(int)((FlatCalculator)rateLine.Calculator).BaseRate}");
			}

			var costingToSave = Helper.NewCosting(null);
			var rate1 = costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			var rate2 = costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "", "AUSYD", "USLAX", "ODOC", 200m, "AUD");
			var rate3 = costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, "", "UAIEV", "USLAX", "ODOC", 300m, "AUD");
			var rate4 = costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, "", "AUSYD", "USLAX", "DDOC", 400m, "AUD");
			var rate5 = costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, "", "UAIEV", "USLAX", "DDOC", 500m, "AUD");

			rate2.RunPreSaveValidation();
			rate3.RunPreSaveValidation();
			rate5.RunPreSaveValidation();

			Factory.Save();

			// All rates but rate1 and rate4 must be already validated
			AssertEquals("PRECONDITION", false, ((ILightValidationInternals)rate1).IsValid);
			AssertEquals("PRECONDITION", true, ((ILightValidationInternals)rate2).IsValid);
			AssertEquals("PRECONDITION", true, ((ILightValidationInternals)rate3).IsValid);
			AssertEquals("PRECONDITION", false, ((ILightValidationInternals)rate4).IsValid);
			AssertEquals("PRECONDITION", true, ((ILightValidationInternals)rate5).IsValid);

			using (RatingDataRegistry.Instance.ValidateOnlyLoadedRatesUponSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var costing = newFactory.Load<Costing>(costingToSave.PK);

				// Simulate opening tab with Origin rates
				var orgRateList = costing.ORGRateEntriesForBinding;

				var actual1 = costing.FetchAllEntriesFromLocalCache().Select(rateToString).ToArray();
				var expected1 = new[]
				{
					"AUSYD|USLAX|ORG|ODOC|200",
					"UAIEV|USLAX|ORG|ODOC|300"
				};
				AssertContainsExactElementsInAnyOrder(
					"Should contain only origin rates loaded in memory",
					expected1, actual1);

				// Update origin rate
				orgRateList[0].TI_OriginLRC = "SGSIN";

				// Revalidate the header
				((IBusiness)costing).RunPreSaveValidationFetch(true);
				costing.RunPreSaveValidation();

				// rate1 (AIR) and rate4 (DST) should now be loaded as well as they were not yet validated
				var actual2 = costing.FetchAllEntriesFromLocalCache().Select(rateToString).ToArray();
				var expected2 = new[]
				{
					"AUSYD|USLAX|AIR|FRT|100",
					"SGSIN|USLAX|ORG|ODOC|200",
					"UAIEV|USLAX|ORG|ODOC|300",
					"AUSYD|USLAX|DST|DDOC|400"
				};
				AssertContainsExactElementsInAnyOrder(
					"Should contain origin rates plus all other rates which are not yet validated",
					expected2, actual2);
			}

			using (RatingDataRegistry.Instance.ValidateOnlyLoadedRatesUponSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var costing = newFactory.Load<Costing>(costingToSave.PK);
				var orgRateList = costing.ORGRateEntriesForBinding;

				var actual3 = costing.FetchAllEntriesFromLocalCache().Select(rateToString).ToArray();
				var expected3 = new[]
				{
					"AUSYD|USLAX|ORG|ODOC|200",
					"UAIEV|USLAX|ORG|ODOC|300"
				};
				AssertContainsExactElementsInAnyOrder(
					"Should contain only origin rates loaded in memory",
					expected3, actual3);

				orgRateList[0].TI_OriginLRC = "SGSIN";

				((IBusiness)costing).RunPreSaveValidationFetch(true);
				costing.RunPreSaveValidation();

				// rate4 should not be loaded in this case as the registry specify that we want to validate only rates which are already loaded,
				// so, we don't additionally load other rates which were not yet loaded
				var actual4 = costing.FetchAllEntriesFromLocalCache().Select(rateToString).ToArray();
				var expected4 = new[]
				{
					"SGSIN|USLAX|ORG|ODOC|200",
					"UAIEV|USLAX|ORG|ODOC|300"
				};
				AssertContainsExactElementsInAnyOrder(
					"Should have only loaded rates since registry specify that we want to validate only rates which are already loaded",
					expected4, actual4);
			}
		}

		public void TestRunPreSaveValidation_ShouldNotAddDefaultRateLines()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];
			var inactiveChargeCode = Helper.ChargeCodes["XYZ"];
			inactiveChargeCode.AC_IsActive = false;

			Factory.Save();

			var chargeCodes = string.Join(",", frtChargeCode.PK, inactiveChargeCode.PK);

			using (RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodes))
			{
				var header = Helper.NewCosting(null);

				var entry = header.AddRateEntry(RatingConstants.RateCategory.AIR);
				var lines = entry.RateLines.Cast<RateLine>().ToArray();

				AssertContainsExactElementsInExactOrder(
					"Expected charges to be added by default after adding a new RateEntry",
					["FRT", "XYZ"],
					lines.Select(x => x.ChargeCode.AC_Code.ToString()));

				header.Validation.ValidateAll();
				Assert("There is an inactive charge code that cause an error for the new RateLines added by default", header.HasErrors);

				entry.RateLines.RemoveAndDeleteAll();

				header.Validation.ValidateAll();
				Assert("There shouldn't be any error as all RateLines has been deleted", !header.HasErrors);

				header.RunPreSaveValidation();
				Assert("No new line should be added during the pre save validation", entry.RateLines.IsNullOrEmpty());
				Assert("There shouldn't be any error as no new lines added", !header.HasErrors);
			}
		}

		public void TestRunPreSaveValidation_DbHits()
		{
			var costingToSave = Helper.NewCosting(null);
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AUSYD", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "US", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "NZ", "US", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, "NZAKL", "USLAX", "FRT", 100m, "AUD");
			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "FRT", 100m, "AUD");

			costingToSave.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, "SGSIN", "GBLON", "FRT", 100m, "AUD");
			Factory.Save();
			Factory.ClearQueryCache();

			using (RowFactory.SetCachedTables())
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var hits = new Dictionary<string, int> {
					{ RateEntrySchema.Constants.TableName, 1 },
					{ RateLinesSchema.Constants.TableName, 0 },
					{ RateLineItemsSchema.Constants.TableName, 0 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
				};

				var costing = newFactory.Load<Costing>(costingToSave.PK);
				// cause a full validation
				var entryToEdit = costing.DSTRateEntriesForBinding[0];
				entryToEdit.TI_RateStartDate = ZDate.Today.AddYears(-1);
				entryToEdit.RateLines.FirstOrDefault();
				// ensure the entry loaded into memory does not count towards the db hits
				((IBusiness)entryToEdit).RunPreSaveValidationFetch(true);
				entryToEdit.RunPreSaveValidation();
				newFactory.ResetDatabaseLoadCount();
				AssertEquals("PRE: only one rate in memory", 1, costing.FetchAllEntriesFromLocalCache().Length);

				using (AssertDbHitsWithUsefulQueryInformation(hits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
				{
					((IBusiness)costing).RunPreSaveValidationFetch(true);
					costing.RunPreSaveValidation();
				}
				var entries = costing.FetchAllEntriesFromLocalCache();
				AssertEquals("all rates were validated", 7, entries.Count(x => x.ValidationCountForTest > 0));
			}
		}

		public void TestValidationHappensForNewlyAddedPublishedRate()
		{
			var creditor = Helper.CreateCreditor();
			var costing = Helper.NewCosting(creditor);

			costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");

			Factory.Save();

			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "US", "");
			rateEntry.IsPublished = true;

			var progressReporterValidation = (IProgressReporterValidation)costing;
			var logs = new StringBuilder();
			using (progressReporterValidation.ReportProgress((message) => logs.Append(message)))
			{
				costing.RunPreSaveValidation();
			}

			var logsAsString = logs.ToString();

			CombineAssertions("Should show progress on 1 records i.e. AIR, LCL, SOR", () =>
			{
				AssertContains("0 of 1 records", logsAsString);
			});
		}

		public void TestDocumentFieldExcludeFromMap()
		{
			var oneTimeQuoteInfo = typeof(RatingHeader).GetProperty("TH_OneTimeQuote");
			Assert(Attribute.IsDefined(oneTimeQuoteInfo, typeof(DocumentFieldExcludeFromMapAttribute), false));
		}

		public void TestWorkflowSetFieldReadonly()
		{
			var oneTimeQuoteInfo = typeof(RatingHeader).GetProperty("TH_OneTimeQuote");
			Assert(Attribute.IsDefined(oneTimeQuoteInfo, typeof(WorkflowSetFieldReadonly), false));
		}

		#region Implementation

		string RenderDocumentNames(PrintTask task)
		{
			var builder = new StringBuilder();
			builder.AppendLine();

			for (var i = 0; i < task.Count; i++)
			{
				foreach (IDeliverable deliverable in task[i])
				{
					if (deliverable.IncludedInPrint)
					{
						builder.AppendLine(deliverable.Name);
					}
				}
			}

			return builder.ToString();
		}

		string RenderCommand(DocumentCommand menu)
		{
			var builder = new StringBuilder();
			builder.AppendLine();

			builder.AppendLine("[Documents]");
			foreach (StmMenuTemplatePivot pivot in menu.Documents)
			{
				builder.Append(pivot.SI_DocumentTitle);
				builder.Append("|");

				if (pivot.Template != null)
				{
					builder.AppendLine(pivot.Template.SO_Name);
				}
				else
				{
					builder.AppendLine("<null>");
				}
			}

			builder.AppendLine("[eDocs]");

			foreach (DocumentStmMenuEDocs edoc in menu.EDocs)
			{
				builder.AppendLine(edoc.SX_Description);
			}

			return builder.ToString();
		}

		static void SelectPages(Quote quote, params string[] pageNames)
		{
			var sets = quote.Factory.Load<RateAttachmentSet>(new ZQuery(RateAttachmentSetSchema.TS_AttachmentName, pageNames));

			quote.SelectedPages.RemoveAndDeleteAll();
			quote.SelectedPages.AddPagesToSelectedPagesCollection(sets);
		}

		public static void AssertNoRelatedEntityIsLoaded(BusinessObjectFactory factory)
		{
			var inMemoryEntriesCount = factory.Load<RateEntry>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length;
			AssertEquals(0, inMemoryEntriesCount);

			RateEntryTest.AssertNoRelatedEntityIsLoaded(factory);
		}

		public static void AssertRelatedEntitiesAreDeleted(BusinessObjectFactory factory, IEnumerable<ZGuid> entryPKs, IEnumerable<ZGuid> linePKs, IEnumerable<ZGuid> lineItemPKs, IEnumerable<ZGuid> stmNotePKs)
		{
			var enteriesCount = factory.Load<RateEntry>(new ZQuery(RateEntrySchema.PK, entryPKs)).Length;
			AssertEquals(0, enteriesCount);

			RateEntryTest.AssertRelatedEntitiesAreDeleted(factory, linePKs, lineItemPKs, stmNotePKs);
		}

		public static bool RatingHeaderFilter(Type type)
		{
			return type.IsSubclassOf(typeof(RatingHeader)) && type != typeof(GlobalTariff);
		}

		#endregion
	}

	#region Business Object TestCase

	[TestedType(typeof(RatingHeader))]
	public sealed class RatingHeaderBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<RatingHeader>();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var header = Factory.NewWithValidTestData<RatingHeader>();
			header.TH_RateType = string.Empty;
			return header;
		}

		protected override bool IsSuppressedForTestDbHits => true;

		public override void TestSaveAndDeleteBusinessObject()
		{
			AssertExceptionThrown<ZSaveException>("RatingHeader with invalid TH_RateType cannot be saved", () => base.TestSaveAndDeleteBusinessObject());
		}

		public override void TestCloneAuditProperties()
		{
			AssertExceptionThrown<ZSaveException>("RatingHeader with invalid TH_RateType cannot be saved", () => base.TestCloneAuditProperties());
		}

		public override void TestCloneAuditContextProperties()
		{
			AssertExceptionThrown<ZSaveException>("RatingHeader with invalid TH_RateType cannot be saved", () => base.TestCloneAuditContextProperties());
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}

	#endregion

	public static class RatingHeaderTestExtensions
	{
		// TODO: remove this helper and the class in a refactor task
		public static RateEntryCollection GetRateEntryCollectionForCategory(this RatingHeader rate, string category)
		{
			return rate.EntryCollections[category].LoadedCollection;
		}
	}
}
