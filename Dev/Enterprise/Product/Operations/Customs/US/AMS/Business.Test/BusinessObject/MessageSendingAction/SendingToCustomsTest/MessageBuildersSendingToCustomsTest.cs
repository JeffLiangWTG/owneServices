// Set this #define to allow test messages to be sent from the test harness
//#define SendTestMessagesToCustoms

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class MessageBuildersSendingToCustomsTest : MessageBuilderTestCase
	{
		public static class Constants
		{
			public static string VNo1
			{
#if SendTestMessagesToCustoms
				get
				{

					return MessageBuildersSendingToCustomsTest.GetVNo("1");
				}
#else
				get { return "VNW50060019"; }
#endif
			}

			public static string VNo2
			{
#if SendTestMessagesToCustoms
				get
				{
					return MessageBuildersSendingToCustomsTest.GetVNo("2");
				}
#else
				get { return "VNW50060027"; }
#endif
			}
			public static string VNo3
			{
#if SendTestMessagesToCustoms
				get
				{
					return MessageBuildersSendingToCustomsTest.GetVNo("3");
				}
#else
				get { return "VNW50060035"; }
#endif
			}
			public static string VNo4
			{
#if SendTestMessagesToCustoms
				get
				{
					return MessageBuildersSendingToCustomsTest.GetVNo("4");
				}
#else
				get { return "VNW50060043"; }
#endif
			}
			public static string VNo5
			{
#if SendTestMessagesToCustoms
				get
				{
					return MessageBuildersSendingToCustomsTest.GetVNo("5");
				}
#else
				get { return "VNW50060050"; }
#endif
			}

			public const string InBondCarrierID = "11-765432100";

			public static string InBondNumber1
			{
#if SendTestMessagesToCustoms
				get
				{

					return MessageBuildersSendingToCustomsTest.GetInBondNumber("1");
				}
#else
				get { return "510035971"; }
#endif
			}

			public static string InBondNumber2
			{
#if SendTestMessagesToCustoms
				get
				{

					return MessageBuildersSendingToCustomsTest.GetInBondNumber("2");
				}
#else
				get { return "515600046"; }
#endif
			}

			public static string InBondNumber3
			{
#if SendTestMessagesToCustoms
				get
				{

					return MessageBuildersSendingToCustomsTest.GetInBondNumber("3");
				}
#else
				get { return "535118511"; }
#endif
			}

			public static string InBondNumber4
			{
#if SendTestMessagesToCustoms
				get
				{

					return MessageBuildersSendingToCustomsTest.GetInBondNumber("4");
				}
#else
				get { return "541012791"; }
#endif
			}

			public static string InBondNumber5
			{
#if SendTestMessagesToCustoms
				get
				{

					return MessageBuildersSendingToCustomsTest.GetInBondNumber("5");
				}
#else
				get { return "565434940"; }
#endif
			}
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08)]
#endif
		public void TestMVOCCOceanUserTest1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "01535", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "72357", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			CreateNewOrGetExistingCusCodeListForCUSOFType("1234");
			var header = CreateHeader("TEST STEP 1", "1111111", 3, GBLON.RL_Code, USLAX.RL_Code, "XXXW");
			header.BH_PortUnladingDCode = "1234";

			var bill_1101 = CreateNewBillWithCargoDetails(header, "1101", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_1102 = CreateNewBillWithCargoDetails(header, "1102", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_1103 = CreateNewBillWithCargoDetails(header, "1103", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			bill_1103.B0_MasterInBondIndicator = ZBool.True;
			var inBondMoveHeader = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, Constants.VNo5, 3);
			inBondMoveHeader.BM_ArrivalDate = header.BH_ETA.AddHours(1);
			var inBondMoveDetailBill_1103 = inBondMoveHeader.MovementDetails.AddNew(bill_1103.PK);
			inBondMoveDetailBill_1103.B9_ForeignDestPortKCode = ForeignPorts[0].ZZD_Code;
			inBondMoveDetailBill_1103.B9_MonetaryValue = 1103m;

			var bill_1104 = CreateNewBillWithCargoDetails(header, "1104", BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard);
			var bill_1105 = CreateNewBillWithCargoDetails(header, "1105", BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard);
			var bill_1106 = CreateNewBillWithCargoDetails(header, "1106", BillOfLadingStatusIndicatorList.Codes.MasterFROB);
			var bill_1107 = CreateNewBillWithCargoDetails(header, "1107", BillOfLadingStatusIndicatorList.Codes.FROB);
			var billRef_1107 = bill_1107.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1106.B0_IssuerCode + bill_1106.B0_MasterBillNumber);

			var bill_1108 = CreateNewBillWithCargoDetails(header, "1108", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			var billRef_1108 = bill_1108.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101.B0_IssuerCode + bill_1101.B0_MasterBillNumber);

			var bill_1109 = CreateNewBillWithCargoDetails(header, "1109", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			var billRef_1109 = bill_1109.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101.B0_IssuerCode + bill_1101.B0_MasterBillNumber);

			var bill_1110 = CreateNewBillWithCargoDetails(header, "1110", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			var billRef_1110 = bill_1110.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101.B0_IssuerCode + bill_1101.B0_MasterBillNumber);

			var bill_1111 = CreateNewBillWithCargoDetails(header, "1111", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			var billRef_1111 = bill_1111.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101.B0_IssuerCode + bill_1101.B0_MasterBillNumber);

			var bill_1112 = CreateNewBillWithCargoDetails(header, "1112", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			var billRef_1112 = bill_1112.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1102.B0_IssuerCode + bill_1102.B0_MasterBillNumber);

			var bill_1113 = CreateNewBillWithCargoDetails(header, "1113", BillOfLadingStatusIndicatorList.Codes.FROB);
			var billRef_1113 = bill_1113.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1106.B0_IssuerCode + bill_1106.B0_MasterBillNumber);

			var bill_1114 = CreateNewBillWithCargoDetails(header, "1114", BillOfLadingStatusIndicatorList.Codes.FROB);
			var billRef_1114 = bill_1114.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1106.B0_IssuerCode + bill_1106.B0_MasterBillNumber);

			var bill_1115 = CreateNewBillWithCargoDetails(header, "1115", BillOfLadingStatusIndicatorList.Codes.FROB);
			var billRef_1115 = bill_1115.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1106.B0_IssuerCode + bill_1106.B0_MasterBillNumber);

			var bill_1116 = CreateNewBill(header, "1116", BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade);
			var bill_1116Container = CreateNewContainer(bill_1116.MovementDetail, GenerateContainerNumber("1116"), "1116");
			bill_1116Container.BC_IsEmpty = ZBool.True;
			bill_1116Container.BC_RL_NKForeignPort = GBLON.RL_Code;
			var bill_1116Commodity = CreateNewCommodity(bill_1116Container, 1585m, "GOODS FOR 1116", "MARKS FOR 1116");
			var bill_1117 = CreateNewBillWithCargoDetails(header, "1117", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			var bill_1118 = CreateNewBillWithCargoDetails(header, "1118", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			var bill_1119 = CreateNewBillWithCargoDetails(header, "1119", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			var bill_1120 = CreateNewBillWithCargoDetails(header, "1120", BillOfLadingStatusIndicatorList.Codes.RegularBill);

#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);
			SendManifestOriginalMessage(header);
			SendAndAssertVesselDepartureMessage(header);

			//transmit vessel stow plan

			var messageAction = new MessageSendingAction(header, ActionCode.VesselArrival);
			AssertNoMessageErrorsBeforeSending(messageAction);
			messageAction.CreateAMSMessages();
			Factory.Save();
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08, 11, 27, 00)]
#endif
		public void TestMVOCCOceanUserTest2()
		{
			CreateNewOrGetExistingCusCodeListForCUSOFType("1234");
			var header = CreateHeader("TEST STEP 2", "1111112", 3, GBLON.RL_Code, USBAL.RL_Code, "XXXW");
			header.BH_PortUnladingDCode = "1234";

			var inBondMoveHeaderIT1 = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, Constants.VNo1, 3);
			inBondMoveHeaderIT1.BM_DestinationPortCode = "0401";
			inBondMoveHeaderIT1.BM_ArrivalDate = header.BH_ETA.AddHours(1);
			var inBondMoveHeaderIT2 = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, Constants.VNo2, 3);
			inBondMoveHeaderIT2.BM_DestinationPortCode = "0401";
			inBondMoveHeaderIT2.BM_ArrivalDate = header.BH_ETA.AddHours(1);
			var inBondMoveHeaderTAndE = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, Constants.VNo3, 3);
			inBondMoveHeaderTAndE.BM_DestinationPortCode = "0401";
			inBondMoveHeaderTAndE.BM_ArrivalDate = header.BH_ETA.AddHours(2);
			var inBondMoveHeaderIE = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._3ImmediateExport, Constants.VNo4, 3);
			inBondMoveHeaderIE.BM_ArrivalDate = header.BH_ETA.AddMinutes(90);
			inBondMoveHeaderIE.BM_DestinationPortCode = header.BH_PortUnladingDCode;

			for (ZInt i = 2101; i <= 2120; i++)
			{
				var bill = CreateNewBillWithCargoDetails(header, i.ToString(), BillOfLadingStatusIndicatorList.Codes.RegularBill);
				bill.B0_MasterInBondIndicator = i != 2119 && i != 2120;

				if (i == 2119)
				{
					bill.CustomsBroker.E2_OA_Address = CustomsBroker.MainAddress.PK;
				}
				else if (bill.B0_MasterInBondIndicator)
				{
					CusInBondMoveDetail inBondMoveDetail;
					if (i == 2101)
					{
						inBondMoveDetail = inBondMoveHeaderIT1.MovementDetails.AddNew(bill.PK);
					}
					else if (i.IsInRange(2102, 2106))
					{
						inBondMoveDetail = inBondMoveHeaderIT2.MovementDetails.AddNew(bill.PK);
					}
					else if (i.IsInRange(2107, 2112))
					{
						inBondMoveDetail = inBondMoveHeaderTAndE.MovementDetails.AddNew(bill.PK);
						inBondMoveDetail.B9_ForeignDestPortKCode = ForeignPorts[i - 2100].ZZD_Code;
					}
					else
					{
						inBondMoveDetail = inBondMoveHeaderIE.MovementDetails.AddNew(bill.PK);
						inBondMoveDetail.B9_ForeignDestPortKCode = ForeignPorts[i - 2100].ZZD_Code;
					}
					inBondMoveDetail.B9_MonetaryValue = new ZDecimal(i);
				}
			}
