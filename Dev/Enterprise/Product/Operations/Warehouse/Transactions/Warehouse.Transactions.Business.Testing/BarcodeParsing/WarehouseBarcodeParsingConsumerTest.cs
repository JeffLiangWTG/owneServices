using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WarehouseBarcodeParsingConsumer))]
	class WarehouseBarcodeParsingConsumerTest : BarcodeParsingConsumerTestCase
	{
		#region Captions

		protected override string ExpectedBuyerCaption => "Client";
		protected override string ExpectedRelatedEntityCaption => "Product";
		protected override string ExpectedSupplierCaption => null;

		#endregion

		#region Collections

		protected override Type ExpectedTypeOfBuyers => typeof(WarehouseClientCollection);
		protected override Type ExpectedTypeOfSuppliers => typeof(ConsignorCollection);

		#endregion

		#region Flags

		protected override bool ExpectedIsBuyerAvailable => true;
		protected override bool ExpectedIsRelatedEntityAvailable => true;
		protected override bool ExpectedIsSupplierAvailable => true;

		#endregion

		#region TestGS1TargetFieldsToDefault

		protected override ZString[] ExpectedGS1TargetFieldsToDefault => new ZString[]
		{
			WarehouseTargetFields.Codes.PartAttrib1, WarehouseTargetFields.Codes.PartAttrib2,
			WarehouseTargetFields.Codes.PartAttrib3, WarehouseTargetFields.Codes.PackingDate,
			WarehouseTargetFields.Codes.ExpiryDate, WarehouseTargetFields.Codes.SerialNumber
		};

		#endregion

		#region TestRelatedEntityRequirements

		protected override RelatedEntityRequirements ExpectedRelatedEntityRequirements =>
			RelatedEntityRequirements.MustHaveBuyer;

		#endregion

		#region TestTargetFields

		protected override CodeDescriptionPairList ExpectedTargetFields => new WarehouseTargetFields();
		protected override CodeDescriptionPairList ExpectedTargetFieldsForEnum => new WarehouseTargetFields();

		#endregion

		#region TestGetRelatedEntityList

		public void TestGetRelatedEntityList()
		{
			// setup data
			var buyer1 = Helper.CreateOrg("Buyer1");
			var buyer2 = Helper.CreateOrg("Buyer2");
			var supplier1 = Helper.CreateOrg("Supplier1");
			var supplier2 = Helper.CreateOrg("Supplier2");
			var part1 = Helper.CreatePart("P1", buyer1);
			var part2 = Helper.CreatePart("P2", buyer1);
			var part3 = Helper.CreatePart("P3", buyer1);
			part2.RelatedOrganisations.AddSupplier(supplier1);
			part3.RelatedOrganisations.AddSupplier(supplier2);
			Factory.Save();
			IBarcodeParsingConsumer consumer = new WarehouseBarcodeParsingConsumer(Factory);
			AssertEquals("Buyer is mandatory for Warehouse Related Entity, so List should be null if Buyer is empty.",
				null, consumer.GetRelatedEntityList(null, supplier1));
			var list1 = consumer.GetRelatedEntityList(buyer1, null);
			AssertEquals("Buyer has been entered, so List should have the correct type.",
				typeof(OrgSupplierPartCollection), list1.GetType());
			AssertContainsExactElementsInAnyOrder(
				new[] { new FilterBusinessObjectDefault("Importer/Supplier", "Property1", buyer1.PK) },
				((OrgSupplierPartCollection)list1).FilterBusinessObjectDefaults);
			// list should contain products owned by Buyer1.
			((OrgSupplierPartCollection)list1).Load();
			AssertContainsExactElementsInAnyOrder(new[] { part1, part2, part3 }, list1);
			// list should contain products owned by Buyer1 & either supplied by Supplier1 or have no supplier.
			var list2 = consumer.GetRelatedEntityList(buyer1, supplier1);
			((OrgSupplierPartCollection)list2).Load();
			AssertContainsExactElementsInAnyOrder(new[] { part1, part2 }, list2);
			var list3 = consumer.GetRelatedEntityList(buyer2, null);
			((OrgSupplierPartCollection)list3).Load();
			AssertEquals("No product matches Buyer 2, should have no results.", 0, list3.Count);
		}

		#endregion

		#region TestGetValidFieldFormatsForTargetField

		public void TestGetValidFieldFormatsForTargetField()
		{
			IBarcodeParsingConsumer consumer = new WarehouseBarcodeParsingConsumer(Factory);
			// isGS1 = true
			const bool IsGS1 = true;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.ProductCode));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.PalletID));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.Location));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.PackageID));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.SerialNumber));
			AssertContainsExactElementsInAnyOrder(new[] { FormatType.D },
				consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.Quantity));
			AssertContainsExactElementsInAnyOrder(new[] { FormatType.D5 },
				consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.PackingDate));
			AssertContainsExactElementsInAnyOrder(new[] { FormatType.D5 },
				consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.ExpiryDate));
			AssertEquals(0,
				consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.PartAttrib1).Count());
			AssertEquals(0,
				consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.PartAttrib2).Count());
			AssertEquals(0,
				consumer.GetValidFieldFormatsForTargetField(IsGS1, WarehouseTargetFields.Codes.PartAttrib3).Count());
			// isGS1 = false
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.ProductCode));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.PalletID));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.Location));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.PackageID));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY,
					FormatType.ANS
				}, consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.SerialNumber));
			AssertContainsExactElementsInAnyOrder(new[] { FormatType.D, FormatType.N },
				consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.Quantity));
			AssertContainsExactElementsInAnyOrder(
				new[] { FormatType.D1, FormatType.D2, FormatType.D3, FormatType.D4, FormatType.D5 },
				consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.PackingDate));
			AssertContainsExactElementsInAnyOrder(
				new[] { FormatType.D1, FormatType.D2, FormatType.D3, FormatType.D4, FormatType.D5 },
				consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.ExpiryDate));
			AssertEquals(0,
				consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.PartAttrib1).Count());
			AssertEquals(0,
				consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.PartAttrib2).Count());
			AssertEquals(0,
				consumer.GetValidFieldFormatsForTargetField(!IsGS1, WarehouseTargetFields.Codes.PartAttrib3).Count());
		}

		#endregion

		#region TestValidateTargetFieldForValidationRules

		public void TestValidateTargetFieldForValidationRules()
		{
			var consumer = (IBarcodeValidationRulesConsumer)new WarehouseBarcodeParsingConsumer(Factory);

			foreach (CodeDescriptionPair targetField in new WarehouseTargetFields())
			{
				if (targetField.Code == WarehouseTargetFields.Codes.ExpiryDate || targetField.Code == WarehouseTargetFields.Codes.PackingDate)
				{
					AssertEquals(
						$"Validation Rules for {targetField.Description} are not supported. Validation of Date Formats can be configured under Maintain -> Products -> Related Organizations -> Attributes.",
						consumer.ValidateTargetFieldForValidationRules(targetField.Code));
				}
				else
				{
					Assert(
						"There should be no expected validation error.",
						string.IsNullOrWhiteSpace(consumer.ValidateTargetFieldForValidationRules(targetField.Code)));
				}
			}
		}

		#endregion
		#region Implementation

		protected override ZString ModuleCode
		{
			get
			{
				return BarcodeModuleTypes.Codes.Warehouse;
			}
		}

		#endregion
	}
}
