using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TWJobDocAddressControl : ZUserControl, ISupportMultipleResourceStringDataSupporter
	{
		public TWJobDocAddressControl()
		{
			CodeBoxesVisible = true;
			InitializeComponent();
			InitializeExtensions();
			SetControlVisible();

			OrganisationPanel.AllowOverlap(OverrideAddressCheckbox);
			DetailsTabControl.AllowOverlap(OverrideAddressCheckbox);
		}

		#region InitializeExtensions
		void InitializeExtensions()
		{
			if (AddressDropEdit.GetExtension<INotificationExtension>() == null)
			{
				AddressDropEdit.Extensions.Add(new NotificationExtension()); // For validation notification icons
			}
		}
		#endregion

		void SetControlVisible()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				EditAddressPanel.Visible = EditContactPanel.Visible = EditLocalAddressPanel.Visible = false;
			}
		}

		protected TWJobDocAddress DocAddress => (TWJobDocAddress)base.CurrentDataItem;

		public override ResourceStringData CaptionResourceString
		{
			get { return MainGroupBox.CaptionResourceString; }
			set { MainGroupBox.CaptionResourceString = value; }
		}

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindToOrganisations))]
		[ZArchitecture.GUI.Testing.ExcludeFromBindToAttributesTest]
		public string BindToOrganisations
		{
			get { return OrganisationFindBox.BindToList; }
			set { OrganisationFindBox.BindToList = value; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			string orgBindingMember = (DataSource == null) ? "" : new KBindingMemberInfo(dataMember, "OrganisationPK");
			OrganisationFindBox.SetDataBinding(DataSource, orgBindingMember);

			ContactDropEdit.BindToList = "Organisation+ContactsActive";
			AddressDropEdit.BindToList = "Organisation+Address_List";

			if (!CodeBoxesVisible)
			{
				TPCCodeTextBox.Visible = false;
				TPCCodeTypeDropEdit.Visible = false;
				AEOCodeTextBox.Visible = false;
				AEOCodeTypeDropEdit.Visible = false;
				CBPCodeTextBox.Visible = false;
				CBPCodeTypeDropEdit.Visible = false;
				FRICodeTextBox.Visible = false;
				FRICodeTypeDropEdit.Visible = false;
			}
		}

		void SetDefaultTextIfLocalAddressIsEmpty()
		{
			var docAddress = DocAddress;
			if (docAddress != null && !docAddress.OrganisationPK.IsEmpty && docAddress.LocalAddressDetail.IsEmpty)
			{
				LocalAddressDetailLabel.Text = Res.GetString("18974493-5c15-4298-81c8-a922d08aa007", "The selected address does not have a traditional Chinese translated address.");
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (DocAddress is TWJobDocAddress docAddress && !docAddress.IsDeleted)
			{
				docAddress.OrgHeaderAfterChange -= DocAddress_OrgHeaderAfterChange;
				docAddress.E2_AddressOverrideInfo.ValueChanged -= AddressOverride_ValueChanged;
				docAddress.DocAddressChanged -= DocAddress_DocAddressChanged;

				if (CodeBoxesVisible && docAddress.Parent is JobDeclaration declaration)
				{
					declaration.JE_MessageTypeInfo.ValueChanged -= MessageTypeValueChanged;
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (DocAddress is TWJobDocAddress docAddress)
			{
				UpdateControlsLayout();
				docAddress.OrgHeaderAfterChange += DocAddress_OrgHeaderAfterChange;
				docAddress.E2_AddressOverrideInfo.ValueChanged += AddressOverride_ValueChanged;
				docAddress.DocAddressChanged += DocAddress_DocAddressChanged;

				if (CodeBoxesVisible)
				{
					UpdateCodeControlVisible(docAddress);
					if (docAddress.Parent is JobDeclaration declaration)
					{
						declaration.JE_MessageTypeInfo.ValueChanged += MessageTypeValueChanged;
					}
				}
			}
		}

		void DocAddress_DocAddressChanged(object sender, EventArgs e)
		{
			SetDefaultTextIfLocalAddressIsEmpty();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (Visible)
			{
				UpdateControlsLayout();
			}
		}

		void DocAddress_OrgHeaderAfterChange(object sender, EventArgs e)
		{
			SetContact();
			SetDefaultTextIfLocalAddressIsEmpty();
		}

		void AddressOverride_ValueChanged(object sender, EventArgs e)
		{
			UpdateControlsLayout();
		}

		void MessageTypeValueChanged(object sender, EventArgs e)
		{
			var docAddress = DocAddress;
			if (docAddress != null)
			{
				UpdateCodeControlVisible(docAddress);
			}
		}

		protected void UpdateCodeControlVisible(TWJobDocAddress docAddress)
		{
			TPCCodeTextBox.Visible = TPCCodeTypeDropEdit.Visible = (docAddress.IsExport && docAddress.IsSupplierDocumentaryAddress()) || (docAddress.IsImport && docAddress.IsImporterDocumentaryAddress());
			AEOCodeTextBox.Visible = AEOCodeTypeDropEdit.Visible = !docAddress.IsLocalProcessorAddress() && !docAddress.IsManufacturerAddress();
			CBPCodeTextBox.Visible = CBPCodeTypeDropEdit.Visible = docAddress.IsSupplierOrImporterDocumentaryAddress();
			FRICodeTextBox.Visible = FRICodeTypeDropEdit.Visible = docAddress.IsManufacturerAddress();
		}

		void SetContact()
		{
			var docAddress = DocAddress;
			if (docAddress != null)
			{
				docAddress.E2_Contact = docAddress.Organisation?.ContactsActive.Cast<OrgContact>().FirstOrDefault()?.OC_ContactName ?? ZString.Empty;
			}
		}

		void UpdateControlsLayout()
		{
			var docAddress = DocAddress;
			if (docAddress != null && !docAddress.IsDeleted && this.Visible)
			{
				using (docAddress.GetValidationSuspender())
				{
					SuspendLayout();
					var isAddressOverride = docAddress.E2_AddressOverride;
					EditAddressPanel.Visible = EditContactPanel.Visible = EditLocalAddressPanel.Visible = isAddressOverride;
					OrganisationPanel.Visible = AddressPanel.Visible = ContactPanel.Visible = LocalAddressDetailLabel.Visible = !isAddressOverride;
					ResumeLayout(false);
					PerformLayout();
				}
				IDCodeTextBox.GetExtension<IStatusbarExtension>().Clear();
			}
		}

		[DefaultValue(true)]
		public bool CodeBoxesVisible { get; set; }

		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => DocAddress;
	}
}
