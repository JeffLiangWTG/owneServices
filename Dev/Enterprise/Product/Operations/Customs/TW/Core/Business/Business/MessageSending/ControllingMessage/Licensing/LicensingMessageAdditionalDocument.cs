using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageAdditionalDocument : IDeclarationAdditionalDocument
	{
		readonly CusTWControllingMessageHeader header;

		public LicensingMessageAdditionalDocument(CusTWControllingMessageHeader header)
		{
			this.header = header;
		}

		ZString IDeclarationAdditionalDocument.ID => header.PermitNumber;

		ZDateTime IDeclarationAdditionalDocument.LPCOExpirationDateTime => header.PermitNoExpirationDate;
	}
}
