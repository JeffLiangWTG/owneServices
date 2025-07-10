using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class ClassificationWrapper : IClassification
	{
		readonly ZString id;
		readonly ZString identificationTypeCode;

		public ClassificationWrapper(ZString id, ZString identificationTypeCode)
		{
			this.id = id;
			this.identificationTypeCode = identificationTypeCode;
		}

		ZString IClassification.ID => id;

		ZString IClassification.IdentificationTypeCode => identificationTypeCode;
	}
}
