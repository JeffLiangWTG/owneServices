using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketLineValidationTest<TDocketLine, TDocket> : WhsDocketLineValidationTestCase<TDocketLine, TDocket>
		where TDocketLine : WhsPickableDocketLine
		where TDocket : WhsPickableDocket
	{
		#region TestValidateWE_TransactionQuantity

		protected override void TestCheckWE_TransactionQuantityCore()
		{
			base.TestCheckWE_TransactionQuantityCore();

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			TestMinDecimal(docketLine.WE_TransactionQuantityInfo, ErrorCheckType.HasErrors, 0m);

			docketLine.WE_TransactionQuantity = -1m;
			AssertHasError(docketLine.WE_TransactionQuantityInfo, "Please enter a value greater than zero");

			docketLine.WE_TransactionQuantity = 10m;
			AssertNoErrors(docketLine.WE_TransactionQuantityInfo);

			docketLine.WE_TransactionQuantity = 0m;
			AssertNoErrors("Should not have warning, not error", docketLine.WE_TransactionQuantityInfo);
			AssertHasWarning(docketLine.WE_TransactionQuantityInfo, "Lines with zero quantities will not be picked");

			Helper.SetClientAttributeType(docket.Client, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(docket.Client, docketLine.SupplierPart, AttributeNumber.Serial, true);
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				docketLine.WE_TransactionQuantity = 2m;
				docketLine.WE_SerialNumber = "SN1";
				AssertHasError(docketLine.WE_TransactionQuantityInfo, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
			}
		}

		#endregion

		#region TestValidateWE_ShortfallQuantityCached

		public void TestValidateWE_ShortfallQuantityCached()
		{
			TestValidateWE_ShortfallQuantityCachedCore();
		}

		protected abstract void TestValidateWE_ShortfallQuantityCachedCore();

		#endregion

		#region TestValidateSumOfUnitsMet

		public void TestValidateSumOfUnitsMet()
		{
			TestValidateSumOfUnitsMetCore();
		}

		protected virtual void TestValidateSumOfUnitsMetCore()
		{
			DocketLine.WE_TransactionQuantity = 10;
			AssertEquals("Precondition", 0m, DocketLine.SumOfUnitsMet);

			DocketLine.Validation.ValidateSumOfUnitsMet();
			AssertEquals("Should have no warning", false, DocketLine.SumOfUnitsMetInfo.HasWarnings());

			DocketLine.ReadOnly = true;
			DocketLine.PickableDocket.WD_WP = ZGuid.NewZGuid();
			DocketLine.PickableDocket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			DocketLine.Validation.ValidateSumOfUnitsMet();
			AssertEquals("Should have no warning", false, DocketLine.SumOfUnitsMetInfo.HasWarnings());

			DocketLine.ReadOnly = false;
			DocketLine.Validation.ValidateSumOfUnitsMet();
			AssertHasWarning("Should have Warning", DocketLine.SumOfUnitsMetInfo, "Shortfall");
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode

		public override void TestCheckWE_WHC_NKOriginalInventoryHeldCode()
		{
			Assert("Tested below", true);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld

		public override void TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held; // Invalid, but currently possible due to bad constraint + transform (being fixed in WI00083269 and WI00083270)
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_OriginalInventoryStatus = ZString.Empty;
			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld

		public override void TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;  // Invalid, but currently possible due to bad constraint + transform (being fixed in WI00083269 and WI00083270)
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_CurrentInventoryStatus = ZString.Empty;
			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		public override void TestCheckWE_WHC_NKOriginalInventoryHeldCode_ListValidation()
		{
			var invalidHoldCode = "ZZZ";
			AssertEquals("Precondition: invalidholdcode not in the list of InventoryHeldCodeCollection.", false, DocketLine.Lookups.InventoryHeldCodeCollection.ToArray().Any(code => code.Code == invalidHoldCode));
			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = invalidHoldCode;
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#region TestValidateLocationString

		protected override bool IsLocationStringFieldUsed
		{
			get { return false; }
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();

			line1.WE_OP = data.Part1.PK;
			line2.WE_OP = data.Part1.PK;

			line1.WE_TransactionQuantity = 110; // only 100 available
			line2.WE_TransactionQuantity = 110; // only 100 available

			Factory.Save();

			// make sure the Product list db hits don't count toward the assertions, as well as additional checks in the WorkOrder's ValidateAll
			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();

			// the actual test
			var dbLoadCount = Factory.DatabaseLoadCount;
			var expectedDbHits = line1.CanCalculateShortfallForAllLinesFromDB ? 1 : 0; // work order will have already calc'd the shortfall in WE_TransactionQuantity
			line1.Validation.ValidateAll();
			line2.Validation.ValidateAll();

			if (!ExpectedShortfallWarning1.IsEmpty)
			{
				AssertHasWarning(line1.WE_ShortfallQuantityCachedInfo, ExpectedShortfallWarning1);
			}

			if (!ExpectedShortfallWarning2.IsEmpty)
			{
				AssertHasWarning(line2.WE_ShortfallQuantityCachedInfo, ExpectedShortfallWarning2);
			}

			Assert("The DB should have been hit only once, thereafter the ShortfallQty on each line should be cached.",
				Factory.DatabaseLoadCount <= dbLoadCount + expectedDbHits);
		}

		protected abstract ZString ExpectedShortfallWarning1 { get; }
		protected abstract ZString ExpectedShortfallWarning2 { get; }

		#endregion

		#region Implementation

		protected override bool CanHaveInventoryAttached
		{
			get { return false; }
		}

		#endregion
	}
}
