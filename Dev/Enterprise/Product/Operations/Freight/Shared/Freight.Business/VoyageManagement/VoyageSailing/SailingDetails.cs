using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public struct SailingDetails : IEquatable<SailingDetails>
	{
		public SailingDetails(ZString transportMode, ZString load, ZString discharge, ZString vessel, ZString voyage, ZGuid shippingLine, ZDateTime etd, ZDateTime eta, ZDateTime std, ZDateTime sta)
		{
			TransportMode = transportMode;
			LoadPort = load;
			DischargePort = discharge;
			Vessel = vessel;
			Voyage = voyage;
			ShippingLine = shippingLine;
			EtdValue = etd;
			EtaValue = eta;
			StdValue = std;
			StaValue = sta;
		}

		public ZString TransportMode { get; }
		public ZString LoadPort { get; }
		public ZString DischargePort { get; }
		public ZString Vessel { get; }
		public ZString Voyage { get; }
		public ZGuid ShippingLine { get; }
		public ZDateTime EtdValue { get; }
		public ZDateTime EtaValue { get; }
		public ZDateTime StdValue { get; }
		public ZDateTime StaValue { get; }

		public static SailingDetails Empty
		{
			get
			{
				return new SailingDetails(
					ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					ZString.Empty,
					ZGuid.Empty,
					ZDateTime.Empty,
					ZDateTime.Empty,
					ZDateTime.Empty,
					ZDateTime.Empty);
			}
		}

		public bool IsEmpty => this == Empty;

		#region Overrides

		public override bool Equals(object obj)
		{
			if (obj is SailingDetails details)
			{
				return details == this;
			}

			return false;
		}

		public bool Equals(SailingDetails other)
		{
			if (other == null)
			{
				return false;
			}

			return TransportMode == other.TransportMode
				&& LoadPort == other.LoadPort
				&& DischargePort == other.DischargePort
				&& Vessel == other.Vessel
				&& Voyage == other.Voyage
				&& ShippingLine == other.ShippingLine
				&& EtdValue == other.EtdValue
				&& EtaValue == other.EtaValue
				&& StdValue == other.StdValue
				&& StaValue == other.StaValue;
		}

		public override int GetHashCode()
		{
			return TransportMode.GetHashCode()
				^ LoadPort.GetHashCode()
				^ DischargePort.GetHashCode()
				^ Vessel.GetHashCode()
				^ Voyage.GetHashCode()
				^ ShippingLine.GetHashCode()
				^ EtdValue.GetHashCode()
				^ EtaValue.GetHashCode()
				^ StdValue.GetHashCode()
				^ StaValue.GetHashCode();
		}

		public static bool operator ==(SailingDetails left, SailingDetails right)
		{
			return left.TransportMode == right.TransportMode
				&& left.LoadPort == right.LoadPort
				&& left.DischargePort == right.DischargePort
				&& left.Vessel == right.Vessel
				&& left.Voyage == right.Voyage
				&& left.EtdValue == right.EtdValue
				&& left.EtaValue == right.EtaValue
				&& left.StdValue == right.StdValue
				&& left.StaValue == right.StaValue;
		}

		public static bool operator !=(SailingDetails left, SailingDetails right)
		{
			return !(left == right);
		}

		#endregion
	}
}
