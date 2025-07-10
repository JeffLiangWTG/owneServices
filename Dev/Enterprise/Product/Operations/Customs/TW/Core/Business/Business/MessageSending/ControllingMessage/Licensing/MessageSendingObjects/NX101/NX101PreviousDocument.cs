using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX101PreviousDocument : IPreviousDocument
	{
		readonly CusTWControllingMessageHeader header;

		public NX101PreviousDocument(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		public ZString FunctionalReferenceID => null;

		public ZString ID => header.TW1_PrePermitNumber;

		public ZInt LineNumeric => ZInt.Zero;
	}
}
