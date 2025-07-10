using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	sealed internal partial class ChangeOthersSecurityControl : ZUserControl
	{
		ISupportChangeOthersSecurity changeOthersSecurity;
		internal GlbSecurityChangeOthersView.ChangeOthersMode mode;
		public GlbSecurityChangeOthersView.ChangeOthersMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = value;
				if (mode == GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner)
				{
					this.changeOthersSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|1b290415-521f-4663-95bb-614b236615d8", "Modify group membership for the following groups");
					this.changeOthersSecurityLabel.Hide();
					this.changeOthersSecurityLabel.Show();
					addStaffButton.Visible = false;
					addGroupButton.Visible = true;
					this.BindingSource.SetBindingMember(this.changeOthersSecurityGrid, "SecurityChangeOthersView");
				}
				else if (mode == GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator)
				{
					this.changeOthersSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|afa72d8a-c236-4a76-9f1c-4e47f4a340d4", "Modify security rights for the following staff and groups");
					this.changeOthersSecurityLabel.Hide();
					this.changeOthersSecurityLabel.Show();
					addStaffButton.Visible = true;
					addGroupButton.Visible = true;
					this.BindingSource.SetBindingMember(this.changeOthersSecurityGrid, "SecurityChangeOthersView");
				}
				else if (mode == GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup)
				{
					this.changeOthersSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|27dd06ee-0fb6-4294-973b-fcf9f5494ecf", "These staff/groups are Group Owners for this group, and can modify membership");
					this.changeOthersSecurityLabel.Hide();
					this.changeOthersSecurityLabel.Show();
					addStaffButton.Visible = true;
					addGroupButton.Visible = true;
					this.BindingSource.SetBindingMember(this.changeOthersSecurityGrid, "GroupOwnersForGroupView");
					zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|34cfdca6-147a-4365-b9eb-6fb1895c34e8", "Group Code");
					zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ChangeOthersSecurityControl|57907fe7-48c8-420e-b4e9-9ab60d41ce25", "Group Description");
					this.changeOthersSecurityGrid.ColumnStyles.Remove(zTextBoxColumnStyleInfo3);
					zTextBoxColumnStyleInfo3.IsVisible = false;
					zTextBoxColumnStyleInfo3.IsUnavailable = true;
					zTextBoxColumnStyleInfo3.Width = 0;
					this.changeOthersSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
					this.changeOthersSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
					this.changeOthersSecurityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
				}
				OnEnabledChanged(null);
			}
		}

		public ChangeOthersSecurityControl()
		{
			InitializeComponent();
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ISupportChangeOthersSecurity ChangeOthersSecurity
		{
			get { return changeOthersSecurity; }
			set { changeOthersSecurity = value; }
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			if (!isInRefreshEnabled)
			{
				isInRefreshEnabled = true;
				RefreshEnabled();
				isInRefreshEnabled = false;
			}
		}

		bool isInRefreshEnabled;

		void RefreshEnabled()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				bool value = Env.CurrentUser.IsController || mode == GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup;
				if (Enabled != value)
				{
					Enabled = value;
				}
			}
		}

		#region Add Group

		GlbSecurityChangeOthersView currentView
		{
			get
			{
				if (mode == GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup)
				{
					return changeOthersSecurity.GroupOwnersForGroupView;
				}
				return changeOthersSecurity.SecurityChangeOthersView;
			}
		}

		void AddGroupButton_Click(object sender, EventArgs e)
		{
			ZFilterGridModule groupModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbGroup);
			EmbeddedModulePopup groupPopup = new EmbeddedModulePopup(groupModule);
			groupPopup.Selected += GroupPopup_Selected;
			IFindBox findBox = new FindBox(
				changeOthersSecurity.CompleteGroupList,
				groupPopup,
				currentView.AddSecurityToChangeOtherGroup);
			// see the default behaviour if this method is NOT called
			IModuleDecisionProvider provider = groupModule.GetModuleDecisionProviderForFindBox(findBox);
			groupModule.OverrideModuleDecisionProvider(provider);
			groupPopup.EmbeddedModulePopupOKButtonStrategy = provider;
			groupPopup.ShowModal(findBox, FindForm());
		}

		void GroupPopup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			BusinessObject[] selectedGroupBusinessObjects;
			selectedGroupBusinessObjects = e.SelectedBusinessObjects;
			foreach (BusinessObject bizo in selectedGroupBusinessObjects)
			{
				currentView.AddSecurityToChangeOtherGroup(((GlbGroup)bizo).GG_Code);
			}
		}

		#endregion

		#region Add Staff

		void AddStaffButton_Click(object sender, EventArgs e)
		{
			ZFilterGridModule staffModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff);
			EmbeddedModulePopup staffPopup = new EmbeddedModulePopup(staffModule);
			staffPopup.Selected += StaffPopup_Selected;
			IFindBox findBox = new FindBox(
				changeOthersSecurity.CompleteStaffList,
				staffPopup,
				currentView.AddSecurityToChangeOtherStaff);
			IModuleDecisionProvider provider = staffModule.GetModuleDecisionProviderForFindBox(findBox);
			staffModule.OverrideModuleDecisionProvider(provider);
			staffPopup.EmbeddedModulePopupOKButtonStrategy = provider;
			staffPopup.ShowModal(findBox, FindForm());
		}

		void StaffPopup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			BusinessObject[] selectedStaffBusinessObjects;
			selectedStaffBusinessObjects = e.SelectedBusinessObjects;
			foreach (BusinessObject bizo in selectedStaffBusinessObjects)
			{
				currentView.AddSecurityToChangeOtherStaff(((GlbStaff)bizo).GS_Code);
			}
		}

		#endregion

		#region Delete

		void DeleteStaffOrGroupButton_Click(object sender, EventArgs e)
		{
			changeOthersSecurityGrid.DeleteMenuItem.PerformClick();
		}

		#endregion

		#region Implementation

		void ChangeOthersSecurityGrid_DoubleClick(object sender, EventArgs e)
		{
			if (changeOthersSecurityGrid.SelectedElements.Length > 0 &&
				changeOthersSecurityGrid.List[changeOthersSecurityGrid.HitTest(changeOthersSecurityGrid.PointToClient(Cursor.Position)).Row] != null)
			{
				ShowForm(changeOthersSecurityGrid.GetSelectedElements<GlbSecurity>()[0]);
			}
		}

		internal IZForm ShowForm(GlbSecurity security)
		{
			Type businessObjectType;
			ControllerID id;
			ZGuid pk;
			if (mode == GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator)
			{
				pk = security.GU_ItemGUID;
				if (security.GU_SecurityRight == GlbSecurity.ChangeOtherGroupSecurityRightName)
				{
					businessObjectType = typeof(GlbGroup);
					id = ControllerIDs.GlbGroup;
				}
				else
				{
					businessObjectType = typeof(GlbStaff);
					id = ControllerIDs.GlbStaff;
				}
			}
			else if (mode == GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup)
			{
				if (!security.GU_GG.IsEmpty)
				{
					pk = security.GU_GG;
					businessObjectType = typeof(GlbGroup);
					id = ControllerIDs.GlbGroup;
				}
				else
				{
					pk = security.GU_GS;
					businessObjectType = typeof(GlbStaff);
					id = ControllerIDs.GlbStaff;
				}
			}
			else
			{
				pk = security.GU_ItemGUID;
				businessObjectType = typeof(GlbGroup);
				id = ControllerIDs.GlbGroup;
			}

			BusinessObject businessObject = ChangeOthersSecurity.Factory.Load(businessObjectType, pk);
			ZController controller = ZControllerFactory.Create(id);
			controller.SetFormsModalTo(FindForm());
			return controller.ShowViewForm(businessObject);
		}

		delegate void SetCodeHandler(string code);

		#endregion

		#region FindBox Class

		class FindBox : IFindBox
		{
			readonly IFindBoxListProvider listProvider;
			readonly IFindBoxPopup popupForm;
			readonly SetCodeHandler setCodeHandler;

			public FindBox(IFindBoxListProvider listProvider, IFindBoxPopup popupForm, SetCodeHandler setCodeHandler)
			{
				this.listProvider = listProvider;
				this.popupForm = popupForm;
				this.setCodeHandler = setCodeHandler;
			}

			#region IFindBox Members

			string IFindBox.Code
			{
				get { return ""; }
				set { setCodeHandler(value); }
			}

			string IFindBox.Description
			{
				get { return ""; }
				set { }
			}

			IFindBoxListProvider IFindBox.ListProvider
			{
				get { return listProvider; }
			}

			IFindBoxPopup IFindBox.PopupForm
			{
				get { return popupForm; }
			}

			#endregion
		}

		#endregion
	}
}
