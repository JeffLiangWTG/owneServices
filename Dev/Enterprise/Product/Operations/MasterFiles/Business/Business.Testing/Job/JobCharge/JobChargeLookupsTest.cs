using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobChargeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxMessagesIsUsingTheCorrectCountryCodeFilter()
		{
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "MSG";
			taxMsg.A9_EnglishMsg = "ENG msg";
			taxMsg.A9_LocalMsg = "Local msg";
			taxMsg.A9_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			var lookup = new JobChargeLookups(jobCharge);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				jobCharge.JR_GC = uSCompany.PK;
				Assert("The TaxMessage should be found if the Parent Company and message country are the same", lookup.CostVATClasses.Contains(taxMsg));
				Assert("The TaxMessage should be found if the Parent Company and message country are the same", lookup.SellVATClasses.Contains(taxMsg));
				jobCharge.JR_GC = ZGuid.Empty;
				Assert("The TaxMessage should not be found if the Parent company is null and the Current Company is different from the message country", !lookup.CostVATClasses.Contains(taxMsg));
				Assert("The TaxMessage should not be found if the Parent company is null and the Current Company is different from the message country", !lookup.SellVATClasses.Contains(taxMsg));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("PreCondition", ZGuid.Empty, jobCharge.JR_GC);
				Assert("The TaxMessage should be found if the Parent company is null and the Current Company and message country are the same", lookup.CostVATClasses.Contains(taxMsg));
				Assert("The TaxMessage should be found if the Parent company is null and the Current Company and message country are the same", lookup.SellVATClasses.Contains(taxMsg));
			}
		}

		public void TestPlacesOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.New<JobCharge>();
				AssertEquals(typeof(ReadOnlyCodeDescriptionPairList), charge.Lookups.PlacesOfSupply.GetType());

				var placesCodes = charge.Lookups.PlacesOfSupply.GetAllCodes();
				var states = new RefCountryStatesDependentCollection(RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Factory);
				states.Load();
				var statesCodes = states.Cast<RefCountryStates>().Select(x => x.RW_Code).ToList();
				statesCodes.Add(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
				statesCodes.Add(PlaceOfSupplyListProvider.Codes.OtherTerritories);
				AssertContainsExactElementsInAnyOrder(statesCodes, placesCodes);

				var types = charge.Lookups.PlaceOfSupplyTypes;
				AssertEquals("Expect 2 elements", 2, types.Count);
				AssertCollectionContains("State", PlaceOfSupplyTypes.State, types);
				AssertCollectionContains("PredefinedRule", PlaceOfSupplyTypes.PredefinedRule, types);
			}
		}

		public void TestInvoiceTypeList()
		{
			JobHeader job1 = Factory.NewJobForTesting<JobHeader>();
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_JH = job1.PK;

			AssertEquals(typeof(InvoiceTypesList), charge.Lookups.InvoiceTypeList.GetType());

			JobInvoicingPlugInImplementorForTest sampleJob = new JobInvoicingPlugInImplementorForTest(Factory);

			((DummyJobHeaderParentJobInvoicingSupporter)sampleJob.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.AgencyBooking;
			job1.Parent = sampleJob;
			charge = Factory.New<JobCharge>();
			charge.JR_JH = job1.PK;
			AssertEquals(typeof(AgencyInvoiceTypesList), charge.Lookups.InvoiceTypeList.GetType());

			((DummyJobHeaderParentJobInvoicingSupporter)sampleJob.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.AgencyBillOfLading;
			job1.Parent = sampleJob;
			charge = Factory.New<JobCharge>();
			charge.JR_JH = job1.PK;
			AssertEquals(typeof(AgencyInvoiceTypesList), charge.Lookups.InvoiceTypeList.GetType());

			((DummyJobHeaderParentJobInvoicingSupporter)sampleJob.InvoicingSupporter).ConsumerType = JobInvoicingConsumerTypes.Shipment;
			job1.Parent = sampleJob;
			charge = Factory.New<JobCharge>();
			charge.JR_JH = job1.PK;
			AssertEquals(typeof(InvoiceTypesList), charge.Lookups.InvoiceTypeList.GetType());

			//null ref test
			var jobCharge = Factory.New<JobCharge>();
			AssertEquals(true, jobCharge.Lookups.InvoiceTypeList != null);

			var job = Factory.NewJobForTesting<JobHeader>();
			jobCharge.JR_JH = job.PK;
			AssertEquals(true, jobCharge.Lookups.InvoiceTypeList != null);

			job.Parent = null;
			AssertEquals(true, jobCharge.Lookups.InvoiceTypeList != null);

			JobInvoicingPlugInImplementorForTest plugin = new JobInvoicingPlugInImplementorForTest(Factory);
			(plugin.InvoicingSupporter as DummyJobHeaderParentJobInvoicingSupporter).ConsumerType = null;
			job.Parent = plugin;
			AssertEquals(true, jobCharge.Lookups.InvoiceTypeList != null);
		}

		public void TestInvoiceTypeList_DeletedJob()
		{
			JobHeader job = Factory.NewJobForTesting<JobHeader>();
			JobCharge charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;

			AssertEquals(typeof(InvoiceTypesList), charge.Lookups.InvoiceTypeList.GetType());

			job.Delete();

			AssertNoExceptionThrown("Should not load InvoiceTypeList for deleted job", () => charge.Lookups.InvoiceTypeList.GetType());
		}

		public void TestChargeCodes()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			JobHeader job = charge.Job;
			AssertEquals("IsInDatabase", false, charge.IsInDatabase);

			AssertNotNull(charge.Lookups.ChargeCodes);
			AssertEquals(typeof(AccChargeCodeCollection), charge.Lookups.ChargeCodes.GetType());
			AssertContainsChargeCodeTypes(charge, Core.Constants.ChargeType.Comment, Core.Constants.ChargeType.Margin, Core.Constants.ChargeType.Disbursement, Core.Constants.ChargeType.Revenue, Core.Constants.ChargeType.ManualJobAccrual);
			AssertDoesNotContainChargeCodeTypes(charge, Core.Constants.ChargeType.Overhead, Core.Constants.ChargeType.NonAccrual);
		}

		void AssertContainsChargeCodeTypes(JobCharge charge, params string[] codeTypes)
		{
			if (codeTypes.Length > 0)
			{
				AssertEquals("Contains AC_ChargeType", true, charge.Lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true).Contains(AccChargeCodeSchema.Constants.AC_ChargeType));
				foreach (var codeType in codeTypes)
				{
					AssertEquals(string.Format("Contains '{0}'", codeType), true, charge.Lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true).Contains(string.Format("'{0}'", codeType)));
				}
			}
			else
			{
				AssertEquals("Does not contain AC_ChargeType", false, charge.Lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true).Contains(AccChargeCodeSchema.Constants.AC_ChargeType));
			}
		}

		void AssertDoesNotContainChargeCodeTypes(JobCharge charge, params string[] codeTypes)
		{
			if (codeTypes.Length > 0)
			{
				AssertEquals("Contains AC_ChargeType", true, charge.Lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true).Contains(AccChargeCodeSchema.Constants.AC_ChargeType));
				foreach (var codeType in codeTypes)
				{
					AssertEquals(string.Format("Does not contain '{0}'", codeType), false, charge.Lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true).Contains(string.Format("'{0}'", codeType)));
				}
			}
			else
			{
				AssertEquals("Does not contain AC_ChargeType", false, charge.Lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true).Contains(AccChargeCodeSchema.Constants.AC_ChargeType));
			}
		}
	}
}
