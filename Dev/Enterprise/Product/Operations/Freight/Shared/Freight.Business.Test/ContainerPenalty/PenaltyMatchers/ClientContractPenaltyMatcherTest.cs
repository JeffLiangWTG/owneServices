using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ClientContractPenaltyMatcherTest : ContractPenaltyMatcherTest
	{
		#region Testing Match Order

		public void TestMatchDetentionOrder()
		{
			ResetForTest();
			SetContractProperties();
			SetMatchingFilterProperties();

			var matcher = GetNewMatcher();

			var detentionWithOriginCountry = AddNewContractDetention(origin: "AU");
			var result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should fall back to load country if no exact load port match exists.", detentionWithOriginCountry.PK, result.PK);

			var detentionWithOriginPort = AddNewContractDetention(origin: "AUSYD");
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match load port over load country.", detentionWithOriginPort.PK, result.PK);

			var detentionWithLocationCountry = AddNewContractDetention(location: "NZ");
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location country over load port/country.", detentionWithLocationCountry.PK, result.PK);

			var detentionWithLocationPort = AddNewContractDetention(location: "NZAKL");
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location port over location country and load port/country.", detentionWithLocationPort.PK, result.PK);

			var detentionWithContainerClass = AddNewContractDetention(containerClass: "40G");
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over location port/country and load port/country.", detentionWithContainerClass.PK, result.PK);

			AddNewContractDetention(origin: "AUSYD", location: "NZAKL");
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over multiple other matching criteria.", detentionWithContainerClass.PK, result.PK);

			var detentionWithContainerClassAndOriginCountry = AddNewContractDetention(containerClass: "40G", origin: "AU");
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class and origin country over just container class.", detentionWithContainerClassAndOriginCountry.PK, result.PK);

			var detentionWithContainerClassAndOriginCountryAndCarrier = AddNewContractDetention(containerClass: "40G", origin: "AU", carrierPK: Filter.Carrier.PK);
			result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match carrier over no carrier.", detentionWithContainerClassAndOriginCountryAndCarrier.PK, result.PK);
		}

		public void TestMatchStorageOrder_Import()
		{
			ResetForTest();
			SetContractProperties();
			SetMatchingFilterProperties();

			var penaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			var matcher = GetNewMatcher();

			var detentionWithOriginCountry = AddNewContractDetention(penaltyType, origin: "AU");
			var result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should fall back to load country if no exact load port match exists.", detentionWithOriginCountry.PK, result.PK);

			var detentionWithOriginPort = AddNewContractDetention(penaltyType, origin: "AUSYD");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match load port over load country.", detentionWithOriginPort.PK, result.PK);

			var detentionWithLocationCountry = AddNewContractDetention(penaltyType, location: "NZ");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location country over load port/country.", detentionWithLocationCountry.PK, result.PK);

			var detentionWithLocationPort = AddNewContractDetention(penaltyType, location: "NZAKL");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location port over location country and load port/country.", detentionWithLocationPort.PK, result.PK);

			var detentionWithContainerClass = AddNewContractDetention(penaltyType, containerClass: "40G");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over location port/country and load port/country.", detentionWithContainerClass.PK, result.PK);

			AddNewContractDetention(penaltyType, origin: "AUSYD", location: "NZAKL");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over multiple other matching criteria.", detentionWithContainerClass.PK, result.PK);

			var detentionWithContainerClassAndOriginCountry = AddNewContractDetention(penaltyType, containerClass: "40G", origin: "AU");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class and origin country over just container class.", detentionWithContainerClassAndOriginCountry.PK, result.PK);

			var detentionWithContainerClassAndOriginCountryAndCarrier = AddNewContractDetention(penaltyType, containerClass: "40G", origin: "AU", carrierPK: Filter.Carrier.PK);
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match carrier over no carrier.", detentionWithContainerClassAndOriginCountryAndCarrier.PK, result.PK);
		}

		public void TestMatchStorageOrder_Export()
		{
			ResetForTest();
			SetContractProperties();
			SetMatchingFilterProperties();
			Filter.Direction = Constants.ContainerDetentionDirection.Export;

			var penaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			var direction = Constants.ContainerDetentionDirection.Export;
			var matcher = GetNewMatcher();

			var detentionWithLocationCountry = AddNewContractDetention(penaltyType, direction, location: "NZ");
			var result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should fall back to location country if no exact location port exists.", detentionWithLocationCountry.PK, result.PK);

			var detentionWithLocationPort = AddNewContractDetention(penaltyType, direction, location: "NZAKL");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location port over location country.", detentionWithLocationPort.PK, result.PK);

			var detentionWithContainerClass = AddNewContractDetention(penaltyType, direction, containerClass: "40G");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over location port/country.", detentionWithContainerClass.PK, result.PK);

			var detentionWithContainerClassAndOriginCountry = AddNewContractDetention(penaltyType, direction, containerClass: "40G", location: "NZ");
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class and location country over just container class.", detentionWithContainerClassAndOriginCountry.PK, result.PK);

			var detentionWithContainerClassAndOriginCountryAndCarrier = AddNewContractDetention(penaltyType, direction, containerClass: "40G", location: "NZ", carrierPK: Filter.Carrier.PK);
			result = matcher.MatchStorage(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match carrier over no carrier.", detentionWithContainerClassAndOriginCountryAndCarrier.PK, result.PK);
		}

		public void TestMatchMDDOrder()
		{
			ResetForTest();
			SetContractProperties();
			SetMatchingFilterProperties();

			var matcher = GetNewMatcher();

			var detentionWithOriginCountry = AddNewContractMDD(origin: "AU");
			var result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should fall back to load country if no exact load port match exists.", detentionWithOriginCountry.PK, result.PK);

			var detentionWithOriginPort = AddNewContractMDD(origin: "AUSYD");
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match load port over load country.", detentionWithOriginPort.PK, result.PK);

			var detentionWithLocationCountry = AddNewContractMDD(location: "NZ");
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location country over load port/country.", detentionWithLocationCountry.PK, result.PK);

			var detentionWithLocationPort = AddNewContractMDD(location: "NZAKL");
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match location port over location country and load port/country.", detentionWithLocationPort.PK, result.PK);

			var detentionWithContainerClass = AddNewContractMDD(containerClass: "40G");
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over location port/country and load port/country.", detentionWithContainerClass.PK, result.PK);

			AddNewContractMDD(origin: "AUSYD", location: "NZAKL");
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class over multiple other matching criteria.", detentionWithContainerClass.PK, result.PK);

			var detentionWithContainerClassAndOriginCountry = AddNewContractMDD(containerClass: "40G", origin: "AU");
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match container class and origin country over just container class.", detentionWithContainerClassAndOriginCountry.PK, result.PK);

			var detentionWithContainerClassAndOriginCountryAndCarrier = AddNewContractMDD(containerClass: "40G", origin: "AU", carrierPK: Filter.Carrier.PK);
			result = matcher.MatchMDD(Filter) as IRatingContractContainerDetention;
			AssertEquals("Matcher should match carrier over no carrier.", detentionWithContainerClassAndOriginCountryAndCarrier.PK, result.PK);
		}

		public void TestMatchContractOrder()
		{
			ResetForTest();
			SetContractProperties();
			SetMatchingFilterProperties();
			SetFilterContractNumber("CN69420");

			var consignorContract = CreateNewContract("CN69420", ConsignorOrg.PK);
			var consignorContractDetention = AddNewContractDetention(containerClass: "40G", contractPK: consignorContract.PK);

			var localClientContract = CreateNewContract(consignorContract.RCT_ContractNumber, ContractOrg.PK);
			var localClientContractDetention = AddNewContractDetention(containerClass: "40G", contractPK: localClientContract.PK);
			var localClientContractDetentionWithCarrier = AddNewContractDetention(containerClass: "40G", contractPK: localClientContract.PK, carrierPK: Filter.Carrier.PK);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var shipment = GetShipment(Filter);
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDischargePort = "NZAKL";
				AssertEquals("Precondition: shipment should be an export.", Directions.Export, shipment?.JobDirection);

				var matcher = GetNewMatcher();
				var result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
				AssertEquals("When a matching contract for Local Client and Carrier exists, matcher should use both Local Client and Carrier.", localClientContractDetentionWithCarrier.PK, result?.PK);

				localClientContractDetentionWithCarrier.RCD_OH_Client = ZGuid.NewZGuid();
				result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
				AssertEquals("When a matching contract for Local Client exists, it should be prioritised.", localClientContractDetention.PK, result?.PK);

				localClientContract.RCT_OH = ZGuid.NewZGuid();
				result = matcher.MatchDetention(Filter) as IRatingContractContainerDetention;
				AssertEquals("When no matching contract for Local Client, matcher should use the client next in priority.", consignorContractDetention.PK, result?.PK);
			}
		}

		#endregion

		#region Contract Penalty Free Days Defaulting with Multiple Shipments

		public void TestMatchImportPenalty_WithMultipleShipments()
		{
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.DET, ContainerDetentionDirection.Import);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.STO, ContainerDetentionDirection.Import);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.MDD, ContainerDetentionDirection.Import);
		}

		public void TestMatchExportPenalty_WithMultipleShipments()
		{
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.DET, ContainerDetentionDirection.Export);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.STO, ContainerDetentionDirection.Export);
			AssertMatchPenalty_WithMultipleShipments(ContainerDetentionPenaltyType.MDD, ContainerDetentionDirection.Export);
		}

		void AssertMatchPenalty_WithMultipleShipments(ZString penaltyType, ZString direction)
		{
			var consol = CreateConsolWithMultipleShipments(3);

			try
			{
				var shipment1 = consol.Shipments[0];
				var shipment2 = consol.Shipments[1];
				var shipment3 = consol.Shipments[2];

				var shipmentClient1 = direction == ContainerDetentionDirection.Export ? shipment1.Consignor : shipment1.Consignee;
				var shipmentClient2 = direction == ContainerDetentionDirection.Export ? shipment2.Consignor : shipment2.Consignee;
				var shipmentClient3 = direction == ContainerDetentionDirection.Export ? shipment3.Consignor : shipment3.Consignee;

				using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
				{
					var orgDetention = consol.ShippingLine.CarrierContainerPenalties.AddNew();
					orgDetention.PD_FreeDays = 3;
					orgDetention.PD_PenaltyType = penaltyType;
					orgDetention.PD_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					orgDetention.PD_Direction = direction;
					orgDetention.PD_OH_Client = shipmentClient1.PK;

					var consolPenalty = direction == ContainerDetentionDirection.Export ? consol.Containers[0].ExportPenalties.AddNew() : consol.Containers[0].ImportPenalties.AddNew();
					consolPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					consolPenalty.CPY_JC_Container = consol.Containers[0].PK;
					consolPenalty.CPY_PenaltyType = penaltyType;
					AssertEquals("Precondition: Consol Penalty Free Days should default to the Client Organization Detention", (ZByte)3, consolPenalty.FreeTimeAsDays);

					var ratingContract1 = CreateNewContract("CN001", shipmentClient2.PK);
					var ratingContract2 = CreateNewContract("CN002", shipmentClient3.PK);
					AddNewContractDetention(penaltyType, direction, 4, ZString.Empty, ZString.Empty, ZString.Empty, ratingContract1.PK, consol.ShippingLinePK);
					AddNewContractDetention(penaltyType, direction, 5, ZString.Empty, ZString.Empty, ZString.Empty, ratingContract2.PK, consol.ShippingLinePK);

					var shipmentPenalty1 = direction == ContainerDetentionDirection.Export ? shipment1.PickupPenalties.AddNew() : shipment1.DeliveryPenalties.AddNew();
					shipmentPenalty1.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					shipmentPenalty1.CPY_JC_Container = consol.Containers[0].PK;
					shipmentPenalty1.CPY_PenaltyType = penaltyType;
					AssertEquals("Shipment Penalty Free Days should default to the Client Organization Detention", (ZByte)3, shipmentPenalty1.FreeTimeAsDays);

					shipment2.ShipmentJobHeader.JH_ClientContractNumber = "CN001";
					var shipmentPenalty2 = direction == ContainerDetentionDirection.Export ? shipment2.PickupPenalties.AddNew() : shipment2.DeliveryPenalties.AddNew();
					shipmentPenalty2.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					shipmentPenalty2.CPY_JC_Container = consol.Containers[0].PK;
					shipmentPenalty2.CPY_PenaltyType = penaltyType;
					AssertEquals("Shipment Penalty Free Days should default to Client Contract CN001 Detention", (ZByte)4, shipmentPenalty2.FreeTimeAsDays);

					shipment3.ShipmentJobHeader.JH_ClientContractNumber = "CN002";
					var shipmentPenalty3 = direction == ContainerDetentionDirection.Export ? shipment3.PickupPenalties.AddNew() : shipment3.DeliveryPenalties.AddNew();
					shipmentPenalty3.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					shipmentPenalty3.CPY_JC_Container = consol.Containers[0].PK;
					shipmentPenalty3.CPY_PenaltyType = penaltyType;
					AssertEquals("Shipment Penalty Free Days should default to Client Contract CN002 Detention", (ZByte)5, shipmentPenalty3.FreeTimeAsDays);
				}
			}
			finally
			{
				foreach (CommonShipment shipment in consol.Shipments)
				{
					if (shipment.Job != null)
					{
						shipment.Job.Dispose();
					}
				}
			}
		}

		#endregion

		#region Test Order
		public void TestMDDPriorityForExport()
		{
			TestCase("CLAG00000001", true, false, ContainerPenaltyPenaltyType.Codes.Detention);
			TestCase("CLAG00000002", true, true, ContainerPenaltyPenaltyType.Codes.Detention);
			TestCase("CLAG00000003", false, true, ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			void TestCase(string contractNo, bool detentionHasCarrier, bool mddHasCarrier, string expectedPenaltyCode)
			{
				ResetForTest();

				var consol = CreateConsolWithMultipleShipments(1);
				var consolShipment = consol.Shipments.First();
				var shipment = Factory.Load<CommonShipment>(consolShipment.PK);
				var packLine = (PackLine)shipment.OuterPackLines.First();
				packLine.JL_PackageCount = 20;

				var contract = CreateNewContract(contractNo, shipment.ConsignorPK);
				var container = consol.Containers[0];
				var jobHeader = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
				jobHeader.JH_ClientContractNumber = contract.RCT_ContractNumber;

				if (detentionHasCarrier)
				{
					AddNewContractDetention(contractPK: contract.PK, carrierPK: consol.ShippingLinePK, origin: "AU", direction: ContainerDetentionDirection.Export);
					AddNewContractDetention(penaltyType: ContainerPenaltyPenaltyType.Codes.Storage, contractPK: contract.PK, carrierPK: consol.ShippingLinePK, origin: "AU", direction: ContainerDetentionDirection.Export);
				}
				else
				{
					AddNewContractDetention(contractPK: contract.PK, origin: "AU", direction: ContainerDetentionDirection.Export);
					AddNewContractDetention(penaltyType: ContainerPenaltyPenaltyType.Codes.Storage, contractPK: contract.PK, origin: "AU", direction: ContainerDetentionDirection.Export);
				}

				if (mddHasCarrier)
				{
					AddNewContractMDD(contractPK: contract.PK, carrierPK: consol.ShippingLinePK, origin: "AU", direction: ContainerDetentionDirection.Export);
				}
				else
				{
					AddNewContractMDD(contractPK: contract.PK, origin: "AU", direction: ContainerDetentionDirection.Export);
				}

				Factory.Save();

				container.JC_FCLWharfGateIn = contract.RCT_EndDate.AddDays(-5);
				Factory.Save();

				Assert(shipment.PickupPenalties.Any(p => p.CPY_PenaltyType == expectedPenaltyCode && p.CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier));
			}
		}

		public void TestMDDPriorityForImport()
		{
			TestCase("CLAG00000001", true, false, ContainerPenaltyPenaltyType.Codes.Detention);
			TestCase("CLAG00000002", true, true, ContainerPenaltyPenaltyType.Codes.Detention);
			TestCase("CLAG00000003", false, true, ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention);
			void TestCase(string contractNo, bool detentionHasCarrier, bool mddHasCarrier, string expectedPenaltyCode)
			{
				ResetForTest();

				var consol = CreateConsolWithMultipleShipments(1);
				var consolShipment = consol.Shipments.First();
				var shipment = Factory.Load<CommonShipment>(consolShipment.PK);
				var packLine = (PackLine)shipment.OuterPackLines.First();
				packLine.JL_PackageCount = 20;

				var contract = CreateNewContract(contractNo, shipment.ConsigneePK);
				var container = consol.Containers[0];
				var jobHeader = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
				jobHeader.JH_ClientContractNumber = contract.RCT_ContractNumber;

				if (detentionHasCarrier)
				{
					AddNewContractDetention(contractPK: contract.PK, carrierPK: consol.ShippingLinePK, origin: "AU");
					AddNewContractDetention(penaltyType: ContainerPenaltyPenaltyType.Codes.Storage, contractPK: contract.PK, carrierPK: consol.ShippingLinePK, origin: "AU");
				}
				else
				{
					AddNewContractDetention(contractPK: contract.PK, origin: "AU");
					AddNewContractDetention(penaltyType: ContainerPenaltyPenaltyType.Codes.Storage, contractPK: contract.PK, origin: "AU");
				}

				if (mddHasCarrier)
				{
					AddNewContractMDD(contractPK: contract.PK, carrierPK: consol.ShippingLinePK, origin: "AU");
				}
				else
				{
					AddNewContractMDD(contractPK: contract.PK, origin: "AU");
				}
				Factory.Save();

				container.JC_FCLWharfGateOut = contract.RCT_EndDate.AddDays(-5);
				Factory.Save();

				AssertEquals(shipment.DeliveryPenalties.Any(p => p.CPY_PenaltyType == expectedPenaltyCode && p.CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier), true);
			}
		}
		#endregion

		#region Implementation

		IRatingContractContainerDetention AddNewContractMDD(ZString direction = default, ZByte freeDays = default, ZString origin = default, ZString location = default, ZString containerClass = default, ZGuid? contractPK = null, ZGuid? carrierPK = null)
		{
			if (direction.IsEmpty)
			{
				direction = Constants.ContainerDetentionDirection.Import;
			}

			return AddNewContractPenalty(Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, direction, freeDays, origin, location, containerClass, contractPK, carrierPK);
		}

		IRatingContractContainerDetention AddNewContractDetention(ZString penaltyType = default, ZString direction = default, ZByte freeDays = default, ZString origin = default, ZString location = default, ZString containerClass = default, ZGuid? contractPK = null, ZGuid? carrierPK = null)
		{
			if (penaltyType.IsEmpty)
			{
				penaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			}

			if (direction.IsEmpty)
			{
				direction = Constants.ContainerDetentionDirection.Import;
			}

			return AddNewContractPenalty(penaltyType, direction, freeDays, origin, location, containerClass, contractPK, carrierPK);
		}

		IRatingContractContainerDetention AddNewContractPenalty(ZString penaltyType = default, ZString direction = default, ZByte freeDays = default, ZString origin = default, ZString location = default, ZString containerClass = default, ZGuid? contractPK = null, ZGuid? carrierPK = null)
		{
			var detention = Factory.New<IRatingContractContainerDetention>();
			detention.RCD_RCT = contractPK ?? Contract.PK;
			detention.RCD_PenaltyType = penaltyType;
			detention.RCD_Direction = direction;
			detention.RCD_OriginPortOrCountry = origin;
			detention.RCD_DetentionPortOrCountry = location;
			detention.RCD_ContainerType = containerClass;
			detention.RCD_OH_Client = carrierPK ?? ZGuid.Empty;
			detention.RCD_FreeDays = freeDays;

			return detention;
		}

		IRatingContract CreateNewContract(string number, ZGuid orgPK)
		{
			var contract = Factory.New<IRatingContract>();
			contract.RCT_OH = orgPK;
			contract.RCT_StartDate = ZDate.Today.AddDays(-15);
			contract.RCT_ContractNumber = number;
			contract.RCT_EndDate = ZDate.Today.AddDays(15);
			contract.RCT_ContractType = ExpectedContractType;
			contract.RCT_IsActive = true;
			contract.RCT_GS_NKContractOwner = "USR";

			return contract;
		}

		protected override ContractPenaltyMatcher GetNewMatcher() => new ClientContractPenaltyMatcher();

		protected override string ExpectedContractType => Constants.RatingContractTypes.Client;

		protected override string[] ValidProcessTypes => new string[]
		{
			Constants.ContainerPenaltyProcessType.Pickup,
			Constants.ContainerPenaltyProcessType.Delivery
		};

		protected override void SetMatchingFilterProperties()
		{
			base.SetMatchingFilterProperties();

			var shipment = GetShipment(Filter);
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.ConsigneePK = ConsigneeOrg.PK;
		}

		OrgHeader ConsignorOrg => consignorOrg ?? (consignorOrg = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader consignorOrg;

		OrgHeader ConsigneeOrg => consigneeOrg ?? (consigneeOrg = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader consigneeOrg;

		OrgHeader CarrierOrg => carrierOrg ?? (carrierOrg = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader carrierOrg;

		protected override void SetupContainerAndRelatedBizos()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_ShippingLineAddress = CarrierOrg.MainAddress.PK;
			consol.JK_TransportMode = TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			var jobHeader = new JobHeader.Loader(Factory, shipment).TryLoadOrCreate();
			jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var container = consol.Containers.AddNew();
			Filter.Container = container;
			Filter.Carrier = consol.ShippingLine;
		}

		protected override void SetFilterDepartureDateTime(ZDateTime value)
			=> GetShipment(Filter).JS_E_DEP = value;

		protected override void SetFilterContractOrg(OrgHeader value)
			=> GetShipment(Filter).ShipmentJobHeader.LocalChargesPK = value.PK;

		protected override void SetFilterContractNumber(ZString value)
			=> GetShipment(Filter).ShipmentJobHeader.JH_ClientContractNumber = value;

		CommonShipment GetShipment(ContainerPenaltyMatchFilter filter) => ((CommonContainer)filter.Container).GetRelatedShipmentsForPenaltyDefaulting(filter.ProcessType).FirstOrDefault();

		CommonConsol CreateConsolWithMultipleShipments(int shipmentCount)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<IForwardingConsol>();
			var commonConsol = consol as CommonConsol;
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			(consol.Transports_Get(0) as Transport).JW_ATD = ZDateTime.Today;

			var container = consol.Containers.AddNew();

			for (int i = 0; i < shipmentCount; i++)
			{
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = $"CONSIGNEE {i}";
				consignee.MainAddress.OA_Address1 = $"Consignee Address {i}";
				consignee.OH_IsConsignee = true;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_FullName = $"CONSIGNOR {i}";
				consignor.MainAddress.OA_Address1 = $"Consignor Address {i}";
				consignor.OH_IsConsignor = true;

				var shipment = Factory.New<IForwardingShipment>();
				consol.AddShipment(shipment);
				shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
				shipment.JS_E_DEP = ZDateTime.Today;
				shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;
				shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;

				var commonShipment = shipment as CommonShipment;
				commonShipment.CreateShipmentJobHeaderWithMutex();
				var localClient = Factory.NewWithValidTestData<OrgHeader>();
				commonShipment.ShipmentJobHeader.LocalChargesPK = localClient.PK;

				var packLine = commonShipment.OuterPackLines.AddNew();
				var commonContainer = container as CommonContainer;
				packLine.SetContainer(commonConsol, commonContainer);
			}

			return commonConsol;
		}

		#endregion
	}
}
