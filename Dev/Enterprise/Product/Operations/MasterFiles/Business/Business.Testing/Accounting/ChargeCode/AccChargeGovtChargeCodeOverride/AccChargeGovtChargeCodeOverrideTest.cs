using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeGovtChargeCodeOverride))]
	sealed class AccChargeGovtChargeCodeOverrideTest : EnterpriseBusinessObjectTestCase
	{
		public void TestACG_Direction()
		{
			var govtOverride = Factory.NewWithValidTestData<AccChargeCode>().GovtChargeCodeOverrides.AddNew();

			govtOverride.ACG_Direction = string.Empty;
			AssertEquals("ALL", govtOverride.ACG_Direction);
			AssertEquals("ALL", govtOverride.ACG_DirectionInfo.PersistentValue);

			govtOverride.ACG_Direction = "ASD";
			AssertEquals("ASD", govtOverride.ACG_Direction);
			AssertEquals("ASD", govtOverride.ACG_DirectionInfo.PersistentValue);
		}

		public void TestACG_TransportMode()
		{
			var govtOverride = Factory.NewWithValidTestData<AccChargeCode>().GovtChargeCodeOverrides.AddNew();

			govtOverride.ACG_TransportMode = string.Empty;
			AssertEquals("ALL", govtOverride.ACG_TransportMode);
			AssertEquals("ALL", govtOverride.ACG_TransportModeInfo.PersistentValue);

			govtOverride.ACG_TransportMode = "ASD";
			AssertEquals("ASD", govtOverride.ACG_TransportMode);
			AssertEquals("ASD", govtOverride.ACG_TransportModeInfo.PersistentValue);
		}

		public void TestIsDuplicateOf()
		{
			var charge = Factory.New<AccChargeCode>();
			var govtChargeCode1 = charge.GovtChargeCodeOverrides.AddNew();
			var govtChargeCode2 = charge.GovtChargeCodeOverrides.AddNew();

			govtChargeCode1.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			govtChargeCode1.ACG_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			govtChargeCode1.ACG_Direction = Core.Constants.FreightShipmentDirection.Code.Import;
			govtChargeCode1.ACG_TransportMode = Constants.TransportModes.Air;

			govtChargeCode2.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			govtChargeCode2.ACG_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			govtChargeCode2.ACG_Direction = Core.Constants.FreightShipmentDirection.Code.Import;
			govtChargeCode2.ACG_TransportMode = Constants.TransportModes.Air;

			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost;
			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_CostSellAll = AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost;
			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_TransportMode = Constants.TransportModes.Sea;
			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_TransportMode = Constants.TransportModes.Sea;
			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_JobType = JobInvoicingConsumerTypes.BrokerageCode;
			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_JobType = JobInvoicingConsumerTypes.QuotedBookingCode;
			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode2.ACG_JobType = JobInvoicingConsumerTypes.OneOffQuotationCode;
			govtChargeCode1.ACG_JobType = JobInvoicingConsumerTypes.OneOffQuotationCode;

			Assert(govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(govtChargeCode2.IsDuplicateOf(govtChargeCode1));

			govtChargeCode1.ACG_Direction = Core.Constants.FreightShipmentDirection.Code.Import;

			Assert(!govtChargeCode1.IsDuplicateOf(govtChargeCode2));
			Assert(!govtChargeCode2.IsDuplicateOf(govtChargeCode1));
		}

		public void TestLookupAttributes()
		{
			Func<string, string> getLookupRef = s => typeof(AccChargeGovtChargeCodeOverride).GetProperty(s).GetAttribute<ListAttribute>().ListDataSourceMember;

			AssertEquals("Lookups.CostSellList", getLookupRef(nameof(AccChargeGovtChargeCodeOverride.ACG_CostSellAll)));
			AssertEquals("Lookups.JobTypes", getLookupRef(nameof(AccChargeGovtChargeCodeOverride.ACG_JobType)));
			AssertEquals("Lookups.DirectionList", getLookupRef(nameof(AccChargeGovtChargeCodeOverride.ACG_Direction)));
			AssertEquals("Lookups.TransportModeList", getLookupRef(nameof(AccChargeGovtChargeCodeOverride.ACG_TransportMode)));
		}

		public void TestSetDefaultValues()
		{
			var govtChargeCodeOverride = Factory.New<AccChargeGovtChargeCodeOverride>();

			AssertEquals("Cost/Sell", "ALL", govtChargeCodeOverride.ACG_CostSellAll);
			AssertEquals("Job Type", "", govtChargeCodeOverride.ACG_JobType);
			AssertEquals("Transport Mode", "ALL", govtChargeCodeOverride.ACG_TransportMode);
			AssertEquals("Service Direction", "ALL", govtChargeCodeOverride.ACG_Direction);
		}

		public void TestLookupValuesCanBeSaveToDatabase()
		{
			var govtChargeCodeOverride = BusinessObject as AccChargeGovtChargeCodeOverride;
			var chargeCodePK = govtChargeCodeOverride.ACG_AC;
			govtChargeCodeOverride.Delete();
			AssertNotNull(govtChargeCodeOverride);

			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.GetAllCodes())
			{
				govtChargeCodeOverride.ACG_JobType = jobType;
				AssertEquals($"JobType: {jobType}", jobType, govtChargeCodeOverride.JobType?.Code ?? "NJR");

				var modeList = !govtChargeCodeOverride.JobType?.IsTransportModeSupported ?? false
					? new string[] { "ALL" }
					: govtChargeCodeOverride.Lookups.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();

				var directionList = !govtChargeCodeOverride.JobType?.IsDirectionSupported ?? false
					? new string[] { "ALL" }
					: govtChargeCodeOverride.Lookups.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray();

				var join = modeList.Join(directionList
					, _ => 1
					, _ => 1
					, (mode, direction) => (mode, direction)
				);

				foreach ((string mode, string direction) in join)
				{
					foreach (var costSellAll in govtChargeCodeOverride.Lookups.CostSellList.GetAllCodes())
					{
						var itemSaveToDb = (AccChargeGovtChargeCodeOverride)base.GetNewBusinessObject();
						itemSaveToDb.ACG_AC = chargeCodePK;
						itemSaveToDb.ACG_CostSellAll = costSellAll;
						itemSaveToDb.ACG_JobType = jobType;
						itemSaveToDb.ACG_TransportMode = mode;
						itemSaveToDb.ACG_Direction = direction;
						itemSaveToDb.ACG_GovtChargeCode = "1234";
						AssertNoExceptionThrown("Should NOT violate DB constraints", () => Factory.Save());
					}
				}
			}
		}

		public void TestACG_Direction_ReadOnly()
		{
			var govtChargeCodeOverride = BusinessObject as AccChargeGovtChargeCodeOverride;

			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.GetAllCodes())
			{
				govtChargeCodeOverride.ACG_JobType = jobType;
				AssertEquals("ReadOnly [" + jobType + "]", !(govtChargeCodeOverride.JobType?.IsDirectionSupported ?? false), govtChargeCodeOverride.ACG_DirectionInfo.ReadOnly);
			}
		}

		public void TestACG_TransportMode_ReadOnly()
		{
			var govtChargeCodeOverride = BusinessObject as AccChargeGovtChargeCodeOverride;

			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.GetAllCodes())
			{
				govtChargeCodeOverride.ACG_JobType = jobType;
				AssertEquals("ReadOnly [" + jobType + "]", !(govtChargeCodeOverride.JobType?.IsTransportModeSupported ?? false), govtChargeCodeOverride.ACG_TransportModeInfo.ReadOnly);
			}
		}

		public void TestIJobConfiguration()
		{
			var govtChargeCodeOverride = BusinessObject as AccChargeGovtChargeCodeOverride;
			AssertNotNull(govtChargeCodeOverride);

			IJobConfiguration jobConfiguration = govtChargeCodeOverride;
			AssertEquals("IncludeOptionsForAllJobTypes", false, jobConfiguration.IncludeOptionsForAllJobTypes);

			foreach (var jobType in govtChargeCodeOverride.Lookups.JobTypes.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				govtChargeCodeOverride.ACG_JobType = jobType;
				AssertEquals($"JobType: {jobType}", jobType, jobConfiguration.JobType);

				foreach (var transportMode in govtChargeCodeOverride.Lookups.TransportModeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					govtChargeCodeOverride.ACG_TransportMode = transportMode;
					AssertEquals($"JobType: {jobType},TransportMode: {transportMode}", transportMode, jobConfiguration.TransportMode);
				}

				foreach (var serviceDirection in govtChargeCodeOverride.Lookups.DirectionList.Cast<CodeDescriptionPair>().Select(x => x.Code))
				{
					govtChargeCodeOverride.ACG_Direction = serviceDirection;
					AssertEquals($"JobType: {jobType}", serviceDirection, jobConfiguration.ServiceDirection);
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (AccChargeGovtChargeCodeOverride)base.GetNewBusinessObject();
			result.ACG_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery()).PK;
			result.ACG_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			result.ACG_Direction = Constants.FreightShipmentDirection.Code.Import;
			result.ACG_TransportMode = Constants.TransportModes.Sea;
			result.ACG_GovtChargeCode = "1234";

			return result;
		}

		#endregion
	}
}
