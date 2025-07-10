using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DocDeliveryContactCollection : NonPersistentBusinessObjectCollection<DocDeliveryContact>
	{
		public DocDeliveryContactCollection(BusinessObjectFactory factory)
			: base(factory ?? new BusinessObjectFactory())
		{
		}

		public DocDeliveryContactCollection(IStmMenuItem menuItem, IDocAddress overridenAddress, BusinessObjectFactory factory)
			: this(factory)
		{
			this.menuItem = menuItem;
			this.overridenAddress = overridenAddress;
		}

		public DocDeliveryContactCollection(IStmMenuItem menuItem, IDocAddress overridenAddress, BusinessObjectFactory factory, DocAutoDelivery docAutoDelivery)
			: this(menuItem, overridenAddress, factory)
		{
			this.docAutoDelivery = docAutoDelivery;
		}

		readonly IStmMenuItem menuItem;
		readonly IDocAddress overridenAddress;
		readonly DocAutoDelivery docAutoDelivery;

		CodeDescriptionPairList overriddenAttachmentTypeList;

		CodeDescriptionPairList overridenNotifyModeTypeList;

		public BusinessObjectCollection Deliverables { get; set; }

		public ZString DefaultEmailFromAddress { get; set; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocDeliveryContact(Factory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newContact = child as DocDeliveryContact;
			if (newContact != null)
			{
				newContact.Initialise(menuItem, overridenAddress, overriddenAttachmentTypeList, overridenNotifyModeTypeList, deliveryLanguage, attachmentTypeDisabled, docAutoDelivery, DefaultEmailFromAddress);
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var newContact = bizOAdded as DocDeliveryContact;
			if (newContact != null)
			{
				newContact.Initialise(menuItem, overridenAddress, overriddenAttachmentTypeList, overridenNotifyModeTypeList, deliveryLanguage, attachmentTypeDisabled, docAutoDelivery, DefaultEmailFromAddress);
			}
		}

		/// <summary>
		/// Update all existing contacts' delivery modes list, as well as for all new contacts added.
		/// </summary>
		/// <param name="list">The list to use instead of the system default.</param>
		public void OverrideAttachmentTypeListOnChildren(CodeDescriptionPairList list)
		{
			overriddenAttachmentTypeList = list;
			foreach (DocDeliveryContact contact in this)
			{
				contact.AttachmentTypes = list;
			}
		}

		public void OverrideNotifyModeTypeListOnChildren(CodeDescriptionPairList list)
		{
			overridenNotifyModeTypeList = list;
			foreach (DocDeliveryContact contact in this)
			{
				contact.NotifyModes = list;
			}
		}

		public void DisableAttachmentType(bool disable)
		{
			attachmentTypeDisabled = disable;
			if (disable)
			{
				OverrideAttachmentTypeListOnChildren(new CodeDescriptionPairList());
			}
		}
		bool attachmentTypeDisabled;

		public void SetDeliveryLanguage(ZString language)
		{
			deliveryLanguage = language;
			foreach (DocDeliveryContact item in this)
			{
				item.DeliveryLanguage = language;
			}
		}

		ZString deliveryLanguage;
	}
}
