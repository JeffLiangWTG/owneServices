using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	public abstract class WorkflowDescriptorNonTransactionedTestCase<T> : TestCase
			where T : WorkflowDescriptor, new()
	{
		protected override void SetUp()
		{
			base.SetUp();

			Factory = new BusinessObjectFactory();
		}

		protected BusinessObjectFactory Factory { get; private set; }

		protected DbConnection TestConnection => Db.Connection;

		public void TestUniversalActivityTriggerAction_ShouldCreateEDIMessages()
		{
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");
			var descriptor = new T();

			if (descriptor.SupportsWorkflowTriggerActionUniversalActivityXML)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var mode = org.EDICommunicationsModes.AddNew();
				mode.EK_Module = descriptor.Code;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				mode.EK_Destination = "Photography Raptor";
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.All;

				GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy = org.PK;

				var job = GetJobForTest();
				var trigger = job.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Start Work";
				trigger.ReferenceCode = "REF";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent69Code;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				trigger.Parent.Logs.AddNew(Events.CustomisableEvent69);

				Factory.Save();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertContainsExactElementsInAnyOrder(Array.Empty<ZString>(), messages.Select(x => x.EM_MessageSubType));

				MasterFilesTestHelper.RunLogWalker();

				var newFactory = new BusinessObjectFactory();
				messages = newFactory.Load<IEDIMessage>(new ZQuery());
				AssertContainsExactElementsInAnyOrder(new[] { EDIMessageSubTypeList.Codes.XmlUniversalActivity }, messages.Select(x => x.EM_MessageSubType.ToString()));

				var message = messages.Single();
				AssertStartsWith("The message should include XML for a UniversalActivity. SAD!", "<UniversalActivity", message.EM_MessageText);
				AssertEquals(EDIMessageStatusList.Codes.Sent, message.EM_Status);
			}
			else
			{
				Assert(true);
			}
		}

		protected abstract IWorkflowProvider GetJobForTest();
	}
}
