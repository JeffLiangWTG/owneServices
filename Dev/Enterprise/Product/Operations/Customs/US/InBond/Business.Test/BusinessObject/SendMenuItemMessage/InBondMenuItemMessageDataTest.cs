using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMenuItemMessageData))]
	sealed class InBondMenuItemMessageDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShowErrorFromInBondWhenNoSecurityRightToSendMessageWithMessageErrors()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			var header1 = Factory.NewWithValidTestData<CusInBondHeader>();
			var header2 = Factory.NewWithValidTestData<CusInBondHeader>();
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.InBondNumber = "111111111";
			movement1.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			var movement2 = header2.MovementHeaders.AddNew();
			movement2.InBondNumber = "222222222";
			movement2.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			var messageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>() { movement1, movement2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var movementHeaderNeedToBeSent = messageData.CreateOrUpdateMovementHeader();
			var (invalidOperationText, result) = (string.Empty, string.Empty);
			AssertNoExceptionThrown(() =>
			{
				(invalidOperationText, result) = messageData.SendMessages(movementHeaderNeedToBeSent);
			});
			CombineAssertions(() =>
			{
				AssertMultilineASCIIEquals(@"INB0000001
There are message errors on this job and you don't have security rights to send with message errors. If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 
Operate -> Customs -> Customs Declarations -> Send With Message Errors

Please fix these errors before sending any messages:

Arrival Date: You have not entered an Arrival Date.
Arrival Firms Code: You have not entered an Arrival Firms Code.

INB0000002
There are message errors on this job and you don't have security rights to send with message errors. If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 
Operate -> Customs -> Customs Declarations -> Send With Message Errors

Please fix these errors before sending any messages:

Arrival Date: You have not entered an Arrival Date.
Arrival Firms Code: You have not entered an Arrival Firms Code.", invalidOperationText);
				AssertEquals("0 messages have been successfully sent. 2 messages have failed to be sent.", result);
			});
		}

		public void TestSaveToDBAndSendArrivalMessages()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			inBondHeader1.BH_OA_Importer_ZAddress.OrgPK = org1.PK;
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			movementHeader1.BM_ArrivalDate = new ZDateTime(2021, 10, 20);
			movementHeader1.BM_DestinationPortCode = "A";
			movementHeader1.BM_FIRMS = "B";
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			movementHeader2.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			var inBondHeader3 = Factory.New<CusInBondHeader>();
			var movementHeader4 = inBondHeader3.MovementHeaders.AddNew();
			movementHeader4.InBondNumber = "111111114";
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2, movementHeader3 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var inBondMenuItemMessageSendingObject1 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject1.InBondNumber = "111111114";
			var inBondMenuItemMessageSendingObject2 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject2.InBondNumber = "111111115";
			inBondMenuItemMessageSendingObject2.ImporterOrgPK = org1.PK;
			inBondMenuItemMessageSendingObject2.InBondCarrierOrgPK = org2.PK;
			inBondMenuItemMessageSendingObject2.InBondCarrierCodeSCAC = "C";
			inBondMenuItemMessageSendingObject2.USDestinationPortCode = "D";
			inBondMenuItemMessageSendingObject2.ForeignDestinationPortCode = "E";
			var inBondMenuItemMessageSendingObject3 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject3.InBondNumber = "111111116";
			inBondMenuItemMessageSendingObject3.ImporterOrgPK = org2.PK;
			inBondMenuItemMessageSendingObject3.InBondCarrierOrgPK = org2.PK;
			inBondMenuItemMessageSendingObject3.InBondCarrierCodeSCAC = "F";
			inBondMenuItemMessageSendingObject3.USDestinationPortCode = "G";
			inBondMenuItemMessageSendingObject3.ForeignDestinationPortCode = "H";
			var inBondMenuItemMessageSendingObject4 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject4.InBondNumber = "111111117";
			inBondMenuItemMessageSendingObject4.ImporterOrgPK = org1.PK;
			var inBondMenuItemMessageSendingObject5 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject5.InBondNumber = "111111118";
			inBondMenuItemMessageData.USDestinationPortCode = "I";
			inBondMenuItemMessageData.FIRMSCode = "J";
			inBondMenuItemMessageData.ArrivalDate = new ZDateTime(2021, 10, 21);
			var inBondNeedToLock = Factory.Load<CusInBondHeader>(inBondHeader2.PK);
			inBondNeedToLock.LockSendCustomsMessageMutex();
			var movementHeadersNeedToSent = inBondMenuItemMessageData.CreateOrUpdateMovementHeader();
			inBondMenuItemMessageData.Factory.Save();
			var (invalidOperationText, result) = inBondMenuItemMessageData.SendMessages(movementHeadersNeedToSent);
			inBondNeedToLock.UnlockSendCustomsMessageMutex();
			AssertEquals("There are 2 messages cannot be sent", "6 messages have been successfully sent. 2 messages have failed to be sent.", result);
			var moveHeader1 = GetInBondMoveHeader("111111111");
			AssertEquals("BM_ArrivalDate of MovementHeader 111111111", new ZDateTime(2021, 10, 20), moveHeader1.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111111", "A", moveHeader1.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111111", "B", moveHeader1.BM_FIRMS);
			AssertEquals("Message ent successfully", 1, moveHeader1.Messages.Count);
			var moveHeader2 = GetInBondMoveHeader("111111112");
			AssertEquals("BM_ArrivalDate of MovementHeader 111111112", new ZDateTime(2021, 10, 21), moveHeader2.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111112", "I", moveHeader2.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111112", "J", moveHeader2.BM_FIRMS);
			AssertEquals("Failed to send message.", 0, moveHeader2.Messages.Count);
			var moveHeader3 = GetInBondMoveHeader("111111113");
			AssertEquals("BM_ArrivalDate of MovementHeader 111111113, the inbond header is locked by mutex", ZDateTime.Empty, moveHeader3.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111113, the inbond header is locked by mutex", ZString.Empty, moveHeader3.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111113, the inbond header is locked by mutex", ZString.Empty, moveHeader3.BM_FIRMS);
			AssertEquals("Failed to send message.", 0, moveHeader3.Messages.Count);
			var moveHeader4 = GetInBondMoveHeader("111111114");
			AssertEquals("BM_ArrivalDate of MovementHeader 111111114", new ZDateTime(2021, 10, 21), moveHeader4.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111114", "I", moveHeader4.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111114", "J", moveHeader4.BM_FIRMS);
			AssertEquals("Message ent successfully", 1, moveHeader4.Messages.Count);
			var moveHeader5 = GetInBondMoveHeader("111111115");
			var header1 = moveHeader5.Header;
			AssertEquals(org1.PK, header1.BH_OA_Importer_ZAddress.OrgPK);
			AssertEquals("111111115 & 111111117", 2, header1.MovementHeaders.Count);
			AssertEquals("Should be INB", CusInBondApplicationCodeList.Codes.InBond, header1.BH_ApplicationCode);
			AssertEquals("Should be current branch", GlbBranch.CurrentBranch.PK, header1.BH_GB);
			Assert("Should be true", header1.BH_PostDepartureOnly);
			AssertEquals("BM_ArrivalDate of MovementHeader 111111115", new ZDateTime(2021, 10, 21), moveHeader5.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111115", "D", moveHeader5.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111115", "J", moveHeader5.BM_FIRMS);
			AssertEquals("InBondCarrierOrgPK of MovementHeader 111111115", org2.PK, moveHeader5.InBondCarrierOrgPK);
			AssertEquals("BM_InBondCarrierSCAC of MovementHeader 111111115", "C", moveHeader5.BM_InBondCarrierSCAC);
			AssertEquals("BM_ForeignDestPortKCode of MovementHeader 111111115", "E", moveHeader5.BM_ForeignDestPortKCode);
			AssertEquals("Message ent successfully", 1, moveHeader5.Messages.Count);
			var moveHeader7 = header1.MovementHeaders.Cast<CusInBondMoveHeader>().First(x => x.InBondNumber == "111111117");
			AssertEquals("BM_ArrivalDate of MovementHeader 111111117", new ZDateTime(2021, 10, 21), moveHeader7.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111117", "I", moveHeader7.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111117", "J", moveHeader7.BM_FIRMS);
			AssertEquals("Message ent successfully", 1, moveHeader7.Messages.Count);
			var moveHeader6 = GetInBondMoveHeader("111111116");
			var header2 = moveHeader6.Header;
			AssertEquals(org2.PK, header2.BH_OA_Importer_ZAddress.OrgPK);
			AssertEquals("Only 111111116", 1, header2.MovementHeaders.Count);
			AssertEquals("Should be INB", CusInBondApplicationCodeList.Codes.InBond, header2.BH_ApplicationCode);
			AssertEquals("Should be current branch", GlbBranch.CurrentBranch.PK, header2.BH_GB);
			Assert("Should be true", header2.BH_PostDepartureOnly);
			AssertEquals("BM_ArrivalDate of MovementHeader 111111116", new ZDateTime(2021, 10, 21), moveHeader6.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111116", "G", moveHeader6.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111116", "J", moveHeader6.BM_FIRMS);
			AssertEquals("InBondCarrierOrgPK of MovementHeader 111111116", org2.PK, moveHeader6.InBondCarrierOrgPK);
			AssertEquals("BM_InBondCarrierSCAC of MovementHeader 111111116", "F", moveHeader6.BM_InBondCarrierSCAC);
			AssertEquals("BM_ForeignDestPortKCode of MovementHeader 111111116", "H", moveHeader6.BM_ForeignDestPortKCode);
			AssertEquals("Message ent successfully", 1, moveHeader6.Messages.Count);
			var moveHeader8 = GetInBondMoveHeader("111111118");
			var header3 = moveHeader8.Header;
			AssertEquals(ZGuid.Empty, header3.BH_OA_Importer_ZAddress.OrgPK);
			AssertEquals("Only 111111118", 1, header3.MovementHeaders.Count);
			AssertEquals("Should be INB", CusInBondApplicationCodeList.Codes.InBond, header3.BH_ApplicationCode);
			AssertEquals("Should be current branch", GlbBranch.CurrentBranch.PK, header3.BH_GB);
			Assert("Should be true", header3.BH_PostDepartureOnly);
			AssertEquals("BM_ArrivalDate of MovementHeader 111111118", new ZDateTime(2021, 10, 21), moveHeader8.BM_ArrivalDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111118", "I", moveHeader8.BM_DestinationPortCode);
			AssertEquals("BM_FIRMS of MovementHeader 111111118", "J", moveHeader8.BM_FIRMS);
			AssertEquals("Message ent successfully", 1, moveHeader8.Messages.Count);
		}

		public void TestSaveToDBAndSendExportMessages()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			inBondHeader1.BH_OA_Importer_ZAddress.OrgPK = org1.PK;
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			movementHeader1.BM_ExportDate = new ZDateTime(2021, 10, 20);
			movementHeader1.BM_DestinationPortCode = "A";
			movementHeader1.BM_ExportTransportMode = "B";
			movementHeader1.BM_ExportLadenOn = "C";
			movementHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader2.InBondNumber = "111111112";
			movementHeader2.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			movementHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			movementHeader3.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var inBondHeader3 = Factory.New<CusInBondHeader>();
			var movementHeader4 = inBondHeader3.MovementHeaders.AddNew();
			movementHeader4.InBondNumber = "111111114";
			movementHeader4.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2, movementHeader3 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			var inBondMenuItemMessageSendingObject1 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject1.InBondNumber = "111111114";
			inBondMenuItemMessageSendingObject1.EntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			var inBondMenuItemMessageSendingObject2 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject2.InBondNumber = "111111115";
			inBondMenuItemMessageSendingObject2.ImporterOrgPK = org1.PK;
			inBondMenuItemMessageSendingObject2.InBondCarrierOrgPK = org2.PK;
			inBondMenuItemMessageSendingObject2.InBondCarrierCodeSCAC = "C";
			inBondMenuItemMessageSendingObject2.USDestinationPortCode = "D";
			inBondMenuItemMessageSendingObject2.ForeignDestinationPortCode = "E";
			inBondMenuItemMessageSendingObject2.EntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var inBondMenuItemMessageSendingObject3 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject3.InBondNumber = "111111116";
			inBondMenuItemMessageSendingObject3.ImporterOrgPK = org2.PK;
			inBondMenuItemMessageSendingObject3.InBondCarrierOrgPK = org2.PK;
			inBondMenuItemMessageSendingObject3.InBondCarrierCodeSCAC = "F";
			inBondMenuItemMessageSendingObject3.USDestinationPortCode = "G";
			inBondMenuItemMessageSendingObject3.ForeignDestinationPortCode = "H";
			inBondMenuItemMessageSendingObject3.EntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var inBondMenuItemMessageSendingObject4 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject4.InBondNumber = "111111117";
			inBondMenuItemMessageSendingObject4.ImporterOrgPK = org1.PK;
			inBondMenuItemMessageSendingObject4.EntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var inBondMenuItemMessageSendingObject5 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject5.InBondNumber = "111111118";
			inBondMenuItemMessageSendingObject5.EntryType = InbondCommonTypeList.Codes._2TransportandExport;
			inBondMenuItemMessageData.USDestinationPortCode = "I";
			inBondMenuItemMessageData.ExportMOT = "J";
			inBondMenuItemMessageData.ExportConveyance = "K";
			inBondMenuItemMessageData.ExportDate = new ZDateTime(2021, 10, 21);
			var inBondNeedToLock = Factory.Load<CusInBondHeader>(inBondHeader2.PK);
			inBondNeedToLock.LockSendCustomsMessageMutex();
			var movementHeadersNeedToSent = inBondMenuItemMessageData.CreateOrUpdateMovementHeader();
			inBondMenuItemMessageData.Factory.Save();
			var (invalidOperationText, result) = inBondMenuItemMessageData.SendMessages(movementHeadersNeedToSent);
			inBondNeedToLock.UnlockSendCustomsMessageMutex();
			AssertEquals("There are 3 messages cannot be sent", "5 messages have been successfully sent. 3 messages have failed to be sent.", result);
			var moveHeader1 = GetInBondMoveHeader("111111111");
			AssertEquals("BM_ExportDate of MovementHeader 111111111", new ZDateTime(2021, 10, 20), moveHeader1.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111111", "A", moveHeader1.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111111", "B", moveHeader1.BM_ExportTransportMode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111111", "C", moveHeader1.BM_ExportLadenOn);
			AssertEquals("Message ent successfully", 1, moveHeader1.Messages.Count);
			var moveHeader2 = GetInBondMoveHeader("111111112");
			AssertEquals("BM_ExportDate of MovementHeader 111111112", new ZDateTime(2021, 10, 21), moveHeader2.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111112", "I", moveHeader2.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111112", "J", moveHeader2.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111112", "K", moveHeader2.BM_ExportLadenOn);
			AssertEquals("Failed to send message.", 0, moveHeader2.Messages.Count);
			var moveHeader3 = GetInBondMoveHeader("111111113");
			AssertEquals("BM_ExportDate of MovementHeader 111111113, the inbond header is locked by mutex", ZDateTime.Empty, moveHeader3.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111113, the inbond header is locked by mutex", ZString.Empty, moveHeader3.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111113, the inbond header is locked by mutex", ZString.Empty, moveHeader3.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111113, the inbond header is locked by mutex", ZString.Empty, moveHeader3.BM_ExportLadenOn);
			AssertEquals("Failed to send message.", 0, moveHeader3.Messages.Count);
			var moveHeader4 = GetInBondMoveHeader("111111114");
			AssertEquals("BM_ExportDate of MovementHeader 111111114", new ZDateTime(2021, 10, 21), moveHeader4.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111114", "I", moveHeader4.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111114", "J", moveHeader4.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111114", "K", moveHeader4.BM_ExportLadenOn);
			AssertEquals("BM_InBondEntryType of MovementHeader 111111114", InbondCommonTypeList.Codes._3ImmediateExport, moveHeader4.BM_InBondEntryType);
			AssertEquals("Message ent successfully", 1, moveHeader4.Messages.Count);
			var moveHeader5 = GetInBondMoveHeader("111111115");
			var header1 = moveHeader5.Header;
			AssertEquals(org1.PK, header1.BH_OA_Importer_ZAddress.OrgPK);
			AssertEquals("111111115 & 111111117", 2, header1.MovementHeaders.Count);
			AssertEquals("Should be INB", CusInBondApplicationCodeList.Codes.InBond, header1.BH_ApplicationCode);
			AssertEquals("Should be current branch", GlbBranch.CurrentBranch.PK, header1.BH_GB);
			Assert("Should be true", header1.BH_PostDepartureOnly);
			AssertEquals("BM_ExportDate of MovementHeader 111111115", new ZDateTime(2021, 10, 21), moveHeader5.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111115", "D", moveHeader5.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111115", "J", moveHeader5.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111115", "K", moveHeader5.BM_ExportLadenOn);
			AssertEquals("InBondCarrierOrgPK of MovementHeader 111111115", org2.PK, moveHeader5.InBondCarrierOrgPK);
			AssertEquals("BM_InBondCarrierSCAC of MovementHeader 111111115", "C", moveHeader5.BM_InBondCarrierSCAC);
			AssertEquals("BM_ForeignDestPortKCode of MovementHeader 111111115", "E", moveHeader5.BM_ForeignDestPortKCode);
			AssertEquals("BM_InBondEntryType of MovementHeader 111111115", InbondCommonTypeList.Codes._1ImmediateTransport, moveHeader5.BM_InBondEntryType);
			AssertEquals("Failed to send message.", 0, moveHeader5.Messages.Count);
			var moveHeader7 = header1.MovementHeaders.Cast<CusInBondMoveHeader>().First(x => x.InBondNumber == "111111117");
			AssertEquals("BM_ExportDate of MovementHeader 111111117", new ZDateTime(2021, 10, 21), moveHeader7.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111117", "I", moveHeader7.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111117", "J", moveHeader7.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111117", "K", moveHeader7.BM_ExportLadenOn);
			AssertEquals("BM_InBondEntryType of MovementHeader 111111117", InbondCommonTypeList.Codes._2TransportandExport, moveHeader7.BM_InBondEntryType);
			AssertEquals("Message ent successfully", 1, moveHeader7.Messages.Count);
			var moveHeader6 = GetInBondMoveHeader("111111116");
			var header2 = moveHeader6.Header;
			AssertEquals(org2.PK, header2.BH_OA_Importer_ZAddress.OrgPK);
			AssertEquals("Only 111111116", 1, header2.MovementHeaders.Count);
			AssertEquals("Should be INB", CusInBondApplicationCodeList.Codes.InBond, header2.BH_ApplicationCode);
			AssertEquals("Should be current branch", GlbBranch.CurrentBranch.PK, header2.BH_GB);
			Assert("Should be true", header2.BH_PostDepartureOnly);
			AssertEquals("BM_ExportDate of MovementHeader 111111116", new ZDateTime(2021, 10, 21), moveHeader6.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111116", "G", moveHeader6.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111116", "J", moveHeader6.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111116", "K", moveHeader6.BM_ExportLadenOn);
			AssertEquals("InBondCarrierOrgPK of MovementHeader 111111116", org2.PK, moveHeader6.InBondCarrierOrgPK);
			AssertEquals("BM_InBondCarrierSCAC of MovementHeader 111111116", "F", moveHeader6.BM_InBondCarrierSCAC);
			AssertEquals("BM_ForeignDestPortKCode of MovementHeader 111111116", "H", moveHeader6.BM_ForeignDestPortKCode);
			AssertEquals("BM_InBondEntryType of MovementHeader 111111116", InbondCommonTypeList.Codes._2TransportandExport, moveHeader6.BM_InBondEntryType);
			AssertEquals("Message ent successfully", 1, moveHeader6.Messages.Count);
			var moveHeader8 = GetInBondMoveHeader("111111118");
			var header3 = moveHeader8.Header;
			AssertEquals(ZGuid.Empty, header3.BH_OA_Importer_ZAddress.OrgPK);
			AssertEquals("Only 111111118", 1, header3.MovementHeaders.Count);
			AssertEquals("Should be INB", CusInBondApplicationCodeList.Codes.InBond, header3.BH_ApplicationCode);
			AssertEquals("Should be current branch", GlbBranch.CurrentBranch.PK, header3.BH_GB);
			Assert("Should be true", header3.BH_PostDepartureOnly);
			AssertEquals("BM_ExportDate of MovementHeader 111111118", new ZDateTime(2021, 10, 21), moveHeader8.BM_ExportDate);
			AssertEquals("BM_DestinationPortCode of MovementHeader 111111118", "I", moveHeader8.BM_DestinationPortCode);
			AssertEquals("BM_ExportTransportMode of MovementHeader 111111118", "J", moveHeader8.BM_ExportTransportMode);
			AssertEquals("BM_ExportLadenOn of MovementHeader 111111118", "K", moveHeader8.BM_ExportLadenOn);
			AssertEquals("BM_InBondEntryType of MovementHeader 111111118", InbondCommonTypeList.Codes._2TransportandExport, moveHeader8.BM_InBondEntryType);
			AssertEquals("Message ent successfully", 1, moveHeader8.Messages.Count);
		}

		public void TestSendArrivalMessages_AllSuccess()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			inBondHeader1.BH_OA_Importer_ZAddress.OrgPK = org1.PK;
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			movementHeader1.BM_ArrivalDate = new ZDateTime(2021, 10, 20);
			movementHeader1.BM_DestinationPortCode = "A";
			movementHeader1.BM_FIRMS = "B";
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			inBondMenuItemMessageData.USDestinationPortCode = "I";
			inBondMenuItemMessageData.FIRMSCode = "J";
			inBondMenuItemMessageData.ArrivalDate = new ZDateTime(2021, 10, 21);
			var movementHeadersNeedToSent = inBondMenuItemMessageData.CreateOrUpdateMovementHeader();
			inBondMenuItemMessageData.Factory.Save();
			var (invalidOperationText, result) = inBondMenuItemMessageData.SendMessages(movementHeadersNeedToSent);
			AssertEquals("All success", "1 message has been successfully sent.", result);
		}

		public void TestMessageType()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			Assert("Is Arrival", inBondMenuItemMessageData.IsArrival);
			Assert("Is not Export", !inBondMenuItemMessageData.IsExport);
			Assert("Is not Pedimento", !inBondMenuItemMessageData.IsPedimento);
			Assert("Is not PrintDocument", !inBondMenuItemMessageData.IsPrintDocument);
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			Assert("Is not Arrival", !inBondMenuItemMessageData.IsArrival);
			Assert("Is Export", inBondMenuItemMessageData.IsExport);
			Assert("Is not Pedimento", !inBondMenuItemMessageData.IsPedimento);
			Assert("Is not PrintDocument", !inBondMenuItemMessageData.IsPrintDocument);
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			Assert("Is not Arrival", !inBondMenuItemMessageData.IsArrival);
			Assert("Is not Export", !inBondMenuItemMessageData.IsExport);
			Assert("Is Pedimento", inBondMenuItemMessageData.IsPedimento);
			Assert("Is not PrintDocument", !inBondMenuItemMessageData.IsPrintDocument);
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.PrintDocument);
			Assert("Is not Arrival", !inBondMenuItemMessageData.IsArrival);
			Assert("Is not Export", !inBondMenuItemMessageData.IsExport);
			Assert("Is not Pedimento", !inBondMenuItemMessageData.IsPedimento);
			Assert("Is PrintDocument", inBondMenuItemMessageData.IsPrintDocument);
		}

		public void TestAllocatePredimentoNumber()
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var bill1 = inBondHeader1.Bills.AddNew();
			var fenNumber1 = bill1.AdditionalReferences.AddNew();
			fenNumber1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber1.BR_ReferenceNum = "A";
			var fenNumber2 = bill1.AdditionalReferences.AddNew();
			fenNumber2.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber2.BR_ReferenceNum = "B";
			var bill2 = inBondHeader1.Bills.AddNew();
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			movementHeader1.InBondNumber = "111111111";
			var movementDetail1 = movementHeader1.MovementDetails.AddNew();
			movementDetail1.B9_B0 = bill1.PK;
			var movementDetail2 = movementHeader1.MovementDetails.AddNew();
			movementDetail2.B9_B0 = bill2.PK;
			var movementHeader2 = inBondHeader1.MovementHeaders.AddNew();
			var movementDetail3 = movementHeader2.MovementDetails.AddNew();
			movementDetail3.B9_B0 = bill1.PK;
			var inBondHeader2 = Factory.New<CusInBondHeader>();
			var bill3 = inBondHeader1.Bills.AddNew();
			var fenNumber3 = bill3.AdditionalReferences.AddNew();
			fenNumber3.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber3.BR_ReferenceNum = "C";
			var bill4 = inBondHeader1.Bills.AddNew();
			var fenNumber4 = bill4.AdditionalReferences.AddNew();
			fenNumber4.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber4.BR_ReferenceNum = "D";
			var movementHeader3 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader3.InBondNumber = "111111113";
			var movementDetail4 = movementHeader3.MovementDetails.AddNew();
			movementDetail4.B9_B0 = bill3.PK;
			var movementHeader4 = inBondHeader2.MovementHeaders.AddNew();
			movementHeader4.InBondNumber = "111111114";
			var movementDetail5 = movementHeader4.MovementDetails.AddNew();
			movementDetail5.B9_B0 = bill4.PK;
			Factory.Save();
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1, movementHeader2 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			var inBondMenuItemMessageSendingObject1 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber == "111111111");
			inBondMenuItemMessageSendingObject1.PedimentoNumber = "E";
			var inBondMenuItemMessageSendingObject2 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.Cast<InBondMenuItemMessageSendingObject>().First(x => x.InBondNumber.IsEmpty);
			inBondMenuItemMessageSendingObject2.InBondNumber = "111111112";
			inBondMenuItemMessageSendingObject2.PedimentoNumber = "F";
			var inBondMenuItemMessageSendingObject3 = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects.AddNew();
			inBondMenuItemMessageSendingObject3.InBondNumber = "111111113";
			inBondMenuItemMessageSendingObject3.PedimentoNumber = "G";
			inBondMenuItemMessageData.AllocatePredimentoNumber();
			inBondMenuItemMessageData.Factory.Save();
			AssertFENNumber(bill1, "F");
			AssertFENNumber(bill2, "E");
			AssertFENNumber(bill3, "G");
			AssertFENNumber(bill4, "D");
			var factory = new BusinessObjectFactory();
			var movementHeader = factory.Load<CusInBondMoveHeader>(movementHeader2.PK);
			AssertEquals("111111112", movementHeader.InBondNumber);
		}

		protected override BusinessObject GetNewBusinessObject() => new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);

		void AssertFENNumber(CusInBondBill bill, ZString number)
		{
			var factory = new BusinessObjectFactory();
			var newBill = factory.Load<CusInBondBill>(bill.PK);
			var fenNumbers = newBill.AdditionalReferences.Where(x => x.BR_Qualifier == ReferenceQualifierList.Codes.FEN);
			AssertEquals(1, fenNumbers.Count());
			var fenNumber = fenNumbers.First();
			AssertEquals(number, fenNumber.BR_ReferenceNum);
		}

		CusInBondMoveHeader GetInBondMoveHeader(string inBondNumber)
		{
			var newFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, inBondNumber);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return newFactory.LoadTop1<CusInBondMoveHeader>(query);
		}
	}
}
