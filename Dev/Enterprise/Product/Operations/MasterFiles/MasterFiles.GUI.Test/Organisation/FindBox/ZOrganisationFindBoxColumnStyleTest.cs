using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZOrganisationFindBoxColumnStyleTest : ZGuidFindBoxColumnStyleTest
	{
		[RequiresSTA]
		public void TestTypedTemp()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.T);
				SendKeyToEditControl(Keys.E);
				SendKeyToEditControl(Keys.M);
				SendKeyToEditControl(Keys.P);

				AssertNull(ZFormModaliser.ActiveForm);

				SendKeyToEditControl(Keys.Tab);

				AssertActivePopup();
			}
		}

		[RequiresSTA]
		public void TestTypedTemp_AllowTemp_False()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				TestForm.Show();
				AssertPreconditions();

				SuperDummy.Collection[0].Collection.AllowNewTemporaryOrganisations = false;

				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.T);
				SendKeyToEditControl(Keys.E);
				SendKeyToEditControl(Keys.M);
				SendKeyToEditControl(Keys.P);

				AssertNull(ZFormModaliser.ActiveForm);

				SendKeyToEditControl(Keys.Tab);
				AssertNull(ZFormModaliser.ActiveForm);
			}
		}

		[RequiresSTA]
		public void TestF5()
		{
			using (TestForm = CreateTestForm(SuperDummy, false))
			{
				TestForm.Show();
				AssertPreconditions();

				SendKeyToEditControl(Keys.Tab);
				SendKeyToEditControl(Keys.F5);

				AssertActivePopup();
			}
		}

		#region Implementation

		protected override ZGrid Grid
		{
			get { return ((ZOrganisationFindBoxColumnStyleTestForm)TestForm).zGrid1; }
		}

		protected override ZForm CreateTestFormCore(BusinessObject superDummy)
		{
			return new ZOrganisationFindBoxColumnStyleTestForm(superDummy);
		}

		void AssertActivePopup()
		{
			try
			{
				TemporaryOrganisationsPopup popup = ZFormModaliser.ActiveForm as TemporaryOrganisationsPopup;
				((IFindBox)((ZOrganisationFindBoxColumnStyle)Grid.Columns["Z0_Guid"].ColumnStyle).EditControl).Code = Child1.Z0_Code;
				AssertNotNull(popup);
				popup.Close();
				Application.DoEvents();

				Assert(SuperDummy.Collection.Count > 0);
				AssertEquals(Child1.PK, SuperDummy.Collection[0].Z0_Guid);
				Assert("Should not hide edit control when popup closes.", Grid.LastFocusedColumn.EditControl.ContainsFocus);
			}
			finally
			{
				DisposeActiveForm();
			}
		}

		void DisposeActiveForm()
		{
			IDisposable disposableForm = ZFormModaliser.ActiveForm;
			if (disposableForm != null)
			{
				disposableForm.Dispose();
			}
		}

		class ZOrganisationFindBoxColumnStyleTestForm : ZForm
		{
			public ZGrid zGrid1;

			public ZOrganisationFindBoxColumnStyleTestForm(BusinessObject bizObj)
				: base(bizObj)
			{
			}

			#region Windows Form Designer generated code

			protected override void InitializeComponent()
			{
				ZOrganisationFindBoxColumnStyleInfo zZOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
				ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
				this.zGrid1 = new ZGrid();
				((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
				this.SuspendLayout();
				// 
				// zGrid1
				// 
				this.zGrid1.AllowNavigation = false;
				this.zGrid1.BindTo = "Collection";
				this.zGrid1.CaptionVisible = false;
				zZOrganisationFindBoxColumnStyleInfo1.BindToList = "Collection";
				zZOrganisationFindBoxColumnStyleInfo1.Caption = "GuidFindBox";
				zZOrganisationFindBoxColumnStyleInfo1.ColumnName = "Z0_Guid";
				zZOrganisationFindBoxColumnStyleInfo1.ModuleID = ModuleIDs.Organisation;
				zZOrganisationFindBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
				zCodeFindBoxColumnStyleInfo1.BindToList = "Collection";
				zCodeFindBoxColumnStyleInfo1.Caption = "Code";
				zCodeFindBoxColumnStyleInfo1.ColumnName = "Z0_Code";
				if (ZGuidFindBoxColumnStyleTestForm.AddGuidColumnFirst)
				{
					this.zGrid1.ColumnStyles.Add(zZOrganisationFindBoxColumnStyleInfo1);
					this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
				}
				else
				{
					this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
					this.zGrid1.ColumnStyles.Add(zZOrganisationFindBoxColumnStyleInfo1);
				}
				this.zGrid1.EnableToolTips = false;
				this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
				this.zGrid1.LayoutKey = "zGrid1";
				this.zGrid1.Location = new System.Drawing.Point(16, 8);
				this.zGrid1.Name = "zGrid1";
				this.zGrid1.Size = new System.Drawing.Size(528, 352);
				this.zGrid1.TabIndex = 0;
				// 
				// TestForm
				// 

				this.ClientSize = new System.Drawing.Size(560, 374);
				this.Controls.Add(this.zGrid1);
				this.Name = "TestForm";
				this.Text = "TestForm";
				((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
				this.ResumeLayout(false);
			}
			#endregion
		}

		#endregion
	}
}
