using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanContainerData))]
	class StowPlanContainerDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIStowPlanContainerDataMember()
		{
			var bill = Factory.New<BillOfLading>();
			var container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "APLU1234";
			container.JC_StowagePosition = "ZZZBBRRRR";
			var packLine1 = bill.OuterPackLines.AddNew();
			packLine1.JL_JC = container.PK;
			packLine1.UNDGs.UNDGSubstanceManagerGuid.Value = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var packLine2 = bill.OuterPackLines.AddNew();
			packLine2.JL_JC = container.PK;
			packLine2.UNDGs.AddNew().SubstancePK = UNDGSubstanceLoader.LoadSubstances(Factory, "0005", "", "IMO").First().PK;
			packLine2.UNDGs.AddNew().SubstancePK = UNDGSubstanceLoader.LoadSubstances(Factory, "0006", "", "IMO").First().PK;

			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "APLU1234";
			container.JC_GrossWeight = 24m;
			container.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "40U2";
			refContainer.RC_Code = "4008";
			container.JC_RC = refContainer.PK;

			var containerData = new StowPlanContainerData(container);
			IStowPlanContainerData data = containerData;
			AssertEquals("APLU1234", data.EquipmentNumber);
			AssertEquals("ZZZBBRRRR", data.StowPosition);
			AssertEquals(24000m, data.GrossWeightInKG);
			AssertEquals(3, data.HazardCodes.Count());
			Assert(data.HazardCodes.Contains("0004a"));
			Assert(data.HazardCodes.Contains("0005"));
			Assert(data.HazardCodes.Contains("0006"));
			AssertEquals("40U2", data.ISOType);
			AssertNotNull(data.Stock);
			var stockData1 = containerData.StockData;
			AssertEquals(stockData1.stock, stock1);
			Assert(containerData.IsRegisteredEditableChildObject(stockData1));

			var stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "APLU4321";
			container.JC_ContainerNum = "APLU4321";
			var stockData2 = containerData.StockData;
			AssertEquals(containerData.StockData.stock, stock2);
			Assert(containerData.IsRegisteredEditableChildObject(stockData2));
			Assert(!containerData.IsRegisteredEditableChildObject(stockData1));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var bill = Factory.New<BillOfLading>();
			return new StowPlanContainerData(bill.RealContainers.AddNew());
		}
	}
}
