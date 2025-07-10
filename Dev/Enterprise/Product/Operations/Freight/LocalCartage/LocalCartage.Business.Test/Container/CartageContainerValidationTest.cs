using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageContainerValidationTest : BaseFreightTest
	{
		[TestDate(2009, 11, 11, 9, 40, 0)]
		public void TestValidateJC_ContainerNum_UsedOnAnotherCartage_IsViewingFromPortTransportLegPlanner()
		{
			var cartage1 = Factory.New<CommonCartage>();
			var container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			container1.JC_ContainerNum = "abcd1234560";
			var container2 = Factory.New<CommonContainer>();
			container2.JC_ContainerNum = "abcd1234560";
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = Factory.New<CommonBookedCtgMove>();
			move2.EW_JC_Container = container2.PK;
			cartage2.ContainerBookedMoves.Add(move2);
			Factory.Save();
			container2.Validation.ValidateJC_ContainerNum();
			AssertHasWarnings("should have this container no is used on another cartage warning because this container does have a cartage", container2.JC_ContainerNumInfo);
			var newFactory = new BusinessObjectFactory();
			FactoryCacheHelper.SetIsViewingFromPortTransportLegPlanner(newFactory);
			var cartage2_2 = newFactory.Load<CommonCartage>(cartage2.PK);
			cartage2_2.Containers.First().Validation.ValidateJC_ContainerNum();
			AssertNoWarnings("validation is disabled since FromCartageLegPlannerControl is ture in in new factory cache.", container1.JC_ContainerNumInfo);
		}

		[TestDate(2009, 11, 11, 9, 40, 0)]
		public void TestValidateJC_ContainerNum_UsedOnAnotherCartage()
		{
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			var container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			container1.JC_ContainerNum = "abcd1234560";
			Factory.Save();
			container1.Validation.ValidateJC_ContainerNum();
			AssertNoWarnings(container1.JC_ContainerNumInfo);
			CommonContainer container2 = Factory.New<CommonContainer>();
			container2.JC_ContainerNum = "abcd1234560";
			Factory.Save();
			container1.Validation.ValidateJC_ContainerNum();
			AssertNoWarnings("should NOT have this container no is used on another cartage warning because the other container is not on a cartage", container1.JC_ContainerNumInfo);
			container2.Validation.ValidateJC_ContainerNum();
			AssertNoWarnings("should NOT have this container no is used on another cartage warning because this container doesn't have a cartage", container2.JC_ContainerNumInfo);
			CommonCartage cartage2 = Factory.New<CommonCartage>();
			var move2 = Factory.New<CommonBookedCtgMove>();
			move2.EW_JC_Container = container2.PK;
			cartage2.ContainerBookedMoves.Add(move2);
			Factory.Save();
			container2.Validation.ValidateJC_ContainerNum();
			AssertHasWarnings("should have this container no is used on another cartage warning because this container does have a cartage", container2.JC_ContainerNumInfo);
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(7).ToDateTime();
			container2.Validation.ValidateJC_ContainerNum();
			AssertNoWarnings("should not a warning because the cartage is more than 6 months old", container2.JC_ContainerNumInfo);
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
