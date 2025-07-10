using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.US.AMS.ServiceTasks.Testing
{
	[TestedType(typeof(AMSMessageSenderServiceTask))]
	sealed class AMSMessageSenderServiceTaskTest : ServiceTaskTestCase<AMSMessageSenderServiceTask>
	{
		public void TestCanRunInAnyBranch()
		{
			var hostedServiceAttribute = GetHostedServiceAttributes().Single();
			Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
		}

		public void TestServiceTaskRuningCompanies()
		{
			var message = Factory.New<StowPlanMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = @"XYZ123".PadRight(80);
			message.EM_MessageType = "MR";
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
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
			Factory.Save();
			var serviceTask = new AMSMessageSenderServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertNotContains("have been processed", log.ToString());
			message = Factory.New<StowPlanMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			serviceTask = new AMSMessageSenderServiceTask();
			log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertContains("have been processed", log.ToString());
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			interchange.EI_InterchangeType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			interchange.EI_Status = CBPEDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Transmit;
			interchange.EI_InterchangeNum = "1";
			interchange.EI_HeaderText =
				"ACR8CAR      MR11032301200500101                                                ";
			interchange.EI_BodyText =
				"M01OTT111USTOWER BRIDGE           451  00001000007 8505989                      " +
				"M02123456789012_79                                                              " +
				"P01270403261100002    0001                                                      " +
				"W02OTT11103230120050100100001000000000000000000000000100017                     ";
			interchange.EI_FooterText =
				"ZCR8CAR      MR                   00004";

			Factory.Save();
			serviceTask = new AMSMessageSenderServiceTask();
			log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertNotContains("Interchange '1' has been processed successfully.", log.ToString());
		}

		public void TestSendingSTWMessage()
		{
			var message = Factory.New<StowPlanMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var serviceTask = new AMSMessageSenderServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			AssertContains("have been processed", log.ToString());
		}

		public void TestDoNotSendIfSCACIsMissing()
		{
			var message = Factory.New<StowPlanMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, ZString.Empty, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			var serviceTask = new AMSMessageSenderServiceTask();
			var log = InitialiseTaskSchedule(serviceTask);
			AssertNoExceptionThrown("We should not throw an exception when we access CurrentBranch", () =>
			{
				RunTaskSchedule(serviceTask);
			});
			AssertContains(@"Message 1 will be discarded for the following reason:\r\nThere is no Customs Interchange Sender ID set up, please configure a Carrier Code (CCC) for Organization Proxy (EDICUS) > Details > Config > Registration Numbers/Code.", log.ToString());
			message.Reload();
			AssertEquals("message.EM_Status", EDIMessage.Status.Discarded, message.EM_Status);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs AMS messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.AMS,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),

					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"US Customs Stow Plan messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.StowPlan,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			var companies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			Array.ForEach(companies, (GlbCompany company) =>
			{
				company.GC_IsActive = false;
			});
			Factory.Save();
		}
	}
}
