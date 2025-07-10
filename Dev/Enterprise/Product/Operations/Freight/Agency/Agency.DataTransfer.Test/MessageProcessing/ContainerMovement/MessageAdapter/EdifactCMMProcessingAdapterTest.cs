using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class EdifactCMMProcessingAdapterTest : CMMProcessingAdapterTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			CustomsCode = Sender.CustomsCodes.AddNew();
			CustomsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			CustomsCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			CustomsCode.OK_CustomsRegNo = "SHCB1";
		}

		public override void TestAttachMessage()
		{
			var originalMessage = EDIMessageTestFactory.New(Factory);
			originalMessage.EM_MessageText = "Test Message Text";
			var adapter = new EdifactCMMProcessingAdapter(originalMessage);
			var containerMovement = Factory.New<Business.ContainerMovement>();
			AssertEquals("Precondition - No container movement messages", 0, containerMovement.Messages.Count);
			adapter.AttachMessage(containerMovement, EDIMessage.Status.Recognised);
			AssertEquals("Correct message count", 1, containerMovement.Messages.Count);
			AssertEquals("Correct message status", EDIMessage.Status.Recognised, containerMovement.Messages[0].EM_Status);
			AssertEquals("Correct message content", "Test Message Text", containerMovement.Messages[0].EM_MessageText);
		}

		public void TestMultipleMatchingVoyageVessel_ContainersHaveConflictingBillOfLadingVoyages()
		{
			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			var newVoyage1 = GetVoyageWithDuplicateDetails();
			var billofLading1 = Factory.New<BillOfLading>();
			billofLading1.JS_JX = newVoyage1.Sailings[0].PK;
			var container1 = billofLading1.RealContainers.AddNew();
			container1.JC_ContainerNum = "CCLU6959308";
			container1.JC_RC = containerType;
			var newVoyage2 = GetVoyageWithDuplicateDetails();
			var billofLading2 = Factory.New<BillOfLading>();
			billofLading2.JS_JX = newVoyage2.Sailings[0].PK;
			var container2 = billofLading2.RealContainers.AddNew();
			container2.JC_ContainerNum = "CCLU4214629";
			container2.JC_RC = containerType;
			Factory.Save();
			#region Message
			string messageWithMultipleContainers = @"UNH+00000000123660+CODECO:D:95B:UN:ITIG12'
BGM+34+INGATE EMPTY+9'
RFF+BN:E3067258'
TDT+20+123+1++CSH:172:184+++9146704:146'
LOC+11+AUSYD'
NAD+MS+SHCB1:160:184'
NAD+CF+CSH:160:184'
EQD+CN+CCLU6959308+45G0:102:5++8+4'
DTM+181:201501231230:203'
LOC+11+AUSYD'
EQD+CN+CCLU4214629+45G0:102:5++8+4'
DTM+181:201501231234:203'
CNT+16:2'";
			#endregion
			var message = CreateIncomingMessage(messageWithMultipleContainers.Replace(System.Environment.NewLine, ""));
			var adapter = new EdifactCMMProcessingAdapter(message);
			adapter.Load();
			AssertNull("As the containers in the message match two separate bills of lading belonging to different sailings, voyage cannot be determined", adapter.Voyage);
		}

		JobVoyage GetVoyageWithDuplicateDetails()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123";
			voyage.JV_RV_NKVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, "9146704")).RV_FK;
			voyage.JV_OH_Line = Factory.NewWithValidTestData<OrgHeader>().PK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			voyage.GenerateSailings();
			return voyage;
		}

		public void TestSenderCodeTypeMapping_PrefilledMessage()
		{
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertEquals("MessageSenderCodeType", CMMOrganisationType.OneStop, adapter.MessageSenderCodeType);
		}

		public override void TestCountryCode_OrgHeader()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + // footer
			"CNT+16:1'" + "UNT+8+1'" + "";
			Sender.OH_RL_NKClosestPort = "NZAKL";
			Factory.Save();
			var adapter = new EdifactCMMProcessingAdapter(CreateIncomingMessage(messageText));
			adapter.Load();
			AssertEquals("NZ", adapter.CountryCode);
			AssertEquals("NZAKL", adapter.PortCode);
		}

		public override void TestCountryCode_Address()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);
			const string messageText = // headder
			"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container
			"EQD+CN+CCLU4214635+42R0:102:5+++5'" + "DTM+7:200806271010:203'" + // footer
			"CNT+16:1'" + "UNT+8+1'" + "";
			Sender.OH_RL_NKClosestPort = "NZAKL";
			Sender.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();
			var adapter = new EdifactCMMProcessingAdapter(CreateIncomingMessage(messageText));
			adapter.Load();
			AssertEquals("AU", adapter.CountryCode);
			AssertEquals("AUBNE", adapter.PortCode);
		}

		public override void TestGarbageMessage()
		{
			try
			{
				NewAdapterWithGarbageMessage().Load();
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				AssertEquals("Enterprise.Edifact.InvalidFormatException", ex.GetType().FullName);
				AssertEquals("Corrupt or Malformed D95B CODECO or COARRI message. Cannot Process.", ex.Message);
			}
		}

		public override void TestCountryOfCustomsCodeIsNotAu()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);

			CustomsCode.OK_CodeType = OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode;
			CustomsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;

			const string messageText =
			"UNH+719+COARRI:D:95B:UN:ITG10'" + "BGM+270+13910566+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:ZZZ'" + "NAD+CF+CSH:160:184'" +
			"EQD+CN+CCLU4214635+42R0:102:5+2++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" +
			"CNT+16:1'" + "UNT+13+1'" + "";

			var adapter = new EdifactCMMProcessingAdapter(CreateIncomingMessage(messageText));
			AssertNoExceptionThrown(() => adapter.Load());
		}

		public override ICMMProcessingAdapter NewAdapterWithPrefilledMessage()
		{
			return new EdifactCMMProcessingAdapter(PrefilledMessage);
		}

		public override ICMMProcessingAdapter NewAdapterWithEmptyMessage()
		{
			return new EdifactCMMProcessingAdapter(EmptyMessage);
		}

		public override ICMMProcessingAdapter NewAdapterWithGarbageMessage()
		{
			return new EdifactCMMProcessingAdapter(GarbageMessage);
		}

		public override ICMMProcessingAdapter NewAdapterWithGateInMessage()
		{
			return new EdifactCMMProcessingAdapter(GateInMessage);
		}

		public override ICMMProcessingAdapter NewAdapterWithGateOutMessage()
		{
			return new EdifactCMMProcessingAdapter(GateOutMessage);
		}

		public override ICMMProcessingAdapter NewAdapterWithContainerLoadMessage()
		{
			return new EdifactCMMProcessingAdapter(ContainerLoadMessage);
		}

		public override ICMMProcessingAdapter NewAdapterWithContainerDischargeMessage()
		{
			return new EdifactCMMProcessingAdapter(ContainerDischargeMessage);
		}

		#region implementation
		EDIMessage EmptyMessage
		{
			get
			{
				return CreateIncomingMessage("");
			}
		}

		EDIMessage GarbageMessage
		{
			get
			{
				const string message = "Do you feel inadiquite?\r\n" + "Have you tried the little blue pill and failed?\r\n" + "Well now there is a little green pill for thoes who want to colour coordinate.\r\n" + "";
				return CreateIncomingMessage(message);
			}
		}

		EDIMessage PrefilledMessage
		{
			get
			{
				#region MessageText
				const string message = // header
				"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container 1
				"EQD+CN+CCLU4214635+42R0:102:5+2++5'" + "RFF+BM:BillOfLading'" + "RFF+BN:BookingReference'" + "RFF+AAE:GoodsDeclarationNumber'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "MEA+AAE+G+KGM:6800'" + "SEL+123456'" + "SEL+654321'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
				"CNT+16:1'" + "UNT+13+1'" + "";
				#endregion
				return CreateIncomingMessage(message);
			}
		}

		EDIMessage GateInMessage
		{
			get
			{
				#region MessageText
				const string messageText = // header
				"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+34+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container 1
				"EQD+CN+CCLU4214635+42R0:102:5+2++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
				"CNT+16:1'" + "UNT+13+1'" + "";
				#endregion
				return CreateIncomingMessage(messageText);
			}
		}

		EDIMessage GateOutMessage
		{
			get
			{
				#region MessageText
				const string messageText = // header
				"UNH+1+CODECO:D:95B:UN:ITG10'" + "BGM+36+PPA8818855-1+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container 1
				"EQD+CN+CCLU4214635+42R0:102:5+2++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
				"CNT+16:1'" + "UNT+13+1'" + "";
				#endregion
				return CreateIncomingMessage(messageText);
			}
		}

		EDIMessage ContainerLoadMessage
		{
			get
			{
				#region MessageText
				const string messageText = // header
				"UNH+719+COARRI:D:95B:UN:ITG10'" + "BGM+270+13910566+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container 1
				"EQD+CN+CCLU4214635+42R0:102:5+2++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
				"CNT+16:1'" + "UNT+13+1'" + "";
				#endregion
				return CreateIncomingMessage(messageText);
			}
		}

		EDIMessage ContainerDischargeMessage
		{
			get
			{
				#region MessageText
				const string messageText = // header
				"UNH+718+COARRI:D:95B:UN:ITG10'" + "BGM+98+13910559+9'" + "TDT+20+0033+1++CSH:172:184+++9290127:146'" + "NAD+MS+SHCB1:160:184'" + "NAD+CF+CSH:160:184'" + // container 1
				"EQD+CN+CCLU4214635+42R0:102:5+2++4'" + "DTM+7:200806271010:203'" + "LOC+165+AUSYD:139:6+SHCB1'" + "LOC+8+AUSYD'" + "FTX+DAR++2::184'" + "FTX+ABS++68::184'" + // footer
				"CNT+16:1'" + "UNT+13+1'" + "";
				#endregion
				return CreateIncomingMessage(messageText);
			}
		}

		ContainerManagementEDIMessage CreateIncomingMessage(string messageText)
		{
			var message = Factory.New<ContainerManagementEDIMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.ContainerManagement;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageText = messageText;
			return message;
		}
		#endregion
	}
}
