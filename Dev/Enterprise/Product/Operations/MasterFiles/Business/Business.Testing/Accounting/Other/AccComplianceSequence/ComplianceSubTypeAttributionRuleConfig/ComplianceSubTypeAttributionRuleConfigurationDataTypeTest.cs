using System.IO;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleConfigurationRegistryDataType))]
	sealed class ComplianceSubTypeAttributionRuleConfigurationDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ComplianceSubTypeAttributionRuleConfigurationRegistryDataType>
	{
		#region Implementation

		protected override ComplianceSubTypeAttributionRuleConfigurationRegistryDataType GetNewDataType()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ComplianceSubTypeAttributionRuleConfigurationRegistryItemEditor"; }
		}

		public override void TestGetSetValidValues()
		{
			ComplianceSubTypeAttributionRuleConfigurationTest.CreateTaxSystemConfigurationForBrazil(new BusinessObjectFactory());
			base.TestGetSetValidValues();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var complianceSubTypeAttributionRuleConfiguration1 = collection.AddNew();
			complianceSubTypeAttributionRuleConfiguration1.Country = "PE";
			complianceSubTypeAttributionRuleConfiguration1.SubType = "TBO";
			complianceSubTypeAttributionRuleConfiguration1.LedgerType = "AR";
			complianceSubTypeAttributionRuleConfiguration1.InvoiceType = "INV";
			complianceSubTypeAttributionRuleConfiguration1.TaxInvoiceRule = "TID";
			complianceSubTypeAttributionRuleConfiguration1.DisbursementRule = "NDB";
			complianceSubTypeAttributionRuleConfiguration1.OriginalRule = "OTO";
			complianceSubTypeAttributionRuleConfiguration1.OrganisationLocation = "";
			complianceSubTypeAttributionRuleConfiguration1.TaxRegistrationType = "DNI";

			var complianceSubTypeAttributionRuleConfiguration2 = collection.AddNew();
			complianceSubTypeAttributionRuleConfiguration2.Country = "PE";
			complianceSubTypeAttributionRuleConfiguration2.SubType = "TBC";
			complianceSubTypeAttributionRuleConfiguration2.LedgerType = "AR";
			complianceSubTypeAttributionRuleConfiguration2.InvoiceType = "CRD";
			complianceSubTypeAttributionRuleConfiguration2.TaxInvoiceRule = "TID";
			complianceSubTypeAttributionRuleConfiguration2.DisbursementRule = "ALL";
			complianceSubTypeAttributionRuleConfiguration2.OriginalRule = "ALL";
			complianceSubTypeAttributionRuleConfiguration2.OrganisationLocation = "";
			complianceSubTypeAttributionRuleConfiguration2.ParentTransactionSubType = "TBO";
			complianceSubTypeAttributionRuleConfiguration2.ThresholdApplies = true;
			complianceSubTypeAttributionRuleConfiguration2.SubTypeThresholdNotMet = "TCD";

			var complianceSubTypeAttributionRuleConfigurationXML = @"<ArrayOfComplianceSubTypeAttributionRuleConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><ComplianceSubTypeAttributionRuleConfiguration><Country>PE</Country><SubType>TBO</SubType><DocumentTitle>BOLETA DE VENTA</DocumentTitle><LedgerType>AR</LedgerType><InvoiceType>INV</InvoiceType><TaxInvoiceRule>TID</TaxInvoiceRule><DisbursementRule>NDB</DisbursementRule><OriginalRule>OTO</OriginalRule><OrganisationLocation/>
			<TaxRegistrationType>DNI</TaxRegistrationType><SelfBillingRule/><TaxRegistrationLocationRule/><VATGroupRule/></ComplianceSubTypeAttributionRuleConfiguration>
			<ComplianceSubTypeAttributionRuleConfiguration><Country>PE</Country><SubType>TBC</SubType><DocumentTitle>NOTA DE CRÉDITO DE BOLETA DE VENTA</DocumentTitle><LedgerType>AR</LedgerType><InvoiceType>CRD</InvoiceType><TaxInvoiceRule>TID</TaxInvoiceRule><DisbursementRule>ALL</DisbursementRule><OriginalRule>ALL</OriginalRule><OrganisationLocation/>
			<TaxRegistrationType/><SelfBillingRule/><TaxRegistrationLocationRule/><VATGroupRule/><NewElementsXMLNode><ParentTransactionSubType>TBO</ParentTransactionSubType><ThresholdApplies>1</ThresholdApplies><SubTypeThresholdNotMet>TCD</SubTypeThresholdNotMet></NewElementsXMLNode></ComplianceSubTypeAttributionRuleConfiguration></ArrayOfComplianceSubTypeAttributionRuleConfiguration>";

			var collectionBR = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var cns = collectionBR.AddNew();
			cns.Country = "BR";
			cns.SubType = "CNS";
			cns.LedgerType = "AR";
			cns.InvoiceType = "CRD";
			cns.TaxInvoiceRule = "TXX";
			cns.OriginalRule = "ARO";
			cns.DisbursementRule = "ALL";
			cns.ParentTransactionSubType = "NFS";
			cns.RequiredTaxSystem = "ISS";

			var xnc = collectionBR.AddNew();
			xnc.Country = "BR";
			xnc.SubType = "XNC";
			xnc.LedgerType = "AR";
			xnc.InvoiceType = "CRD";
			xnc.TaxInvoiceRule = "TXX";
			xnc.OriginalRule = "ARO";
			xnc.DisbursementRule = "ALL";
			xnc.ParentTransactionSubType = "XND";
			xnc.ExcludedTaxSystem = "ISS";

			var complianceSubTypeAttributionRuleConfigurationBRXML = @"<ArrayOfComplianceSubTypeAttributionRuleConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><ComplianceSubTypeAttributionRuleConfiguration><Country>BR</Country><SubType>CNS</SubType><DocumentTitle>CANCELAMENTO DE NOTA FISCAL DE SERVIÇOS ELETRÔNICA</DocumentTitle><LedgerType>AR</LedgerType><InvoiceType>CRD</InvoiceType><TaxInvoiceRule>TXX</TaxInvoiceRule><DisbursementRule>ALL</DisbursementRule><OriginalRule>ARO</OriginalRule><OrganisationLocation/>
			<TaxRegistrationType/><SelfBillingRule/><TaxRegistrationLocationRule/><VATGroupRule/><NewElementsXMLNode><ParentTransactionSubType>NFS</ParentTransactionSubType><RequiredTaxSystem>ISS</RequiredTaxSystem></NewElementsXMLNode></ComplianceSubTypeAttributionRuleConfiguration>
			<ComplianceSubTypeAttributionRuleConfiguration><Country>BR</Country><SubType>XNC</SubType><DocumentTitle>NOTA DE CREDITO</DocumentTitle><LedgerType>AR</LedgerType><InvoiceType>CRD</InvoiceType><TaxInvoiceRule>TXX</TaxInvoiceRule><DisbursementRule>ALL</DisbursementRule><OriginalRule>ARO</OriginalRule><OrganisationLocation/>
			<TaxRegistrationType/><SelfBillingRule/><TaxRegistrationLocationRule/><VATGroupRule/><NewElementsXMLNode><ParentTransactionSubType>XND</ParentTransactionSubType><ExcludedTaxSystem>ISS</ExcludedTaxSystem></NewElementsXMLNode></ComplianceSubTypeAttributionRuleConfiguration></ArrayOfComplianceSubTypeAttributionRuleConfiguration>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection , complianceSubTypeAttributionRuleConfigurationXML),
				new ValidSampleAndBinaryValueInDB(collectionBR , complianceSubTypeAttributionRuleConfigurationBRXML),
			};
		}

		public void TestReadOldXMLSchema()
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(ComplianceSubTypeAttributionRuleConfiguration));

			var oldXml = @"<ComplianceSubTypeAttributionRuleConfiguration><Country>PE</Country><SubType>TBO</SubType><DocumentTitle>BOLETA DE VENTA</DocumentTitle><LedgerType>AR</LedgerType><InvoiceType>INV</InvoiceType><TaxInvoiceRule>TID</TaxInvoiceRule><DisbursementRule>NDB</DisbursementRule><OriginalRule>OTO</OriginalRule><OrganisationLocation/>
			<TaxRegistrationType>DNI</TaxRegistrationType><SelfBillingRule/><TaxRegistrationLocationRule/><VATGroupRule/></ComplianceSubTypeAttributionRuleConfiguration>";

			using (MemoryStream readerStream = new MemoryStream(Encoding.Unicode.GetBytes(oldXml)))
			using (XmlTextReader reader1 = new XmlTextReader(readerStream))
			{
				var complianceSubTypeAttributionRuleConfiguration1 = (ComplianceSubTypeAttributionRuleConfiguration)serializer.Deserialize(reader1);
				AssertEquals("PE", complianceSubTypeAttributionRuleConfiguration1.Country);
				AssertEquals("TBO", complianceSubTypeAttributionRuleConfiguration1.SubType);
				AssertEquals("BOLETA DE VENTA", complianceSubTypeAttributionRuleConfiguration1.DocumentTitle);
				AssertEquals("AR", complianceSubTypeAttributionRuleConfiguration1.LedgerType);
				AssertEquals("INV", complianceSubTypeAttributionRuleConfiguration1.InvoiceType);
				AssertEquals("TID", complianceSubTypeAttributionRuleConfiguration1.TaxInvoiceRule);
				AssertEquals("NDB", complianceSubTypeAttributionRuleConfiguration1.DisbursementRule);
				AssertEquals("OTO", complianceSubTypeAttributionRuleConfiguration1.OriginalRule);
				AssertEquals("DNI", complianceSubTypeAttributionRuleConfiguration1.TaxRegistrationType);
				AssertEquals("", complianceSubTypeAttributionRuleConfiguration1.ParentTransactionSubType);
			}
		}

		#endregion
	}
}
