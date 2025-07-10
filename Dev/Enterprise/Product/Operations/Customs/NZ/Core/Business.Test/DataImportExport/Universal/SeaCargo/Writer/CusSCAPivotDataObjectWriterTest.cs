using System.Linq;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class CusSCAOceanBillDataObjectWriterTest
	{
		protected override CusSCAPackingLine AddPackage1(CusSCAContainer container, CusSCAHouse bill)
		{
			var result = base.AddPackage1(container, bill);
			var undg = result.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").First().PK;
			return result;
		}

		protected override CusSCAPackingLine AddPackage2(CusSCAContainer container, CusSCAHouse bill)
		{
			var result = base.AddPackage2(container, bill);
			var undg = result.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1002", "", "IMO").First().PK;
			return result;
		}
	}
}
