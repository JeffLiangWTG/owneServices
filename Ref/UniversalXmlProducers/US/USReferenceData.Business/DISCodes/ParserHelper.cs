using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Utilities;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Business.DISCodes
{
	public static class ParserHelper
	{
		public static RefCusCodeListAttribute[] GetAttributes(DISCode disCode)
		{
			var list = new List<RefCusCodeListAttribute>();

			if (disCode.DocumentLabelCode != null)
			{
				var documentLabelCode = disCode.DocumentLabelCode;
				if (documentLabelCode.Contains("_"))
				{
					documentLabelCode = documentLabelCode.Replace(" ", "");
				}
				AddAttribute(list, AttributeNames.USDISDocCode, documentLabelCode.Trim());
			}

			if (disCode.Metadata != null)
			{
				var metaData = disCode.Metadata.ToUpper(CultureInfo.InvariantCulture).Replace(" ", "");
				var isAddCER = AddUSDISRequiredDataAttribute(list, metaData);
				AddOptionalDataAttribute(list, metaData, isAddCER);

				if (metaData.Contains(DISCodeValues.PDFOnly))
				{
					AddAttribute(list, AttributeNames.USDISSupportedFiletypes, AttributeValues.PDF);
				}
			}

			if (disCode.DocumentDescription != null)
			{
				var description = disCode.DocumentDescription.ToUpper(CultureInfo.InvariantCulture).Replace(" ", "");
				AddUSDISFormGroupAttribute(list, description);
				AddUSDISPackageCategoryAttribute(list, description);
			}
			else
			{
				AddAttribute(list, AttributeNames.USDISFormGroup, AttributeValues.NOGROUP);
				AddAttribute(list, AttributeNames.USDISPackageCategory, AttributeValues.GEN);
			}

			return list.ToArray();
		}

		static void AddUSDISPackageCategoryAttribute(List<RefCusCodeListAttribute> list, string description)
		{
			if (description.Contains(AttributeValues.CBMA))
			{
				AddAttribute(list, AttributeNames.USDISPackageCategory, AttributeValues.CBMA);
			}
			else if (description.Contains(AttributeValues.USMCA))
			{
				AddAttribute(list, AttributeNames.USDISPackageCategory, AttributeValues.USMCA);
			}
			else if (description.Contains(AttributeValues.NAFTA))
			{
				AddAttribute(list, AttributeNames.USDISPackageCategory, AttributeValues.NAFTA);
			}
			else
			{
				AddAttribute(list, AttributeNames.USDISPackageCategory, AttributeValues.GEN);
			}
		}

		static void AddUSDISFormGroupAttribute(List<RefCusCodeListAttribute> list, string description)
		{
			if (description.Contains(DISCodeValues.ForeignTradeZone))
			{
				AddAttribute(list, AttributeNames.USDISFormGroup, AttributeValues.DRW);
				AddAttribute(list, AttributeNames.USDISFormGroup, AttributeValues.FTZ);
			}
			else if (description.Contains(DISCodeValues.Drawback))
			{
				AddAttribute(list, AttributeNames.USDISFormGroup, AttributeValues.DRW);
			}
			else
			{
				AddAttribute(list, AttributeNames.USDISFormGroup, AttributeValues.NOGROUP);
			}
		}

		static bool AddUSDISRequiredDataAttribute(List<RefCusCodeListAttribute> list, string metaData)
		{
			bool isCertifcateNumber = metaData.Contains(DISCodeValues.CertifcateNumber);
			bool isIssueDdate = metaData.Contains(DISCodeValues.IssueDdate);
			bool isExpirationDate = metaData.Contains(DISCodeValues.ExpirationDate);
			bool isGrossTonnage = metaData.Contains(DISCodeValues.GrossTonnage);
			bool isNetTonnage = metaData.Contains(DISCodeValues.NetTonnage);

			if (isCertifcateNumber)
			{
				AddAttribute(list, AttributeNames.USDISRequiredData, AttributeValues.CertificateNumber);
			}

			if (isIssueDdate)
			{
				AddAttribute(list, AttributeNames.USDISRequiredData, AttributeValues.IssueDate);
			}

			if (isExpirationDate)
			{
				AddAttribute(list, AttributeNames.USDISRequiredData, AttributeValues.ExpiryDate);
			}

			if (isGrossTonnage)
			{
				AddAttribute(list, AttributeNames.USDISRequiredData, AttributeValues.GrossTonnage);
			}

			if (isNetTonnage)
			{
				AddAttribute(list, AttributeNames.USDISRequiredData, AttributeValues.NetTonnage);
			}
			return isCertifcateNumber || isIssueDdate || isExpirationDate || isGrossTonnage || isNetTonnage;
		}

		static void AddOptionalDataAttribute(List<RefCusCodeListAttribute> list, string metaData, bool isAddCER)
		{
			if (metaData.Contains(DISCodeValues.CommercialInvoice) || metaData.Contains(DISCodeValues.InvoiceNumber))
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.INV);
			}

			if (metaData.Contains(DISCodeValues.PurchaseOrder))
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.PCK);
			}

			if (isAddCER)
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.CER);
			}

			if (metaData.Contains(DISCodeValues.BondType) || metaData.Contains(DISCodeValues.BondAmount))
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.BND);
			}

			if (metaData.Contains(DISCodeValues.Commodity)
				|| metaData.Contains(DISCodeValues.VehicleIdentificationNumber)
				|| metaData.Contains(DISCodeValues.VehicleManufacturerNumber)
				|| metaData.Contains(DISCodeValues.CasNumber)
				|| metaData.Contains(DISCodeValues.ManufacturerNumber))
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.COM);
			}

			if (metaData.Contains(DISCodeValues.EpaRegistrationNo) || metaData.Contains(DISCodeValues.EpaProducerEstablishmentNo))
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.TOX);
			}

			if (metaData.Contains(DISCodeValues.PermitNumber))
			{
				AddAttribute(list, AttributeNames.OptionalData, AttributeValues.PER);
			}
		}

		static void AddAttribute(List<RefCusCodeListAttribute> list, string name, string value)
		{
			var attribute = new RefCusCodeListAttribute()
			{
				ZZE_ZXE_NKName = name,
				ZZE_Value = value
			};
			list.Add(attribute);
		}
	}
}
