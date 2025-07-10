using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ContainerPenalty))]
	sealed class ContainerPenaltyBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDetentionDirection()
		{
			AssertEquals(ContainerDetentionDirection.Export, ContainerPenalty.GetDirection(ContainerPenaltyProcessType.Export));
			AssertEquals(ContainerDetentionDirection.Export, ContainerPenalty.GetDirection(ContainerPenaltyProcessType.Pickup));
			AssertEquals(ContainerDetentionDirection.Import, ContainerPenalty.GetDirection(ContainerPenaltyProcessType.Import));
			AssertEquals(ContainerDetentionDirection.Import, ContainerPenalty.GetDirection(ContainerPenaltyProcessType.Delivery));
		}

		public void TestDefaultCreditorFromRelatedPartiesWhenConsolIsExport()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "NZAKA";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			var relatedPartyForDeparture = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForDeparture.OH_IsCreditor = true;

			departureCTO.SetRelatedParty(relatedPartyForDeparture, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");
			Factory.Save();

			var departurePackCFSTransport = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_DeparturePackCFSTransportAddress = departurePackCFSTransport.MainAddress.PK;

			var relatedPartyForTransport = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForTransport.OH_IsCreditor = true;

			departurePackCFSTransport.SetRelatedParty(relatedPartyForTransport, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");
			Factory.Save();

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());
			shippingLine.OH_IsCreditor = false;

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;

				AssertEquals(relatedPartyForDeparture.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
				AssertEquals(relatedPartyForTransport.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(ZGuid.Empty, penalty.Creditor?.PK ?? ZGuid.Empty);
			}

			shippingLine.OH_IsCreditor = true;
			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(shippingLine.PK, penalty.Creditor.PK);
			}

			var relatedPartyForshippingLinePartiesPAD = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForshippingLinePartiesPAD.OH_IsCreditor = true;

			var shippingLineParties = shippingLine.AllRelatedParties;
			shippingLineParties.SetRelatedParty(relatedPartyForshippingLinePartiesPAD, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");

			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyForshippingLinePartiesPAD.PK, penalty.Creditor.PK);
			}

			var relatedPartyForshippingLinePartiesPIC = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForshippingLinePartiesPIC.OH_IsCreditor = true;
			shippingLineParties.SetRelatedParty(relatedPartyForshippingLinePartiesPIC, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");

			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" }) // Pickup takes priority over Pickup And Delivery
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyForshippingLinePartiesPIC.PK, penalty.Creditor.PK);
			}
		}

		public void TestDefaultCreditorFromRelatedPartiesWhenConsolIsImport()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			shippingLine.OH_IsCreditor = false;

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			var relatedPartyForArrival = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForArrival.OH_IsCreditor = true;

			var arrivalParties = arrivalCTO.AllRelatedParties;
			arrivalParties.SetRelatedParty(relatedPartyForArrival, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");

			Factory.Save();

			var arrivalUnpackCFSTransport = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalUnpackCFSTransport.MainAddress.PK;

			var relatedPartyForTransport = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForTransport.OH_IsCreditor = true;

			var portParties = arrivalUnpackCFSTransport.AllRelatedParties;
			portParties.SetRelatedParty(relatedPartyForTransport, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");

			Factory.Save();

			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;

				AssertEquals(relatedPartyForArrival.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
				AssertEquals(relatedPartyForTransport.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(ZGuid.Empty, penalty.Creditor?.PK ?? ZGuid.Empty);
			}

			shippingLine.OH_IsCreditor = true;
			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(shippingLine.PK, penalty.Creditor.PK);
			}

			var relatedPartyForshippingLinePartiesPAD = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForshippingLinePartiesPAD.OH_IsCreditor = true;

			var shippingLineParties = shippingLine.AllRelatedParties;
			shippingLineParties.SetRelatedParty(relatedPartyForshippingLinePartiesPAD, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");

			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyForshippingLinePartiesPAD.PK, penalty.Creditor.PK);
			}

			var relatedPartyForshippingLinePartiesDLV = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForshippingLinePartiesDLV.OH_IsCreditor = true;

			shippingLineParties.SetRelatedParty(relatedPartyForshippingLinePartiesDLV, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery, consol.JK_TransportMode, consol.JK_ConsolMode, "AUMEL");

			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" }) // Delivery takes priority over Pickup And Delivery
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_RL_NKLocation = "AUMEL";
				penalty.CPY_PenaltyType = penaltyType;
				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyForshippingLinePartiesDLV.PK, penalty.Creditor.PK);
			}
		}

		public void TestDefaultCreditor()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var departureCTO = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			var departurePackCFSTransport = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_DeparturePackCFSTransportAddress = departurePackCFSTransport.MainAddress.PK;

			var arrivalCTO = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;

			var arrivalUnpackCFSTransport = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalUnpackCFSTransport.MainAddress.PK;

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				AssertEquals(arrivalCTO.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(shippingLine.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
				AssertEquals(arrivalUnpackCFSTransport.PK, penalty.Creditor.PK);

				penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				AssertEquals(departureCTO.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier; // when Consol is import and we update export penalties, we do NOT use SPC default creditor
				AssertEquals(creditor.PK, penalty.Creditor.PK);

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
				AssertEquals(departurePackCFSTransport.PK, penalty.Creditor.PK);
			}

			consol.JK_AgentType = AgentType.CoLoad;

			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(creditor.PK, penalty.Creditor.PK);

				penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(shippingLine.PK, penalty.Creditor.PK);
			}

			consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;

			creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(creditor.PK, penalty.Creditor.PK);

				penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(shippingLine.PK, penalty.Creditor.PK); // when Consol is import and we update export penalties, we do NOT use SPC default creditor
			}

			var relatedPartyPAD = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyPAD.OH_IsCreditor = true;
			creditor.SetRelatedParty(relatedPartyPAD, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode);

			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyPAD.PK, penalty.Creditor.PK);
			}

			var relatedPartyDLV = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyDLV.OH_IsCreditor = true;
			creditor.SetRelatedParty(relatedPartyDLV, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery, consol.JK_TransportMode, consol.JK_ConsolMode);

			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ImportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyDLV.PK, penalty.Creditor.PK);
			}

			consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(creditor.PK, penalty.Creditor.PK);
			}

			relatedPartyPAD = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyPAD.OH_IsCreditor = true;

			creditor.SetRelatedParty(relatedPartyPAD, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_TransportMode, consol.JK_ConsolMode);
			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyPAD.PK, penalty.Creditor.PK);
			}

			var relatedPartyPIC = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyPIC.OH_IsCreditor = true;

			creditor.SetRelatedParty(relatedPartyPIC, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup, consol.JK_TransportMode, consol.JK_ConsolMode);
			Factory.Save();

			foreach (var penaltyType in new string[] { "STO", "DET", "TWT" })
			{
				var container = consol.Containers.AddNew();
				var penalty = container.ExportPenalties.AddNew();
				penalty.CPY_PenaltyType = penaltyType;

				penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				AssertEquals(relatedPartyPIC.PK, penalty.Creditor.PK);
			}
		}

		public void TestDefaultCreditor_PenaltyTypeMDD()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = AgentType.Agent;

			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			shippingLine.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var container = consol.Containers.AddNew();
			var penalty1 = container.ImportPenalties.AddNew();
			penalty1.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			AssertEquals(shippingLine.PK, penalty1.Creditor.PK);

			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;
			var penalty2 = container.ImportPenalties.AddNew();
			penalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			AssertEquals(shippingLine.PK, penalty2.Creditor.PK);

			consol.JK_OA_CreditorAddress_ZAddress.OrgPK = creditor.PK;
			consol.JK_AgentType = AgentType.CoLoad;
			var penalty3 = container.ImportPenalties.AddNew();
			penalty3.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			AssertEquals(creditor.PK, penalty3.Creditor.PK);

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "HKHKG";

			var penalty4 = container.ImportPenalties.AddNew();
			penalty4.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			AssertEquals(shippingLine.PK, penalty4.Creditor.PK);
		}

		public void TestDurationAsDays()
		{
			var now = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();

			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

			penalty.CPY_Duration = new ZDateTime(now.Year, 1, 11);
			AssertEquals((ZByte)10, penalty.DurationAsDays);

			penalty.CPY_Duration = new ZDateTime(now.Year, 1, 1);
			AssertEquals((ZByte)byte.MinValue, penalty.DurationAsDays);

			penalty.CPY_Duration = new ZDateTime(now.Year, 12, 1);
			AssertEquals((ZByte)byte.MaxValue, penalty.DurationAsDays);

			penalty.DurationAsDays = 20;
			AssertEquals((ZDateTime)TimeSpan.FromDays(20), penalty.CPY_Duration);

			penalty.DurationAsDays = byte.MinValue;
			AssertEquals(ZDateTime.Empty, penalty.CPY_Duration);

			penalty.DurationAsDays = byte.MaxValue;
			AssertEquals((ZDateTime)TimeSpan.FromDays(byte.MaxValue), penalty.CPY_Duration);
		}

		public void TestFreeTimeAsDays()
		{
			var now = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();

			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

			penalty.CPY_FreeTime = new ZDateTime(now.Year, 1, 11);
			AssertEquals((ZByte)10, penalty.FreeTimeAsDays);

			penalty.CPY_FreeTime = new ZDateTime(now.Year, 1, 1);
			AssertEquals((ZByte)byte.MinValue, penalty.FreeTimeAsDays);

			penalty.CPY_FreeTime = new ZDateTime(now.Year, 12, 1);
			AssertEquals((ZByte)byte.MaxValue, penalty.FreeTimeAsDays);

			penalty.FreeTimeAsDays = 20;
			AssertEquals((ZDateTime)TimeSpan.FromDays(20), penalty.CPY_FreeTime);

			penalty.FreeTimeAsDays = byte.MinValue;
			AssertEquals(ZDateTime.Empty, penalty.CPY_FreeTime);

			penalty.FreeTimeAsDays = byte.MaxValue;
			AssertEquals((ZDateTime)TimeSpan.FromDays(byte.MaxValue), penalty.CPY_FreeTime);
		}

		public void TestCPY_PenaltyTypeDefault()
		{
			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_RL_NKLoadPort = "NZAKL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var container1 = consol1.Containers.AddNew();

			var penalty1 = container1.ImportPenalties.AddNew();
			penalty1.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;
			AssertEquals(ContainerPenaltyPenaltyType.Descriptions.Detention, penalty1.PenaltyTypeDescription);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Carrier, penalty1.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Days, penalty1.CPY_TimeUnit);
			AssertEquals("AUSYD", penalty1.CPY_RL_NKLocation);

			var penalty2 = container1.ImportPenalties.AddNew();
			penalty2.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			AssertEquals(ContainerPenaltyPenaltyType.Descriptions.Storage, penalty2.PenaltyTypeDescription);
			AssertEquals(ContainerPenaltyCreditorType.Codes.CTO, penalty2.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Days, penalty2.CPY_TimeUnit);
			AssertEquals("AUSYD", penalty2.CPY_RL_NKLocation);

			var penalty3 = container1.ImportPenalties.AddNew();
			penalty3.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			AssertEquals(ContainerPenaltyPenaltyType.Descriptions.TruckWaitTime, penalty3.PenaltyTypeDescription);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Transport, penalty3.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Hours, penalty3.CPY_TimeUnit);
			AssertEquals("AUSYD", penalty3.CPY_RL_NKLocation);

			var penalty4 = container1.ImportPenalties.AddNew();
			penalty4.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention;
			AssertEquals(ContainerPenaltyPenaltyType.Descriptions.MergedDemurrageAndDetention, penalty4.PenaltyTypeDescription);
			AssertEquals(ContainerPenaltyCreditorType.Codes.Carrier, penalty4.CPY_CreditorType);
			AssertEquals(ContainerPenaltyTimeUnit.Codes.Days, penalty4.CPY_TimeUnit);
			AssertEquals("AUSYD", penalty4.CPY_RL_NKLocation);
		}

		public void TestFreeTimeAsDays_ForStoragePenaltyType()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(CountryCodes.Italy))
			{
				var consol = Factory.New<CommonConsol>();
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var container = consol.Containers.AddNew();

				using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 7 }))
				{
					var importPenalty = container.ImportPenalties.AddNew();
					importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					importPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
					AssertEquals(ZByte.Zero, importPenalty.FreeTimeAsDays);

					importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
					AssertEquals((ZByte)7, importPenalty.FreeTimeAsDays);
				}

				using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 5 }))
				{
					var exportPenalty = container.ExportPenalties.AddNew();
					exportPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
					exportPenalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
					AssertEquals(ZByte.Zero, exportPenalty.FreeTimeAsDays);

					exportPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
					AssertEquals((ZByte)5, exportPenalty.FreeTimeAsDays);
				}
			}
		}

		public void TestCPY_PenaltyTypeDescription()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();

			foreach (var penaltyType in penalty.Lookups.PenaltyTypeList.GetAllCodes())
			{
				foreach (var creditorType in penalty.Lookups.CreditorTypeList.GetAllCodes())
				{
					penalty.CPY_PenaltyType = penaltyType;
					penalty.CPY_CreditorType = creditorType;
					if (penaltyType == ContainerPenaltyPenaltyType.Codes.Storage)
					{
						if (penalty.CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
						{
							AssertEquals("Demurrage", penalty.PenaltyTypeDescription);
						}
						else
						{
							AssertEquals(ContainerPenaltyPenaltyType.Descriptions.Storage, penalty.PenaltyTypeDescription);
						}
					}
					else if (penaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
					{
						AssertEquals(ContainerPenaltyPenaltyType.Descriptions.Detention, penalty.PenaltyTypeDescription);
					}
					else if (penaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
					{
						AssertEquals(ContainerPenaltyPenaltyType.Descriptions.MergedDemurrageAndDetention, penalty.PenaltyTypeDescription);
					}
					else
					{
						AssertEquals(ContainerPenaltyPenaltyType.Descriptions.TruckWaitTime, penalty.PenaltyTypeDescription);
					}
				}
			}
		}

		public void TestTotalCostByModifyingPerUnitCostOrDuration()
		{
			var now = ZDateTime.Now;

			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();

			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;

			penalty.CPY_PerUnitCost = 40m;
			penalty.CPY_Duration = new ZDateTime(now.Year, 1, 16, 0, 0, 0);

			AssertEquals(600m, penalty.CPY_TotalCost);

			penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
			penalty.CPY_Duration = new ZDateTime(now.Year, 1, 16, 0, 0, 0);
			penalty.CPY_PerUnitCost = 20m;
			AssertEquals(300m, penalty.CPY_TotalCost);

			penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
			penalty.CPY_PerUnitCost = 40m;
			penalty.CPY_Duration = new ZDateTime(now.Year, 1, 1, 15, 0, 0);
			AssertEquals(600m, penalty.CPY_TotalCost);

			penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Storage;
			penalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
			penalty.CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
			penalty.CPY_PerUnitCost = 40m;
			penalty.CPY_Duration = new ZDateTime(now.Year, 1, 1, 0, 10, 0);
			AssertEquals(6.67m, penalty.CPY_TotalCost);

			penalty.CPY_PerUnitCost = 10m;
			AssertEquals(6.67m, penalty.CPY_TotalCost);
		}

		public void TestDefaultCurrencyByLocationFallbackToLoginCountryCurrency()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_AgentType = AgentType.Agent;

			var container = consol.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_ProcessType = ContainerPenaltyProcessType.Pickup;
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;

			AssertEquals(CurrencyCodes.NewZealand, penalty.CPY_RX_NKCurrency);

			penalty.CPY_RL_NKLocation = "USLAX";
			AssertEquals(CurrencyCodes.UnitedStates, penalty.CPY_RX_NKCurrency);

			penalty.CPY_RL_NKLocation = "";

			AssertEquals(CurrencyCodes.Australia, penalty.CPY_RX_NKCurrency);
		}

		public void TestPenaltyCurrencyFallsBackToCurrentCompanyCurrency_WhenCountryCurrencyIsNotDefined()
		{
			// Arrange
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_AgentType = AgentType.Agent;

			var container = consol.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_ProcessType = ContainerPenaltyProcessType.Pickup;
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;

			// Act & Assert
			CombineAssertions(() =>
			{
				penalty.CPY_RL_NKLocation = "UAODS";
				AssertEquals("Penalty Currency must be derived from Country Currency Code.",
					expected: CurrencyCodes.Ukraine, actual: penalty.CPY_RX_NKCurrency);

				var antarctica = RefCountry.LoadFromCountryCode(Factory, countryCode: "AQ");
				antarctica.RN_RX_NKLocalCurrency = ZString.Empty;
				penalty.CPY_RL_NKLocation = "AQZGN";
				AssertEquals("Penalty Currency must be derived from Current Company Currency Code when Country Currency Code is not available.",
					expected: CurrencyCodes.Australia, actual: penalty.CPY_RX_NKCurrency);

				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = ZString.Empty;
				penalty.CPY_RL_NKLocation = ZString.Empty;
				AssertEquals("Penalty Currency must have USD fallback value.",
					expected: CurrencyCodes.UnitedStates, actual: penalty.CPY_RX_NKCurrency);
			});
		}

		public void TestPenaltyCurrencyFallsBackToCurrentCompanyCurrency_WhenCurrencyIsNotActive()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "USSAV";
			consol.JK_RL_NKDischargePort = "PACCT";
			consol.JK_AgentType = AgentType.Agent;

			var container = consol.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_ProcessType = ContainerPenaltyProcessType.Pickup;
			penalty.CPY_PenaltyType = ContainerPenaltyPenaltyType.Codes.Detention;

			CombineAssertions(() =>
			{
				penalty.CPY_RL_NKLocation = "PACCT";
				AssertEquals("Precondition: Penalty Currency must be derived from Country Currency Code.",
					expected: CurrencyCodes.Panama, actual: penalty.CPY_RX_NKCurrency);

				var country = RefCountry.LoadFromCountryCode(Factory, countryCode: "PA");
				var currency = country.LocalCurrency;
				currency.RX_IsActive = false;
				penalty.CPY_RL_NKLocation = ZString.Empty;
				penalty.CPY_RL_NKLocation = "PACCT";
				AssertEquals("Penalty Currency must be derived from Current Company Currency Code when Country Currency Code is not available.",
					expected: CurrencyCodes.Australia, actual: penalty.CPY_RX_NKCurrency);

				currency = GlbCompany.CurrentCompany?.LocalCurrency;
				currency.RX_IsActive = false;
				penalty.CPY_RL_NKLocation = ZString.Empty;
				AssertEquals("Penalty Currency must have USD fallback value.",
					expected: CurrencyCodes.UnitedStates, actual: penalty.CPY_RX_NKCurrency);
			});
		}

		public void TestGetFirstFreeDay()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			var penalty = container.ImportPenalties.AddNew();

			penalty.CPY_FirstFreeDay = ZDateTimeOffset.Empty;
			AssertEquals(ZDateTime.Empty, penalty.FirstFreeDay);

			var invalidDateTime = DateTime.MinValue;
			penalty.CPY_FirstFreeDay = new ZDateTimeOffset(invalidDateTime);
			AssertEquals(false, penalty.CPY_FirstFreeDay.IsValid);
			AssertEquals(ZDateTime.Empty, penalty.FirstFreeDay);

			penalty.CPY_FirstFreeDay = new ZDateTimeOffset(2020, 12, 20);
			AssertEquals(new ZDateTime(2020,12,20), penalty.FirstFreeDay);
		}
	}
}
