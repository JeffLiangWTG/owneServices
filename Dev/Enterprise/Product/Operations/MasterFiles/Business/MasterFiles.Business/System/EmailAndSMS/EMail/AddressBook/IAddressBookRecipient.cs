
using CargoWise.Types;
namespace Enterprise.MasterFiles.Business
{
	public interface IAddressBookRecipient
	{
		OrgHeader Organisation { get; }
		ZString Title { get; }
		ZString Location { get; } // location isn't always the Organisation's MainAddress (e.g. Branch)
		ZString Phone { get; }
		bool IsActive { get; }
		ZString Role { get; }
		ZString Name { get; }
		ZString Email { get; }
		ZGuid PK { get; }
	}
}
