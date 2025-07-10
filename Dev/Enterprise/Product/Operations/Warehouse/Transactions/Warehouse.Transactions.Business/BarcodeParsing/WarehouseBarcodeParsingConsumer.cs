using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsingEngine;
using Enterprise.BarcodeParsingEngine.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WarehouseBarcodeParsingConsumer : BarcodeParsingConsumer<WarehouseTargetField>, IBarcodeValidationRulesConsumer
	{
		public WarehouseBarcodeParsingConsumer(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region BuyerCaption

		protected override string BuyerCaption
		{
			get { return Res.GetString("ad9d1fc5-478d-4272-b692-da31cc6f3553", "Client"); }
		}

		#endregion

		#region RelatedEntityCaption

		protected override string RelatedEntityCaption
		{
			get { return Res.GetString("30323c59-b09e-4763-82bc-1febaef74f57", "Product"); }
		}

		#endregion

		#region ModuleCode

		protected override ZString ModuleCode
		{
			get { return BarcodeModuleTypes.Codes.Warehouse; }
		}

		#endregion

		#region Buyers

		protected override OrgHeaderCollection Buyers
		{
			get { return new WarehouseClientCollection(Factory); }
		}

		#endregion

		#region Suppliers

		protected override OrgHeaderCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		#endregion

		#region GetRelatedEntityList

		protected override IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier)
		{
			IBusinessObjectCollection result = null;

			if (buyer != null)
			{
				var parts = new OrgSupplierPartCollection(Factory, supplier, buyer, false, PartFilterOptions.OwnerMandatory);
				parts.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", buyer.PK));
				result = parts;
			}

			return result;
		}

		#endregion

		#region GetValidFieldFormatsForTargetField

		protected override IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField)
		{
			FormatType[] result;

			if (ValidCommonFieldFormats.ContainsKey(targetField))
			{
				result = ValidCommonFieldFormats[targetField];
			}
			else
			{
				if (isGS1 && ValidGS1FieldFormats.ContainsKey(targetField))
				{
					result = ValidGS1FieldFormats[targetField];
				}
				else if (!isGS1 && ValidNonGS1FieldFormats.ContainsKey(targetField))
				{
					result = ValidNonGS1FieldFormats[targetField];
				}
				else
				{
					result = Array.Empty<FormatType>();
				}
			}

			return result;
		}

		static FormatType[] NonDateFormats => new[] { FormatType.D, FormatType.N, FormatType.A, FormatType.AS, FormatType.AN, FormatType.ANY, FormatType.ANS };

		readonly Dictionary<string, FormatType[]> ValidCommonFieldFormats = new Dictionary<string, FormatType[]>
		{
			{ nameof(WarehouseTargetField.PRC), NonDateFormats }, // Product code
			{ nameof(WarehouseTargetField.LCN), NonDateFormats }, // Location
			{ nameof(WarehouseTargetField.PID), NonDateFormats }, // Pallet ID
			{ nameof(WarehouseTargetField.PCK), NonDateFormats }, // Package ID
			{ nameof(WarehouseTargetField.SER), NonDateFormats }, // Serial Number
		};

		readonly Dictionary<string, FormatType[]> ValidGS1FieldFormats = new Dictionary<string, FormatType[]>
		{
			{ nameof(WarehouseTargetField.QTY), new[] { FormatType.D } }, // Quantity - Numeric
			{ nameof(WarehouseTargetField.PKD), new[] { FormatType.D5 } }, // Packing Date - D5
			{ nameof(WarehouseTargetField.EXD), new[] { FormatType.D5 } }, // Expiry Date - D5
		};

		static FormatType[] DateFormats => new[] { FormatType.D1, FormatType.D2, FormatType.D3, FormatType.D4, FormatType.D5 };

		readonly Dictionary<string, FormatType[]> ValidNonGS1FieldFormats = new Dictionary<string, FormatType[]>
		{
			{ nameof(WarehouseTargetField.QTY), new[] { FormatType.D, FormatType.N } }, // Quantity - Numeric
			{ nameof(WarehouseTargetField.PKD), DateFormats }, // Packing Date - dates
			{ nameof(WarehouseTargetField.EXD), DateFormats }, // Expiry Date - dates
		};

		#endregion

		#region RelatedEntityRequirements

		protected override RelatedEntityRequirements RelatedEntityRequirements
		{
			get { return RelatedEntityRequirements.MustHaveBuyer; }
		}

		#endregion

		#region TargetFields

		protected override ReadOnlyCodeDescriptionPairList TargetFields => new WarehouseTargetFields();

		#endregion

		#region GS1TargetFieldsToDefault

		protected override IEnumerable<ZString> GS1TargetFieldsToDefault
		{
			get
			{
				return new ZString[]
				{
					WarehouseTargetFields.Codes.PartAttrib1,
					WarehouseTargetFields.Codes.PartAttrib2,
					WarehouseTargetFields.Codes.PartAttrib3,
					WarehouseTargetFields.Codes.PackingDate,
					WarehouseTargetFields.Codes.ExpiryDate,
					WarehouseTargetFields.Codes.SerialNumber,
				};
			}
		}

		#endregion

		#region IsRelatedEntityAvailable

		protected override bool IsRelatedEntityAvailable
		{
			get { return true; }
		}

		#endregion

		#region IsBuyerAvailable

		protected override bool IsBuyerAvailable
		{
			get { return true; }
		}

		#endregion

		#region IsSupplierAvailable

		protected override bool IsSupplierAvailable
		{
			get { return true; }
		}

		#endregion

		#region IBarcodeValidationRulesConsumer

		string IBarcodeValidationRulesConsumer.ValidateTargetFieldForValidationRules(ZString targetField)
		{
			var failedField = string.Empty;
			if (targetField == WarehouseTargetFields.Codes.PackingDate)
			{
				failedField = WarehouseTargetFields.Descriptions.PackingDate;
			}
			else if (targetField == WarehouseTargetFields.Codes.ExpiryDate)
			{
				failedField = WarehouseTargetFields.Descriptions.ExpiryDate;
			}

			return string.IsNullOrEmpty(failedField)
				? failedField
				: Res.GetString("a925ee1a-abbd-484e-bf01-64edce102354",
						"Validation Rules for {0} are not supported. Validation of Date Formats can be configured under Maintain -> Products -> Related Organizations -> Attributes.",
						failedField);
		}

		#endregion
	}
}
