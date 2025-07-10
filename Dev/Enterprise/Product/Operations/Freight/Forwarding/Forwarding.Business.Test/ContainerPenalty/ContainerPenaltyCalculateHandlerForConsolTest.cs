using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ContainerPenaltyCalculateHandlerForConsolTest : TestCaseWithFactory
	{
		public void TestHandleContainerDateChanging_NotShipperOwned()
		{
			AssertHandleContainerDateChanging(false);
		}

		public void TestHandleContainerDateChanging_ShipperOwned()
		{
			AssertHandleContainerDateChanging(true);
		}

		public void AssertHandleContainerDateChanging(bool containerIsShipperOwned)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();
			var importDetention = carrier.CarrierContainerPenalties.AddNew();
			importDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			importDetention.PD_FreeDays = 1;

			var importStorage = carrier.CarrierContainerPenalties.AddNew();
			importStorage.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importStorage.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			importStorage.PD_FreeDays = 2;

			var exportDetention = carrier.CarrierContainerPenalties.AddNew();
			exportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			exportDetention.PD_FreeDays = 3;

			var exportStorage = carrier.CarrierContainerPenalties.AddNew();
			exportStorage.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportStorage.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			exportStorage.PD_FreeDays = 4;

			var importCTO = Factory.NewWithValidTestData<OrgHeader>();
			importCTO.ServiceImportCTOStorages.AddNew().PD_FreeDays = 5;
			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();
			exportCTO.ServiceExportCTOStorages.AddNew().PD_FreeDays = 6;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;
			var container = consol.Containers.AddNew();
			container.JC_IsShipperOwned = containerIsShipperOwned;
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			var handler = new ContainerPenaltyCalculateHandlerForConsol(container);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
			AssertArrivalDetentionPenalty(1);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
			AssertEquals((ZByte)2, container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertArrivalDetentionPenalty(1);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
			AssertArrivalDetentionPenalty(1);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);
			AssertEquals((ZByte)2, container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertArrivalDetentionPenalty(1);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ArrivalCTOStorageStartDate);
			AssertEquals((ZByte)2, container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateIn);
			AssertEquals((ZByte)4, container.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)6, container.ExportPenalties.FindOrCreateDepartureCTOStoragePenalty(false).FreeTimeAsDays);
			AssertDepartureDetentionPenalty(3);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
			AssertDepartureDetentionPenalty(3);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
			AssertEquals((ZByte)4, container.ExportPenalties.FindOrCreateDepartureCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)6, container.ExportPenalties.FindOrCreateDepartureCTOStoragePenalty(false).FreeTimeAsDays);
			AssertDepartureDetentionPenalty(3);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
			AssertEquals((ZByte)2, container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertArrivalDetentionPenalty(1);

			void AssertArrivalDetentionPenalty(int expectedFreeDaysWhenNotShipper)
			{
				if (container.JC_IsShipperOwned)
				{
					AssertNull(container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false));
				}
				else
				{
					AssertEquals((ZByte)expectedFreeDaysWhenNotShipper, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);
				}
			}

			void AssertDepartureDetentionPenalty(int expectedFreeDaysWhenNotShipper)
			{
				if (container.JC_IsShipperOwned)
				{
					AssertNull(container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false));
				}
				else
				{
					AssertEquals((ZByte)expectedFreeDaysWhenNotShipper, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);
				}
			}
		}

		public void TestHandleContainerDateChanging_MDD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();

			var importMDD = carrier.CarrierContainerPenalties.AddNew();
			importMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDD.PD_FreeDays = 7;

			var exportMDD = carrier.CarrierContainerPenalties.AddNew();
			exportMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			exportMDD.PD_FreeDays = 8;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			var handler = new ContainerPenaltyCalculateHandlerForConsol(container);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
			AssertEquals((ZByte)7, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
			AssertEquals((ZByte)7, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);
			AssertEquals((ZByte)7, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
			AssertEquals((ZByte)8, container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false).FreeTimeAsDays);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
			AssertEquals((ZByte)8, container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false).FreeTimeAsDays);

			container.ImportPenalties.DeleteAll();
			container.ExportPenalties.DeleteAll();
			handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
			AssertEquals((ZByte)7, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);
		}

		public void TestHandleContainerAdded()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();

			var importDetention = carrier.CarrierContainerPenalties.AddNew();
			importDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			importDetention.PD_FreeDays = 1;

			var importStorage = carrier.CarrierContainerPenalties.AddNew();
			importStorage.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importStorage.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			importStorage.PD_FreeDays = 2;

			var exportDetention = carrier.CarrierContainerPenalties.AddNew();
			exportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			exportDetention.PD_FreeDays = 3;

			var exportStorage = carrier.CarrierContainerPenalties.AddNew();
			exportStorage.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportStorage.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			exportStorage.PD_FreeDays = 4;

			var importCTO = Factory.NewWithValidTestData<OrgHeader>();
			importCTO.ServiceImportCTOStorages.AddNew().PD_FreeDays = 5;
			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();
			exportCTO.ServiceExportCTOStorages.AddNew().PD_FreeDays = 6;

			var importMDD = carrier.CarrierContainerPenalties.AddNew();
			importMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDD.PD_FreeDays = 7;

			var exportMDD = carrier.CarrierContainerPenalties.AddNew();
			exportMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			exportMDD.PD_FreeDays = 8;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;

			var container1 = Factory.New<ForwardingContainer>();
			consol.Containers.Add(container1);
			AssertNull(container1.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false));
			AssertNull(container1.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false));
			AssertNull(container1.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			consol.Containers.Add(container2);

			AssertEquals((ZByte)2, container2.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertEquals((ZByte)5, container2.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
			AssertNull(container2.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
		}

		public void TestDefaultingWithDuplicatePenaltyType()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TSTCAR";
			carrier.OH_IsShippingLine = true;
			carrier.CustomsCodes.AddNew("HID", "FWA", "US");
			carrier.OrgFountains.DeleteAll();

			var importDetention = carrier.CarrierContainerPenalties.AddNew();
			importDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			importDetention.PD_FreeDays = 1;

			var exportDetention = carrier.CarrierContainerPenalties.AddNew();
			exportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
			exportDetention.PD_FreeDays = 3;

			var importMDD = carrier.CarrierContainerPenalties.AddNew();
			importMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDD.PD_FreeDays = 7;

			var exportMDD = carrier.CarrierContainerPenalties.AddNew();
			exportMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
			exportMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			exportMDD.PD_FreeDays = 8;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2021, 1, 1);

			var handler = new ContainerPenaltyCalculateHandlerForConsol(container);
			{
				container.ImportPenalties.DeleteAll();
				container.ExportPenalties.DeleteAll();
				var importMddPenalty = container.ImportPenalties.AddNew();
				importMddPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
				importMddPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importMddPenalty.FreeTimeAsDays = 20;

				var importDetentionPenalty = container.ImportPenalties.AddNew();
				importDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				importDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importDetentionPenalty.FreeTimeAsDays = 20;

				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
				AssertEquals("MDD penalty should not default", (ZByte)20, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);
				AssertEquals("DET penalty should not default", (ZByte)20, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.EmptyReturnedBy);
				AssertEquals("MDD penalty should not default", (ZByte)20, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);
				AssertNull("DET penalty should not be created when MDD exists", container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false));
			}
			{
				container.ImportPenalties.DeleteAll();
				container.ExportPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
				var mddPenalty = container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false);
				AssertNull(mddPenalty);
				var detPenalty = container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays;
				AssertEquals((ZByte)1, detPenalty);

				var importDetentionPenalty = container.ImportPenalties.AddNew();
				importDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				importDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importDetentionPenalty.FreeTimeAsDays = 20;
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLWharfGateOut);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
			{
				container.ImportPenalties.DeleteAll();
				container.ExportPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				var importDetentionPenalty = container.ImportPenalties.AddNew();
				importDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				importDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importDetentionPenalty.FreeTimeAsDays = 20;
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyReturnGateIn);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
			{
				container.ImportPenalties.DeleteAll();
				container.ExportPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);

				var importDetentionPenalty = container.ImportPenalties.AddNew();
				importDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				importDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importDetentionPenalty.FreeTimeAsDays = 20;
				container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays = 20;

				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLAvailable);
				AssertNull(container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
				AssertEquals((ZByte)1, container.ImportPenalties.FindOrCreateArrivalCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
			{
				container.ImportPenalties.DeleteAll();
				container.ExportPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);

				var exportDetentionPenalty = container.ExportPenalties.AddNew();
				exportDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				exportDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				exportDetentionPenalty.FreeTimeAsDays = 20;
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.ContainerYardEmptyPickupGateOut);
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
			{
				container.ImportPenalties.DeleteAll();
				container.ExportPenalties.DeleteAll();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);

				var exportDetentionPenalty = container.ExportPenalties.AddNew();
				exportDetentionPenalty.CPY_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				exportDetentionPenalty.CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				exportDetentionPenalty.FreeTimeAsDays = 20;
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);

				container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).Delete();
				handler.HandleContainerDateChanging(ContainerPenaltyRelatedDateType.FCLOnBoardVessel);
				AssertNull(container.ExportPenalties.FindOrCreateDepartureMDDPenalty(false));
				AssertEquals((ZByte)3, container.ExportPenalties.FindOrCreateDepartureCarrierDetentionPenalty(false).FreeTimeAsDays);
			}
		}

		public void TestDefaulting_CarrierMDDAndConsigneeSTOPenalties()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 4 }))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(GlbCompany.CurrentCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 3 }))
			{
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_Code = "TSTCAR";
				carrier.OH_IsShippingLine = true;
				carrier.CustomsCodes.AddNew("HID", "FWA", "US");
				carrier.OrgFountains.DeleteAll();

				var importMDD = carrier.CarrierContainerPenalties.AddNew();
				importMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				importMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
				importMDD.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importMDD.PD_FreeDays = 11;
				importMDD.PD_FirstFreeDayType = "FCD";

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_FullName = "CONSIGNEE";
				consignee.MainAddress.OA_Address1 = "Consignee Address";
				consignee.OH_IsConsignee = true;

				var importSTOConsignee = consignee.ConsigneeContainerPenalties.AddNew();
				importSTOConsignee.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				importSTOConsignee.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
				importSTOConsignee.PD_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				importSTOConsignee.PD_FreeDays = 8;
				importSTOConsignee.PD_FirstFreeDayType = "FCD";
				importSTOConsignee.PD_OH_Carrier = carrier.PK;

				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsigneePK = consignee.PK;

				var container = consol.Containers.AddNew();
				container.JC_FCLAvailable = new ZDateTime(2022, 10, 1);
				shipment.OuterPackLines.AddNew();

				container.ImportPenalties.DeleteAll();
				container.JC_FCLUnloadFromVessel = new ZDateTime(2022, 10, 12);
				CombineAssertions(() =>
				{
					AssertEquals((ZByte)3, container.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
					AssertNull(container.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false));
					AssertEquals((ZByte)11, container.ImportPenalties.FindOrCreateArrivalMDDPenalty(false).FreeTimeAsDays);
				});
			}
		}

		public void TestHandleContainerAdded_WithMDD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var exportStorage = carrier.CarrierContainerPenalties.AddNew();
			exportStorage.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			exportStorage.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.STO;
			exportStorage.PD_FreeDays = 13;

			var importMDD = carrier.CarrierContainerPenalties.AddNew();
			importMDD.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
			importMDD.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.MDD;
			importMDD.PD_FreeDays = 12;

			var importCTO = Factory.NewWithValidTestData<OrgHeader>();
			var exportCTO = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "CNSZX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = importCTO.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = exportCTO.MainAddress.PK;

			var container1 = Factory.New<ForwardingContainer>();
			consol.Containers.Add(container1);
			AssertNull(container1.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false));
			AssertNull(container1.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false));
			AssertNull(container1.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_FCLAvailable = new ZDateTime(2021, 1, 1);
			consol.Containers.Add(container2);

			AssertEquals((ZByte)13, container2.ImportPenalties.FindOrCreateArrivalCarrierStoragePenalty(false).FreeTimeAsDays);
			AssertNull(container2.ImportPenalties.FindOrCreateArrivalMDDPenalty(false));
			AssertEquals((ZByte)0, container2.ImportPenalties.FindOrCreateArrivalCTOStoragePenalty(false).FreeTimeAsDays);
		}

		public void TestCalculateManualPenalties_AddNewDeliveryPenaltiesWhenEnumerateImportPenalties_NoException()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_TransportMode = TransportModes.Sea;

			var container = consol.Containers.AddNew();
			container.JC_FCLAvailable = new ZDateTime(2022, 1, 1, 1, 1, 1);

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			var importPenalty = container.ImportPenalties.AddNew();
			importPenalty.CPY_PenaltyType = ContainerDetentionPenaltyType.STO;
			importPenalty.CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;

			container.JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;
			container.ImportPenalties.Where(x => !(x.CPY_PenaltyType == ContainerDetentionPenaltyType.STO && x.CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)).ForEach(x => x.Delete());
			container.DeliveryPenalties.DeleteAll();

			AssertEquals(0, container.DeliveryPenalties.Count);

			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				AssertNoExceptionThrown(() => container.JC_FCLWharfGateOut = new ZDateTime(2022, 1, 1, 2, 1, 1));
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				AssertNotEquals(0, container.DeliveryPenalties.Count);
			}
		}
	}
}
