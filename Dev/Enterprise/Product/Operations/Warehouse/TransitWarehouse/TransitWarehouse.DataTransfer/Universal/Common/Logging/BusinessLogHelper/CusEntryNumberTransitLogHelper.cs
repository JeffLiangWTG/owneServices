using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class CusEntryNumberTransitLogHelper : TransitLogTableHelper<AdditionalReference, TransitLogColumnIDs.CusEntryNumberColumn>
	{
		protected override ZString GetValue(AdditionalReference additionalReference, TransitLogColumnIDs.CusEntryNumberColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.CusEntryNumberColumn.Type:
					return GetEntryType(additionalReference);
				case TransitLogColumnIDs.CusEntryNumberColumn.Number:
					return GetEntryNum(additionalReference);
				case TransitLogColumnIDs.CusEntryNumberColumn.CountryCode:
					return GetCountryCode(additionalReference);
				default:
					return ZString.Empty;
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.CusEntryNumberColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.CusEntryNumberColumn.Type:
					return Res.GetString("009727cc-da26-4761-9d39-f06dc750e895", "CODE");
				case TransitLogColumnIDs.CusEntryNumberColumn.Number:
					return Res.GetString("480d20b0-7ded-4a37-b5f8-7b57794be40c", "REFERENCE");
				case TransitLogColumnIDs.CusEntryNumberColumn.CountryCode:
					return Res.GetString("b29c0581-bd42-44b1-b91c-922e4bc0ef87", "COUNTRY");
			}

			return ZString.Empty;
		}

		public static ZString GetEntryType(AdditionalReference additionalReference)
		{
			if (additionalReference != null)
			{
				return additionalReference.Type.Code.GetValueOrDefault();
			}

			return ZString.Empty;
		}

		public static ZString GetEntryNum(AdditionalReference additionalReference)
		{
			if (additionalReference != null)
			{
				return additionalReference.ReferenceNumber.GetValueOrDefault();
			}

			return ZString.Empty;
		}

		public static ZString GetCountryCode(AdditionalReference additionalReference)
			=> additionalReference?.CountryOfIssue?.Code ?? ZString.Empty;
	}
}
