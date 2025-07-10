using System;
using Enterprise.BarcodeParsingEngine;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class BarcodeValidationRuleInfo : DataObjectInfo
	{
		public BarcodeValidationRuleInfo()
		{
			Prefix = "";
			TargetField = "";
		}

		public BarcodeValidationRuleInfo(IBarcodeValidationRule rule) : this()
		{
			Prefix = rule.Prefix;
			MinLength = rule.MinLength;
			MaxLength = rule.MaxLength;
			FormatEnumValue = (int)rule.Format;
			TargetField = rule.TargetField;
		}

		#region Properties

		public string Prefix { get; set; }

		/// <summary>
		/// This is the 'Format' Enum's integer value. Even though this Enum is shared in the Web Service
		/// and RF via Barcode Parsing Engine, re-generating the WebService creates a copy of the Enum.
		/// To work around this we send the integer value and cast it on the RF side back to the Enum.
		/// </summary>
		public int FormatEnumValue { get; set; }

		public short MinLength { get; set; }

		public short MaxLength { get; set; }

		public string TargetField { get; set; }

		#endregion
	}
}
