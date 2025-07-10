using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AddressBookRecipientCollection : NonPersistentBusinessObjectCollection<AddressBookRecipient>
	{
		public AddressBookRecipientCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			return base.ElementCanBeAdded(bizO) && !Contains((AddressBookRecipient)bizO);
		}

		public new bool Contains(BusinessObject businessObject)
		{
			if (!(businessObject is AddressBookRecipient) && businessObject is IAddressBookRecipient)
			{
				return this.Any(recipient => ((IAddressBookRecipient)recipient).PK == businessObject.PK);
			}
			return base.Contains(businessObject);
		}

		public void ReplaceRecipients(IEnumerable recipients)
		{
			RemoveAll();
			AddRange(recipients);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
