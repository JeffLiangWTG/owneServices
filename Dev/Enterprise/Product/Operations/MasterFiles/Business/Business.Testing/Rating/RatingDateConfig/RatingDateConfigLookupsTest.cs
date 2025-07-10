using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	public class RatingDateConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
			=> AssertCode
			(
				expected: new[]
				{
					JobInvoicingConsumerTypes.ForwardingConsolCode,
					JobInvoicingConsumerTypes.GatewayConsolCode
				},
				actual: RatingDateConfig.Lookups.JobTypeList
			);

		public void TestDirectionList()
			=> AssertCode
			(
				expected: new[]
				{
					Constants.FreightShipmentDirection.Code.All,
					Constants.FreightShipmentDirection.Code.Import,
					Constants.FreightShipmentDirection.Code.Export,
					Constants.FreightShipmentDirection.Code.Domestic,
					Constants.FreightShipmentDirection.Code.Other
				},
				actual: RatingDateConfig.Lookups.DirectionList
			);

		public void TestModeList()
			=> AssertCode
			(
				expected: new[]
				{
					Constants.TransportModes.Air,
					Constants.TransportModes.Sea
				},
				actual: RatingDateConfig.Lookups.TransportModeList
			);

		public void TestDateTypeList_WhenAir_NoJobType()
		{
			RatingDateConfig.RDT_JobType = "";
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Air;
			AssertCode
			(
				expected: new[]
				{
					JobDateTypes.Codes.ArrivalDate,
					JobDateTypes.Codes.DepartureDate,
					JobDateTypes.Codes.HouseBillIssueDate,
					JobDateTypes.Codes.JobOpenDate,
					JobDateTypes.Codes.AWBIssueDate
				},
				actual: RatingDateConfig.Lookups.DateTypeList
			);
		}

		public void TestDateTypeList_WhenAir_JobTypeFCC_GC()
		{
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Air;
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertCode
			(
				expected: new[]
				{
					JobDateTypes.Codes.ArrivalDate,
					JobDateTypes.Codes.DepartureDate,
					JobDateTypes.Codes.HouseBillIssueDate,
					JobDateTypes.Codes.FirstContainerGateInDate,
					JobDateTypes.Codes.LastContainerGateInDate,
					JobDateTypes.Codes.CFSReceivalStartDate,
					JobDateTypes.Codes.InterimReceiptDate,
					JobDateTypes.Codes.AWBIssueDate
				},
				actual: RatingDateConfig.Lookups.DateTypeList
			);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertCode
			(
				expected: new[]
				{
					JobDateTypes.Codes.ArrivalDate,
					JobDateTypes.Codes.DepartureDate,
					JobDateTypes.Codes.HouseBillIssueDate,
					JobDateTypes.Codes.JobOpenDate,
					JobDateTypes.Codes.FirstContainerGateInDate,
					JobDateTypes.Codes.LastContainerGateInDate,
					JobDateTypes.Codes.CFSReceivalStartDate,
					JobDateTypes.Codes.InterimReceiptDate,
					JobDateTypes.Codes.AWBIssueDate
				},
				actual: RatingDateConfig.Lookups.DateTypeList
			);
		}

		public void TestDateTypeList()
		{
			RatingDateConfig.RDT_JobType = "";
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			AssertCode
			(
				expected: new[]
				{
					JobDateTypes.Codes.ArrivalDate,
					JobDateTypes.Codes.DepartureDate,
					JobDateTypes.Codes.HouseBillIssueDate,
					JobDateTypes.Codes.JobOpenDate
				},
				actual: RatingDateConfig.Lookups.DateTypeList
			);
		}

		public void TestDisplayCorrectDateTypeList()
		{
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertCode
			(
				expected: new[]
				{
					JobDateTypes.Codes.ArrivalDate,
					JobDateTypes.Codes.DepartureDate,
					JobDateTypes.Codes.HouseBillIssueDate,
					JobDateTypes.Codes.FirstContainerGateInDate,
					JobDateTypes.Codes.LastContainerGateInDate,
					JobDateTypes.Codes.CFSReceivalStartDate,
					JobDateTypes.Codes.InterimReceiptDate
				},
				actual: RatingDateConfig.Lookups.DateTypeList
			);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertCode
			(
				expected: new[]
				{
					JobDateTypes.Codes.ArrivalDate,
					JobDateTypes.Codes.DepartureDate,
					JobDateTypes.Codes.HouseBillIssueDate,
					JobDateTypes.Codes.FirstContainerGateInDate,
					JobDateTypes.Codes.LastContainerGateInDate,
					JobDateTypes.Codes.CFSReceivalStartDate,
					JobDateTypes.Codes.JobOpenDate,
					JobDateTypes.Codes.InterimReceiptDate
				},
				actual: RatingDateConfig.Lookups.DateTypeList
			);
		}

		public void TestRateTypeList()
		{
			AssertCode
			(
				expected: new[] { JobRateTypes.Codes.Cost },
				actual: RatingDateConfig.Lookups.RateTypeList
			);
		}

		public void TestDisplayCorrectContainerModeList_AccordingToMode()
		{
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			AssertCode
			(
				expected: new[] {
					Constants.ContainerModes.All,
					Constants.ContainerModes.FCL,
					Constants.ContainerModes.LCL,
					Constants.ContainerModes.ShippersConsol,
					Constants.ContainerModes.Bulk,
					Constants.ContainerModes.Liquid,
					Constants.ContainerModes.BreakBulk,
					Constants.ContainerModes.BuyersConsol,
					Constants.ContainerModes.RollOnRollOff,
					Constants.ContainerModes.Other
				},
				actual: RatingDateConfig.Lookups.ContainerModeList
			);
		}

		public void TestDisplayCorrectContainerModeList_AccordingToJobType()
		{
			RatingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			var expectedCodes = new[]
			{
				Constants.ContainerModes.All,
				Constants.ContainerModes.FCL,
				Constants.ContainerModes.LCL,
				Constants.ContainerModes.ShippersConsol,
				Constants.ContainerModes.Bulk,
				Constants.ContainerModes.Liquid,
				Constants.ContainerModes.BreakBulk,
				Constants.ContainerModes.RollOnRollOff,
				Constants.ContainerModes.BuyersConsol,
				Constants.ContainerModes.Other
			};
			AssertCode(expectedCodes, RatingDateConfig.Lookups.ContainerModeList);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertCode(expectedCodes, RatingDateConfig.Lookups.ContainerModeList);
		}

		#region Implementation

		RatingDateConfig RatingDateConfig => ratingDateConfig ?? (ratingDateConfig = Factory.NewWithValidTestData<RatingDateConfig>());
		RatingDateConfig ratingDateConfig;

		void AssertCode(string[] expected, CodeDescriptionPairList actual)
			=> AssertContainsExactElementsInAnyOrder
			(
				expected,
				actual: actual.Cast<CodeDescriptionPair>().Select(x => x.Code)
			);

		#endregion
	}
}
