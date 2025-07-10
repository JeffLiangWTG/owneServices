using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class CustomFieldControlLabelRenamerTest : TestCaseWithFactory
	{
		public void TestCustomLabelRenamed()
		{
			using (ZLabel testLabel = LabelForTest)
			{
				OrgHeader configOrg = Factory.New<OrgHeader>();

				using (CustomLabelControlRenamer renamer = new CustomLabelControlRenamer(
							 testLabel, fEditControl, new MockCustomLabelsProvider(configOrg, CustomLabelStyles.ShowByDefault), OrgHeader.Schema.OH_FullName))
				{
					AssertEquals("default:", testLabel.Text);

					OrgCustomLabels newLabel = configOrg.CustomLabels.AddNew();
					newLabel.OT_Caption = "new_label_caption";
					newLabel.OT_FieldName = "fieldname";
					AssertEquals("new_label_caption:", testLabel.Text);
				}
			}
		}

		public void TestCustomLabelRenamed_CaptionWithColon()
		{
			using (ZLabel testLabel = LabelForTest)
			{
				OrgHeader configOrg = Factory.New<OrgHeader>();

				using (CustomLabelControlRenamer renamer = new CustomLabelControlRenamer(
							 testLabel, fEditControl, new MockCustomLabelsProvider(configOrg, CustomLabelStyles.ShowByDefault), OrgHeader.Schema.OH_FullName, captionWithColon: false))
				{
					AssertEquals("default", testLabel.Text);

					OrgCustomLabels newLabel = configOrg.CustomLabels.AddNew();
					newLabel.OT_Caption = "new_label_caption";
					newLabel.OT_FieldName = "fieldname";
					AssertEquals("new_label_caption", testLabel.Text);
				}
			}
		}

		public void TestCustomLabelVisibilityChanged()
		{
			using (ZLabel testLabel = LabelForTest)
			{
				OrgHeader configOrg = Factory.New<OrgHeader>();

				OrgCustomLabels newLabel = configOrg.CustomLabels.AddNew();
				newLabel.OT_Caption = "new_label_caption";
				newLabel.OT_FieldName = "fieldname";

				using (CustomLabelControlRenamer renamer = new CustomLabelControlRenamer(
							 testLabel, fEditControl, new MockCustomLabelsProvider(configOrg, CustomLabelStyles.None), OrgHeader.Schema.OH_FullName))
				{
					AssertEquals("Visible at first", true, testLabel.Visible);
					AssertEquals("Visible at first", true, fEditControl.Visible);
					newLabel.Delete();
					AssertEquals("Not visible after label unconfigured", false, testLabel.Visible);
					AssertEquals("Not visible after label unconfigured", false, fEditControl.Visible);
				}
			}
		}

		#region Implementation

		Control fEditControl;

		protected override void SetUp()
		{
			base.SetUp();
			fEditControl = new TextBox();
		}

		ZLabel LabelForTest
		{
			get
			{
				ZLabel fLabel = new ZLabel();
				fLabel.Name = "Invisible Test Label";
				fLabel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				return fLabel;
			}
		}

		protected class MockCustomLabelsProvider : ICustomLabelsProvider
		{
			public MockCustomLabelsProvider(OrgHeader configOrg, CustomLabelStyles styles)
			{
				this.fConfigOrg = configOrg;
				this.Styles = styles;
			}

			public readonly CustomLabelStyles Styles;
			protected OrgHeader fConfigOrg;

			#region ICustomLabelsProvider Members

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return (fConfigOrg == null) ? null : new MockConfigOrgProvider(fConfigOrg); }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				CustomLabelInfoList result = new CustomLabelInfoList(typeof(OrgHeader), configOrg, (NoResString)"", factory);
				result.Add("fieldname", OrgHeader.Schema.OH_FullName, typeof(ZString), (NoResString)"default", (NoResString)"", Styles);
				return result;
			}

			#endregion
		}

		protected class MockConfigOrgProvider : ICustomLabelsConfigOrgProvider
		{
			public MockConfigOrgProvider(OrgHeader configOrg)
			{
				fConfigOrg = configOrg;
			}

			public event System.EventHandler ConfigOrgChanged
			{
				add { }
				remove { }
			}

			public OrgHeader ConfigOrg
			{
				get { return fConfigOrg; }
			}

			protected OrgHeader fConfigOrg;

			public BusinessObjectFactory Factory
			{
				get { return ConfigOrg != null ? ConfigOrg.Factory : new BusinessObjectFactory(); }
			}
		}

		#endregion
	}
}
