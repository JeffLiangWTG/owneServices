using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class OriginWrapper : IOrigin
{
	public OriginWrapper(ZString country, ZString code, int sequenceNumeric)
	{
		this.country = country;
		this.code = code;
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly ZString country;
	readonly ZString code;

	public string CountryCode => country;
	public string RegionId => null;
	public string TypeCode => code;

	public int SequenceNumeric { get; }
}
