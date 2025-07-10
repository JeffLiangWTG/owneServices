using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[UseSnapshotProtection]
	sealed class RateDataImporterTest : TestCase
	{
		public void TestIsActive()
		{
			RateDataImporter rateDataImporter = RateDataImporter.New(Factory.New<CompanyTariff>());
			AssertEquals(false, rateDataImporter.IsActive);

			rateDataImporter = RateDataImporter.New(Factory.New<ClientRate>());
			AssertEquals(false, rateDataImporter.IsActive);

			rateDataImporter = RateDataImporter.New(Factory.New<Quote>());
			AssertEquals(false, rateDataImporter.IsActive);

			rateDataImporter = RateDataImporter.New(Factory.New<Costing>());
			AssertEquals(true, rateDataImporter.IsActive);
		}

		public void TestEnableIATARateImport()
		{
			var rateDataImporter = RateDataImporter.New(Factory.New<CompanyTariff>());
			AssertEquals(false, rateDataImporter.EnableIATARateImport);

			rateDataImporter = RateDataImporter.New(Factory.New<ClientRate>());
			AssertEquals(false, rateDataImporter.EnableIATARateImport);

			rateDataImporter = RateDataImporter.New(Factory.New<Quote>());
			AssertEquals(false, rateDataImporter.EnableIATARateImport);

			var cost = Factory.New<Costing>();
			rateDataImporter = RateDataImporter.New(cost);
			AssertEquals(true, rateDataImporter.EnableIATARateImport);

			cost.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			AssertEquals(false, rateDataImporter.EnableIATARateImport);
		}

		void OnImportCompleted(ZGuid costingPK, int expectedRateEntriesCount)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory = new BusinessObjectFactory();
				var costing = factory.Load<RatingHeader>(costingPK);

				AssertEquals("RateEntry count", expectedRateEntriesCount, costing.AIRRateEntriesForBinding.Count);

				var expected = new[]
				{
					new { Reference = "", Event = (ZString)Events.AddedARecordToTheSystem.Description, Table = RatingHeaderSchema.Constants.TableName },
					new { Reference = ZString.Format("{0} TACT Rate Entries were added", expectedRateEntriesCount).ToString(), Event = (ZString)Events.EditedARecord.Description, Table = RatingHeaderSchema.Constants.TableName }
				};

				var actual = new StmALogCollectionView(costing).Cast<StmALog>().Select(x => new { Reference = (string)x.SL_Reference, Event = x.SL_EventDescription, Table = (string)x.SL_Table })
					.ToList();
				var comparer = actual.CreateComparerForElements((x, y) => x.Event == y.Event && x.Reference == y.Reference && x.Table == y.Table, x => HashCodeHelper.GetCompositeHashCode(new object[] { x.Event, x.Reference, x.Table }));

				AssertContainsExactElementsInAnyOrder(comparer, expected, actual);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2010, 4, 10)]
		[ExpectNoExceptions]
		public void TestImport_IATA_TACT_80CharactersFormat()
		{
			AssertImportTactRatesHasNoErrors(BaseSourcePath + @"Enterprise\Product\Operations\Rating\Rating.DataTransfer.Test\TACT\Testing\Test80CharTACT.146", 4);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 7, 25)]
		[ExpectNoExceptions]
		public void TestImport_IATA_TACT_150CharactersFormat()
		{
			AssertImportTactRatesHasNoErrors(BaseSourcePath + @"Enterprise\Product\Operations\Rating\Rating.DataTransfer.Test\TACT\Testing\Test150CharTACT.054", 9);
		}

		public void TestImportTACT_80CharactersFileAndExtensionIsUnknown_DetermineFromTACTLineLength()
		{
			var realFilePath = TempForTest.GetTempFileName();

			File.WriteAllText(realFilePath, @"
GC550  ASYDAUBODFR                    00250KAUD200000050019991001        55837 A
GC550  ASYDAUBODFR                    00500KAUD200000038519991001        55837 A
GC550  ASYDAUBODFR                    01000KAUD200000035019991001        55837 A
GS550  ASYDAULAXUS                    00001KAUD200000139519990405        74076 A
GS550  ASYDAULAXUS                    00045KAUD200000066019990405        74076 A
GS550  ASYDAULAXUS                    00100KAUD200000040019990405        74076 A");

			var stream = new FileStream(realFilePath, FileMode.Open, FileAccess.Read);

			try
			{
				var tcs = new TaskCompletionSource<bool>();
				var testCosting = Factory.New<Costing>();

				Factory.Save();

				var rateDataImporter = RateDataImporter.New(testCosting);
				rateDataImporter.ImportCompletedHandler = () => tcs.TrySetResult(true);

				ThreadPool.QueueUserWorkItem(_ => rateDataImporter.ImportIATA_TACT("c:\\test.xxx", stream));        // It is just a string, file is not being created

				tcs.Task.GetAwaiter().GetResult();

				OnImportCompleted(testCosting.PK, 2);
			}
			finally
			{
				stream.Dispose();
				File.Delete(realFilePath);
			}
		}

		public void TestImportTACT_150CharactersFileAndExtensionIsUnknown_DetermineFromTACTLineLength()
		{
			var realFilePath = TempForTest.GetTempFileName();

			File.WriteAllText(realFilePath, @"
GS5B   CAR  N29870FRADE36630HKGHK                       00045K                                 EUR20000009002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK                       00100K                                 EUR20000006002019090920190909        A          AY  A  
MC5B   CAR  N29870FRADE36630HKGHK                       00000K                                 EUR20000130002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK                       00001K                                 EUR20000005602016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630SINSG                       00100K                                 EUR20000004352016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630SINSG                       00300K                                 EUR20000004182016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630SINSG                       00500K                                 EUR20000004022016010120160101        A          BA  A  ");

			var stream = new FileStream(realFilePath, FileMode.Open, FileAccess.Read);

			try
			{
				var tcs = new TaskCompletionSource<bool>();
				var testCosting = Factory.New<Costing>();

				Factory.Save();

				var rateDataImporter = RateDataImporter.New(testCosting);
				rateDataImporter.ImportCompletedHandler = () => tcs.TrySetResult(true);

				ThreadPool.QueueUserWorkItem(_ => rateDataImporter.ImportIATA_TACT("c:\\test.xxx", stream));        // It is just a string, file is not being created

				tcs.Task.GetAwaiter().GetResult();

				OnImportCompleted(testCosting.PK, 2);
			}
			finally
			{
				stream.Dispose();
				File.Delete(realFilePath);
			}
		}

		public void TestImportTACT_FileIsCorrupted_150_Beginning_ShowError()
		{
			var data = @"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
GS5B   CAR  N29870FRADE36630HKGHK    AY                 00001K                                 EUR20000012002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK    AY                 00045K                                 EUR20000009002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK    AY                 00100K                                 EUR20000006002019090920190909        A          AY  A  
MC5B   CAR  N29870FRADE36630HKGHK    AY                 00000K                                 EUR20000130002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00001K                                 EUR20000005602016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00100K                                 EUR20000004352016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00300K                                 EUR20000004182016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00500K                                 EUR20000004022016010120160101        A          BA  A  ";

			AssertImportTactRatesHasErrors("c:\\test.054", data, "Canceled");      // It is just a string, file is not being created
		}

		public void TestImportTACT_FileIsCorrupted_150_End_ShowError()
		{
			var data = @"GS5B   CAR  N29870FRADE36630HKGHK    AY                 00001K                                 EUR20000012002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK    AY                 00045K                                 EUR20000009002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK    AY                 00100K                                 EUR20000006002019090920190909        A          AY  A  
MC5B   CAR  N29870FRADE36630HKGHK    AY                 00000K                                 EUR20000130002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00001K                                 EUR20000005602016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00100K                                 EUR20000004352016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00300K                                 EUR20000004182016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630HKGHK    BA                 00500K                                 EUR20000004022016010120160101        A          BA  A  
MC5B   CAR  N29870FRADE36630HKGHK    BA                 00000K         ..........,";

			AssertImportTactRatesHasErrors("c:\\test.054", data, "Canceled");      // It is just a string, file is not being created
		}

		public void TestImportTACT_FileIsCorrupted_80_Beginning_ShowError()
		{
			var data = @"AAAAAAAAAAAAAAAAAAAAAAAA
GC550  ASYDAUBODFR                    00001KAUD200000145519990405        55068 A
GC550  ASYDAUBODFR                    00045KAUD200000112519991001        55837 A
GC550  ASYDAUBODFR                    00100KAUD200000077519991001        55837 A
GC550  ASYDAUBODFR                    00250KAUD200000050019991001        55837 A
GC550  ASYDAUBODFR                    00500KAUD200000038519991001        55837 A";

			AssertImportTactRatesHasErrors("c:\\test.146", data, "Canceled");      // It is just a string, file is not being created
		}

		public void TestImportTACT_FileIsCorrupted_80_End_ShowError()
		{
			var data = @"GC550  ASYDAUBODFR                    00001KAUD200000145519990405        55068 A
GC550  ASYDAUBODFR                    00045KAUD200000112519991001        55837 A
GC550  ASYDAUBODFR                    00100KAUD200000077519991001        55837 A
GC550  ASYDAUBODFR                    00250KAUD200000050019991001        55837 A
GC550  ASYDAUBODFR                    00500KAUD200000038519991001        55837 A
GC550  ASYDAUBODFR                    01000KAUD200000035019991...........";

			AssertImportTactRatesHasErrors("c:\\test.146", data, "Canceled");      // It is just a string, file is not being created
		}

		void AssertImportTactRatesHasNoErrors(string filePath, int expectedRateEntriesCreated)
		{
			var tcs = new TaskCompletionSource<bool>();
			var testCosting = Factory.New<Costing>();

			Factory.Save();

			var rateDataImporter = RateDataImporter.New(testCosting);
			rateDataImporter.ImportCompletedHandler = () =>
			{
				try
				{
					OnImportCompleted(testCosting.PK, expectedRateEntriesCreated);
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			};

			ThreadPool.QueueUserWorkItem(_ => rateDataImporter.ImportIATA_TACT(filePath, new FileStream(filePath, FileMode.Open, FileAccess.Read)));

			tcs.Task.GetAwaiter().GetResult();

			AssertNotContains(RateDataImporter.ImportFailedPhrase, rateDataImporter.ImportReport);
		}

		void AssertImportTactRatesHasErrors(string filePath, string fileData, string expectedError)
		{
			var realFilePath = TempForTest.GetTempFileName();

			File.WriteAllText(realFilePath, fileData);
			var stream = new FileStream(realFilePath, FileMode.Open, FileAccess.Read);

			try
			{
				var tcs = new TaskCompletionSource<bool>();
				var testCosting = Factory.New<Costing>();

				Factory.Save();

				var rateDataImporter = RateDataImporter.New(testCosting);
				rateDataImporter.ImportCompletedHandler = () => tcs.TrySetResult(true);

				ThreadPool.QueueUserWorkItem(_ => rateDataImporter.ImportIATA_TACT(filePath, stream));
				tcs.Task.GetAwaiter().GetResult();

				AssertContains(expectedError, rateDataImporter.ImportReport);
			}
			finally
			{
				stream.Dispose();
				File.Delete(realFilePath);
			}
		}

		[ExpectNoExceptions]
		public void TestImport_ForwardAir()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var testCsvStream = resourceRetriever.GetStream("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.Test.csv"))
			{
				var rateDataImporter = RateDataImporter.New(Factory.New<Costing>());
				var testCsvPath = resourceRetriever.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataImporter.ForwardAir.TestFiles.Test.csv", "Test.csv");
				rateDataImporter.ImportForwardAirRates(testCsvPath, testCsvStream);
			}
		}

		public void TestImport_RoundingValidation()
		{
			var rateDataImporter = RateDataImporter.New(Factory.New<Costing>());
			rateDataImporter.Rounding = "SSS";
			AssertEquals("Rounding is invalid", true, rateDataImporter.RoundingInfo.HasErrors());

			rateDataImporter.Rounding = RatingRoundingTypes.Chargeable;
			AssertEquals("Rounding is valid", false, rateDataImporter.RoundingInfo.HasErrors());
		}

		public void TestRegisterConstructorOverride()
		{
			AssertEquals(typeof(RateDataImporter), RateDataImporter.New(Factory.New<CompanyTariff>()).GetType());

			RateDataImporter.RegisterType(typeof(RateDataImporter));
			AssertEquals(typeof(RateDataImporter), RateDataImporter.New(Factory.New<CompanyTariff>()).GetType());
		}

		public void TestIsDefaultFreightChargeCodeValid()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testCosting = Factory.NewWithValidTestData<Costing>();
			testCosting.TH_OH = testOrg.PK;
			Factory.Save();

			ZQuery query = new ZQuery();
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
			var chargeCode = Factory.LoadTop1<AccChargeCode>(query);

			chargeCode.AC_IsActive = false;
			Factory.Save();
			Env.Registry.FreightChargeCode = Guid.Empty;

			RateDataImporter rateDataImporter = RateDataImporter.New(testCosting);
			AssertEquals(false, rateDataImporter.IsDefaultFreightChargeCodeValid);

			chargeCode.AC_IsActive = true;
			Factory.Save();
			Env.Registry.FreightChargeCode = chargeCode.PK.ToGuid();
			AssertEquals(true, rateDataImporter.IsDefaultFreightChargeCodeValid);
		}

		#region Sort Entries With Error

		public void TestSortEntriesWithError()
		{
			var cost = Factory.NewWithValidTestData<Costing>();
			RateEntry entry1 = cost.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			RateEntry entry2 = cost.AddRateEntry("AIR", "LSE", "YYYYY", "USLAX");
			RateEntry entry3 = cost.AddRateEntry("AIR", "LSE", "AUMEL", "NZAKL");
			RateEntry entry4 = cost.AddRateEntry("AIR", "LSE", "AUMEL", "NNNNN");

			RateDataImporter importer = RateDataImporter.New(cost);
			cost.RunPreSaveValidation();
			Assert(!entry1.HasErrors);
			Assert(entry2.HasErrors);
			Assert(!entry3.HasErrors);
			Assert(entry4.HasErrors);

			importer.SortEntriesWithError();
			AssertEquals(entry4, cost.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection[0]);
			AssertEquals(entry2, cost.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection[1]);
			AssertEquals(entry3, cost.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection[2]);
			AssertEquals(entry1, cost.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection[3]);
		}

		#endregion

		public void TestImportTACT_ActionCodeIsDelete()
		{
			const string data =
@"GC1A   550  A60650BNAUS38760IQTPE                       00001K                                 USD20000008472020030120200301        AATL           D  
GC1A   550  A60650BNAUS38760IQTPE                       00001K                                 USD20000008472020040120200401        AATL           A  
GC1A   550  A60650BNAUS38760IQTPE                       00045K                                 USD20000006642020030120200301        AATL           D  
GC1A   550  A60650BNAUS38760IQTPE                       00045K                                 USD20000006642020040120200401        AATL           A  
GC1A   550  A60650BNAUS38760IQTPE                       00300K                                 USD20000005172020030120200301        AATL           D  
GC1A   550  A60650BNAUS38760IQTPE                       00300K                                 USD20000005172020040120200401        AATL           A  
MC1A   501  A60650BNAUS38760IQTPE                       00000K                                 USD20000085002020040120200401        AATL           A  ";

			var stream = new MemoryStream(Encoding.ASCII.GetBytes(data));

			var tcs = new TaskCompletionSource<bool>();
			var testCosting = Factory.New<Costing>();
			int expectedRateEntriesCreated = 1;

			Factory.Save();

			var rateDataImporter = RateDataImporter.New(testCosting);
			rateDataImporter.ImportCompletedHandler = () =>
			{
				try
				{
					OnImportCompleted(testCosting.PK, expectedRateEntriesCreated);
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			};

			ThreadPool.QueueUserWorkItem(_ => rateDataImporter.ImportIATA_TACT("IF040Y.054", stream));

			tcs.Task.GetAwaiter().GetResult();

			AssertNotContains(RateDataImporter.ImportFailedPhrase, rateDataImporter.ImportReport);
			AssertContains("4 rates have been successfully imported", rateDataImporter.ImportReport);
			AssertContains("3 rates have been skipped as no longer valid - file is a difference file with records from the previous month", rateDataImporter.ImportReport);
		}

		#region Implementation

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}

		BusinessObjectFactory factory;

		#endregion
	}

	[TestedType(typeof(RateDataImporter))]
	sealed class RateDataImporterMandatoryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return RateDataImporter.New(Factory.New<Costing>());
		}
	}
}
