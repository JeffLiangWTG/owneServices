using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// A control for selecting an organisation and address.
	/// SetDataBinding() takes a dataSource and dataMember that point to the OrgAddress foreign key.
	/// For example, SetDataBinding(shipment, "JS_OA_NotifyParty_ZAddress");
	/// In this example, property JS_OA_NotifyParty_ZAddress of type ZAddress is also expected.
	/// </summary>
	[DefaultDataSourceBindingMember(null)]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ZOrgAddressControl : ZUserControl,
		IFetchHintGenerator,
		IExtendedControl,
		INotificationDataMembers,
		IZAddressParent,
		IDontNeedExtraSpaceForLabel,
		IDisposable
	{
		#region Custom Adornment Layout

		class ZOrgAddressControlAdornmentLayout : AdornmentLayout<ZOrgAddressControl>
		{
			public override IEnumerable<Control> GetBackroundAdornmentTargets(ZOrgAddressControl source)
			{
				yield return source.OrganisationFindBox.CodeBox;
				yield return source.OrganisationFindBox.DescriptionBox;
				yield return source.AddressDropEdit.CodeBox;
				yield return source.AddressDropEdit.DescriptionBox;
			}

			public override IEnumerable<IIconLayout> GetIconAdornmentTargets(ZOrgAddressControl source)
			{
				yield return new IconLayout(source.OrganisationFindBox.PopupButton, IconAlignment.Center);
			}
		}

		#endregion

		#region Constructors

		static ZOrgAddressControl()
		{
			NotificationAdornmentFactory.RegisterCustomLayout(new ZOrgAddressControlAdornmentLayout());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "Baseline")]
		public ZOrgAddressControl()
		{
			InitializeComponent();
			InitializeExtensions();
			InitializeEventHandlers();

			ContactsLink.TabStop = false;
			AddressesLink.TabStop = false;

			GroupBox.SetResourceStringIdentifyingControl(this);

			var resources = new ResourceManager(typeof(ZOrganisationControl).Namespace + ".Organisation.UserControls.ZOrganisationControl.ZOrganisationControlIcons", typeof(ZOrganisationControl).Assembly);
			ContactsLink.Image = (System.Drawing.Image)resources.GetObject("ContactsLink.Image");
			AddressesLink.Image = (System.Drawing.Image)resources.GetObject("AddressesLink.Image");

#if DEBUG
			TypeDescriptor.AddAttributes(AddressLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		#endregion

		#region Initialization

		void InitializeExtensions()
		{
			Extensions = new DefaultControlExtensionCollection(this);
			Extensions.Substitute<IHintExtension>(
				CompositeHintExtension.Create(OrganisationFindBox, AddressDropEdit));

			OrganisationFindBox.Extensions.Add(new HintExtension());
			AddressDropEdit.Extensions.Add(new HintExtension());
		}

		void InitializeEventHandlers()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				OrganisationFindBox.Validated += (sender, args) => Extensions.Get<IValidationExtension>().Validate();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (OnlyStopOnDebtor)
			{
				ContactsLink.TabStop = false;
				AddressesLink.TabStop = false;
				AddressDropEdit.TabStop = false;
				var zAddress = BindingSource.Current as ZAddress;

				if (zAddress != null && zAddress.AddressFKInfo != null)
				{
					addressFKInfo_ValueChangedHolder = zAddress.AddressFKInfo;
					addressFKInfo_ValueChangedHolder.ValueChanged += AddressFKInfo_ValueChanged;
				}
			}
		}

		ZPropertyInfo addressFKInfo_ValueChangedHolder;

		void AddressFKInfo_ValueChanged(object sender, EventArgs e)
		{
			AddressDropEdit.TabStop = false;
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}

				if (addressFKInfo_ValueChangedHolder != null)
				{
					addressFKInfo_ValueChangedHolder.ValueChanged -= AddressFKInfo_ValueChanged;
					addressFKInfo_ValueChangedHolder = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Caption

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
				GroupBox.Text = value;
			}
		}

		public override ResourceStringData CaptionResourceString
		{
			get { return GroupBox.CaptionResourceString; }
			set { GroupBox.CaptionResourceString = value; }
		}

		#endregion

		#region Popup Caption

		[Category(ZGUIConstants.DesignerCategory)]
		public string PopupCaption
		{
			get { return OrganisationFindBox.PopupCaption; }
			set { OrganisationFindBox.PopupCaption = value; }
		}

		#endregion

		#region IDataBoundControl Members

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (!string.IsNullOrEmpty(dataMember))
			{
				OrganisationFindBox.SetDataBinding(dataSource, dataMember + ZAddress.Schema.OrgPK.Replace(ZAddress.Schema.BindingSuffix, ""));
				AddressDropEdit.SetDataBinding(dataSource, dataMember + ZAddress.Schema.AddressBindingPK.Replace(ZAddress.Schema.BindingSuffix, ""));
			}
			else
			{
				OrganisationFindBox.SetDataBinding(dataSource, ZAddress.Schema.OrgPKColumnName);
				AddressDropEdit.SetDataBinding(dataSource, ZAddress.Schema.AddressFK);
			}

			Extensions.SetDataBinding(dataSource, dataMember);

			if (dataSource != null && ZAddress != null)
			{
				ZAddress.IsOrgVisible = true;
				//WI00238352 - I have no idea why these notifications don't refresh on their own, but this does the trick
				ZAddress.AddressFKInfo.AdditionalValidation += () =>
				{
					var notifs = Extensions.OfType<NotificationExtension>().FirstOrDefault();
					if (notifs != null && ZAddress != null && ZAddress.AddressFKInfo != null && ZAddress.AddressFKInfo.Notifications.Count() != notifs.Notifications.Count())
					{
						notifs.Notifications = ZAddress.AddressFKInfo.Notifications;
						notifs.Redraw();
					}
				};
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (ZAddress != null)
			{
				ZAddress.IsOrgVisible = true;
			}
		}

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return Array.Empty<PropertyDescriptor>();
		}

		#endregion

		#region INotificationDataMembers Members

		string[] INotificationDataMembers.NotificationDataMembers
		{
			get
			{
				//WI00238352 - fix a bug where _ZAddress properties aren't properly binding to the notifications of their underlying property (hopefully?)
				if (DataMember.EndsWith("_ZAddress", StringComparison.Ordinal))
				{
					return new string[] { DataMember.Substring(0, DataMember.Length - "_ZAddress".Length), DataMember };
				}
				return new string[] { DataMember + "." + ZAddress.Schema.AddressFK, DataMember };
			}
		}

		#endregion

		#region IExtendedControl Members

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		#endregion

		#region ReadOnly

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ReadOnly
		{
			get { return OrganisationFindBox.ReadOnly; }
		}

		#endregion

		#region Code Parsing

		internal ZGuid ParseCode(string code)
		{
			ZGuid result = ZGuid.Invalid;
			if (Parse != null)
			{
				result = Parse(code);
			}
			return result;
		}

		/// <summary>
		/// Parsing the string code which is entered into the address drop edit control into guid pk
		/// </summary>
		public Converter<ZString, ZGuid> Parse;

		#endregion

		#region IZAddressParent Members

		void IZAddressParent.SetControlSize()
		{
		}

		ZGuid IZAddressParent.ParseCode(string code)
		{
			return ParseCode(code);
		}

		bool IZAddressParent.CheckZAddressBindingSuffix => false;

		#endregion

		#region Link Button Clicks

