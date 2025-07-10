using System;
using Enterprise.BarcodeParsingEngine;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class BarcodeParsingRuleComponentInfo : DataObjectInfo
	{
		public BarcodeParsingRuleComponentInfo()
		{
			ApplicationIdentifier = "";
			TargetField = "";
		}

		public BarcodeParsingRuleComponentInfo(IBarcodeRuleComponent ruleComponent)
			: this()
		{
			ApplicationIdentifier = ruleComponent.ApplicationIdentifier;

			// some characters when sent as a string in XML will cause the WebService to crash, so we send the byte representation instead.
			Delimiter = (byte)ruleComponent.Delimiter;
			FormatEnumValue = (int)ruleComponent.Format;
			MaxLength = ruleComponent.MaxLength;
			MinLength = ruleComponent.MinLength;
			Sequence = ruleComponent.Sequence;
			TargetField = ruleComponent.TargetField;
			IsDelimiterMultiComponent = ruleComponent.IsDelimiterMultiComponent;
		}

		#region Properties

		public string ApplicationIdentifier { get; set; }

		public byte Delimiter { get; set; }

		public bool IsDelimiterMultiComponent { get; set; }

		/// <summary>
		/// This is the 'Format' Enum's integer value. Even though this Enum is shared in the Web Service
		/// and RF via Barcode Parsing Engine, re-generating the WebService creates a copy of the Enum.
		/// To work around this we send the integer value and cast it on the RF side back to the Enum.
		/// </summary>
		public int FormatEnumValue { get; set; }

		public short MaxLength { get; set; }

		public short MinLength { get; set; }

		public short Sequence { get; set; }

		public string TargetField { get; set; }

		#endregion
	}
}
