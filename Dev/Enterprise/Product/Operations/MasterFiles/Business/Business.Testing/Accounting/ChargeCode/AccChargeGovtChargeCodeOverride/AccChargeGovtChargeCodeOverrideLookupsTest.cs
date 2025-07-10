using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeGovtChargeCodeOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsProvideCorrectJobTypeListOptions()
		{
			var actualJobTypes = govtChargeCodeOverride.Lookups.JobTypes.Cast<CodeDescriptionPair>();

			AssertOptionsAsExpected(l => l.JobTypes, c => GetExpectedJobTypes());
			foreach (var jobType in actualJobTypes.Where(c => c.Code != "NJR" && c.Code != "ALL"))
			{
				Assert("The JobType should be JobInvoicingConsumerType", jobType is JobInvoicingConsumerType);
			}

			CodeDescriptionPairList GetExpectedJobTypes()
			{
				var result = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
				result.Add(new AllJobsConsumerType());
				result.Add(new CodeDescriptionPair("NJR", "Non-Job"));
				result.Add(JobInvoicingConsumerTypes.OneOffQuotation);
				result.Sort();
				return result;
			}
		}

		public void TestLookupsProvideCorrectDirectionListOptions()
		{
			var lookups = govtChargeCodeOverride.Lookups;

			AssertEquals("DirectionList.Count", 5, lookups.DirectionList.Count);
			Assert("The DirectionList should contain 'ALL'", lookups.DirectionList.ContainsCode(Constants.TransportModeDescriptions.All));
			Assert("The DirectionList should contain 'Import'", lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Import));
			Assert("The DirectionList should contain 'Export'", lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Export));
			Assert("The DirectionList should contain 'Domestic'", lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Domestic));
			Assert("The DirectionList should contain 'Other'", lookups.DirectionList.ContainsCode(Constants.FreightShipmentDirection.Code.Other));
		}

		public void TestLookupsProvideCorrectCostSellListOptions()
		{
			var costSellList = govtChargeCodeOverride.Lookups.CostSellList;

			AssertEquals("CostSellList.Count", 3, costSellList.Count);
			Assert("The CostSellList should contain 'ALL'", costSellList.ContainsCode("ALL"));
			Assert("The CostSellList should contain 'COS'", costSellList.ContainsCode("COS"));
			Assert("The CostSellList should contain 'REV'", costSellList.ContainsCode("REV"));
		}

		public void TestLookupsProvideCorrectTransportModeListOptions()
		{
			var jobTypesCase1 = new[]
{
				JobInvoicingConsumerTypes.Brokerage.Code,
			};

			foreach (var jobType in jobTypesCase1)
			{
				AssertOptionsAsExpected(l => l.TransportModeList, c =>
				new CodeDescriptionPairList {
					new CodeDescriptionPair(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.FixedTransportInstallations, Constants.TransportModeDescriptions.FixedTransportInstallations.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportModeDescriptions.InlandWaterwayTransport.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.OwnPropulsion, Constants.TransportModeDescriptions.OwnPropulsion.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Mail, Constants.TransportModeDescriptions.Mail.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
				}, jobType);
			}
			var jobTypesCase2 = new[]
{
				JobInvoicingConsumerTypes.CFSLoadListCode,
				JobInvoicingConsumerTypes.CFSShipmentCode,
			};

			foreach (var jobType in jobTypesCase2)
			{
				AssertOptionsAsExpected(l => l.TransportModeList, c =>
				new CodeDescriptionPairList {
					new CodeDescriptionPair(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
				}, jobType);
			}

			var jobTypesCase3 = new[]
			{
				JobInvoicingConsumerTypes.QuotedBookingCode,
				JobInvoicingConsumerTypes.OneOffQuotationCode,
			};

			foreach (var jobType in jobTypesCase3)
			{
				AssertOptionsAsExpected(c => c.TransportModeList, c =>
				new CodeDescriptionPairList {
					new CodeDescriptionPair(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail.ToString()),
					new CodeDescriptionPair(Constants.TransportModes.Courier, Constants.TransportModeDescriptions.Courier.ToString()),
				}, jobType);
			}

			var otherJobTypes = allJobTypes
				.Except(jobTypesCase1)
				.Except(jobTypesCase2)
				.Except(jobTypesCase3);

			var expectedList = new CodeDescriptionPairList(OLookUpEditType.TransportType);
			expectedList.Insert(0, new CodeDescriptionPair(Constants.TransportModes.All, Constants.TransportModeDescriptions.All));

			foreach (var jobType in otherJobTypes)
			{
				AssertOptionsAsExpected(l => l.TransportModeList, c => expectedList, jobType);
			}
		}

		void AssertOptionsAsExpected(Func<AccChargeGovtChargeCodeOverrideLookups, CodeDescriptionPairList> codePairListGetter, Func<IJobConfiguration, CodeDescriptionPairList> expectedResultGetter, string jobType = "ALL")
		{
			govtChargeCodeOverride.ACG_JobType = jobType;
			var govtChargeCodeOverrideLookup = new AccChargeGovtChargeCodeOverrideLookups(govtChargeCodeOverride);

			var optionsActual = codePairListGetter(govtChargeCodeOverrideLookup).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();
			var optionsExpected = expectedResultGetter(govtChargeCodeOverride).Cast<CodeDescriptionPair>().Select(c => (c.Code, c.Description)).ToArray();

			AssertArrayEqualsByElements(optionsExpected, optionsActual);
		}

		protected override void SetUp()
		{
			base.SetUp();

			govtChargeCodeOverride = Factory.New<AccChargeGovtChargeCodeOverride>();
			allJobTypes = govtChargeCodeOverride.Lookups.JobTypes.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();
		}

		AccChargeGovtChargeCodeOverride govtChargeCodeOverride;
		string[] allJobTypes;
	}
}
