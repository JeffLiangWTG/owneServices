using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class PackingListCustomUserControlTest : TestCaseWithFactory
	{
		public void TestCustomAttribute1Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customAttribute1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomAttribute1Label");
					AssertEquals("Custom Attribute 1", customAttribute1Label.Text);
				}
			}
		}

		public void TestCustomAttribute1Label()
		{
			var customAttrib1 = organisation.CustomLabels.AddNew();
			customAttrib1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomAttribute1;
			customAttrib1.OT_Caption = "Test Attr1";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customAttribute1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomAttribute1Label");
					AssertEquals("Test Attr1", customAttribute1Label.Text);
				}
			}
		}

		public void CustomAttribute2Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customAttribute2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomAttribute2Label");
					AssertEquals("Custom Attribute 2", customAttribute2Label.Text);
				}
			}
		}

		public void TestCustomAttribute2Label()
		{
			var customAttrib2 = organisation.CustomLabels.AddNew();
			customAttrib2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomAttribute2;
			customAttrib2.OT_Caption = "Test Attr2";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customAttribute2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomAttribute2Label");
					AssertEquals("Test Attr2", customAttribute2Label.Text);
				}
			}
		}

		public void TestCustomFlag1Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customFlag1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomFlag1Label");
					AssertEquals("Custom Flag 1", customFlag1Label.Text);
				}
			}
		}

		public void TestCustomFlag1Label()
		{
			var customFlag1 = organisation.CustomLabels.AddNew();
			customFlag1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomFlag1;
			customFlag1.OT_Caption = "Test Flag 1";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customFlag1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomFlag1Label");
					AssertEquals("Test Flag 1", customFlag1Label.Text);
				}
			}
		}

		public void TestCustomFlag2Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customFlag2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomFlag2Label");
					AssertEquals("Custom Flag 2", customFlag2Label.Text);
				}
			}
		}

		public void TestCustomFlag2Label()
		{
			var customFlag2 = organisation.CustomLabels.AddNew();
			customFlag2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomFlag2;
			customFlag2.OT_Caption = "Test Flag 2";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customFlag2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomFlag2Label");
					AssertEquals("Test Flag 2", customFlag2Label.Text);
				}
			}
		}

		public void TestCustomDate1Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDate1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDate1Label");
					AssertEquals("Custom Date 1", customDate1Label.Text);
				}
			}
		}

		public void TestCustomDate1Label()
		{
			var customDate1 = organisation.CustomLabels.AddNew();
			customDate1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomDate1;
			customDate1.OT_Caption = "Test Custom Date 1";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDate1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDate1Label");
					AssertEquals("Test Custom Date 1", customDate1Label.Text);
				}
			}
		}

		public void TestCustomDate2DateEdit_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDate2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDate2Label");
					AssertEquals("Custom Date 2", customDate2Label.Text);
				}
			}
		}

		public void TestCustomDate2DateEdit()
		{
			var customDate2 = organisation.CustomLabels.AddNew();
			customDate2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomDate2;
			customDate2.OT_Caption = "Test Custom Date 2";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDate2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDate2Label");
					AssertEquals("Test Custom Date 2", customDate2Label.Text);
				}
			}
		}

		public void TestCustomDecimal1Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDecimal1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDecimal1Label");
					AssertEquals("Custom Number 1", customDecimal1Label.Text);
				}
			}
		}

		public void TestCustomDecimal1Label()
		{
			var customDecimal1 = organisation.CustomLabels.AddNew();
			customDecimal1.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomDecimal1;
			customDecimal1.OT_Caption = "Test Num 1";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDecimal1Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDecimal1Label");
					AssertEquals("Test Num 1", customDecimal1Label.Text);
				}
			}
		}

		public void TestCustomDecimal2Label_DefaultCaption()
		{
			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDecimal2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDecimal2Label");
					AssertEquals("Custom Number 2", customDecimal2Label.Text);
				}
			}
		}

		public void TestCustomDecimal2Label()
		{
			var customDecimal2 = organisation.CustomLabels.AddNew();
			customDecimal2.OT_FieldName = Enterprise.Core.Constants.CustomLabels.CustomsPackingList.CustomDecimal2;
			customDecimal2.OT_Caption = "Test Num 2";

			using (var form = new PackingListForm(packingList))
			{
				form.Show();
				form.FindSingle<ZTemplateTabControl>(x => x.Name == "MainTabControl").SelectedTab = form.CustomTabPage;
				using (var control = form.PackingListCustomUserControl)
				{
					var customDecimal2Label = control.FindSingle<ZLabel>(x => x.Name == "CustomDecimal2Label");
					AssertEquals("Test Num 2", customDecimal2Label.Text);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Supplier = organisation.PK;
			packingList = declaration.LoadOrCreateCusPackingList(Factory);
		}

		CusPackingList packingList;
		OrgHeader organisation;
	}
}
