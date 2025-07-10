using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineFetchStrategyTest : TestCaseWithFactory
	{
		public void TestJL_Calc_DGClassFetchHints()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "SDFG5656543";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "IUUF4475774";

			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			var undg1 = pack1.UNDGs.AddNew();
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "1001";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			var undg2 = pack2.UNDGs.AddNew();

			var subs2 = UNDGSubstanceLoader.LoadSubstances(Factory, "1002", "", "IMO").FirstOrDefault();
			if (subs2 == null)
			{
				Factory.New<UNDGSubstance>();
				subs2.DG_Code = "1002";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			undg2.DI_DG = subs2.PK;
			undg2.LinkDefault(subs2);
			var undg3 = pack2.UNDGs.AddNew();
			undg3.DI_DG = subs.PK;
			undg3.LinkDefault(subs);

			container1.AddPackLine(pack1);
			container2.AddPackLine(pack2);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var packLine1 = factory2.Load<PackLine>(pack1.PK);
			var packLine2 = factory2.Load<PackLine>(pack2.PK);
			AssertEquals("UNDGDataItem is not hit on load", 0, factory2.GetTableHitCount(UNDGDataItemSchema.Constants.TableName));

			AssertEquals("2.1", packLine1.JL_Calc_DGClass);
			AssertEquals("Mixed", packLine2.JL_Calc_DGClass);

			AssertEquals("UNDGDataItem is hit once only", 1, factory2.GetTableHitCount(UNDGDataItemSchema.Constants.TableName));
		}

		public void TestJL_Calc_DGSubstanceFetchHints()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "SDFG5656543";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "IUUF4475774";

			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			var undg1 = pack1.UNDGs.AddNew();
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_Code = "1001";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			undg1.LinkDefault(subs);
			var undg2 = pack2.UNDGs.AddNew();
			var subs2 = UNDGSubstanceLoader.LoadSubstances(Factory, "1002", "", "IMO").FirstOrDefault();
			if (subs2 == null)
			{
				Factory.New<UNDGSubstance>();
				subs2.DG_Code = "1002";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}
			undg2.LinkDefault(subs2);

			container1.AddPackLine(pack1);
			container2.AddPackLine(pack2);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			AssertEquals("Pre-condition", 0, factory2.GetTableHitCount(UNDGDataItemSchema.Constants.TableName));

			var packLine1 = factory2.Load<PackLine>(pack1.PK);
			var packLine2 = factory2.Load<PackLine>(pack2.PK);
			AssertEquals("UNDGDataItem is not hit on load", 0, factory2.GetTableHitCount(UNDGDataItemSchema.Constants.TableName));

			AssertEquals("1001", packLine1.JL_Calc_DGSubstance);
			AssertEquals("1002", packLine2.JL_Calc_DGSubstance);

			AssertEquals("UNDGDataItem is hit once only", 1, factory2.GetTableHitCount(UNDGDataItemSchema.Constants.TableName));
		}
	}
}
