using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class EmailContactItemLookups : ContactItemProxyLookups
	{
		public EmailContactItemLookups(EmailContactItem emailContactItem)
			: base(emailContactItem)
		{
		}

		new EmailContactItem Parent
		{
			get { return (EmailContactItem)base.Parent; }
		}

		public override CodeDescriptionPairList DescriptionList
		{
			get { return new EmailContactItemDescriptionList(); }
		}

		public override CodeDescriptionPairList SelectableDescriptionList
		{
			get
			{
				var result = new EmailContactItemDescriptionList();
				var contact = Parent.Contact;
				if (contact != null)
				{
					foreach (var description in contact.EmailContactItems.Items.Select(item => item.OI_Description))
					{
						if (!description.EqualsIgnoringCase(Parent.OI_Description))
						{
							result.RemoveCode(description);
						}
					}
				}

				return result;
			}
		}
	}
}
