using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class GenericLandedCostingConfig
	{
		public GenericLandedCostingConfig(XElement element)
		{
			LoadFromXElement(element);
		}

		#region Properties

		public ZString CountryCode { get; set; }

		public IEnumerable<FormalEntryConfig> FormalEntryConfigs
		{
			get { return fFormalEntryConfigs; }
		}
		List<FormalEntryConfig> fFormalEntryConfigs;

		public IEnumerable<LCEntryCustomsDisbursementCodeMapping> LCEntryCustomsDisbursementCodeMappings
		{
			get { return fLCEntryCustomsDisbursementCodeMappings; }
		}
		List<LCEntryCustomsDisbursementCodeMapping> fLCEntryCustomsDisbursementCodeMappings;

		public IEnumerable<LandedLineCostItemSetting> LandedLineCostItemSettings
		{
			get { return fLandedLineCostItemColumnSettings; }
		}
		List<LandedLineCostItemSetting> fLandedLineCostItemColumnSettings;

		#endregion

		#region Schema

		public static class Schema
		{
			public const string ElementName = "GenericLandedCostingConfig";
			public const string CountryCode = "CountryCode";
			public const string FormalEntryConfigs = "FormalEntryConfigs";
			public const string LCEntryCustomsDisbursementCodeMappings = "LCEntryCustomsDisbursementCodeMappings";
			public const string LandedLineCostItemSettings = "LandedLineCostItemSettings";
		}

		#endregion

		#region Load From XML

		void LoadFromXElement(XElement element)
		{
			CountryCode = element.Attribute(Schema.CountryCode)?.Value;

			fFormalEntryConfigs = new List<FormalEntryConfig>();
			var configsElement = element.Element(Schema.FormalEntryConfigs);
			if (configsElement != null)
			{
				foreach (var configElement in configsElement.Elements(FormalEntryConfig.Schema.ElementName))
				{
					var config = new FormalEntryConfig(configElement);
					fFormalEntryConfigs.Add(config);
				}
			}

			fLCEntryCustomsDisbursementCodeMappings = new List<LCEntryCustomsDisbursementCodeMapping>();
			var mappingsElement = element.Element(Schema.LCEntryCustomsDisbursementCodeMappings);
			if (mappingsElement != null)
			{
				foreach (var mappingElement in mappingsElement.Elements(LCEntryCustomsDisbursementCodeMapping.Schema.ElementName))
				{
					var mapping = new LCEntryCustomsDisbursementCodeMapping(mappingElement);
					fLCEntryCustomsDisbursementCodeMappings.Add(mapping);
				}
			}

			fLandedLineCostItemColumnSettings = new List<LandedLineCostItemSetting>();
			var settingsElement = element.Element(Schema.LandedLineCostItemSettings);
			if (settingsElement != null)
			{
				foreach (var settingElement in settingsElement.Elements(LandedLineCostItemSetting.Schema.ElementName))
				{
					var setting = new LandedLineCostItemSetting(settingElement);
					fLandedLineCostItemColumnSettings.Add(setting);
				}
			}
		}

		#endregion
	}

	public class FormalEntryConfig
	{
		public FormalEntryConfig(XElement element)
		{
			LoadFromXElement(element);
		}

		#region Properties

		public ZString EntryType { get; set; }
		public bool IsAdditionalEntryLine { get; set; }

		#endregion

		#region Schema

		public static class Schema
		{
			public const string ElementName = "FormalEntryConfig";
			public const string EntryType = "EntryType";
			public const string IsAdditionalEntryLine = "IsAdditionalEntryLine";
		}

		#endregion

		#region Load From XML

		void LoadFromXElement(XElement element)
		{
			EntryType = element.Attribute(Schema.EntryType)?.Value;
			ZString strValue = element.Attribute(Schema.IsAdditionalEntryLine)?.Value;
			IsAdditionalEntryLine = strValue == "Y";
		}

		#endregion
	}

	public class LCEntryCustomsDisbursementCodeMapping
	{
		public LCEntryCustomsDisbursementCodeMapping(XElement mappingElement)
		{
			LoadFromXElement(mappingElement);
		}

		#region Schema

		public static class Schema
		{
			public const string ElementName = "LCEntryCustomsDisbursementCodeMapping";
			public const string Description = "Description";
			public const string EntryDisbursementCode = "EntryDisbursementCode";
			public const string LCDisbursementCode = "LCDisbursementCode";
			public const string CusEntryLineApplicablePropertyName = "CusEntryLineApplicablePropertyName";
			public const string JobComInvoiceLineAmountPropertyName = "JobComInvoiceLineAmountPropertyName";
			public const string HeaderLevelFee = "HeaderLevelFee";
		}

		#endregion

		#region Properties

		public ZString EntryDisbursementCode { get; set; }
		public ZString LCDisbursementCode { get; set; }
		public ZString Description { get; set; }
		public ZString CusEntryLineApplicablePropertyName { get; set; }
		public ZString JobComInvoiceLineAmountPropertyName { get; set; }
		public bool HeaderLevelFee { get; set; }

		#endregion

		#region Load From XML

		void LoadFromXElement(XElement element)
		{
			EntryDisbursementCode = element.Attribute(Schema.EntryDisbursementCode)?.Value;
			LCDisbursementCode = element.Attribute(Schema.LCDisbursementCode)?.Value;
			Description = element.Attribute(Schema.Description)?.Value;
			CusEntryLineApplicablePropertyName = element.Attribute(Schema.CusEntryLineApplicablePropertyName)?.Value;
			JobComInvoiceLineAmountPropertyName = element.Attribute(Schema.JobComInvoiceLineAmountPropertyName)?.Value;
			ZString strValue = element.Attribute(Schema.HeaderLevelFee)?.Value;
			HeaderLevelFee = strValue.IsEmpty ? ZBool.False : ZBool.ParseSafe(strValue, ZBool.False);
		}

		#endregion
	}

	public class LandedLineCostItemSetting : ICustomsChargeLCItemSetting
	{
		public LandedLineCostItemSetting()
		{
		}

		public LandedLineCostItemSetting(XElement settingsElement) : base()
		{
			LoadFromXElement(settingsElement);
		}

		#region Schema

		public static class Schema
		{
			public const string ElementName = "LandedLineCostItemSetting";
			public const string CostType = "CostType";
			public const string Description = "Description";
			public const string IsDuty = "IsDuty";
			public const string NumberOfDecimals = "NumberOfDecimals";
			public const string GridColumnWidth = "GridColumnWidth";
			public const string DocumentCustomLabelCode = "DocumentCustomLabelCode";
			public const string DocumentColumnWidth = "DocumentColumnWidth";
			public const string DocumentMacro = "DocumentMacro";
		}

		#endregion

		#region Properties

		public ZString CostType { get; set; }
		public ZString Description { get; set; }
		public ZBool IsDuty { get; set; }
		public ZInt NumberOfDecimals { get; set; }
		public ZInt GridColumnWidth { get; set; }
		public ZString DocumentCustomLabelCode { get; set; }
		public ZInt DocumentColumnWidth { get; set; }
		public ZString DocumentMacro { get; set; }

		#endregion

		#region Load From XML

		void LoadFromXElement(XElement element)
		{
			CostType = element.Attribute(Schema.CostType)?.Value;
			Description = element.Attribute(Schema.Description)?.Value;
			IsDuty = element.Attribute(Schema.IsDuty)?.Value == "Y";
			ZString strValue = element.Attribute(Schema.NumberOfDecimals)?.Value;
			NumberOfDecimals = strValue.IsEmpty ? ZInt.Zero : ZInt.ParseSafe(strValue, ZInt.Zero);
			strValue = element.Attribute(Schema.GridColumnWidth)?.Value;
			GridColumnWidth = strValue.IsEmpty ? ZInt.Zero : ZInt.ParseSafe(strValue, ZInt.Zero);
			strValue = element.Attribute(Schema.DocumentColumnWidth)?.Value;
			DocumentCustomLabelCode = element.Attribute(Schema.DocumentCustomLabelCode)?.Value;
			DocumentColumnWidth = strValue.IsEmpty ? ZInt.Zero : ZInt.ParseSafe(strValue, ZInt.Zero);
			DocumentMacro = element.Attribute(Schema.DocumentMacro)?.Value;
		}

		#endregion
	}
}
