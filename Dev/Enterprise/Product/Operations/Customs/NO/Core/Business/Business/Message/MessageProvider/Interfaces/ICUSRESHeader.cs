using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

interface ICUSRESHeader
{
	ZString DeclarationID { get; }

	ZString MessageType { get; }
	ZString MessageFunction { get; }

	ImmutableHashSet<ZString> MessageTextCodes { get; }

	ZString RequestedControlAction { get; }

	ZString ReleaseNumber { get; }

	ZDateTime ReleaseDate { get; }

	ZDateTime LimitDate { get; }

	ZDateTime CreationDate { get; }
}
