using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public static class ColumnsWithAMSTypeHelper
	{
		public static IEnumerable<string> GetElementsToBeHide(string amsType)
		{
			IEnumerable<string> fieldsInColumnToClearOut = null;

			switch (amsType)
			{
				case AMSProgramList.Codes.MO1:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_Packages, AMSLine.Schema.US_PackagesUQ,
						AMSLine.Schema.US_NetWeight, AMSLine.Schema.US_NetWeightUQ, AMSLine.Schema.US_InspecDateTime };
					break;
				case AMSProgramList.Codes.MO2:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_CertNumber, AMSLine.Schema.US_IssueDate, AMSLine.Schema.US_IsDocSubmitted };
					break;
				case AMSProgramList.Codes.MO4:
				case AMSProgramList.Codes.MO6:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_NetWeight, AMSLine.Schema.US_NetWeightUQ };
					break;
				case AMSProgramList.Codes.MO5:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_Packages, AMSLine.Schema.US_PackagesUQ,
						AMSLine.Schema.US_NetWeight, AMSLine.Schema.US_NetWeightUQ, AMSLine.Schema.US_InspecDateTime };
					break;
				case AMSProgramList.Codes.EG1:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_OuterPackage, AMSLine.Schema.US_OuterPackageUQ,
						AMSLine.Schema.US_InnerPackage, AMSLine.Schema.US_InnerPackageUQ, AMSLine.Schema.US_InnerAmount, AMSLine.Schema.US_InnerAmountUQ,
						AMSLine.Schema.US_InnerWeight, AMSLine.Schema.US_InnerWeightUQ, AMSLine.Schema.US_TotalWeight, AMSLine.Schema.US_TotalWeightUQ,
						AMSLine.Schema.US_TotalQuantity, AMSLine.Schema.US_TotalQuantityUQ, AMSLine.Schema.US_InspecDateTime };
					break;
				case AMSProgramList.Codes.EG2:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_PermitNumber };
					break;
				case AMSProgramList.Codes.PN1:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_InspecDateTime, AMSLine.Schema.US_NetWeight,
						AMSLine.Schema.US_NetWeightUQ, AMSLine.Schema.US_Packages, AMSLine.Schema.US_PackagesUQ };
					break;
				case AMSProgramList.Codes.OR1:
					fieldsInColumnToClearOut = new[] { AMSLine.Schema.US_ProductLabel, AMSLine.Schema.US_LotNumber,
						AMSLine.Schema.US_LotEntity, AMSLine.Schema.US_OA_FinalHandler, AMSLine.Schema.US_OA_CerFinalHandler };
					break;
			}

			return fieldsInColumnToClearOut;
		}

		public static void ClearIrrelevantData(BusinessObject bizObj, IEnumerable<string> fieldsToClearOut)
		{
			if (fieldsToClearOut != null)
			{
				foreach (var fieldToClearOut in fieldsToClearOut)
				{
					var info = bizObj.ZPropertyInfoHash.GetPropertySafe(fieldToClearOut);
					if (info != null && !info.Value.IsEmpty)
					{
						info.ClearValue();
					}
				}
			}
		}
	}
}
