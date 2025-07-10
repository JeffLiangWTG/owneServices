using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class MessageBuildersSendMessageToCustomsTest : MessageBuilderTestCase
	{
		public void TestInBondUserTest1()
		{
			#region Step 1
			//Precondition: CBP step - Add 7 Ocean BOLs on File with no In-Bonds
			var header = CreateInBondHeader(InBondHeaderTypeList.Codes.AMS, "XXXS", TransportModeCodes.Codes.VesselNonContainer);
			var moveHeader1 = CreateInBondDataWithBillsAndDetails(header, "100000001", new string[] { "MB0000001" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			var moveHeader2 = CreateInBondDataWithBillsAndDetails(header, "100000002", new string[] { "MB0000002" }, InbondCommonTypeList.Codes._3ImmediateExport);
			var moveHeader3 = CreateInBondDataWithBillsAndDetails(header, "100000003", new string[] { "MB0000003" });
			CreateInBondDataWithBillsAndDetails(header, "100000004", new string[] { "MB0000004" });
			CreateInBondDataWithBillsAndDetails(header, "100000005", new string[] { "MB0000005" });
			var moveHeader6 = CreateInBondDataWithBillsAndDetails(header, "100000006", new string[] { "MB0000006" });
			var moveHeader7 = CreateInBondDataWithBillsAndDetails(header, "100000007", new string[] { "MB0000007" });
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
			//1.	Submit a QP to add 7 In-bonds to 7 BOLs using Bonded Carrier ID = 11-987654300
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				obj.Send();
			}

			//2.	Submit a QP to delete In-bond 1 from BOL 1
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var sendingObj = new InBondMessageSendingObject(moveHeader1, InBondMessageType.DepartureDelete);
			sendingObj.Send();
			Factory.Save();
			#endregion
			#region Step 2
			//Precondition: CBP step - Arrive Conveyance
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			//3.	Submit a WP to Export in-bond 2 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader2, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			moveHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			//4.	Submit a WP to Arrive in-bond 3 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//5.	Submit a WP to Export in-bond 3 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//6.	Submit a WP to Arrive in-bond by BOL 4 at destination (Action Code = 2)
			//7.	Submit a WP to Export in-bond by BOL 4 from destination (Action Code = 6)
			//8.	Submit a WP to Arrive in-bond by BOL 5 at destination (Action Code = 3)
			//9.	Submit a WP to Export in-bond by BOL 5 from destination (Action Code = 7)
			//10.	Submit a WP to Divert in-bond 6 to a new destination (Action Code = Z)
			//11.	Submit a WP to Arrive in-bond 6 at new destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader6, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//12.	Submit a WP to Export in-bond 6 from new destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader6, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//13.	Submit a WP to Transfer Liability of in-bond 7 to another Bonded Carrier ID = 11-765432100 for XXXW (Action Code = A)
			moveHeader7.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			SetTOLData(moveHeader7);
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelTransferOfLiability);
			sendingObj.Send();
			//14.	Submit a WP to Arrive in-bond 7 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//15.	Submit a WP to Export in-bond 7 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			Factory.Save();
			#endregion
		}

		public void TestInBondUserTest2()
		{
			//Preconditions: CBP add 7 Rail BOLs on File with 7 In-Bonds using Bonded Carrier ID = 22-987564322
			//				CBP add Consist with 7 BOLs
			//				Arrive Conveyance
			//				Arrive each In-bond at Destination
			#region Step 1
			var header = CreateInBondHeader(InBondHeaderTypeList.Codes.AMS, "RWRS", TransportModeCodes.Codes.RailNonContainer);
			var moveHeader1 = CreateInBondDataWithBillsAndDetails(header, "200000001", new string[] { "MB0000021" }, InbondCommonTypeList.Codes._1ImmediateTransport, "22-987564322");
			moveHeader1.MovementDetails[0].B9_PreviousITNumber = "123456782";
			var moveHeader2 = CreateInBondDataWithBillsAndDetails(header, "200000002", new string[] { "MB0000022" }, InbondCommonTypeList.Codes._3ImmediateExport, "22-987564322");
			moveHeader2.MovementDetails[0].B9_PreviousITNumber = "564323454";
			var moveHeader3 = CreateInBondDataWithBillsAndDetails(header, "200000003", new string[] { "MB0000023" }, inBondCarrierID: "22-987564322");
			var moveDetails3 = moveHeader3.MovementDetails[0];
			moveDetails3.B9_FirstSecondaryNotifyParty = "RWRS";
			moveDetails3.B9_SecondSecondaryNotifyParty = "XXXS";
			moveDetails3.B9_ThirdSecondaryNotifyParty = "1512SV3";
			moveDetails3.B9_FourthSecondaryNotifyParty = "XXXY";
			moveDetails3.B9_PreviousITNumber = "100200332";
			var moveHeader4 = CreateInBondDataWithBillsAndDetails(header, "200000004", new string[] { "MB0000024" }, inBondCarrierID: "22-987564322");
			moveHeader4.MovementDetails[0].B9_PreviousITNumber = "200600304";
			var moveHeader5 = CreateInBondDataWithBillsAndDetails(header, "200000005", new string[] { "MB0000025" }, inBondCarrierID: "22-987564322");
			moveHeader5.MovementDetails[0].B9_PreviousITNumber = "300600403";
			var moveHeader6 = CreateInBondDataWithBillsAndDetails(header, "200000006", new string[] { "MB0000026" }, inBondCarrierID: "22-987564322");
			moveHeader6.MovementDetails[0].B9_PreviousITNumber = "900300402";
			var moveHeader7 = CreateInBondDataWithBillsAndDetails(header, "200000007", new string[] { "MB0000027" }, inBondCarrierID: "22-987564322");
			moveHeader7.MovementDetails[0].B9_PreviousITNumber = "478562206";
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
			//1.	Submit a QP to add 7 Subsequent In-bonds to 7 BOLs using Bonded Carrier ID = 22-987564322 for RWRS
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				obj.Send();
			}

			Factory.Save();
			#endregion
			#region Steps 2-15
			//2.	Submit a QP to delete Sub-leg In-bond 1 from BOL 1
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var sendingObj = new InBondMessageSendingObject(moveHeader1, InBondMessageType.DepartureDelete);
			sendingObj.Send();
			moveHeader2.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			//3.	Submit a WP to Export Sub-leg In-bond 2 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader2, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			moveHeader3.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			//4.	Submit a WP to Arrive Sub-leg in-bond 3 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//5.	Submit a WP to Export Sub-leg in-bond 3 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//6.	Submit a WP to Arrive Sub-leg in-bond by BOL 4 at destination (Action Code = 2)
			//7.	Submit a WP to Export Sub-leg in-bond by BOL 4 from destination (Action Code = 6)
			//8.	Submit a WP to Arrive Sub-leg in-bond by BOL 5 at destination (Action Code = 3)
			//9.	Submit a WP to Export Sub-leg in-bond by Container 5 from destination (Action Code = 7)
			//10.	Submit a WP to Divert Sub-leg in-bond 6 to a new destination (Action Code = Z)
			//11.	Submit a WP to Arrive Sub-leg in-bond 6 at new destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader6, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//12.	Submit a WP to Export Sub-leg in-bond 6 from new destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader6, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//13.	Submit a WP to Transfer Liability of Sub-leg in-bond 7 to another Bonded Carrier ID = 11-765432100 for XXXW (Action Code = A)
			SetTOLData(moveHeader7);
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelTransferOfLiability);
			sendingObj.Send();
			//14.	Submit a WP to Arrive Sub-leg in-bond 7 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//15.	Submit a WP to Export Sub-leg in-bond 7 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			Factory.Save();
			#endregion
		}

		public void TestInBondUserTest3()
		{
			#region Step 1
			var carrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "XXXS"));
			if (carrier == null)
			{
				carrier = Factory.New<USCarrierCombined>();
				carrier.UI_Code = "XXXS";
				carrier.UI_Name = "ACE M1 TEST CARRIER 2";
			}

			carrier.UI_ModeOfTransportation = "30";
			Factory.Save();
			var header = CreateInBondHeader(InBondHeaderTypeList.Codes.FullData, "XXXS", TransportModeCodes.Codes.TruckNonContainer);
			var moveHeader1 = CreateInBondDataWithBillsAndDetails(header, "300000001", new string[] { "BOL1", "BOL2", "BOL3" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			var moveHeader2 = CreateInBondDataWithBillsAndDetails(header, "300000002", new string[] { "BOL4", "BOL5" });
			var moveHeader3 = CreateInBondDataWithBillsAndDetails(header, "300000003", new string[] { "BOL6", "BOL7" });
			var moveHeader4 = CreateInBondDataWithBillsAndDetails(header, "300000004", new string[] { "BOL8", "BOL9" });
			var moveHeader5 = CreateInBondDataWithBillsAndDetails(header, "300000005", new string[] { "BOL10", "BOL11" });
			var moveHeader6 = CreateInBondDataWithBillsAndDetails(header, "300000006", new string[] { "BOL12", "BOL13" }, InbondCommonTypeList.Codes._3ImmediateExport);
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
			//1.	Submit a QP to add 7 Subsequent In-bonds to 7 BOLs using Bonded Carrier ID = 22-987564322 for RWRS
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				obj.Send();
			}

			Factory.Save();
			#endregion
			#region Steps 2-12
			//2.	Submit a QP to Delete in-bond 1 from BOL #1 (Action Code = B)
			//3.	Submit a QP to Delete in-bond 2 from BOLs #4-5 (Action Code = D)
			//4.	Submit a WP to Arrive entire in-bond 1 at destination (Action Code = 1)
			var sendingObj = new InBondMessageSendingObject(moveHeader1, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//5.	Submit a WP to Export in-bond 3 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//6.	Submit a WP to Export in-bond by BOL 8 from destination (Action Code = 6)
			//7.	Submit a WP to Arrive in-bond by BOL 9 at destination (Action Code = 2)
			//8.	Submit a WP to Arrive in-bond by Container/Equipment 10 at destination (Action Code = 3)
			//9.	Submit a WP to Arrive in-bond by BOL 9 at destination (Action Code = 7)
			//10.	Submit a WP to Divert in-bond 5 to a new destination (Action Code = Z)
			//11.	Submit a WP to Arrive in-bond 5 at new destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader5, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//12.	Submit a WP to Export in-bond 6 by Container/Equipment 10 at destination (Action Code = 7)
			#endregion
			#region Steps 13-17
			//13.	Submit a QP to add 6 BOLs (BOL #14 - BOL #19) all for a Bonded Warehouse Withdrawal 
			//for a total of 3 unique In-bond entries for types 61, 62 and 63 using Bonded Carrier ID = 11-987654300.
			var moveHeader7 = CreateInBondDataWithBillsAndDetails(header, "300000007", new string[] { "BOL14" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			var moveHeader8 = CreateInBondDataWithBillsAndDetails(header, "300000008", new string[] { "BOL15" });
			var moveHeader9 = CreateInBondDataWithBillsAndDetails(header, "300000009", new string[] { "BOL16" }, InbondCommonTypeList.Codes._3ImmediateExport);
			Factory.Save();
			collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				if (obj.US_InBondNumber.StartsWith("300000007") || obj.US_InBondNumber.StartsWith("300000008") || obj.US_InBondNumber.StartsWith("300000009"))
				{
					obj.Send();
				}
			}

			Factory.Save();
			//14.	Submit a QP to Delete in-bond 7 from BOL #14 (Action Code = D)
			//15.	Submit a WP to Transfer Liability of in-bond 8 to another Bonded Carrier ID = 11-765432100 for XXXW (Action Code = A)
			SetTOLData(moveHeader8);
			sendingObj = new InBondMessageSendingObject(moveHeader8, InBondMessageType.InBondLevelTransferOfLiability);
			sendingObj.Send();
			//16.	Submit a WP to Arrive in-bond 8 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader8, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//17.	Submit a WP to Export in-bond by BOL 16 from destination (Action Code = 6)
			#endregion
		}

		public void TestInBondUserTest4()
		{
			#region Steps 1-2
			//Precondition: CBP add 7 Truck BOLs with no In-Bonds
			var header = CreateInBondHeader(InBondHeaderTypeList.Codes.FullData, "XXXW", TransportModeCodes.Codes.TruckNonContainer);
			var moveHeader1 = CreateInBondDataWithBillsAndDetails(header, "400000001", new string[] { "BOL1" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			var moveHeader2 = CreateInBondDataWithBillsAndDetails(header, "400000002", new string[] { "BOL2" }, InbondCommonTypeList.Codes._3ImmediateExport);
			var moveHeader3 = CreateInBondDataWithBillsAndDetails(header, "400000003", new string[] { "BOL3" });
			CreateInBondDataWithBillsAndDetails(header, "400000004", new string[] { "BOL4" });
			CreateInBondDataWithBillsAndDetails(header, "400000005", new string[] { "BOL5" });
			var moveHeader6 = CreateInBondDataWithBillsAndDetails(header, "400000006", new string[] { "BOL6" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			var moveHeader7 = CreateInBondDataWithBillsAndDetails(header, "400000007", new string[] { "BOL7" }, InbondCommonTypeList.Codes._2TransportandExport);
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
			//1.	Submit a QP to add 7 In-bonds to 7 Truck BOLs using Bonded Carrier ID = 11-987654300
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				obj.Send();
			}

			//2.	Submit a QP to delete In-bond 1 from BOL 1
			moveHeader1.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var sendingObj = new InBondMessageSendingObject(moveHeader1, InBondMessageType.DepartureDelete);
			sendingObj.Send();
			Factory.Save();
			#endregion
			#region Steps 3-14
			//Precondition: CBP arrive Conveyance
			//3.	Submit a WP to Export in-bond 2 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader2, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//4.	Submit a WP to Arrive in-bond 3 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//5.	Submit a WP to Export in-bond 3 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader3, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			//6.	Submit a WP to Arrive in-bond by BOL 4 at destination (Action Code = 2)
			//7.	Submit a WP to Export in-bond by BOL 4 from destination (Action Code = 6)
			//8.	Submit a WP to Arrive in-bond by BOL 5 at destination (Action Code = 3)
			//9.	Submit a WP to Export in-bond by BOL 5 from destination (Action Code = 7)
			//10.	Submit a WP to Divert in-bond 6 to a new destination (Action Code = Z)
			//11.	Submit a WP to Arrive in-bond 6 at new destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader6, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//12.	Submit a WP to Transfer Liability of in-bond 7 to another Bonded Carrier ID = 11-765432100 for XXXW (Action Code = A)
			SetTOLData(moveHeader7);
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelTransferOfLiability);
			sendingObj.Send();
			//13.	Submit a WP to Arrive in-bond 7 at destination (Action Code = 1)
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelArrival);
			sendingObj.Send();
			//14.	Submit a WP to Export in-bond 7 from destination (Action Code = 5)
			sendingObj = new InBondMessageSendingObject(moveHeader7, InBondMessageType.InBondLevelExportation);
			sendingObj.Send();
			Factory.Save();
			#endregion
		}

		public void TestInBondUserTest4_TradeStep3()
		{
			var header = CreateInBondHeader(InBondHeaderTypeList.Codes.FullData, "XXXW", TransportModeCodes.Codes.TruckNonContainer);
			//15.	Submit a QP Long to add 3 In-bonds and 3 Truck BOLs (Not on File in ACE) using Bonded Carrier ID = 11-987654300
			var moveHeader1 = CreateInBondDataWithBillsAndDetails(header, "410000001", new string[] { "BOL1" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			CreateInBondDataWithBillsAndDetails(header, "410000002", new string[] { "BOL2" });
			CreateInBondDataWithBillsAndDetails(header, "410000003", new string[] { "BOL3" }, InbondCommonTypeList.Codes._3ImmediateExport);
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				obj.Send();
			}

			collection.Factory.Save();
			//16.	Submit a QP to delete In-bond 1
			var sendingObj = new InBondMessageSendingObject(moveHeader1, InBondMessageType.DepartureDelete);
			AssertNoMessageErrorsBeforeSending(header);
			sendingObj.Send();
			Factory.Save();
			AssertEquals(2, moveHeader1.Messages.Count);
		}

		public void TestInBondUserTest4_TradeStep4()
		{
			var header = CreateInBondHeader(InBondHeaderTypeList.Codes.FullData, "XXXY", TransportModeCodes.Codes.FixedTransportInstallations);
			//17.	Submit a QP Long to add 3 In-bonds for Pipeline using Bonded Carrier ID = 11-987654300
			CreateInBondDataWithBillsAndDetails(header, "420000001", new string[] { "BOL1" }, InbondCommonTypeList.Codes._1ImmediateTransport);
			var moveHeader2 = CreateInBondDataWithBillsAndDetails(header, "420000002", new string[] { "BOL2" });
			CreateInBondDataWithBillsAndDetails(header, "420000003", new string[] { "BOL3" }, InbondCommonTypeList.Codes._3ImmediateExport);
			Factory.Save();
			AssertNoMessageErrorsBeforeSending(header);
			var collection = new InBondMessageSendingObjectCollection(new InBondMessageSendingHeaderObject(header.PK, InBondMessageType.DepartureAdd, new Customs.Business.SendsMessagesToCustomsShutterUpperer()));
			foreach (InBondMessageSendingObject obj in collection)
			{
				obj.Send();
			}

			collection.Factory.Save();
			//18.	 Submit a QP to delete In-bond 2
			var sendingObj = new InBondMessageSendingObject(moveHeader2, InBondMessageType.DepartureDelete);
			AssertNoMessageErrorsBeforeSending(header);
			sendingObj.Send();
			Factory.Save();
			AssertEquals(2, moveHeader2.Messages.Count);
		}

		void AssertNoMessageErrorsBeforeSending(BusinessObject bO)
		{
			bO.RunPreSaveValidation();
			var messageErrors = new Customs.Business.CustomsNotificationCollector(bO, true, false, Enterprise.Customs.Business.CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors().ToUniqueMessageListString();
			AssertEquals("Messages", "", messageErrors);
		}

		CusInBondHeader CreateInBondHeader(string headerType, ZString carrierSCAC, string transportMode)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3905", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "S004", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = headerType;
			header.BH_ImportTransportMode = transportMode;
			if (!carrierSCAC.IsEmpty)
			{
				header.BH_CarrierSCAC = carrierSCAC;
			}

			header.BH_ImportConveyanceName = "APL VESSEL";
			header.BH_VoyageNumber = "1234A";
			header.BH_ETA = new ZDateTime(2012, 10, 02, 18, 20, 10);
			header.BH_PortUnladingDCode = "3905";
			header.BH_FIRMS = "S004";
			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Bulgaria;
			return header;
		}

		CusInBondMoveHeader CreateInBondDataWithBillsAndDetails(CusInBondHeader header, string inBondNumber, string[] billNumbers, string inBondEntryType = InbondCommonTypeList.Codes._2TransportandExport, string inBondCarrierID = "11-987654300")
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "50200", "TORONTO, ONT, CA.", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60200", "TORONTO, ONT, CA.", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			var moveHeader = CreateMoveHeader(inBondEntryType, inBondNumber, inBondCarrierID);
			header.MovementHeaders.Add(moveHeader);
			foreach (var billNum in billNumbers)
			{
				var bill = CreateBill(billNum);
				header.Bills.Add(bill);
				var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
				CusInBondContainer billContainer = null;
				if (moveDetail.Containers.Count > 0)
				{
					billContainer = moveDetail.Containers[0];
				}
				else
				{
					billContainer = moveDetail.Containers.AddNew();
				}

				billContainer.BC_ContainerNum = "NC";
				var commodity = billContainer.Commodities.AddNew();
				commodity.BY_HarmonisedTariff = "00000002";
				commodity.BY_PieceCount = 436;
				commodity.BY_Description = "T";
				commodity.BY_MarksAndNumbers = "T";
				commodity.BY_MonetaryValue = 20;
				commodity.BY_GrossWeight = 5m;
			}

			return moveHeader;
		}

		CusInBondMoveHeader CreateMoveHeader(string inBondEntryType, string inBondNumber, string inBondCarrierID)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3905", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_InBondEntryType = inBondEntryType;
			moveHeader.AllocateInBondNumber(inBondNumber);
			moveHeader.BM_InBondCarrierSCAC = "XXXS";
			if (inBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport || inBondEntryType == InbondCommonTypeList.Codes._2TransportandExport)
			{
				moveHeader.BM_DestinationPortCode = "3902";
			}
			else
			{
				moveHeader.BM_DestinationPortCode = "3905";
			}

			if (inBondEntryType == InbondCommonTypeList.Codes._2TransportandExport || inBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport)
			{
				moveHeader.BM_ForeignDestPortKCode = "50200";
			}

			moveHeader.BM_MonetaryValue = 323.25m;
			moveHeader.BM_InBondCarrierID = inBondCarrierID;
			moveHeader.BM_BTAIndicator = YesNoDefaultList.Codes.Yes;
			return moveHeader;
		}

		CusInBondBill CreateBill(string billNumber)
		{
			var bill = Factory.New<CusInBondBill>();
			bill.B0_IssuerCode = "XXXS";
			bill.B0_MasterBillNumber = billNumber;
			bill.B0_PortOfLadingKCode = "60200";
			bill.B0_ManifestQty = 436;
			bill.B0_ManifestUQ = InBondManifestUQList.Codes.ENV;
			bill.B0_Weight = 974.64m;
			bill.B0_WeightUQ = Core.Constants.Weight.Pounds;
			bill.B0_Volume = 3.54m;
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicFeet;
			bill.B0_PlaceOfReceiptDCode = "3902";
			bill.ForeignShipper.E2_AddressOverride = ZBool.True;
			bill.ForeignShipper.E2_CompanyName = "Test 1";
			bill.ForeignShipper.E2_Address1 = "Address 1";
			bill.Consignee.E2_AddressOverride = ZBool.True;
			bill.Consignee.E2_CompanyName = "Test 2";
			bill.Consignee.E2_Address1 = "Address 1";
			bill.NotifyParty.E2_AddressOverride = ZBool.True;
			bill.NotifyParty.E2_CompanyName = "Test 3";
			bill.NotifyParty.E2_Address1 = "Address 1";
			return bill;
		}

		void SetTOLData(IInBondMessagingHeader iMsgHeader)
		{
			iMsgHeader.TOLCarrierCode = "XXXW";
			iMsgHeader.TOLCarrierID = "11-765432100";
		}
	}
}
