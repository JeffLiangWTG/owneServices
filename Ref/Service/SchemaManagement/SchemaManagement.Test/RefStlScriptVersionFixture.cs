using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefStlScriptVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefStlScript
				{
					STL_PK = Guid.NewGuid(),
					STL_FeatureCode = "AAA",
					STL_RoleName = "Role Name",
					STL_ModuleName = "Module Name",
					STL_FunctionName = "Functional Name",
					STL_FeatureName = "Feature Name",
					STL_DataGranularity = "DAY",
					STL_TransactionDateUtc = "2021-09-22 00:00:00.0000000",
					STL_GuidReference = "XXX",
					STL_TransactionCount = "1",
					STL_FromClause = "AAA",
					STL_WhereClause = null,
					STL_WithOptionRecompile = false,
					STL_UsedInBilling = true,
					STL_ActiveOn = "ALL",
					STL_DateType = "DTE"
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefStlScript)data).STL_FeatureName  =  "New Feature Name";
			return true;
		}
	}
}
