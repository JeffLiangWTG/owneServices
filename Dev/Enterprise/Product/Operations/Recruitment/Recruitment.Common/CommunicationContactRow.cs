using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Recruitment.Common
{
	public class CommunicationContactRow : NonPersistentBusinessObject
	{
		public CommunicationContactRow(CommunicationContact contact)
			: base(contact.Factory)
			=> Contact = contact;

		CommunicationContactRow(BusinessObjectFactory factory)
			: base(factory)
		{
			Contact = CommunicationContact.CreateUncommittedRow(factory);
			Selected = true;
		}

		public static CommunicationContactRow CreateUncommittedRow(BusinessObjectFactory factory)
			=> new CommunicationContactRow(factory);

		public ZBool Selected
		{
			get => selected;
			set => SetNonPersistentPropertyValue(SelectedInfo, ref selected, value);
		}
		ZBool selected;

		public ZPropertyInfo SelectedInfo => GetZPropertyInfo(nameof(Selected));

		public CommunicationContact Contact { get; }
	}
}
