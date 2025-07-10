using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(RefContainerStock))]
	internal class RefContainerStockTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLastMovement()
		{
			ZDateTime today = ZDateTime.Today;
			AssertEquals("No Movements", null, Stock1.LastMovement);
			ContainerMovement movement1 = Stock1.Movements.AddNew();
			movement1.E9_MovementDate = today.AddDays(-4);
			ContainerMovement movement2 = Stock1.Movements.AddNew();
			movement2.E9_MovementDate = today.AddDays(-3);
			ContainerMovement movement3 = Stock1.Movements.AddNew();
			ContainerMovement movement4 = Stock2.Movements.AddNew();
			movement4.E9_MovementDate = today.AddDays(-2);
			AssertEquals(movement2, Stock1.LastMovement);
			movement3.E9_MovementDate = today.AddDays(-2);
			AssertEquals(movement3, Stock1.LastMovement);
		}

		public void TestMovementsFilter()
		{
			RefContainerStock stock = Factory.New<RefContainerStock>();
			ContainerMovement movement1 = stock.Movements.AddNew();
			movement1.E9_MovementType = ContainerMovementTypes.Codes.WharfGateOut;
			movement1.E9_OtherLocation = "Movement 1";
			ContainerMovement movement2 = stock.Movements.AddNew();
			movement2.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement2.E9_OtherLocation = "Movement 2";
			stock.Filter.MovementType = "";
			AssertContainsExactElementsInAnyOrder("Movements[]", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement1, movement2 }, stock.Movements.ToArray());
			AssertContainsExactElementsInAnyOrder("Filter.Movements[]", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement1, movement2 }, stock.Filter.Movements.ToArray());
			stock.Filter.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			stock.Filter.Find();
			AssertContainsExactElementsInAnyOrder("Movements[WGI]", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement1, movement2 }, stock.Movements.ToArray());
			AssertContainsExactElementsInAnyOrder("Filter.Movements[WGI]", (m) => m.E9_OtherLocation, new ContainerMovement[] { movement2 }, stock.Filter.Movements.ToArray());
		}

		public void TestLoader()
		{
			RefContainer containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			RefContainerStock container1 = Factory.New<RefContainerStock>();
			container1.R6_ContainerNum = "FAKU4100011";
			container1.R6_RC = containerType.PK;
			RefContainerStock container2 = Factory.New<RefContainerStock>();
			container2.R6_ContainerNum = "FAKU4100027";
			container2.R6_RC = containerType.PK;
			Factory.Save();
			AssertEquals(container1, RefContainerStock.Load(Factory, "FAKU4100011"));
			AssertEquals(null, RefContainerStock.Load(Factory, "FAKU4100032"));
		}

		public void TestCheckConstraintsForR6_OwnerType()
		{
			var containerOwnershipList = new ContainerOwnershipList();
			containerOwnershipList.AddPairIfNotExist(string.Empty, string.Empty);
			foreach (CodeDescriptionPair item in containerOwnershipList)
			{
				var stock = Factory.NewWithValidTestData<RefContainerStock>();
				stock.R6_OwnerType = item.Code;
				AssertNoExceptionThrown(item.Code, () => Factory.Save());
			}
		}

		public void TestContainerNumberReadOnly()
		{
			Stock1.R6_RC = Factory.LoadTop1<RefContainer>(new ZQuery()).PK;
			AssertEquals(false, Stock1.R6_ContainerNumInfo.ReadOnly);
			Stock1.Factory.Save();
			AssertEquals(true, Stock1.R6_ContainerNumInfo.ReadOnly);
		}

		public void TestISOType()
		{
			Stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			AssertEquals("Stock1.R6_RC_ISOType", "22G0", Stock1.R6_RC_ISOType);
			AssertEquals("Stock1.R6_RC_ISOType", "42R0", Stock2.R6_RC_ISOType);
		}

		public void TestIsHighCube()
		{
			Stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC").PK;
			Stock1.Container.RC_IsHighCube = false;
			Stock2.Container.RC_IsHighCube = true;
			AssertEquals("Stock1.R6_RC_IsHighCube", false, Stock1.R6_RC_IsHighCube);
			AssertEquals("Stock2.R6_RC_IsHighCube", true, Stock2.R6_RC_IsHighCube);
		}

		public void TestHasTynes()
		{
			Stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Stock1.Container.RC_HasTynes = false;
			Stock2.Container.RC_HasTynes = true;
			AssertEquals("Stock1.R6_RC_HasTynes", false, Stock1.R6_RC_HasTynes);
			AssertEquals("Stock2.R6_RC_HasTynes", true, Stock2.R6_RC_HasTynes);
		}

		public void TestHasVents()
		{
			Stock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Stock2.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			Stock1.Container.RC_HasVents = false;
			Stock2.Container.RC_HasVents = true;
			AssertEquals("Stock1.R6_RC_HasVents", false, Stock1.R6_RC_HasVents);
			AssertEquals("Stock2.R6_RC_HasVents", true, Stock2.R6_RC_HasVents);
		}

		#region ContainerNumberIsNotUnique
		public void TestContainerNumberIsNotUniqueReturnsValueNotInDb()
		{
			var stock1 = Factory.New<RefContainerStock>();
			stock1.R6_ContainerNum = "AAAA9999999";
			stock1.R6_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			Factory.Save();
			var stock2 = Factory.New<RefContainerStock>();
			stock2.R6_ContainerNum = "AAAA9999999";
			stock2.R6_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			var refContainerStock = RefContainerStock.Load(Factory, "AAAA9999999");
			AssertEquals("stock2 should be loaded (it is not in the database)", stock2.PK, refContainerStock.PK);
		}

		#endregion
		#region Implementation
		RefContainerStock Stock1
		{
			get
			{
				if (stock1 == null)
				{
					stock1 = Factory.New<RefContainerStock>();
					stock1.R6_ContainerNum = "Stock1";
				}

				return stock1;
			}
		}

		RefContainerStock stock1;
		RefContainerStock Stock2
		{
			get
			{
				if (stock2 == null)
				{
					stock2 = Factory.New<RefContainerStock>();
					stock2.R6_ContainerNum = "Stock2";
				}

				return stock2;
			}
		}

		RefContainerStock stock2;
		#endregion
	}
}
