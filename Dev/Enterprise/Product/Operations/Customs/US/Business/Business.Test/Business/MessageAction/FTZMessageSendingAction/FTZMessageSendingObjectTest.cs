using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FTZMessageSendingObject))]
	sealed class FTZMessageSendingObjectTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		[TestDate(2009, 1, 1)]
		public void TestIFTZHeaderMembers_SingleConveyance()
		{
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			Declaration.Bills.AddNew();
			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.FillWithValidTestData();
			warehouseOrg.OH_FullName = "Warehouse Organisation";
			warehouseOrg.OH_IsWarehouseClient = true;
			warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "887766554433");
			AssertEquals("Validation Mode", true, Declaration.IsFTZAdmissionValidationMode);
			AssertEquals("Entry Filer should be set by default", "SV9", Declaration.US_EntryFilerCode);
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
			Declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "654-73-1975");
			Declaration.IOROrgPK = importer.PK;
			AssertEquals("Include PTT in admission should be readonly", true, Declaration.US_F_IncludePTT_ReadOnly);
			var sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
			IFTZHeader ftzHeader = sendingObject;
			IFTZConveyance fTZConveyance = ftzHeader.Conveyances.First();
			Declaration.FTZZoneID = "1530A01";
			AssertEquals("ForeignTradeZone", "1530A0109", ftzHeader.FTZAdmissionNumber);
			AssertEquals("ForeignTradeZone", "1530A01|09|", Declaration.FTZAdmissionNumber);
			Declaration.FTZControlNumber = "00000004";
			AssertEquals("ForeignTradeZone", "1530A010900000004", ftzHeader.FTZAdmissionNumber);
			AssertEquals("ForeignTradeZone", "1530A01|09|00000004", Declaration.FTZAdmissionNumber);
			Declaration.FTZControlNumber = ZString.Empty;
			AssertEquals("ForeignTradeZone", "1530A0109", ftzHeader.FTZAdmissionNumber);
			AssertEquals("ForeignTradeZone", "1530A01|09|", Declaration.FTZAdmissionNumber);
			Declaration.FTZZoneID = ZString.Empty;
			AssertEquals("ForeignTradeZone", "", ftzHeader.FTZAdmissionNumber);
			AssertEquals("ForeignTradeZone", "|09|", Declaration.FTZAdmissionNumber);
			Declaration.FTZYear = ZString.Empty;
			AssertEquals("ForeignTradeZone", "", ftzHeader.FTZAdmissionNumber);
			Declaration.FTZControlNumber = "00000045";
			AssertEquals("Admission Number", "00000045", ftzHeader.FTZAdmissionNumber);
			AssertEquals("Control Number", "00000045", Declaration.FTZControlNumber);
			AssertEquals("Admission Number", "||00000045", Declaration.FTZAdmissionNumber);
			Declaration.FTZControlNumber = "00|00045";
			AssertEquals("Control Number", "0000045", Declaration.FTZControlNumber);
			AssertEquals("Transport Mode", TransportModeCodes.Codes.VesselNonContainer, fTZConveyance.TransportMode);
			Declaration.US_SchDEntry = "1234";
			AssertEquals("FTZ Port Code", "1234", ftzHeader.PortCode);
			Declaration.US_F_DirectDelivery = true;
			AssertEquals("Direct Delivery Indicator", true, ftzHeader.DirectDeliveryIndicator);
			AssertEquals("Include PTT in admission should not be readonly", false, Declaration.US_F_IncludePTT_ReadOnly);
			Declaration.US_EntryFilerCode = "XJ5";
			AssertEquals("Entry Filer Code", "XJ5", ftzHeader.EntryFilerCode);
			Declaration.US_F_RoutingDetails = "3311ABC12";
			AssertEquals("ABI Routing code", "3311ABC12", ftzHeader.ABIRoutingCode);
			AssertEquals("887766554433", ftzHeader.IRSIdentifier);
			warehouseOrg.CustomsCodes.RemoveAndDeleteAll();
			var irsID = warehouseOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "887766554433");
			AssertEquals("IRS Identifier - EIN should be taken from organization", "887766554433", ftzHeader.IRSIdentifier);
			irsID.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			AssertEquals("IRS Identifier - EIN should be taken from organization", "887766554433", ftzHeader.IRSIdentifier);
			irsID.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertEquals("IRS Identifier - EIN should be taken from organization", "887766554433", ftzHeader.IRSIdentifier);
			Declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			AssertEquals("Admission Type", FTZAdmissionTypeCodeList.Codes.RegularAdmission, ftzHeader.AdmissionType);
			var uscarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery());
			Declaration.JE_MasterBillIssuerSCAC = uscarrier.UI_Code;
			AssertEquals("SCAC Identifier", uscarrier.UI_Code, fTZConveyance.CarrierSCAC);
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			Declaration.JE_VesselName = vessel.RV_Code;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
			AssertEquals("Import Conveyance Name", uscarrier.UI_Name, fTZConveyance.ConveyanceName);
			AssertEquals("Transport Mode", TransportModeCodes.Codes.AirNonContainer, fTZConveyance.TransportMode);
			Declaration.JE_VoyageFlightNo = "1234567890";
			AssertEquals("VoyageFlightNo", "1234567890", fTZConveyance.VoyageNumber);
			Declaration.US_DateOfExport = new ZDateTime(2011, 10, 24);
			AssertEquals("Export Date", new ZDate(2011, 10, 24), fTZConveyance.ExportDate);
			Declaration.JE_DateOfArrival = new ZDateTime(2011, 10, 25);
			AssertEquals("Import Date", new ZDate(2011, 10, 25), fTZConveyance.ImportDate);
			AssertEquals("Estimated Date of Arrival should be defaulted from Import Date", Declaration.JE_DateOfArrival, Declaration.JE_DateOfFirstArrival);
			AssertEquals("Estimated Date of Arrival", Declaration.JE_DateOfFirstArrival, fTZConveyance.EstimatedDateOfArrival);
			Declaration.US_SchDArrival = "1234";
			AssertEquals("Port of Unlading", "1234", fTZConveyance.PortOfUnlading);
			Declaration.JE_DateOfFirstArrival = new ZDateTime(2011, 10, 26);
			AssertEquals("Estimated Date of Arrival", new ZDate(2011, 10, 25), fTZConveyance.EstimatedDateOfArrival);
			AssertEquals("No. of bills", 1, ftzHeader.Bills.Count());
			Declaration.FTZYear = "09";
			AssertEquals("Calendar Year", 9, ftzHeader.CalendarYear);
			Declaration.US_F_IncludePTT = true;
			Declaration.US_F_DirectDelivery = false;
			AssertEquals("Include PTT in admission indicator should be cleared", false, Declaration.US_F_IncludePTT);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("Validation Mode", false, Declaration.IsFTZAdmissionValidationMode);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			Declaration.JE_VoyageFlightNo = "143";
			AssertEquals("0143", fTZConveyance.VoyageNumber);
			Declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.ZoneToZone;
			AssertEquals("Include PTT in admission should not be readonly", false, Declaration.US_F_IncludePTT_ReadOnly);
			Declaration.US_F_IncludePTT = true;
			Declaration.US_F_DirectDelivery = false;
			AssertEquals("Include PTT in admission indicator should NOT be cleared", true, Declaration.US_F_IncludePTT);
			Declaration.US_US_NKLocationOfGoods = "LWH1";
			AssertEquals("LWH1", ftzHeader.FirmsIdentifier);
			AssertEquals("654-73-1975", ftzHeader.ImporterOfRecordID);
			sendingObject.US_FTZContactName = "Dan Brown";
			AssertEquals("Dan Brown", ftzHeader.ContactName);
			sendingObject.US_FTZContactPhone = "0288108815";
			AssertEquals("0288108815", ftzHeader.ContactPhone);
			sendingObject.US_DeleteConveyance = true;
			sendingObject.US_ChangeOrAddBillOfLading = true;
			sendingObject.US_DeleteBillOfLading = true;
			sendingObject.US_ChangeOrAddHTSLine = true;
			sendingObject.US_DeleteHTSLine = true;
			sendingObject.US_ChangeAdmittedQuantity = true;
			sendingObject.US_CancelOrAddPTT = true;
			sendingObject.US_OtherReason = true;
			AssertEquals("0203040506070809", string.Join("", ftzHeader.ReasonCodes));
			sendingObject.US_Remarks = "Another Reason";
			AssertEquals("Another Reason", ftzHeader.Remarks);
		}

		[TestDate(2000, 1, 1, 12, 0, 0)]
		public void TestFTZHeaderMembers_Splits()
		{
			CombineAssertions(() =>
			{
				Declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				Declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
				var bill1 = Declaration.Bills.AddNew();
				bill1.US_SESplitShip = true;
				var carrier1 = Factory.New<USCarrierCombined>();
				carrier1.UI_Code = "C1";
				carrier1.UI_Name = "Air Company 1";
				var split1 = bill1.ITAndSplitDetails.AddNew();
				split1.US_ITNumber = "1";
				split1.US_ArrivalDate = ZDateTime.Now;
				split1.US_CarrierCode = "C1";
				split1.US_FlightNumber = "FLT01";
				var split2 = bill1.ITAndSplitDetails.AddNew();
				split2.US_ITNumber = "2";
				split2.US_ArrivalDate = ZDateTime.Now.AddDays(1);
				split2.US_CarrierCode = "C1";
				split2.US_FlightNumber = "FLT02";
				var bill2 = Declaration.Bills.AddNew();
				bill2.US_SESplitShip = true;
				var carrier2 = Factory.New<USCarrierCombined>();
				carrier2.UI_Code = "C2";
				carrier2.UI_Name = "Air Company 2";
				var split3 = bill2.ITAndSplitDetails.AddNew();
				split3.US_ITNumber = "3";
				split3.US_ArrivalDate = ZDateTime.Now;
				split3.US_CarrierCode = "C2";
				split3.US_FlightNumber = "FLT11";
				var split4 = bill2.ITAndSplitDetails.AddNew();
				split4.US_ITNumber = "4";
				split4.US_ArrivalDate = ZDateTime.Now.AddDays(3);
				split4.US_CarrierCode = "C2";
				split4.US_FlightNumber = "FLT12";
				var invoice1 = Declaration.Invoices.AddNew();
				invoice1.JZ_CU_RelatedHouseBill = bill1.PK;
				invoice1.US_SplitShipmentDetail = "C1/FLT01/01-JAN-00";
				invoice1.InvoiceLines.AddNew().JI_Tariff = "00001";
				var invoice2 = Declaration.Invoices.AddNew();
				invoice2.JZ_CU_RelatedHouseBill = bill1.PK;
				invoice2.US_SplitShipmentDetail = "C1/FLT02/02-JAN-00";
				invoice2.InvoiceLines.AddNew().JI_Tariff = "00002";
				var invoice3 = Declaration.Invoices.AddNew();
				invoice3.JZ_CU_RelatedHouseBill = bill2.PK;
				invoice3.US_SplitShipmentDetail = "C2/FLT11/01-JAN-00";
				invoice3.InvoiceLines.AddNew().JI_Tariff = "00003";
				var invoice4 = Declaration.Invoices.AddNew();
				invoice4.JZ_CU_RelatedHouseBill = bill2.PK;
				invoice4.US_SplitShipmentDetail = "C2/FLT12/04-JAN-00";
				invoice4.InvoiceLines.AddNew().JI_Tariff = "00004";
				var invoice5 = Declaration.Invoices.AddNew();
				invoice5.JZ_CU_RelatedHouseBill = bill2.PK;
				invoice5.InvoiceLines.AddNew().JI_Tariff = "00005";
				var carrier3 = Factory.New<USCarrierCombined>();
				carrier3.UI_Code = "C0";
				carrier3.UI_Name = "Air Company 0";
				Declaration.JE_MasterBillIssuerSCAC = "C0";
				Declaration.JE_VoyageFlightNo = "FLT00";
				Declaration.US_DateOfExport = ZDateTime.Now.AddDays(-1).Date;
				Declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(10).Date;
				Declaration.US_SchDArrival = "1234";
				Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				var invoice0 = Declaration.Invoices.AddNew();
				invoice0.JZ_CU_RelatedHouseBill = Declaration.Bills.AddNew().PK;
				invoice0.InvoiceLines.AddNew().JI_Tariff = "00000";
				declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				var sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
				IFTZHeader ftzHeader = sendingObject;
				AssertEquals("Transport Mode", Core.Constants.TransportModes.Air, ftzHeader.TransportMode);
				AssertEquals("Conveyance number should be", 6, ftzHeader.Conveyances.Count());
				AssertEquals("Export date", true, ftzHeader.Conveyances.All(_ => _.ExportDate == ZDateTime.Now.AddDays(-1).Date));
				AssertEquals("Point of lading", true, ftzHeader.Conveyances.All(_ => _.PortOfUnlading == "1234"));
				AssertEquals("Transport mode", true, ftzHeader.Conveyances.All(_ => _.TransportMode == TransportModeCodes.Codes.AirNonContainer));
				AssertEquals("Non-split bill's carrier code", "C0", ftzHeader.Conveyances.First().CarrierSCAC);
				AssertEquals("Non-split bill's conveyance name", "Air Company 0", ftzHeader.Conveyances.First().ConveyanceName);
				AssertEquals("Non-split bill's voyage number", "FLT00", ftzHeader.Conveyances.First().VoyageNumber);
				AssertEquals("Non-split bill's import date", ZDateTime.Now.AddDays(10).Date, ftzHeader.Conveyances.First().ImportDate);
				AssertEquals("Non-split bill's Arrival date", ZDateTime.Now.AddDays(10).Date, ftzHeader.Conveyances.First().EstimatedDateOfArrival);
				AssertEquals("Non-split bill", "00000", ftzHeader.Conveyances.First().Bills.Single().Lines.Single().Tariff);
				var splits = ftzHeader.Conveyances.Skip(1).OrderBy(_ => _.VoyageNumber);
				AssertEquals("Split bill's carrier code", ",C1,C1,C2,C2", string.Join(",", splits.Select(_ => _.CarrierSCAC)));
				AssertEquals("Split bill's conveyance name", ",Air Company 1,Air Company 1,Air Company 2,Air Company 2", string.Join(",", splits.Select(_ => _.ConveyanceName)));
				AssertEquals("Split bill's voyage number", ",FLT01,FLT02,FLT11,FLT12", string.Join(",", splits.Select(_ => _.VoyageNumber)));
				AssertEquals("Split bill's import date", "1,1,2,1,4", string.Join(",", splits.Select(_ => _.ImportDate.Day)));
				AssertEquals("Split bill's Arrival date  Without ShipmentDetails Selected ", ZDateTime.Now.AddDays(10).Date, splits.First().EstimatedDateOfArrival);
				AssertEquals("Split bill's Arrival date", "1,2,1,4", string.Join(",", splits.Skip(1).Select(_ => _.EstimatedDateOfArrival.Day)));
				AssertEquals("First split bill", "00001", ftzHeader.Conveyances.Skip(1).First().Bills.Single().Lines.Single().Tariff);
				AssertEquals("Second split bill", "00002", ftzHeader.Conveyances.Skip(2).First().Bills.Single().Lines.Single().Tariff);
				AssertEquals("Third split bill", "00003", ftzHeader.Conveyances.Skip(3).First().Bills.Single().Lines.Single().Tariff);
				AssertEquals("Fourth split bill", "00004", ftzHeader.Conveyances.Skip(4).First().Bills.Single().Lines.Single().Tariff);
				AssertEquals("Fifth split bill", "00005", ftzHeader.Conveyances.Skip(5).First().Bills.Single().Lines.Single().Tariff);
			});
		}

		public void TestSetDefaults()
		{
			GlbStaff.CurrentUser.GS_FullName = "Timothy Kensington-Double-Barrelled-Shotgun";
			GlbStaff.CurrentUser.GS_WorkPhone = "+18005550100";
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
			AssertEquals("Contact Name truncated due to max length", "Timothy Kensington-Double-Barrelled-Shot", sendingObject.US_FTZContactName);
			AssertEquals("Contact Phone Number", "8005550100", sendingObject.US_FTZContactPhone);
			var branch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			branch.GB_Phone = "+1 738 294 5000";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Dan Brown";
			staff.GS_WorkPhone = "+86 158 5050 3354";
			staff.GS_GB_HomeBranch = branch.PK;
			USCustomsDataRegistry.Instance.CargoReleaseFTZContact.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, staff.PK.ToGuid());
			sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
			AssertEquals("Dan Brown", sendingObject.US_FTZContactName);
			AssertEquals("15850503354", sendingObject.US_FTZContactPhone);
			staff.GS_WorkPhone = ZString.Empty;
			sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
			AssertEquals("Dan Brown", sendingObject.US_FTZContactName);
			AssertEquals("7382945000", sendingObject.US_FTZContactPhone);
		}

		public void TestValidateUS_FTZContactName()
		{
			var errorMsg = "Contact Name is mandatory when Phone Number is entered.";
			var sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
			sendingObject.US_FTZContactPhone = ZString.Empty;
			sendingObject.US_FTZContactName = ZString.Empty;
			AssertNoMessageError(sendingObject.US_FTZContactNameInfo, errorMsg);
			sendingObject.US_FTZContactPhone = "12345678";
			AssertHasMessageError(sendingObject.US_FTZContactNameInfo, errorMsg);
			sendingObject.US_FTZContactName = "Dino";
			AssertNoMessageError(sendingObject.US_FTZContactNameInfo, FTZMessageSendingObject.FTZContactNameWithPeriod);
			sendingObject.US_FTZContactName = "Dino.";
			AssertHasMessageError(sendingObject.US_FTZContactNameInfo, FTZMessageSendingObject.FTZContactNameWithPeriod);
		}

		public void TestValidateUS_Remarks()
		{
			var errorMsg = "Remarks is mandatory when Other/Remarks is ticked.";
			var sendingObject = new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);
			sendingObject.US_OtherReason = true;
			AssertHasMessageError(sendingObject.US_RemarksInfo, errorMsg);
			sendingObject.US_Remarks = "Random Reason";
			AssertNoMessageError(sendingObject.US_RemarksInfo, errorMsg);
			sendingObject.US_OtherReason = false;
			AssertEquals(ZString.Empty, sendingObject.US_Remarks);
			AssertNoMessageError(sendingObject.US_RemarksInfo, errorMsg);
		}

		protected override BusinessObject GetNewBusinessObject() => new FTZMessageSendingObject(Declaration, UpdateActionCode.Add);

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				}

				return declaration;
			}
		}
	}
}
