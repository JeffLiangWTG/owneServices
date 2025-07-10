using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class EntryLineDocWrapper : NonPersistentBusinessObject
	{
		public EntryLineDocWrapper(CusEntryLine line)
		{
			this.line = line;
		}
		readonly CusEntryLine line;
		JobDeclaration Declaration
		{
			get { return line.Declaration; }
		}

		public ZString IsCommercial
		{
			get { return "X"; }
		}

		public ZString Description
		{
			get { return line.Description; }
		}

		public ZString Tariff1
		{
			get { return line.CL_AdValoremTariff; }
		}

		public ZString Tariff2
		{
			get
			{
				var secondaryLine = line.ChildSecondaryEntryLines.FirstOrDefault();

				return secondaryLine != null ? secondaryLine.CL_AdValoremTariff : ZString.Empty;
			}
		}

		public ZDecimal EntryLineValue1
		{
			get { return line.RoundedCustomsValue; }
		}

		public ZString EntryLineValue2
		{
			get
			{
				var secondaryLine = line.ChildSecondaryEntryLines.FirstOrDefault();

				return secondaryLine != null ? secondaryLine.RoundedCustomsValue.ToString() : "";
			}
		}

		public ZString CountryOfOriginForLine
		{
			get { return ((ICusEntryLine)line).CountryOfOrigin; }
		}

		public ZString LineItemQuantity
		{
			get { return Declaration != null && Declaration.IsConsumptionFTZ ? ((ICusEntryLine)line).FTZLineItemQuantity.ToString() + " " + line.InvoiceLines[0].US_ManifestUQ : ""; }
		}

		public ZDateTime FTZFilingDate
		{
			get { return Declaration != null && Declaration.IsConsumptionFTZ ? ((ICusEntryLine)line).PrivilegedStatusFilingDate : ZDateTime.Empty; }
		}

		public ZString IsZoneStatusP
		{
			get { return Declaration != null && Declaration.IsConsumptionFTZ ? ((ICusEntryLine)line).ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign ? "X" : "" : ""; }
		}

		public ZString IsZoneStatusN
		{
			get { return Declaration != null && Declaration.IsConsumptionFTZ ? ((ICusEntryLine)line).ZoneStatus == ZoneStatusList.Codes.NonPrivilegedForeign ? "X" : "" : ""; }
		}

		ZString PartyType
		{
			get
			{
				if (!line.Header.Entities_3461.Any(x => x.EntityCode == EntityCodeList.Codes.ManufacturerSupplier))
				{
					return EntityCodeList.Codes.ManufacturerSupplier;
				}
				else if (!line.Header.Entities_3461.Any(x => x.EntityCode == EntityCodeList.Codes.Consignee))
				{
					return EntityCodeList.Codes.Consignee;
				}
				else if (!line.Header.Entities_3461.Any(x => x.EntityCode == EntityCodeList.Codes.BuyingParty))
				{
					return EntityCodeList.Codes.BuyingParty;
				}
				else if (!line.Header.Entities_3461.Any(x => x.EntityCode == EntityCodeList.Codes.SellingParty))
				{
					return EntityCodeList.Codes.SellingParty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString IsManufacturerParty
		{
			get { return TickPartyTypeCheckBox(EntityCodeList.Codes.ManufacturerSupplier); }
		}

		public PartyDocWrapper Party
		{
			get
			{
				var party = ((ISimplifiedEntryLine)line).Entities.FirstOrDefault(x => x.EntityCode == PartyType);
				return party != null ? new PartyDocWrapper(party, Declaration?.PrintSocialSecurityNumberOnDocument ?? false) : null;
			}
		}

		public ZString IsConsignee
		{
			get { return TickPartyTypeCheckBox(EntityCodeList.Codes.Consignee); }
		}

		public ZString IsBuyingParty
		{
			get { return TickPartyTypeCheckBox(EntityCodeList.Codes.BuyingParty); }
		}

		public ZString IsSellingParty
		{
			get { return TickPartyTypeCheckBox(EntityCodeList.Codes.SellingParty); }
		}

		ZString TickPartyTypeCheckBox(ZString partyType)
		{
			return PartyType == partyType ? "X" : "";
		}
	}
}
