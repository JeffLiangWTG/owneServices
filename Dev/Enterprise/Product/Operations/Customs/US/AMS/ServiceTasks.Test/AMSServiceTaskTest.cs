using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.AMS.ServiceTasks.Testing
{
	[TestedType(typeof(AMSServiceTask))]
	sealed class AMSServiceTaskTest : ServiceTaskTestCase<AMSServiceTask>
	{
		[TestDate(2025, 3, 3)]
		public void TestRunTask_UCMPSwitchOnOrOff()
		{
			var serviceTask = new AMSServiceTask();
			InitialiseTaskSchedule(serviceTask);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTaskAMS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: false))
			{
				var message = CreateMessage(GlbBranch.CurrentBranch);
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message processed", EDIMessage.Status.Received, message.EM_Status);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.UCMPServiceTaskAMS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, value: true))
			{
				var message = CreateMessage(GlbBranch.CurrentBranch);
				RunTaskSchedule(serviceTask);
				message.Reload();
				AssertEquals("Message not processed", EDIMessage.Status.Queued, message.EM_Status);
			}
		}

		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("30Seconds", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		[TestDate(2012, 06, 19)]
		public void TestSetCorrectSenderOnEmail()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = "AU";
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company2.PK;
			company2.CompanyName = "Test Company AU";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "MAX";
			staff.GS_EmailAddress = "test@test.com";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			company2.GC_OH_OrgProxy = orgHeader.PK;
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = "INB";
			header.BH_CarrierSCAC = "CARL";
			header.BH_GB = Env.CurrentBranch.PK;
			header.BH_VoyageNumber = "ST013";
			header.BH_ImportConveyanceName = "HYUNDAI SINGAPORE";
			header.BH_PortUnladingDCode = "0901";
			header.BH_ETA = new ZDateTime(2012, 06, 19);
			var headerBill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			headerBill.B0_BH = header.PK;
			headerBill.B0_IssuerCode = "CARL";
			headerBill.B0_MasterBillNumber = "CBP01307";
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			moveHeader.InBondNumber = "333210146";
			var moveHederDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveHederDetail.B9_BM = moveHeader.PK;
			moveHederDetail.B9_B0 = headerBill.PK;
			Factory.Save();
			var message = CreateMessage(branch);
			var serviceTask = new AMSServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			AssertNoExceptionThrown("We should not throw an exception when we access CurrentBranch", () =>
			{
				RunTaskSchedule(serviceTask);
			});
			message.Reload();
			AssertEquals("CusInBondMoveHeader", message.EM_LinkTable);
			AssertEquals(moveHeader.PK, message.EM_LinkUniqueID);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.FromDisplayName == company.CompanyName));
			AssertNotNull(email);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs AMS messages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.AMS,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs Stow Plan mesages inbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.StowPlan,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		EDIMessage CreateMessage(GlbBranch branch)
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Receive;
			interchange.EI_Status = "QUE";
			interchange.EI_From = "USC";
			interchange.EI_SessionGUID = new ZGuid("D1BBFCF9-5708-4AA6-9C77-C62120C805A9");
			interchange.EI_GB = branch.PK;
			interchange.EI_To = "HYEDUKCMT";
			interchange.EI_HeaderText = "ACR          RC12061920375415774                                                ";
			interchange.EI_BodyText =
				"M01CARL30ITHYUNDAI SINGAPORE      ST013     000001                              R01CARL0901HYUNDAI SINGAPORE      ST013000001120619                             J01CARL                                                                         R02CBP01307    69000000000462333210146      1206192037             1            R020901    390262200                                                            B04SNPOTT1                                                                      R03BILL ON FILE                                                                 R05NC                                                                           ";

			interchange.EI_FooterText = "ZCR          RC                   00008                                         ";
			var message = Factory.New<AMSEDIMessage>();
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			message.EM_EI = interchange.PK;
			message.EM_GB = branch.PK;
			message.EM_MessageNum = "OTT11";
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.StatusNotification;
			message.EM_MessageText = interchange.EI_InterchangeText;
			var originalMessage = Factory.New<AMSEDIMessage>();
			originalMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "OTT11";
			originalMessage.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			originalMessage.EM_SystemCreateUser = "MAX";
			Factory.Save();
			return message;
		}
	}
}
