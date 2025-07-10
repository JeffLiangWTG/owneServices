using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class AgentIndirectWrapper : IParty
{
	public string Name => string.Empty;

	public string Id => string.Empty;

	public string FunctionCode => "3";

	public IAddress Address => null;

	public IContact Contact => null;
}
