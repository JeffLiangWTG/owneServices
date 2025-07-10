using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DataTransfer
{
	public class XmlInvoiceDataImporter : XmlDeclarationDataImporter
	{
		public XmlInvoiceDataImporter(XmlDocument xmlDoc, BaseJobDeclaration toJobDec) : base(xmlDoc, toJobDec)
		{
		}

		#region Overrides

		public override void Import()
		{
			base.Import();
			CreateInvoice();
			if (CancelImport)
			{
				FireLogEvent(Res.GetString("c1d37a39-a21a-42d7-b883-d27fa78b5bc6", "Import Canceled"));
			}
			else
			{
				FireLogEvent(Res.GetString("0ec53246-df6e-49c9-bc03-1ca38eb51248", "--------------------\r\nImport Successful"));
			}
		}

		public override int TotalRecordCount
		{
			get
			{
				XmlNodeList invoiceNodes = DocReader.XmlDoc.SelectNodes(@"//" + CommercialInvoiceXsd.XPath.Invoices.InvoiceHeader);
				XmlNodeList invoiceLineNodes = DocReader.XmlDoc.SelectNodes(@"//" + CommercialInvoiceXsd.XPath.InvoiceLine.NodeName);
				XmlNodeList invoiceChargesNodes = DocReader.XmlDoc.SelectNodes(@"//" + CommercialInvoiceXsd.XPath.InvoiceCharge.NodeName);

				return invoiceNodes.Count + invoiceChargesNodes.Count + invoiceLineNodes.Count;
			}
		}

		public override int FailedRecordCount
		{
			get { return 0; }
		}

		public override int ProcessedRecordCount
		{
			get { return fProcessedRecordCount; }
		}
		int fProcessedRecordCount;

		#endregion

		#region Create Business Objects

		protected CommercialInvoiceXmlReader commercialInvoiceReader;
		protected void CreateInvoice()
		{
			foreach (XmlNode invoiceNode in InvoiceNodes)
			{
				commercialInvoiceReader = new CommercialInvoiceXmlReader(invoiceNode, DocReader.XmlDoc);
				BaseJobComInvoiceHeader invHead = toJobDec.Invoices.AddNew();
				SetPropertyInfoValue(invHead.JZ_InvoiceNumberInfo, commercialInvoiceReader.InvoiceNumber);
				SetPropertyInfoValue(invHead.JZ_InvoiceAmountInfo, commercialInvoiceReader.InvoiceAmount);
				SetOrganisation(commercialInvoiceReader, commercialInvoiceReader.ConsignorNode, invHead.JZ_OH_SupplierInfo);

				string currencyCode = commercialInvoiceReader.InvoiceAmountCurrency;
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);
				if (currency != null)
				{
					invHead.JZ_RX_NKInvoice_Currency = currency.RX_Code;
				}
				invHead.JZ_InvoiceDate = commercialInvoiceReader.InvoiceDate;
				invHead.JZ_ValuationDateOverride = commercialInvoiceReader.ValuationDate;
				SetPropertyInfoValue(invHead.JZ_GroupInvoiceInfo, commercialInvoiceReader.IsGroupInvoice);
				//<xs:element name="RelatedGroupInvoiceNumber" type="xs:string" minOccurs="0"/>

				SetPropertyInfoValue(invHead.JZ_IncoTermInfo, commercialInvoiceReader.IncoTerm);

				//<xs:element name="OwnerReference" type="xs:normalizedString" minOccurs="0"/>
				SetPropertyInfoValue(invHead.JZ_WeightInfo, commercialInvoiceReader.Weight);
				SetPropertyInfoValue(invHead.JZ_WeightUQInfo, commercialInvoiceReader.WeightUnit);
				SetPropertyInfoValue(invHead.JZ_VolumeInfo, commercialInvoiceReader.Volume);
				SetPropertyInfoValue(invHead.JZ_VolumeUQInfo, commercialInvoiceReader.VolumeUnit);
				CreateAdditionalCustomsDetails(invoiceNode, invHead);
				CreateInvoiceCharges(invHead);
				CreateInvoiceLines(invHead);
				fProcessedRecordCount++;
				FireLogEvent("");
				if (CancelImport)
				{
					break;
				}
			}
		}

		protected void SetOrganisation(XmlDocReader reader, XmlNode orgNode, ZPropertyInfo propertyInfo)
		{
			if (orgNode != null)
			{
				OrgHeader org = reader.GetOrganisation(factory, null, orgNode);
				if (org != null)
				{
					propertyInfo.Value = org.PK;
				}
			}
		}

		protected void CreateInvoiceCharges(BaseJobComInvoiceHeader invHead)
		{
			CommercialInvoiceChargesXmlReader[] commercialInvoiceChargesReaders = commercialInvoiceReader.CommercialInvoiceChargesXmlReaders;

			foreach (CommercialInvoiceChargesXmlReader commercialInvoiceChargesReader in commercialInvoiceChargesReaders)
			{
				string chargeCode = commercialInvoiceChargesReader.ChargeType;
				ZDecimal amount = 0M;
				ZDecimal.TryParse(commercialInvoiceChargesReader.ChargeValue, out amount);
				string currency = commercialInvoiceChargesReader.ChargeValueCurrency;
				BaseJobComInvHeaderCharge invCharge = invHead.Charges.AddNew(chargeCode, amount, currency);
				SetPropertyInfoValue(invCharge.J7_IsGSTApplicableInfo, commercialInvoiceChargesReader.GSTApplies);
				SetPropertyInfoValue(invCharge.J7_IsDutiableInfo, commercialInvoiceChargesReader.DutyApplies);
				SetPropertyInfoValue(invCharge.J7_IsIncludedInITOTInfo, commercialInvoiceChargesReader.IsIncludedInTotal);
				fProcessedRecordCount++;
				FireLogEvent("");
				if (CancelImport)
				{
					break;
				}
			}
		}

		protected virtual void CreateAdditionalCustomsDetails(XmlNode invoiceNode, BaseJobComInvoiceHeader invHead)
		{
			//this is for country specific implementations
		}

		protected void CreateInvoiceLines(BaseJobComInvoiceHeader invHead)
		{
			CommercialInvoiceLinesXmlReader[] commercialInvoiceLinesReaders = commercialInvoiceReader.CommercialInvoiceLinesXmlReaders;

			foreach (CommercialInvoiceLinesXmlReader commercialInvoiceLinesReader in commercialInvoiceLinesReaders)
			{
				BaseJobComInvoiceLine invLine = toJobDec.InvoiceLines.AddNew();
				invLine.JI_JZ = invHead.PK;

				SetPropertyInfoValue(invLine.JI_InvoiceQuantityInfo, commercialInvoiceLinesReader.InvoiceQty);
				SetPropertyInfoValue(invLine.JI_InvoiceUQInfo, commercialInvoiceLinesReader.InvoiceQtyUnit);
				SetPropertyInfoValue(invLine.JI_LinePriceInfo, commercialInvoiceLinesReader.LinePrice);
				SetPropertyInfoValue(invLine.JI_PartNoInfo, commercialInvoiceLinesReader.ProductNumber);
				if (!string.IsNullOrEmpty(commercialInvoiceLinesReader.ProductDescription))
				{
					SetPropertyInfoValue(invLine.JI_DescriptionInfo, commercialInvoiceLinesReader.ProductDescription);
				}
				SetPropertyInfoValue(invLine.JI_CustomsUnitQtyInfo, commercialInvoiceLinesReader.CustomsInvoiceQtyUnit);
				SetPropertyInfoValue(invLine.JI_CustomsQuantityInfo, commercialInvoiceLinesReader.CustomsInvoiceQty);
				SetPropertyInfoValue(invLine.JI_OrderNumberInfo, commercialInvoiceLinesReader.OrderNumber);
				if (!string.IsNullOrEmpty(commercialInvoiceLinesReader.OriginOfGoods))
				{
					SetPropertyInfoValue(invLine.JI_CountryOfOriginInfo, commercialInvoiceLinesReader.OriginOfGoods);
				}
				SetPropertyInfoValue(invLine.JI_ConcessionOrderInfo, commercialInvoiceLinesReader.Concession);

				if (invLine.JI_PartNo.IsEmpty)
				{
					string lookupCode = commercialInvoiceLinesReader.TariffLookup;
					BaseCusClassification classification = BaseCusClassification.LoadFromLookupCode(factory, lookupCode, toJobDec.IsImport, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					if (classification != null)
					{
						invLine.JI_CC = classification.PK;
					}

					if (invLine.JI_CC.IsEmpty)
					{
						SetPropertyInfoValue(invLine.JI_TariffInfo, commercialInvoiceLinesReader.TariffCode);
					}
				}
				SetPropertyInfoValue(invLine.JI_VolumeInfo, commercialInvoiceLinesReader.Volume);
				SetPropertyInfoValue(invLine.JI_VolumeUQInfo, commercialInvoiceLinesReader.VolumeUnit);
				SetPropertyInfoValue(invLine.JI_WeightInfo, commercialInvoiceLinesReader.Weight);
				SetPropertyInfoValue(invLine.JI_WeightUQInfo, commercialInvoiceLinesReader.WeightUnit);
				SetPropertyInfoValue(invLine.JI_CustomAttrib1Info, commercialInvoiceLinesReader.CustomText1);
				SetPropertyInfoValue(invLine.JI_CustomAttrib2Info, commercialInvoiceLinesReader.CustomText2);
				SetPropertyInfoValue(invLine.JI_CustomAttrib3Info, commercialInvoiceLinesReader.CustomText3);

				fProcessedRecordCount++;
				FireLogEvent("");
				if (CancelImport)
				{
					break;
				}
			}
		}

		#endregion

		#region Implementation

		protected XmlNodeList InvoiceNodes
		{
			get
			{
				if (fInvoiceNodes == null)
				{
					fInvoiceNodes = DocReader.XmlDoc.SelectNodes(@"//" + CommercialInvoiceXsd.XPath.Invoices.InvoiceHeader);
				}

				return fInvoiceNodes;
			}
		}
		protected XmlNodeList fInvoiceNodes;

		#endregion
	}
}
