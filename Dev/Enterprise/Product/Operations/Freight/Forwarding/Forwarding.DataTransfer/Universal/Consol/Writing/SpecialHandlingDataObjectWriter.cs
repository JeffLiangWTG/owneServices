using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	sealed class SpecialHandlingDataObjectWriter : DataObjectWriter<NonSecurityJobConsolAWBSpecialHandling, CodeDescriptionPair>
	{
		public SpecialHandlingDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override CodeDescriptionPair PopulateDataObject(NonSecurityJobConsolAWBSpecialHandling sourceBO)
		{
			var specialHandlingDataObject = new CodeDescriptionPair();
			specialHandlingDataObject.Code = sourceBO.JKH_Code;
			specialHandlingDataObject.Description = new AWBSpecialHandlingCodeDescriptionPairList().GetDescriptionFromCode(specialHandlingDataObject.Code);

			return specialHandlingDataObject;
		}
	}
}
