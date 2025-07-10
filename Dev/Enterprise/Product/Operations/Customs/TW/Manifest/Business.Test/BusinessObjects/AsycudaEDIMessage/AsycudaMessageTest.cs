using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaMessage))]
	sealed class AsycudaMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestDefaultValues()
		{
			AssertEquals(MessageTypeList.Codes.FHM, message.EM_MessageType);
		}

		public void TestSystemCreateTimeUtcPlusEight()
		{
			AssertEquals(ZDateTime.Empty, message.SystemCreateTimeUtcPlusEight);
			var testCreateTimeUtc = new ZDateTime(2022, 2, 11, 03, 00, 00);
			message.EM_SystemCreateTimeUtc = testCreateTimeUtc;
			AssertEquals(testCreateTimeUtc.AddHours(8), message.SystemCreateTimeUtcPlusEight);
		}

		public void TestSystemLastEditTimeUtcPlusEight()
		{
			AssertEquals(ZDateTime.Empty, message.SystemLastEditTimeUtcPlusEight);
			var testLastEditTimeUtc = new ZDateTime(2022, 2, 12, 04, 00, 00);
			message.EM_SystemLastEditTimeUtc = testLastEditTimeUtc;
			AssertEquals(testLastEditTimeUtc.AddHours(8), message.SystemLastEditTimeUtcPlusEight);
		}

		public void TestInterchangeProperties()
		{
			AssertEquals(ZString.Empty, message.InterchangeApplicationCode);
			AssertEquals(ZString.Empty, message.InterchangeSender);
			AssertEquals(ZString.Empty, message.InterchangeReceiver);
			AssertEquals(ZString.Empty, message.InterchangeStatus);
			AssertEquals(ZString.Empty, message.InterchangeNumber);
			AssertEquals(ZDateTimeOffset.Empty, message.InterchangeDeliveredTime);
			AssertEquals(ZDateTime.Empty, message.InterchangeCreateTime);
			AssertEquals(ZDateTime.Empty, message.InterchangeCreateTimePlusEight);
			AssertEquals(ZString.Empty, message.InterchangeCreateUser);
			AssertEquals(ZDateTime.Empty, message.InterchangeLastEditTime);
			AssertEquals(ZDateTime.Empty, message.InterchangeLastEditTimePlusEight);
			AssertEquals(ZString.Empty, message.InterchangeLastEditUser);
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "TWC";
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeNum = "XXXX.ABCD";
			var testDeliveredTime = new ZDateTimeOffset(2022, 02, 10, 0, 0, 0, TimeSpan.Zero);
			interchange.EI_DeliveredTime = testDeliveredTime;
			var testCreateTimeUtc = new ZDateTime(2022, 2, 11, 01, 00, 00);
			interchange.EI_SystemCreateTimeUtc = testCreateTimeUtc;
			interchange.EI_SystemCreateUser = "~BP";
			var testLastEditTimeUtc = new ZDateTime(2022, 2, 12, 02, 00, 00);
			interchange.EI_SystemLastEditTimeUtc = testLastEditTimeUtc;
			interchange.EI_SystemLastEditUser = "JPG";
			message.EM_EI = interchange.PK;
			AssertEquals("TWC", message.InterchangeApplicationCode);
			AssertEquals("From", message.InterchangeSender);
			AssertEquals("To", message.InterchangeReceiver);
			AssertEquals(EDIInterchange.Status.Queued, message.InterchangeStatus);
			AssertEquals("XXXX.ABCD", message.InterchangeNumber);
			AssertEquals(testDeliveredTime, message.InterchangeDeliveredTime);
			AssertEquals(testCreateTimeUtc, message.InterchangeCreateTime);
			AssertEquals(testCreateTimeUtc.AddHours(8), message.InterchangeCreateTimePlusEight);
			AssertEquals("~BP", message.InterchangeCreateUser);
			AssertEquals(testLastEditTimeUtc, message.InterchangeLastEditTime);
			AssertEquals(testLastEditTimeUtc.AddHours(8), message.InterchangeLastEditTimePlusEight);
			AssertEquals("JPG", message.InterchangeLastEditUser);
		}

		public void TestEM_ApplicationCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_ApplicationCodeInfo);
			AssertEquals("Message Application Code", resourceStringData.Caption);
			AssertEquals("Msg. App. Code", resourceStringData.ShortCaption);
		}

		public void TestEM_MessageOwner_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_MessageOwnerInfo);
			AssertEquals("Message Owner", resourceStringData.Caption);
			AssertEquals("Msg. Owner", resourceStringData.ShortCaption);
		}

		public void TestEM_MessageType_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_MessageTypeInfo);
			AssertEquals("Message Type", resourceStringData.Caption);
			AssertEquals("Msg. Type", resourceStringData.ShortCaption);
		}

		public void TestEM_MessageSubType_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_MessageSubTypeInfo);
			AssertEquals("Message Sub Type", resourceStringData.Caption);
			AssertEquals("Msg. Sub Type", resourceStringData.ShortCaption);
		}

		public void TestEM_ReceiveTransmit_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_ReceiveTransmitInfo);
			AssertEquals("Message Direction", resourceStringData.Caption);
			AssertEquals("Msg. Dir", resourceStringData.ShortCaption);
		}

		public void TestEM_MessageNum_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_MessageNumInfo);
			AssertEquals("Message Number", resourceStringData.Caption);
			AssertEquals("Msg. No.", resourceStringData.ShortCaption);
		}

		public void TestEM_ApplicationReference_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_ApplicationReferenceInfo);
			AssertEquals("Application Reference", resourceStringData.Caption);
			AssertEquals("App. Ref.", resourceStringData.ShortCaption);
		}

		public void TestEM_Status_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_StatusInfo);
			AssertEquals("Message Status", resourceStringData.Caption);
			AssertEquals("Msg. Status", resourceStringData.ShortCaption);
		}

		public void TestEM_SendWithMessageErrors_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_SendWithMessageErrorsInfo);
			AssertEquals("Send with error", resourceStringData.Caption);
		}

		public void TestEM_SystemCreateTimeUtc_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_SystemCreateTimeUtcInfo);
			AssertEquals("Message Create Time (UTC)", resourceStringData.Caption);
			AssertEquals("Msg. Create Time (UTC)", resourceStringData.ShortCaption);
		}

		public void TestSystemCreateTimeUtcPlusEight_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.SystemCreateTimeUtcPlusEightInfo);
			AssertEquals("Message Create Time (UTC+8)", resourceStringData.Caption);
			AssertEquals("Msg. Create Time", resourceStringData.ShortCaption);
		}

		public void TestEM_SystemCreateUser_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_SystemCreateUserInfo);
			AssertEquals("Message Create User", resourceStringData.Caption);
			AssertEquals("Msg. Create User", resourceStringData.ShortCaption);
		}

		public void TestEM_SystemLastEditTimeUtc_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_SystemLastEditTimeUtcInfo);
			AssertEquals("Message Last Edit Time (UTC)", resourceStringData.Caption);
			AssertEquals("Msg. Last Edit Time (UTC)", resourceStringData.ShortCaption);
		}

		public void TestSystemLastEditTimeUtcPlusEight_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.SystemLastEditTimeUtcPlusEightInfo);
			AssertEquals("Message Last Edit Time (UTC+8)", resourceStringData.Caption);
			AssertEquals("Msg. Last Edit Time", resourceStringData.ShortCaption);
		}

		public void TestEM_SystemLastEditUser_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.EM_SystemLastEditUserInfo);
			AssertEquals("Message Last Edit User", resourceStringData.Caption);
			AssertEquals("Msg. Last Edit User", resourceStringData.ShortCaption);
		}

		public void TestInterchangeApplicationCode_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeApplicationCodeInfo);
			AssertEquals("Interchange Application Code", resourceStringData.Caption);
			AssertEquals("Int. App. Code", resourceStringData.ShortCaption);
		}

		public void TestInterchangeSender_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeSenderInfo);
			AssertEquals("From", resourceStringData.Caption);
		}

		public void TestInterchangeReceiver_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeReceiverInfo);
			AssertEquals("To", resourceStringData.Caption);
		}

		public void TestInterchangeStatus_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeStatusInfo);
			AssertEquals("Interchange Status", resourceStringData.Caption);
			AssertEquals("Int. Status", resourceStringData.ShortCaption);
		}

		public void TestInterchangeNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeNumberInfo);
			AssertEquals("Interchange Number", resourceStringData.Caption);
			AssertEquals("Int. No.", resourceStringData.ShortCaption);
		}

		public void TestInterchangeDeliveredTime_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeDeliveredTimeInfo);
			AssertEquals("Delivered Time", resourceStringData.Caption);
		}

		public void TestInterchangeCreateTime_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeCreateTimeInfo);
			AssertEquals("Interchange Create Time (UTC)", resourceStringData.Caption);
			AssertEquals("Int. Create Time (UTC)", resourceStringData.ShortCaption);
		}

		public void TestInterchangeCreateTimePlusEight_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeCreateTimePlusEightInfo);
			AssertEquals("Interchange Create Time (UTC+8)", resourceStringData.Caption);
			AssertEquals("Int. Create Time", resourceStringData.ShortCaption);
		}

		public void TestInterchangeCreateUser_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeCreateUserInfo);
			AssertEquals("Interchange Create User", resourceStringData.Caption);
			AssertEquals("Int. Create User", resourceStringData.ShortCaption);
		}

		public void TestInterchangeLastEditTime_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeLastEditTimeInfo);
			AssertEquals("Interchange Last Edit Time (UTC)", resourceStringData.Caption);
			AssertEquals("Int. Last Edit Time (UTC)", resourceStringData.ShortCaption);
		}

		public void TestInterchangeLastEditTimePlusEight_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeLastEditTimePlusEightInfo);
			AssertEquals("Interchange Last Edit Time (UTC+8)", resourceStringData.Caption);
			AssertEquals("Int. Last Edit Time", resourceStringData.ShortCaption);
		}

		public void TestInterchangeLastEditUser_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(message.InterchangeLastEditUserInfo);
			AssertEquals("Interchange Last Edit User", resourceStringData.Caption);
			AssertEquals("Int. Last Edit User", resourceStringData.ShortCaption);
		}

		public void TestGetMessageReferenceNumber()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				CombineAssertions(() =>
				{
					Factory.Save();
					AssertEquals("0000000001", message.EM_MessageNum);

					var message2 = Factory.New<AsycudaMessage>();
					Factory.Save();
					AssertEquals("0000000002", message2.EM_MessageNum);

					var expectedNumberFountain = Env.NumberFountains.GetOutgoingTWCustomsMessageNumber();
					expectedNumberFountain.SetNext(Factory, 13);

					var message3 = Factory.New<AsycudaMessage>();
					Factory.Save();
					AssertEquals("0000000013", message3.EM_MessageNum);
					var message4 = Factory.New<AsycudaMessage>();
					Factory.Save();
					AssertEquals("0000000014", message4.EM_MessageNum);
				});
			}
		}

		[TestDate(2022, 06, 27)]
		public void TestGetNumberFountainNumbersAndFillInPlaceHolders()
		{
			var orgCusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.FillWithValidTestData();
			orgCusCode.OK_CustomsRegNo = "52889317";
			orgCusCode.OK_CodeType = "VAT";
			orgCusCode.OK_RN_NKCodeCountry = "TW";
			var message = Factory.New<AsycudaMessage>();
			message.EM_MessageText = MessageConstants.FunctionalReferenceIDPlaceHolderHtml;
			Factory.Save();
			var currentCompanyOrgProxyTWVATOrgCusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == "VAT" && x.OK_RN_NKCodeCountry == "TW");
			CombineAssertions(() =>
			{
				AssertNotNull(currentCompanyOrgProxyTWVATOrgCusCode);
				AssertContains("5288931722JUN27", message.EM_MessageText);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<AsycudaMessage>();
		}
		AsycudaMessage message;
	}
}
