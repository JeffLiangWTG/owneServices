using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGCountryReferenceManyToManyCollection<UNDGSubstance>))]
	public class CountryReferenceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.Factory.Save();
			return new UNDGCountryReferenceManyToManyCollection<UNDGSubstance>(substance);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<UNDGCountryReference>();
		}
	}

	class UNDGCountryReferenceManyToManyCollectionTest : TestCaseWithFactory
	{
		public void TestCollectionFunctionality()
		{
			var subs1 = DGSubstanceTestHelper.Create("0000", "A", UNDGSubstanceStandardTypes.IATA);
			var countryRef = Factory.NewWithValidTestData<UNDGCountryReference>();
			Factory.Save();
			{
				var collection = new UNDGCountryReferenceManyToManyCollection<UNDGSubstance>(Factory.Load<UNDGSubstance>(subs1.PK));
				collection.Add(countryRef);
				var pivot = Factory.LoadTop1<UNDGCountryReferencePivot>(new ZQuery());
				CombineAssertions("collection can be added to", () =>
				{
					AssertEquals(1, collection.Count);
					AssertNotNull(pivot);
				});
				Factory.Save();
			}

			{
				var factory = new BusinessObjectFactory();
				var collection = new UNDGCountryReferenceManyToManyCollection<UNDGSubstance>(factory.Load<UNDGSubstance>(subs1.PK));
				collection.Load();
				collection.Add(factory.Load<UNDGCountryReference>(countryRef.PK));
				factory.Save();
				AssertEquals("adding twice does nothing", 1, collection.Count);
				AssertEquals("adding twice does not add a duplicate pivot", 1, factory.Load<UNDGCountryReferencePivot>(new ZQuery()).Length);
			}

			{
				var factory = new BusinessObjectFactory();
				var collection = new UNDGCountryReferenceManyToManyCollection<UNDGSubstance>(factory.Load<UNDGSubstance>(subs1.PK));
				collection.Load();
				AssertEquals("collection is initially loaded", 1, collection.Count);
			}

			{
				var factory = new BusinessObjectFactory();
				var collection = new UNDGCountryReferenceManyToManyCollection<UNDGSubstance>(factory.Load<UNDGSubstance>(subs1.PK));
				collection.Load();
				collection.RemoveAndDelete(factory.Load<UNDGCountryReference>(countryRef.PK));
				factory.Save();
				AssertEquals("collection can be deleted from", 0, collection.Count);
				AssertEquals("pivot deleted", 0, factory.Load<UNDGCountryReferencePivot>(new ZQuery()).Length);
			}
		}

		public void TestAddNewIsNotSupported()
		{
			var subs1 = DGSubstanceTestHelper.Create("1111", "A", UNDGSubstanceStandardTypes.IATA);
			var collection = new UNDGCountryReferenceManyToManyCollection<UNDGSubstance>(Factory.Load<UNDGSubstance>(subs1.PK));
			AssertExceptionThrown<NotSupportedException>(() => collection.AddNew());
		}

		#region TestAttachDetachEvents

		public void TestAttachingCountryReferenceAddsEvent()
		{
			DGSubstanceTestHelper.Create("1234", "A", UNDGSubstanceStandardTypes.IMO);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "NZ";
			countryReference.DCR_Type = "NZT";

			substance.UNDGCountryReferences.Add(countryReference);

			Factory.Save();

			var attachLogs = substance.Logs.Find(log => log.SL_SE_NKEvent == "ATC").ToArray();
			AssertEquals("Should be an attach log", attachLogs.Length, 1);
		}

		public void TestDetachingCountryReferenceAddsEvent()
		{
			DGSubstanceTestHelper.Create("1234", "A", UNDGSubstanceStandardTypes.IMO);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "NZ";
			countryReference.DCR_Type = "NZT";

			substance.UNDGCountryReferences.Add(countryReference);

			Factory.Save();

			substance.UNDGCountryReferences.Remove(countryReference);

			Factory.Save();

			var detachLogs = substance.Logs.Find(log => log.SL_SE_NKEvent == "DTC").ToArray();
			AssertEquals("Should be a detach log", detachLogs.Length, 1);
		}

		public void TestAttachingThenDetachingInSingleTransactionDoesNotCreateEvent()
		{
			DGSubstanceTestHelper.Create("1234", "A", UNDGSubstanceStandardTypes.IMO);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "NZ";
			countryReference.DCR_Type = "NZT";

			Factory.Save();

			substance.UNDGCountryReferences.Add(countryReference);
			substance.UNDGCountryReferences.Remove(countryReference);

			Factory.Save();

			var attachLogs = substance.Logs.Find(log => log.SL_SE_NKEvent == "ATC").ToArray();
			var detachLogs = substance.Logs.Find(log => log.SL_SE_NKEvent == "DTC").ToArray();

			AssertEquals("No attach or detach logs should be created", attachLogs.Length, 0);
			AssertEquals("No attach or detach logs should be created", detachLogs.Length, 0);
		}

		public void TestAttachEventReferencesGeneratedCorrectly()
		{
			DGSubstanceTestHelper.Create("1234", "A", UNDGSubstanceStandardTypes.IMO);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "FR";
			countryReference.DCR_Type = "ICPE";

			Factory.Save();

			substance.UNDGCountryReferences.Add(countryReference);

			Factory.Save();

			var attachLog = substance.Logs.Find(log => log.SL_SE_NKEvent == "ATC").First();
			AssertEquals("Attach log reference is correct", attachLog.SL_Reference, "ICPE Section Code 1234|TYP=IMO 1234 (A)");
		}

		public void TestDeleteEventReferencesGeneratedCorrectly()
		{
			DGSubstanceTestHelper.Create("1234", "A", UNDGSubstanceStandardTypes.IMO);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "FR";
			countryReference.DCR_Type = "ICPE";

			substance.UNDGCountryReferences.Add(countryReference);

			Factory.Save();

			substance.UNDGCountryReferences.Remove(countryReference);

			var detachLog = substance.Logs.Find(log => log.SL_SE_NKEvent == "DTC").First();
			AssertEquals("Attach log reference is correct", detachLog.SL_Reference, "ICPE Section Code 1234|TYP=IMO 1234 (A)");
		}

		public void TestAttachEventFunctionsCorrectlyWithOtherStandards()
		{
			var ridSubstance = Factory.New<UNDGSubstanceRID>();
			ridSubstance.RID_UNNO = "1234";
			ridSubstance.RID_Variant = "A";

			Factory.Save();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "FR";
			countryReference.DCR_Type = "ICPE";

			ridSubstance.UNDGCountryReferences.Add(countryReference);

			Factory.Save();

			var attachLog = ridSubstance.Logs.Find(log => log.SL_SE_NKEvent == "ATC").First();
			AssertEquals("Events work with other standards", attachLog.SL_Reference, "ICPE Section Code 1234|TYP=RID 1234 (A)");
		}

		public void TestAttachEventNotAddedWhenLoadingCollection()
		{
			DGSubstanceTestHelper.Create("1234", "A", UNDGSubstanceStandardTypes.IMO);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();

			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Code = "1234";
			countryReference.DCR_Description = "Some Description";
			countryReference.DCR_RN_NKCountry = "FR";
			countryReference.DCR_Type = "ICPE";

			substance.UNDGCountryReferences.Add(countryReference);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var loadedSubstance = UNDGSubstanceLoader.LoadSubstances(otherFactory, "1234", "A", UNDGSubstanceStandardTypes.IMO).First();
			_ = loadedSubstance.UNDGCountryReferences;

			var attachLogs = loadedSubstance.Logs.Find(log => log.SL_SE_NKEvent == "ATC");
			AssertEquals("Should be no new attach records", attachLogs.FirstOrDefault(log => !log.IsInDatabase), null);
		}

		#endregion
	}
}
