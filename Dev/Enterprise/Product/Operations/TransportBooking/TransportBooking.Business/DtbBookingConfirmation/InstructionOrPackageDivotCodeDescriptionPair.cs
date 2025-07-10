using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Business
{
	public class InstructionOrPackageDivotCodeDescriptionPair : CodeDescriptionPair, ICodeDescription, IIdentified
	{
		public InstructionOrPackageDivotCodeDescriptionPair(ZGuid pk, string code, MultilingualString description)
			: base(code, description)
		{
			Identifier = pk;
		}

		public ZGuid Identifier { get; }

		object ICodeDescription.PK => Identifier;

		public override bool Equals(object obj)
		{
			var codeDescription = obj as ICodeDescription;

			return codeDescription != null && Identifier == (ZGuid)codeDescription.PK;
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}
	}
}
