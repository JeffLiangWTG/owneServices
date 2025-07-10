using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	[DisableWorkflowSettingPropertiesAfterOnSaving]
	public class DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving : DummyWithWorkflow
	{
		public DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override void OnSaving()
		{
			base.OnSaving();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, "Test log");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		protected override string GetWorkflowType() => "DUA";
	}
}
