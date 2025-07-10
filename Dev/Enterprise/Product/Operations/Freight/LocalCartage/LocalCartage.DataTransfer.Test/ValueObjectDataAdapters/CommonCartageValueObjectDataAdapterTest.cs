using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Testing
{
	abstract class CommonCartageValueObjectDataAdapterTest : ValueObjectDataAdapterTest<CommonCartage, Xsd.CartageJob>
	{
		protected override string ExpectedRootCollectionElementName
		{
			get
			{
				return "CartageJobs";
			}
		}

		protected override string ExpectedRootElementName
		{
			get
			{
				return "CartageJob";
			}
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		public abstract void TestTargetType();

		protected abstract CommonCartage GetJobCartage();
		protected abstract ZString ExpectedBookingStatus { get; }

		protected abstract ZString ExpectedBookingAction { get; }

		protected abstract ZString ExpectedCartageType { get; }

		protected abstract ZString ExpectedReference { get; }

		protected abstract void AssertXsdSailing(Xsd.CartageJob xsdCartage);
		protected abstract void AssertXsdLegs(Xsd.CartageJob xsdCartage);
		public void TestExportToValueObject()
		{
			CommonCartage cartage = GetJobCartage();
			cartage.BookingInformation.BookingComment = "comment";
			cartage.BookingInformation.BookingStatus = ExpectedBookingStatus;
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			Adapter.ExportToValueObject(cartage, xsdCartage, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("XsdCartage.Type", "CJHR", xsdCartage.Type);
			AssertEquals("XsdCartage.JobType", ExpectedCartageType, xsdCartage.JobType);
			AssertEquals("XsdCartage.Action", ExpectedBookingAction, xsdCartage.Action);
			AssertEquals("XsdCartage.ActionType", ExpectedBookingStatus, xsdCartage.ActionType);
			AssertEquals("XsdCartage.MessageDescription", "comment", xsdCartage.MessageDescription);
			AssertEquals("XsdCartage.MessageResponseAddress", Env.Registry.MailboxEmailAddress, xsdCartage.MessageResponseAddress);
			AssertEquals("XsdCartage.MessageSystemType", Constants.ProductName, xsdCartage.MessageSystemType);
			AssertEquals("Xsd.GoodsDescription", "Goods description", xsdCartage.GoodsDescription);
			AssertEquals("Xsd.ClientJobReference", ExpectedReference, xsdCartage.ClientJobReference);
			AssertEquals("PortOfLoading", "AUSYD", xsdCartage.SailingInfo.PortOfLoading);
			AssertEquals("PortOfDischarge", "GBLON", xsdCartage.SailingInfo.PortOfDischarge);
			AssertXsdSailing(xsdCartage);
			Assert("Local Transport legs count is not zero", xsdCartage.CartageLegs.Count > 0);
			AssertXsdLegs(xsdCartage);
			Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs[0];
			Xsd.AdditionalInstructionsLoadingUnloadingConstraints xsdPickupConstraints = xsdLeg.Pickup.AdditionalInstructions.LoadingUnloadingConstraints;
			AssertEquals("C01", xsdPickupConstraints.AccessPoint);
			AssertEquals("C02", xsdPickupConstraints.Communication);
			AssertEquals("C03", xsdPickupConstraints.DockHeight);
			AssertEquals("C04", xsdPickupConstraints.ContainerHandling);
			AssertEquals("YES", xsdPickupConstraints.LabourRequired);
			AssertEquals("C05", xsdPickupConstraints.FurtherConstraints);
		}

		public CommonCartage GetInternalCartageInNewFactory(CommonShipment shipment)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CommonCartageBehaviorStrategyProvider.SetProvider(factory, new CartageBehaviorStrategyProvider());
			((IBusinessObjectFactoryInternals)factory).CanSave = false;
			CommonCartage cartage = factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T00001234";
			InternalCartageManagerHelper.PopulateCartage(cartage, shipment.IsExport() ? new ShipmentPickupCartageType(shipment) : new ShipmentDeliveryCartageType(shipment));
			return cartage;
		}

		public CommonCartage GetInternalCartageInNewFactory(ICartageParent parent)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CommonCartageBehaviorStrategyProvider.SetProvider(factory, new CartageBehaviorStrategyProvider());
			((IBusinessObjectFactoryInternals)factory).CanSave = false;
			CommonCartage cartage = factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T00001234";
			InternalCartageManagerHelper.PopulateCartage(cartage, parent.CartageTypes.First());
			return cartage;
		}

		public void TestExportContainer()
		{
			CommonShipment shipment = GetNewShipment(Constants.TransportModes.Sea, false);
			CommonConsol consol = shipment.Consols[0];
			consol.Containers.RemoveAndDeleteAll();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "Container2";
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			container.JC_AirVentFlow = 6m;
			container.JC_AirVentFlowRateUnit = "A";
			container.JC_HumidityPercent = 7;
			container.JC_SetPointTemp = -8m;
			container.JC_SetPointTempUnit = "C";
			Factory.Save();
			CommonCartage cartage = GetInternalCartageInNewFactory(shipment);
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			Adapter.ExportToValueObject(cartage, xsdCartage, new ValueObjectExportContext(new NotificationBuffer()));
			Assert(xsdCartage.CartageLegs.Count > 0);
			Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs[0];
			AssertEquals("Leg should have most dangerous goods code as code of default substance pivot of leg's first UNDG data item", "SUBSb", xsdLeg.MostDangerousGoodsCode);
			Xsd.CartageLegContainer xsdContainer = xsdLeg.Item as Xsd.CartageLegContainer;
			AssertNotNull("CommonContainer Leg", xsdContainer);
			AssertEquals(6m, xsdContainer.AirVentFlow);
			AssertEquals("A", xsdContainer.AirVentFlowRateUnit);
			AssertEquals((ZByte)7, xsdContainer.HumidityPercent);
			AssertEquals(-8m, xsdContainer.SetPointTemperature);
			AssertEquals("C", xsdContainer.SetPointTemperatureUnit);
		}

		public void TestExportContainerWithLegWhoseFirstUNDGHasNoPivot()
		{
			CommonShipment shipment = GetNewShipment(Constants.TransportModes.Sea, false);
			CommonConsol consol = shipment.Consols[0];
			consol.Containers.RemoveAndDeleteAll();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "Container2";
			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			container.JC_AirVentFlow = 6m;
			container.JC_AirVentFlowRateUnit = "A";
			container.JC_HumidityPercent = 7;
			container.JC_SetPointTemp = -8m;
			container.JC_SetPointTempUnit = "C";
			Factory.Save();
			CommonCartage cartage = GetInternalCartageInNewFactory(shipment);
			cartage.BookedMovesCollection.First().UNDGs.First().UNDGSubstancePivotCollection.DeleteAll();
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			Adapter.ExportToValueObject(cartage, xsdCartage, new ValueObjectExportContext(new NotificationBuffer()));
			Assert(xsdCartage.CartageLegs.Count > 0);
			Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs[0];
			AssertEquals("As first UNDG has no substance pivots, leg should have blank most dangerous goods code", ZString.Empty, xsdLeg.MostDangerousGoodsCode);
			Xsd.CartageLegContainer xsdContainer = xsdLeg.Item as Xsd.CartageLegContainer;
			AssertNotNull("CommonContainer Leg", xsdContainer);
			AssertEquals(6m, xsdContainer.AirVentFlow);
			AssertEquals("A", xsdContainer.AirVentFlowRateUnit);
			AssertEquals((ZByte)7, xsdContainer.HumidityPercent);
			AssertEquals(-8m, xsdContainer.SetPointTemperature);
			AssertEquals("C", xsdContainer.SetPointTemperatureUnit);
		}

		public void TestContainerDatesPopulatedFromDocsAndCartage()
		{
			CommonShipment shipment = GetNewShipment("SEA", false);
			CommonConsol consol = shipment.Consols[0];
			consol.Containers.RemoveAndDeleteAll();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "Container2";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			Factory.Save();
			CommonCartage cartage = GetInternalCartageInNewFactory(shipment);
			Assert(cartage.Containers.Any());
			cartage.Containers.First().JC_FCLAvailable = ZDateTime.Empty;
			cartage.Containers.First().JC_LCLAvailable = ZDateTime.Empty;
			cartage.Containers.First().JC_LCLStorageCommences = ZDateTime.Empty;
			cartage.Containers.First().JC_ArrivalCTOStorageStartDate = ZDateTime.Empty;
			shipment.DocsAndCartage.JP_FCLAvailable = CurrentDate.AddDays(1);
			shipment.DocsAndCartage.JP_FCLStorageCommences = CurrentDate.AddDays(1);
			shipment.DocsAndCartage.JP_LCLAvailable = CurrentDate.AddDays(3);
			shipment.DocsAndCartage.JP_LCLStorageCommences = CurrentDate.AddDays(4);
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			Adapter.ExportToValueObject(cartage, xsdCartage, new ValueObjectExportContext(new NotificationBuffer()));
			Assert(xsdCartage.CartageLegs.Count > 0);
			Xsd.CartageLeg xsdLeg = xsdCartage.CartageLegs[0];
			Xsd.CartageLegContainer xsdContainer = xsdLeg.Item as Xsd.CartageLegContainer;
			AssertNotNull("CommonContainer Leg", xsdContainer);
			AssertEquals(CurrentDate.AddDays(1), xsdContainer.FCLAvailable);
			AssertEquals(CurrentDate.AddDays(1), xsdContainer.ContainerAdditionalInfo.FCLStorageDate);
		}

		[TestDate(2006, 1, 1)]
		public void TestExportOfCustomAttributes_Shipment()
		{
			CommonShipment shipment = GetNewShipment("SEA", false);
			shipment.DocsAndCartage.JP_CustomAttrib1 = "Cust1";
			shipment.DocsAndCartage.JP_CustomAttrib2 = "Cust2";
			shipment.DocsAndCartage.JP_CustomDate1 = new ZDateTime(2006, 12, 1);
			shipment.DocsAndCartage.JP_CustomDate2 = new ZDateTime(2006, 12, 1);
			shipment.DocsAndCartage.JP_CustomDecimal1 = 1.0m;
			shipment.DocsAndCartage.JP_CustomDecimal2 = 1.1m;
			shipment.DocsAndCartage.JP_CustomFlag1 = ZBool.True;
			shipment.DocsAndCartage.JP_CustomFlag2 = ZBool.True;
			Factory.Save();
			CommonCartage cartage = GetInternalCartageInNewFactory(shipment);
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			Adapter.ExportToValueObject(cartage, xsdCartage, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Cust1", xsdCartage.CustomAttributes.CustomAttr1);
			AssertEquals("Cust2", xsdCartage.CustomAttributes.CustomAttr2);
			AssertEquals(new ZDateTime(2006, 12, 1), xsdCartage.CustomAttributes.CustomDate1);
			AssertEquals(new ZDateTime(2006, 12, 1), xsdCartage.CustomAttributes.CustomDate2);
			AssertEquals(1.0m, xsdCartage.CustomAttributes.CustomDecimal1);
			AssertEquals(1.1m, xsdCartage.CustomAttributes.CustomDecimal2);
			AssertEquals(ZBool.True, xsdCartage.CustomAttributes.CustomFlag1);
			AssertEquals(ZBool.True, xsdCartage.CustomAttributes.CustomFlag2);
		}

		[TestDate(2006, 1, 1)]
		public void TestExportOfCustomAttributes_Declaration()
		{
			BusinessObject parent = GetNewDeclaration("SEA", "IMP");
			JobDocsAndCartage docsAndCartage = (JobDocsAndCartage)parent["DocsAndCartage"];
			docsAndCartage.JP_CustomAttrib1 = "Cust1";
			docsAndCartage.JP_CustomAttrib2 = "Cust2";
			docsAndCartage.JP_CustomDate1 = new ZDateTime(2006, 12, 1);
			docsAndCartage.JP_CustomDate2 = new ZDateTime(2006, 12, 1);
			docsAndCartage.JP_CustomDecimal1 = 1.0m;
			docsAndCartage.JP_CustomDecimal2 = 1.1m;
			docsAndCartage.JP_CustomFlag1 = ZBool.True;
			docsAndCartage.JP_CustomFlag2 = ZBool.True;
			Factory.Save();
			CommonCartage cartage = GetInternalCartageInNewFactory((ICartageParent)parent);
			Xsd.CartageJob xsdCartage = new Xsd.CartageJob();
			Adapter.ExportToValueObject(cartage, xsdCartage, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Cust1", xsdCartage.CustomAttributes.CustomAttr1);
			AssertEquals("Cust2", xsdCartage.CustomAttributes.CustomAttr2);
			AssertEquals(new ZDateTime(2006, 12, 1), xsdCartage.CustomAttributes.CustomDate1);
			AssertEquals(new ZDateTime(2006, 12, 1), xsdCartage.CustomAttributes.CustomDate2);
			AssertEquals(1.0m, xsdCartage.CustomAttributes.CustomDecimal1);
			AssertEquals(1.1m, xsdCartage.CustomAttributes.CustomDecimal2);
			AssertEquals(ZBool.True, xsdCartage.CustomAttributes.CustomFlag1);
			AssertEquals(ZBool.True, xsdCartage.CustomAttributes.CustomFlag2);
		}

		public abstract void TestImportCommentsAndBookingStatus();

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[] { "BillTo", "Carrier", "CartageOrg", "Consignee", "Consignor", "ActionType", "MessageDescription", "ClientJobReference", "ClientTransportDocument", "ClientQuoteReference", "OuterPacks", "DropMode", "CartageContractorJobNumber", "Notes", "RequestedServiceTime", "SailingInfo/Item", "CartageLegs/Delivery", "CartageLegs/PickupTimeInDate", "CartageLegs/DeliverTimeInDate", "CartageLegs/Pickup", "CartageLegs/DeliveryDemurrage", "CartageLegs/LegStatus", "CartageLegs/DeliverTimeOutDate", "CartageLegs/EstimatedDeliveryDate", "CartageLegs/EstimatedPickupDate", "CartageLegs/PickupDemurrage", "CartageLegs/PickupTimeOutDate", "CartageLegs/CartageLegDates", "CartageLegs/TransportCo", "CustomAttributes", "CartageLegs/LegType", "JobNumber", "PackageID", "TransportRef", "TransportBillToAddress", "BillToAddress", "CarrierAddress", "CartageLegs/DangerousGoods" };
			}
		}

		protected OrgHeader OrgProxy
		{
			get
			{
				if (fOrgProxy == null)
				{
					fOrgProxy = Factory.New<OrgHeader>();
					fOrgProxy.OH_RL_NKClosestPort = "AUBNE";
					fOrgProxy.OH_Code = "TESORG";
					fOrgProxy.OH_FullName = "TEST ORGPROXY";
					fOrgProxy.MainAddress.OA_Address1 = "10 HUTCHESON STREET";
					Factory.Save();
				}

				return fOrgProxy;
			}
		}

		OrgHeader fOrgProxy;

		protected IQuotedBooking GetNewQuoteBooking(ZString transportMode)
		{
			IQuotedBooking quotedBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });
			JobHeader quotedBookingJob = new JobHeader.Loader((IJobHeaderParent)quotedBooking).TryCreate();
			quotedBookingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			quotedBookingJob.JH_GB = GlbBranch.CurrentBranch.PK;
			CommonShipment shipment = (CommonShipment)quotedBooking.ForwardingShipment;
			shipment.ConsigneePK = Consignee_GB.PK;
			shipment.ConsignorPK = Consignor_AU.PK;
			shipment.JS_GoodsDescription = "Goods description";
			shipment.JS_TransportMode = transportMode;
			shipment.JS_OuterPacks = 20;
			shipment.JS_ActualVolume = 0.01m;
			shipment.JS_ActualWeight = 0.01m;
			PackLine line = shipment.OuterPackLines[0];
			line.JL_Height = 0.001m;
			line.JL_Length = 0.001m;
			line.JL_Width = 0.001m;
			line.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
			UNDGDataItem dgItem = line.UNDGs.AddNew();
			dgItem.DI_DG = Substance.PK;
			return quotedBooking;
		}

		protected CommonShipment GetNewShipment(ZString transportMode, bool isExport, string uniqueConsignRef = "S00001000")
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true }))
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true }))
			{
				CommonShipment shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				shipment.JS_UniqueConsignRef = uniqueConsignRef;
				shipment.ConsignorPK = isExport ? Consignor_AU.PK : Consignee_GB.PK;
				shipment.ConsigneePK = isExport ? Consignee_GB.PK : Consignor_AU.PK;
				CommonConsol consol = GetNewConsol(transportMode, isExport);
				shipment.Consols.Add(consol);
				shipment.JS_GoodsDescription = "Goods description";
				shipment.JS_TransportMode = transportMode;
				shipment.JS_OuterPacks = 20;
				shipment.JS_ActualVolume = 0.01m;
				shipment.JS_ActualWeight = 0.01m;
				PackLine line = shipment.OuterPackLines[0];
				line.JL_Height = 0.001m;
				line.JL_Length = 0.001m;
				line.JL_Width = 0.001m;
				line.JL_F3_NKPackType = Core.Constants.PkgUnit.Bag;
				UNDGDataItem dgItem = line.UNDGs.AddNew();
				dgItem.DI_DG = Substance.PK;
				shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "SpecialInstructions");
				shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "PickupInstructionsNote");
				shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "DeliveryInstructionsNote");
				shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "HandlingInstructions");
				return shipment;
			}
		}

		[TestDate(2009, 1, 1)]
		public new void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			var unlimitedFreeDaysOptions = new ContainerPenaltyFreeDaysOptions { UnlimitedFreeDays = true };
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			using (FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, unlimitedFreeDaysOptions))
			{
				base.TestExportToValueObject_ForFullyPopulatedBizO();
			}
		}

		[TestDate(2009, 1, 1)]
		public new void TestExportToAndImportFromAndExportToValueObject_ForFullyPopulatedBizO()
		{
			base.TestExportToAndImportFromAndExportToValueObject_ForFullyPopulatedBizO();
		}

		protected BusinessObject GetNewDeclaration(ZString transportMode, ZString messageType)
		{
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_TransportMode] = transportMode;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = messageType;
			Factory.Save();
			return declaration;
		}

		public CommonConsol GetNewConsol(ZString transportMode, bool isExport)
		{
			CommonConsol consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			SetupConsol(consol, GetNewConsolContainer(), transportMode, isExport);
			return consol;
		}

		void SetupConsol(CommonConsol consol, CommonContainer containerToAdd, ZString transportMode, bool isExport)
		{
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = isExport ? "AUSYD" : "GBLON";
			consol.JK_RL_NKDischargePort = isExport ? "GBLON" : "AUSYD";
			Transport transport = consol.Transports[0];
			if (transportMode == Core.Constants.TransportModes.Air)
			{
				transport.JW_VoyageFlight = "QF281";
			}
			else
			{
				transport.JW_Vessel = "New Vessel";
				transport.JW_VoyageFlight = "123";
				transport.JW_TerminalAvailabilityDate = CurrentDate.AddDays(1);
				transport.JW_TerminalStorageDate = CurrentDate.AddDays(1);
				consol.Containers.Add(containerToAdd);
				consol.JK_OA_ArrivalCTOAddress = CTO.MainAddress.PK;
				consol.JK_OA_DepartureCTOAddress = CTO.MainAddress.PK;
				consol.JK_OA_ContainerYardEmptyPickupAddress = LocalCFS.MainAddress.PK;
			}

			transport.JW_ETD = CurrentDate;
			transport.JW_ETA = CurrentDate.AddDays(2);
			consol.JK_OA_PackDepotAddress = LocalCFS.MainAddress.PK;
		}

		public CommonConsol GetNewLoadList(ZString transportMode, bool isExport)
		{
			CommonConsol consol = (CommonConsol)Factory.New<Freight.Integration.CFS.ICFSLoadListConsol>();
			SetupConsol(consol, GetNewLoadListContainer(), transportMode, isExport);
			return consol;
		}

		protected OrgHeader Consignee_GB
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = Factory.New<OrgHeader>();
					fConsignee.OH_Code = "CONSIGN1";
					fConsignee.OH_IsConsignee = true;
					fConsignee.OH_FullName = "Consignee for test";
					fConsignee.MainAddress.OA_Address1 = "Test Address Line 1";
					fConsignee.OH_RL_NKClosestPort = "GBLON";
					OrgAddress address = fConsignee.Addresses[0];
					address.OA_AccessPoint = "C01";
					address.OA_CommunicationRequired = "C02";
					address.OA_Dock_Height = "C03";
					address.OA_LabourRequired = "YES";
					address.OA_ContainerHandling = "C04";
					address.OA_LoadingUnloadingConstraints = "C05";
				}

				return fConsignee;
			}
		}

		OrgHeader fConsignee;

		protected OrgHeader Consignor_AU
		{
			get
			{
				if (fConsignor == null)
				{
					fConsignor = Factory.New<OrgHeader>();
					fConsignor.OH_Code = "LOCALCON";
					fConsignor.OH_IsConsignor = true;
					fConsignor.OH_FullName = "Consignor for test";
					fConsignor.MainAddress.OA_Address1 = "Test Address Line 2";
					fConsignor.OH_RL_NKClosestPort = "AUSYD";
					OrgAddress address = fConsignor.Addresses[0];
					address.OA_AccessPoint = "T01";
					address.OA_CommunicationRequired = "T02";
					address.OA_Dock_Height = "T03";
					address.OA_LabourRequired = "YES";
					address.OA_ContainerHandling = "T04";
					address.OA_LoadingUnloadingConstraints = "T05";
				}

				return fConsignor;
			}
		}

		OrgHeader fConsignor;

		protected OrgHeader LocalCFS
		{
			get
			{
				if (fCFS == null)
				{
					fCFS = Factory.New<OrgHeader>();
					fCFS.OH_Code = "LCLCFS";
					fCFS.OH_IsMiscFreightServices = true;
					fCFS.OH_IsContainerYard = true;
					fCFS.OH_IsPackDepot = true;
					fCFS.OH_IsUnpackDepot = true;
					fCFS.OH_FullName = "Local Forwarder ";
					fCFS.MainAddress.OA_Address1 = "Test Address Line";
					OrgAddress pickupAndDeliveryAddress = fCFS.Addresses.AddNew();
					pickupAndDeliveryAddress.OA_Address1 = "PickupAndDelivery";
					pickupAndDeliveryAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
					fCFS.OH_RL_NKClosestPort = "AUSYD";
					OrgAddress address = fCFS.Addresses[0];
					address.OA_AccessPoint = "C01";
					address.OA_CommunicationRequired = "C02";
					address.OA_Dock_Height = "C03";
					address.OA_LabourRequired = "YES";
					address.OA_ContainerHandling = "C04";
					address.OA_LoadingUnloadingConstraints = "C05";
				}

				return fCFS;
			}
		}

		OrgHeader fCFS;

		protected OrgHeader CTO
		{
			get
			{
				if (fCTO == null)
				{
					fCTO = Factory.New<OrgHeader>();
					fCTO.OH_Code = "NEWCTO";
					fCTO.OH_FullName = "New CTO";
					fCTO.OH_IsMiscFreightServices = true;
					fCTO.OH_IsSeaCTO = true;
					fCTO.OH_IsAirCTO = true;
					fCTO.MainAddress.OA_Address1 = "CTO Main Address";
					OrgAddress address = fCTO.Addresses[0];
					address.OA_AccessPoint = "T01";
					address.OA_CommunicationRequired = "T02";
					address.OA_Dock_Height = "T03";
					address.OA_LabourRequired = "YES";
					address.OA_ContainerHandling = "T04";
					address.OA_LoadingUnloadingConstraints = "T05";
				}

				return fCTO;
			}
		}

		OrgHeader fCTO;

		protected CommonContainer GetNewConsolContainer()
		{
			CommonContainer result = (CommonContainer)Factory.New<Enterprise.Integration.Forwarding.IForwardingContainer>();
			SetupConsolContainer(result);
			return result;
		}

		protected virtual void SetupConsolContainer(CommonContainer container)
		{
			container.JC_ContainerNum = "Container1";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_RC = new RefContainer.Loader(Factory).LoadFromCode("20GP").PK;
			container.JC_SealNum = "123456";
			container.JC_AdditionalSealNum = "789";
			container.JC_DepartureSlotDateTime = CurrentDate;
			container.JC_DepartureSlotReference = "DepSlotRef";
			container.JC_ArrivalSlotDateTime = CurrentDate.AddDays(1);
			container.JC_ArrivalSlotReference = "ArrSlotRef";
			container.JC_ReleaseNum = "ReleaseNo";
			container.JC_EmptyRequired = CurrentDate;
			container.JC_PackDate = CurrentDate;
			container.JC_TempRecorderSerialNo = "ABC";
			container.JC_LCLStorageCommences = CurrentDate;
			container.JC_LCLUnpack = CurrentDate;
			container.JC_LCLAvailable = CurrentDate;
			container.JC_FCLAvailable = CurrentDate;
			container.JC_ArrivalEstimatedDelivery = CurrentDate;
			container.JC_EmptyReturnedBy = CurrentDate;
			container.JC_AirVentFlow = 6m;
			container.JC_AirVentFlowRateUnit = "A";
			container.JC_HumidityPercent = 7;
			container.JC_SetPointTemp = 8m;
			container.JC_SetPointTempUnit = "C";
		}

		protected CommonContainer GetNewLoadListContainer()
		{
			CommonContainer result = (CommonContainer)Factory.New<Freight.Integration.CFS.ICFSContainer>();
			SetupConsolContainer(result);
			return result;
		}

		protected UNDGSubstance Substance
		{
			get
			{
				if (substance == null)
				{
					substance = Factory.New<UNDGSubstance>();
					substance.DG_Class = "4.2";
					substance.DG_UNNO = "SUBS";
					substance.DG_Variant = "b";
					substance.DG_Variation = "METAL ARYLS, WATER-REACTIVE, N.O.S.";
					substance.DG_PSN = "METAL ARYLS, WATER-REACTIVE, N.O.S.";
					Factory.Save();
				}

				return substance;
			}
		}

		UNDGSubstance substance;

		protected CommonCartage GetSampleJobCartage(ZString transportMode)
		{
			CommonShipment shipment = GetNewShipment(transportMode, true);
			shipment.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2006, 5, 3);
			Factory.Save();
			CommonCartage cartage = GetInternalCartageInNewFactory(shipment);
			return cartage;
		}

		protected ZDateTime CurrentDate;
		protected ZDateTime ZeroDurationDateTime;
		OrgHeader CurrentOrgProxy;
		protected override void SetUp()
		{
			base.SetUp();
			CurrentOrgProxy = GlbCompany.CurrentCompany.OrgProxy;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = OrgProxy.PK;
			CurrentDate = new ZDateTime(2006, 5, 2);
			ZeroDurationDateTime = ZDateTime.DefaultDurationEpoch;
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = CurrentOrgProxy.PK;
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected Lazy<EmbeddedResourceRetriever> resourceRetriever;

		protected CommonCartageValueObjectDataAdapter Adapter => (CommonCartageValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
	}
}
