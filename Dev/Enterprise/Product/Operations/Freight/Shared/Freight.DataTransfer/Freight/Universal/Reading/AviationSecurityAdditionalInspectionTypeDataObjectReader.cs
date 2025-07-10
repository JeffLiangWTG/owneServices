using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.DataTransfer
{
	public class AviationSecurityAdditionalInspectionTypeDataObjectReader : AviationSecurityInspectionTypeDataObjectReader
	{
		public AviationSecurityAdditionalInspectionTypeDataObjectReader(CodeDescriptionPair inspectionTypeDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ISupportInspectionType inspectionType, Logs logs, bool isInDatabase, string reason = null)
			: base(inspectionTypeDataObject, logger, factory, inspectionType, logs, isInDatabase, reason)
		{
			AdditionalInspectionType = inspectionType.AdditionalInspectionType;
		}

		protected CusEntryNumber AdditionalInspectionType;
		protected override string Type => (EZC.NoResString)"Additional Inspection";

		protected override CusEntryNumber GetExistingBusinessObject()
		{
			return AdditionalInspectionType;
		}
	}
}
