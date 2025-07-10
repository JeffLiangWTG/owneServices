using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BookingRequest
{
	sealed class BookingRequestDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var consol = CreateConsol();
			AssertBookingRequest3(consol);

			AddTransportBookingPickupDeliveryInfos(consol);
			AssertBookingRequest2(consol);

			AssertBookingRequest4(consol);
		}

		void AssertBookingRequest2(ForwardingConsol consol)
		{
			var pivotPK = new ZGuid("13b964a2-f90f-4a9f-8b82-d9f8880108b9");
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertContents(consol, pivotPK, CreateBookingRequest2Content());
			}
		}

		void AssertBookingRequest3(ForwardingConsol consol)
		{
			var pivotPK = new ZGuid("edb92584-2bcc-4f1a-973a-9b03ad56c1fd");
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.DoNotGroup;
				AssertContents(consol, pivotPK, CreateBookingRequest3DoNotGroupContent());
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
				AssertContents(consol, pivotPK, CreateBookingRequest3NonDoNotGroupContent());
			}
		}

		void AssertBookingRequest4(ForwardingConsol consol)
		{
			var pivotPK = new ZGuid("edb92584-2bcc-4f1a-973a-9b03ad56c1fd");
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consol.JK_AgentType = Core.Constants.AgentType.Direct;

				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_FullName = "CONSPA";
				orgHeader.OH_RL_NKClosestPort = "AUSYD";
				orgHeader.MainAddress.Address1 = "Unit 15";
				orgHeader.MainAddress.Address2 = "5 Lost Lane";
				orgHeader.MainAddress.City = "Sydney";
				orgHeader.MainAddress.Postcode = "2000";
				orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

				Factory.Save();

				var declaration = BaseJobDeclaration.New(Factory);
				declaration.JE_MessageType = "EXP";
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
				declaration.JE_RL_NKFinalDestination = "ZAAAM";
				declaration.JE_RL_NKOrigin = "AUSYD";
				declaration.JE_JS = consol.Shipments[0].PK;
				declaration.JE_OH_ExternalBroker = orgHeader.PK;
				AssertContents(consol, pivotPK, CreateBookingRequest4Content());
			}
		}

		ForwardingConsol CreateConsol()
		{
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			sendingForwarder.OH_Code = "SSS";
			sendingForwarder.OH_FullName = "Sending Agent";
			sendingForwarder.MainAddress.CompanyName = "Sending Agent Company";
			sendingForwarder.MainAddress.OA_Address1 = "Sending Agent Address 1";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_Code = "RRR";
			receivingForwarder.OH_FullName = "Receiving Agent";
			receivingForwarder.MainAddress.CompanyName = "Receiving Agent Company";
			receivingForwarder.MainAddress.OA_Address1 = "Receiving Agent Address 1";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "SG";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsShippingLine = true;
			carrier.MainAddress.CompanyName = "Carrier Company";
			carrier.MainAddress.OA_Address1 = "Carrier Address 1";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			carrier.CustomsCodes.AddNew("CCC", "NUM1", "US");

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "CCC";
			creditor.OH_FullName = "CREDITOR";
			creditor.MainAddress.CompanyName = "Creditor Company";
			creditor.MainAddress.OA_Address1 = "Creditor Address 1";
			creditor.MainAddress.OA_RN_NKCountryCode = "CN";
			creditor.OH_IsCreditor = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";
			consol.JK_RL_NKLoadPort = "USSEA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_UniqueConsignRef = "C00009999";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER1";
			PopulateAdditionalReferenceNumber(container1.AdditionalReferenceNumbers.AddNew(), "NUM1", ZString.Empty, CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CustomerLoadReference);

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER2";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONTAINER3";

			var container4 = consol.Containers.AddNew();
			container4.JC_ContainerNum = ZString.Empty;

			var container5 = consol.Containers.AddNew();
			container5.JC_ContainerNum = ZString.Empty;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment1.JS_UniqueConsignRef = "JSASM";
			shipment1.JS_GoodsDescription = "ASM JS_GoodsDescription";
			shipment1.JS_MarksAndNumbers = "ASM JS_MarksAndNumbers";

			var shipment1SubShipment1 = shipment1.CoLoadShipments.AddNew();
			shipment1SubShipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipment1SubShipment1.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipment1SubShipment1.JS_UniqueConsignRef = "Sub01";
			shipment1SubShipment1.JS_GoodsDescription = "Sub1 JS_GoodsDescription";
			shipment1SubShipment1.JS_MarksAndNumbers = "Sub1 JS_MarksAndNumbers";

			shipment1SubShipment1.InnerPackLines.RemoveAndDeleteAll();
			shipment1SubShipment1.OuterPackLines.RemoveAndDeleteAll();
			shipment1SubShipment1.CusEntryNumbers.RemoveAndDeleteAll();

			var cusEntryNumber = shipment1SubShipment1.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			cusEntryNumber.CE_EntryNum = "11321";
			cusEntryNumber.CE_ParentID = shipment1SubShipment1.PK;
			cusEntryNumber.CE_ParentTable = shipment1SubShipment1.TableName;

			var shipment1SubShipment1OuterPackLine1 = PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 100, Constants.PkgUnit.BaleCompressed
				, 100, Constants.Weight.Kilograms
				, 100, Constants.Volume.CubicMetres
				, "BaleCompressed1 MarksAndNumbers", "BaleCompressed1 DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container1.PK, "HC01"
				, "Ref01", "ImportRef01", "ExportRef01", "OuturnComment01", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment1OuterPackLine1HsCode1 = shipment1SubShipment1OuterPackLine1.HarmonisedCodes.AddNew();
			shipment1SubShipment1OuterPackLine1HsCode1.JLH_RN_NKCountry = "US";
			shipment1SubShipment1OuterPackLine1HsCode1.JLH_Code = "USHS";

			var shipment1SubShipment1OuterPackLine1HsCode2 = shipment1SubShipment1OuterPackLine1.HarmonisedCodes.AddNew();
			shipment1SubShipment1OuterPackLine1HsCode2.JLH_RN_NKCountry = "CN";
			shipment1SubShipment1OuterPackLine1HsCode2.JLH_Code = "CNHS";

			PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 110, Constants.PkgUnit.Tube
				, 110, Constants.Weight.Kilograms
				, 110, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container2.PK, "HC04"
				, "Ref04", "ImportRef04", "ExportRef04", "OuturnComment04", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment1.OuterPackLines.AddNew()
				, 11000, Constants.PkgUnit.Tube
				, 11000, Constants.Weight.Grams
				, 11000, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, ZGuid.Empty, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2 = shipment1.CoLoadShipments.AddNew();
			shipment1SubShipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.BreakBulk;
			shipment1SubShipment2.JS_F3_NKTotalCountPackType = Core.Constants.PkgUnit.BaleCompressed;
			shipment1SubShipment2.JS_UniqueConsignRef = "Sub02";
			shipment1SubShipment2.JS_GoodsDescription = "Sub1 JS_GoodsDescription";
			shipment1SubShipment2.JS_MarksAndNumbers = "Sub1 JS_MarksAndNumbers";

			shipment1SubShipment2.InnerPackLines.RemoveAndDeleteAll();
			shipment1SubShipment2.OuterPackLines.RemoveAndDeleteAll();

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 120, Constants.PkgUnit.BaleCompressed
				, 120, Constants.Weight.Kilograms
				, 120, Constants.Volume.CubicMetres
				, "BaleCompressed2 MarksAndNumbers", "BaleCompressed2 DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container4.PK, "HC07"
				, "Ref07", "ImportRef07", "ExportRef07", "OuturnComment07", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 121, Constants.PkgUnit.Tote
				, 121, Constants.Weight.Kilograms
				, 121, Constants.Volume.CubicMetres
				, "Tote MarksAndNumbers", "Tote DetailedDescription"
				, true, -2, 3, Constants.Temperature.Centigrade
				, container5.PK, "HC08"
				, "Ref08", "ImportRef08", "ExportRef08", "OuturnComment08", Core.Constants.CargoTypes.Hazardous);

			PopulatePackLine(shipment1SubShipment2.OuterPackLines.AddNew()
				, 12000, Constants.PkgUnit.Tube
				, 12000, Constants.Weight.Kilograms
				, 12000, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, -2, 2, Constants.Temperature.Centigrade
				, container2.PK, "HC09",
				"Ref01", "ImportRef09", "ExportRef01", "OuturnComment09", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2OuterPackLines1 = shipment1SubShipment2.OuterPackLines.AddNew();

			PopulatePackLine(shipment1SubShipment2OuterPackLines1
				, 12100, Constants.PkgUnit.Tube
				, 12100, Constants.Weight.Kilograms
				, 12100, Constants.Volume.CubicMetres
				, "Tube MarksAndNumbers", "Tube DetailedDescription"
				, true, 1, 3, Constants.Temperature.Centigrade,
				ZGuid.Empty, "HC10",
				"Ref10", "ImportRef10", "ExportRef10", "OuturnComment10", Core.Constants.CargoTypes.Hazardous);

			var shipment1SubShipment2OuterPackLines1HsCode1 = shipment1SubShipment2OuterPackLines1.HarmonisedCodes.AddNew();
			shipment1SubShipment2OuterPackLines1HsCode1.JLH_RN_NKCountry = "US";
			shipment1SubShipment2OuterPackLines1HsCode1.JLH_Code = "HSUS";

			var shipment1SubShipment2OuterPackLines1HsCode2 = shipment1SubShipment2OuterPackLines1.HarmonisedCodes.AddNew();
			shipment1SubShipment2OuterPackLines1HsCode2.JLH_RN_NKCountry = "CN";
			shipment1SubShipment2OuterPackLines1HsCode2.JLH_Code = "CNHS";

			Factory.Save();

			return consol;
		}

		PackLine PopulatePackLine(PackLine packLine, ZInt packageCount, ZString packType, ZDecimal weight, ZString unitOfWeight, ZDecimal volume, ZString unitOfVolume, ZString marksAndNumbers, ZString detailedDescription
			, ZBool requiresTemperatureControl, int requiredTemperatureMinimum, int requiredTemperatureMaximum, ZString requiredTemperatureUnit, ZGuid containerPK, ZString harmonisedCode
			, ZString referenceNumber, ZString importReferenceNumber, ZString exportReferenceNumber, ZString outturnComment, ZString commodityCode)
		{
			packLine.JL_PackageCount = packageCount;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = unitOfWeight;
			packLine.JL_ActualVolume = volume;
			packLine.JL_ActualVolumeUQ = unitOfVolume;
			packLine.JL_MarksAndNumbers = marksAndNumbers;
			packLine.JL_DetailedDescription = detailedDescription;
			packLine.JL_RequiredTemperatureMinimum = requiredTemperatureMinimum;
			packLine.JL_RequiredTemperatureMaximum = requiredTemperatureMaximum;
			packLine.JL_RequiredTemperatureUnit = requiredTemperatureUnit;
			packLine.JL_RequiresTemperatureControl = requiresTemperatureControl;
			packLine.JL_JC = containerPK;
			packLine.JL_HarmonisedCode = harmonisedCode;
			packLine.JL_RefNumber = referenceNumber;
			packLine.JL_ImportRefNumber = importReferenceNumber;
			packLine.JL_ExportRefNumber = exportReferenceNumber;
			packLine.JL_OutturnComment = outturnComment;
			packLine.JL_RH_NKCommodityCode = commodityCode;

			return packLine;
		}

		void PopulateAdditionalReferenceNumber(CusEntryNumber cusEntryNumber, ZString entryNum, ZString countryCode, ZString entryType)
		{
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;
			cusEntryNumber.CE_EntryType = entryType;
		}

		ITransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = ObjectFactory.New<ITransportBookingTestHelper>(Factory)); }
		}

		ITransportBookingTestHelper helper;

		PkgPackage CreatePkgPackage(IDtbBookingConsolidation consolidation, string kjJobID)
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consolidation.PK;
			packageJob.KJ_JobID = kjJobID;
			packageJob.KJ_ParentTableCode = "KB";

			var package = packageJob.Packages.AddNew("CNT");
			package.KP_PackageQty = 1;
			package.KP_F3_NKPackType = "CNT";
			package.KP_DimensionUQ = "M";
			package.KP_Length = 3;
			package.KP_Width = 4;
			package.KP_Height = 2;
			package.KP_WeightUQ = "KG";
			package.KP_Weight = 1000;
			package.KP_VolumeUQ = "M3";
			package.KP_Volume = 24;
			package.Container.K0_RC_ContainerType = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			return package;
		}

		OrgHeader CreateOrgHeader(string name, string address1, string address2)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = name;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.MainAddress.Address1 = address1;
			orgHeader.MainAddress.Address2 = address2;
			orgHeader.MainAddress.City = "Sydney";
			orgHeader.MainAddress.Postcode = "2000";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";

			return orgHeader;
		}

		IDtbBooking CreateDtbBooking(IDtbBookingConsolidation consolidation, OrgHeader companyAddress, OrgAddress address1, OrgAddress address2, string direction, string kmJobID, PkgPackage package, string instructionType, string orgType = "CFS", string template = "EFPR")
		{
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = kmJobID;
			booking.KM_KT_NKBookingTemplate = template;
			booking.KM_Direction = direction;
			if (companyAddress != null)
			{
				booking.Address.OrganisationPK = companyAddress.PK;
			}

			helper.CreateInstruction(booking, instructionType, orgType, address1, package, new ZDateTime(2021, 11, 1));
			helper.CreateInstruction(booking, instructionType, orgType, address2, package, new ZDateTime(2021, 12, 1));

			return booking;
		}

		public void AddTransportBookingPickupDeliveryInfos(ForwardingConsol consol)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "PICKUPDELIVERY";
			container.JC_ContainerCount = 1;
			container.JC_DeliveryMode = "CFS/CFS";
			container.JC_IsShipperOwned = true;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			var address = CreateOrgHeader("ORG", "Unit ", "Lost Lane").MainAddress;
			var address1 = CreateOrgHeader("ORG1", "Unit 1", "1 Lost Lane").MainAddress;
			var address2 = CreateOrgHeader("ORG2", "Unit 2", "2 Lost Lane").MainAddress;
			var address3 = CreateOrgHeader("ORG3", "Unit 3", "3 Lost Lane").MainAddress;
			var address4 = CreateOrgHeader("ORG4", "Unit 4", "4 Lost Lane").MainAddress;
			var address5 = CreateOrgHeader("ORG5", "Unit 5", "5 Lost Lane").MainAddress;
			var address6 = CreateOrgHeader("ORG6", "Unit 6", "6 Lost Lane").MainAddress;
			var address7 = CreateOrgHeader("ORG7", "Unit 7", "7 Lost Lane").MainAddress;
			var address8 = CreateOrgHeader("ORG8", "Unit 8", "8 Lost Lane").MainAddress;

			var consolidation1 = Helper.CreateConsolidation();
			consolidation1.KB_ParentID = consol.PK;
			consolidation1.KB_ParentTableCode = consol.TablePrefix;
			consolidation1.KB_JobDirection = "PIC";

			var consolidation2 = Helper.CreateConsolidation();
			consolidation2.KB_ParentID = consol.PK;
			consolidation2.KB_ParentTableCode = consol.TablePrefix;
			consolidation2.KB_JobDirection = "DLV";
			var package1 = CreatePkgPackage(consolidation1, "P001");
			var package2 = CreatePkgPackage(consolidation2, "P002");

			var noTransportCompanyBooking = CreateDtbBooking(consolidation1, null, address, address, "EXP", "TB001", package1, "PIC");
			var otherOrgBooking = CreateDtbBooking(consolidation1, consol.ShippingLine, address, address, "EXP", "TB003", package1, "PIC", orgType: "CTO");
			var pickupBooking1 = CreateDtbBooking(consolidation1, consol.ShippingLine, address1, address2, "EXP", "TB011", package1, "PIC");
			var pickupBooking2 = CreateDtbBooking(consolidation1, consol.ShippingLine, address3, address4, "ORG", "TB012", package1, "PIC");
			var deliveryBooking1 = CreateDtbBooking(consolidation2, consol.ShippingLine, address5, address6, "IMP", "TB021", package2, "DLV");
			var deliveryBooking2 = CreateDtbBooking(consolidation2, consol.ShippingLine, address7, address8, "DST", "TB022", package2, "DLV");
		}

		string CreateBookingRequest2Content()
		{
			return @"[2,4] Shipper
[2,16] Carrier
[2,27] BOOKING REQUEST
[3,4] SENDING AGENT COMPANY
[3,16] CARRIER COMPANY
[4,4] SENDING AGENT ADDRESS 1
[4,16] CARRIER ADDRESS 1
[7,4] AUSTRALIA
[7,16] AUSTRALIA
[10,4] Consignee
[10,16] Notify Party
[10,28] Carrier Booking References
[11,4] RECEIVING AGENT COMPANY
[12,4] RECEIVING AGENT ADDRESS 1
[14,28] Carrier Booking Office
[15,4] SINGAPORE
[15,28]  - 
[16,28] Origin
[16,40] Destination
[17,28] USSEA - SEATTLE, WA, UNITED STATES
[17,40] AUSYD - SYDNEY, AUSTRALIA
[18,4] Vessel
[18,16] Lloyds/IMO
[18,28] Voyage
[18,40] Mode
[18,44] Pickup/Delivery
[19,40] FCL
[19,44] ☑ Door Pickup
[20,44] ☑ Door Delivery
[21,4] Place of Receipt
[21,16] Place of Delivery
[21,28] Port of Load
[21,40] Port of Discharge
[22,4] USSEA - SEATTLE, WA, UNITED STATES
[22,16] AUSYD - SYDNEY, AUSTRALIA
[22,28] USSEA - SEATTLE, WA, UNITED STATES
[22,40] AUSYD - SYDNEY, AUSTRALIA
[23,4] Earliest Departure
[23,16] Latest Delivery
[23,40] Nature of Cargo
[24,40] ☐ Hazardous
[24,46] ☐ Out Of Gauge
[26,4] Transport Details
[27,4] #
[27,6] Mode
[27,10] Type
[27,16] Vessel
[27,24] Voyage
[27,28] Load
[27,32] ETD
[27,40] Discharge
[27,44] ETA
[28,4] 1
[28,6] SEA
[28,10] MAI
[28,28] USSEA
[28,40] AUSYD
[30,4] Goods and Equipment Details 
[31,4] Container
[31,16] ISO Type
[31,20] Quality
[31,24] Packs
[31,28] Net (KG)
[31,34] Tare (KG)
[31,40] Gross (KG)
[31,46] Volume (M3)
[32,4] 1 Container(s) 
[32,24] 120
[32,28] 120.000
[32,34] 0.000
[32,40] 120.000
[32,46] 120.000
[33,4] Pickup Empty From: 
[33,28] Empty Required By: 
[34,28] Estimated Full Pickup: 
[35,4]    Packs:
[35,8] 120
[35,14] BLC
[35,16] Marks: BaleCompressed2 MarksAndNumbers
[35,34] Goods Description: BaleCompressed2 DetailedDescription
[36,8] 120.000
[36,14] KG
[37,8] 120.000
[37,14] M3
[38,4] HC: HC07
[42,4] Temp:-2 to 3 Centigrade
[42,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[43,4] Container
[43,16] ISO Type
[43,20] Quality
[43,24] Packs
[43,28] Net (KG)
[43,34] Tare (KG)
[43,40] Gross (KG)
[43,46] Volume (M3)
[44,4] 1 Container(s) 
[44,24] 121
[44,28] 121.000
[44,34] 0.000
[44,40] 121.000
[44,46] 121.000
[45,4] Pickup Empty From: 
[45,28] Empty Required By: 
[46,28] Estimated Full Pickup: 
[49,42] Created By
[51,4] Goods and Equipment Details 
[52,4]    Packs:
[52,8] 121
[52,14] TOT
[52,16] Marks: Tote MarksAndNumbers
[52,34] Goods Description: Tote DetailedDescription
[53,8] 121.000
[53,14] KG
[54,8] 121.000
[54,14] M3
[55,4] HC: HC08
[59,4] Temp:-2 to 3 Centigrade
[59,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[60,4] Container
[60,16] ISO Type
[60,20] Quality
[60,24] Packs
[60,28] Net (KG)
[60,34] Tare (KG)
[60,40] Gross (KG)
[60,46] Volume (M3)
[61,4] CONTAINER1 
[61,24] 100
[61,28] 100.000
[61,34] 0.000
[61,40] 100.000
[61,46] 100.000
[62,4] Pickup Empty From: 
[62,28] Empty Required By: 
[63,28] Estimated Full Pickup: 
[64,4]    Customer Load Reference:
[64,16] NUM1
[65,4]    Packs:
[65,8] 100
[65,14] BLC
[65,16] Marks: BaleCompressed1 MarksAndNumbers
[65,34] Goods Description: BaleCompressed1 DetailedDescription
[66,8] 100.000
[66,14] KG
[67,8] 100.000
[67,14] M3
[68,4] HC: HC01
[69,4] HS (US): USHS
[71,4] BKG:11321
[72,4] Temp:-2 to 3 Centigrade
[72,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[73,4] Container
[73,16] ISO Type
[73,20] Quality
[73,24] Packs
[73,28] Net (KG)
[73,34] Tare (KG)
[73,40] Gross (KG)
[73,46] Volume (M3)
[74,4] CONTAINER2 
[74,24] 12110
[74,28] 12110.000
[74,34] 0.000
[74,40] 12110.000
[74,46] 12110.000
[75,4] Pickup Empty From: 
[75,28] Empty Required By: 
[76,28] Estimated Full Pickup: 
[77,4]    Packs:
[77,8] 110
[77,14] TUB
[77,16] Marks: Tube MarksAndNumbers
[77,34] Goods Description: Tube DetailedDescription
[78,8] 110.000
[78,14] KG
[79,8] 110.000
[79,14] M3
[80,4] HC: HC04
[83,4] BKG:11321
[84,4] Temp:-2 to 3 Centigrade
[84,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[85,4]    Packs:
[85,8] 12000
[85,14] TUB
[85,16] Marks: Tube MarksAndNumbers
[85,34] Goods Description: Tube DetailedDescription
[86,8] 12000.000
[86,14] KG
[87,8] 12000.000
[87,14] M3
[88,4] HC: HC09
[92,4] Temp:-2 to 2 Centigrade
[92,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[93,4] Container
[93,16] ISO Type
[93,20] Quality
[93,24] Packs
[93,28] Net (KG)
[93,34] Tare (KG)
[93,40] Gross (KG)
[93,46] Volume (M3)
[94,4] CONTAINER3 
[94,24] 0
[94,28] 0.000
[94,34] 0.000
[94,40] 0.000
[94,46] 0.000
[95,4] Pickup Empty From: 
[95,28] Empty Required By: 
[96,28] Estimated Full Pickup: 
[99,42] Created By
[101,4] Goods and Equipment Details 
[102,4] Container
[102,16] ISO Type
[102,20] Quality
[102,24] Packs
[102,28] Net (KG)
[102,34] Tare (KG)
[102,40] Gross (KG)
[102,46] Volume (M3)
[103,4] PICKUPDELIVERY (Shipper Owned)
[103,16] 22G0 
[103,24] 0
[103,28] 0.000
[103,34] 2280.000
[103,40] 3280.000
[103,46] 0.000
[104,4] Empty Pickup: 
[104,40] Empty Req. By: 
[105,4] Full Pickup:ORG1, UNIT 1, 1 LOST LANE, SYDNEY, AU
[105,40] Est. Full Pickup:01-Nov-21 00:00
[106,4] Full Pickup:ORG2, UNIT 2, 2 LOST LANE, SYDNEY, AU
[106,40] Est. Full Pickup:01-Dec-21 00:00
[107,4] Full Pickup:ORG3, UNIT 3, 3 LOST LANE, SYDNEY, AU
[107,40] Est. Full Pickup:01-Nov-21 00:00
[108,4] Full Pickup:ORG4, UNIT 4, 4 LOST LANE, SYDNEY, AU
[108,40] Est. Full Pickup:01-Dec-21 00:00
[109,4] Full Delivery:ORG5, UNIT 5, 5 LOST LANE, SYDNEY, AU
[109,40] Est. Full Delivery:01-Nov-21 00:00
[110,4] Full Delivery:ORG6, UNIT 6, 6 LOST LANE, SYDNEY, AU
[110,40] Est. Full Delivery:01-Dec-21 00:00
[111,4] Full Delivery:ORG7, UNIT 7, 7 LOST LANE, SYDNEY, AU
[111,40] Est. Full Delivery:01-Nov-21 00:00
[112,4] Full Delivery:ORG8, UNIT 8, 8 LOST LANE, SYDNEY, AU
[112,40] Est. Full Delivery:01-Dec-21 00:00
[114,4] Additional References
[115,4] BOL Number
[116,4] Shipper Ref
[117,4] Freight Forwarder Ref
[117,16] C00009999
[118,4] Carrier Contract No
[118,28] Contract Named Account
[120,4] Additional Parties
[121,4] Notify Party 2
[121,16] Notify Party 3
[121,28] Forwarder
[121,40] Freight Payer
[122,28] SENDING AGENT COMPANY
[123,28] SENDING AGENT ADDRESS 1
[126,28] AUSTRALIA
[138,4] Additional Instructions/Clauses
[139,4] Charge Payment Instructions
[139,38] Freight Payable At
[140,6] Freight Charges
[140,38] USSEA - Seattle, WA, United States
[141,14] ☑ Prepaid
[141,22] ☐ Collect
[141,30] ☐ Payable Elsewhere
[142,6] Optional Charge Payment Instruction by Category
[143,10] Origin Port Charge
[143,22] ☐ Prepaid
[143,30] ☐ Collect
[144,10] Origin Haulage
[144,22] ☐ Prepaid
[144,30] ☐ Collect
[145,10] Destination Port Charge
[145,22] ☐ Prepaid
[145,30] ☐ Collect
[146,10] Destination Haulage
[146,22] ☐ Prepaid
[146,30] ☐ Collect
[147,4] Goods Handling Instruction
[152,42] Created By
";
		}

		string CreateBookingRequest3DoNotGroupContent()
		{
			return @"[2,4] Shipper
[2,16] Carrier
[2,27] BOOKING REQUEST
[3,4] SENDING AGENT COMPANY
[3,16] CARRIER COMPANY
[4,4] SENDING AGENT ADDRESS 1
[4,16] CARRIER ADDRESS 1
[7,4] AUSTRALIA
[7,16] AUSTRALIA
[10,4] Consignee
[10,16] Notify Party
[10,28] Carrier Booking References
[11,4] RECEIVING AGENT COMPANY
[12,4] RECEIVING AGENT ADDRESS 1
[14,28] Carrier Booking Office
[15,4] SINGAPORE
[15,28]  - 
[16,28] Origin
[16,40] Destination
[17,28] USSEA - SEATTLE, WA, UNITED STATES
[17,40] AUSYD - SYDNEY, AUSTRALIA
[18,4] Vessel
[18,16] Lloyds/IMO
[18,28] Voyage
[18,40] Mode
[18,44] Pickup/Delivery
[19,40] FCL
[19,44] ☐ Door Pickup
[20,44] ☐ Door Delivery
[21,4] Place of Receipt
[21,16] Place of Delivery
[21,28] Port of Load
[21,40] Port of Discharge
[22,4] USSEA - SEATTLE, WA, UNITED STATES
[22,16] AUSYD - SYDNEY, AUSTRALIA
[22,28] USSEA - SEATTLE, WA, UNITED STATES
[22,40] AUSYD - SYDNEY, AUSTRALIA
[23,4] Earliest Departure
[23,16] Latest Delivery
[23,40] Nature of Cargo
[24,40] ☐ Hazardous
[24,46] ☐ Out Of Gauge
[26,4] Transport Details
[27,4] #
[27,6] Mode
[27,10] Type
[27,16] Vessel
[27,24] Voyage
[27,28] Load
[27,32] ETD
[27,40] Discharge
[27,44] ETA
[28,4] 1
[28,6] SEA
[28,10] MAI
[28,28] USSEA
[28,40] AUSYD
[30,4] Equipment Details 
[31,4] Container
[31,16] ISO Type
[31,20] Quality
[31,24] Packs
[31,28] Net (KG)
[31,34] Tare (KG)
[31,40] Gross (KG)
[31,46] Volume (M3)
[32,4] 1 Container(s) 
[32,24] 120
[32,28] 120.000
[32,34] 0.000
[32,40] 120.000
[32,46] 120.000
[33,4] Pickup Empty From: 
[33,28] Empty Required By: 
[34,28] Estimated Full Pickup: 
[35,4] 1 Container(s) 
[35,24] 121
[35,28] 121.000
[35,34] 0.000
[35,40] 121.000
[35,46] 121.000
[36,4] Pickup Empty From: 
[36,28] Empty Required By: 
[37,28] Estimated Full Pickup: 
[38,4] CONTAINER1 
[38,24] 100
[38,28] 100.000
[38,34] 0.000
[38,40] 100.000
[38,46] 100.000
[39,4] Pickup Empty From: 
[39,28] Empty Required By: 
[40,28] Estimated Full Pickup: 
[41,4] Customer Load Reference:
[41,16] NUM1
[42,4] CONTAINER2 
[42,24] 12110
[42,28] 12110.000
[42,34] 0.000
[42,40] 12110.000
[42,46] 12110.000
[43,4] Pickup Empty From: 
[43,28] Empty Required By: 
[44,28] Estimated Full Pickup: 
[45,4] CONTAINER3 
[45,24] 0
[45,28] 0.000
[45,34] 0.000
[45,40] 0.000
[45,46] 0.000
[46,4] Pickup Empty From: 
[46,28] Empty Required By: 
[47,28] Estimated Full Pickup: 
[48,4] Goods Details 
[51,42] Created By
[53,4] Goods Details 
[54,4]  Packs:
[54,8] 120
[54,14] BLC
[54,16] Marks & Number:
[54,34] Goods Description:
[55,8] 120.000
[55,14] KG
[55,16] BaleCompressed2 MarksAndNumbers
[55,34] BaleCompressed2 DetailedDescription
[56,8] 120.000
[56,14] M3
[57,4] HC: HC07 
[61,4] Temp:-2 to 3 Centigrade
[61,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[62,4]                  Packs in Containers
[62,16] 1 x 
[62,36] 120 BLC
[62,40] 120.000 KG
[62,46] 120.000 M3
[63,4]  Packs:
[63,8] 121
[63,14] TOT
[63,16] Marks & Number:
[63,34] Goods Description:
[64,8] 121.000
[64,14] KG
[64,16] Tote MarksAndNumbers
[64,34] Tote DetailedDescription
[65,8] 121.000
[65,14] M3
[66,4] HC: HC08 
[70,4] Temp:-2 to 3 Centigrade
[70,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[71,4]                  Packs in Containers
[71,16] 1 x 
[71,36] 121 TOT
[71,40] 121.000 KG
[71,46] 121.000 M3
[72,4]  Packs:
[72,8] 100
[72,14] BLC
[72,16] Marks & Number:
[72,34] Goods Description:
[73,8] 100.000
[73,14] KG
[73,16] BaleCompressed1 MarksAndNumbers
[73,34] BaleCompressed1 DetailedDescription
[74,8] 100.000
[74,14] M3
[75,4] HC: HC01 HS: (US) USHS 
[78,4] BKG:11321
[79,4] Temp:-2 to 3 Centigrade
[79,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[80,4]                  Packs in Containers
[80,16] CONTAINER1
[80,36] 100 BLC
[80,40] 100.000 KG
[80,46] 100.000 M3
[81,4]  Packs:
[81,8] 110
[81,14] TUB
[81,16] Marks & Number:
[81,34] Goods Description:
[82,8] 110.000
[82,14] KG
[82,16] Tube MarksAndNumbers
[82,34] Tube DetailedDescription
[83,8] 110.000
[83,14] M3
[84,4] HC: HC04 
[87,4] BKG:11321
[88,4] Temp:-2 to 3 Centigrade
[88,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[89,4]                  Packs in Containers
[89,16] CONTAINER2
[89,36] 110 TUB
[89,40] 110.000 KG
[89,46] 110.000 M3
[90,4]  Packs:
[90,8] 12000
[90,14] TUB
[90,16] Marks & Number:
[90,34] Goods Description:
[91,8] 12000.000
[91,14] KG
[91,16] Tube MarksAndNumbers
[91,34] Tube DetailedDescription
[92,8] 12000.000
[92,14] M3
[93,4] HC: HC09 
[97,4] Temp:-2 to 2 Centigrade
[97,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[98,4]                  Packs in Containers
[98,16] CONTAINER2
[98,36] 12000 TUB
[98,40] 12000.000 KG
[98,46] 12000.000 M3
[101,42] Created By
[103,4] Additional References
[104,4] BOL Number
[105,4] Shipper Ref
[106,4] Freight Forwarder Ref
[106,16] C00009999
[107,4] Carrier Contract No
[107,28] Contract Named Account
[109,4] Additional Parties
[110,4] Notify Party 2
[110,16] Notify Party 3
[110,28] Forwarder
[110,40] Freight Payer
[111,28] SENDING AGENT COMPANY
[112,28] SENDING AGENT ADDRESS 1
[115,28] AUSTRALIA
[127,4] Additional Instructions/Clauses
[128,4] Charge Payment Instructions
[128,38] Freight Payable At
[129,6] Freight Charges
[129,38] USSEA - Seattle, WA, United States
[130,14] ☑ Prepaid
[130,22] ☐ Collect
[130,30] ☐ Payable Elsewhere
[131,6] Optional Charge Payment Instruction by Category
[132,10] Origin Port Charge
[132,22] ☐ Prepaid
[132,30] ☐ Collect
[133,10] Origin Haulage
[133,22] ☐ Prepaid
[133,30] ☐ Collect
[134,10] Destination Port Charge
[134,22] ☐ Prepaid
[134,30] ☐ Collect
[135,10] Destination Haulage
[135,22] ☐ Prepaid
[135,30] ☐ Collect
[136,4] Goods Handling Instruction
[141,42] Created By
";
		}

		string CreateBookingRequest3NonDoNotGroupContent()
		{
			return @"[2,4] Shipper
[2,16] Carrier
[2,27] BOOKING REQUEST
[3,4] SENDING AGENT COMPANY
[3,16] CARRIER COMPANY
[4,4] SENDING AGENT ADDRESS 1
[4,16] CARRIER ADDRESS 1
[7,4] AUSTRALIA
[7,16] AUSTRALIA
[10,4] Consignee
[10,16] Notify Party
[10,28] Carrier Booking References
[11,4] RECEIVING AGENT COMPANY
[12,4] RECEIVING AGENT ADDRESS 1
[14,28] Carrier Booking Office
[15,4] SINGAPORE
[15,28]  - 
[16,28] Origin
[16,40] Destination
[17,28] USSEA - SEATTLE, WA, UNITED STATES
[17,40] AUSYD - SYDNEY, AUSTRALIA
[18,4] Vessel
[18,16] Lloyds/IMO
[18,28] Voyage
[18,40] Mode
[18,44] Pickup/Delivery
[19,40] FCL
[19,44] ☐ Door Pickup
[20,44] ☐ Door Delivery
[21,4] Place of Receipt
[21,16] Place of Delivery
[21,28] Port of Load
[21,40] Port of Discharge
[22,4] USSEA - SEATTLE, WA, UNITED STATES
[22,16] AUSYD - SYDNEY, AUSTRALIA
[22,28] USSEA - SEATTLE, WA, UNITED STATES
[22,40] AUSYD - SYDNEY, AUSTRALIA
[23,4] Earliest Departure
[23,16] Latest Delivery
[23,40] Nature of Cargo
[24,40] ☐ Hazardous
[24,46] ☐ Out Of Gauge
[26,4] Transport Details
[27,4] #
[27,6] Mode
[27,10] Type
[27,16] Vessel
[27,24] Voyage
[27,28] Load
[27,32] ETD
[27,40] Discharge
[27,44] ETA
[28,4] 1
[28,6] SEA
[28,10] MAI
[28,28] USSEA
[28,40] AUSYD
[30,4] Equipment Details 
[31,4] Container
[31,16] ISO Type
[31,20] Quality
[31,24] Packs
[31,28] Net (KG)
[31,34] Tare (KG)
[31,40] Gross (KG)
[31,46] Volume (M3)
[32,4] 1 Container(s) 
[32,24] 120
[32,28] 120.000
[32,34] 0.000
[32,40] 120.000
[32,46] 120.000
[33,4] Pickup Empty From: 
[33,28] Empty Required By: 
[34,28] Estimated Full Pickup: 
[35,4] 1 Container(s) 
[35,24] 121
[35,28] 121.000
[35,34] 0.000
[35,40] 121.000
[35,46] 121.000
[36,4] Pickup Empty From: 
[36,28] Empty Required By: 
[37,28] Estimated Full Pickup: 
[38,4] CONTAINER1 
[38,24] 100
[38,28] 100.000
[38,34] 0.000
[38,40] 100.000
[38,46] 100.000
[39,4] Pickup Empty From: 
[39,28] Empty Required By: 
[40,28] Estimated Full Pickup: 
[41,4] Customer Load Reference:
[41,16] NUM1
[42,4] CONTAINER2 
[42,24] 12110
[42,28] 12110.000
[42,34] 0.000
[42,40] 12110.000
[42,46] 12110.000
[43,4] Pickup Empty From: 
[43,28] Empty Required By: 
[44,28] Estimated Full Pickup: 
[45,4] CONTAINER3 
[45,24] 0
[45,28] 0.000
[45,34] 0.000
[45,40] 0.000
[45,46] 0.000
[46,4] Pickup Empty From: 
[46,28] Empty Required By: 
[47,28] Estimated Full Pickup: 
[48,4] Goods Details 
[51,42] Created By
[53,4] Goods Details 
[54,4]  Packs:
[54,8] 12451
[54,14] BBK
[54,16] Marks & Number:
[54,34] Goods Description:
[55,8] 12451.000
[55,14] KG
[55,16] ASM JS_MarksAndNumbers
[55,34] ASM JS_GoodsDescription
[56,8] 12451.000
[56,14] M3
[57,4] HC: HC01, HC04, HC07, HC08, HC09 HS: (US) USHS 
[60,4] BKG:11321
[61,4] Temp:-2 to 2 Centigrade
[62,4]                  Packs in Containers
[62,16] 1 x 
[62,36] 120 BBK
[62,40] 120.000 KG
[62,46] 120.000 M3
[63,16] 1 x 
[63,36] 121 BBK
[63,40] 121.000 KG
[63,46] 121.000 M3
[64,16] CONTAINER1
[64,36] 100 BBK
[64,40] 100.000 KG
[64,46] 100.000 M3
[65,16] CONTAINER2
[65,36] 12110 BBK
[65,40] 12110.000 KG
[65,46] 12110.000 M3
[67,4] Additional References
[68,4] BOL Number
[69,4] Shipper Ref
[70,4] Freight Forwarder Ref
[70,16] C00009999
[71,4] Carrier Contract No
[71,28] Contract Named Account
[73,4] Additional Parties
[74,4] Notify Party 2
[74,16] Notify Party 3
[74,28] Forwarder
[74,40] Freight Payer
[75,28] SENDING AGENT COMPANY
[76,28] SENDING AGENT ADDRESS 1
[79,28] AUSTRALIA
[91,4] Additional Instructions/Clauses
[92,4] Charge Payment Instructions
[92,38] Freight Payable At
[93,6] Freight Charges
[93,38] USSEA - Seattle, WA, United States
[94,14] ☑ Prepaid
[94,22] ☐ Collect
[94,30] ☐ Payable Elsewhere
[95,6] Optional Charge Payment Instruction by Category
[96,10] Origin Port Charge
[96,22] ☐ Prepaid
[96,30] ☐ Collect
[97,10] Origin Haulage
[97,22] ☐ Prepaid
[97,30] ☐ Collect
[98,10] Destination Port Charge
[98,22] ☐ Prepaid
[98,30] ☐ Collect
[99,10] Destination Haulage
[99,22] ☐ Prepaid
[99,30] ☐ Collect
[100,4] Goods Handling Instruction
[105,42] Created By
";
		}

		string CreateBookingRequest4Content()
		{
			return @"[2,4] Shipper
[2,16] Carrier
[2,27] BOOKING REQUEST
[3,16] CARRIER COMPANY
[4,16] CARRIER ADDRESS 1
[7,16] AUSTRALIA
[10,4] Consignee
[10,16] Notify Party
[10,28] Carrier Booking References
[14,28] Carrier Booking Office
[15,28]  - 
[16,28] Origin
[16,40] Destination
[17,28] USSEA - SEATTLE, WA, UNITED STATES
[17,40] AUSYD - SYDNEY, AUSTRALIA
[18,4] Vessel
[18,16] Lloyds/IMO
[18,28] Voyage
[18,40] Mode
[18,44] Pickup/Delivery
[19,40] FCL
[19,44] ☑ Door Pickup
[20,44] ☑ Door Delivery
[21,4] Place of Receipt
[21,16] Place of Delivery
[21,28] Port of Load
[21,40] Port of Discharge
[22,4] USSEA - SEATTLE, WA, UNITED STATES
[22,16] AUSYD - SYDNEY, AUSTRALIA
[22,28] USSEA - SEATTLE, WA, UNITED STATES
[22,40] AUSYD - SYDNEY, AUSTRALIA
[23,4] Earliest Departure
[23,16] Latest Delivery
[23,40] Nature of Cargo
[24,40] ☐ Hazardous
[24,46] ☐ Out Of Gauge
[26,4] Transport Details
[27,4] #
[27,6] Mode
[27,10] Type
[27,16] Vessel
[27,24] Voyage
[27,28] Load
[27,32] ETD
[27,40] Discharge
[27,44] ETA
[28,4] 1
[28,6] SEA
[28,10] MAI
[28,28] USSEA
[28,40] AUSYD
[30,4] Equipment Details 
[31,4] Container
[31,16] ISO Type
[31,20] Quality
[31,24] Packs
[31,28] Net (KG)
[31,34] Tare (KG)
[31,40] Gross (KG)
[31,46] Volume (M3)
[32,4] 1 Container(s) 
[32,24] 120
[32,28] 120.000
[32,34] 0.000
[32,40] 120.000
[32,46] 120.000
[33,4] Pickup Empty From: 
[33,28] Empty Required By: 
[34,28] Estimated Full Pickup: 
[35,4] 1 Container(s) 
[35,24] 121
[35,28] 121.000
[35,34] 0.000
[35,40] 121.000
[35,46] 121.000
[36,4] Pickup Empty From: 
[36,28] Empty Required By: 
[37,28] Estimated Full Pickup: 
[38,4] CONTAINER1 
[38,24] 100
[38,28] 100.000
[38,34] 0.000
[38,40] 100.000
[38,46] 100.000
[39,4] Pickup Empty From: 
[39,28] Empty Required By: 
[40,28] Estimated Full Pickup: 
[41,4] Customer Load Reference:
[41,16] NUM1
[42,4] CONTAINER2 
[42,24] 12110
[42,28] 12110.000
[42,34] 0.000
[42,40] 12110.000
[42,46] 12110.000
[43,4] Pickup Empty From: 
[43,28] Empty Required By: 
[44,28] Estimated Full Pickup: 
[45,4] CONTAINER3 
[45,24] 0
[45,28] 0.000
[45,34] 0.000
[45,40] 0.000
[45,46] 0.000
[46,4] Pickup Empty From: 
[46,28] Empty Required By: 
[47,28] Estimated Full Pickup: 
[50,42] Created By
[52,4] Equipment Details 
[53,4] Container
[53,16] ISO Type
[53,20] Quality
[53,24] Packs
[53,28] Net (KG)
[53,34] Tare (KG)
[53,40] Gross (KG)
[53,46] Volume (M3)
[54,4] PICKUPDELIVERY (Shipper Owned)
[54,16] 22G0 
[54,24] 0
[54,28] 0.000
[54,34] 2280.000
[54,40] 3280.000
[54,46] 0.000
[55,4] Empty Pickup: 
[55,40] Empty Req. By: 
[56,4] Full Pickup:ORG1, UNIT 1, 1 LOST LANE, SYDNEY, AU
[56,40] Est. Full Pickup:01-Nov-21 00:00
[57,4] Full Pickup:ORG2, UNIT 2, 2 LOST LANE, SYDNEY, AU
[57,40] Est. Full Pickup:01-Dec-21 00:00
[58,4] Full Pickup:ORG3, UNIT 3, 3 LOST LANE, SYDNEY, AU
[58,40] Est. Full Pickup:01-Nov-21 00:00
[59,4] Full Pickup:ORG4, UNIT 4, 4 LOST LANE, SYDNEY, AU
[59,40] Est. Full Pickup:01-Dec-21 00:00
[60,4] Full Delivery:ORG5, UNIT 5, 5 LOST LANE, SYDNEY, AU
[60,40] Est. Full Delivery:01-Nov-21 00:00
[61,4] Full Delivery:ORG6, UNIT 6, 6 LOST LANE, SYDNEY, AU
[61,40] Est. Full Delivery:01-Dec-21 00:00
[62,4] Full Delivery:ORG7, UNIT 7, 7 LOST LANE, SYDNEY, AU
[62,40] Est. Full Delivery:01-Nov-21 00:00
[63,4] Full Delivery:ORG8, UNIT 8, 8 LOST LANE, SYDNEY, AU
[63,40] Est. Full Delivery:01-Dec-21 00:00
[64,4] Goods Details 
[65,4]  Packs:
[65,8] 120
[65,14] BLC
[65,16] Marks & Number:
[65,34] Goods Description:
[66,8] 120.000
[66,14] KG
[66,16] BaleCompressed2 MarksAndNumbers
[66,34] BaleCompressed2 DetailedDescription
[67,8] 120.000
[67,14] M3
[68,4] HC: HC07 
[72,4] Temp:-2 to 3 Centigrade
[72,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[73,4]                  Packs in Containers
[73,16] 1 x 
[73,36] 120 BLC
[73,40] 120.000 KG
[73,46] 120.000 M3
[74,4]  Packs:
[74,8] 121
[74,14] TOT
[74,16] Marks & Number:
[74,34] Goods Description:
[75,8] 121.000
[75,14] KG
[75,16] Tote MarksAndNumbers
[75,34] Tote DetailedDescription
[76,8] 121.000
[76,14] M3
[77,4] HC: HC08 
[81,4] Temp:-2 to 3 Centigrade
[81,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[82,4]                  Packs in Containers
[82,16] 1 x 
[82,36] 121 TOT
[82,40] 121.000 KG
[82,46] 121.000 M3
[83,4]  Packs:
[83,8] 100
[83,14] BLC
[83,16] Marks & Number:
[83,34] Goods Description:
[84,8] 100.000
[84,14] KG
[84,16] BaleCompressed1 MarksAndNumbers
[84,34] BaleCompressed1 DetailedDescription
[85,8] 100.000
[85,14] M3
[86,4] HC: HC01 HS: (US) USHS 
[89,4] BKG:11321
[90,4] Temp:-2 to 3 Centigrade
[90,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[91,4]                  Packs in Containers
[91,16] CONTAINER1
[91,36] 100 BLC
[91,40] 100.000 KG
[91,46] 100.000 M3
[92,4]  Packs:
[92,8] 110
[92,14] TUB
[92,16] Marks & Number:
[92,34] Goods Description:
[93,8] 110.000
[93,14] KG
[93,16] Tube MarksAndNumbers
[93,34] Tube DetailedDescription
[94,8] 110.000
[94,14] M3
[95,4] HC: HC04 
[98,4] BKG:11321
[99,4] Temp:-2 to 3 Centigrade
[99,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[100,4]                  Packs in Containers
[100,16] CONTAINER2
[100,36] 110 TUB
[100,40] 110.000 KG
[100,46] 110.000 M3
[103,42] Created By
[105,4] Goods Details 
[106,4]  Packs:
[106,8] 12000
[106,14] TUB
[106,16] Marks & Number:
[106,34] Goods Description:
[107,8] 12000.000
[107,14] KG
[107,16] Tube MarksAndNumbers
[107,34] Tube DetailedDescription
[108,8] 12000.000
[108,14] M3
[109,4] HC: HC09 
[113,4] Temp:-2 to 2 Centigrade
[113,16] Height: 0.000M    Length: 0.000M    Width: 0.000M
[114,4]                  Packs in Containers
[114,16] CONTAINER2
[114,36] 12000 TUB
[114,40] 12000.000 KG
[114,46] 12000.000 M3
[116,4] Additional References
[117,4] BOL Number
[118,4] Shipper Ref
[118,16] JSASM
[119,4] Freight Forwarder Ref
[119,16] C00009999
[120,4] Carrier Contract No
[120,28] Contract Named Account
[122,4] Additional Parties
[123,4] Notify Party 2
[123,16] Notify Party 3
[123,28] Forwarder
[123,40] Freight Payer
[124,28] SENDING AGENT COMPANY
[125,28] SENDING AGENT ADDRESS 1
[128,28] AUSTRALIA
[131,40] Customs Broker
[132,40] CONSPA
[133,40] UNIT 15
[134,40] 5 LOST LANE
[135,40] SYDNEY
[135,48] NSW
[136,40] AUSTRALIA
[136,48] 2000
[140,4] Additional Instructions/Clauses
[141,4] Charge Payment Instructions
[141,38] Freight Payable At
[142,6] Freight Charges
[142,38] USSEA - Seattle, WA, United States
[143,14] ☑ Prepaid
[143,22] ☐ Collect
[143,30] ☐ Payable Elsewhere

[144,6] Optional Charge Payment Instruction by Category
[145,10] Origin Port Charge
[145,22] ☐ Prepaid
[145,30] ☐ Collect
[146,10] Origin Haulage
[146,22] ☐ Prepaid
[146,30] ☐ Collect
[147,10] Destination Port Charge
[147,22] ☐ Prepaid
[147,30] ☐ Collect
[148,10] Destination Haulage
[148,22] ☐ Prepaid
[148,30] ☐ Collect
[149,4] Goods Handling Instruction
[154,42] Created By";
		}
	}
}
