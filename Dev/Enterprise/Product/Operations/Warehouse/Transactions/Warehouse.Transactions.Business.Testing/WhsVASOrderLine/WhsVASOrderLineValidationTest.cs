using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsVASOrderLineValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWVL_ExpiryDateIsValidZDateRange

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestCheckWVL_ExpiryDateIsValidZDateRange()
		{
			var today = ZDate.Today;
			var dateAfterMaximumFutureYears = today.AddYears(DateRangeValidation.MaximumFutureYears + 1);
			var dateBeforeMaximumFutureYears = today.AddYears(DateRangeValidation.MaximumFutureYears - 1);

			var vasOrderLine = Factory.New<WhsVASOrderLine>();
			vasOrderLine.WVL_ExpiryDate = today;
			AssertNoErrors(vasOrderLine.WVL_ExpiryDateInfo);

			vasOrderLine.WVL_ExpiryDate = dateBeforeMaximumFutureYears;
			AssertNoErrors(vasOrderLine.WVL_ExpiryDateInfo);

			vasOrderLine.WVL_ExpiryDate = dateAfterMaximumFutureYears;
			AssertHasError(vasOrderLine.WVL_ExpiryDateInfo, String.Format("The date '{0}' is more than {1} years from now and thus is not valid.", dateAfterMaximumFutureYears.ToString("dd-MMM-yyyy"), DateRangeValidation.MaximumFutureYears));
		}

		#endregion

		#region TestCheckWVL_OP_Product

		public void TestCheckWVL_OP_Product()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			data.Part1.OP_IsActive = false;
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			AssertHasError(vasOrderLine.WVL_OP_ProductInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);

			data.Part1.OP_IsActive = true;
			vasOrderLine.WVL_OP_Product = ZGuid.Empty;
			vasOrderLine.WVL_OP_Product = data.Part1.PK;
			AssertNoError(vasOrderLine.WVL_OP_ProductInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);

			vasOrderLine.WVL_OP_Product = Factory.New<OrgSupplierPart>().PK;
			AssertHasError(vasOrderLine.WVL_OP_ProductInfo, WhsValidationHelper.ProductRelationshipsErrorMessage);

			vasOrderLine.ReadOnly = true;
			vasOrderLine.WVL_OP_Product = Factory.New<OrgSupplierPart>().PK;
			AssertNoError(vasOrderLine.WVL_OP_ProductInfo, WhsValidationHelper.ProductRelationshipsErrorMessage);

			data.Part1.OP_IsActive = false;
			vasOrderLine.WVL_OP_Product = data.Part1.PK;
			AssertNoError(vasOrderLine.WVL_OP_ProductInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
		}

		#endregion

		#region TestCheckWVL_Quantity

		public void TestCheckWVL_Quantity()
		{
			var vasOrderLine = Factory.New<WhsVASOrderLine>();
			vasOrderLine.WVL_Quantity = 0;
			AssertHasError(vasOrderLine.WVL_QuantityInfo, "Please enter a quantity greater than zero.");

			vasOrderLine.WVL_Quantity = 1;
			AssertNoError(vasOrderLine.WVL_QuantityInfo, "Please enter a quantity greater than zero.");
		}

		#endregion

		#region TestCheckWVL_SerialNumber

		public void TestCheckWVL_SerialNumber()
		{
			var vasOrderLine = Factory.New<WhsVASOrderLine>();
			vasOrderLine.WVL_Quantity = 1m;
			vasOrderLine.WVL_SerialNumber = "SET";
			AssertNoErrors("Precondition: 1 qty and Serial number should have no error", vasOrderLine.WVL_QuantityInfo);

			vasOrderLine.WVL_Quantity = 10m;
			AssertHasError("Should have error for serial number + 10 qty", vasOrderLine.WVL_QuantityInfo, Business.PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			vasOrderLine.WVL_SerialNumber = "";
			AssertNoErrors("Should have no error for no serial number + 10 qty", vasOrderLine.WVL_QuantityInfo);

			vasOrderLine.WVL_SerialNumber = "ere";
			AssertHasError("Should have error for serial number + 10 qty", vasOrderLine.WVL_QuantityInfo, Business.PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

			vasOrderLine.WVL_Quantity = 1m;
			AssertNoErrors("Should have no error for serial number + 1 qty", vasOrderLine.WVL_QuantityInfo);
		}

		#endregion
	}
}
