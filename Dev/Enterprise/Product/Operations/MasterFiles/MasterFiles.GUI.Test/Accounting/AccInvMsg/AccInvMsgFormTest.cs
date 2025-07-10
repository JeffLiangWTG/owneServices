using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccInvMsgForm))]
	public class AccInvMsgFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccInvMsg sequence = Factory.New<AccInvMsg>();
			return new AccInvMsgForm(sequence);
		}

		#region Test TAX Group Code Dropdown component visible when there is an override of Tax Message Groups Management registry key

		public void TestTAXGroupCodeVisibleWhenRegistryOverride()
		{
			using (var form = (AccInvMsgForm)GetFormToBashCore())
			{
				var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
				testValue.Add("N11", (NoResString)"Description N1.1", true, "N1.1");
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testValue);

				form.Show();
				Application.DoEvents(); // needed for binding to occur

				var taxGroup = form.A9_TaxGroupZDropEdit;
				Assert("TAX Group Code Component shown up", taxGroup.Visible);
				var govtCode = form.GovtCodeText;
				Assert("TAX Group Govt. Code shown up", govtCode.Visible);
				Assert("TAX Group Govt. Code read-only", govtCode.ReadOnly);

				taxGroup.ShowDropDown(); //force loading values
				taxGroup.Select(); //avoid exception on SelectItem() below
				taxGroup.SelectItem("N11");
				taxGroup.CommitBoundValue();
				Application.DoEvents();

				AssertEquals("TAX Group Govt. Code value", "N1.1", form.GovtCodeText.Text);
			}
		}

		#endregion

		#region Test TAX Group Code Dropdown component not visible when there is not an override of Tax Message Groups Management registry key

		public void TestTAXGroupCodeNotVisibleWhenRegistryOverride()
		{
			using (var form = (AccInvMsgForm)GetFormToBashCore())
			{
				var testValue = new Registry.Business.CodeDescriptionBoolRelatedItemCollection();
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, testValue);
				form.Show();
				Assert("TAX Group Code Component NOT shown up", !form.A9_TaxGroupZDropEdit.Visible);

				Assert("TAX Group Govt. Code NOT shown up", !form.GovtCodeText.Visible);
			}
		}

		#endregion

		public void TestFormHasAuditPlugin()
		{
			using (var form = (AccInvMsgForm)GetFormToBash())
			{
				Assert($"{form.Name} should have Audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
