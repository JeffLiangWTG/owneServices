using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.GPS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	[DebuggerDisplay("Leg: {ToString()}")]
	internal class LegSegment
	{
		public LegSegment(CommonCartageLeg leg, LegSegmentType legType)
		{
			Type = legType;
			Leg = leg;
		}
		public readonly LegSegmentType Type;
		public readonly CommonCartageLeg Leg;

		public List<LegSegment> PreceedingAddresses = new List<LegSegment>();
		public List<LegSegment> GroupedAddresses = new List<LegSegment>();

		public ZDateTime TimeIn
		{
			set
			{
				TimeInInfo.Value = value;
				GroupedAddresses.ForEach(a => a.TimeIn = value);
			}
			get { return (ZDateTime)TimeInInfo.Value; }
		}

		public ZPropertyInfo TimeInInfo
		{
			get
			{
				switch (Type)
				{
					case LegSegmentType.PickUp:
						return Leg.JU_PickupTimeInInfo;
					case LegSegmentType.WaitPoint:
						return Leg.JU_WaitPointTimeInInfo;
					case LegSegmentType.Delivery:
						return Leg.JU_DeliverTimeInInfo;
					default:
						return null;
				}
			}
		}

		public ZDateTime TimeOut
		{
			set
			{
				TimeOutInfo.Value = value;
				GroupedAddresses.ForEach(a => a.TimeOut = value);
			}
			get { return (ZDateTime)TimeOutInfo.Value; }
		}

		public ZPropertyInfo TimeOutInfo
		{
			get
			{
				switch (Type)
				{
					case LegSegmentType.PickUp:
						return Leg.JU_PickupTimeOutInfo;
					case LegSegmentType.WaitPoint:
						return Leg.JU_WaitPointTimeOutInfo;
					case LegSegmentType.Delivery:
						return Leg.JU_DeliverTimeOutInfo;
					default:
						return null;
				}
			}
		}

		public ZBool HasNoTimeIn
		{
			get { return TimeIn.IsEmpty; }
		}

		public ZBool HasTimeInOnly
		{
			get { return !TimeIn.IsEmpty && TimeOut.IsEmpty; }
		}

		public ZBool HasTimeInAndOut
		{
			get { return !TimeIn.IsEmpty && !TimeOut.IsEmpty; }
		}

		public ZDecimal Latitude => ZGeography.NormalizeLatitudeDegree(OrgAddress?.Latitude ?? DocAddress?.E2_Latitude ?? 0);
		public ZDecimal Longitude => ZGeography.NormalizeLongitudeDegree(OrgAddress?.Longitude ?? DocAddress?.E2_Longitude ?? 0);

		//A workaround due to DBNull requiring an invalid response, versus a valid, empty value WI00100952
		public ZGeography GeofencePolygon
		{
			get
			{
				var address = OrgAddress?.OA_GeofencePolygon;
				return address == null || !address.Value.IsValid ? ZGeography.Empty : address.Value;
			}
		}

		public bool HasValidGeoLocation
		{
			get { return Latitude != 0 || Longitude != 0 || GeofencePolygon != null; }
		}

		public bool HasValidAddress => DocAddress != null && (OrgAddress != null || DocAddress.E2_AddressOverride);

		public bool HasValidOrgAddress => OrgAddress != null;

		public bool IsSameAddress(LegSegment segment)
		{
			return segment.DocAddress != null && DocAddress != null && DocAddress.IsTheSameAddressAs(segment.DocAddress);
		}

		OrgAddress OrgAddress => DocAddress != null && !DocAddress.E2_AddressOverride ? DocAddress.Address : null;

		JobDocAddress DocAddress
		{
			get { return docAddress ?? (docAddress = GetDocAddress()); }
		}
		JobDocAddress docAddress;

		JobDocAddress GetDocAddress()
		{
			switch (Type)
			{
				case LegSegmentType.PickUp:
					return Leg.PickupFromDocAddress;
				case LegSegmentType.WaitPoint:
					return Leg.WaitPointDocAddress;
				case LegSegmentType.Delivery:
					return Leg.DeliverToDocAddress;
				default:
					return null;
			}
		}

		public ZString AddressCode
		{
			get
			{
				if (addressCode.IsEmpty)
				{
					if (DocAddress != null && DocAddress.E2_AddressOverride)
					{
						var freeTextAddress = new ZStringBuilder();
						freeTextAddress.AppendIfNotEmpty(DocAddress.E2_Address1);
						freeTextAddress.AppendIfNotEmpty(DocAddress.E2_Address2);
						freeTextAddress.AppendIfNotEmpty(DocAddress.E2_City);
						addressCode = DocAddress.CompanyName + GPSConstants.FenceSeperator + freeTextAddress.ToStringWithDelimiterBetweenAppends(" ");
					}
					else if (OrgAddress != null)
					{
						addressCode = OrgAddress.Header.OH_Code + GPSConstants.FenceSeperator + OrgAddress.OA_Code;
					}
				}

				return addressCode;
			}
		}
		ZString addressCode;

		public ZString TypeDescription
		{
			get
			{
				switch (Type)
				{
					case LegSegmentType.PickUp:
						return Res.GetString("5B37824A-0282-421F-9677-011DB838139B", "Pickup");
					case LegSegmentType.WaitPoint:
						return Res.GetString("D84430CF-93DC-4E53-972B-38F7A3A76616", "Waiting Point");
					case LegSegmentType.Delivery:
						return Res.GetString("FEA9AC59-D14C-4839-979F-7964BFD970BD", "Delivery");
					default:
						return ZString.Empty;
				}
			}
		}

		public override string ToString()
		{
			string result = "";
			if (Leg.PickupFromDocAddress != null && Leg.PickupFromDocAddress.Address != null)
			{
				result += "PU: " + Leg.PickupFromDocAddress.Address.Header.OH_Code + " - " + Leg.PickupFromDocAddress.Address.OA_Code + " ";
			}
			if (Leg.WaitPointDocAddress != null && Leg.WaitPointDocAddress.Address != null)
			{
				result += "WP: " + Leg.WaitPointDocAddress.Address.Header.OH_Code + " - " + Leg.WaitPointDocAddress.Address.OA_Code + " ";
			}
			if (Leg.DeliverToDocAddress != null && Leg.DeliverToDocAddress.Address != null)
			{
				result += "DL: " + Leg.DeliverToDocAddress.Address.Header.OH_Code + " - " + Leg.DeliverToDocAddress.Address.OA_Code + " ";
			}
			if (!Leg.JU_PickupTimeIn.IsEmpty)
			{
				result += "PUTI: " + Leg.JU_PickupTimeIn.ToString() + " ";
			}
			if (!Leg.JU_PickupTimeOut.IsEmpty)
			{
				result += "PUTO: " + Leg.JU_PickupTimeOut.ToString() + " ";
			}
			if (!Leg.JU_DeliverTimeIn.IsEmpty)
			{
				result += "DLTI: " + Leg.JU_DeliverTimeIn.ToString() + " ";
			}
			if (!Leg.JU_DeliverTimeIn.IsEmpty)
			{
				result += "DLTO: " + Leg.JU_DeliverTimeOut.ToString() + " ";
			}
			result += (NoResString)"Type: " + Type.ToString();
			return result;
		}
	}

	internal enum LegSegmentType
	{
		Delivery,
		WaitPoint,
		PickUp,
		None
	}
}
