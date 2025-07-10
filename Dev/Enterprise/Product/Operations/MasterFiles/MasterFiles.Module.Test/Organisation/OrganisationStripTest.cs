using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrganisationStripTest : TestCaseWithFactory
	{
		public void TestHandlesModuleOrgForwarderFilter()
		{
			AssertProvidesControlsFor(new OrgForwarderModuleFilter("Forwarder Blah"));
		}

		public void TestHandlesModuleOrgReceivablesFilter()
		{
			AssertProvidesControlsFor(new OrgReceivablesModuleFilter("Receivables Blah"));
		}

		public void TestHandlesModuleOrgCodeMappingForeignFilter()
		{
			AssertProvidesControlsFor(new OrgCodeMappingForeignModuleFilter("Code Mapping (Foreign Code) Blah"));
		}

		public void TestHandlesModuleOrgCodeMappingLocalFilter()
		{
			AssertProvidesControlsFor(new OrgCodeMappingLocalModuleFilter("Code Mapping (Local Code) Blah"));
		}

		public void TestHandlesModuleOrgTypeModuleFilter()
		{
			AssertProvidesControlsFor(new OrgTypeModuleFilter("OrgType Blah"));
		}

		public void TestHandlesRelatedOrgTypeModuleFilter()
		{
			AssertProvidesControlsFor(new OrgRelatedPartiesModuleFilter("When We Fly"));
		}

		public void TestHandlesOrgAddressActiveStatusAndInfoModuleFilter()
		{
			using (var strip = new OrganisationFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, new OrgAddressWithActiveStatusModuleTextFilter("Blah Blah Blah"));
				AssertEquals(1, controls.Length);
				Assert(controls[0] is OrgAddressActiveStatusAndInfoFilterControl);

				foreach (var control in controls)
				{
					control.Dispose();
				}
			}
		}

		public void TestHandlesOrgContactActiveStatusAndInfoModuleFilter()
		{
			using (var strip = new OrganisationFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, new OrgContactsActiveStatusAndInfoModuleFilter("Blah Blah Blah"));
				AssertEquals(1, controls.Length);
				Assert(controls[0] is OrgContactsActiveStatusAndInfoFilterControl);

				foreach (var control in controls)
				{
					control.Dispose();
				}
			}
		}

		public void TestHandlesOrgLocatedWithinModuleFilter()
		{
			AssertProvidesControlsFor(new OrgLocatedWithinModuleFilter("OrgLocatedWithin Blah", Factory));
		}

		public void TestHandlesOrgTaxConfigurationModuleFilter()
		{
			AssertProvidesControlsFor(new OrgTaxConfigurationModuleFilter("OrgTaxConfigurationModuleFilter"));
		}

		public void TestCorrectControlForRelatedOrg()
		{
			using (OrganisationFilterStrip stripter = new OrganisationFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(stripter, new OrgRelatedPartiesModuleFilter("Sad Statue"));
				bool found = false;
				foreach (Control ctr in controls)
				{
					if (ctr.GetType() == typeof(OrgRelatedPartiesFilterControl))
					{
						found = true;
						break;
					}
				}
				foreach (Control ctr in controls)
				{
					ctr.Dispose();
				}
				AssertEquals("should return correct control", true, found);
			}
		}

		public void TestFeesAndChargesFilter()
		{
			using (var stripter = new OrganisationFilterStrip())
			{
				var controls = GetCurrentFilterControls(stripter, new FeesAndChargesFilter("Test", (a, b) => new ZQuery()));
				var found = false;

				foreach (Control ctr in controls)
				{
					if (ctr.GetType() == typeof(FeesAndChargesFilterControl))
					{
						found = true;
						break;
					}
				}

				foreach (var ctr in controls)
				{
					ctr.Dispose();
				}
				AssertEquals("should return correct control", true, found);
			}
		}

		[RequiresSTA]
		public void TestHandlesSecondaryTypeTextModuleFilter()
		{
			using (var form = new OrgFilterStripForm(new OrganisationFilterBusinessObject()))
			{
				form.Show();

				var filterStrip = form.AddFilterStrip("Secondary Type");
				var moduleTextFilter = new OrgSecondaryTypeModuleFilter("Secondary Type", (value) => { return new ZQuery(); }, new CodeDescriptionPairList());
				var controls = GetCurrentFilterControls(filterStrip, moduleTextFilter);
				AssertEquals(1, controls.Length);

				var dropEditControl = controls[0] as ZDropEdit;
				AssertNotNull(dropEditControl);
				AssertEquals(false, dropEditControl.ShowDescriptionBox);
				AssertEquals(false, dropEditControl.ShowDescriptionInDropDown);

				dropEditControl.Dispose();
			}
		}

		public void TestHandlesModuleOrgCreditScoresDnBRatingModuleFilter()
		{
			AssertProvidesControlsFor(new OrgCreditScoresDnBRatingModuleFilter("D&B Rating Blah"));
		}

		public void TestHandlesModuleOrgSalesMainCompetitorModuleFilter()
		{
			AssertProvidesControlsFor(new OrgSalesMainCompetitorModuleFilter("Sales Main Competitor Filter Test", (ZGuid a, ZString b) => new ZQuery()));
		}

		public void TestHandlesModuleOrgHasMainCompetitorModuleFilter()
		{
			AssertProvidesControlsFor(new OrgHasMainCompetitorModuleFilter("Has Main Competitor Filter Test", (ZBool a, ZString b) => new ZQuery()));
		}

		class OrgFilterStripForm : ZForm
		{
			public OrgFilterStripForm(OrganisationFilterBusinessObject filterBizO)
				: base(filterBizO)
			{
				filterStripControl = new OrganisationFilterControl(new OrgHeaderCollection(new BusinessObjectFactory()), DataSource);
				Controls.Add(filterStripControl);
			}
			readonly OrganisationFilterControl filterStripControl;

			public new OrganisationFilterBusinessObject DataSource
			{
				get { return (OrganisationFilterBusinessObject)base.DataSource; }
			}

			public OrganisationFilterStrip AddFilterStrip(string description)
			{
				FilterStrip filterStripBizO = DataSource.FilterStrips.AddNew();
				filterStripBizO.FilterDescription = description;
				filterStripControl.AddFilterStrip(filterStripBizO);
				Control panel = filterStripControl.Controls.Find("FilterStripsPanel", false)[0];
				return (OrganisationFilterStrip)panel.Controls[panel.Controls.Count - 1];
			}
		}

		#region Implementation

		void AssertProvidesControlsFor(ModuleFilter filter)
		{
			using (OrganisationFilterStrip strip = new OrganisationFilterStrip())
			{
				Control[] controls = GetCurrentFilterControls(strip, filter);
				Assert("Should provide controls for " + filter.GetType().Name, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}

		Control[] GetCurrentFilterControls(OrganisationFilterStrip strip, ModuleFilter filter)
		{
			MethodInfo info = typeof(OrganisationFilterStrip).GetMethod(
				"GetCurrentFilterControls", BindingFlags.Instance | BindingFlags.NonPublic);

			return (Control[])info.Invoke(strip, new object[] { filter });
		}

		#endregion
	}
}