#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);

			SendManifestOriginalMessage(header);
			SendAndAssertVesselDepartureMessage(header);
			SendAndAssertInBondDiversionMessage(inBondMoveHeaderIT1);

			var messageAction = new MessageSendingAction(header, ActionCode.VesselArrival);
			AssertNoMessageErrorsBeforeSending(messageAction);
			messageAction.CreateAMSMessages();
			Factory.Save();

			SendAndAssertInBondArrivalMessage(header);
			SendAndAssertInBondExportationMessage(header);

			SendSubsequentAndInBondArrivalMessageForBill2101(inBondMoveHeaderIT1);
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08, 11, 27, 00)]
#endif
		public void TestMVOCCOceanUserTest3()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("A304");
			CreateNewOrGetExistingCusCodeListForCUSOFType("1234");
			var header = CreateHeader("TEST STEP 3", "1111113", 7, GBLON.RL_Code, USLAX.RL_Code, "XXXW");
			header.BH_PortUnladingDCode = "1234";
			header.BH_FIRMS = "A304";

			// Conventional In-Bond
			var bill_3101 = CreateNewBillWithCargoDetails(header, "3101", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3101.B0_MasterInBondIndicator = ZBool.True;
			bill_3101.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3101InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, Constants.InBondNumber1, 7);
			var bill_3101InBondMovementDetail = bill_3101InBondMovement.MovementDetails.AddNew(bill_3101.PK);
			bill_3101InBondMovementDetail.B9_MonetaryValue = 3101m;

			var bill_3102 = CreateNewBillWithCargoDetails(header, "3102", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3102.B0_MasterInBondIndicator = ZBool.True;
			bill_3102.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3102InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, Constants.InBondNumber2, 7);
			var bill_3102InBondMovementDetail = bill_3102InBondMovement.MovementDetails.AddNew(bill_3102.PK);
			bill_3102InBondMovementDetail.B9_MonetaryValue = 3102m;

			var bill_3103 = CreateNewBillWithCargoDetails(header, "3103", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3103.B0_MasterInBondIndicator = ZBool.True;
			bill_3103.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3103InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, Constants.InBondNumber3, 7);
			var bill_3103InBondMovementDetail = bill_3103InBondMovement.MovementDetails.AddNew(bill_3103.PK);
			bill_3103InBondMovementDetail.B9_MonetaryValue = 3103m;
			bill_3103InBondMovementDetail.B9_ForeignDestPortKCode = ForeignPorts[3].ZZD_Code;

			var bill_3104 = CreateNewBillWithCargoDetails(header, "3104", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3104.B0_MasterInBondIndicator = ZBool.True;
			bill_3104.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3104InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, Constants.InBondNumber4, 7);
			var bill_3104InBondMovementDetail = bill_3104InBondMovement.MovementDetails.AddNew(bill_3104.PK);
			bill_3104InBondMovementDetail.B9_MonetaryValue = 3104m;
			bill_3104InBondMovementDetail.B9_ForeignDestPortKCode = ForeignPorts[4].ZZD_Code;

			var bill_3105 = CreateNewBillWithCargoDetails(header, "3105", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3105.B0_MasterInBondIndicator = ZBool.True;
			bill_3105.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3105InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._3ImmediateExport, Constants.InBondNumber5, 7);
			var bill_3105InBondMovementDetail = bill_3105InBondMovement.MovementDetails.AddNew(bill_3105.PK);
			bill_3105InBondMovementDetail.B9_MonetaryValue = 3105m;
			bill_3105InBondMovementDetail.B9_ForeignDestPortKCode = ForeignPorts[5].ZZD_Code;

			// Paperless In-Bond
			var bill_3106 = CreateNewBillWithCargoDetails(header, "3106", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3106.B0_MasterInBondIndicator = ZBool.True;
			bill_3106.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3106InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, Constants.VNo1, 7);
			var bill_3106InBondMovementDetail = bill_3106InBondMovement.MovementDetails.AddNew(bill_3106.PK);
			bill_3106InBondMovementDetail.B9_MonetaryValue = 3106m;

			var bill_3107 = CreateNewBillWithCargoDetails(header, "3107", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3107.B0_MasterInBondIndicator = ZBool.True;
			bill_3107.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3107InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, Constants.VNo2, 7);
			var bill_3107InBondMovementDetail = bill_3107InBondMovement.MovementDetails.AddNew(bill_3107.PK);
			bill_3107InBondMovementDetail.B9_MonetaryValue = 3107m;

			var bill_3108 = CreateNewBillWithCargoDetails(header, "3108", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3108.B0_MasterInBondIndicator = ZBool.True;
			bill_3108.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3108InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, Constants.VNo3, 7);
			var bill_3108InBondMovementDetail = bill_3108InBondMovement.MovementDetails.AddNew(bill_3108.PK);
			bill_3108InBondMovementDetail.B9_MonetaryValue = 3108m;
			bill_3108InBondMovementDetail.B9_ForeignDestPortKCode = ForeignPorts[8].ZZD_Code;

			var bill_3109 = CreateNewBillWithCargoDetails(header, "3109", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3109.B0_MasterInBondIndicator = ZBool.True;
			bill_3109.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3109InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, Constants.VNo4, 7);
			var bill_3109InBondMovementDetail = bill_3109InBondMovement.MovementDetails.AddNew(bill_3109.PK);
			bill_3109InBondMovementDetail.B9_MonetaryValue = 3109m;
			bill_3109InBondMovementDetail.B9_ForeignDestPortKCode = ForeignPorts[9].ZZD_Code;

			var bill_3110 = CreateNewBillWithCargoDetails(header, "3110", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3110.B0_MasterInBondIndicator = ZBool.True;
			bill_3110.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3110InBondMovement = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._3ImmediateExport, Constants.VNo5, 7);
			var bill_3110InBondMovementDetail = bill_3110InBondMovement.MovementDetails.AddNew(bill_3110.PK);
			bill_3110InBondMovementDetail.B9_MonetaryValue = 3110m;
			bill_3110InBondMovementDetail.B9_ForeignDestPortKCode = ForeignPorts[10].ZZD_Code;

			// MVOCC
			var bill_3111 = CreateNewBillWithCargoDetails(header, "3111", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_3111Container = bill_3111.MovementDetail.Containers[0];
			bill_3111Container.BC_ContainerNum = "TURE1231112";
			bill_3111Container.BC_Seal1 = "SL3111";
			bill_3111.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3112 = CreateNewBillWithCargoDetails(header, "3112", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_3112Container = bill_3112.MovementDetail.Containers[0];
			bill_3112Container.BC_ContainerNum = "TURE1231122";
			bill_3112Container.BC_Seal1 = "SL3112";
			bill_3112.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3113 = CreateNewBillWithCargoDetails(header, "3113", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_3113Container = bill_3113.MovementDetail.Containers[0];
			bill_3113Container.BC_ContainerNum = "TURE1231132";
			bill_3113Container.BC_Seal1 = "SL3113";
			bill_3113.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3114 = CreateNewBillWithCargoDetails(header, "3114", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_3114Container = bill_3114.MovementDetail.Containers[0];
			bill_3114Container.BC_ContainerNum = "TURE1231142";
			bill_3114Container.BC_Seal1 = "SL3114";
			bill_3114.SecondaryNotifyParties.AddNewIfNotExist("XXXS");

			// Regular Bills
			var bill_3115 = CreateNewBillWithCargoDetails(header, "3115", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3115.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3116 = CreateNewBillWithCargoDetails(header, "3116", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill_3116.SecondaryNotifyParties.AddNewIfNotExist("XXXS");

			// NVOCC
			var bill_3117 = CreateNewBillWithCargoDetails(header, "3117", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_3117.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_3111.B0_IssuerCode + bill_3111.B0_MasterBillNumber);
			var bill_3117Container = bill_3117.MovementDetail.Containers[0];
			bill_3117Container.BC_ContainerNum = bill_3111Container.BC_ContainerNum;
			bill_3117Container.BC_Seal1 = bill_3111Container.BC_Seal1;
			bill_3117.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3118 = CreateNewBillWithCargoDetails(header, "3118", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_3118.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_3112.B0_IssuerCode + bill_3112.B0_MasterBillNumber);
			var bill_3118Container = bill_3118.MovementDetail.Containers[0];
			bill_3118Container.BC_ContainerNum = bill_3112Container.BC_ContainerNum;
			bill_3118Container.BC_Seal1 = bill_3112Container.BC_Seal1;
			bill_3118.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3119 = CreateNewBillWithCargoDetails(header, "3119", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_3119.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_3113.B0_IssuerCode + bill_3113.B0_MasterBillNumber);
			var bill_3119Container = bill_3119.MovementDetail.Containers[0];
			bill_3119Container.BC_ContainerNum = bill_3113Container.BC_ContainerNum;
			bill_3119Container.BC_Seal1 = bill_3113Container.BC_Seal1;
			bill_3119.SecondaryNotifyParties.AddNewIfNotExist("XXXS");
			var bill_3120 = CreateNewBillWithCargoDetails(header, "3120", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_3120.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_3114.B0_IssuerCode + bill_3114.B0_MasterBillNumber);
			var bill_3120Container = bill_3120.MovementDetail.Containers[0];
			bill_3120Container.BC_ContainerNum = bill_3114Container.BC_ContainerNum;
			bill_3120Container.BC_Seal1 = bill_3114Container.BC_Seal1;
			bill_3120.SecondaryNotifyParties.AddNewIfNotExist("XXXS");

#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);

			SendManifestOriginalMessage(header);
			SendAndAssertVesselDepartureMessage(header);
			SendAndAssertManifestAmendmentMessage(header);
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08, 11, 27, 00)]
#endif
		public void TestMVOCCOceanUserTest4()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("A304");
			CreateNewOrGetExistingCusCodeListForCUSOFType("1234");
			var header = CreateHeader("TEST STEP 4", "1111114", 7, GBLON.RL_Code, USLAX.RL_Code, "XXXS");
			header.BH_PortUnladingDCode = "1234";
			header.BH_FIRMS = "A304";

			var bill = header.Bills.AddNew();
			bill.B0_MasterBillNumber = "4101";
			bill.B0_PortOfLadingKCode = "72357";
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.MasterBill;
			bill.B0_MasterInBondIndicator = false;
			bill.B0_IssuerCode = "XXXS";
			bill.B0_ManifestQty = 200;
			bill.B0_ManifestUQ = ManifestUnitList.Codes.MixedTypePack;
			bill.B0_Weight = 50m;
			bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			bill.B0_PlaceOfReceipt = "LONDON";

			var billContainer = bill.MovementDetail.Containers.AddNew();
			billContainer.BC_ContainerNum = "TURE2122123";
			billContainer.BC_RC = refContainer.PK;
			billContainer.BC_Seal1 = "SL123";

			var commodity = billContainer.Commodities.AddNew();
			commodity.BY_HarmonisedTariff = "10.2030.40";
			commodity.BY_PieceCount = 200;
			commodity.BY_Description = "T";
			commodity.BY_MarksAndNumbers = "T";
			commodity.BY_GrossWeight = 1m;
			commodity.BY_GrossWeightUnit = "KG";
			commodity.BY_MonetaryValue = 1m;
#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);
			SendManifestOriginalMessage(header);
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08)]
#endif
		public void TestNVOCCOceanUserTest0()
		{
			#region variable that should be changed if a retransmit is done
			var voyage = "00001";
			var testSequence = "01";
			#endregion

			CreateNewOrGetExistingCusCodeListForCUSOFType("1234");
			var header = CreateHeader("NVO STEP 0", "2111110", 3, GBLON.RL_Code, USLAX.RL_Code, "OTT1", voyage);
			header.BH_PortUnladingDCode = "1234";
			var bill_0101 = CreateNewBillWithCargoDetails(header, testSequence + "01", BillOfLadingStatusIndicatorList.Codes.HouseBill);

#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			bill_0101.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, "AABC234334");
			AssertNoMessageErrorsBeforeSending(header);
			SendManifestOriginalMessage(header);
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08)]
#endif
		public void TestNVOCCOceanUserTest1()
		{
			#region variable that should be changed if a retransmit is done
			var voyage = "00001";
			var testSequence = "11";
			// MVOCC Bills
			var bill_1101 = "AABCCWJAN2401";
			var bill_1102 = "AABCCWJAN2402";
			var bill_1103 = "AABCCWJAN2403";
			//var bill_1104 = "AABCCWJAN2404";
			//var bill_1105 = "AABCCWJAN2405";
			var bill_1106 = "AABCCWJAN0206";
			var bill_1107 = "AABCCWJAN0207";

			#endregion

			CreateNewOrGetExistingCusCodeListForCUSOFType("0401");
			var header = CreateHeader("NVO STEP 1", "2111111", 3, GBLON.RL_Code, USLAX.RL_Code, "OTT1", voyage);
			header.BH_PortUnladingDCode = "0401";

			// NVOCC Bills

			var bill_1108 = CreateNewBillWithCargoDetails(header, testSequence + "08", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1108.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101);
			var bill_1109 = CreateNewBillWithCargoDetails(header, testSequence + "09", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1109.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101);
			var bill_1110 = CreateNewBillWithCargoDetails(header, testSequence + "10", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1110.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1101);

			var bill_1111 = CreateNewBillWithCargoDetails(header, testSequence + "11", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1111.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1102);
			var bill_1112 = CreateNewBillWithCargoDetails(header, testSequence + "12", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1112.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1102);

			var bill_1113 = CreateNewBillWithCargoDetails(header, testSequence + "13", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1113.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1103);
			var bill_1114 = CreateNewBillWithCargoDetails(header, testSequence + "14", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1114.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1103);
			var bill_1115 = CreateNewBillWithCargoDetails(header, testSequence + "15", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1115.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1103);
			var bill_1116 = CreateNewBillWithCargoDetails(header, testSequence + "16", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_1116.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1103);

			var bill_1117 = CreateNewBillWithCargoDetails(header, testSequence + "17", BillOfLadingStatusIndicatorList.Codes.FROB);
			bill_1117.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1106);

			var bill_1118 = CreateNewBillWithCargoDetails(header, testSequence + "18", BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF);
			bill_1118.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_1107);
			bill_1118.B0_ForeignPortOfUnladingKCode = "01535";
			bill_1118.B0_PlaceOfDelivery = "CATOR";

