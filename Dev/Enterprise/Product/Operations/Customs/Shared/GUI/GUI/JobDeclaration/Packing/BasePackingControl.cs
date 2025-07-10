using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public interface IBasePackingControl : IDisposable
	{
		BaseJobDeclaration JobDeclaration { get; set; }
		DockStyle Dock { get; set; }
		void SetDataBinding(object dataSource, string dataMember);
	}

	public partial class BasePackingControl : BaseCustomsEntryUserControl, IBasePackingControl
	{
		public BasePackingControl()
		{
			InitializeComponent();
			ParentChanged += new EventHandler(BasePackingControl_ParentChanged);
		}

		public override BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				base.JobDeclaration = value;
				HookToTabControlTabPageChangedEvent();
			}
		}

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			if (JobDeclaration != null && !JobDeclaration.IsBillIssueDateVisible)
			{
				HouseBillsGrid.RemoveFromAvailableColumns(CusDecHouseBillSchema.CU_IssueDate.Name);
			}
		}

		#region SelectedIndexChanged of All Parent Tab Controls

		void BasePackingControl_ParentChanged(object sender, EventArgs e)
		{
			HookToTabControlTabPageChangedEvent();
		}

		bool isTabPageChangedEventHooked;
		void HookToTabControlTabPageChangedEvent()
		{
			if (!isTabPageChangedEventHooked && this.Parent != null &&
				JobDeclaration != null && JobDeclaration.IsPluggedIntoShipment)
			{
				ZTabControl parentTabControl = GetParentTabControl(this);
				while (parentTabControl != null)
				{
					parentTabControl = GetParentTabControl(parentTabControl);
				}
				isTabPageChangedEventHooked = true;
			}
		}

		ZTabControl GetParentTabControl(Control control)
		{
			ZTabPage tabPage = GetParentTabPage(control);
			return tabPage != null ? tabPage.Parent as ZTabControl : null;
		}

		ZTabPage GetParentTabPage(Control control)
		{
			Control tabPage = control;
			while (tabPage != null && !(tabPage is ZTabPage))
			{
				tabPage = tabPage.Parent;
			}
			return tabPage as ZTabPage;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
