using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DummyForm))]
	public class ExporterSchemePluginTest : ZFormBasherTest
	{
		public void TestTopLevelParentOrg()
		{
			using (ExporterSchemePlugin testPlugIn = GetNewPluginToTest(Org))
			{
				AssertEquals("Parent Relationship set", Org, testPlugIn.BusinessEntity);
			}
		}

		public void TestPluginIsDockedToFill()
		{
			using (ExporterSchemePlugin testPlugIn = GetNewPluginToTest(Org))
			{
				AssertEquals("Control is Docked to Fill", DockStyle.Fill, testPlugIn.UserControl.Dock);
			}
		}

		public virtual void TestLicence()
		{
			using (ExporterSchemePlugin testPlugIn = GetNewPluginToTest(Org))
			{
				testPlugIn.OnUserControlShown();
				AssertEquals("Control is enabled", false, testPlugIn.UserControl.GetReadOnly());
			}
		}

		protected virtual ExporterSchemePlugin GetNewPluginToTest(OrgHeader hostEntity)
		{
			return new ExporterSchemePlugin(Org);
		}

		protected virtual ZString CountryForTest
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		public override Type FormToBashType
		{
			get { return typeof(DummyForm); }
		}

		#region Organisation

		protected override Form GetFormToBashCore()
		{
			DummyForm testForm = new DummyForm(Org);
			testForm.Height = 600;
			testForm.Width = 700;
			return testForm;
		}

		OrgHeader fOrg;
		OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = new BusinessObjectFactory().LoadTop1<OrgHeader>(new ZQuery());
				}
				return fOrg;
			}
		}

		#endregion

		#region Dummy Form

		protected class DummyForm : ZForm
		{
			public DummyForm(OrgHeader org)
				: base(org)
			{
				this.Org = org;
				this.CaptionRenderingEnabled = true;
				PlugIns.Add(ControllerIDs.ExporterScheme);
			}

			public ZTemplateTabControl TabControl;

			internal protected override ZTabControl TopLevelTabControl
			{
				get { return TabControl; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.TabControl = new ZTemplateTabControl();
				this.Controls.Add(TabControl);
			}

			protected OrgHeader Org;
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			countryChange = GlbCompany.CurrentCompany.TemporarilySetCountry(CountryForTest);
			base.SetUp();
		}

		protected override void TearDown()
		{
			countryChange.Dispose();
			base.TearDown();
		}

		IDisposable countryChange;

		#endregion
	}
}
