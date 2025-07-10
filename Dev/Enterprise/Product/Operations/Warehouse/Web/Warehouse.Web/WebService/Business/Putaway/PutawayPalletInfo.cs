using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PutawayPalletInfo : DataObjectInfo
	{
		public PutawayPalletInfo()
		{
			PalletID = "";
			Location = "";
			Location_UserFriendly = "";
			LocationFormattedCheckDigit = "";
			ClientCode = "";
			LocationPutawaySequence = "";
			AllocatedPalletID = "";
			ClientCode = "";
			LocationPK = Guid.Empty;

			ProductCode = "";
			PartAttrib1Name = "";
			PartAttrib1 = "";
			PartAttrib2Name = "";
			PartAttrib2 = "";
			PartAttrib3Name = "";
			PartAttrib3 = "";
			SerialNumber = "";
			ExpiryDate = "";
			PackingDate = "";
		}

		public PutawayPalletInfo(string palletID, string location, string locationUserFriendly, string locationFormattedCheckDigit, string clientCode, Guid docketPK)
			: this(palletID, location, locationUserFriendly, locationFormattedCheckDigit, Guid.Empty, clientCode, docketPK, string.Empty, string.Empty)
		{
		}

		public PutawayPalletInfo(
			string palletID,
			string location,
			string locationUserFriendly,
			string locationFormattedCheckDigit,
			Guid locationPK,
			string clientCode,
			Guid docketPK,
			string locationPutawaySequence,
			string allocatedPalletID)
			: this()
		{
			PalletID = palletID;
			Location = location;
			Location_UserFriendly = locationUserFriendly;
			LocationFormattedCheckDigit = locationFormattedCheckDigit;
			LocationPK = locationPK;
			ClientCode = clientCode;
			LocationPutawaySequence = locationPutawaySequence;
			AllocatedPalletID = allocatedPalletID;
			DocketPK = docketPK;
		}

		public string PalletID
		{
			get;
			set;
		}

		public string Location
		{
			get;
			set;
		}

		public string Location_UserFriendly
		{
			get;
			set;
		}

		public string LocationFormattedCheckDigit
		{
			get;
			set;
		}

		public Guid LocationPK
		{
			get;
			set;
		}

		public string LocationPutawaySequence
		{
			get;
			set;
		}

		public string AllocatedPalletID
		{
			get;
			set;
		}

		public string ClientCode
		{
			get;
			set;
		}

		public Guid DocketPK
		{
			get;
			set;
		}

		public string ProductCode
		{
			get;
			set;
		}

		public string PartAttrib1Name
		{
			get;
			set;
		}

		public string PartAttrib1
		{
			get;
			set;
		}

		public string PartAttrib2Name
		{
			get;
			set;
		}

		public string PartAttrib2
		{
			get;
			set;
		}

		public string PartAttrib3Name
		{
			get;
			set;
		}
		public string PartAttrib3
		{
			get;
			set;
		}

		public string SerialNumber
		{
			get;
			set;
		}

		public string ExpiryDate
		{
			get;
			set;
		}

		public string PackingDate
		{
			get;
			set;
		}

		static string ManyString => Res.GetString("4814b820-4fbe-4d05-a482-056a096ebfec", "<Many>");

		internal void ConsolidateProductInfos(
			IEnumerable<string> productCodes,
			string partAttrib1Name,
			IEnumerable<string> partAttrib1s,
			string partAttrib2Name,
			IEnumerable<string> partAttrib2s,
			string partAttrib3Name,
			IEnumerable<string> partAttrib3s,
			IEnumerable<string> serialNumbers,
			IEnumerable<DateTime> expiryDates,
			IEnumerable<DateTime> packingDates)
		{
			ProductCode = ConsolidateAttribute(productCodes);
			PartAttrib1Name = partAttrib1Name;
			PartAttrib1 = ConsolidateAttribute(partAttrib1s);
			PartAttrib2Name = partAttrib2Name;
			PartAttrib2 = ConsolidateAttribute(partAttrib2s);
			PartAttrib3Name = partAttrib3Name;
			PartAttrib3 = ConsolidateAttribute(partAttrib3s);
			SerialNumber = ConsolidateAttribute(serialNumbers);
			ExpiryDate = ConsolidateAttribute(expiryDates.Select(d => d == DateTime.MinValue ? "" : d.ToShortDateString()));
			PackingDate = ConsolidateAttribute(packingDates.Select(d => d == DateTime.MinValue ? "" : d.ToShortDateString()));
		}

		string ConsolidateAttribute(IEnumerable<string> values)
		{
			var arr = values.Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToArray();
			if (arr.Length == 0)
			{
				throw new ArgumentException("There must be at least one element", nameof(values));
			}
			else if (arr.Length == 1)
			{
				return Argument.NotNull(arr[0], nameof(values));
			}
			else
			{
				return ManyString;
			}
		}
	}
}
