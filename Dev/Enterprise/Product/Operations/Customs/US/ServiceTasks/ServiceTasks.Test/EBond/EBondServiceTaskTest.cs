using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using EDIMessage = Enterprise.Customs.US.Business.EDIMessage;

namespace Enterprise.Customs.US.ServiceTasks.Testing
{
	[TestedType(typeof(EBondServiceTask))]
	sealed class EBondServiceTaskTest : ServiceTaskTestCase<EBondServiceTask>
	{
		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1Minute", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestHostedServiceAttributeParameters()
		{
			AssertSingleHostedServiceAttribute("UXB", "United States eBond Customs Messaging", "USC");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 01, 01)]
		public void TestRunTask()
		{
			var groupZZ1 = Factory.New<GlbGroup>();
			groupZZ1.GG_Code = "ZZ1";
			var staffZ1 = groupZZ1.Staff.AddNew();
			staffZ1.GS_Code = "Z1";
			staffZ1.GS_LoginName = "z1";
			staffZ1.GS_EmailAddress = "jason@test.email.com";
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupZZ1.PK.ToGuid());
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = EBondMssageProcessorFactoryTest.CreateDeclarationForEBondMessage(Factory);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.SubstitutionBond;
			declaration.US_InsuranceDisposition = InsuranceDispositionCodeList.Codes.BondRejectedByCBPSeeAttachedErrors;
			var interchange = EBondMssageProcessorFactoryTest.CreateInterchangeForEBondMessage(Factory);
			Factory.Save();
			var serviceTask = new EBondServiceTask();
			InitialiseTaskSchedule(serviceTask);
			RunTaskSchedule(serviceTask);
			CombineAssertions(() =>
			{
				var message = interchange.ContainedMessages[0];
				AssertMultilineASCIIEquals("Should process the target interchange.", string.Format(@"
Information|	Interchange 'EBond190319054146' has been processed successfully.
Information|	Processing Message #{0}
Information|	Match Declaration B00001398 from the message {0}.
Information|	Update bond data on Declaration B00001398 from the message {0}.
Information|	Create email to group BondStatusNotificationGroup from the message {0}.
Information|	Saving...
Information|	1 message processed", message.EM_MessageNum), serviceTask.ServiceLogger.ToString());
				declaration.ReloadSafe();
				AssertEquals("Should update EM_Status.", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals("Should update US_BondDesignationCode.", BondDesignationCodeList.Codes.BasicBond, declaration.US_BondDesignationCode);
				AssertEquals("Should update US_InsuranceDisposition.", InsuranceDispositionCodeList.Codes.AcceptedByCBP, declaration.US_InsuranceDisposition);
			});
		}

		public void TestCompanyBranchBecomeInactiveDuringProcessing() => new USServiceTaskTestCommon().AssertCompanyBranchBecomeInactiveDuringProcessing<EBondServiceTask>();

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"US Customs eBond interchanges inbound",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.USeBond),
				};
			}
		}
	}
}
