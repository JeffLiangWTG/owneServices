using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class PartyWrapper : IParty
{
	protected PartyWrapper(OrgHeader party)
	{
		this.party = party;
	}
	readonly OrgHeader party;

	public string Id => CachedValueHelper.GetValue(ref identificationNumber, party.GetIdentificationNumber);
	CachedValue<string> identificationNumber;

	public string Name => CachedValueHelper.GetValue(ref name, GetNameCore);
	CachedValue<string> name;
	string GetNameCore() => string.IsNullOrEmpty(Id) ? (string)party.OH_FullName : null;

	public IAddress Address => CachedValueHelper.GetValue(ref address, GetAddressCore);
	CachedValue<IAddress> address;
	IAddress GetAddressCore() => (string.IsNullOrEmpty(Id) && !string.IsNullOrEmpty(Name)) ? new AddressWrapper(party.MainAddress) : null;

	public string FunctionCode => string.Empty;

	public IContact Contact => ContactCore;

	protected virtual IContact ContactCore => party.GetActiveContacts().FirstOrDefault() is OrgContact orgContact ? new ContactWrapper(orgContact) : null;

	public static PartyWrapper New(OrgHeader party) =>
		party == null ? null : new PartyWrapper(party);
}
