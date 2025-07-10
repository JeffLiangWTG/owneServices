using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class MessageRecipientParty
	{
		public MessageRecipientParty(OrgHeader party, ZString fallbackEmail)
			: this(party, fallbackEmail, ZString.Empty)
		{ }

		public MessageRecipientParty(OrgHeader party, ZString fallbackEmail1, ZString fallbackEmail2)
		{
			Party = party;
			FallbackEmail = fallbackEmail1;
			if (FallbackEmail.IsEmpty)
			{
				FallbackEmail = fallbackEmail2;
			}
		}

		public MessageRecipientParty(OrgAddress address)
		{
			if (address != null && address.Header != null)
			{
				Party = address.Header;
				FallbackEmail = address.OA_Email;
			}
		}

		public MessageRecipientParty(JobDocAddress docAddress)
		{
			if (docAddress != null && docAddress.HasRealOrganisation)
			{
				Party = docAddress.Organisation;

				FallbackEmail = docAddress.E2_Email;

				if (FallbackEmail.IsEmpty && docAddress.Contact != null)
				{
					FallbackEmail = docAddress.Contact.OC_Email;
				}

				if (FallbackEmail.IsEmpty && docAddress.HasRealAddress)
				{
					FallbackEmail = docAddress.Address.OA_Email;
				}
			}
		}

		public OrgHeader Party { get; private set; }
		public ZString FallbackEmail { get; private set; }
	}

	public class MessageRecipientPartyCollection : IEnumerable<MessageRecipientParty>
	{
		internal MessageRecipientPartyCollection()
		{
		}

		readonly List<MessageRecipientParty> list = new List<MessageRecipientParty>();

		public void AddNotNullAndNotDuplicatedItem(MessageRecipientParty item)
		{
			if (item != null && item.Party != null && list.All(el => el.Party != item.Party))
			{
				list.Add(item);
			}
		}

		public void AddRangeNotNullAndNotDuplicatedItems(MessageRecipientParty[] items)
		{
			foreach (var item in items)
			{
				AddNotNullAndNotDuplicatedItem(item);
			}
		}

		public MessageRecipientParty[] ToArray()
		{
			return list.ToArray();
		}

		#region IEnumerable<OrgHeader> Members

		public IEnumerator<MessageRecipientParty> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}
}
