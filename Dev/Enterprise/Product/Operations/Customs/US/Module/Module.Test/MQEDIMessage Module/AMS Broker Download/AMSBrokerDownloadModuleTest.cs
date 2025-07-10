using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(AMSBrokerDownloadModule))]
	sealed class AMSBrokerDownloadModuleTest : MQEDIMessageModuleTest
	{
		public void TestFilterControl()
		{
			using (var module = new AMSBrokerDownloadModule())
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					var control = (MQEDIMessageFilterControl)module.EmbeddedControl;
					form.Controls.Add(control);
					form.Show();
					Assert(control.FilteredGrid.Columns.Contains(MQEDIMessage.Schema.EM_ActionStatus));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SendOrReceiveHumanReadable));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_ApplicationCode));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_ApplicationReference));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageNum));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageType));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageSubType));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_MessageSubTypeDescription));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_User));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeSender));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeReceiver));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SendingUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemCreateUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_DateTimeInterchangeSent));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeNumber));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_InterchangeStatus));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemLastEditUser));
					Assert(!control.FilteredGrid.Columns.Contains(EDIMessage.Schema.EM_SystemLastEditTimeUtc));
				}
			}
		}

		public override void TestSetToComplete()
		{
			AssertSetToCompleteMenuItem(ApplicationIdentifierCodeList.Codes.BrokerManifestDownload);
		}

		public void TestEmbeddedControl()
		{
			using (var module = (MQEDIMessageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("EmbeddedControl.GetType()", typeof(AMSBrokerDownloadMessageFilterControl), module.EmbeddedControl.GetType());
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = new AMSBrokerDownloadModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.AMSBrokerDownloadMessages, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.AMSBrokerDownloadMessage;
	}
}
