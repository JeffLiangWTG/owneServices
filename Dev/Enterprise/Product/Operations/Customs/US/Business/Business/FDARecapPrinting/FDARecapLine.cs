using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.FDARecapPrinting
{
	public class FDARecapLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FDARecapLine(FDA fda, bool shouldPrintInvoiceHeading)
			: base(fda.Factory)
		{
			this.fda = fda;
			this.shouldPrintInvoiceHeading = shouldPrintInvoiceHeading;
		}
		readonly FDA fda;
		readonly bool shouldPrintInvoiceHeading;

		public ZBool ShouldPrintInvoiceHeading
		{
			get { return shouldPrintInvoiceHeading; }
		}

		public ZString InvoiceNumber
		{
			get { return fda.InvoiceNumber; }
		}

		public ZString MergedLineNumber
		{
			get
			{
				var invoiceLine = fda.InvoiceLine;
				return invoiceLine != null ? invoiceLine.JI_Calc_MergedLineNumber.PadLeft(4, '0') + "/" : "";
			}
		}

		public ZString FDALineNo
		{
			get { return fda.US_FDALineNo.ToString().PadLeft(4, '0'); }
		}

		public ZString CommercialDesc
		{
			get { return fda.US_FDACommercialDesc; }
		}

		public ZString CountryOfOriginForLine
		{
			get
			{
				var result = ZString.Empty;
				if (PrintCountryOfOriginForLine)
				{
					var invoiceLine = fda.InvoiceLine;
					result = invoiceLine != null ? invoiceLine.US_UC_NKCountryOfOrigin : ZString.Empty;
				}
				return result;
			}
		}

		public ZString TradeBrandName
		{
			get { return fda.US_TradeBrandName; }
		}

		public ZString ProductCode
		{
			get { return fda.US_FDAProductCode; }
		}

		public ZString ProductionCountry
		{
			get { return fda.US_UC_NKFDAProduction; }
		}

		public ZString FDAValue
		{
			get { return fda.FDAValue; }
		}

		public ZDateTime ConfirmationDate
		{
			get { return fda.US_FDAConfirmDate; }
		}

		public ZString ConfirmationNumber
		{
			get { return fda.US_PNC; }
		}

		public ZString ContainerDimentionType
		{
			get { return fda.US_FDAContainerDimType; }
		}

		public ZString ContainerDimentionUQ
		{
			get { return fda.US_DimUQ; }
		}

		public ZDecimal ContainerDimention1
		{
			get { return fda.US_ContainerDim1; }
		}

		public ZDecimal ContainerDimention2
		{
			get { return fda.US_ContainerDim2; }
		}

		public ZDecimal ContainerDimention3
		{
			get { return fda.US_ContainerDim3; }
		}

		public ZString UltimateConsigneeAddress1
		{
			get
			{
				var invoiceLine = fda.InvoiceLine;
				return invoiceLine != null && invoiceLine.ConsigneeOrgAddress != null ?
					GetAddressLine1(invoiceLine.ConsigneeOrgAddress.GetCustomsAddressDetailsFallingBackToMainAddress())
					: ZString.Empty;
			}
		}

		public ZString UltimateConsigneeAddress2
		{
			get
			{
				var invoiceLine = fda.InvoiceLine;
				return invoiceLine != null && invoiceLine.ConsigneeOrgAddress != null ?
					GetAddressLine2(invoiceLine.ConsigneeOrgAddress.GetCustomsAddressDetailsFallingBackToMainAddress())
					: ZString.Empty;
			}
		}

		public ZString ShipperAddress1
		{
			get { return GetAddressLine1(fda.ShipperAddress); }
		}

		public ZString ShipperAddress2
		{
			get { return GetAddressLine2(fda.ShipperAddress); }
		}

		public ZString ManufacturerAddress1
		{
			get { return GetAddressLine1(fda.ManufacturerAddress); }
		}

		public ZString ManufacturerAddress2
		{
			get { return GetAddressLine2(fda.ManufacturerAddress); }
		}

		ZString GetAddressLine1(OrgAddress address)
		{
			return address != null ? address.EffectiveCompanyNameTruncated + " " + address.OA_Address1 : "";
		}

		ZString GetAddressLine2(OrgAddress address)
		{
			return address != null ? (address.OA_Address2 + " " + address.OA_PostCode + " " + address.OA_City + " " + address.CountryName).Trim() : "";
		}

		#region Quantities

		public ZString Quantity1
		{
			get { return fda.US_FDAQty1 != ZDecimal.Zero ? fda.US_FDAQty1.ToString() : ""; }
		}

		public ZString UQ1
		{
			get { return fda.US_FDAQty1 != ZDecimal.Zero ? fda.US_FDAMeasure1 : ZString.Empty; }
		}

		public ZString Quantity2
		{
			get { return fda.US_FDAQty2 != ZDecimal.Zero ? fda.US_FDAQty2.ToString() : ""; }
		}

		public ZString UQ2
		{
			get { return fda.US_FDAQty2 != ZDecimal.Zero ? fda.US_FDAMeasure2 : ZString.Empty; }
		}

		public ZString Quantity3
		{
			get { return fda.US_FDAQty3 != ZDecimal.Zero ? fda.US_FDAQty3.ToString() : ""; }
		}

		public ZString UQ3
		{
			get { return fda.US_FDAQty3 != ZDecimal.Zero ? fda.US_FDAMeasure3 : ZString.Empty; }
		}

		public ZString Quantity4
		{
			get { return fda.US_FDAQty4 != ZDecimal.Zero ? fda.US_FDAQty4.ToString() : ""; }
		}

		public ZString UQ4
		{
			get { return fda.US_FDAQty4 != ZDecimal.Zero ? fda.US_FDAMeasure4 : ZString.Empty; }
		}

		public ZString Quantity5
		{
			get { return fda.US_FDAQty5 != ZDecimal.Zero ? fda.US_FDAQty5.ToString() : ""; }
		}

		public ZString UQ5
		{
			get { return fda.US_FDAQty5 != ZDecimal.Zero ? fda.US_FDAMeasure5 : ZString.Empty; }
		}

		public ZString Quantity6
		{
			get { return fda.US_FDAQty6 != ZDecimal.Zero ? fda.US_FDAQty6.ToString() : ""; }
		}

		public ZString UQ6
		{
			get { return fda.US_FDAQty6 != ZDecimal.Zero ? fda.US_FDAMeasure6 : ZString.Empty; }
		}

		#endregion

		internal bool PrintCountryOfOriginForLine
		{
			get;
			set;
		}
	}
}
