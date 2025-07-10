using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobConfigurationSelectorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			AssertJobTypeList(BizObj.JobTypeList);
		}

		public void TestGetBaseJobTypeList()
		{
			AssertJobTypeList(JobConfigurationSelectorLookups.GetBaseJobTypeList());
		}

		void AssertJobTypeList(CodeDescriptionPairList jobTypeList)
		{
			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", jobTypeList[0].Code);
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", jobTypeList[0] is JobInvoicingConsumerType);
			AssertEquals("The JobTypeList should contain 'SHP'", true, jobTypeList.ContainsCode("SHP"));
			AssertEquals("The JobTypeList should contain 'QSH'", true, jobTypeList.ContainsCode("QSH"));
			AssertEquals("The JobTypeList should contain 'CLL'", true, jobTypeList.ContainsCode("CLL"));
			AssertEquals("The JobTypeList should contain 'CSH'", true, jobTypeList.ContainsCode("CSH"));
			AssertEquals("The JobTypeList should contain 'BRK'", true, jobTypeList.ContainsCode("BRK"));
			AssertEquals("The JobTypeList should contain 'WSJ'", true, jobTypeList.ContainsCode("WSJ"));
			AssertEquals("The JobTypeList should contain 'WVO'", true, jobTypeList.ContainsCode("WVO"));
		}

		public void TestDirectionList()
		{
			AssertDirectionList(BizObj.DirectionList);
		}

		public void TestGetBaseDirectionList()
		{
			AssertDirectionList(JobConfigurationSelectorLookups.GetBaseDirectionList());
		}

		void AssertDirectionList(CodeDescriptionPairList directionList)
		{
			AssertEquals("DirectionList.Count", 5, directionList.Count);
			AssertEquals("The DirectionList should contain 'All' as the first element", "ALL", directionList[0].Code);
			AssertEquals("The DirectionList should contain 'Import'", true, directionList.ContainsCode(Constants.FreightShipmentDirection.Code.Import));
			AssertEquals("The DirectionList should contain 'Export'", true, directionList.ContainsCode(Constants.FreightShipmentDirection.Code.Export));
			AssertEquals("The DirectionList should contain 'Domestic'", true, directionList.ContainsCode(Constants.FreightShipmentDirection.Code.Domestic));
			AssertEquals("The DirectionList should contain 'Other'", true, directionList.ContainsCode(Constants.FreightShipmentDirection.Code.Other));
		}

		#region Mode

		public void TestModeList()
		{
			AssertModeList(BizObj.ModeList);
		}

		public void TestGetBaseModeList()
		{
			AssertModeList(JobConfigurationSelectorLookups.GetBaseModeList());
		}

		void AssertModeList(CodeDescriptionPairList modeList)
		{
			AssertEquals("ModeList.Count", 8, modeList.Count);
			AssertEquals("The ModeList should contain 'ALL' as the first element", "ALL", BizObj.ModeList[0].Code);
			AssertEquals("The ModeList should contain 'AIR'", true, modeList.ContainsCode(Constants.TransportModes.Air));
			AssertEquals("The ModeList should contain 'SEA'", true, modeList.ContainsCode(Constants.TransportModes.Sea));
			AssertEquals("The ModeList should contain 'ROA'", true, modeList.ContainsCode(Constants.TransportModes.Road));
			AssertEquals("The ModeList should contain 'RAI'", true, modeList.ContainsCode(Constants.TransportModes.Rail));
			AssertEquals("The ModeList should contain 'FAS'", true, modeList.ContainsCode(Constants.TransportModes.AirSea));
			AssertEquals("The ModeList should contain 'FSA'", true, modeList.ContainsCode(Constants.TransportModes.SeaAir));
			AssertEquals("The ModeList should contain 'COU'", true, modeList.ContainsCode(Constants.TransportModes.Courier));
		}

		public void TestTransportModeList_AccordingToJobType()
		{
			CombineAssertions(() =>
			{
				AssertDisplaysCorrectTransportModeListForJobType(JobInvoicingConsumerTypes.ForwardingConsolCode,
					new[]
					{
						Constants.TransportModes.All,
						Constants.TransportModes.Air,
						Constants.TransportModes.Sea,
						Constants.TransportModes.SeaAir,
						Constants.TransportModes.AirSea,
						Constants.TransportModes.Road,
						Constants.TransportModes.Rail,
						Constants.TransportModes.Courier,
					});

				AssertDisplaysCorrectTransportModeListForJobType(JobInvoicingConsumerTypes.GatewayConsolCode,
					new[]
					{
						Constants.TransportModes.All,
						Constants.TransportModes.Air,
						Constants.TransportModes.Sea,
						Constants.TransportModes.SeaAir,
						Constants.TransportModes.AirSea,
						Constants.TransportModes.Road,
						Constants.TransportModes.Rail,
						Constants.TransportModes.Courier,
					});

				AssertDisplaysCorrectTransportModeListForJobType(JobInvoicingConsumerTypes.ShipmentCode,
					new[]
					{
						Constants.TransportModes.All,
						Constants.TransportModes.Air,
						Constants.TransportModes.Sea,
						Constants.TransportModes.SeaAir,
						Constants.TransportModes.AirSea,
						Constants.TransportModes.Road,
						Constants.TransportModes.Rail,
						Constants.TransportModes.Courier,
					});

				AssertDisplaysCorrectTransportModeListForJobType(JobInvoicingConsumerTypes.QuotedBookingCode,
					new[]
					{
						Constants.TransportModes.All,
						Constants.TransportModes.Air,
						Constants.TransportModes.Sea,
						Constants.TransportModes.Road,
						Constants.TransportModes.Rail,
						Constants.TransportModes.Courier,
					});
			});

			void AssertDisplaysCorrectTransportModeListForJobType(string jobType, string[] expectedTransportModes)
			{
				var autoRateDate = new AutoRateDate();
				autoRateDate.JobType = jobType;
				var transportModes = autoRateDate.ModeList.Cast<CodeDescriptionPair>().Select(x => x.Code);
				AssertContainsExactElementsInExactOrder($"Incorrect transport modes for job type {jobType}",
					expectedTransportModes,
					transportModes);
			}
		}

		#endregion

		#region Implementation

		protected abstract IJobConfigurationSelector GetNewBizObj { get; }

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj;
		}

		protected IJobConfigurationSelector BizObj { get; set; }

		protected void AssertListsAreSame(CodeDescriptionPairList list1, CodeDescriptionPairList list2)
		{
			AssertEquals("Number of items is different", list1.Count, list2.Count);
			foreach (ICodeDescription item in list1)
			{
				Assert("Lists are different", list2.Contains(item));
			}
		}

		protected CodeDescriptionPairList JobTypeList
		{
			get { return fJobTypeList ?? (fJobTypeList = BizObj.JobTypeList); }
		}
		CodeDescriptionPairList fJobTypeList;

		#endregion

	}
}
