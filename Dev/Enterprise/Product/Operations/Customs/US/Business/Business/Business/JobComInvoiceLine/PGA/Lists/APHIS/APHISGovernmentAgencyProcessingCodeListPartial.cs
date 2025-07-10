using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class APHISGovernmentAgencyProcessingCodeList
	{
		public static ICodeDescriptionPairList GetListForProgram(BusinessObjectFactory factory, ZString programCode)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("APHISGovernmentAgencyProcessingCodeList_" + programCode, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (programCode)
					{
						case APHISProgramCodeList.Codes.ABS:
							result.AddPair(Codes.CBPAgriculture, Descriptions.CBPAgriculture);
							result.AddPair(Codes.APHISPlantInspectionStation, Descriptions.APHISPlantInspectionStation);
							break;
						case APHISProgramCodeList.Codes.APQ:
							result.AddPair(Codes.CBPAgriculture, Descriptions.CBPAgriculture);
							result.AddPair(Codes.APHISPlantInspectionStation, Descriptions.APHISPlantInspectionStation);
							result.AddPair(Codes.APHISPreClearance, Descriptions.APHISPreClearance);
							break;
						case APHISProgramCodeList.Codes.AAC:
						case APHISProgramCodeList.Codes.AVS:
							result.AddPair(Codes.CBPAgriculture, Descriptions.CBPAgriculture);
							result.AddPair(Codes.APHISVSPortVeterinarian, Descriptions.APHISVSPortVeterinarian);
							result.AddPair(Codes.APHISVSAnimalImportCenter, Descriptions.APHISVSAnimalImportCenter);
							break;
					}
					return result;
				});
		}
	}
}
