using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(GlbGroupForm))]
	class ADEnabledGlbGroupFormBasherTest : ZFormBasherTest
	{
		public void TestADFieldsForNonSystemDefinedGroup()
		{
			using (var form = SetupForm(forSystemDefinedGroup: false, setupADConfig: true))
			{
				form.Show();
				var checkBox = form.FindSingle<ZCheckBox>("ADLinkedCheckBox");
				var dropEdit = form.FindSingle<ZDropEdit>("DomainNameDropEdit");
				AssertEquals(true, checkBox.Visible);
				AssertEquals(true, dropEdit.Visible);
			}
		}

		public void TestADFieldsForSystemDefinedGroup()
		{
			using (var form = SetupForm(forSystemDefinedGroup: true, setupADConfig: true))
			{
				form.Show();
				var tabPage = form.FindSingleOrDefault<ZTabPage>(p => p.Text == "Active Directory");
				AssertNull(tabPage);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return SetupForm(forSystemDefinedGroup: false, setupADConfig: true);
		}

		GlbGroupForm SetupForm(bool forSystemDefinedGroup, bool setupADConfig)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_IsSystemDefined = forSystemDefinedGroup;

			if (setupADConfig)
			{
				var adRegistry = ObjectFactory.Get<IADRegistry>();
				adRegistry.IsIntegrationEnabled = true;
				var domain = ObjectFactory.Get<IDomainCredentials>();
				domain.DomainName = "domain1";
				adRegistry.DomainCredentialsCollection = new[] { domain };
			}

			return new GlbGroupForm(group) { ControllerID = ControllerIDs.GlbGroup };
		}
	}
}
