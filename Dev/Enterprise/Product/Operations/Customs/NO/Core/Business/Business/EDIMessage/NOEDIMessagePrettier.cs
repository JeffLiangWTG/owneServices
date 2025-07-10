using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class NOEDIMessagePrettier(NOEDIMessage message) : IEDIMessagePrettier
{
	readonly NOEDIMessage message = Argument.NotNull(message, nameof(message));

	public ZString MakeHumanReadable() => message.EM_MessageText;
}
