using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class AgentDirectWrapper : IParty
{
	public AgentDirectWrapper(OrgHeader party)
	{
		this.party = Argument.NotNull(party, nameof(party));
	}
	readonly OrgHeader party;

	public string Name => string.Empty;

	public string Id => party.GetEORI();

	public string FunctionCode => "2";

	public IAddress Address => null;

	public IContact Contact => null;
}
