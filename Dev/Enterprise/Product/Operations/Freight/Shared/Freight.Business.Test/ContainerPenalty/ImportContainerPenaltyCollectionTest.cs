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
	[TestedType(typeof(ImportContainerPenaltyCollection))]
	sealed class ImportContainerPenaltyCollectionTest : ActiveBusinessObjectCollectionTestCase<ImportContainerPenaltyCollection>
	{
		#region Implementation

		protected override ImportContainerPenaltyCollection GetCollectionToTest()
		{
			return Factory.New<CommonContainer>().ImportPenalties;
		}

		#endregion

		public void TestDefaultOfAddNew()
		{
			var conso = GetConsol();
			var container = conso.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();

			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			AssertEquals(ContainerPenaltyProcessType.Import, penalty.CPY_ProcessType);
			AssertEquals("HKHKG", penalty.CPY_RL_NKLocation);
			AssertEquals("HKD", penalty.CPY_RX_NKCurrency);
		}

		public void TestFindOrCreateContainerPenalty()
		{
			var consol = GetConsol();
			var container = consol.Containers.AddNew();

			var penalty1 = container.ImportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport);
			AssertNull(penalty1);

			var penalty2 = container.ImportPenalties.FindOrCreateContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
				ContainerPenaltyCreditorType.Codes.Transport,
				timeUnit: ContainerPenaltyTimeUnit.Codes.Hours,
				creditor: consol.ArrivalUnpackCFSTransportAddress);

			AssertEquals(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, penalty2.CPY_PenaltyType);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Transport, penalty2.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Hours, penalty2.CPY_TimeUnit);
			AssertEquals(consol.ArrivalUnpackCFSTransportAddress.OA_OH, penalty2.CPY_OH_Creditor);
			AssertEquals(ContainerPenaltyProcessType.Import, penalty2.CPY_ProcessType);
			AssertEquals("HKHKG", penalty2.CPY_RL_NKLocation);
			AssertEquals("HKD", penalty2.CPY_RX_NKCurrency);

			penalty1 = container.ImportPenalties.FindContainerPenalty(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, ContainerPenaltyCreditorType.Codes.Transport);
			AssertNotNull(penalty1);
			AssertEquals(ContainerPenaltyPenaltyType.Codes.TruckWaitTime, penalty1.CPY_PenaltyType);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Transport, penalty1.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Hours, penalty1.CPY_TimeUnit);
			AssertEquals(consol.ArrivalUnpackCFSTransportAddress.OA_OH, penalty1.CPY_OH_Creditor);
			AssertEquals(ContainerPenaltyProcessType.Import, penalty1.CPY_ProcessType);
			AssertEquals("HKHKG", penalty1.CPY_RL_NKLocation);
			AssertEquals("HKD", penalty1.CPY_RX_NKCurrency);
		}

		public void TestDurationWhenManuallyAdd()
		{
			var strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var conso = GetConsol();
				var container = Factory.New<IForwardingContainer>() as CommonContainer;
				conso.Containers.Add(container);
				strategy.Setup(x => x.CalculateStorageStart(It.IsAny<ZString>())).Returns(() => (container.JC_FCLAvailable, container.JC_FCLAvailable, "CTO Available"));
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
				container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 5);

				container.JC_EmptyReturnedBy = new ZDateTime(2020, 1, 7);
				container.JC_ContainerYardEmptyReturnGateIn = new ZDateTime(2020, 1, 13);

				container.ImportPenalties.DeleteAll();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
				penalty.FreeTimeAsDays = 2;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals((ZByte)6, penalty.DurationAsDays);

				penalty.FreeTimeAsDays = 3;
				penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				AssertEquals("JC_ArrivalCTOStorageStartDate should not be calculated based on FreeTimeAsDays.", (ZByte)5, penalty.DurationAsDays);
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
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = address.PK;
			var container = consol.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;

			AssertEquals(org.PK, penalty.CPY_OH_Creditor);
		}

		public void TestDuplicateWarning()
		{
			var consol = GetConsol();
			var container = consol.Containers.AddNew();
			var penalty1 = container.ImportPenalties.AddNew();
			penalty1.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty1.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;

			var penalty2 = container.ImportPenalties.AddNew();
			penalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			penalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;

			AssertHasWarning(penalty2.CPY_CreditorTypeInfo, "You can't enter more than one penalty for same creditor and location.");
		}

		public void TestDefaultArrivalStoragePenalty()
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
			var container = Factory.New<IForwardingContainer>() as CommonContainer;
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns<IContainerPenaltyMatchResult>(null);
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLAvailable, "CTO Available"));
				consol.Containers.Add(container);

				container.JC_FCLAvailable = new ZDateTime(2020, 1, 1);
				container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2020, 1, 1);
				container.JC_FCLWharfGateOut = new ZDateTime(2020, 1, 9);
				AssertEquals(1, container.ImportPenalties.Count);
				Assert(container.ImportPenalties.Any(x => x.FreeTimeAsDays == 0 && x.DurationAsDays == 9 && x.CPY_CreditorType == "CTO"));
			}

			strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var matchResultMock1 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock1.Setup(x => x.FreeDays).Returns(2);
				var matchResultMock2 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock2.Setup(x => x.FreeDays).Returns(3);

				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CTO", It.IsAny<ZString>(), null)).Returns(matchResultMock1.Object);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(matchResultMock2.Object);
				container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLAvailable, "CTO Available"));
				consol.Containers.Add(container);
				container.JC_FCLAvailable = new ZDateTime(2020, 2, 1);
				container.JC_FCLWharfGateOut = new ZDateTime(2020, 2, 9);
				AssertEquals(1, container.ImportPenalties.Count);
				Assert(container.ImportPenalties.Any(x => x.FreeTimeAsDays == 2 && x.DurationAsDays == 7 && x.CPY_CreditorType == "CTO"));
			}

			strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var matchResultMock1 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock1.Setup(x => x.FreeDays).Returns(3);
				var matchResultMock2 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock2.Setup(x => x.FreeDays).Returns(0);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(matchResultMock1.Object);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CTO", It.IsAny<ZString>(), null)).Returns(matchResultMock2.Object);
				container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLAvailable, "CTO Available"));
				consol.Containers.Add(container);
				container.JC_FCLAvailable = new ZDateTime(2020, 3, 1);
				container.JC_FCLWharfGateOut = new ZDateTime(2020, 3, 9);
				AssertEquals(1, container.ImportPenalties.Count);
				Assert(container.ImportPenalties.Any(x => x.FreeTimeAsDays == 0 && x.DurationAsDays == 9 && x.CPY_CreditorType == "CTO"));
			}

			strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var matchResultMock1 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock1.Setup(x => x.FreeDays).Returns(0);
				var matchResultMock2 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock2.Setup(x => x.FreeDays).Returns(3);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(matchResultMock1.Object);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CTO", It.IsAny<ZString>(), null)).Returns(matchResultMock2.Object);
				container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLAvailable, "CTO Available"));
				consol.Containers.Add(container);
				container.JC_FCLAvailable = new ZDateTime(2020, 4, 1);
				container.JC_FCLWharfGateOut = new ZDateTime(2020, 4, 9);
				AssertEquals(1, container.ImportPenalties.Count);
				Assert(container.ImportPenalties.Any(x => x.FreeTimeAsDays == 3 && x.DurationAsDays == 6 && x.CPY_CreditorType == "CTO"));
			}

			strategy = new Mock<IContainerDefaultingStrategy>();
			using (ObjectFactory.Substitute("ForwardingContainerDefaultingStrategy", strategy.Object))
			{
				var matchResultMock1 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock1.Setup(x => x.FreeDays).Returns(0);
				var matchResultMock2 = new Mock<IContainerPenaltyMatchResult>();
				matchResultMock2.Setup(x => x.FreeDays).Returns(0);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CAR", It.IsAny<ZString>(), null)).Returns(matchResultMock1.Object);
				strategy.Setup(x => x.GetMatchedStoragePenalty(It.IsAny<ZString>(), "CTO", It.IsAny<ZString>(), null)).Returns(matchResultMock2.Object);
				container = Factory.New<IForwardingContainer>() as CommonContainer;
				strategy.Setup(x => x.CalculateStorageStartAvailableDate(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => container.JC_FCLAvailable);
				strategy.Setup(x => x.CalculateAvailableDateForStorage(It.IsAny<ZString>(), It.IsAny<ZString>(), It.IsAny<ZString>(), null)).Returns(() => new ContainerPenaltyDate(container.JC_FCLAvailable, "CTO Available"));
				consol.Containers.Add(container);
				container.JC_FCLAvailable = new ZDateTime(2020, 5, 1);
				container.JC_ArrivalCTOStorageStartDate = new ZDateTime(2020, 5, 1);
				container.JC_FCLWharfGateOut = new ZDateTime(2020, 5, 9);
				AssertEquals(1, container.ImportPenalties.Count);
				Assert(container.ImportPenalties.Any(x => x.FreeTimeAsDays == 0 && x.DurationAsDays == 9 && x.CPY_CreditorType == "CTO"));
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
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = address.PK;

			return consol;
		}
	}
}
