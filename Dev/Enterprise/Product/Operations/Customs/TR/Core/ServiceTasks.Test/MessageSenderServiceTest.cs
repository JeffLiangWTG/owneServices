using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.TR.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderService))]
	class MessageSenderServiceTest : ServiceTaskTestCase<MessageSenderService>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "TRS", hostedServiceAttribute.Code);
				AssertEquals("Description", "TR Customs Message Sender", hostedServiceAttribute.Description);
				AssertEquals("Category", "TRC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1Minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", Core.Constants.CountryCodes.Turkey, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(MessageSenderService).GetMethod(nameof(MessageSenderService.IsRequired));
			Assert("HostedServiceRequirement is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			CertificateRequirementChecker.ResetForTesting();
			AssertEquals("There is no Certificate configured in Turkey.", MessageSenderService.IsRequired());
			GlbExternalPasswordHelperTest.SetupGlbExternalPassword_TRK(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, MessageSenderService.IsRequired());
		}

		[TestDate(2020, 01, 03, 15, 36, 0)]
		public void TestRunTask()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var manifest = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
				var message = Factory.New<TRManifestMessage>();
				message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
				message.EM_Status = Enterprise.Messaging.Business.EDIMessage.Status.Queued;
				message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TRCustoms;
				message.EM_MessageType = TRMessageTypes.Codes.TRO;
				message.EM_LinkedObject = (BusinessObject)manifest;
				message.EM_GB = GlbBranch.CurrentBranch.PK;
				message.EM_MessageOwner = "CZH";
				message.EM_IsTestMessage = true;
				message.EM_MessageText = "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>";
				message.EM_ApplicationReference = "RefID1";
				Factory.Save();

				var task = new MessageSenderService();
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				CombineAssertions(() =>
				{
					var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
					AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);

					var interchange = interchangesCreated[0];
					AssertEquals("EI_ApplicationCode", "TRC", interchange.EI_ApplicationCode);
					AssertEquals("EI_InterchangeNum", "00000000000001", interchange.EI_InterchangeNum);
					AssertEquals("EI_InterchangeType", "TRO", interchange.EI_InterchangeType);
					AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
					AssertEquals("EI_To", "TROCustomsTest", interchange.EI_To);
					AssertEquals("EI_From ", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
					AssertEquals("EI_BodyText", "<Message><OtherInfo><Code>AAA+111+BBB+222</Code></OtherInfo></Message>", interchange.EI_BodyText);
					AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
					AssertEquals("EI_HeaderText", string.Empty, interchange.EI_HeaderText);

					message.Reload();
					AssertEquals("EM_EI", interchange.PK, message.EM_EI);
					AssertEquals("EM_Status", EDIMessage.Status.Sent, message.EM_Status);
				});
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.TRS,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.TRCustoms,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
