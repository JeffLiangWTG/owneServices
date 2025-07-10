using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ExportContainerPenaltyCollection))]
	sealed class ExportContainerPenaltyCollectionTest : ActiveBusinessObjectCollectionTestCase<ExportContainerPenaltyCollection>
	{
		#region Implementation

		protected override ExportContainerPenaltyCollection GetCollectionToTest()
		{
			return Factory.New<CommonContainer>().ExportPenalties;
		}

		#endregion

		public void TestDefaultOfAddNew()
		{
			var consol = GetConsol();
			var container = consol.Containers.AddNew();
			var penalty = container.ExportPenalties.AddNew();

			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			AssertEquals(ContainerPenaltyProcessType.Export, penalty.CPY_ProcessType);
			AssertEquals("NZAKL", penalty.CPY_RL_NKLocation);
			AssertEquals("NZD", penalty.CPY_RX_NKCurrency);
		}

		public void TestFindOrCreateContainerPenalty()
		{
			var consol = GetConsol();
			var container = consol.Containers.AddNew();

			var penalty1 = container.ExportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport);
			AssertNull(penalty1);

			var penalty2 = container.ExportPenalties.FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				ContainerPenaltyCreditorType.Codes.Transport,
				timeUnit: ContainerPenaltyTimeUnit.Codes.Hours,
				creditor: consol.DeparturePackCFSTransportAddress);

			AssertEquals(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, penalty2.CPY_PenaltyType);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Transport, penalty2.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Hours, penalty2.CPY_TimeUnit);
			AssertEquals(consol.DeparturePackCFSTransportAddress.OA_OH, penalty2.CPY_OH_Creditor);
			AssertEquals(ContainerPenaltyProcessType.Export, penalty2.CPY_ProcessType);
			AssertEquals("NZAKL", penalty2.CPY_RL_NKLocation);
			AssertEquals("NZD", penalty2.CPY_RX_NKCurrency);

			penalty1 = container.ExportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport);
			AssertNotNull(penalty1);
			AssertEquals(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, penalty1.CPY_PenaltyType);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Transport, penalty1.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Hours, penalty1.CPY_TimeUnit);
			AssertEquals(consol.DeparturePackCFSTransportAddress.OA_OH, penalty1.CPY_OH_Creditor);
			AssertEquals(ContainerPenaltyProcessType.Export, penalty1.CPY_ProcessType);
			AssertEquals("NZAKL", penalty1.CPY_RL_NKLocation);
			AssertEquals("NZD", penalty1.CPY_RX_NKCurrency);
		}

		public void TestDurationWhenManuallyAdd()
		{
			var strategy = new Mock<IContainerDefaultingStrategy>();

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLOnBoardVessel, "FCL Loaded"));

				container.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 1);
				container.JC_FCLOnBoardVessel = new ZDateTime(2020, 1, 5);

				container.ExportPenalties.DeleteAll();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
				penalty.FreeTimeAsDays = 3;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

				AssertEquals("The registry setting 'CTO Storage Free Days for Export' should not be used to default STO CAR.", (ZByte)3, penalty.FreeTimeAsDays);
				AssertEquals((ZByte)2, penalty.DurationAsDays);
			}
		}

		public void TestDefaultCreditorForTWT()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG00A";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "ADDR000A";
			address.OA_RL_NKRelatedPortCode = "CNQIN";

			var consol = GetConsol();
			consol.JK_OA_DeparturePackCFSTransportAddress = address.PK;
			var container = consol.Containers.AddNew();
			var penalty = container.ExportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;

			AssertEquals(org.PK, penalty.CPY_OH_Creditor);
		}

		public void TestDuplicateWarning()
		{
			var consol = GetConsol();
			var container = consol.Containers.AddNew();
			var penalty1 = container.ExportPenalties.AddNew();
			penalty1.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty1.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;

			var penalty2 = container.ExportPenalties.AddNew();
			penalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;

			AssertHasWarning(penalty2.CPY_CreditorTypeInfo, "You can't enter more than one penalty for same creditor and location.");
		}

		public void TestDefaultDepartureStoragePenalty()
		{
			var consol = GetConsol();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			var departurePackCFSTransport = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_DeparturePackCFSTransportAddress = departurePackCFSTransport.MainAddress.PK;

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			var arrivalUnpackCFSTransport = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalUnpackCFSTransport.MainAddress.PK;

			var strategy = new Mock<IContainerDefaultingStrategy>();
			strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns<IContainerPenaltyMatchResult>(null);
			var container = Factory.New<IForwardingContainer>() as CommonContainer;
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				consol.Containers.Add(container);

				container.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 1);
				container.JC_FCLOnBoardVessel = new ZDateTime(2020, 1, 9);
				AssertEquals(1, container.ExportPenalties.Count);
			}

			strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var twoFreeDaysMatchResultMock = new Mock<IContainerPenaltyMatchResult>();
				twoFreeDaysMatchResultMock.Setup(x => x.FreeDays).Returns(2);

				var threeFreeDaysMatchResultMock = new Mock<IContainerPenaltyMatchResult>();
				threeFreeDaysMatchResultMock.Setup(x => x.FreeDays).Returns(2);

				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CTO", It.IsAny<ZString>(), null)).Returns(twoFreeDaysMatchResultMock.Object);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(threeFreeDaysMatchResultMock.Object);
				container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLOnBoardVessel, "FCL Load"));
				consol.Containers.Add(container);
				container.JC_FCLWharfGateIn = new ZDateTime(2020, 2, 1);
				container.JC_FCLOnBoardVessel = new ZDateTime(2020, 2, 9);
				AssertEquals(1, container.ExportPenalties.Count);
				Assert(container.ExportPenalties.Any(x => x.FreeTimeAsDays == 2 && x.DurationAsDays == 7 && x.CPY_CreditorType == "CTO"));
			}

			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				strategy = new Mock<IContainerDefaultingStrategy>();
				var zeroFreeDaysMatchResultMock = new Mock<IContainerPenaltyMatchResult>();
				zeroFreeDaysMatchResultMock.Setup(x => x.FreeDays).Returns(8);

				var threeFreeDaysMatchResultMock = new Mock<IContainerPenaltyMatchResult>();
				threeFreeDaysMatchResultMock.Setup(x => x.FreeDays).Returns(3);

				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(threeFreeDaysMatchResultMock.Object);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CTO", It.IsAny<ZString>(), null)).Returns(zeroFreeDaysMatchResultMock.Object);
				container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLOnBoardVessel, "FCL Load"));
				consol.Containers.Add(container);
				container.JC_FCLWharfGateIn = new ZDateTime(2020, 3, 1);
				container.JC_FCLOnBoardVessel = new ZDateTime(2020, 3, 9);
				AssertEquals(1, container.ExportPenalties.Count);
				Assert(container.ExportPenalties.Any(x => x.FreeTimeAsDays == 2 && x.DurationAsDays == 7 && x.CPY_CreditorType == "CTO"));
			}
		}

		CommonConsol GetConsol()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG000";
			var address = Factory.New<OrgAddress>();
			address.OA_OH = org.PK;
			address.OA_Code = "ADDR0001";
			address.OA_RL_NKRelatedPortCode = "SGSIN";
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.JK_OA_DeparturePackCFSTransportAddress = address.PK;
			return consol;
		}
	}
}
