using System;
using System.Linq;
using Enterprise.BarcodeParsing.Business;
using Enterprise.BarcodeParsingEngine;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class BarcodeParsingRuleInfo : DataObjectInfo
	{
		public BarcodeParsingRuleInfo()
		{
			Terminator = "";
		}

		public BarcodeParsingRuleInfo(IBarcodeRule rule)
			: this()
		{
			IsPartialRule = rule.IsPartialRule;
			RuleNumber = rule.RuleNumber;
			RuleName = rule.RuleName;

			// Sending the GS1 Terminator as a string in XML causes the Web Service
			// to crash. To work around this we will send an empty string across and
			// send the GS1 Terminator's character code (int) instead.
			bool isTerminatorGS1 = rule.Terminator == BarcodeRule.GS1Terminator;
			Terminator = isTerminatorGS1 ? "" : rule.Terminator;

			if (isTerminatorGS1)
			{
				TerminatorCharacterCode = BarcodeRule.GS1Terminator[0];
			}

			Components.AddRange(rule.Components.Select(c => new BarcodeParsingRuleComponentInfo(c)));
		}

		#region Components

		public BarcodeParsingRuleComponentInfoCollection Components
		{
			get { return components ?? (components = new BarcodeParsingRuleComponentInfoCollection()); }
			set { components = value; }
		}

		BarcodeParsingRuleComponentInfoCollection components;

		#endregion

		#region Properties

		public bool IsPartialRule { get; set; }

		public short RuleNumber { get; set; }

		public string RuleName { get; set; }

		public string Terminator { get; set; }

		public int TerminatorCharacterCode { get; set; }

		#endregion
	}
}
