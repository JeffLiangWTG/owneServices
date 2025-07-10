using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanContainerStockData))]
	class StowPlanContainerStockDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIStowPlanContainerStockDataMembers()
		{
			var stock = Factory.New<RefContainerStock>();
			var owner = Factory.New<OrgHeader>();
			owner.OH_Code = "OWNER";
			owner.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			stock.R6_OH_Owner = owner.PK;
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_ISOType = "ISO";
			stock.R6_RC = refContainer.PK;

			var stockData = new StowPlanContainerStockData(stock);
			AssertEquals("SCAC", stockData.ContainerOperator);
			AssertEquals("ISO", stockData.EquipmentSizeType);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StowPlanContainerStockData(Factory.New<RefContainerStock>());
		}
	}
}
