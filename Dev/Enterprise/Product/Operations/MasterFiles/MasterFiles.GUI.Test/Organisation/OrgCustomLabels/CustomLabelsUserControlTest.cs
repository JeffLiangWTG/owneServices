using System.Collections;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CustomLabelsUserControlTest : TestCase
	{
		[RequiresSTA]
		public void TestPopulateFieldsAndBindTos()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			MockBizO bizO = factory.New<MockBizO>();

			using (ZForm form = new ZForm(bizO))
			{
				CustomLabelsUserControl customFieldsCtrl = new CustomLabelsUserControl();
				form.Controls.Add(customFieldsCtrl);
				form.Show();
				Application.DoEvents();
				customFieldsCtrl.CustomLabelsProvider = new MockCustomLabelsProvider();
				customFieldsCtrl.PropertyNamesToExclude = new string[] { "xx_Field4" };

				ArrayList customFieldEditControls = new ArrayList();

				foreach (Control ctrl in customFieldsCtrl.Controls)
				{
					if (ctrl.Visible && ctrl.Text.IndexOf("no fields") != -1)
					{
						Fail("No fields available should not be shown!");
					}

					string controlBindingMember = ctrl.GetBindingMember();

					if (controlBindingMember.StartsWith("xx_"))
					{
						customFieldEditControls.Add(ctrl);
					}
				}

				AssertEquals("1 field not enabled and 1 field not enabled, 6 should be showing", 6, customFieldEditControls.Count);
				AssertEquals("xx_Field1", ((Control)customFieldEditControls[0]).GetBindingMember());
				AssertEquals("xx_Field2", ((Control)customFieldEditControls[1]).GetBindingMember());
				AssertEquals("xx_Field3", ((Control)customFieldEditControls[2]).GetBindingMember());
				AssertEquals("xx_Field5", ((Control)customFieldEditControls[3]).GetBindingMember());
				AssertEquals("xx_Field6", ((Control)customFieldEditControls[4]).GetBindingMember());
				AssertEquals("xx_Field8", ((Control)customFieldEditControls[5]).GetBindingMember());
			}
		}

		public void TestCaptionRenderingEnabled()
		{
			using (CustomLabelsUserControl control = new CustomLabelsUserControl())
			{
				AssertEquals("Labels are added to the control in code", false, control.CaptionRenderingEnabled);
			}
		}

		#region Test Classes

		class TestCustomLabelInfoList : CustomLabelInfoList
		{
			public TestCustomLabelInfoList(OrgHeader configOrg, BusinessObjectFactory factory)
				: base(typeof(MockBizO), configOrg, (NoResString)"", factory)
			{
				Add("F1", "xx_Field1", (NoResString)"Field1", CustomLabelStyles.None);
				Add("F2", "xx_Field2", (NoResString)"Field22", CustomLabelStyles.None);
				Add("F3", "xx_Field3", (NoResString)"Field333", CustomLabelStyles.None);
				Add("F4", "xx_Field4", (NoResString)"Field4444", CustomLabelStyles.None);
				Add("F5", "xx_Field5", (NoResString)"Field55555", CustomLabelStyles.None);
				Add("F6", "xx_Field6", (NoResString)"Field666666", CustomLabelStyles.None);
				Add("F7", "xx_Field7", (NoResString)"Field7777777", CustomLabelStyles.None);
				Add("F8", "xx_Field8", (NoResString)"Field88888888", CustomLabelStyles.None);
			}
		}

		class MockBizO : OrgHeader
		{
			public MockBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime xx_Field1
			{
				get { return ZDateTime.Empty; }
			}

			public ZString xx_Field2
			{
				get { return ZString.Empty; }
			}

			public ZBool xx_Field3
			{
				get { return false; }
			}

			public ZDecimal xx_Field4
			{
				get { return 0; }
			}

			public ZDecimal xx_Field5
			{
				get { return 0; }
			}

			public ZDateTime xx_Field6
			{
				get { return ZDateTime.Empty; }
			}

			public ZBool xx_Field7
			{
				get { return false; }
			}

			public ZDateTime xx_Field8
			{
				get { return ZDateTime.Empty; }
			}
		}

		class MockCustomLabelsProvider : ICustomLabelsProvider
		{
			public ICustomLabelsConfigOrgProvider ConfigOrgProvider
			{
				get { return new MockCustomLabelsConfigOrgProvider(); }
			}

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				return new TestCustomLabelInfoList(configOrg, factory);
			}
		}

		class MockCustomLabelsConfigOrgProvider : ICustomLabelsConfigOrgProvider
		{
			public event System.EventHandler ConfigOrgChanged
			{
				add { }
				remove { }
			}

			OrgHeader fConfigOrg;
			public OrgHeader ConfigOrg
			{
				get
				{
					if (fConfigOrg == null)
					{
						var factory = new BusinessObjectFactory();
						fConfigOrg = factory.New<OrgHeader>();

						fConfigOrg.CustomLabels.AddNew();
						fConfigOrg.CustomLabels.AddNew();
						fConfigOrg.CustomLabels.AddNew();
						fConfigOrg.CustomLabels.AddNew();
						fConfigOrg.CustomLabels.AddNew();
						fConfigOrg.CustomLabels.AddNew();
						fConfigOrg.CustomLabels.AddNew();

						fConfigOrg.CustomLabels[0].OT_FieldName = "F1";
						fConfigOrg.CustomLabels[0].OT_Caption = "x";
						fConfigOrg.CustomLabels[0].OT_Position = 1;
						fConfigOrg.CustomLabels[1].OT_FieldName = "F2";
						fConfigOrg.CustomLabels[1].OT_Caption = "x";
						fConfigOrg.CustomLabels[1].OT_Position = 2;
						fConfigOrg.CustomLabels[2].OT_FieldName = "F3";
						fConfigOrg.CustomLabels[2].OT_Caption = "x";
						fConfigOrg.CustomLabels[2].OT_Position = 3;
						fConfigOrg.CustomLabels[3].OT_FieldName = "F4";
						fConfigOrg.CustomLabels[3].OT_Caption = "x";
						fConfigOrg.CustomLabels[3].OT_Position = 4;
						fConfigOrg.CustomLabels[4].OT_FieldName = "F5";
						fConfigOrg.CustomLabels[4].OT_Caption = "x";
						fConfigOrg.CustomLabels[4].OT_Position = 5;
						fConfigOrg.CustomLabels[5].OT_FieldName = "F6";
						fConfigOrg.CustomLabels[5].OT_Caption = "x";
						fConfigOrg.CustomLabels[5].OT_Position = 6;
						fConfigOrg.CustomLabels[6].OT_FieldName = "F8";
						fConfigOrg.CustomLabels[6].OT_Caption = "x";
						fConfigOrg.CustomLabels[6].OT_Position = 7;
					}
					return fConfigOrg;
				}
			}

			public BusinessObjectFactory Factory
			{
				get { return ConfigOrg.Factory; }
			}
		}

		#endregion
	}
}
