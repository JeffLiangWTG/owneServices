using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Business;

public class PrePostCarriageLocationWrapper : IPrePostCarriageLocation
{
	public PrePostCarriageLocationWrapper(ZString port)
	{
		UNLOCO = port;
	}

	public PrePostCarriageLocationWrapper(ISupportWebAddressValidation address, string transportMode = default)
	{
		Address = address;
		TransportMode = transportMode;
	}

	public string TransportMode { get; private set; }
	public ISupportWebAddressValidation Address { get; private set; }
	public ZString UNLOCO { get; private set; }

	public bool IsPort => !UNLOCO.IsEmpty && Address is null;

	public bool IsEmpty => UNLOCO.IsEmpty && Address is null;

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		var another = (PrePostCarriageLocationWrapper)obj;
		if (Address is null && another.Address is null)
		{
			return UNLOCO == another.UNLOCO;
		}
		else if (Address is null || another.Address is null)
		{
			return false;
		}
		else if (!Address.GeoLocation.IsEmpty && !another.Address.GeoLocation.IsEmpty)
		{
			return Address.GeoLocation == another.Address.GeoLocation;
		}
		else if (!Address.GeoLocation.IsEmpty || !another.Address.GeoLocation.IsEmpty)
		{
			return false;
		}
		else
		{
			return Address.Postcode == another.Address.Postcode
				&& Address.City == another.Address.City
				&& Address.CountryCodeISO2 == another.Address.CountryCodeISO2
				&& Address.ClosestPort == another.Address.ClosestPort;
		}
	}

	public override int GetHashCode()
	{
		if (Address is null)
		{
			return UNLOCO.GetHashCode();
		}
		else if (!Address.GeoLocation.IsEmpty)
		{
			return Address.GeoLocation.GetHashCode();
		}
		else
		{
			return Address.Postcode.GetHashCode()
				^ Address.City.GetHashCode()
				^ Address.CountryCodeISO2.GetHashCode()
				^ Address.ClosestPort.GetHashCode();
		}
	}
}
