using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFInvoiceImporter
	{
		public void Import(CusISFHeader header, JobComInvoiceHeader invoice)
		{
			var existingLineKeys = new HashSet<LineKey>();
			var existingContainers = new HashSet<ZString>();

			foreach (var isfLine in header.Lines)
			{
				var lineKey = new LineKey(
					isfLine.BL_TextProductCode,
					isfLine.BL_PartAttrib1,
					isfLine.BL_PartAttrib2,
					isfLine.BL_PartAttrib3,
					isfLine.BL_FormattedHarmonisedNum,
					isfLine.ManufacturerDocAddress?.E2_OA_Address ?? ZGuid.Empty,
					isfLine.BL_RN_NKGoodsOrigin
				);
				existingLineKeys.Add(lineKey);
			}

			foreach (var equipment in header.Equipments.Where(e => e.BE_EquipCode == USContainerCodeList.Codes.CN))
			{
				existingContainers.Add(equipment.BE_ContainerNum.ToUpperInvariant());
			}

			foreach (var invoiceLine in invoice.InvoiceLines.OfType<JobComInvoiceLine>())
			{
				var lineKey = new LineKey(
					invoiceLine.JI_PartNo,
					invoiceLine.JI_PartAttrib1,
					invoiceLine.JI_PartAttrib2,
					invoiceLine.JI_PartAttrib3,
					invoiceLine.JI_Tariff,
					invoiceLine.JI_OA_ManufacturerAddress,
					invoiceLine.US_UC_NKCountryOfOrigin
				);

				if (existingLineKeys.Add(lineKey))
				{
					var isfLine = header.Lines.AddNew();
					isfLine.BL_TextProductCode = invoiceLine.JI_PartNo;
					isfLine.BL_PartAttrib1 = invoiceLine.JI_PartAttrib1;
					isfLine.BL_PartAttrib2 = invoiceLine.JI_PartAttrib2;
					isfLine.BL_PartAttrib3 = invoiceLine.JI_PartAttrib3;
					CopyTariffToISFLine(isfLine, invoiceLine);
					var docAddress = header.DocAddresses.OfType<ISFDocAddress>().FirstOrDefault(a => a.DocAddressType == DocAddressType.Manufacturer && a.E2_OA_Address == invoiceLine.JI_OA_ManufacturerAddress);
					if (docAddress == null)
					{
						docAddress = header.DocAddresses.CreateWithAddressType(DocAddressType.Manufacturer);
						docAddress.E2_AddressOverride = false;
						docAddress.E2_OA_Address = invoiceLine.JI_OA_ManufacturerAddress;
					}
					isfLine.BL_ManufacturerDocAddressPK = docAddress.PK;
					isfLine.BL_RN_NKGoodsOrigin = invoiceLine.US_UC_NKCountryOfOrigin;
				}
			}

			foreach (var lineRefs in invoice.InvoiceHeaderRefs)
			{
				if (lineRefs.J2_ReferenceType == Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.CN && existingContainers.Add(lineRefs.J2_ReferenceNumber.ToUpperInvariant()))
				{
					var equipment = header.Equipments.AddNew();
					equipment.BE_EquipCode = USContainerCodeList.Codes.CN;
					equipment.BE_ContainerNum = lineRefs.J2_ReferenceNumber.SubstringSafe(0, CusISFEquip.Schema.BE_ContainerNumMaxLength);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Setter will reformat and truncate the number.")]
		void CopyTariffToISFLine(CusISFLine isfLine, JobComInvoiceLine invoiceLine)
		{
			isfLine.BL_FormattedHarmonisedNum = invoiceLine.JI_Tariff;
		}

		sealed class LineKey
		{
			ZString PartNum { get; }
			ZString Attribute1 { get; }
			ZString Attribute2 { get; }
			ZString Attribute3 { get; }

			ZString Tariff { get; }

			ZGuid ManufacturerAddressPk { get; }
			ZString OriginCountry { get; }

			public LineKey(ZString partNum, ZString attribute1, ZString attribute2, ZString attribute3, ZString tariff, ZGuid manufacturerAddressPK, ZString originCountry)
			{
				PartNum = partNum;
				Attribute1 = attribute1;
				Attribute2 = attribute2;
				Attribute3 = attribute3;
				Tariff = tariff.Replace(".", "");
				ManufacturerAddressPk = manufacturerAddressPK;
				OriginCountry = originCountry;
			}

			bool Equals(LineKey other)
			{
				return
					PartNum.Equals(other.PartNum) &&
					Attribute1.Equals(other.Attribute1) &&
					Attribute2.Equals(other.Attribute2) &&
					Attribute3.Equals(other.Attribute3) &&
					Tariff.Equals(other.Tariff) &&
					ManufacturerAddressPk.Equals(other.ManufacturerAddressPk) &&
					OriginCountry.Equals(other.OriginCountry);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}
				if (ReferenceEquals(this, obj))
				{
					return true;
				}
				if (obj.GetType() != this.GetType())
				{
					return false;
				}
				return Equals((LineKey)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = PartNum.GetHashCode();
					hashCode = (hashCode * 397) ^ Attribute1.GetHashCode();
					hashCode = (hashCode * 397) ^ Attribute2.GetHashCode();
					hashCode = (hashCode * 397) ^ Attribute3.GetHashCode();
					hashCode = (hashCode * 397) ^ Tariff.GetHashCode();
					hashCode = (hashCode * 397) ^ ManufacturerAddressPk.GetHashCode();
					hashCode = (hashCode * 397) ^ OriginCountry.GetHashCode();
					return hashCode;
				}
			}

			public static bool operator ==(LineKey left, LineKey right)
			{
				return Equals(left, right);
			}

			public static bool operator !=(LineKey left, LineKey right)
			{
				return !Equals(left, right);
			}
		}
	}
}
