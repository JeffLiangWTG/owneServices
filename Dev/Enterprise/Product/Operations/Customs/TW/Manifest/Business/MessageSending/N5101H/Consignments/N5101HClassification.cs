using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HClassification : IClassification
	{
		public N5101HClassification(string id, string identificationTypeCode)
		{
			this.id = id;
			this.identificationTypeCode = identificationTypeCode;
		}

		readonly string id;
		readonly string identificationTypeCode;

		ZString IClassification.ID => id;

		ZString IClassification.IdentificationTypeCode => identificationTypeCode;
	}
}