#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);
			SendManifestOriginalMessage(header);
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08)]
#endif
		public void TestNVOCCOceanUserTest2()
		{
			#region variable that should be changed if a retransmit is done
			var voyage = "BL619";
			var testSequence = "21";
			#endregion

			CreateNewOrGetExistingCusCodeListForCUSOFType("0401");
			var header = CreateHeader("NVO STEP 2", "2111112", 3, GBLON.RL_Code, USLAX.RL_Code, "XXXW", voyage);
			header.BH_PortUnladingDCode = "0401";
			header.BH_ETA = new ZDateTime(2012, 6, 22);
			// MVOCC Bills
			//var bill_2100 = "XXXWBLJUN1910";
			var bill_2101 = "XXXWBLJUN1911";
			//var bill_2101 = CreateNewBillWithCargoDetails(header, testSequence + "01", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2101.B0_MasterInBondIndicator = ZBool.True;
			var bill_2102 = "XXXWBLJUN1912";
			//var bill_2102 = CreateNewBillWithCargoDetails(header, testSequence + "02", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2102.B0_MasterInBondIndicator = ZBool.True;
			var bill_2103 = "XXXWBLJUN1913";
			//var bill_2103 = CreateNewBillWithCargoDetails(header, testSequence + "03", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2103.B0_MasterInBondIndicator = ZBool.True;
			var bill_2104 = "XXXWBLJUN1914";
			//var bill_2104 = CreateNewBillWithCargoDetails(header, testSequence + "04", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2104.B0_MasterInBondIndicator = ZBool.True;
			var bill_2105 = "XXXWBLJUN1915";
			//var bill_2105 = CreateNewBillWithCargoDetails(header, testSequence + "05", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2105.B0_MasterInBondIndicator = ZBool.True;
			var bill_2106 = "XXXWBLJUN1916";
			//var bill_2106 = CreateNewBillWithCargoDetails(header, testSequence + "06", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2106.B0_MasterInBondIndicator = ZBool.True;
			var bill_2107 = "XXXWBLJUN1917";
			//var bill_2107 = CreateNewBillWithCargoDetails(header, testSequence + "07", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2107.B0_MasterInBondIndicator = ZBool.True;
			var bill_2108 = "XXXWBLJUN1918";
			//var bill_2108 = CreateNewBillWithCargoDetails(header, testSequence + "08", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//bill_2108.B0_MasterInBondIndicator = ZBool.True;
			//var bill_2109 = "XXXWBLJUN1919";
			//var bill_2109 = CreateNewBillWithCargoDetails(header, testSequence + "09", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			//var bill_2110 = "XXXWBLJUN1920";
			//var bill_2110 = CreateNewBillWithCargoDetails(header, testSequence + "10", BillOfLadingStatusIndicatorList.Codes.MasterBill);

			// NVOCC Bills

			var bill_2111 = CreateNewBill(header, testSequence + "11", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2111.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2101);
			bill_2111.B0_ManifestQty = 500;
			var bill_2111Container = CreateNewContainer(bill_2111.MovementDetail, "OTTO1850780HS0", "15880");
			var bill_2111commodity = CreateNewCommodity(bill_2111Container, 2111m * 1.5m, "GOODS FOR 2111", "MARKS FOR 2111");

			var bill_2112 = CreateNewBill(header, testSequence + "12", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2112.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2102);
			bill_2112.B0_ManifestQty = 20;
			var bill_2112Container = CreateNewContainer(bill_2112.MovementDetail, "OTTO280780HS01", "588");
			var bill_2112commodity = CreateNewCommodity(bill_2112Container, 2112m * 1.5m, "GOODS FOR 2112", "MARKS FOR 2112");

			var bill_2113 = CreateNewBill(header, testSequence + "13", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2113.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2103);
			bill_2113.B0_ManifestQty = 150;
			var bill_2113Container = CreateNewContainer(bill_2113.MovementDetail, "PPHU3850780HS0", "15860");
			var bill_2113commodity = CreateNewCommodity(bill_2113Container, 2113m * 1.5m, "GOODS FOR 2113", "MARKS FOR 2113");

			var bill_2114 = CreateNewBill(header, testSequence + "14", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2114.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2104);
			bill_2114.B0_ManifestQty = 151;
			var bill_2114Container = CreateNewContainer(bill_2114.MovementDetail, "CPHU5050780HS0", "15883");
			var bill_2114commodity = CreateNewCommodity(bill_2114Container, 2114m * 1.5m, "GOODS FOR 2114", "MARKS FOR 2114");

			var bill_2115 = CreateNewBill(header, testSequence + "15", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2115.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2105);
			bill_2115.B0_ManifestQty = 41;
			var bill_2115Container = CreateNewContainer(bill_2115.MovementDetail, "WPHU4850780HS0", "15801");
			var bill_2115commodity = CreateNewCommodity(bill_2115Container, 2115m * 1.5m, "GOODS FOR 2115", "MARKS FOR 2115");

			var bill_2116 = CreateNewBill(header, testSequence + "16", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2116.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2106);
			bill_2116.B0_ManifestQty = 150;
			var bill_2116Container = CreateNewContainer(bill_2116.MovementDetail, "TPHU4850780HS0", "15983");
			var bill_2116commodity = CreateNewCommodity(bill_2116Container, 2116m * 1.5m, "GOODS FOR 2116", "MARKS FOR 2116");

			var inBondMoveHeaderIT1 = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, "VNW06191231", 3);
			inBondMoveHeaderIT1.BM_ArrivalDate = header.BH_ETA.AddHours(1);
			var inBondMoveHeaderIT1Bill_2111 = inBondMoveHeaderIT1.MovementDetails.AddNew(bill_2111.PK);
			var inBondMoveHeaderIT2Bill_2114 = inBondMoveHeaderIT1.MovementDetails.AddNew(bill_2112.PK);
			var inBondMoveHeaderIT2Bill_2117 = inBondMoveHeaderIT1.MovementDetails.AddNew(bill_2113.PK);

			var inBondMoveHeaderIT2 = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._1ImmediateTransport, "VNW06191249", 3);
			inBondMoveHeaderIT2.BM_ArrivalDate = header.BH_ETA.AddHours(1);
			var inBondMoveHeaderIT1Bill_2120 = inBondMoveHeaderIT2.MovementDetails.AddNew(bill_2114.PK);
			var inBondMoveHeaderIT2Bill_2123 = inBondMoveHeaderIT2.MovementDetails.AddNew(bill_2115.PK);
			var inBondMoveHeaderIT2Bill_2126 = inBondMoveHeaderIT2.MovementDetails.AddNew(bill_2116.PK);

			var bill_2117 = CreateNewBill(header, testSequence + "17", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2117.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2107);
			bill_2117.B0_ManifestQty = 560;
			var bill_2117Container = CreateNewContainer(bill_2117.MovementDetail, "OTTO1234112HS0", "18154");
			var bill_2117commodity = CreateNewCommodity(bill_2117Container, 2117m * 1.5m, "GOODS FOR 2117", "MARKS FOR 2117");

			var bill_2118 = CreateNewBill(header, testSequence + "18", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2118.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2107);
			bill_2118.B0_ManifestQty = 600;
			var bill_2118Container = CreateNewContainer(bill_2118.MovementDetail, "OTTO1234112HS0", "18154");
			var bill_2118commodity = CreateNewCommodity(bill_2118Container, 2118m * 1.5m, "GOODS FOR 2118", "MARKS FOR 2118");

			var inBondMoveHeaderTAndE = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._2TransportandExport, "VNW06191256", 3);
			inBondMoveHeaderTAndE.BM_DestinationPortCode = "0901";
			inBondMoveHeaderTAndE.BM_ArrivalDate = header.BH_ETA.AddHours(2);
			inBondMoveHeaderTAndE.BM_ForeignDestPortKCode = "01528";
			var inBondMoveHeaderTAndEBill_2117 = inBondMoveHeaderTAndE.MovementDetails.AddNew(bill_2117.PK);
			var inBondMoveHeaderTAndEBill_2118 = inBondMoveHeaderTAndE.MovementDetails.AddNew(bill_2118.PK);

			var bill_2119 = CreateNewBill(header, testSequence + "30", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2119.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2108);
			bill_2119.B0_ManifestQty = 750;
			var bill_2119Container = CreateNewContainer(bill_2119.MovementDetail, "OTTO3407190HS1", "40758");
			var bill_2119commodity = CreateNewCommodity(bill_2119Container, 2119m * 1.5m, "GOODS FOR 2119", "MARKS FOR 2119");

			var bill_2120 = CreateNewBill(header, testSequence + "30", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2120.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2108);
			bill_2120.B0_ManifestQty = 754;
			var bill_2120Container = CreateNewContainer(bill_2120.MovementDetail, "OTTO3407190HS1", "40758");
			var bill_2120commodity = CreateNewCommodity(bill_2120Container, 2120m * 1.5m, "GOODS FOR 2120", "MARKS FOR 2120");

			var inBondMoveHeaderIE = CreateNewInBondMoveHeader(header, InbondCommonTypeList.Codes._3ImmediateExport, "VNW06191264", 3);
			inBondMoveHeaderIE.BM_ArrivalDate = header.BH_ETA.AddMinutes(90);
			inBondMoveHeaderIE.BM_DestinationPortCode = "0401";
			inBondMoveHeaderIE.BM_ForeignDestPortKCode = "01528";
			var inBondMoveHeaderIEBill_2119 = inBondMoveHeaderIE.MovementDetails.AddNew(bill_2119.PK);
			var inBondMoveHeaderIEBill_2120 = inBondMoveHeaderIE.MovementDetails.AddNew(bill_2120.PK);

