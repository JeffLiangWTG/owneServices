using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ConsolidatedJobDeclarationCollection<BaseJobDeclaration>))]
	sealed class ConsolidatedJobDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddJobDeclaration()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = Factory.New<DummyConsolidatedDeclaration>();
				IConsolidatedJobDeclarationCollection<BaseJobDeclaration> cusReconDeclarationCollection = new ConsolidatedJobDeclarationCollection<BaseJobDeclarationForTesting>(consolidatedDeclaration);
				AssertExceptionThrown<ArgumentException>("JobDeclaration with wrong type cannot be added", () => cusReconDeclarationCollection.Add(Factory.New<BaseJobDeclaration>()));
				cusReconDeclarationCollection = new ConsolidatedJobDeclarationCollection<BaseJobDeclaration>(consolidatedDeclaration);
				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				AssertExceptionThrown<DeveloperNotificationException>("JobDeclaration with no entries cannot be added", () => cusReconDeclarationCollection.Add(declaration));
				declaration.ActiveEntryHeaders.AddNew();
				AssertExceptionThrown<DeveloperNotificationException>("JobDeclaration with wrong entry status cannot be added", () => cusReconDeclarationCollection.Add(declaration));
				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				cusReconDeclarationCollection.Add(declaration);
				AssertEquals("Added JobDeclaration is accessible to the adding collection", declaration, cusReconDeclarationCollection[0]);
				var reconEntry = Factory.LoadTop1<ConsolidatedDeclarationEntry>(new ZQuery(CusReconEntrySchema.CRE_CRD, consolidatedDeclaration.PK));
				AssertEquals("CusReconEntry created for added JobDeclaration", declaration.ActiveEntryHeaders[0].PK, reconEntry.CRE_CH_OriginalEntry);
				AssertEquals("Entry status should be set for added declaration", ConsolidatedEntryStatusList.Codes.AppliedToConsolidation, declaration.JE_EntryStatus);
			});
		}

		public void TestRemoveJobDeclaration()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = Factory.New<DummyConsolidatedDeclaration>();
				var cusReconDeclarationCollection = new ConsolidatedJobDeclarationCollection<BaseJobDeclarationForTesting>(consolidatedDeclaration);
				var declaration = Factory.New<BaseJobDeclarationForTesting>();
				declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				var cusEntry = declaration.ActiveEntryHeaders.AddNew();
				cusReconDeclarationCollection.Add(declaration);
				AssertEquals("Declaration has been added to consolidation", 1, cusReconDeclarationCollection.Count);
				declaration.JE_EntryStatus = ZString.Empty;
				cusReconDeclarationCollection.Remove(declaration);
				AssertEquals("Declaration can be removed from consolidation", 0, cusReconDeclarationCollection.Count);
				AssertEquals("ReconEntry is deleted after removal", false, Factory.Exists(typeof(ConsolidatedDeclarationEntry), new ZQuery(CusReconEntrySchema.CRE_CRD, consolidatedDeclaration.PK).AddToFilter(CusReconEntrySchema.CRE_CH_OriginalEntry, cusEntry.PK)));
				AssertEquals("Entry status should be reset for removed declaration", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_EntryStatus);
			});
		}

		public void TestConcurrency()
		{
			var declaration = Factory.New<DummyJobDeclaration>();
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			var consolidatedDeclaration = Factory.New<DummyConsolidatedDeclaration>();
			var cusReconDeclarationCollection = new ConsolidatedJobDeclarationCollection<DummyJobDeclaration>(consolidatedDeclaration);
			consolidatedDeclaration.JobDeclarations.Add(declaration);

			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;
			anotherFactory.Load<DummyJobDeclaration>(declaration.PK).JE_EntryStatus = "";
			anotherFactory.Save();

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		public void TestFilterByCusReconEntry()
		{
			var valid = Factory.New<BaseJobDeclaration>();
			valid.ActiveEntryHeaders.AddNew();
			var invalid = Factory.New<BaseJobDeclaration>();
			invalid.ActiveEntryHeaders.AddNew();
			var consolidatedDeclaration = Factory.NewWithValidTestData<DummyConsolidatedDeclaration>();
			consolidatedDeclaration.CRD_JE_LeadDeclaration = valid.PK;
			var cusReconEntry = Factory.New<ConsolidatedDeclarationEntry>();
			cusReconEntry.CRE_CRD = consolidatedDeclaration.PK;
			cusReconEntry.CRE_CH_OriginalEntry = valid.ActiveEntryHeaders[0].PK;
			cusReconEntry.CRE_OA_DeclarantAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses[0].PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				var cusReconDeclarationCollection = new ConsolidatedJobDeclarationCollection<BaseJobDeclaration>(consolidatedDeclaration);
				cusReconDeclarationCollection.Load();
				AssertEquals("Valid", true, valid.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
				AssertEquals("Invalid", false, invalid.MatchesFilter(cusReconDeclarationCollection.CompleteFilter));
			});
		}

		public void TestIsCongruentOn()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = Factory.NewWithValidTestData<DummyConsolidatedDeclaration>();
				var cusReconDeclarationCollection = new ConsolidatedJobDeclarationCollection<BaseJobDeclaration>(consolidatedDeclaration);
				for (int i = 0; i < 3; i++)
				{
					var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
					jobDeclaration.ActiveEntryHeaders.AddNew();
					jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
					cusReconDeclarationCollection.Add(jobDeclaration);
				}
				cusReconDeclarationCollection[0].JE_DateAtFinalDestination = ZDate.Today;
				cusReconDeclarationCollection[1].JE_CustomsProfile = "\t\r";
				cusReconDeclarationCollection[2].JE_TotalVolume = -64543.4564m;
				AssertEquals("Congruent with nothing to check on", true, cusReconDeclarationCollection.IsCongruentOn());
				AssertEquals("Incongruent on Guid", false, cusReconDeclarationCollection.IsCongruentOn(dec => dec.PK));
				AssertEquals("Incongruent on Date", false, cusReconDeclarationCollection.IsCongruentOn(dec => dec.JE_DateAtFinalDestination));
				AssertEquals("Incongruent on String", false, cusReconDeclarationCollection.IsCongruentOn(dec => dec.JE_CustomsProfile));
				AssertEquals("Incongruent on Decimal", false, cusReconDeclarationCollection.IsCongruentOn(dec => dec.JE_TotalVolume));
				AssertEquals("Congruent on properties no different", true, cusReconDeclarationCollection.IsCongruentOn(dec => dec.JE_DateAtOrigin, dec => dec.JE_GoodsOrigin, dec => dec.JE_TotalNoOfPieces));
				cusReconDeclarationCollection.Remove(cusReconDeclarationCollection[2]);
				cusReconDeclarationCollection.Remove(cusReconDeclarationCollection[1]);
				AssertEquals("Congruent with single declaration", true, cusReconDeclarationCollection.IsCongruentOn(dec => dec.PK, dec => dec.JE_DateAtFinalDestination, dec => dec.JE_CustomsProfile, dec => dec.JE_TotalVolume));
				cusReconDeclarationCollection.Remove(cusReconDeclarationCollection[0]);
				AssertEquals("Congruent with no declaration", true, cusReconDeclarationCollection.IsCongruentOn(dec => dec.PK, dec => dec.JE_DateAtFinalDestination, dec => dec.JE_CustomsProfile, dec => dec.JE_TotalVolume));
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ConsolidatedJobDeclarationCollection<BaseJobDeclaration>(Factory.New<DummyConsolidatedDeclaration>());
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.ActiveEntryHeaders.AddNew();
			declaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			return declaration;
		}
	}
}
