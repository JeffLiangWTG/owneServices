using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101AdditionalDeclarationWrapper : IAdditionalDeclaration
	{
		readonly ZDecimal sequenceNumeric;

		readonly ZString id;

		public NX101AdditionalDeclarationWrapper(ZDecimal sequenceNumeric, ZString id)
		{
			this.sequenceNumeric = sequenceNumeric;
			this.id = id;
		}

		ZString IAdditionalDeclaration.ID => id;

		ZString IAdditionalDeclaration.TypeCode => null;

		ZDecimal IAdditionalDeclaration.SequenceNumeric => sequenceNumeric;
	}
}
