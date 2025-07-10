using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TaxRateXmlParser
	{
		const string TaxRatesResourceFileName = "Enterprise.MasterFiles.Business.Accounting.Other.AccTaxRate.TaxRates.xml";

		static Stream GetResourceStream()
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(TaxRatesResourceFileName);
		}

#if DEBUG
		public static Stream GetResourceStream_ForTest() => GetResourceStream();

#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IReadOnlyDictionary<ZString, IEnumerable<ITaxRateConfiguration>> BuildTaxRatesDictionaryBasedOnCountry()
			=> TaxRatesByCountryCode.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:The static field may not be thread safe", Justification = "Readonly static Lazy. Interfaces are read only. Implementation uses immutable collections.")]
		readonly static Lazy<IReadOnlyDictionary<ZString, IEnumerable<ITaxRateConfiguration>>> TaxRatesByCountryCode = new Lazy<IReadOnlyDictionary<ZString, IEnumerable<ITaxRateConfiguration>>>(CreateTaxRatesDictionaryFromXml);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded XML node name")]
		static IReadOnlyDictionary<ZString, IEnumerable<ITaxRateConfiguration>> CreateTaxRatesDictionaryFromXml()
		{
			var taxRatesForcountries = new Dictionary<ZString, List<ITaxRateConfiguration>>();

			using (var fileStream = GetResourceStream())
			{
				var xml = new XmlDocument();
				xml.Load(fileStream);
				var rootNode = xml.DocumentElement;
				foreach (XmlNode node in rootNode)
				{
					var nodeCountryCode = node.SelectSingleNode("CountryCode");
					var nodeTaxID = node.SelectSingleNode("TaxID");
					var nodeType = node.SelectSingleNode((NoResString)"Type");
					var nodeDescription = node.SelectSingleNode((NoResString)"Description");
					var nodeExtraType = node.SelectSingleNode("ExtraType");
					var nodeIsDefaultGST = node.SelectSingleNode("IsDefaultGST");
					var nodeIsDefaultFreeGST = node.SelectSingleNode("IsDefaultFreeGST");
					var nodeIsDefaultGSTReverse = node.SelectSingleNode("IsDefaultGSTReverse");
					var nodeIsDefaultFreeGSTReverse = node.SelectSingleNode("IsDefaultFreeGSTReverse");
					var nodeIsDefaultNotReport = node.SelectSingleNode("IsDefaultNotReport");
					var nodePostingGroup = node.SelectSingleNode("PostingGroup");
					var nodeReferenceRateType = node.SelectSingleNode("ReferenceRateType");
					var nodeReferenceExtraRateType = node.SelectSingleNode("ReferenceExtraRateType");
					var nodeTaxSystem = node.SelectSingleNode("TaxSystem");
					var nodeRateSource = node.SelectSingleNode("RateSource");

					if (nodeCountryCode == null || nodeTaxID == null)
					{
						continue;
					}

					var countryCode = nodeCountryCode.InnerText;
					var extraRateType = nodeExtraType == null ? ZString.Empty : new ZString(nodeExtraType.InnerText);

					var postingGroup = ZShort.ParseSafe(nodePostingGroup.InnerText, AccTaxRate.DefaultPostingGroupID);

					var isDefaultGST = IsNodeTextSetToYes(nodeIsDefaultGST);
					var isDefaultFreeGST = IsNodeTextSetToYes(nodeIsDefaultFreeGST);
					var isDefaultGSTReverse = IsNodeTextSetToYes(nodeIsDefaultGSTReverse);
					var isDefaultFreeGSTReverse = IsNodeTextSetToYes(nodeIsDefaultFreeGSTReverse);
					var isDefaultNotReport = IsNodeTextSetToYes(nodeIsDefaultNotReport);

					var taxSystem = nodeTaxSystem == null ? null : nodeTaxSystem.InnerText;
					var rateSource = nodeRateSource == null ? "TID" : nodeRateSource.InnerText;

					var taxConfiguration = new TaxRateConfiguration(
						countryCode,
						nodeTaxID.InnerText,
						nodeType.InnerText,
						nodeDescription.InnerText,
						extraRateType,
						isDefaultGST,
						isDefaultFreeGST,
						isDefaultGSTReverse,
						isDefaultFreeGSTReverse,
						isDefaultNotReport,
						postingGroup,
						nodeReferenceRateType.InnerText,
						nodeReferenceExtraRateType?.InnerText,
						taxSystem,
						rateSource);

					if (taxRatesForcountries.TryGetValue(countryCode, out var result))
					{
						result.Add(taxConfiguration);
					}
					else
					{
						taxRatesForcountries.Add(countryCode, new List<ITaxRateConfiguration> { taxConfiguration });
					}
				}
			}
			return taxRatesForcountries.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.ToImmutableArray().AsEnumerable());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded data in TaxRates XML")]
		static bool IsNodeTextSetToYes(XmlNode node) => node != null && string.Equals(node.InnerText, "Yes", StringComparison.OrdinalIgnoreCase);
	}

	public class TaxRateConfiguration : NonPersistentBusinessObject, ITaxRateConfiguration
	{
		public ZString CountryCode { get; }
		public ZString TaxID { get; }
		public ZString Type { get; }
		public ZString Description { get; }
		public ZString ExtraType { get; }
		public ZBool IsDefaultGST { get; }
		public ZBool IsDefaultFreeGST { get; }
		public ZBool IsDefaultGSTReverse { get; }
		public ZBool IsDefaultFreeGSTReverse { get; }
		public ZBool IsDefaultNotReport { get; }
		public ZShort PostingGroup { get; }
		public ZString ReferenceRateType { get; }
		public ZString ReferenceExtraRateType { get; }
		public ZString TaxSystem { get; }
		public ZString RateSource { get; }

		public TaxRateConfiguration(ZString countryCode, ZString taxID, ZString type, ZString description, ZString extraType,
			ZBool isDefaultGST, ZBool isDefaultFreeGST, ZBool isDefaultGSTReverse,
			ZBool isDefaultFreeGSTReverse, ZBool isDefaultNotReport, ZShort postingGroup, ZString referenceRateType, ZString referenceExtraRateType,
			ZString taxSystem, ZString rateSource)
		{
			this.CountryCode = countryCode;
			this.TaxID = taxID;
			this.Type = type;
			this.Description = description;
			this.ExtraType = extraType;
			this.IsDefaultFreeGST = isDefaultFreeGST;
			this.IsDefaultFreeGSTReverse = isDefaultFreeGSTReverse;
			this.IsDefaultGST = isDefaultGST;
			this.IsDefaultGSTReverse = isDefaultGSTReverse;
			this.IsDefaultNotReport = isDefaultNotReport;
			this.PostingGroup = postingGroup;
			this.ReferenceRateType = referenceRateType;
			this.ReferenceExtraRateType = referenceExtraRateType;
			this.TaxSystem = taxSystem;
			this.RateSource = rateSource;
		}
	}
}
