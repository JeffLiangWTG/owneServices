using System;
using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(StatementWorkflowDescriptor))]
	sealed class StatementWorkflowDescriptorTest : WorkflowDescriptorTestCase<StatementWorkflowDescriptor>
	{
		#region ID / Description

		public override void TestID()
		{
			AssertEquals("Correct Code", "STM", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Description", "Statement", WorkflowDescriptor.Description);
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals(string.Empty, WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		public void TestControllerId()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AssertEquals("ControllerID In CA.", ControllerIDs.Customs.CustomsStatement, WorkflowDescriptor.ControllerID);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("ControllerID In US.", ControllerIDs.Customs.CustomsStatement, WorkflowDescriptor.ControllerID);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("ControllerID In AU.", ControllerIDs.Customs.CustomsStatement, WorkflowDescriptor.ControllerID);
			}
		}

		#endregion

		public override void TestSubTypes()
		{
			Assert(true);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				GetStatement(),
			};
		}

		BaseCusStatementHeader GetStatement()
		{
			var statement = Factory.New<BaseCusStatementHeader>();
			statement.B2_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;

			return statement;
		}

		#region Workflow Triggers

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					CusStatementHeaderSchema.B2_ProcessDate,
					CusStatementHeaderSchema.B2_ProcessPort,
					CusStatementHeaderSchema.B2_PaymentType,
					CusStatementHeaderSchema.B2_PrintDate,
					CusStatementHeaderSchema.B2_DueDate,
					CusStatementHeaderSchema.B2_PaymentAuthorizationDate,
					CusStatementHeaderSchema.B2_Status
				};
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.OrgProxy;
			}
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CustomsStatementHdr, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		#endregion

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			var statement = (BaseCusStatementHeader)workflowProvider;
			var mode = statement.Importer.EDICommunicationsModes.AddNew();
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			mode.EK_Module = "STM";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";
		}

		protected override OrgHeader[] GetExpectedOrganisationsForPartyType(ProcessTask task, MessageRecipientPartyType partyType)
		{
			var statement = (BaseCusStatementHeader)task.Parent;
			partyType = partyType & ~MessageRecipientPartyType.Consignee;
			return base.GetExpectedOrganisationsForPartyType(task, partyType);
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeCountry;

		protected override void SetUp()
		{
			base.SetUp();
			disposable = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
		}

		protected override void TearDown()
		{
			disposable.Dispose();
			base.TearDown();
		}
		IDisposable disposable;
	}
}
