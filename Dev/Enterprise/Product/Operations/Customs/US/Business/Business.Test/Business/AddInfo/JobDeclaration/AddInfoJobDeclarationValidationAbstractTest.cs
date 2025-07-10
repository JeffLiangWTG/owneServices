using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	abstract class AddInfoJobDeclarationValidationAbstractTest : BusinessObjectValidationTestCase
	{
		protected void CreateTestLocoMapping()
		{
			CreateLocoMapIfNotExists("2701", "USLAX", USLocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMapIfNotExists("2702", "USLAX", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMapIfNotExists("2970", "USLAX", USLocoMapSystemUsageList.Codes.SCD, false);
			CreateLocoMapIfNotExists("2973", "USLAX", USLocoMapSystemUsageList.Codes.SCD, false);
			CreateLocoMapIfNotExists("2974", "USLAX", USLocoMapSystemUsageList.Codes.Air);
			CreateLocoMapIfNotExists("2775", "USLAX", USLocoMapSystemUsageList.Codes.Air);
			CreateLocoMapIfNotExists("2776", "USLAX", USLocoMapSystemUsageList.Codes.Air);
			CreateLocoMapIfNotExists("2791", "USLAX", USLocoMapSystemUsageList.Codes.Air);
			CreateLocoMapIfNotExists("2792", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists("29213", "BBBGI", USLocoMapSystemUsageList.Codes.SCK, false);
			CreateLocoMapIfNotExists("29201", "BBBGI", USLocoMapSystemUsageList.Codes.SCK, false);
			CreateLocoMapIfNotExists("29210", "BBBGI", USLocoMapSystemUsageList.Codes.SCK);
			Factory.Save();
		}

		protected void CreateLocoMapIfNotExists(string localPort, string unLoco, string usage, bool isSystem = false)
		{
			ZQuery codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			if (isSystem)
			{
				codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);
			}

			var locoMap = Factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = Factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}
		}

		JobDeclaration jobDeclaration;
		protected JobDeclaration Declaration => jobDeclaration ?? (jobDeclaration = Factory.New<JobDeclaration>());
	}
}
