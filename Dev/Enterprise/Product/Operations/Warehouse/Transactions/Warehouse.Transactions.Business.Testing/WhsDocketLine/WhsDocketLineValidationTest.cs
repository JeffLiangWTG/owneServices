using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketLineValidationTestCase<TDocketLine, TDocket> : WhsBusinessObjectValidationTestCase
		where TDocketLine : WhsDocketLine
		where TDocket : WhsDocket
	{
		#region TestCheckWE_OP

		#region TestCheckWE_OP_InactiveProduct

		public void TestCheckWE_OP_InactiveProduct()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			OrgSupplierPart part = Helper.CreateProduct(Docket.Client, "P1");
			part.OP_IsActive = false;

			DocketLine.WE_OP = part.PK;
			AssertHasError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
			DocketLine.WE_OP = ZGuid.Empty;
			part.OP_IsActive = true;

			DocketLine.WE_OP = part.PK;
			AssertNoError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP

		public void TestCheckWE_OP()
		{
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			AssertHasError(DocketLine.WE_OPInfo, WhsValidationHelper.ProductRelationshipsErrorMessage);

			DocketLine.ReadOnly = true;
			DocketLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			AssertNoError(DocketLine.WE_OPInfo, WhsValidationHelper.ProductRelationshipsErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_InvalidProduct

		public void TestCheckWE_OP_InvalidProduct()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			DocketLine.WE_OP = part.PK;
			DocketLine.SupplierPart.OP_PartNum = "TEST";
			AssertNoError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);

			part.OP_PartNum = CodeLists.ProductType.Codes.Invalid;
			DocketLine.WE_OP = ZGuid.Empty;
			DocketLine.WE_OP = part.PK;
			AssertHasError(DocketLine.WE_OPInfo, OrgSupplierPartCollection.InvalidProductErrorMessage);
		}

		#endregion

		#region TestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed

		public void TestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed()
		{
			if (ShouldRunTestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed)
			{
				var client = Helper.CreateClient("CLIENT");
				var whs = Helper.CreateWarehouse("WHS");
				Helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetClientAttributeType(client, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // should set use Expiry Date.

				var docket = GetNewDocket();
				docket.WD_OH_Client = client.PK;
				docket.WD_WW_Whs = whs.PK;

				var docketLine = docket.Lines.AddNew();

				AssertNoError("Precondition", docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Normal Attribute - NO ERROR
				var partWithNormalAttribute = Helper.CreateProduct(client, "P1");
				Helper.SetProductAttributeUse(client, partWithNormalAttribute, AttributeNumber.One, true);
				docketLine.WE_OP = partWithNormalAttribute.PK;
				AssertNoError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute but without Product-Client-Warehouse parameter - ERROR
				var partWithJulianAttributeButWithoutClientWhsParam = Helper.CreateProduct(client, "P2");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeButWithoutClientWhsParam, AttributeNumber.Two, true); // should set use Expiry Date.
				docketLine.WE_OP = partWithJulianAttributeButWithoutClientWhsParam.PK;
				AssertHasError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute but with wrong client Product-Client-Warehouse parameter - ERROR
				var partWithJulianAttributeAndClientWhsParam_InvalidClient = Helper.CreateProduct(client, "P3");
				var incorrectClient = Helper.CreateClient("CLIENT2");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidClient, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidClient, incorrectClient, whs);
				docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_InvalidClient.PK;
				AssertHasError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute but with wrong warehouse Product-Client-Warehouse parameter - ERROR
				var partWithJulianAttributeAndClientWhsParam_InvalidWhs = Helper.CreateProduct(client, "P4");
				var incorrectWhs = Helper.CreateWarehouse("WH2");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_InvalidWhs, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_InvalidWhs, client, incorrectWhs);
				docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_InvalidWhs.PK;
				AssertHasError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life = 0 - ERROR
				var partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife = Helper.CreateProduct(client, "P5");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife, client, whs);
				docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_ZeroMaxShelfLife.PK;
				AssertHasError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);

				// Product with Julian Batch Number Attribute and with Product-Client-Warehouse parameter that has Maximum Shelf Life > 0 - NO ERROR
				var partWithJulianAttributeAndClientWhsParam_Correct = Helper.CreateProduct(client, "P6");
				Helper.SetProductAttributeUse(client, partWithJulianAttributeAndClientWhsParam_Correct, AttributeNumber.Two, true); // should set use Expiry Date.
				Helper.CreateProductParamsByWhsAndClient(partWithJulianAttributeAndClientWhsParam_Correct, client, whs).W3_MaximumShelfLife = 5;
				docketLine.WE_OP = partWithJulianAttributeAndClientWhsParam_Correct.PK;
				AssertNoError(docketLine.WE_OPInfo, PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage);
			}
			else
			{
				Assert("Validation is not required.", true);
			}
		}

		protected virtual bool ShouldRunTestCheckWE_OP_CanCalculateExpiryDateIfJulianBatchNumberIsUsed
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region TestCheckWE_TransactionQuantity

		public void TestCheckWE_TransactionQuantity()
		{
			TestCheckWE_TransactionQuantityCore();
		}

		protected virtual void TestCheckWE_TransactionQuantityCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			AssertQuantityForSerialNumberProduct(data, WhsDocketLine.Schema.WE_TransactionQuantity);
		}

		#region AssertQuantityForSerialNumberProduct

		protected void AssertQuantityForSerialNumberProduct(TestDataSimpleEnvironment data, ZString propertyName)
		{
			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;

			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;

			var info = docketLine.ZPropertyInfoHash[propertyName];
			info.Value = (ZDecimal)2m;
			AssertNoErrors(info);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			info.Value = (ZDecimal)2m;

			if (docketLine.Validation.IsAttributeValidationRequired)
			{
				AssertNoError(info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				docketLine.WE_SerialNumber = "SN01";

				info.Value = (ZDecimal)2m;
				AssertHasError(info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				info.Value = (ZDecimal)1m;
				AssertNoError(info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
				info.Value = (ZDecimal)2m;
				AssertNoError(info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);

				using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					info.Value = (ZDecimal)2m;
					AssertNoError(info, PartAttributeValidation.MustBeOneOrLessForSerialNumberProductsMessage);
				}
			}
			else
			{
				AssertNoErrors(info);
			}
		}

		#endregion

		#endregion

		#region TestCheckWE_F3_NKPackType

		public void TestCheckWE_F3_NKPackType()
		{
			var docket = GetNewDocket();
			var docketLine = docket.Lines.AddNew();
			docketLine.Validation.ValidateWE_F3_NKPackType();
			AssertMandatoryValidationError(docketLine.WE_F3_NKPackTypeInfo, true);

			docketLine.WE_F3_NKPackType = "UNT";
			AssertMandatoryValidationError(docketLine.WE_F3_NKPackTypeInfo, false);

			var expectedErrorMessage = "The Pack Type {0} has not been defined for this Product.\r\nPack Types are defined on the Maintain -> Warehouse -> Products -> Details -> Unit Conversions Tab.\r\nBecause this Pack Type is not defined, a conversion to Units is not possible.";
			docket.WD_OH_Client = Helper.CreateClient().PK;
			docketLine.WE_OP = Helper.CreateProduct(docket.Client, "P1").PK;
			docketLine.WE_F3_NKPackType = "BAG";
			AssertEquals(false, docketLine.Product.IsPackTypeUsedByProduct("BAG"));
			AssertHasWarning("Not defined PackType warning should exist", docketLine.WE_F3_NKPackTypeInfo, string.Format(expectedErrorMessage, docketLine.WE_F3_NKPackType));

			docketLine.SupplierPart.PartUnits.AddNew();
			docketLine.SupplierPart.PartUnits[0].OF_PackType = "BAG";
			docketLine.Validation.ValidateWE_F3_NKPackType();
			AssertEquals(true, docketLine.Product.IsPackTypeUsedByProduct("BAG"));
			AssertNoWarning("No warning related to define should exist", docketLine.WE_F3_NKPackTypeInfo, string.Format(expectedErrorMessage, docketLine.WE_F3_NKPackType));
		}

		public void TestCheckWE_F3_NKPackType_Error()
		{
			using (WarehouseDataRegistry.Instance.PackTypesValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var docket = GetNewDocket();
				var docketLine = docket.Lines.AddNew();
				docketLine.Validation.ValidateWE_F3_NKPackType();

				var expectedErrorMessage = "The Pack Type {0} has not been defined for this Product.\r\nPack Types are defined on the Maintain -> Warehouse -> Products -> Details -> Unit Conversions Tab.\r\nBecause this Pack Type is not defined, a conversion to Units is not possible.";
				docket.WD_OH_Client = Helper.CreateClient().PK;
				docketLine.WE_OP = Helper.CreateProduct(docket.Client, "P1").PK;
				docketLine.WE_F3_NKPackType = "BAG";

				AssertEquals(false, docketLine.Product.IsPackTypeUsedByProduct("BAG"));
				AssertHasError("Not defined PackType Error should exist", docketLine.WE_F3_NKPackTypeInfo, string.Format(expectedErrorMessage, docketLine.WE_F3_NKPackType));
			}
		}

		#endregion

		#region Location String

		#region TestCheckLocationString

		public void TestCheckLocationString()
		{
			if (IsLocationStringFieldUsed)
			{
				var whs = Helper.CreateWarehouse("WH1", "A");
				var whs1 = Helper.CreateWarehouse("WH2", "B");
				Factory.Save();

				Docket.WD_WW_Whs = whs.PK;

				DocketLine.LocationString = "A";
				AssertLocationHasError(DocketLine.LocationStringInfo, "");

				DocketLine.LocationString = "B";
				AssertLocationHasError(DocketLine.LocationStringInfo, "Please enter a valid Location.");

				DocketLine.LocationString = "X";
				AssertLocationHasError(DocketLine.LocationStringInfo, "Please enter a valid Location.");
			}
			else
			{
				Assert("Location is not used.", true);
			}
		}

		#endregion

		#region TestCheckLocationString_CheckLocationIsInDocketWarehouse

		public void TestCheckLocationString_CheckLocationIsInDocketWarehouse()
		{
			if (IsLocationStringFieldUsed)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

				var whs2 = Helper.CreateWarehouse("WH2", "B");
				var docket = GetNewDocket();
				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = whs2.PK;

				var docketLine = docket.Lines.AddNew();
				docketLine.WE_OP = data.Part1.PK;
				docketLine.WE_TransactionQuantity = 10m;
				docketLine.WE_WL = whs2.DefaultLocation.PK;

				var expectedErrorMessage = ZString.Format("This Location does not belong to the {0}'s Warehouse.", docket.Description);
				AssertNoError(docketLine.LocationStringInfo, expectedErrorMessage);

				docketLine.WE_WL = data.Whs1.DefaultLocation.PK;
				docketLine.Validation.ValidateLocationString();
				AssertHasError(docketLine.LocationStringInfo, expectedErrorMessage);

				docketLine.WE_WL = whs2.DefaultLocation.PK;
				docketLine.Validation.ValidateLocationString();
				AssertNoError(docketLine.LocationStringInfo, expectedErrorMessage);
			}
			else
			{
				Assert("Location is not used.", true);
			}
		}

		#endregion

		#region TestCheckLocationString_FixedWidthLocation

		public void TestCheckLocationString_FixedWidthLocation()
		{
			if (IsLocationStringFieldUsed)
			{
				var warehouse = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
				Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
				Factory.Save();

				var docket = GetNewDocket();
				docket.WD_WW_Whs = warehouse.PK;

				var docketLine = (TDocketLine)docket.Lines.AddNew();
				docketLine.LocationString = "A-1-1-1";
				AssertHasError(docketLine.LocationStringInfo, "Please enter a valid Location.");

				docketLine.LocationString = "A00100101";
				AssertNoErrors(docketLine.LocationStringInfo);

				docketLine.LocationString = "A00100103";
				AssertHasError(docketLine.LocationStringInfo, "Please enter a valid Location.");

				docketLine.LocationString = "A-001-001-01";
				AssertNoErrors(docketLine.LocationStringInfo);
			}
			else
			{
				Assert("Location is not used.", true);
			}
		}

		#endregion

		#region IsLocationFieldUsed

		protected virtual bool IsLocationStringFieldUsed
		{
			get { return true; }
		}

		#endregion

		#region AssertLocationHasError

		protected void AssertLocationHasError(ZPropertyInfo info, ZString message)
		{
			if (!message.IsEmpty)
			{
				AssertHasError(info, message);
			}
			else
			{
				AssertNoErrors(info);
			}
		}

		#endregion

		#endregion

		#region TestAttributes

		#region TestCheckWE_ExpiryDate

		public void TestCheckWE_ExpiryDate()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			DocketLine.WE_OP = Helper.CreateProduct(Docket.Client, "P1").PK;
			using (new PartAttributeValidationChecker.AttributeCallChecker(DocketLine.Validation.IsAttributeValidationRequired
				, DocketLine.Docket.Client
				, DocketLine.SupplierPart
				, DocketLine.WE_ExpiryDateInfo
				, -1
			))
			{
				DocketLine.WE_ExpiryDate = ZDate.Today;
			}

			using (new PartAttributeValidationChecker.AttributeCallChecker(DocketLine.Validation.IsAttributeValidationRequired
				, DocketLine.Docket.Client
				, DocketLine.SupplierPart
				, DocketLine.WE_ExpiryDateInfo
				, -1
			))
			{
				DocketLine.WE_ExpiryDate = ZDate.Today.AddYears(25);
			}
		}

		#endregion

		#region TestCheckWE_ExpiryDateIsValidZDateTimeRange

		[TestDate(2019, 10, 10)]
		public virtual void TestCheckWE_ExpiryDateIsValidZDateTimeRange()
		{
			var docket = GetNewDocket();
			var line = docket.Lines.AddNew();
			line.WE_ExpiryDate = ZDate.Today;
			AssertNoErrors(line.WE_ExpiryDateInfo);

			line.WE_ExpiryDate = ZDate.Today.AddYears(50); // Expiry dates have a larger range
			AssertNoErrors(line.WE_ExpiryDateInfo);

			line.WE_ExpiryDate = new ZDate(1941, 06, 22);
			AssertHasError(line.WE_ExpiryDateInfo, $"The date '22-Jun-1941' is more than {ValidationLimits.PastYearsBeforeError} years old and thus is not valid.");
		}

		#endregion

		#region TestCheckWE_ExpiryDateIsValidZDateTimeRange_PastYearsBeforeWarning

		[TestDate(2019, 6, 4)]
		public virtual void TestCheckWE_ExpiryDateIsValidZDateTimeRange_PastYearsBeforeWarning()
		{
			var docket = GetNewDocket();
			var line = docket.Lines.AddNew();
			line.WE_ExpiryDate = ZDate.Today;
			AssertNoErrors(line.WE_ExpiryDateInfo);

			line.WE_ExpiryDate = ZDate.Today.AddDays(1);
			AssertNoErrors(line.WE_ExpiryDateInfo);

			line.WE_ExpiryDate = ZDate.Today.AddYears(-1).AddDays(-1);
			AssertHasWarning(line.WE_ExpiryDateInfo, ExpectedErrorExpiryDatePastYearsBeforeWarning());
		}

		public virtual string ExpectedErrorExpiryDatePastYearsBeforeWarning() => $"The date '03-Jun-2018' is more than {new TypeValidationLimits().PastYearsBeforeWarning} year old.";

		#endregion

		#region TestCheckWE_PackingDate

		public void TestCheckWE_PackingDate()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			DocketLine.WE_OP = Helper.CreateProduct(Docket.Client, "P1").PK;
			using (new PartAttributeValidationChecker.AttributeCallChecker(DocketLine.Validation.IsAttributeValidationRequired
				, DocketLine.Docket.Client
				, DocketLine.SupplierPart
				, DocketLine.WE_PackingDateInfo
				, -2
			))
			{
				DocketLine.WE_PackingDate = ZDate.Today;
			}
		}

		#endregion

		#region TestCheckWE_PackingDateIsValidZDateTimeRange

		public virtual void TestCheckWE_PackingDateIsValidZDateTimeRange()
		{
			DocketLine.WE_PackingDate = ZDate.Today;
			AssertNoErrors(DocketLine.WE_PackingDateInfo);

			DocketLine.WE_PackingDate = new ZDate(1941, 06, 22);
			AssertHasError(DocketLine.WE_PackingDateInfo, $"The date '22-Jun-1941' is more than {ValidationLimits.PastYearsBeforeError} years old and thus is not valid.");
		}

		#endregion

		#region Test Release Captured Attributes

		#region TestCheckWE_PartAttrib1_WithReleaseCapturedAttributes

		public void TestCheckWE_PartAttrib1_WithReleaseCapturedAttributes()
		{
			AssertCheckPartAttributeWithReleaseCapturedAttributes(AttributeNumber.One, WhsDocketLineSchema.WE_PartAttrib1, line => line.WE_PartAttrib1Info);
		}

		#endregion

		#region TestCheckWE_PartAttrib2_WithReleaseCapturedAttributes

		public void TestCheckWE_PartAttrib2_WithReleaseCapturedAttributes()
		{
			AssertCheckPartAttributeWithReleaseCapturedAttributes(AttributeNumber.Two, WhsDocketLineSchema.WE_PartAttrib2, line => line.WE_PartAttrib2Info);
		}

		#endregion

		#region TestCheckWE_PartAttrib3_WithReleaseCapturedAttributes

		public void TestCheckWE_PartAttrib3_WithReleaseCapturedAttributes()
		{
			AssertCheckPartAttributeWithReleaseCapturedAttributes(AttributeNumber.Three, WhsDocketLineSchema.WE_PartAttrib3, line => line.WE_PartAttrib3Info);
		}

		#endregion

		void AssertCheckPartAttributeWithReleaseCapturedAttributes(AttributeNumber attributeNumber, SchemaColumn attributeColumn, Func<TDocketLine, ZPropertyInfo> getPropertyInfo)
		{
			var docket = GetNewDocket();
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			docket.WD_OH_Client = data.Org1.PK;

			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.VIN);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true, setReleaseCaptured: true);

			var docketLine = (TDocketLine)docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 2m;
			var propertyInfo = getPropertyInfo(docketLine);
			AssertNoErrors("Precondition:", propertyInfo);

			docketLine[attributeColumn] = "Some Data";
			if (IsReleaseCapturedValidationRequired(docketLine))
			{
				AssertHasError(propertyInfo, "This attribute is specified as Release Captured for this Product, no value should be entered.");
			}
			else
			{
				AssertNoErrors(propertyInfo);
			}

			docketLine[attributeColumn] = "";
			AssertNoErrors(propertyInfo);
		}

		protected virtual bool IsReleaseCapturedValidationRequired(TDocketLine line)
		{
			return line.Validation.IsAttributeValidationRequired;
		}

		#endregion

		#region TestCheckWE_PartAttrib1

		public void TestCheckWE_PartAttrib1()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			DocketLine.WE_OP = Helper.CreateProduct(Docket.Client, "P1").PK;
			using (new PartAttributeValidationChecker.AttributeCallChecker(DocketLine.Validation.IsAttributeValidationRequired
				, DocketLine.Docket.Client
				, DocketLine.SupplierPart
				, DocketLine.WE_PartAttrib1Info
				, 1
			))
			{
				DocketLine.WE_PartAttrib1 = "AAA";
			}

			DocketLine.WE_PartAttrib1 = "   AAA";
			AssertHasError(DocketLine.WE_PartAttrib1Info, WhsValidationHelper.ValueHasToBeTrimmed);
		}

		public void TestCheckWE_PartAttrib1_JulianBatchNumber()
		{
			TestCheckWE_PartAttrib_JulianBatchNumberCore(WhsDocketLineSchema.WE_PartAttrib1, AttributeNumber.One, line => line.WE_PartAttrib1Info);
		}

		#endregion

		#region TestCheckWE_PartAttrib2

		public void TestCheckWE_PartAttrib2()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			DocketLine.WE_OP = Helper.CreateProduct(Docket.Client, "P1").PK;
			using (new PartAttributeValidationChecker.AttributeCallChecker(DocketLine.Validation.IsAttributeValidationRequired
				, DocketLine.Docket.Client
				, DocketLine.SupplierPart
				, DocketLine.WE_PartAttrib2Info
				, 2
			))
			{
				DocketLine.WE_PartAttrib2 = "AAA";
			}

			DocketLine.WE_PartAttrib2 = "   AAA";
			AssertHasError(DocketLine.WE_PartAttrib2Info, WhsValidationHelper.ValueHasToBeTrimmed);
		}

		public void TestCheckWE_PartAttrib2_JulianBatchNumber()
		{
			TestCheckWE_PartAttrib_JulianBatchNumberCore(WhsDocketLineSchema.WE_PartAttrib2, AttributeNumber.Two, line => line.WE_PartAttrib2Info);
		}

		#endregion

		#region TestCheckWE_PartAttrib3

		public void TestCheckWE_PartAttrib3()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;
			DocketLine.WE_OP = Helper.CreateProduct(Docket.Client, "P1").PK;
			using (new PartAttributeValidationChecker.AttributeCallChecker(DocketLine.Validation.IsAttributeValidationRequired
				, DocketLine.Docket.Client
				, DocketLine.SupplierPart
				, DocketLine.WE_PartAttrib3Info
				, 3
			))
			{
				DocketLine.WE_PartAttrib3 = "AAA";
			}

			DocketLine.WE_PartAttrib3 = "   AAA";
			AssertHasError(DocketLine.WE_PartAttrib3Info, WhsValidationHelper.ValueHasToBeTrimmed);
		}

		public void TestCheckWE_PartAttrib3_JulianBatchNumber()
		{
			TestCheckWE_PartAttrib_JulianBatchNumberCore(WhsDocketLineSchema.WE_PartAttrib3, AttributeNumber.Three, line => line.WE_PartAttrib3Info);
		}

		#endregion

		#region TestCheckWE_SerialNumber

		public void TestCheckWE_SerialNumber_Required()
		{
			TestCheckWE_SerialNumber_RequiredCore();
		}

		protected virtual void TestCheckWE_SerialNumber_RequiredCore()
		{
			var docket = GetNewDocket();
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			docket.WD_OH_Client = data.Org1.PK;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 1m;
			docketLine.WE_SerialNumber = "Ser";
			AssertNoErrors("Precondition: No errors for serial number existing", docketLine.WE_SerialNumberInfo);

			docketLine.WE_SerialNumber = "";
			if (docketLine.Validation.IsAttributeValidationRequired)
			{
				AssertHasError("Errors for missing manadatory serial number", docketLine.WE_SerialNumberInfo, "Please enter a Serial Number.");

				docketLine.WE_SerialNumber = "FRD";
			}
			AssertNoErrors("No errors for another serial number", docketLine.WE_SerialNumberInfo);
		}

		public void TestCheckWE_SerialNumber_TrimmedCorrectly()
		{
			TestCheckWE_SerialNumber_TrimmedCorrectlyCore();
		}

		protected virtual void TestCheckWE_SerialNumber_TrimmedCorrectlyCore()
		{
			var docket = GetNewDocket();
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			docket.WD_OH_Client = data.Org1.PK;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var docketLine = docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 1m;
			docketLine.WE_SerialNumber = "Ser";
			if (docketLine.Validation.IsAttributeValidationRequired)
			{
				AssertNoErrors("Precondition: No errors for serial number", docketLine.WE_SerialNumberInfo);
				docketLine.WE_SerialNumber = "   Ser";
				AssertHasError("Errors for serial number untrimmed", docketLine.WE_SerialNumberInfo, WhsValidationHelper.ValueHasToBeTrimmed);

				docketLine.WE_SerialNumber = "TGT";
			}
			AssertNoErrors("No errors for another serial number", docketLine.WE_SerialNumberInfo);
		}

		public void TestCheckWE_SerialNumber_ReleaseCaptured()
		{
			TestCheckWE_SerialNumber_ReleaseCapturedCore();
		}

		protected virtual void TestCheckWE_SerialNumber_ReleaseCapturedCore()
		{
			var docket = GetNewDocket();
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			docket.WD_OH_Client = data.Org1.PK;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var docketLine = (TDocketLine)docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			docketLine.WE_TransactionQuantity = 1m;
			var propertyInfo = docketLine.WE_SerialNumberInfo;
			AssertNoErrors("Precondition:", propertyInfo);

			docketLine.WE_SerialNumber = "Some Data";
			if (IsReleaseCapturedValidationRequired(docketLine))
			{
				AssertHasError(propertyInfo, "Serial Number is specified as Release Captured for this Product, no value should be entered.");
			}
			else
			{
				AssertNoErrors(propertyInfo);
			}

			docketLine.WE_SerialNumber = "";
			AssertNoErrors(propertyInfo);
		}

		#endregion

		#region TestIsAttributeValidationRequired

		public virtual void TestIsAttributeValidationRequired()
		{
			AssertEquals(false, DocketLine.Validation.IsAttributeValidationRequired);
		}

		#endregion

		#region TestCheckWE_PartAttrib_JulianBatchNumber_MultiAttribute

		public void TestCheckWE_PartAttrib_JulianBatchNumber_MultiAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;

			TestCheckWE_PartAttrib_JulianBatchNumber_AdditionalSetup(data);

			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLine = (TDocketLine)docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			AssertTestCheckWE_PartAttrib_JulianBatchNumber_MultiAttribute(docketLine);
		}

		protected virtual void AssertTestCheckWE_PartAttrib_JulianBatchNumber_MultiAttribute(TDocketLine docketLine)
		{
			docketLine.WE_PartAttrib1 = "ABCD";
			docketLine.WE_PartAttrib2 = "ABCD";
			docketLine.WE_PartAttrib3 = "ABCD";

			docketLine.Validation.ValidateAll();
			AssertJulianBatchNumberAttribute(AssertHasError, docketLine.WE_PartAttrib1Info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			AssertJulianBatchNumberAttribute(AssertNoError, docketLine.WE_PartAttrib2Info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			AssertJulianBatchNumberAttribute(AssertNoError, docketLine.WE_PartAttrib3Info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
		}

		protected virtual void TestCheckWE_PartAttrib_JulianBatchNumber_AdditionalSetup(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestCheckWE_PartAttrib_JulianBatchNumberCore

		void TestCheckWE_PartAttrib_JulianBatchNumberCore(SchemaStringColumn partAttributeSchemaColumn, AttributeNumber attributeNumber, Func<TDocketLine, ZPropertyInfo> getPropertyInfo)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, true);
			data.Part1.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YYDDD;

			TestCheckWE_PartAttrib_JulianBatchNumber_AdditionalSetup(data);

			var docket = GetNewDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLine = (TDocketLine)docket.Lines.AddNew();
			docketLine.WE_OP = data.Part1.PK;
			var info = getPropertyInfo(docketLine);
			AssertTestCheckWE_PartAttrib_JulianBatchNumber(data, info, attributeNumber);
		}

		protected virtual void AssertTestCheckWE_PartAttrib_JulianBatchNumber(TestDataSimpleEnvironment data, ZPropertyInfo info, AttributeNumber attributeNumber)
		{
			var docketLine = info.BizObj as TDocketLine;
			if (docketLine != null)
			{
				AssertJulianBatchNumberAttribute(AssertNoError, info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				docketLine[info.Name] = "ABCD";
				AssertJulianBatchNumberAttribute(AssertNoError, info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				Helper.SetClientAttributeType(data.Org1, attributeNumber, PartAttributeTypeList.Codes.JulianBatchNumber);
				docketLine[info.Name] = "ABCD";
				AssertJulianBatchNumberAttribute(AssertHasError, info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				docketLine[info.Name] = "12345ABCD";
				AssertJulianBatchNumberAttribute(AssertHasError, info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);

				docketLine[info.Name] = "ABCD12345";
				AssertJulianBatchNumberAttribute(AssertNoError, info, PartAttributeValidation.JulianBatchNumberIncorrectFormatMessage);
			}
		}

		protected void AssertJulianBatchNumberAttribute(Action<ZPropertyInfo, string> assert, ZPropertyInfo info, string expectedErrorMessage)
		{
			if (IsJulianBatchNumberFormatValidationRequired(info))
			{
				assert(info, expectedErrorMessage);
			}
			else
			{
				Assert("No validation Required", true);
			}
		}

		protected virtual bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo)
		{
			return true;
		}

		#endregion

		#endregion

		#region TestCheckWE_CustomAttributes

		public void TestCheckWE_CustomAttributes()
		{
			Docket.WD_OH_Client = Helper.CreateClient().PK;

			if (((ICustomLabelsConfigOrgProvider)DocketLine).ConfigOrg != null)
			{
				new CustomLabelsTestCase(DocketLine, new WhsDocketLine.CustomLabelsProvider(DocketLine.Docket)).TestCustomLabelsMandatoryValidation();
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestCheckWE_TransferFromPalletID

		public void TestCheckWE_TransferFromPalletID()
		{
			DocketLine.WE_TransferFromPalletId = "AAA";
			AssertNoErrors(DocketLine.WE_TransferFromPalletIdInfo);

			DocketLine.WE_TransferFromPalletId = "   AAA";
			AssertHasError(DocketLine.WE_TransferFromPalletIdInfo, WhsValidationHelper.ValueHasToBeTrimmed);

			TestCheckWE_WE_TransferFromPalletIdCore();
		}

		protected virtual void TestCheckWE_WE_TransferFromPalletIdCore()
		{
		}

		#endregion

		#region TestCheckWE_PalletID

		public void TestCheckWE_PalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			DocketLine.WE_PalletID = "AAA";
			AssertNoErrors(DocketLine.WE_PalletIDInfo);

			DocketLine.WE_PalletID = "   AAA";
			AssertHasError(DocketLine.WE_PalletIDInfo, WhsValidationHelper.ValueHasToBeTrimmed);

			TestCheckWE_WE_PalletIDCore(data);
		}

		protected virtual void TestCheckWE_WE_PalletIDCore(TestDataSimpleEnvironment data)
		{
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode

		public virtual void TestCheckWE_WHC_NKOriginalInventoryHeldCode()
		{
			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = "";
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = "DAM";
			AssertNoErrors("Should only validate held code when receiving in inventory/changing held code.", DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld

		public virtual void TestCheckWE_WHC_NKOriginalInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_TransactionQuantity = -1; // Special case for stocktake adjustments
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo);

			DocketLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region TestCheckWE_WHC_NKOriginalInventoryHeldCode_ListValidation

		public virtual void TestCheckWE_WHC_NKOriginalInventoryHeldCode_ListValidation()
		{
			var invalidHoldCode = "ZZZ";
			AssertEquals("Precondition: invalidholdcode not in the list of InventoryHeldCodeCollection.", false,
				DocketLine.Lookups.InventoryHeldCodeCollection.ToArray().Any(code => code.Code == invalidHoldCode));
			DocketLine.WE_WHC_NKOriginalInventoryHeldCode = invalidHoldCode;
			AssertHasError(DocketLine.WE_WHC_NKOriginalInventoryHeldCodeInfo, "Enter a valid Hold Code.");
		}

		#endregion

		#region TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld

		public virtual void TestCheckWE_WHC_NKCurrentInventoryHeldCode_MandatoryWhenHeld()
		{
			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please enter a Hold Code.");

			DocketLine.WE_TransactionQuantity = -1; // Special case for stocktake adjustments
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo);

			DocketLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKCurrentInventoryHeldCodeInfo, "Please do not enter a Hold Code.");
		}

		#endregion

		#region TestCheckWE_WHC_NKOrderedHeldCode

		public void TestCheckWE_WHC_NKOrderedHeldCode()
		{
			TestCheckWE_WHC_NKOrderedHeldCodeCore();
		}

		protected virtual void TestCheckWE_WHC_NKOrderedHeldCodeCore()
		{
			DocketLine.Validation.ValidateAll();
			AssertNoErrors(DocketLine.WE_WHC_NKOrderedHeldCodeInfo);

			DocketLine.WE_WHC_NKOrderedHeldCode = InventoryStatus.Codes.Held;
			DocketLine.Validation.ValidateAll();
			AssertHasError(DocketLine.WE_WHC_NKOrderedHeldCodeInfo, "Please do not enter a value.");
		}

		#endregion

		#region TestCheckWE_AdjustmentArrivalDateIsValidZDateTimeRange

		public virtual void TestCheckWE_AdjustmentArrivalDateIsValidZDateTimeRange()
		{
			DocketLine.WE_AdjustmentArrivalDate = ZDateTimeOffset.Now;
			AssertNoErrors(DocketLine.WE_AdjustmentArrivalDateInfo);

			DocketLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(1941, 06, 22);
			AssertHasError(DocketLine.WE_AdjustmentArrivalDateInfo, $"The date '22-Jun-1941' is more than {ValidationLimits.PastYearsBeforeError} years old and thus is not valid.");
		}

		#endregion

		#region TestCheckWE_RequiredByDate

		public void TestCheckWE_RequiredByDate()
		{
			AssertEquals(false, DocketLine.WE_RequiredByDateInfo.HasErrors());

			DocketLine.WE_RequiredByDate = ZDateTimeOffset.Invalid;
			AssertEquals(true, DocketLine.WE_RequiredByDateInfo.HasErrors());
		}

		#endregion

		#region TestCheckWE_RequiredByDate_IsValidZDateTimeRange

		public virtual void TestCheckWE_RequiredByDate_IsValidZDateTimeRange()
		{
			DocketLine.WE_RequiredByDate = ZDateTimeOffset.Now;
			AssertNoErrors(DocketLine.WE_RequiredByDateInfo);

			DocketLine.WE_RequiredByDate = new ZDateTimeOffset(1941, 06, 22);
			AssertHasError(DocketLine.WE_RequiredByDateInfo, $"The date '22-Jun-1941' is more than {ValidationLimits.PastYearsBeforeError} years old and thus is not valid.");
		}

		#endregion

		#region TestCheckWE_ClientOrderedUnits

		public void TestCheckWE_ClientOrderedUnits()
		{
			TestMinDecimal(DocketLine.WE_ClientOrderedUnitsInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestCheckWE_DockLineType

		public void TestCheckWE_DockLineType()
		{
			var statusList = new DocketStatus();
			foreach (var status in statusList.GetAllCodes())
			{
				var docket = Docket;
				var line = docket.Lines.AddNew();
				line.WE_DocketLineStatus = status;
				var type = line.WE_DocketLineType;
				line.WE_DocketLineType = "";
				line.WE_DocketLineType = type;

				if (ValidStatuses.Contains(status))
				{
					AssertNoErrors(line.WE_DocketLineTypeInfo);
				}
				else
				{
					var errorMessage = string.Format("An expected error has occurred while saving the docket lines: Docket line type can't be '{0}' with status '{1}'. Please reload the form to proceed your changes.", line.WE_DocketLineType, line.WE_DocketLineStatus);
					AssertHasError(line.WE_DocketLineTypeInfo, errorMessage);
				}
			}
		}

		#endregion

		#region TestCheckWE_DocketLineStatus

		public void TestCheckWE_DocketLineStatus()
		{
			var statusList = new DocketStatus();
			foreach (var status in statusList.GetAllCodes())
			{
				var docket = Docket;
				var line = docket.Lines.AddNew();
				line.WE_DocketLineStatus = status;
				if (ValidStatuses.Contains(status))
				{
					AssertNoErrors(line.WE_DocketLineStatusInfo);
				}
				else
				{
					var errorMessage = string.Format("An expected error has occurred while saving the docket lines: Docket line status can't be '{0}' with type '{1}'. Please reload the form to proceed your changes.", line.WE_DocketLineStatus, line.WE_DocketLineType);
					AssertHasError(line.WE_DocketLineStatusInfo, errorMessage);
				}
			}
		}

		#region ValidStatuses

		protected virtual IEnumerable<ZString> ValidStatuses => new ZString[] { "", "FIN", "CAN" };

		#endregion

		#endregion

		#region TestFKsToNotValidateForCancelledRecords

		public void TestShouldValidateFKToCancelledRecord()
		{
			var docket = Docket;
			var line = docket.Lines.AddNew();
			var validation = new TestDocketLineValidation(line);

			var list = new string[]
			{
				WhsDocketLineSchema.Constants.WE_WL,
				WhsDocketLineSchema.Constants.WE_WL_TransferFrom,
				WhsDocketLineSchema.Constants.WE_WE_MatchingLine,
				WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating,
				WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine,
				WhsDocketLineSchema.Constants.WE_WPL_PutawayLine,
				WhsDocketLineSchema.Constants.WE_F3_NKPackType,
				WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_WHC_NKCurrentInventoryHeldCode,
				WhsDocketLineSchema.Constants.WE_P9_Task,
			};

			foreach (var propertyInfo in line.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(p => p.IsPersistent))
			{
				if (list.Contains(propertyInfo.Name))
				{
					AssertEquals("NK/FK which cannot be cancelled.", false, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
				else
				{
					AssertEquals("All other properties should just return base condition of true.", true, validation.ShouldValidateFKToCancelledRecordExposed(propertyInfo));
				}
			}
		}

		#endregion

		// calculated properties

		#region TestCheckHeldCodeChangeQuantity

		public void TestCheckHeldCodeChangeQuantity()
		{
			var docketLine = GetNewDocketLineReadyToFinalise();
			docketLine.Docket.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(docketLine.Docket.Client, docketLine.Docket.Warehouse, "1", Notify);
			Helper.CreateWhsOrderLine(order, docketLine.SupplierPart, 5m);
			Helper.CreatePickNew(order);

			if (CanHaveInventoryAttached)
			{
				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeChangeQuantity = 10m;
				AssertHasError(docketLine.HeldCodeChangeQuantityInfo, "Quantity cannot be greater than Available To Transfer Quantity");

				docketLine.HeldCodeChangeQuantity = 5m;
				AssertNoErrors(docketLine.HeldCodeChangeQuantityInfo);

				docketLine.HeldCodeChangeQuantity = 6m;
				AssertHasErrors(docketLine.HeldCodeChangeQuantityInfo);

				docketLine.HeldCodeChangeQuantity = 4m;
				AssertNoErrors(docketLine.HeldCodeChangeQuantityInfo);

				docketLine.HeldCodeChangeQuantity = -1m;
				AssertHasErrors(docketLine.HeldCodeChangeQuantityInfo);
			}
			else
			{
				Assert("Can not be attached to inventory, impossible to change held code.", true);
			}
		}

		#endregion

		#region TestCheckHeldCodeChangeQuantity_ConsidersReservedStock

		public void TestCheckHeldCodeChangeQuantity_ConsidersReservedStock()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.Docket.FinaliseDocket();
				Factory.Save();

				var order = Helper.CreateWhsOrder(docketLine.Docket.Client, docketLine.Docket.Warehouse, "1", Notify);
				var orderLine = Helper.CreateWhsOrderLine(order, docketLine.SupplierPart, 6m);
				orderLine.ReserveStockIfAbleTo(docketLine.Inventory[0]);
				AssertNoErrors("Precondition", docketLine.HeldCodeChangeQuantityInfo);

				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeChangeQuantity = 4m;
				AssertNoErrors(docketLine.HeldCodeChangeQuantityInfo);

				docketLine.HeldCodeChangeQuantity = 5m;
				AssertHasError(docketLine.HeldCodeChangeQuantityInfo, "Quantity cannot be greater than Available To Transfer Quantity");

				docketLine.HeldCodeChangeQuantity = 4m;
				AssertNoErrors(docketLine.HeldCodeChangeQuantityInfo);
			}
			else
			{
				Assert("Can not be attached to inventory, impossible to change held code.", true);
			}
		}

		#endregion

		#region TestCheckHeldCodeToChangeTo

		public void TestCheckHeldCodeToChangeTo()
		{
			if (CanHaveInventoryAttached)
			{
				var docketLine = GetNewDocketLineReadyToFinalise();
				docketLine.Docket.FinaliseDocket();
				Factory.Save();

				docketLine.IsInventoryEditForm = true;
				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
				AssertNoErrors(docketLine.HeldCodeToChangeToInfo);

				docketLine.HeldCodeToChangeTo = "ABC";
				AssertHasError(docketLine.HeldCodeToChangeToInfo, "Enter a valid Hold Code Change.");

				docketLine.HeldCodeToChangeTo = "";
				AssertHasError(docketLine.HeldCodeToChangeToInfo, "The current Hold Code of this record is ''. This field is used to change the current Hold Code, so it is not valid to select the current Hold Code.");

				docketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
				AssertNoErrors(docketLine.HeldCodeToChangeToInfo);

				docketLine.IsInventoryEditForm = true;
				Factory.Save(); // Change Status + Held Code

				var heldDocketLine = Factory.LoadTop1<TDocketLine>(new ZQuery(WhsDocketLineSchema.WE_CurrentInventoryStatus, InventoryStatus.Codes.Held));
				heldDocketLine.HeldCodeToChangeTo = string.Empty;
				AssertNoErrors(heldDocketLine.HeldCodeToChangeToInfo);
			}
			else
			{
				Assert("Can not be attached to inventory, impossible to change held code.", true);
			}
		}

		#endregion

		#region Locations

		protected bool HasMessageThatContainParts(ZPropertyInfo info, string[] expectedPartsOfTheWarningMessage, bool isErrorExpected = false)
		{
			var messages = isErrorExpected ? info.GetErrors() : info.GetWarnings();
			foreach (var notification in messages)
			{
				if (notification.Message.Contains(expectedPartsOfTheWarningMessage[0]) &&
					notification.Message.Contains(expectedPartsOfTheWarningMessage[1]) &&
					notification.Message.Contains(expectedPartsOfTheWarningMessage[2]))
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region TestCheckWE_GS_NKPutawayBy

		public void TestCheckWE_GS_NKPutawayBy()
		{
			var user = Helper.CreateGlbStaff("AAA", "A.A");
			var errorMessage = ListValidation.InvalidCodeError + DocketLine.WE_GS_NKPutawayByInfo.HumanReadableName + ".";

			DocketLine.WE_GS_NKPutawayBy = "";
			AssertNoErrors(DocketLine.WE_GS_NKPutawayByInfo);

			DocketLine.WE_GS_NKPutawayBy = "AAA";
			AssertNoErrors(DocketLine.WE_GS_NKPutawayByInfo);

			DocketLine.WE_GS_NKPutawayBy = "BBB";
			AssertHasError(DocketLine.WE_GS_NKPutawayByInfo, errorMessage);
		}

		#endregion

		#region TestCheckWE_StockOnHandInfo

		public void TestCheckWE_StockOnHandInfo()
		{
			TestMinDecimal(DocketLine.WE_StockOnHandInfo, ErrorCheckType.HasErrors, 0);
		}

		#endregion

		#region TestCheckWE_PackQuantity

		public void TestCheckWE_PackQuantity()
		{
			var docket = GetNewDocket();
			var docketLine = docket.Lines.AddNew();
			docketLine.WE_PackQuantity = 10m;
			AssertNoErrors(docketLine.WE_PackQuantityInfo);

			docketLine.WE_PackQuantity = 123456789123456789m;
			AssertHasError(docketLine.WE_PackQuantityInfo, "The number 123,456,789,123,456,789 is too large, the maximum value allowed for selection is 999,999,999,999,999.999.");
		}

		#endregion

		#region TestCalculateCapacity

		public void TestCalculateCapacity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client = data.Org1;
			var part = data.Part1;
			var partWeight = data.Part2;
			var partVolume = Helper.CreateProduct(client, "Part3");
			var partWeightVolume = Helper.CreateProduct(client, "Part4");
			var whs = data.Whs1;

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var location1 = locations[0];
			var location2 = locations[1];

			Helper.SetLocationMaxWeightAndVolume(location2, 20m, Constants.Weight.Kilograms, 12m, Constants.Volume.CubicMetres); // max Weight specified
			location2.WLV_MaxQuantity = 10;
			Helper.SetProductWeightAndVolume(part, 0m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(partWeight, 5m, Constants.Weight.Kilograms, 0m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(partVolume, 0m, Constants.Weight.Kilograms, 4m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(partWeightVolume, 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, "R1", partWeightVolume, 3m, location2, "");
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var docket = Factory.New<TDocket>();
			var availableCapacity = docket.LocationCapacityValidationManager.GetLocationAvailableCapacity(location2);
			AssertEquals("Location capacity - receive already in this location ", 10m - 3m, availableCapacity.Quantity);
			AssertEquals("Location capacity - receive already in this location ", 20m - 3m, availableCapacity.Weight);
			AssertEquals("Location capacity - receive already in this location ", 12m - 3m, availableCapacity.Volume);

			if (CanHaveInventoryAttached)
			{
				var line = (TDocketLine)docket.Lines.AddNew();
				line.Docket.WD_WW_Whs = whs.PK;

				AssertLocationPropInfoHasError(line, part, partWeight, partVolume, location2, () => line.Validation.ValidateWE_WL(), line.WE_WLInfo);
				if (ValidateLocationString)
				{
					line = (TDocketLine)docket.Lines.AddNew();
					line.Docket.WD_WW_Whs = whs.PK;

					AssertLocationPropInfoHasError(line, part, partWeight, partVolume, location2, () => line.Validation.ValidateLocationString(), line.LocationStringInfo);
				}
			}
		}

		public void TestCalculateCapacity_Staged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetLocationMaxWeightAndVolume(data.Whs1.DefaultOutboundDockDoorLocation, 20m, Constants.Weight.Kilograms, 12m, Constants.Volume.CubicMetres); // max Weight specified
			data.Whs1.DefaultOutboundDockDoorLocation.WLV_MaxQuantity = 10;
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 8m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var docket = Factory.New<TDocket>();

			if (CanHaveInventoryAttached)
			{
				var line = (TDocketLine)docket.Lines.AddNew();
				line.Docket.WD_WW_Whs = data.Whs1.PK;

				var outboundDDL = data.Whs1.DefaultOutboundDockDoorLocation;
				line.WE_TransactionQuantity = 5m;
				line.WE_OP = data.Part1.PK;
				line.WE_WL = outboundDDL.PK;

				line.Validation.ValidateWE_WL();
				AssertHasError(line.WE_WLInfo, "Total required Quantity (5) exceeds the maximum available Quantity (2.000) for this location.");

				if (ValidateLocationString)
				{
					line.LocationString = outboundDDL.HumanReadableShortcutName;
					line.Validation.ValidateLocationString();
					AssertHasError(line.LocationStringInfo, "Total required Quantity (5) exceeds the maximum available Quantity (2.000) for this location.");
				}
			}
		}

		void AssertLocationPropInfoHasError(TDocketLine line, OrgSupplierPart part, OrgSupplierPart partWeight, OrgSupplierPart partVolume, WhsLocation location2, Action validate, ZPropertyInfo propInfo)
		{
			using (line.SuspendValidationTesting())
			{
				// Check Quantity
				line.WE_TransactionQuantity = 5;
				line.WE_OP = part.PK;
				line.WE_WL = location2.PK;
				line.LocationString = location2.HumanReadableShortcutName;
				AssertNoErrors(propInfo);
				AssertNoWarnings(propInfo);

				line.WE_TransactionQuantity = 10;
				validate();
				AssertHasError(propInfo, "Total required Quantity (10) exceeds the maximum available Quantity (7.000) for this location.");
				AssertNoWarnings(propInfo);

				// Check Weight
				line.WE_TransactionQuantity = 1;
				propInfo.ClearAllNotifications();
				line.WE_OP = partWeight.PK;
				validate();
				AssertNoErrors(propInfo);
				AssertNoWarnings(propInfo);

				line.WE_TransactionQuantity = 5;
				propInfo.ClearAllNotifications();
				line.WE_OP = partWeight.PK;
				validate();
				AssertNoErrors(propInfo);
				AssertHasWarning(propInfo, "Total required Weight (25.00 KG) exceeds the maximum available Weight (17.00 KG) for this location.");

				// Check Volume
				propInfo.ClearAllNotifications();
				line.WE_OP = partVolume.PK;
				validate();
				AssertNoErrors(propInfo);
				AssertHasWarning(propInfo, "Total required Volume (20.000 M3) exceeds the maximum available Volume (9.000 M3) for this location.");

				line.WE_TransactionQuantity = 10;
				propInfo.ClearAllNotifications();
				line.WE_OP = partVolume.PK;
				validate();
				AssertHasErrors(propInfo);
				AssertNoWarnings(propInfo);

				// delete should clear required cache
				var docket = line.Docket;
				line.Delete();
				var requiredCapacity = docket.LocationCapacityValidationManager.GetLocationRequiredCapacity(location2);
				AssertEquals("line is deleted", 0m, requiredCapacity.Quantity);
				AssertEquals("line is deleted", 0m, requiredCapacity.Weight);
				AssertEquals("line is deleted", 0m, requiredCapacity.Volume);
			}
		}

		#endregion

		#region TestCalculateCapacity_InTransit

		public void TestCalculateCapacity_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client = data.Org1;
			var sourceLocation = data.Whs1.FindLocation("A-1");
			sourceLocation.WLV_MaxQuantity = 10m;
			var destLocation = data.Whs1.FindLocation("A-2");
			destLocation.WLV_MaxQuantity = 10m;
			var myProduct = Helper.CreateProduct(client, "Part4");

			Helper.SetLocationMaxWeightAndVolume(sourceLocation, 20m, Constants.Weight.Kilograms, 12m, Constants.Volume.CubicMetres);
			Helper.SetLocationMaxWeightAndVolume(destLocation, 15m, Constants.Weight.Kilograms, 12m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(myProduct, 1m, Constants.Weight.Kilograms, 1m, Constants.Volume.CubicMetres);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(client, data.Whs1, "R1", myProduct, 5m, sourceLocation, "");
			var transfer = Helper.CreateWhsTransfer(client, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, myProduct, 5m, sourceLocation, destLocation);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Inventory status should be InTransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Inventory status should be InTransit.", 5m, transferLine.QtyCommittedIncludingMatchingLines);
			Factory.Save();

			var availableCapacity = transfer.LocationCapacityValidationManager.GetLocationAvailableCapacity(sourceLocation);
			AssertEquals("Location capacity - Inventory is in transit ", 10m, availableCapacity.Quantity);
			AssertEquals("Location capacity - Inventory is in transit ", 20m, availableCapacity.Weight);
			AssertEquals("Location capacity - Inventory is in transit ", 12m, availableCapacity.Volume);

			transfer.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			availableCapacity = receive.LocationCapacityValidationManager.GetLocationAvailableCapacity(destLocation); // use receive's location capacity check so transfer is not excluded from capacity check
			AssertEquals("Location capacity - Inventory is in transit ", 10m - 5m, availableCapacity.Quantity);
			AssertEquals("Location capacity - Inventory is in transit ", 15m - 5m, availableCapacity.Weight);
			AssertEquals("Location capacity - Inventory is in transit ", 12m - 5m, availableCapacity.Volume);
		}

		#endregion

		#region Implementation

		protected TDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
		}

		protected virtual TDocket GetNewDocket()
		{
			return Factory.New<TDocket>();
		}

		protected TDocketLine GetNewDocketLine()
		{
			return Factory.New<TDocketLine>();
		}

		protected TDocketLine DocketLine
		{
			get { return docketLine ?? (docketLine = (TDocketLine)Docket.Lines.AddNew()); }
			set { docketLine = value; }
		}

		protected virtual bool CanHaveInventoryAttached
		{
			get { return true; }
		}

		protected virtual bool ValidateLocationString
		{
			get { return true; }
		}

		#region GetNewDocketLineReadyToFinalise

		protected TDocketLine GetNewDocketLineReadyToFinalise(TestDataSimpleEnvironment data = null)
		{
			data = data ?? new TestDataSimpleEnvironment(Factory, 2, 1);
			return (TDocketLine)DocketHelper.GetNewFinalisableDocketWithOneLine(data).Lines[0];
		}

		FinalisableDocketHelper<TDocket> DocketHelper
		{
			get { return docketHelper ?? (docketHelper = GetNewDocketHelper()); }
		}

		protected abstract FinalisableDocketHelper<TDocket> GetNewDocketHelper();
		FinalisableDocketHelper<TDocket> docketHelper;

		#endregion

		#region TestDocketLineValidation

		class TestDocketLineValidation : WhsDocketLineValidation
		{
			public TestDocketLineValidation(WhsDocketLine parent)
				: base(parent)
			{
			}

			public bool ShouldValidateFKToCancelledRecordExposed(ZPropertyInfo info) => ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		TDocket docket;
		TDocketLine docketLine;

		#endregion
	}
}
