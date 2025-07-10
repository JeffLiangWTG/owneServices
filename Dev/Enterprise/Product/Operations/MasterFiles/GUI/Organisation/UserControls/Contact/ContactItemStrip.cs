using System;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ContactItemStrip : ZUserControl, IReadOnlyToggleControl
	{
		public ContactItemStrip()
		{
			InitializeComponent();
			SetupDeleteButton();
			SetupDescriptionDropDownList();
		}

		#region CurrentDataItem

		public new ContactItemProxy CurrentDataItem
		{
			get { return (ContactItemProxy)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			RefreshReadOnlyControls();
		}

		#endregion

		#region IsMandatory

		public bool IsMandatory
		{
			get { return isMandatory; }
			set
			{
				isMandatory = value;
				DescriptionDropDownList.CodeBox.ReadOnly = isMandatory;
				deleteButton.Visible = !isMandatory;
			}
		}
		bool isMandatory;

		#endregion

		#region DescriptionDropDownList

		public void SetupDescriptionDropDownList()
		{
			DescriptionDropDownList.CodeBox.KeyDown += DescriptionDropDownList_TextChanged;
			DescriptionDropDownList.DropDownClosed += DescriptionDropDownList_DropDownClosed;
		}

		public void FocusDescriptionDropDownList()
		{
			DescriptionDropDownList.Focus();
		}

		void DescriptionDropDownList_TextChanged(object sender, EventArgs e)
		{
			if (!clearingDescriptionDropDownListText)
			{
				DescriptionDropDownList.ShowDropDown();
			}
		}

		void DescriptionDropDownList_DropDownClosed(object sender, EventArgs e)
		{
			ClearDescriptionIfInvalid();
		}

		void ClearDescriptionIfInvalid()
		{
			var currentDataItem = CurrentDataItem;
			if (currentDataItem == null)
			{
				return;
			}

			var list = DescriptionDropDownList.List as ICodeDescriptionPairList;
			if (list != null && !list.ContainsCode(DescriptionDropDownList.Text))
			{
				clearingDescriptionDropDownListText = true;
				DescriptionDropDownList.Text = "";
				clearingDescriptionDropDownListText = false;
			}
		}
		bool clearingDescriptionDropDownListText;

		#endregion

		#region DeleteButton

		void SetupDeleteButton()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				deleteButton.FlatStyle = FlatStyle.Flat;
				deleteButton.BackgroundImage = Icons.GetImage(IconTypes.MinusButtonRest);
				deleteButton.MouseEnter += deleteButton_MouseEnter;
				deleteButton.MouseLeave += deleteButton_MouseLeave;
			}
		}

		void deleteButton_MouseEnter(object sender, EventArgs e)
		{
			deleteButton.BackgroundImage = Icons.GetImage(IconTypes.MinusButtonActive);
		}

		void deleteButton_MouseLeave(object sender, EventArgs e)
		{
			deleteButton.BackgroundImage = Icons.GetImage(IconTypes.MinusButtonRest);
		}

		protected virtual void deleteButton_Click(object sender, EventArgs e)
		{
			CurrentDataItem.Delete();
		}

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get { return readOnly || CurrentDataItem == null || CurrentDataItem.ReadOnly; }
			set
			{
				readOnly = value;
				RefreshReadOnlyControls();
			}
		}
		bool readOnly;

		protected virtual void RefreshReadOnlyControls()
		{
			DescriptionDropDownList.Enabled = !ReadOnly;
			deleteButton.Enabled = !ReadOnly;
			deleteButton.BackgroundImage = deleteButton.Enabled ? Icons.GetImage(IconTypes.MinusButtonRest) : Icons.GetImage(IconTypes.MinusButtonDisabled);
		}

		#endregion
	}
}
