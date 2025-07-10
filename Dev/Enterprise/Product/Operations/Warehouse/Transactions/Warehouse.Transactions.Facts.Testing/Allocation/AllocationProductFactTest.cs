using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class AllocationProductFactTest : ProductFactTest
	{
		#region TestIsDynamic

		public void TestIsDynamic_False() => TestIsDynamic(isDynamic: false);
		public void TestIsDynamic_True() => TestIsDynamic(isDynamic: true);

		void TestIsDynamic(bool isDynamic)
		{
			var factory = new BusinessObjectFactory();
			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new AllocationProductFact(factory.New<OrgSupplierPart>(), relationMock.Object, isDynamic, true, "Style1", "M", "RED", "SML");
			AssertEquals(nameof(AllocationProductFact.IsDynamic), isDynamic, productFact.IsDynamic);
		}

		#endregion

		#region TestHasPalletDefined

		public void TestHasPalletDefined() => TestHasPalletDefined(unitsPerPallet: 5, hasPalletDefined: true);
		public void TestHasPalletDefined_OneToOne() => TestHasPalletDefined(unitsPerPallet: 1, hasPalletDefined: true);
		public void TestHasPalletDefined_False() => TestHasPalletDefined(unitsPerPallet: 0, hasPalletDefined: false);

		void TestHasPalletDefined(int unitsPerPallet, bool hasPalletDefined)
		{
			var factory = new BusinessObjectFactory();
			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);

			var product = factory.New<OrgSupplierPart>();
			new WhsTestHelperFunctionsEnv(factory).CreateProductUnit(product, "PLT", unitsPerPallet);

			var productFact = new AllocationProductFact(product, relationMock.Object, false, true, "Style1", "M", "RED", "SML");
			AssertEquals(nameof(AllocationProductFact.HasPalletDefined), hasPalletDefined, productFact.HasPalletDefined);
		}

		#endregion

		#region TestHasPickFaces

		public void TestHasPickFaces_False() => TestHasPickFaces(hasPickFaces: false);
		public void TestHasPickFaces_True() => TestHasPickFaces(hasPickFaces: true);

		void TestHasPickFaces(bool hasPickFaces)
		{
			var factory = new BusinessObjectFactory();
			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new AllocationProductFact(factory.New<OrgSupplierPart>(), relationMock.Object, true, hasPickFaces, "Style1", "M", "RED", "SML");
			AssertEquals(nameof(AllocationProductFact.HasPickFaces), hasPickFaces, productFact.HasPickFaces);
		}

		#endregion

		#region TestIsUsingExpiryDate

		public void TestIsUsingExpiryDate_False() => TestIsUsingExpiryDate(isUsingExpiryDate: false);
		public void TestIsUsingExpiryDate_True() => TestIsUsingExpiryDate(isUsingExpiryDate: true);

		void TestIsUsingExpiryDate(bool isUsingExpiryDate)
		{
			var factory = new BusinessObjectFactory();
			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
			relationMock.SetupGet(r => r.OU_UseExpiryDate).Returns(isUsingExpiryDate);

			var productFact = new AllocationProductFact(factory.New<OrgSupplierPart>(), relationMock.Object, true, false, "Style1", "M", "RED", "SML");
			AssertEquals(nameof(AllocationProductFact.IsUsingExpiryDate), isUsingExpiryDate, productFact.IsUsingExpiryDate);
		}

		#endregion

		#region TestIsUsingSpecifiedSerialNumbers

		public void TestIsUsingSpecifiedSerialNumbers() => TestIsUsingSpecifiedSerialNumbers(useSerialNumber: true, isReleaseCaptured: false, pickMode: WhsPickMode.Codes.AttributeSpecified, expectedResult: true);
		public void TestIsUsingSpecifiedSerialNumbers_PickModeCaseInsensitive() => TestIsUsingSpecifiedSerialNumbers(useSerialNumber: true, isReleaseCaptured: false, pickMode: "asp", expectedResult: true);
		public void TestIsUsingSpecifiedSerialNumbers_NotSerialNumber() => TestIsUsingSpecifiedSerialNumbers(useSerialNumber: false, isReleaseCaptured: false, pickMode: WhsPickMode.Codes.AttributeSpecified, expectedResult: false);
		public void TestIsUsingSpecifiedSerialNumbers_ReleaseCaptured() => TestIsUsingSpecifiedSerialNumbers(useSerialNumber: true, isReleaseCaptured: true, pickMode: WhsPickMode.Codes.AttributeSpecified, expectedResult: false);
		public void TestIsUsingSpecifiedSerialNumbers_AttributeNeutral() => TestIsUsingSpecifiedSerialNumbers(useSerialNumber: true, isReleaseCaptured: false, pickMode: WhsPickMode.Codes.AttributeNeutral, expectedResult: false);
		public void TestIsUsingSpecifiedSerialNumbers_UnexpectedPickMode() => TestIsUsingSpecifiedSerialNumbers(useSerialNumber: true, isReleaseCaptured: false, pickMode: "abc", expectedResult: false);

		void TestIsUsingSpecifiedSerialNumbers(bool useSerialNumber, bool isReleaseCaptured, string pickMode, bool expectedResult)
		{
			var factory = new BusinessObjectFactory();
			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
			relationMock.SetupGet(r => r.OU_UseSerialNumber).Returns(useSerialNumber);
			relationMock.SetupGet(r => r.OU_IsSerialNumberReleaseCaptured).Returns(isReleaseCaptured);
			relationMock.SetupGet(r => r.OU_PickMode).Returns(pickMode);

			var productFact = new AllocationProductFact(factory.New<OrgSupplierPart>(), relationMock.Object, true, false, "Style1", "M", "RED", "SML");
			AssertEquals(nameof(AllocationProductFact.IsUsingSpecifiedSerialNumbers), expectedResult, productFact.IsUsingSpecifiedSerialNumbers);
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated

		#region TestGetQuantityThatCanBeAllocated_SKU

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_BreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.CanBreakUOMs, 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_AllModes()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, GetAllAllocationModes(), 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_OtherModes()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateCases | UOMAllocationModes.AllocatePallets, 5m, 5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_OtherModes_BreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateCases | UOMAllocationModes.AllocatePallets | UOMAllocationModes.CanBreakUOMs, 5m, 5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases, 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_BreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases | UOMAllocationModes.CanBreakUOMs, 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_AllModes()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, GetAllAllocationModes(), 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_OtherModes()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.AllocatePallets, 5m, 5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_OtherModes_BreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.AllocatePallets | UOMAllocationModes.CanBreakUOMs, 5m, 5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets, 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_BreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets | UOMAllocationModes.CanBreakUOMs, 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_AllModes()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, GetAllAllocationModes(), 5m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_OtherModes()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.AllocateCases, 5m, 5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_OtherModes_BreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.AllocateCases | UOMAllocationModes.CanBreakUOMs, 5m, 5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_InventoryLimited()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, 5m, 10m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_OrderLimited()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, 10m, 5m, 5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_Decimal()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, 0.5m, 0.5m, 0.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_Decimal_CanBreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.CanBreakUOMs, 0.5m, 0.5m, 0.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_Decimal_GreaterThanOne()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, 1.5m, 1.5m, 1.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_SplitCase_Decimal_GreaterThanOne_CanBreakUOMs()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.CanBreakUOMs, 1.5m, 1.5m, 1.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_Decimal()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases, 0.5m, 0.5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_Decimal_GreaterThanOne()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases, 1.5m, 1.5m, 1.0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_Decimal_WithSplitCase()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases | UOMAllocationModes.AllocateSplitCases, 0.5m, 0.5m, 0.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Case_Decimal_GreaterThanOne_WithSplitCase()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases | UOMAllocationModes.AllocateSplitCases, 1.5m, 1.5m, 1.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_Decimal()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets, 0.5m, 0.5m, 0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_Decimal_GreaterThanOne()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets, 1.5m, 1.5m, 1.0m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_Decimal_WithSplitCase()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets | UOMAllocationModes.AllocateSplitCases, 0.5m, 0.5m, 0.5m);

		public void TestGetQuantityThatCanBeAllocated_SKU_Pallet_Decimal_GreaterThanOne_WithSplitCase()
			=> TestGetQuantityThatCanBeAllocated_SKU(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets | UOMAllocationModes.AllocateSplitCases, 1.5m, 1.5m, 1.5m);

		void TestGetQuantityThatCanBeAllocated_SKU(
			string skuUomType,
			UOMAllocationModes modes,
			decimal inventoryQuantity,
			decimal orderedQuantity,
			decimal allocatedQuantity)
		{
			var factory = new BusinessObjectFactory();

			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOT").F3_UOMType = skuUomType;
			var product = factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "BOT";

			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
			var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");
			AssertEquals(allocatedQuantity, productFact.GetQuantityThatCanBeAllocated(inventoryQuantity, orderedQuantity, modes));

			// Check cached quantity
			AssertEquals(allocatedQuantity, productFact.GetQuantityThatCanBeAllocated(inventoryQuantity, orderedQuantity, modes));
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_Case()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_Pallet()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_SplitCase()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, UOMPackTypesList.Codes.Pallet);

		void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType(string uomType, UOMAllocationModes mode, string skuUomType = UOMPackTypesList.Codes.SplitCase)
		{
			var factory = new BusinessObjectFactory();
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType;
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

			var product = factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 10, "UNT", "BOX");

			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
			var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

			AssertEquals("Should be able to pick all stock.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, mode));
			AssertEquals("Should be able to pick all stock.", 20m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, mode));
			AssertEquals("Should be able to pick all stock.", 50m, productFact.GetQuantityThatCanBeAllocated(50m, 50m, mode));
			AssertEquals("Should be able to 5x BOX.", 50m, productFact.GetQuantityThatCanBeAllocated(51m, 51m, mode));

			AssertEquals("Should be able to pick all stock.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, mode | UOMAllocationModes.CanBreakUOMs));
			AssertEquals("Should be able to pick all stock.", 20m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, mode | UOMAllocationModes.CanBreakUOMs));
			AssertEquals("Should be able to pick all stock.", 50m, productFact.GetQuantityThatCanBeAllocated(50m, 50m, mode | UOMAllocationModes.CanBreakUOMs));
			AssertEquals("Should be able to 5x BOX.", 50m, productFact.GetQuantityThatCanBeAllocated(51m, 51m, mode | UOMAllocationModes.CanBreakUOMs));
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers_Case()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases, twoConversions: false);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers_Pallet()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets, twoConversions: false);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers_SplitCase()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, twoConversions: false, skuUomType: UOMPackTypesList.Codes.Pallet);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers_Case_MultipleConversions()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases, twoConversions: true);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers_Pallet_MultipleConversions()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets, twoConversions: true);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers_SplitCase_MultipleConversions()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, twoConversions: true, skuUomType: UOMPackTypesList.Codes.Pallet);

		void TestGetQuantityThatCanBeAllocated_MultipleOfNonSKUPackType_LargeNumbers(string uomType, UOMAllocationModes mode, bool twoConversions, string skuUomType = UOMPackTypesList.Codes.SplitCase)
		{
			var factory = new BusinessObjectFactory();
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType;
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType;
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

			var product = factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			if (twoConversions)
			{
				CreateUnitPart(product, 500000, "UNT", "BOX");
			}

			CreateUnitPart(product, 1000000, "UNT", "CTN");

			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
			var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

			var quantityToAllocate = 3000000000m;
			AssertGreaterThan("Precondition.", quantityToAllocate, int.MaxValue);

			AssertEquals("Should be able to pick all stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate, quantityToAllocate, mode));
			AssertEquals("Should be able to pick all stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate, quantityToAllocate, mode | UOMAllocationModes.CanBreakUOMs));

			AssertEquals("Should be able to pick all stock.", quantityToAllocate + 1000000m, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 1000000m, quantityToAllocate + 1000000m, mode));
			AssertEquals("Should be able to pick all stock.", quantityToAllocate + 1000000m, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 1000000m, quantityToAllocate + 1000000m, mode | UOMAllocationModes.CanBreakUOMs));

			AssertEquals("Should be able to pick 3b stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 100000m, quantityToAllocate + 100000m, mode));
			AssertEquals("Should be able to pick 3b stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 100000m, quantityToAllocate + 100000m, mode | UOMAllocationModes.CanBreakUOMs));

			// Fallback to greedy allocation with excessively high number and no GCD. In these cases, it actually results in the same result, but won't throw an exception.
			AssertEquals("Should be able to pick 3b stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 1m, quantityToAllocate + 1m, mode));
			AssertEquals("Should be able to pick 3b stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 1m, quantityToAllocate + 1m, mode | UOMAllocationModes.CanBreakUOMs));

			// Also fallback to greedy allocation if the arrays would result in out of memory...
			quantityToAllocate = 750000000m;
			AssertEquals("Should be able to pick 3b stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 1000m, quantityToAllocate + 1000m, mode));
			AssertEquals("Should be able to pick 3b stock.", quantityToAllocate, productFact.GetQuantityThatCanBeAllocated(quantityToAllocate + 1000m, quantityToAllocate + 1000m, mode | UOMAllocationModes.CanBreakUOMs));
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType

		public void TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType_Case()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType(UOMPackTypesList.Codes.Case, UOMAllocationModes.AllocateCases);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType_Pallet()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType(UOMPackTypesList.Codes.Pallet, UOMAllocationModes.AllocatePallets, skdUomType: UOMPackTypesList.Codes.Case);

		public void TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType_SplitCase()
			=> TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType(UOMPackTypesList.Codes.SplitCase, UOMAllocationModes.AllocateSplitCases, UOMPackTypesList.Codes.Pallet);

		void TestGetQuantityThatCanBeAllocated_MultipleOfLargerNonSKUPackType(string uomType, UOMAllocationModes mode, string skuUomType = UOMPackTypesList.Codes.SplitCase, string skdUomType = UOMPackTypesList.Codes.Pallet)
		{
			var factory = new BusinessObjectFactory();
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType;
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "SKD").F3_UOMType = skdUomType;
			factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

			var product = factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 10, "UNT", "BOX");
			CreateUnitPart(product, 2, "BOX", "SKD");

			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
			var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

			// 30 units = 2 boxes of 10 on a pallet + 1 box of 0
			CombineAssertions(() =>
			{
				AssertEquals("Should be able to pick the loose box.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, mode));
				AssertEquals("Should not be able to pick the box on another pack type.", 0m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, mode));
				AssertEquals("Should be able to pick the excess box.", 10m, productFact.GetQuantityThatCanBeAllocated(30m, 30m, mode));

				AssertEquals("Should be able to pick the loose box.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, mode | UOMAllocationModes.CanBreakUOMs));
				AssertEquals("Can break packs and take boxes.", 20m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, mode | UOMAllocationModes.CanBreakUOMs));
				AssertEquals("Can break packs and take boxes.", 30m, productFact.GetQuantityThatCanBeAllocated(30m, 30m, mode | UOMAllocationModes.CanBreakUOMs));
			});
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_BigAndSmallPackType

		public void TestGetQuantityThatCanBeAllocated_BigAndSmallPackType()
		{
			var uomTypes = new[] { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Pallet };

			foreach (var breakUoms in new[] { UOMAllocationModes.None, UOMAllocationModes.CanBreakUOMs })
			{
				foreach (var uomType1 in uomTypes)
				{
					foreach (var uomType2 in uomTypes)
					{
						var skuUomType = uomTypes.First(u => u != uomType1 && u != uomType2);

						var factory = new BusinessObjectFactory();
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType1;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

						var product = factory.New<OrgSupplierPart>();
						product.OP_StockKeepingUnit = "UNT";

						CreateUnitPart(product, 4, "UNT", "CTN");
						CreateUnitPart(product, 6, "UNT", "BOX");

						var relationMock = new Mock<IOrgPartRelation>();
						relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
						var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

						var canBreak = breakUoms == UOMAllocationModes.CanBreakUOMs;

						CombineAssertions($"UOM1: {uomType1}, UOM2: {uomType2}, Can Break?: {canBreak}", () =>
						{
							AssertEquals($"Should be able to pick 1x {uomType1} pack type.", 4m, productFact.GetQuantityThatCanBeAllocated(4m, 4m, breakUoms | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x {uomType2} pack type.", 6m, productFact.GetQuantityThatCanBeAllocated(6m, 6m, breakUoms | GetAllocationMode(uomType2)));

							AssertEquals($"Should be able to pick 1x {uomType1} pack type.", 4m, productFact.GetQuantityThatCanBeAllocated(10m, 4m, breakUoms | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x {uomType2} pack type.", 6m, productFact.GetQuantityThatCanBeAllocated(10m, 6m, breakUoms | GetAllocationMode(uomType2)));

							AssertEquals($"Should be able to pick 1x {uomType1} pack type.", 4m, productFact.GetQuantityThatCanBeAllocated(4m, 10m, breakUoms | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x {uomType2} pack type.", 6m, productFact.GetQuantityThatCanBeAllocated(6m, 10m, breakUoms | GetAllocationMode(uomType2)));

							AssertEquals($"Should be able to pick 1x {uomType1} + 1x {uomType2} pack type.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, breakUoms | GetAllocationMode(uomType1) | GetAllocationMode(uomType2)));
							AssertEquals($"Should be able to pick 2x {uomType1} + 2x {uomType2} pack type if can break, otherwise 3x {uomType1}.", canBreak ? 20m : 18m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, breakUoms | GetAllocationMode(uomType1) | GetAllocationMode(uomType2)));
							AssertEquals($"Should be able to pick 10x {uomType1} + 10x {uomType2} pack type.", 100m, productFact.GetQuantityThatCanBeAllocated(100m, 100m, breakUoms | GetAllocationMode(uomType1) | GetAllocationMode(uomType2)));
						});
					}
				}
			}
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_BigMedAndSmallPackType

		public void TestGetQuantityThatCanBeAllocated_BigMedAndSmallPackType()
		{
			var uomTypes = new[] { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Pallet };

			foreach (var breakUoms in new[] { UOMAllocationModes.None, UOMAllocationModes.CanBreakUOMs })
			{
				foreach (var uomType1 in uomTypes)
				{
					foreach (var uomType2 in uomTypes)
					{
						var skuAndBigUomType = uomTypes.First(u => u != uomType1 && u != uomType2);

						var factory = new BusinessObjectFactory();
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "SKD").F3_UOMType = skuAndBigUomType;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType1;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuAndBigUomType;

						var product = factory.New<OrgSupplierPart>();
						product.OP_StockKeepingUnit = "UNT";

						CreateUnitPart(product, 12, "UNT", "SKD");
						CreateUnitPart(product, 6, "UNT", "BOX");
						CreateUnitPart(product, 4, "UNT", "CTN");

						var relationMock = new Mock<IOrgPartRelation>();
						relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
						var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

						if (breakUoms == UOMAllocationModes.CanBreakUOMs)
						{
							AssertEquals($"Should be able to pick 2x BOX off the SKD + 1x CTN.", 16m, productFact.GetQuantityThatCanBeAllocated(16m, 16m, breakUoms | GetAllocationMode(uomType1) | GetAllocationMode(uomType2)));
						}
						else
						{
							AssertEquals($"Should be able to pick 1x CTN.", 4m, productFact.GetQuantityThatCanBeAllocated(16m, 16m, breakUoms | GetAllocationMode(uomType1) | GetAllocationMode(uomType2)));
						}
					}
				}
			}
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_CombinatorialCases

		public void TestGetQuantityThatCanBeAllocated_CombinatorialCases()
		{
			var uomTypes = new[] { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Pallet };

			foreach (var breakUoms in new[] { UOMAllocationModes.None, UOMAllocationModes.CanBreakUOMs })
			{
				foreach (var uomType1 in uomTypes)
				{
					foreach (var uomType2 in uomTypes)
					{
						var skuUomType = uomTypes.First(u => u != uomType1 && u != uomType2);

						var factory = new BusinessObjectFactory();
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "SKD").F3_UOMType = uomType1;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

						var product = factory.New<OrgSupplierPart>();
						product.OP_StockKeepingUnit = "UNT";

						// Box = 10 units
						// Carton = 10 boxes = 100 units
						// Skid = 10 cartons = 1000 units
						CreateUnitPart(product, 10, "UNT", "BOX");
						CreateUnitPart(product, 10, "BOX", "CTN");
						CreateUnitPart(product, 10, "CTN", "SKD");

						var relationMock = new Mock<IOrgPartRelation>();
						relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
						var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

						var canBreak = breakUoms == UOMAllocationModes.CanBreakUOMs;

						CombineAssertions($"UOM1: {uomType1}, UOM2: {uomType2}, Can Break?: {canBreak}", () =>
						{
							// Test non-combinatorial cases
							AssertEquals($"Should be able to pick 1x UNT.", 1m, productFact.GetQuantityThatCanBeAllocated(1m, 1m, breakUoms | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 9x UNT.", 9m, productFact.GetQuantityThatCanBeAllocated(9m, 9m, breakUoms | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x BOX.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x CTN.", 100m, productFact.GetQuantityThatCanBeAllocated(100m, 100m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD.", 1000m, productFact.GetQuantityThatCanBeAllocated(1000m, 1000m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));

							AssertEquals($"Should be able to pick 1x UNT if we can break.", canBreak ? 1m : 0m, productFact.GetQuantityThatCanBeAllocated(10m, 1m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x UNT if we can break.", canBreak ? 1m : 0m, productFact.GetQuantityThatCanBeAllocated(100m, 1m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x UNT if we can break.", canBreak ? 1m : 0m, productFact.GetQuantityThatCanBeAllocated(1000m, 1m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x BOX if we can break.", canBreak ? 10m : 0m, productFact.GetQuantityThatCanBeAllocated(100m, 10m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x BOX if we can break.", canBreak ? 10m : 0m, productFact.GetQuantityThatCanBeAllocated(1000m, 10m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x CTN if we can break.", canBreak ? 100m : 0m, productFact.GetQuantityThatCanBeAllocated(1000m, 100m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));

							// Combinatorial cases
							var uomType1IsSplitCase = uomType1 == UOMPackTypesList.Codes.SplitCase;
							var uomType2IsSplitCase = uomType2 == UOMPackTypesList.Codes.SplitCase;
							var skuIsSplitCase = skuUomType == UOMPackTypesList.Codes.SplitCase;
							var anyUomTypeIsSplitCase = uomType1IsSplitCase || uomType2IsSplitCase || skuIsSplitCase;
							AssertEquals($"Should be able to pick 1x UNT + 1x BOX.", 11m, productFact.GetQuantityThatCanBeAllocated(11m, 11m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x UNT + 1x BOX + 1x CTN.", 111m, productFact.GetQuantityThatCanBeAllocated(111m, 111m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x UNT + 1x BOX + 1x CTN + 1x SKD.", 1111m, productFact.GetQuantityThatCanBeAllocated(1111m, 1111m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x UNT + 1x BOX + 1x CTN + 1x SKD + decimal leftovers.", anyUomTypeIsSplitCase ? 1111.11m : 1111m, productFact.GetQuantityThatCanBeAllocated(1111.11m, 1111.11m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x UNT + 1x BOX + 1x CTN + decimal leftovers, if we can't pick the Skid.", (uomType1 != uomType2 && !canBreak ? 111m : 1111m) + (uomType2IsSplitCase || skuIsSplitCase ? .11m : 0m), productFact.GetQuantityThatCanBeAllocated(1111.11m, 1111.11m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x SKD if we can't pick the rest.", (uomType1 != uomType2 ? 1000m : 1110m) + (uomType1IsSplitCase ? .11m : 0m), productFact.GetQuantityThatCanBeAllocated(1111.11m, 1111.11m, breakUoms | GetAllocationMode(uomType1)));
						});
					}
				}
			}
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_NotGreedy

		public void TestGetQuantityThatCanBeAllocated_NotGreedy()
		{
			var uomTypes = new[] { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Pallet };

			foreach (var breakUoms in new[] { UOMAllocationModes.None, UOMAllocationModes.CanBreakUOMs })
			{
				foreach (var uomType1 in uomTypes)
				{
					foreach (var uomType2 in uomTypes)
					{
						var skuUomType = uomTypes.First(u => u != uomType1 && u != uomType2);

						var factory = new BusinessObjectFactory();
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "SKD").F3_UOMType = uomType1;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

						var product = factory.New<OrgSupplierPart>();
						product.OP_StockKeepingUnit = "UNT";

						// Box = 30 units
						// Carton = 40 units
						// Skid = 50 units
						CreateUnitPart(product, 30, "UNT", "BOX");
						CreateUnitPart(product, 40, "UNT", "CTN");
						CreateUnitPart(product, 50, "UNT", "SKD");

						var relationMock = new Mock<IOrgPartRelation>();
						relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
						var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

						var canBreak = breakUoms == UOMAllocationModes.CanBreakUOMs;

						CombineAssertions($"UOM1: {uomType1}, UOM2: {uomType2}, Can Break?: {canBreak}", () =>
						{
							// Test non-combinatorial cases
							AssertEquals($"Should be able to pick 1x BOX.", 30m, productFact.GetQuantityThatCanBeAllocated(30m, 30m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x CTN.", 40m, productFact.GetQuantityThatCanBeAllocated(40m, 40m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD.", 50m, productFact.GetQuantityThatCanBeAllocated(50m, 50m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));

							// Combinatorial cases
							AssertEquals($"Should be able to pick 1x BOX + 1x CTN, if allowed to break, otherwise 1x SKD.", canBreak ? 70m : 50m, productFact.GetQuantityThatCanBeAllocated(70m, 70m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x BOX + 1x SKD.", 80m, productFact.GetQuantityThatCanBeAllocated(80m, 80m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x CTN + 1x SKD.", 90m, productFact.GetQuantityThatCanBeAllocated(90m, 90m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 2x SKD.", 100m, productFact.GetQuantityThatCanBeAllocated(100m, 100m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 4x SKD.", 200m, productFact.GetQuantityThatCanBeAllocated(200m, 200m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 5x CTN if allowed to break.", canBreak || uomType1 == uomType2 ? 200m : 0m, productFact.GetQuantityThatCanBeAllocated(200m, 200m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(skuUomType)));

							AssertEquals($"Should be able to pick 1x SKD.", 50m, productFact.GetQuantityThatCanBeAllocated(200m, 50m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x CTN if allowed to break.", canBreak ? 40m : 0m, productFact.GetQuantityThatCanBeAllocated(200m, 40m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1) | GetAllocationMode(skuUomType)));
							AssertEquals($"Should be able to pick 1x CTN + 1x BOX if allowed to break or 1x SKD if allows.", canBreak ? 70m : (uomType1 == uomType2 ? 50m : 0m), productFact.GetQuantityThatCanBeAllocated(100m, 70m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(skuUomType)));
						});
					}
				}
			}
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_CombinatorialCases_NotGreedy_OverlappingConversions

		public void TestGetQuantityThatCanBeAllocated_CombinatorialCases_NotGreedy_OverlappingConversions()
		{
			var uomTypes = new[] { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Pallet };

			foreach (var breakUoms in new[] { UOMAllocationModes.None, UOMAllocationModes.CanBreakUOMs })
			{
				foreach (var uomType1 in uomTypes)
				{
					foreach (var uomType2 in uomTypes)
					{
						var skuUomType = uomTypes.First(u => u != uomType1 && u != uomType2);

						var factory = new BusinessObjectFactory();
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "SKD").F3_UOMType = uomType1;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

						var product = factory.New<OrgSupplierPart>();
						product.OP_StockKeepingUnit = "UNT";

						// Skid = 15 units
						// Carton = 10 units
						// Box = 8 units
						// E.g. ordered 20 units, best is to take carton + box for 18 rather than Skid for 15
						// Note also that 15+10+8 = 33 = 2 Skids, these close conversions create more clashes for combinatorial cases where we cant break packs
						CreateUnitPart(product, 15, "UNT", "SKD");
						CreateUnitPart(product, 10, "UNT", "CTN");
						CreateUnitPart(product, 8, "UNT", "BOX");

						var relationMock = new Mock<IOrgPartRelation>();
						relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
						var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

						var canBreak = breakUoms == UOMAllocationModes.CanBreakUOMs;

						CombineAssertions($"UOM1: {uomType1}, UOM2: {uomType2}, Can Break?: {canBreak}", () =>
						{
							// Test non-combinatorial cases
							AssertEquals($"Should be able to pick 1x BOX.", 8m, productFact.GetQuantityThatCanBeAllocated(8m, 8m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x CTN.", 10m, productFact.GetQuantityThatCanBeAllocated(10m, 10m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD.", 15m, productFact.GetQuantityThatCanBeAllocated(15m, 15m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 4x SKD.", 60m, productFact.GetQuantityThatCanBeAllocated(60m, 60m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x BOX if allowed to break.", canBreak ? 8m : 0m, productFact.GetQuantityThatCanBeAllocated(15m, 8m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x CTN if allowed to break.", canBreak ? 10m : 0m, productFact.GetQuantityThatCanBeAllocated(15m, 10m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 6x CTN if allowed to break.", canBreak || uomType1 == uomType2 ? 60m : 0m, productFact.GetQuantityThatCanBeAllocated(60m, 60m, breakUoms | GetAllocationMode(uomType2)));
							AssertEquals($"Should be able to pick 6x CTN + 1x BOX if allowed to break, otherwise 1x BOX.", canBreak || uomType1 == uomType2 ? 68m : 8m, productFact.GetQuantityThatCanBeAllocated(68m, 68m, breakUoms | GetAllocationMode(uomType2)));

							// Combinatorial cases, should not greedily take a Skid if it's not optimal and we can break
							AssertEquals($"Should be able to pick 4x SKD + 1x BOX.", 70m, productFact.GetQuantityThatCanBeAllocated(70m, 70m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 2x BOX if allowed to break, otherwise 1x SKD.", canBreak ? 16m : 15m, productFact.GetQuantityThatCanBeAllocated(16m, 16m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x CTN + 1x BOX if allowed to break, otherwise 1x SKD.", canBreak ? 18m : 15m, productFact.GetQuantityThatCanBeAllocated(18m, 18m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 2x cartons if allowed to break, otherwise 1x SKD.", canBreak ? 20m : 15m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD + 1x CTN.", 23m, productFact.GetQuantityThatCanBeAllocated(23m, 23m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD + 1x CTN if allowed to break, otherwise 1x SKD.", canBreak ? 23m : 15m, productFact.GetQuantityThatCanBeAllocated(25m, 23m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD + 1x BOX.", 25m, productFact.GetQuantityThatCanBeAllocated(25m, 25m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
						});
					}
				}
			}
		}

		#endregion

		#region TestGetQuantityThatCanBeAllocated_CombinatorialCases_PrimeNumbers

		public void TestGetQuantityThatCanBeAllocated_CombinatorialCases_PrimeNumbers()
		{
			var uomTypes = new[] { UOMPackTypesList.Codes.Case, UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Codes.Pallet };

			foreach (var breakUoms in new[] { UOMAllocationModes.None, UOMAllocationModes.CanBreakUOMs })
			{
				foreach (var uomType1 in uomTypes)
				{
					foreach (var uomType2 in uomTypes)
					{
						var skuUomType = uomTypes.First(u => u != uomType1 && u != uomType2);

						var factory = new BusinessObjectFactory();
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType1;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "SKD").F3_UOMType = uomType2;
						factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;

						var product = factory.New<OrgSupplierPart>();
						product.OP_StockKeepingUnit = "UNT";

						// Box = 13 units
						// Carton = 17 units
						// Skid = 47 units
						// Using primes will stress any GCD type logic we do as there is no commonality (besides 1) with the pack types
						CreateUnitPart(product, 7, "UNT", "BOX");
						CreateUnitPart(product, 13, "UNT", "CTN");
						CreateUnitPart(product, 47, "UNT", "SKD");

						var relationMock = new Mock<IOrgPartRelation>();
						relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);
						var productFact = new AllocationProductFact(product, relationMock.Object, false, false, "Style1", "M", "RED", "SML");

						var canBreak = breakUoms == UOMAllocationModes.CanBreakUOMs;

						CombineAssertions($"UOM1: {uomType1}, UOM2: {uomType2}, Can Break?: {canBreak}", () =>
						{
							// Test non-combinatorial cases
							AssertEquals($"Should be able to pick 1x BOX.", 7m, productFact.GetQuantityThatCanBeAllocated(7m, 7m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x CTN.", 13m, productFact.GetQuantityThatCanBeAllocated(13m, 13m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD.", 47m, productFact.GetQuantityThatCanBeAllocated(47m, 47m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x BOX + 1x CTN.", 20m, productFact.GetQuantityThatCanBeAllocated(20m, 20m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 3x CTN + 1x BOX.", 46m, productFact.GetQuantityThatCanBeAllocated(46m, 46m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));

							// Combinatorial cases
							AssertEquals($"Should be able to pick 3x CTN + 1x BOX if allowed to break.", canBreak ? 46m : 0m, productFact.GetQuantityThatCanBeAllocated(47m, 46m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 2x BOX + 1x CTN if allowed to break, otherwise 2x CTN.", canBreak ? 27m : 26m, productFact.GetQuantityThatCanBeAllocated(27m, 27m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
							AssertEquals($"Should be able to pick 1x SKD + 2x BOX if allowed to break, otherwise 1x SKD and 1x CTN.", canBreak ? 61m : 60m, productFact.GetQuantityThatCanBeAllocated(61m, 61m, breakUoms | GetAllocationMode(uomType2) | GetAllocationMode(uomType1)));
						});
					}
				}
			}
		}

		#endregion

		#endregion

		#region Implementation

		static void CreateUnitPart(OrgSupplierPart product, ZDecimal qtyInParent, ZString packType, ZString parentPackType)
		{
			var unit = product.PartUnits.AddNew();
			unit.OF_PackType = packType;
			unit.OF_ParentPackType = parentPackType;
			unit.OF_QuantityInParent = qtyInParent;
		}

		static UOMAllocationModes GetAllocationMode(string uomType)
		{
			switch (uomType)
			{
				case UOMPackTypesList.Codes.SplitCase:
					return UOMAllocationModes.AllocateSplitCases;

				case UOMPackTypesList.Codes.Case:
					return UOMAllocationModes.AllocateCases;

				case UOMPackTypesList.Codes.Pallet:
					return UOMAllocationModes.AllocatePallets;

				default:
					return UOMAllocationModes.None;
			}
		}

		static UOMAllocationModes GetAllAllocationModes()
		{
			return UOMAllocationModes.AllocateCases | UOMAllocationModes.AllocateSplitCases | UOMAllocationModes.AllocatePallets | UOMAllocationModes.CanBreakUOMs;
		}

		protected override ProductFact GetProductFact(IOrgSupplierPart part, IOrgPartRelation relation)
		{
			var factory = new BusinessObjectFactory();

			OrgSupplierPart product = null;
			if (part != null)
			{
				product = factory.New<OrgSupplierPart>();
				product.OP_PartNum = part.OP_PartNum;
				product.OP_StockKeepingUnit = part.OP_StockKeepingUnit;
				product.OP_RH_NKCommodityCode = part.OP_RH_NKCommodityCode;
			}

			return new AllocationProductFact(product, relation, false, false, "Style1", "M", "RED", "SML");
		}

		#endregion
	}
}