#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);
			SendManifestOriginalMessage(header);
#endif
		}

#if !SendTestMessagesToCustoms
		[TestDate(2012, 05, 08)]
#endif
		public void TestNVOCCOceanUserTest3()
		{
			#region variable that should be changed if a retransmit is done
			var voyage = "00001";
			var testSequence = "31";
			#endregion

			CreateNewOrGetExistingCusCodeListForFIRMSType("A304");
			CreateNewOrGetExistingCusCodeListForCUSOFType("1234");
			var header = CreateHeader("NVO STEP 3", "2111113", 3, GBLON.RL_Code, USLAX.RL_Code, "OTT1", voyage);
			header.BH_PortUnladingDCode = "1234";
			header.BH_FIRMS = "A304";

			// MVOCC Bills
			var bill_2101 = CreateNewBillWithCargoDetails(header, testSequence + "01", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_2102 = CreateNewBillWithCargoDetails(header, testSequence + "02", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_2103 = CreateNewBillWithCargoDetails(header, testSequence + "03", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_2104 = CreateNewBillWithCargoDetails(header, testSequence + "04", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_2105 = CreateNewBillWithCargoDetails(header, testSequence + "05", BillOfLadingStatusIndicatorList.Codes.MasterBill);
			var bill_2106 = CreateNewBillWithCargoDetails(header, testSequence + "06", BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard);
			var bill_2107 = CreateNewBillWithCargoDetails(header, testSequence + "07", BillOfLadingStatusIndicatorList.Codes.SimpleForeignRetainedOnBoard);

			// NVOCC Bills
			var bill_2108 = CreateNewBillWithCargoDetails(header, testSequence + "08", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2108.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2101.B0_IssuerCode + bill_2101.B0_MasterBillNumber);
			bill_2108.CustomsBroker.E2_OA_Address = CustomsBroker.MainAddress.PK;
			var bill_2109 = CreateNewBillWithCargoDetails(header, testSequence + "09", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2109.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2102.B0_IssuerCode + bill_2102.B0_MasterBillNumber);
			var bill_2110 = CreateNewBillWithCargoDetails(header, testSequence + "10", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2110.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2103.B0_IssuerCode + bill_2103.B0_MasterBillNumber);
			var bill_2111 = CreateNewBillWithCargoDetails(header, testSequence + "11", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2111.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2104.B0_IssuerCode + bill_2104.B0_MasterBillNumber);
			var bill_2112 = CreateNewBillWithCargoDetails(header, testSequence + "12", BillOfLadingStatusIndicatorList.Codes.HouseBill);
			bill_2112.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2105.B0_IssuerCode + bill_2105.B0_MasterBillNumber);
			var bill_2113 = CreateNewBillWithCargoDetails(header, testSequence + "13", BillOfLadingStatusIndicatorList.Codes.FROB);
			bill_2113.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2106.B0_IssuerCode + bill_2106.B0_MasterBillNumber);
			var bill_2114 = CreateNewBillWithCargoDetails(header, testSequence + "14", BillOfLadingStatusIndicatorList.Codes.FROB);
			bill_2114.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, bill_2107.B0_IssuerCode + bill_2107.B0_MasterBillNumber);

#if SendTestMessagesToCustoms
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
#else
			AssertNoMessageErrorsBeforeSending(header);
			SendManifestOriginalMessage(header);
#endif
		}

		void AssertNoMessageErrorsBeforeSending(BusinessObject bO)
		{
			bO.RunPreSaveValidation();
			var messageErrors = new Customs.Business.CustomsNotificationCollector(bO, true, false, Enterprise.Customs.Business.CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().ToUniqueMessageListString();
			AssertEquals("Messages", "", messageErrors);
		}

#if SendTestMessagesToCustoms
		internal static string GetVNo(string prefix)
		{
			var length = 8 - prefix.Length;
			var result = "VNW" + prefix + new Random().Next(int.Parse("".PadRight(length, '9'))).ToString().PadLeft(length, '0');
			int actualCheckDigit = Convert.ToInt32(result.Substring(10, 1));
			int expectedCheckDigit = InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit(result);

			if (actualCheckDigit != expectedCheckDigit)
			{
				result = result.Substring(0, 10) + expectedCheckDigit.ToString();
			}
			return result;
		}

		internal static string GetInBondNumber(string prefix)
		{
			var length = 7 - prefix.Length;
			var result = "5" + new Random().Next(int.Parse("".PadRight(length, '9'))).ToString().PadLeft(length, '0');
			result += InBondNumberCheckDigitCalculator.GetCheckDigit(result);
			return result;
		}
#else
		void SendManifestOriginalMessage(CusInBondHeader header)
		{
			var messageAction = new MessageSendingAction(header, ActionCode.Creating);
			AssertNoMessageErrorsBeforeSending(messageAction);
			messageAction.CreateAMSMessages();
			Factory.Save();
		}

		void SendAndAssertVesselDepartureMessage(CusInBondHeader header)
		{
			var messageAction = new MessageSendingAction(header, ActionCode.VesselDeparture);
			var movement = messageAction.Movements[0];
			movement.MM_Send = true;
			movement.MM_ForeignDeparturePort = "50200";
			movement.MM_Date = ZDateTime.Today.AddDays(-3);

			AssertNoMessageErrorsBeforeSending(messageAction);
			messageAction.CreateAMSMessages();
			Factory.Save();
			//An amendment feature for vessel departure is not available for this action at this time. If error occurs, re-send new data.
		}

		void SendAndAssertInBondDiversionMessage(CusInBondMoveHeader inBondMoveHeaderIT1)
		{
			inBondMoveHeaderIT1.BM_DestinationPortCode = "1803";
			var messageAction = new MessageSendingAction(inBondMoveHeaderIT1.Header, ActionCode.InBondDiversion);
			var listOfMovements = new TypedEnumerable<MessageSendingMovement>(messageAction.Movements);
			listOfMovements.First(x => x.MM_RelatedDetails.Contains(Constants.VNo1)).MM_Send = true;

			AssertNoMessageErrorsBeforeSending(messageAction);
			messageAction.CreateAMSMessages();
			Factory.Save();

			inBondMoveHeaderIT1.Messages.Sort(AMSEDIMessage.Schema.EM_MessageNum, System.ComponentModel.ListSortDirection.Descending);
			var message = inBondMoveHeaderIT1.Messages[0];
			var icmh01 = message.MessageBlock.MessageBlocks.OfType<ICMH01>().FirstOrDefault();
			AssertNotNull("icmh01", icmh01);
			AssertEquals("MessageCode", InBondAndVesselEventMessageCodeList.Codes.RequestForInBondDiversion, icmh01.MessageCode);
		}

		void SendAndAssertInBondArrivalMessage(CusInBondHeader header)
		{
			var messageAction = new MessageSendingAction(header, ActionCode.InBondArrival);
			messageAction.Movements.SetToSend();

			var sendingObj = messageAction.ObjectsToSend.First(x => x.MB_RelatedDetails.Contains(Constants.VNo1));
			sendingObj.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBond;

			var listOfSendingObj = new List<MessageSendingObject>(new TypedEnumerable<MessageSendingObject>(messageAction.ObjectsToSend));
			var sendingObjUnderVNumber = listOfSendingObj.Find(x => x.MB_RelatedDetails.Contains(Constants.VNo2));
			foreach (MessageSendingObject obj in sendingObjUnderVNumber)
			{
				if (obj.MB_BillOfLadingSequenceNumber == "2107" ||
					obj.MB_BillOfLadingSequenceNumber == "2108" ||
					obj.MB_BillOfLadingSequenceNumber == "2111" ||
					obj.MB_BillOfLadingSequenceNumber == "2112")
				{
					obj.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByBillOfLading;
				}
				else if (obj.MB_BillOfLadingSequenceNumber == "2109" || obj.MB_BillOfLadingSequenceNumber == "2110")
				{
					obj.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ArriveInBondByContainer;
				}
			}

			messageAction.CreateAMSMessages();
			Factory.Save();
		}

		void SendAndAssertInBondExportationMessage(CusInBondHeader header)
		{
			var messageAction = new MessageSendingAction(header, ActionCode.InBondExportation);
			messageAction.Movements.SetToSend();

			var listOfSendingObj = new List<MessageSendingObject>(new TypedEnumerable<MessageSendingObject>(messageAction.ObjectsToSend));
			var sendingObjUnderVNumber = listOfSendingObj.Find(x => x.MB_RelatedDetails.Contains(Constants.VNo3));
			foreach (MessageSendingObject obj in sendingObjUnderVNumber)
			{
				if (obj.MB_BillOfLadingSequenceNumber == "2107" ||
					obj.MB_BillOfLadingSequenceNumber == "2108" ||
					obj.MB_BillOfLadingSequenceNumber == "2113" ||
					obj.MB_BillOfLadingSequenceNumber == "2114" ||
					obj.MB_BillOfLadingSequenceNumber == "2111" ||
					obj.MB_BillOfLadingSequenceNumber == "2112")
				{
					obj.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ExportInBondByBillOfLading;
				}
				else if (obj.MB_BillOfLadingSequenceNumber == "2109" ||
					obj.MB_BillOfLadingSequenceNumber == "2110" ||
					obj.MB_BillOfLadingSequenceNumber == "2115")
				{
					obj.MB_BillActionCode = InBondAndVesselEventMessageCodeList.Codes.ExportInBondByContainer;
				}
			}

			messageAction.CreateAMSMessages();
			Factory.Save();
		}

		void SendSubsequentAndInBondArrivalMessageForBill2101(CusInBondMoveHeader inBondMoveHeader)
		{
			//TODO: should enter subsequent inBond movement 
			// add subsequesnt for inBondMoveHeader1 and arrival us port will be 0401
			var messageAction = new MessageSendingAction(inBondMoveHeader.Header, ActionCode.SubsequentInBondOriginal);
			var listOfMovements = new TypedEnumerable<MessageSendingMovement>(messageAction.Movements);
			listOfMovements.First(x => x.MM_RelatedDetails.Contains(Constants.VNo1)).MM_Send = true;

			messageAction.CreateAMSMessages();
			Factory.Save();

			inBondMoveHeader.Messages.Sort(AMSEDIMessage.Schema.EM_MessageNum, System.ComponentModel.ListSortDirection.Descending);
			var message = inBondMoveHeader.Messages[0];
			var inpi01 = message.MessageBlock.MessageBlocks.OfType<INPI01>().FirstOrDefault();
			AssertNotNull("inpi01", inpi01);
			AssertEquals("InBondCarrierID", inpi01.BondedCarrierID, inBondMoveHeader.BM_InBondCarrierID);

			messageAction = new MessageSendingAction(inBondMoveHeader.Header, ActionCode.InBondArrival);
			listOfMovements = new TypedEnumerable<MessageSendingMovement>(messageAction.Movements);
			listOfMovements.First(x => x.MM_RelatedDetails.Contains(Constants.VNo1)).MM_Send = true;

			messageAction.CreateAMSMessages();
			Factory.Save();
		}

		void SendAndAssertManifestAmendmentMessage(CusInBondHeader header)
		{
			var bill3122 = CreateNewBillWithCargoDetails(header, "3122", BillOfLadingStatusIndicatorList.Codes.RegularBill);
			bill3122.B0_PortOfLadingKCode = "72357";
			bill3122.B0_MasterInBondIndicator = true;

			var messageAction = new MessageSendingAction(header, ActionCode.AmendingAdd);
			var listOfSendingObjects = new List<MessageSendingObject>(new TypedEnumerable<MessageSendingObject>(messageAction.MessageSendingObjects));
			var sendingObj = listOfSendingObjects.Find(x => x.MB_BillOfLadingSequenceNumber.EndsWith("3102"));
			sendingObj.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.DeleteBill;

			sendingObj = listOfSendingObjects.Find(x => x.MB_BillOfLadingSequenceNumber.EndsWith("3111"));
			sendingObj.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.DeleteBill;
			messageAction.CreateAMSMessages();

			var bill3115 = header.Bills.Find(x => x.B0_MasterBillNumber.EndsWith("3115")).First();
			bill3115.B0_ManifestQty = 6;
			bill3115.B0_ManifestUQ = ManifestUnitList.Codes.Box;

			var bill3111 = header.Bills.Find(x => x.B0_MasterBillNumber.EndsWith("3111")).First();
			bill3111.B0_ManifestQty = 215;
			bill3111.B0_ManifestUQ = ManifestUnitList.Codes.Carton;
			messageAction = new MessageSendingAction(header, ActionCode.AmendingAdd);
			messageAction.CreateAMSMessages();
			Factory.Save();
		}
#endif

		void CreateNewOrGetExistingCusCodeListForCUSOFType(ZString code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, code, "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		void CreateNewOrGetExistingCusCodeListForFIRMSType(ZString code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, code, "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}

		CusInBondHeader CreateHeader(ZString vesselName, ZString lloydsNumber, int daysCount, ZString loadPort, ZString dischargePort, ZString carrierSCAC, string voyage = null)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			var mostInterestingTransport = consol.MostInterestingTransportForBinding[0];
			mostInterestingTransport.JW_VoyageFlight = voyage ?? new Random().Next(9).ToString() + ZDateTime.Now.ToString("mmss");
			mostInterestingTransport.JW_Vessel = vesselName;
			mostInterestingTransport.JW_ETD = ZDateTime.Now;
			mostInterestingTransport.JW_ETA = ZDateTime.Today.AddDays(daysCount);
			consol.JK_RL_NKLastForeignPort = dischargePort;
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.Synchroniser.Synchronise(true);
			header.BH_OverrideFreightDefaults = true;
			header.BH_CarrierSCAC = carrierSCAC;
			header.BH_LloydsNumber = lloydsNumber;
			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Austria;
			return header;
		}

		CusInBondMoveHeader CreateNewInBondMoveHeader(CusInBondHeader header, ZString entryType, ZString inBondNumber, ZInt noOfDaysForETA)
		{
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_InBondEntryType = entryType;
			inBondMoveHeader.BM_InBondCarrierSCAC = header.BH_CarrierSCAC;
			inBondMoveHeader.BM_ArrivalDate = ZDateTime.Today.AddDays(noOfDaysForETA);
			inBondMoveHeader.BM_DestinationPortCode = inBondMoveHeader.IsImmediateExportEntryType ? header.BH_PortUnladingDCode : (ZString)"0401";
			inBondMoveHeader.BM_InBondCarrierID = Constants.InBondCarrierID;
			if (!inBondMoveHeader.IsImmediateTransportEntryType)
			{
				inBondMoveHeader.BM_ExportLadenOn = "TEST STEP 0";
				inBondMoveHeader.BM_ExportDate = inBondMoveHeader.BM_ArrivalDate.AddHours(1);
			}
			inBondMoveHeader.AllocateInBondNumber(inBondNumber);
			return inBondMoveHeader;
		}

		CusInBondBill CreateNewBillWithCargoDetails(CusInBondHeader header, ZString billNoSuffix, ZString billStatus)
		{
			var bill = CreateNewBill(header, billNoSuffix, billStatus);
			var container = CreateNewContainer(bill.MovementDetail, GenerateContainerNumber(billNoSuffix), billNoSuffix);
			var commodity = CreateNewCommodity(container, ZDecimal.ParseSafe(billNoSuffix, ZDecimal.Zero) * 1.5m, "GOODS FOR " + billNoSuffix, "MARKS FOR " + billNoSuffix);
			return bill;
		}

		CusInBondBill CreateNewBill(CusInBondHeader header, ZString billNoSuffix, ZString billStatus)
		{
			var consol = header.Consol;
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "XXXW";
			bill.B0_MasterBillNumber = BillNoPrefix + billNoSuffix.PadLeft(6, '0');
			bill.B0_BillStatus = billStatus;
			bill.B0_RL_NKPortOfLading = consol.JK_RL_NKLoadPort;
			bill.B0_ManifestQty = ZInt.ParseSafe(billNoSuffix, new Random().Next(9999));
			bill.B0_ManifestUQ = ManifestUnitList.Codes.MixedTypePack;
			bill.B0_Weight = new ZDecimal(bill.B0_ManifestQty * 1.1m).Round(0);
			bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			bill.B0_Volume = new ZDecimal(bill.B0_ManifestQty * 0.07m).Round(0);
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicMetres;
			bill.ForeignShipper.E2_OA_Address = Shipper.MainAddress.PK;
			bill.Consignee.E2_OA_Address = Consignee.MainAddress.PK;
			return bill;
		}

		ZString GenerateContainerNumber(ZString partialNo)
		{
			return ZDateTime.Today.ToString("MMMM").Substring(0, 1) + ZDateTime.Today.ToString("ddd").ToUpper() + partialNo + new Random().Next(999).ToString().PadRight(3, '0');
		}

		CusInBondCargoDesc CreateNewCommodity(CusInBondContainer container, ZDecimal monetaryValue, ZString description, ZString marksAndNumbers)
		{
			var commodity = container.Commodities.AddNew();
			commodity.BY_HarmonisedTariff = Tariffs[Math.Max(new Random().Next(Tariffs.Length) - 1, 0)].UE_Tariff;
			commodity.BY_MonetaryValue = monetaryValue;
			commodity.BY_Description = description;
			commodity.BY_MarksAndNumbers = marksAndNumbers;
			return commodity;
		}

		CusInBondContainer CreateNewContainer(CusInBondMoveDetail moveDetail, ZString containerNum, ZString seal1)
		{
			var container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = containerNum;
			container.BC_RC = ContainerType.PK;
			container.BC_Seal1 = seal1;
			return container;
		}

		ZString BillNoPrefix
		{
			get
			{
				if (!billNoPrefix.HasValue)
				{
					var clientInitial = ClientInitial;
					var uniqueNumberLength = 6 - clientInitial.Length;
					billNoPrefix = clientInitial + new Random().Next(int.Parse("".PadLeft(uniqueNumberLength, '9'))).ToString().PadLeft(uniqueNumberLength, '0');
				}
				return billNoPrefix.Value;
			}
		}
		ZString? billNoPrefix;

		ZString ClientInitial
		{
			get
			{
#if SendTestMessagesToCustoms
				return "CW" + GlbStaff.CurrentUser.GS_Code.Left(2);
#else
				return "HB";
#endif
			}
		}

		USCTariff[] Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					var query = new ZQuery(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
					query.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
					query.MaximumRows = 50;
					tariffs = Factory.Load<USCTariff>(query);
				}
				return tariffs;
			}
		}
		USCTariff[] tariffs;

		RefContainer ContainerType
		{
			get
			{
				if (containerType == null)
				{
					containerType = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "RCD"));
					if (containerType == null)
					{
						containerType = Factory.New<RefContainer>();
						containerType.RC_Code = "RCD";
						containerType.SetCountrySpecificContainerCode("40", Enterprise.Core.Constants.CountryCodes.UnitedStates);
					}
				}
				return containerType;
			}
		}
		RefContainer containerType;

		OrgHeader Shipper
		{
			get
			{
				if (shipper == null)
				{
					shipper = LoadOrNew("UKEXP");
				}
				return shipper;
			}
		}
		OrgHeader shipper;

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = LoadOrNew("USIMP");
				}
				return consignee;
			}
		}
		OrgHeader consignee;

		OrgHeader CustomsBroker
		{
			get
			{
				if (customsBroker == null)
				{
					customsBroker = LoadOrNew("USBROKER");
					if (!customsBroker.IsInDatabase)
					{
						customsBroker.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ABIRoutingCode, "1512SV3", Core.Constants.CountryCodes.UnitedStates);
					}
				}
				return customsBroker;
			}
		}
		OrgHeader customsBroker;

		OrgHeader LoadOrNew(ZString code)
		{
			return Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code)) ?? CreateOrganisation(code);
		}

		RefUNLOCO GBLON
		{
			get { return gblon ?? (gblon = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "GBLON")); }
		}
		RefUNLOCO gblon;

		RefUNLOCO USLAX
		{
			get { return uslax ?? (uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX")); }
		}
		RefUNLOCO uslax;

		RefUNLOCO USBAL
		{
			get { return usbal ?? (usbal = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USBAL")); }
		}
		RefUNLOCO usbal;

		ZZRefCusCodeListCombined[] ForeignPorts
		{
			get
			{
				if (foreignPorts == null)
				{
					var codeListQuery = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
					var codeListAttributeQuery = new ZDBOnlySubQuery(typeof(ZZRefCusCodeListAttributeCombined), ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, true);
					codeListAttributeQuery.AddToFilter(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZXE_NKName, RefCusCodeListAttributeTypes.Codes.PortValidType));
					codeListQuery.MaximumRows = 30;
					foreignPorts = Factory.Load<ZZRefCusCodeListCombined>(codeListQuery);
				}
				return foreignPorts;
			}
		}
		ZZRefCusCodeListCombined[] foreignPorts;

		OrgHeader CreateOrganisation(ZString code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = code + " COMPANY NAME";
			org.MainAddress.OA_Address1 = code + " ADDRESS 1";
			org.MainAddress.OA_City = code + " CITY";
			org.MainAddress.OA_State = code + " STATE";
			org.MainAddress.OA_PostCode = new Random().Next(999999).ToString().PadLeft(6, '0');
			return org;
		}
	}

#if SendTestMessagesToCustoms
	class MessageBuilderTestCase : TestCase
	{
		protected BusinessObjectFactory Factory;
		protected override void SetUp()
		{
			Factory = new BusinessObjectFactory();
			base.SetUp();
			sendTestMessagesToCustoms = true;
			startTime = DateTime.Now;

			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "XXXW";
			carrier.UI_Name = "AMS ACE M1 TEST CARRIER";
			carrier.UI_ModeOfTransportation = "10";

			refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40"));
			if (refContainer == null)
			{
				refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = "40";
			}
			Factory.Save();
		}
		DateTime startTime;
		protected bool sendTestMessagesToCustoms;
		protected RefContainer refContainer;

		protected override void TearDown()
		{
			base.TearDown();
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.AMS);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, startTime);
			query.FetchOnlyFromLocalCache = true;
			var messages = Factory.Load<EDIMessage>(query);
			if (messages.Length > 0)
			{
				foreach (EDIMessage message in messages)
				{
					message.EM_ApplicationReference = "SendTestMessagesToCustoms";
				}
				Factory.Save();
			}
		}
	}
#else
	class MessageBuilderTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			sendTestMessagesToCustoms = false;

			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "XXXW";
			carrier.UI_Name = "AMS ACE M1 TEST CARRIER";
			carrier.UI_ModeOfTransportation = "10";

			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "XXXS";
			carrier2.UI_Name = "AMS ACE M1 TEST CARRIER 2";
			carrier2.UI_ModeOfTransportation = "10";

			var carrier3 = Factory.New<USCarrierCombined>();
			carrier3.UI_Code = "OTT1";
			carrier3.UI_Name = "TEST CARRIER";
			carrier3.UI_ModeOfTransportation = "10";

			refContainer = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40"));
			if (refContainer == null)
			{
				refContainer = Factory.New<RefContainer>();
				refContainer.RC_Code = "40";
				refContainer.SetCountrySpecificContainerCode("20", Enterprise.Core.Constants.CountryCodes.UnitedStates);
			}

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "01535", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "72357", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "41352", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "50200", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			for (var i = 0; i < 30; i++)
			{
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, i.ToString().PadLeft(4, '0'), string.Format("Test {0}", i), ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			}
			Factory.Save();
		}
		protected bool sendTestMessagesToCustoms;
		protected RefContainer refContainer;
	}
#endif
}
