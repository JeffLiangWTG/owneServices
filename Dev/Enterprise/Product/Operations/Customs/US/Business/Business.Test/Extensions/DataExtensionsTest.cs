using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DataExtensionsTest : TestCase
	{
		public void TestHasApprovedTIBExtension()
		{
			var factory = new BusinessObjectFactory();
			var message = factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "~15000";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.TIBExtensionOrClosureResponse;
			message.EM_MessageText =
"B018888XJ5TX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: XJ5 23456789                                           " +
"E1RF995 EXT GRANTED SUBJECT TO REVIEW             XJ5  23456789     B00000001   " +
"Y  8888XJ5X100007";
			message.EM_Status = MQEDIMessage.Status.Queued;
			Assert(message.HasApprovedTIBExtension());

			message.EM_MessageText =
"B018888XJ5TX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: XJ5 23456789                                           " +
"E1RF998 TRANSACTION DATA REJECTED                 XJ5  23456789     B00000001   " +
"Y  8888XJ5X100007";

			Assert(!message.HasApprovedTIBExtension());
		}

		public void TestShouldTrimSCACFromBills()
		{
			var billNumberNotValidSCAC = (ZString)"SSSSBN001";
			var billNumberValidSCAC = (ZString)"SCACBN001";
			var factory = new BusinessObjectFactory();
			var carrier = factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = factory.New<USCarrierCombined>();
			carrier2.UI_Code = "SSSS";
			carrier2.UI_ModeOfTransportation = "20";
			Assert("Should NOT trim SCAC from the bill number", !billNumberNotValidSCAC.ShouldTrimSCACFromBills(new List<ZString> { "SCAC" }));
			Assert("Should trim SCAC from the bill number", billNumberValidSCAC.ShouldTrimSCACFromBills(new List<ZString> { "SCAC" }));
		}
	}
}
