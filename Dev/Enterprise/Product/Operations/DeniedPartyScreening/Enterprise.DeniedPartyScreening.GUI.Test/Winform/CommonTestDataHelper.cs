using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public static class CommonTestDataHelper
	{
		public static RefComplianceList CreateComplianceList(BusinessObjectFactory factory, string code, bool isActive = true, bool? isExcluded = null)
		{
			var complianceList = factory.New<RefComplianceList>();
			complianceList.RCL_ListCode = code;
			complianceList.RCL_ListType = code + " Type";
			complianceList.RCL_ListName = code + " Name";
			complianceList.RCL_ListDescription = code + " Description";
			complianceList.RCL_ListPublisher = code + " Publisher";
			complianceList.RCL_IsActive = isActive;
			complianceList.RCL_LastUpdatedDate = new ZDate(2021, 6, 30);

			if (isExcluded.HasValue)
			{
				complianceList.RCL_IsExcluded = isExcluded.Value;
			}

			return complianceList;
		}
	}
}
