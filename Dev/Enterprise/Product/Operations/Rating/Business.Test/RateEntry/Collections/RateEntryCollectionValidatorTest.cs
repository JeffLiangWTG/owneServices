using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateEntryCollectionValidatorTest : RatingTestCase
	{
		/*
		 * Case 1
		 * Old State: (whats in the db)
		 *
		 * Entry1: day 1 - 10
		 * Entry2: day 11 - 15
		 *
		 * New State:
		 *
		 * Entry1: day 1 - 4
		 * Entry2: day 5 - 10 <- Previously is compared against Entry 1 in old state
		 */
		public void TestValidate_OverlappingDates_WhenChanged_DoesNotHaveErrors()
		{
			//arrange
			var costing = Factory.NewWithValidTestData<Costing>();
			var entry1 = CreateEntry(costing, 1, 10);
			Factory.Save();

			//act
			var entry2 = CreateEntry(costing, 5, 12); // initially entry2 should overlap
			entry1.TI_RateEndDate = ZDate.Today.AddDays(3); // after changing entry1, there should be no overlaps
			costing.RunPreSaveValidation();

			//asserts
			AssertNoRowError(entry2, ErrorMessages.OverlappingDatesOnRateEntry);
		}

		/*
		 * Case 2
		 * Old State: (whats in the db)
		 *
		 * Entry1: day 1 - 10
		 *
		 * New State:
		 *
		 * Entry1: deleted
		 * Entry2: day 1 - 10 <- Previously is compared against Entry 1 in old state even though its deleted.
		 */
		public void TestValidate_DuplicateRateEntries_AfterDeletingOriginal_DoesNotHaveErrors()
		{
			//arrange
			var costing = Factory.NewWithValidTestData<Costing>();
			var entry = CreateEntry(costing, 1, 10);
			Factory.Save();

			//act
			costing.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.RemoveAndDelete(entry);
			var entry2 = CreateEntry(costing, 1, 10);
			costing.RunPreSaveValidation();

			//asserts
			AssertNoRowError(entry2, ErrorMessages.OverlappingDatesOnRateEntry);
		}

		public void TestValidate_LocalRateNotPublished_DoesNotLoadAnyGlobalEntries()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT", FlatCalculator.Code);
			globalChargeCode.Factory.Save();

			var localTariff = Factory.New<CompanyTariff>();
			CreateEntry(localTariff, 1, 10, "AUSYD", 1);

			var globalTariff = Factory.New<GlobalTariff>();
			CreateEntry(globalTariff, 1, 10, "AUSYD", 1);
			Factory.Save();

			// Modify an entry, but not to publish it
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var localTariffInFactory2 = factory2.Load<CompanyTariff>(localTariff.PK);
			var entry2 = (RateEntry)localTariffInFactory2.AllEntriesCollection[0];
			entry2.TI_RateStartDate = entry2.TI_RateStartDate.AddDays(1);
			var globalTariffInFactory2 = localTariffInFactory2.GlobalRatingHeader;
			AssertNotNull(globalTariffInFactory2);

			var hits = new Dictionary<string, int> {
					{ RateEntrySchema.Constants.TableName, 0 },
					{ RateLinesSchema.Constants.TableName, 0 },
					{ RatingHeaderSchema.Constants.TableName, 0 },
				};

			factory2.ResetDatabaseLoadCount();
			using (RowFactory.SetCachedTables())
			using (AssertDbHitsWithUsefulQueryInformation(hits, factory2, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				localTariffInFactory2.EntryCollectionValidator.Validate();
				var globalEntriesLoaded = globalTariffInFactory2.FetchAllEntriesFromLocalCache();
				AssertEquals("no global entries were loaded, since no local entry was published", 0, globalEntriesLoaded.Length);
			}
		}

		public void TestValidate_LocalRateIsPublished_DoesNotLoadAllGlobalEntries()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT", FlatCalculator.Code);
			globalChargeCode.Factory.Save();

			var localTariff = Factory.New<CompanyTariff>();
			CreateEntry(localTariff, 1, 10, "AUSYD", 1);

			var globalTariff = Factory.New<GlobalTariff>();
			CreateEntry(globalTariff, 2, 8, "AUSYD", 1);
			CreateEntry(globalTariff, 2, 8, "AUMEL", 1);
			CreateEntry(globalTariff, 2, 8, "AUBNE", 1);
			Factory.Save();
			AssertEquals("number of global entries", 3, globalTariff.FetchAllEntriesFromLocalCache().Length);

			// Publish a local rate entry that causes an overlap
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var localTariffInFactory2 = factory2.Load<CompanyTariff>(localTariff.PK);
			var entry2 = (RateEntry)localTariffInFactory2.AllEntriesCollection[0];
			entry2.IsPublished = true;
			var globalTariffInFactory2 = localTariffInFactory2.GlobalRatingHeader;
			AssertNotNull(globalTariffInFactory2);

			// Should be 1 hit on RateEntry to load the other half of the overlap
			var hits = new Dictionary<string, int> {
					{ RateEntrySchema.Constants.TableName, 1 },
					{ RateLinesSchema.Constants.TableName, 0 },
					{ RatingHeaderSchema.Constants.TableName, 0 },
				};

			factory2.ResetDatabaseLoadCount();
			using (RowFactory.SetCachedTables())
			using (AssertDbHitsWithUsefulQueryInformation(hits, factory2, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				localTariffInFactory2.EntryCollectionValidator.Validate();
				AssertEquals(false, globalTariffInFactory2.AllEntriesIsLoaded);
				var globalEntriesLoaded = globalTariffInFactory2.FetchAllEntriesFromLocalCache();
				AssertEquals("only the global entries with overlap are loaded", 2, globalEntriesLoaded.Length);
				AssertEquals(true, globalEntriesLoaded[0].HasRowErrors);
				AssertEquals(true, globalEntriesLoaded[1].HasRowErrors);
				AssertHasRowError(globalEntriesLoaded[0], ErrorMessages.OverlappingDatesOnRateEntry);
				AssertHasRowError(globalEntriesLoaded[1], ErrorMessages.OverlappingDatesOnRateEntry);
			}
		}

		public void TestValidate_EntriesWithoutLines_HasWarning()
		{
			var header = Factory.New<Costing>();
			var rates = new[]
			{
				CreateEntry(header, 1, 2, linesCount: 0),
				CreateEntry(header, 3, 4, linesCount: 2),
				CreateEntry(header, 5, 6, linesCount: 0),
				CreateEntry(header, 7, 8, linesCount: 1),
			};

			Factory.Save();
			header.EntryCollectionValidator.Validate();

			AssertHasRowWarning(rates[0], ErrorMessages.EntryHasNoLines);
			AssertNoRowWarnings(rates[1]);
			AssertHasRowWarning(rates[2], ErrorMessages.EntryHasNoLines);
			AssertNoRowWarnings(rates[3]);

			var unsavedEntryWithoutLines = CreateEntry(header, 10, 12);
			header.EntryCollectionValidator.Validate();

			AssertHasRowWarning("Expected warning regardless of entry being saved or not", unsavedEntryWithoutLines, ErrorMessages.EntryHasNoLines);
		}

		[TestDate(2016, 1, 1)]
		public void TestValidate_EntriesWithOverlaps_AddErrors()
		{
			var header = Factory.New<Costing>();
			var rates = new[]
			{
				// 1
				CreateEntry(header, 1, 10),				// -------------------
				CreateEntry(header, 1, 10),				// -------------------
				CreateEntry(header, 1, 10),				// -------------------
				CreateEntry(header, 1, 10, "AUSYD"),	// -------------------

				// 2
				CreateEntry(header, 20, 25),			// -------------------
				CreateEntry(header, 21, 26),			//		-------------------
				CreateEntry(header, 22, 27),			//				-------------------
				CreateEntry(header, 22, 27, "AUSYD"),	// -------------------

				// 3
				CreateEntry(header, 30, 35),			// -------------------
				CreateEntry(header, 32, 37),			//		-------------------
				CreateEntry(header, 30, 35),			// -------------------
				CreateEntry(header, 30, 35, "AUSYD"),	// -------------------

				// 4
				CreateEntry(header, 42, 47),			//		-------------------
				CreateEntry(header, 40, 45),			// -------------------
				CreateEntry(header, 42, 47),			//		-------------------
				CreateEntry(header, 42, 47, "AUSYD"),	// -------------------

				// 5
				CreateEntry(header, 52, 57),			//			-------------------
				CreateEntry(header, 51, 56),			//		-------------------
				CreateEntry(header, 50, 55),			// -------------------
				CreateEntry(header, 50, 55, "AUSYD"),	// -------------------

				// 6
				CreateEntry(header, 60, 68),			// -------------------
				CreateEntry(header, 62, 66),			//		--------
				CreateEntry(header, 60, 68),			// -------------------
				CreateEntry(header, 60, 68, "AUSYD"),	// -------------------

				// 7
				CreateEntry(header, 72, 76),			//		---------
				CreateEntry(header, 70, 78),			// -------------------
				CreateEntry(header, 72, 76),			//		---------
				CreateEntry(header, 72, 76, "AUSYD"),	// -------------------

				// 8
				CreateEntry(header, 80, 82),			// ------
				CreateEntry(header, 82, 84),			//		------
				CreateEntry(header, 84, 86),			//			 ------
				CreateEntry(header, 87, 89),			//					------
				CreateEntry(header, 84, 86, "AUSYD"),	//			 ------

				// 9
				CreateEntry(header, 95, null),			//			---------------...
				CreateEntry(header, 90, 92),			// ----
				CreateEntry(header, 94, 100),			//		  -------
				CreateEntry(header, 102, 110),			//					---------------
				CreateEntry(header, 90, 92, "AUSYD"),	// ----
			};

			header.EntryCollectionValidator.Validate();

			// 1
			AssertHasRowError(rates[0], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[1], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[2], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[3]);

			// 2
			AssertHasRowError(rates[4], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[5], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[6], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[7]);

			// 3
			AssertHasRowError(rates[8], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[9], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[10], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[11]);

			// 4
			AssertHasRowError(rates[12], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[13], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[14], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[15]);

			// 5
			AssertHasRowError(rates[16], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[17], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[18], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[19]);

			// 6
			AssertHasRowError(rates[20], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[21], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[22], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[23]);

			// 7
			AssertHasRowError(rates[24], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[25], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[26], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[27]);

			// 8
			AssertHasRowError(rates[28], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[29], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[30], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[31]);
			AssertNoRowErrors(rates[32]);

			// 9
			AssertHasRowError(rates[33], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[34]);
			AssertHasRowError(rates[35], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(rates[36], ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowErrors(rates[37]);
		}

		[TestDate(2017, 07, 14)]
		[UseSnapshotProtection]
		public void TestHandleOverlappingRateEntriesSaveException()
		{
			using (RunNonTransactioned())
			{
				var header = Factory.New<Costing>();
				var overlappingEntry1 = CreateEntry(header, 0, 60, "AUSYD");
				Factory.Save();

				var overlappingEntry2 = CreateEntry(header, 0, 60, "AUSYD");

				var duplicateSaveException = AssertExceptionThrown<ZSaveException>(() => Factory.Save());

				var dataException = duplicateSaveException.InnerException;
				var sqlException = dataException.InnerException as SqlException;

				var actual = RateEntryCollectionValidator.HandleOverlapException(header, sqlException);

				AssertMultilineASCIIEquals(ErrorMessages.OverlappingRatesCreatedByConcurrency, actual);
				AssertHasRowError(overlappingEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
				AssertHasRowError(overlappingEntry2, ErrorMessages.OverlappingDatesOnRateEntry);
				AssertNotNull("Issue Report only generated when data is valid but was rejected by trigger", ErrorReporter.LastMessageReported.IsNullOrEmpty());
			}
		}

		[TestDate(2017, 07, 14)]
		[UseSnapshotProtection]
		public void TestHandleOverlappingRateEntriesSaveException_HandlesInnerExceptions()
		{
			using (RunNonTransactioned())
			{
				var header = Factory.New<Costing>();
				var overlappingEntry1 = CreateEntry(header, 0, 60, "AUSYD");
				Factory.Save();

				var overlappingEntry2 = CreateEntry(header, 0, 60, "AUSYD");

				var zSaveException = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
				var actual = RateEntryCollectionValidator.HandleOverlapException(header, zSaveException);

				AssertMultilineASCIIEquals(ErrorMessages.OverlappingRatesCreatedByConcurrency, actual);

				AssertHasRowError(overlappingEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
				AssertHasRowError(overlappingEntry2, ErrorMessages.OverlappingDatesOnRateEntry);
				Assert("Issue Report only generated when data is valid but was rejected by trigger", ErrorReporter.LastMessageReported.IsNullOrEmpty());
			}
		}

		public void TestHandleOverlappingRateEntriesWithDifferentMatchingLocations()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);

			AssertEquals("Expected TI_FirstLoadLRC to be empty", ZString.Empty, entry1.TI_FirstLoadLRC);
			AssertEquals("Expected TI_LastDischargeLRC to be empty", ZString.Empty, entry1.TI_LastDischargeLRC);
			AssertEquals("Expected TI_FirstRouteSetLoadPortLRC to be empty", ZString.Empty, entry1.TI_FirstRouteSetLoadPortLRC);
			AssertEquals("Expected TI_LastRouteSetDischargePortLRC to be empty", ZString.Empty, entry1.TI_LastRouteSetDischargePortLRC);

			AssertEquals("Expected TI_FirstLoadLRC to be empty", ZString.Empty, entry2.TI_FirstLoadLRC);
			AssertEquals("Expected TI_LastDischargeLRC to be empty", ZString.Empty, entry2.TI_LastDischargeLRC);
			AssertEquals("Expected TI_FirstRouteSetLoadPortLRC to be empty", ZString.Empty, entry2.TI_FirstRouteSetLoadPortLRC);
			AssertEquals("Expected TI_LastRouteSetDischargePortLRC to be empty", ZString.Empty, entry2.TI_LastRouteSetDischargePortLRC);

			clientRate.RunPreSaveValidation();

			AssertHasRowError(entry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError(entry2, ErrorMessages.OverlappingDatesOnRateEntry);

			var rateEntryLocations1 = entry1.RateEntryLocations;
			var rateEntryLocations2 = entry2.RateEntryLocations;

			rateEntryLocations1.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, "AUSYD");
			rateEntryLocations1.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge, "AUBRN");

			rateEntryLocations2.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad, "AUSYD");
			rateEntryLocations2.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge, "AUBRN");

			clientRate.RunPreSaveValidation();

			AssertEquals("Expected TI_FirstLoadLRC to be 'AUSYD'", "AUSYD", entry1.TI_FirstLoadLRC);
			AssertEquals("Expected TI_LastDischargeLRC to be 'AUBRN'", "AUBRN", entry1.TI_LastDischargeLRC);

			AssertEquals("Expected TI_FirstRouteSetLoadPortLRC to be 'AUSYD'", "AUSYD", entry2.TI_FirstRouteSetLoadPortLRC);
			AssertEquals("Expected TI_LastRouteSetDischargePortLRC to be 'AUBRN'", "AUBRN", entry2.TI_LastRouteSetDischargePortLRC);

			AssertNoRowError(entry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertNoRowError(entry2, ErrorMessages.OverlappingDatesOnRateEntry);
		}

		[UseSnapshotProtection]
		public void TestHandleOverlappingRateEntriesSaveException_BulkRateUpdater()
		{
			using (RunNonTransactioned())
			{
				var header = Factory.New<Costing>();
				var overlappingEntry1 = CreateEntry(header, 0, 60, "AUSYD");
				var overlappingEntry2 = CreateEntry(header, 0, 60, "AUSYD");

				var duplicateSaveException = AssertExceptionThrown<ZSaveException>(() => Factory.Save());

				var entries = new SimpleRateEntryCollection(Factory) { overlappingEntry1, overlappingEntry2 };
				var actual = RateEntryCollectionValidator.HandleOverlapExceptionForBulkRateUpdater(duplicateSaveException, entries);

				var message = "Due to the constructor being the one used for BulkRateUpdater, we expect that error message";
				AssertMultilineASCIIEquals(message, ErrorMessages.OverlappingRatesCreatedByBulkRateUpdater, actual);
				Assert("But we don't report the exception as an issue for bulk rater updater", ErrorReporter.LastMessageReported.IsNullOrEmpty());
			}
		}

		[TestDate(2017, 07, 14)]
		[UseSnapshotProtection]
		public void TestHandleOverlappingRateEntriesSaveException_WithManyDuplicates()
		{
			using (RunNonTransactioned())
			{
				var header = Factory.New<Costing>();
				var entry1 = CreateEntry(header, 0, 60, "AUSYD");
				Factory.Save();
				for (int i = 0; i < 10; i++)
				{
					CreateEntry(header, 0, 60, "AUSYD");
				}

				var duplicateSaveException = AssertExceptionThrown<ZSaveException>(() => Factory.Save());

				var dataException = duplicateSaveException.InnerException;
				var sqlException = dataException.InnerException as SqlException;

				var actual = RateEntryCollectionValidator.HandleOverlapException(header, sqlException);
				AssertMultilineASCIIEquals(ErrorMessages.OverlappingRatesCreatedByConcurrency, actual);
				Assert("Issue Report only generated when data is valid but was rejected by trigger", ErrorReporter.LastMessageReported.IsNullOrEmpty());
				var errorCount = 0;
				foreach (var rateEntry in header.EntryCollections[entry1.TI_RateCategory].LazyLoadingCollection)
				{
					if (rateEntry.RowErrors.Any(x => x.Message.Contains(ErrorMessages.OverlappingDatesOnRateEntry)))
					{
						++errorCount;
					}
				}

				AssertEquals("if validation didn't add the row errors, the exception handler will add on all rows", 11, errorCount);
			}
		}

		public void TestValidate_EntriesWithErrorsHavingOverlaps_ShouldNotValidateForOverlapping()
		{
			var expectedError = "You have entered rates with an overlapping dates. Please enter rates without overlapping dates so the system can autorate using the correct rate.";

			var header = Helper.NewClientRate(NewClient);
			var entry1 = CreateEntry(header, 0, null);
			var line = entry1.AddFlatRateLine("FRT", 100.00);
			var entry2 = CreateEntry(header, 0, null);
			var entry3 = CreateEntry(header, 0, null);

			AssertEquals("Pre-condition: No error on RateLine", false, line.HasErrors);
			header.EntryCollectionValidator.Validate();
			AssertEquals("entry1 should have overlapping error as it is overlapping with entry2 & entry3.", true, entry1.RowErrors.Contains(expectedError));
			AssertEquals("entry2 should have overlapping error as it is overlapping with entry1 & entry3.", true, entry2.RowErrors.Contains(expectedError));
			AssertEquals("entry3 should have overlapping error as it is overlapping with entry1 & entry2.", true, entry3.RowErrors.Contains(expectedError));

			line.TL_RateCalculator = CalculatorConstants.Type.PER;
			entry2.TI_RateStartDate = ZDate.Invalid;

			AssertEquals("Pre-condition: Error on RateEntry.", true, entry2.HasErrorsNotIncludingChildren);
			AssertEquals("Pre-condition: Error on RateLine.", true, line.HasErrors);
			header.EntryCollectionValidator.Validate();
			AssertEquals("entry1 should have overlapping error as it is overlapping with entry3.", true, entry1.RowErrors.Contains(expectedError));
			AssertEquals("entry2 should not have overlapping error as it is excluded from overlapping validation as it has error.", false, entry2.RowErrors.Contains(expectedError));
			AssertEquals("entry3 should have overlapping error as it is overlapping with entry1.", true, entry3.RowErrors.Contains(expectedError));
		}

		public void TestValidate_ContainerTypeOrClassOverlap_AddErrors()
		{
			var expectedError = "You have entered rates with an overlapping dates. Please enter rates without overlapping dates so the system can autorate using the correct rate.";
			var container1 = Factory.NewWithValidTestData<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "Z2";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry1.TI_RC = container1.PK;
			entry2.TI_RC = container2.PK;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to match not ticked", false, entry1.RowErrors.Contains(expectedError));

			entry1.TI_MatchContainerRateClass = true;
			entry2.TI_MatchContainerRateClass = true;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different freight rate class", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to different freight rate class", false, entry2.RowErrors.Contains(expectedError));

			container2.RC_HandlingRateClass = "ZUB";
			entry2.Validation.ValidateTI_MatchContainerRateClass();
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different freight rate class", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to different freight rate class", false, entry2.RowErrors.Contains(expectedError));

			container2.RC_FreightRateClass = "ZUB";
			entry2.Validation.ValidateTI_MatchContainerRateClass();
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is duplicate due to same freight rate class", true, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is duplicate due to same freight rate class", true, entry2.RowErrors.Contains(expectedError));

			entry1.TI_MatchContainerRateClass = false;
			entry2.TI_MatchContainerRateClass = false;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to match not ticked", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to match not ticked", false, entry2.RowErrors.Contains(expectedError));

			entry1.TI_RC = ZGuid.Empty;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different containers", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to different containers", false, entry2.RowErrors.Contains(expectedError));
		}

		public void TestValidate_NotTickedContainerClass()
		{
			var expectedError = "You have entered rates with an overlapping dates. Please enter rates without overlapping dates so the system can autorate using the correct rate.";
			var container1 = Factory.NewWithValidTestData<RefContainer>();
			container1.RC_FreightRateClass = "20GN";
			container1.RC_Code = "20GP";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_FreightRateClass = "40GN";
			container2.RC_Code = "40GP";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");

			entry1.TI_RateStartDate = ZDate.Today;
			entry1.TI_RateEndDate = ZDate.Today.AddDays(5);
			entry2.TI_RateStartDate = ZDate.Today.AddDays(2);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(6);

			entry1.TI_RC = container1.PK;
			entry2.TI_RC = container2.PK;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different container type", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to different container type", false, entry2.RowErrors.Contains(expectedError));

			entry2.TI_RC = container1.PK;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is duplicate due to same container type and overlapped", true, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is duplicate due to same container type and overlapped", true, entry2.RowErrors.Contains(expectedError));

			entry1.TI_RateStartDate = ZDate.Today;
			entry1.TI_RateEndDate = ZDate.Today;
			entry2.TI_RateStartDate = ZDate.Today.AddDays(1);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(2);
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to same container but not overlapped", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to same container but not overlapped", false, entry2.RowErrors.Contains(expectedError));
		}

		public void TestValidate_TickedContainerClass()
		{
			var expectedError = "You have entered rates with an overlapping dates. Please enter rates without overlapping dates so the system can autorate using the correct rate.";
			var container1 = Factory.NewWithValidTestData<RefContainer>();
			container1.RC_FreightRateClass = "20GN";
			container1.RC_Code = "20GP";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_FreightRateClass = "40GN";
			container2.RC_Code = "40GP";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry1.TI_MatchContainerRateClass = true;
			entry2.TI_MatchContainerRateClass = true;

			entry1.TI_RC = container1.PK;
			entry2.TI_RC = container2.PK;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different container type and container class", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to different container type and container class", false, entry2.RowErrors.Contains(expectedError));

			entry2.TI_RC = container1.PK;
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is duplicate due to same container class", true, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is duplicate due to same container class", true, entry2.RowErrors.Contains(expectedError));

			entry2.TI_RC = container2.PK;
			container2.RC_FreightRateClass = "20GN";
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is duplicate due to same container class", true, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is duplicate due to same container class", true, entry2.RowErrors.Contains(expectedError));

			entry1.TI_RateStartDate = ZDate.Today;
			entry1.TI_RateEndDate = ZDate.Today;
			entry2.TI_RateStartDate = ZDate.Today.AddDays(1);
			entry2.TI_RateEndDate = ZDate.Today.AddDays(2);
			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to same container class but not overlapped", false, entry1.RowErrors.Contains(expectedError));
			AssertEquals("Is not duplicate due to same container class but not overlapped", false, entry2.RowErrors.Contains(expectedError));
		}

		public void TestGetOverlappingRateEntries_ShouldConsiderMatchContainerRateClass_DifferentContainersType()
		{
			var containerA = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			containerA.RC_FreightRateClass = "20GN";
			var containerB = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			containerB.RC_FreightRateClass = "40GN";

			var costing = Factory.New<Costing>();
			var entryA = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entryA.TI_RateStartDate = ZDate.Today;
			entryA.TI_RC = containerA.PK;
			entryA.TI_MatchContainerRateClass = true;

			var entryB = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entryB.TI_RateStartDate = ZDate.Today;
			entryB.TI_RC = containerA.PK;
			entryB.TI_MatchContainerRateClass = false;

			costing.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different container rate class ", false, entryA.RowErrors.HasErrors());
			AssertEquals("Is not duplicate due to different container rate class", false, entryB.RowErrors.HasErrors());
			AssertNoExceptionThrown("It's not duplicate and there should not be an exception since TI_MatchContainerRateClass and ContainerClass are considered in GetOverlappingRateEntries", () => Factory.Save());

			entryB.TI_RC = containerB.PK;
			entryB.TI_MatchContainerRateClass = true;
			costing.EntryCollectionValidator.Validate();
			AssertEquals("Is not duplicate due to different container rate class", false, entryA.RowErrors.HasErrors());
			AssertEquals("Is not duplicate due to different container rate class", false, entryB.RowErrors.HasErrors());
			AssertNoExceptionThrown("It's not duplicate and there should not be an exception since TI_MatchContainerRateClass and ContainerClass are considered in GetOverlappingRateEntries", () => Factory.Save());
		}

		public void TestGetOverlappingRateEntries_ShouldConsiderContainerRateClass_SameContainerClass()
		{
			var containerA = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			containerA.RC_FreightRateClass = "20GN";
			var containerB = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			containerB.RC_FreightRateClass = "20GN";

			var costing = Factory.New<Costing>();

			var entryA = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entryA.TI_RateStartDate = ZDate.Today;
			entryA.TI_RC = containerA.PK;
			entryA.TI_MatchContainerRateClass = true;

			var entryB = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entryB.TI_RateStartDate = ZDate.Today;
			entryB.TI_RC = containerB.PK;
			entryB.TI_MatchContainerRateClass = true;

			costing.EntryCollectionValidator.Validate();
			AssertEquals("Duplicated due to different container type and container class", true, entryA.RowErrors.HasErrors());
			AssertEquals("Duplicated due to different container type but same container class", true, entryB.RowErrors.HasErrors());
			AssertExceptionThrown<ZSaveException>("It's duplicated and so There should be an exception thrown", () => Factory.Save());
		}

		public void TestGetOverlappingRateEntries_MatchContainerRateClass_DuplicatedForCategoryCYD()
		{
			var container20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			container20GP.RC_HandlingRateClass = "C20";
			var container40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			container40GP.RC_HandlingRateClass = "C40";

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entryCYD1 = clientRate.AddRateEntry(RatingConstants.RateCategory.CYD, startDate: ZDate.Today, container: "20GP", unitType: "CNT");
			var entryCYD2 = clientRate.AddRateEntry(RatingConstants.RateCategory.CYD, startDate: ZDate.Today, container: "40GP", matchContainerRateClass: true, unitType: "CNT");
			var entryFCL1 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, startDate: ZDate.Today, container: "20GP", matchContainerRateClass: true);
			var entryFCL2 = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL, startDate: ZDate.Today, container: "20GP");

			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Not duplicated with another entry", false, entryCYD1.RowErrors.HasErrors());
			AssertEquals("Not duplicated with another entry", false, entryCYD2.RowErrors.HasErrors());
			AssertEquals("Not duplicated with another entry", false, entryFCL1.RowErrors.HasErrors());
			AssertEquals("Not duplicated with another entry", false, entryFCL2.RowErrors.HasErrors());
			AssertNoExceptionThrown("It's not duplicated and so There should be no exception thrown", () => Factory.Save());

			var entryCYD3 = clientRate.AddRateEntry(RatingConstants.RateCategory.CYD, startDate: ZDate.Today, container: "20GP", matchContainerRateClass: true, unitType: "CNT");

			clientRate.EntryCollectionValidator.Validate();
			AssertEquals("Duplicated to entryCYD3 as both have same container type", true, entryCYD1.RowErrors.HasErrors());
			AssertEquals("Not duplicated with another entry", false, entryCYD2.RowErrors.HasErrors());
			AssertEquals("Duplicated to entryCYD1 as both have same container type", true, entryCYD3.RowErrors.HasErrors());
			AssertEquals("Not duplicated with another entry", false, entryFCL1.RowErrors.HasErrors());
			AssertEquals("Not duplicated with another entry", false, entryFCL2.RowErrors.HasErrors());
			AssertExceptionThrown<ZSaveException>("It's duplicated and so There should be an exception thrown", () => Factory.Save());
		}

		public void TestCYEntry_ContainerTypeHasError_WhenUnitTypeIsEmpty()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entryCYDWithOutYardUnitType = clientRate.AddRateEntry(RatingConstants.RateCategory.CYD, startDate: ZDate.Today, container: "20GP");
			Assert("The type size is not allowed when Unit Type is left blank.", entryCYDWithOutYardUnitType.TI_RCInfo.HasErrors());

			var entryCYDWithYardUnitType = clientRate.AddRateEntry(RatingConstants.RateCategory.CYD, startDate: ZDate.Today, container: "20GP", unitType: "CNT");
			Assert("The type size is allowed when Unit Type is not blank.", !entryCYDWithYardUnitType.TI_RCInfo.HasErrors());
		}

		public void TestGetOverlappingRateEntries()
		{
			var costing = Factory.New<Costing>();

			#region Setup Data

			/*
							origin Mode  ContainerType IsContainerClassMatch		StartData		duplicate
			entry 0:AUSYD		Sea			20GP							false								today			no
			entry 1:AUSYD		Sea			40GP							false								today			no
			entry 2:AUSYD		Sea			20GP							true								today			yes(with entry 3):although entry 3 container type is different but IsContainerClassMatch is true and both have the same charge rate class(40GN)
			entry 3:AUSYD		Sea			40GP							true								today			yes(with entry 2):although entry 2 container type is different but IsContainerClassMatch is true and both have the same charge rate class(40GN)
			entry 4:AUSYD		Sea			20T8							true								today			no:no duplicated due to different container class(20GN) with entry rate 2 and 3(40GN)
			entry 5:AUSYD		Sea			20T8							false								today			no:no duplicated even container type is the same with rate entry 4 but match container rate class is not selected
			entry 6:AUSYD		Sea			40T8							false								today			yes(with entry 7):same container type with entry rate 7(40T8)
			entry 7:AUSYD		Sea			40T8							false								today			yes(with entry 6):same container type with entry rate 6(40T8)
			*/
			var container0 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			container0.RC_FreightRateClass = "40GN";

			var container1 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			container1.RC_FreightRateClass = "40GN";

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "20T8";
			container2.RC_FreightRateClass = "20GN";

			var container3 = Factory.NewWithValidTestData<RefContainer>();
			container3.RC_Code = "40T8";

			var entry0 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry0.TI_RateStartDate = ZDate.Today;
			entry0.TI_RC = container0.PK;

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry1.TI_RateStartDate = ZDate.Today;
			entry1.TI_RC = container1.PK;

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry2.TI_RateStartDate = ZDate.Today;
			entry2.TI_RC = container0.PK;
			entry2.TI_MatchContainerRateClass = true;

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry3.TI_RateStartDate = ZDate.Today;
			entry3.TI_RC = container1.PK;
			entry3.TI_MatchContainerRateClass = true;

			var entry4 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry4.TI_RateStartDate = ZDate.Today;
			entry4.TI_RC = container2.PK;
			entry4.TI_MatchContainerRateClass = true;

			var entry5 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry5.TI_RateStartDate = ZDate.Today;
			entry5.TI_RC = container2.PK;

			var entry6 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry6.TI_RateStartDate = ZDate.Today;
			entry6.TI_RC = container3.PK;

			var entry7 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "");
			entry7.TI_RateStartDate = ZDate.Today;
			entry7.TI_RC = container3.PK;

			#endregion

			costing.EntryCollectionValidator.Validate();

			CombineAssertions(() =>
			{
				AssertEquals("Rate Entry 0 : No Duplicated due to different container type", false, entry0.RowErrors.HasErrors());
				AssertEquals("Rate Entry 1 : No Duplicated due to different container type", false, entry1.RowErrors.HasErrors());
				AssertEquals("Rate Entry 2 : Duplicated due to different container type but same container class with entry rate 3", true, entry2.RowErrors.HasErrors());
				AssertEquals("Rate Entry 3 : Duplicated due to different container type but same container class with entry rate 2", true, entry3.RowErrors.HasErrors());
				AssertEquals("Rate Entry 4 : No Duplicated due to different container class with entry rate 2 and 3", false, entry4.RowErrors.HasErrors());
				AssertEquals("Rate Entry 5 : No Duplicated even container type is the same with rate entry 4 but match container rate class is not selected", false, entry5.RowErrors.HasErrors());
				AssertEquals("Rate Entry 6 : Duplicated due to same container type with entry rate 7", true, entry6.RowErrors.HasErrors());
				AssertEquals("Rate Entry 7 : Duplicated due to same container type with entry rate 6", true, entry7.RowErrors.HasErrors());

				AssertExceptionThrown<ZSaveException>("It's duplicated and so There should be an exception thrown", () => Factory.Save());
			});
		}

		public void TestValidate_IdenticalRatesWithDifferentContractNumber_CanBeSaved()
		{
			var header = Factory.New<Costing>();
			var entry1 = CreateEntry(header, 0, 60, "AUSYD");
			entry1.TI_ContractNumber = "A12345";

			var entry2 = CreateEntry(header, 0, 60, "AUSYD");
			entry2.TI_ContractNumber = "B67890";

			header.EntryCollectionValidator.Validate();
			AssertEquals(false, entry1.RowErrors.HasErrors());
			AssertEquals(false, entry2.RowErrors.HasErrors());
			AssertNoExceptionThrown(() => Factory.Save());

			entry2.TI_ContractNumber = "A12345";
			header.EntryCollectionValidator.Validate();
			AssertEquals("Rates are duplicated (same contract number)", true, entry1.RowErrors.HasErrors());
			AssertEquals("Rates are duplicated (same contract number)", true, entry2.RowErrors.HasErrors());
			AssertExceptionThrown<ZSaveException>("It's duplicated and so There should be an exception thrown", () => Factory.Save());
		}

		public void TestValidate_IdenticalRatesWithDifferentContractNumberLinked_CannotBeSaved()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader("CODE1"));
			var firstRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			firstRateEntry.TI_ContractNumber = "C12345";
			var secondRateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
			secondRateEntry.TI_ContractNumber = "C12345";

			AssertEquals("Precondition", true, firstRateEntry.IsDuplicate(secondRateEntry));

			secondRateEntry.TI_ContractNumberLinked = true;
			AssertEquals("ContractNumberLink-age should not affect duplicate check", true, firstRateEntry.IsDuplicate(secondRateEntry));

			AssertExceptionThrown(typeof(ZSaveException), Factory.Save);
		}

		public void TestValidate_IdenticalRatesWithDifferentSystemConsolidationStatus_CanBeSaved()
		{
			var header = Factory.New<Costing>();
			var entry1 = CreateEntry(header, 0, 60, "AUSYD");
			entry1.TI_ShipmentConsolidationStatus = "STS";

			var entry2 = CreateEntry(header, 0, 60, "AUSYD");
			entry2.TI_ShipmentConsolidationStatus = "CNS";

			header.EntryCollectionValidator.Validate();
			AssertEquals(false, entry1.RowErrors.HasErrors());
			AssertEquals(false, entry2.RowErrors.HasErrors());
			AssertNoExceptionThrown(() => Factory.Save());

			entry2.TI_ShipmentConsolidationStatus = "STS";
			header.EntryCollectionValidator.Validate();
			AssertEquals("Rates are duplicated (same shipment consolidation status)", true, entry1.RowErrors.HasErrors());
			AssertEquals("Rates are duplicated (same shipment consolidation status)", true, entry2.RowErrors.HasErrors());
			AssertExceptionThrown<ZSaveException>("It's duplicated and so There should be an exception thrown", () => Factory.Save());
		}

		public void TestValidate_IdenticalRatesWithDifferentHBLDeliveryMode_CanBeSaved()
		{
			var header = Factory.New<ClientRate>();
			header.TH_OH = NewClient.PK;

			var entry1 = CreateEntry(header, 0, 60, "AUSYD");
			entry1.TI_HBLDeliveryMode = "CFS/CFS";

			var entry2 = CreateEntry(header, 0, 60, "AUSYD");
			entry2.TI_HBLDeliveryMode = "DOOR/DOOR";

			header.EntryCollectionValidator.Validate();
			AssertEquals(false, entry1.RowErrors.HasErrors());
			AssertEquals(false, entry2.RowErrors.HasErrors());
			AssertNoExceptionThrown(() => Factory.Save());

			entry2.TI_HBLDeliveryMode = "CFS/CFS";
			header.EntryCollectionValidator.Validate();
			AssertEquals("Rates are duplicated (same HBL Delivery Mode)", true, entry1.RowErrors.HasErrors());
			AssertEquals("Rates are duplicated (same HBL Delivery Mode)", true, entry2.RowErrors.HasErrors());
			AssertExceptionThrown<ZSaveException>("It's duplicated and so There should be an exception thrown", () => Factory.Save());
		}

		public void TestValidate_PublishedEntryHavingNoExpiryDate_CanBeSaved()
		{
			var header = Helper.NewClientRate(NewClient);
			var entry = CreateEntry(header, 0, null, "AUSYD");
			entry.TI_RateEndDate = ZDate.Empty;
			entry.IsPublished = true;

			AssertNoExceptionThrown("Empty date should be checked and should not cause issue while verifying and publishing entry to global rate", () =>
			{
				header.RunPreSaveValidation();
			});

			Factory.Save();

			using (var command = Db.Connection.Command($"SELECT {RateEntrySchema.Constants.TI_RateEndDate} FROM {RateEntrySchema.Constants.SqlSchemaName}.{RateEntrySchema.Constants.TableName}"))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals(DBNull.Value, reader.GetValue(0));
			}
		}

		RateEntry CreateEntry(RatingHeader header, int fromDay, int? toDay, string origin = "UAIEV", int linesCount = 0)
		{
			var entry = header.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, origin, "");
			entry.TI_RateStartDate = ZDate.Today.AddDays(fromDay);

			if (toDay.HasValue)
			{
				entry.TI_RateEndDate = ZDate.Today.AddDays(toDay.Value);
			}

			if (linesCount > 0)
			{
				for (var i = 0; i < linesCount; i++)
				{
					entry.AddRateLine("FRT", UnitCalculator.Code, Core.Constants.Weight.Kilograms);
				}
			}
			else
			{
				entry.RateLines.RemoveAndDeleteAll();
			}

			return entry;
		}
	}
}
