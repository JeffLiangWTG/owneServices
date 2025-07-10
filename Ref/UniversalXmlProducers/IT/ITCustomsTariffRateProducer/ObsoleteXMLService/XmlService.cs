using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.XmlService
{
	public class XmlService : IXmlService
	{
		XmlDocument _universalXml;
		XmlElement _root;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")] // Please double check this supression
		public XmlService(ILogger logger)
		{
			_logger = Argument.NotNull(logger, nameof(logger));

			CreateUniversalXml();
		}

		public void CreateUniversalXml()
		{
			var setupXml = "<UniversalReferenceData>" + PublicationXmlContent + SchemaXmlContent + "</UniversalReferenceData>";

			_universalXml = new XmlDocument();
			_universalXml.LoadXml(setupXml);

			_root = _universalXml.DocumentElement;
		}

		public XmlDocument GetUniversalXml()
		{
			return _universalXml;
		}

		public void SaveXml(string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			var parentDirectory = fileInfo.DirectoryName;

			if (!Directory.Exists(parentDirectory))
			{
				Directory.CreateDirectory(parentDirectory);
			}

			_universalXml.Save(filePath);
		}

		public void SerializeThenAppend(RefCusTariff refCusTariff)
		{
			if (refCusTariff == null)
			{
				return;
			}

			XmlNode importedNode = null;

			var element = SerializeToElement(refCusTariff);
			if (element != null)
			{
				importedNode = _universalXml.ImportNode(element, true);
			}

			if (importedNode == null)
			{
				_logger.Log($"{refCusTariff.TariffCode} Unable to create XML Entry");
				return;
			}

			_root.AppendChild(importedNode);
		}

		static XmlElement SerializeToElement(RefCusTariff refCusTariff)
		{
			var doc = new XmlDocument();
			using (var writer = doc.CreateNavigator().AppendChild())
			{
				var xmlSerializerNamesplaces = new XmlSerializerNamespaces();
				xmlSerializerNamesplaces.Add(string.Empty, string.Empty);

				new XmlSerializer(typeof(RefCusTariff)).Serialize(writer, refCusTariff, xmlSerializerNamesplaces);
			}

			return doc.DocumentElement;
		}

		public void UpdatePublicationDate(DateTime publicationDate)
		{
			var publicationNode = _root.SelectSingleNode("//PublicationTime");

			publicationNode.InnerText = publicationDate.ToString("s");
		}

		const string PublicationXmlContent = "<DataSource>IT VAT and Excise</DataSource><PublicationTime></PublicationTime><UpdateType>Full</UpdateType>";

		const string SchemaXmlContent = @"<Schema>
        <EntityType Name=""RefCusTariff"" Data=""true"">
          <Key>
            <PropertyRef Name=""ZZ1_TariffCode"" />
            <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
            <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
            <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
          </Key>
          <Property Name=""RefCusRate"" Type=""RefCusRate"" />
          <Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
          <Property Name=""RefCusVATApplicability"" Type=""RefCusVATApplicability"" />
          <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
          <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""IMP"" />
          <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
          <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
        </EntityType>
        <EntityType Name=""RefCusRate"" Data=""true"">
          <Key>
            <PropertyRef Name=""RefCusApplicability"" />
            <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
            <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
            <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
            <PropertyRef Name=""ZZ2_ZZS_NKPreference"" />
            <PropertyRef Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" />
            <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
          </Key>
          <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
          <Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
          <Property Name=""ZZ2_EndDate"" Type=""datetime"" ConstantValue=""2079-06-06T23:59:00"" />
          <Property Name=""ZZ2_RateFormula"" Type=""varchar"" MaxLength=""500"" />
          <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
          <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
          <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" MaxLength=""5"" />
          <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""IT"" />
          <Property Name=""ZZ2_ZZS_NKPreference"" Type=""varchar"" MaxLength=""10"" />
          <Property Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
          <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""IT"" />
        </EntityType>
        <EntityType Name=""RefCusApplicability"" Data=""true"">
          <Key>
            <PropertyRef Name=""ZZT_AdditionalCode"" />
            <PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
            <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
          </Key>
          <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
          <Property Name=""ZZT_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
          <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" ConstantValue=""2079-06-06T23:59:00"" />
          <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
          <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
          <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
        </EntityType>
        <EntityType Name=""RefCusRateUOM"" Data=""true"">
          <Key>
            <PropertyRef Name=""ZXG_UOM""/>
          </Key>
          <Property Name=""ZXG_UOM"" Type=""varchar"" MaxLength=""10"" />
        </EntityType>
        <EntityType Name=""RefCusVATApplicability"" Data=""true"">
          <Key>
            <PropertyRef Name=""ZX5_AdditionalCode"" />
            <PropertyRef Name=""ZX5_ZZF_NKTaxOrFeeCode"" />
            <PropertyRef Name=""ZX5_ZZZ_NKDataGrouping""/>
          </Key>
          <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
          <Property Name=""ZX5_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
          <Property Name=""ZX5_EndDate"" Type=""smalldatetime"" ConstantValue=""2079-06-06T23:59:00"" />
          <Property Name=""ZX5_StartDate"" Type=""smalldatetime"" />
          <Property Name=""ZX5_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
          <Property Name=""ZX5_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""IT"" />
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
          <Key>
            <PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
            <PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
         </Key>
         <Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
         <Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
        </EntityType>
        <EntityType Name=""RefCusCondition"" Data=""true"">
          <Key>
            <PropertyRef Name=""RefCusApplicability"" />
            <PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
            <PropertyRef Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" />
            <PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
          </Key>
          <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
          <Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
          <Property Name=""ZX1_Comment"" Type=""nvarchar"" />
          <Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" ConstantValue=""False"" />
          <Property Name=""ZX1_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
          <Property Name=""ZX1_IsImport"" Type=""bit"" ConstantValue=""True"" />
          <Property Name=""ZX1_Source"" Type=""nvarchar"" DefaultValue=""EU Taric"" />
          <Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
          <Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""5"" />
          <Property Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""IT"" />
          <Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""IT"" />
        </EntityType>
        <EntityType Name=""RefCusConditionValue"" Data=""true"">
          <Key>
            <PropertyRef Name=""ZX3_Value"" />
            <PropertyRef Name=""ZX3_ZX4_NKValueType"" />
            <PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
          </Key>
          <Property Name=""ZX3_Value"" Type=""nvarchar"" MaxLength=""500"" />
          <Property Name=""ZX3_ZX4_NKValueType"" Type=""varchar"" MaxLength=""5"" />
          <Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
        </EntityType>
      </Schema>";

		readonly ILogger _logger;
	}
}