#if DEBUG
		public
#endif
 void ContactsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (ZAddress != null && ZAddress.OrgHeader != null)
			{
				IOrganisationController controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
				controller.ShowForm(ZAddress.OrgHeader, OrganisationTabPages.Contacts, FormAction.Edit);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("1A7FBA33-2279-4529-BB8E-99F302D15B5F", "Organization form could not be opened, because valid Organization has not been selected.") + " ");
			}
		}

#if DEBUG
		public
#endif
 void AddressesLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (ZAddress != null && ZAddress.OrgHeader != null)
			{
				IOrganisationController controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
				controller.ShowForm(ZAddress.OrgHeader, OrganisationTabPages.Address, FormAction.Edit);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("1A7FBA33-2279-4529-BB8E-99F302D15B5F", "Organization form could not be opened, because valid Organization has not been selected.") + " ");
			}
		}

		void WebLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string target = e.Link.LinkData as string;

			if (target != null)
			{
				ShowWebLink(target);
			}
		}

		protected void ShowWebLink(string target)
		{
			WebUrlLauncher.Launch(target);
		}

		#endregion

		#region ZAddress

		protected ZAddress ZAddress
		{
			get { return (ZAddress)BindingSource.Current; }
		}

		#endregion

		#region Properties

		[DefaultValue(false)]
		public bool OnlyStopOnDebtor { get; set; }

		#endregion

	}
}
