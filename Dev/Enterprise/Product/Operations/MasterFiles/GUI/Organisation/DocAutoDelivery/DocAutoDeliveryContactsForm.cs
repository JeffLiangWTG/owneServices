using System.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class DocAutoDeliveryContactsForm : ZChildForm
	{
		public DocAutoDeliveryContactsForm()
		{
		}

		public DocAutoDeliveryContactsForm(DocAutoDeliveryContactViewer contactViewer) : base(contactViewer)
		{
			this.ContactViewer = contactViewer;
		}

		#region Implementation

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataBindings.RemoveBinding(nameof(RelatedPartyIsVisible));
				DataBindings.RemoveBinding(nameof(LocalPortIsVisible));
				DataBindings.RemoveBinding(nameof(ForeignPortIsVisible));
				DataBindings.RemoveBinding(nameof(RelatedPartyLabelText));
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				DataBindings.Add(new KBinding(nameof(RelatedPartyIsVisible), DataSource, "DocContactTypeIsConsign"));
				DataBindings.Add(new KBinding(nameof(LocalPortIsVisible), DataSource, "DocContactTypeIsConsign"));
				DataBindings.Add(new KBinding(nameof(ForeignPortIsVisible), DataSource, "DocContactTypeIsConsign"));
				DataBindings.Add(new KBinding(nameof(RelatedPartyLabelText), DataSource, "DocContactType"));
			}
		}

		#region Properties for binding

		#region RelatedPartyIsVisible

		public ZBool RelatedPartyIsVisible
		{
			get { return RelatedPartyGuidFindBox.Visible; }
			set
			{
				RelatedPartyGuidFindBox.Visible = value;
			}
		}

		#endregion

		#region LocalPortIsVisible

		public ZBool LocalPortIsVisible
		{
			get { return LocalPortFindBox.Visible; }
			set
			{
				LocalPortFindBox.Visible = value;
			}
		}

		#endregion

		#region ForeignPortIsVisible

		public ZBool ForeignPortIsVisible
		{
			get { return ForeignPortFindBox.Visible; }
			set
			{
				ForeignPortFindBox.Visible = value;
			}
		}

		#endregion

		#region RelatedPartyLabelText

		protected ZString fDocContactType;
		public ZString RelatedPartyLabelText
		{
			get { return RelatedPartyGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption; }
			set
			{
				fDocContactType = value;
				if (fDocContactType == ContactType.Consignee.Code)
				{
					RelatedPartyGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("DocAutoDeliveryContactsForm|RelatedConsignor", "Related Consignor");
				}
				else if (fDocContactType == ContactType.Consignor.Code)
				{
					RelatedPartyGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("DocAutoDeliveryContactsForm|RelatedConsignee", "Related Consignee");
				}
				else
				{
					RelatedPartyGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption = "";
				}
			}
		}

		#endregion

		#endregion

		#endregion

		#region Button Clicks

		void AutoDeliverButton_Click(object sender, System.EventArgs e)
		{
			if (ContactViewer.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("a4df6eed-9080-4b36-b025-edc81875ee8f", "Please correct any errors before viewing auto-delivery contacts."));
			}
			else
			{
				ContactViewer.GenerateAutoDeliveryContacts();
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#endregion

		readonly DocAutoDeliveryContactViewer ContactViewer;

		#endregion

		#region Metadata

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<DocAutoDeliveryContactsForm>()
				.Property("ForeignPortIsVisible", ZBool.True, false)
				.Property("LocalPortIsVisible", ZBool.True, false)
				.Property("RelatedPartyIsVisible", ZBool.True, false)
				.Property("RelatedPartyLabelText", ZString.Empty, false)
				.Result;
		}

		#endregion
	}
}
