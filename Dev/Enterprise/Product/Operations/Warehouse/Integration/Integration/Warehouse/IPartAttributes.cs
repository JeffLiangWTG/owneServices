using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IPartAttributes
	{
		ZDate ExpiryDate { get; }
		ZDate PackingDate { get; }
		ZString PartAttrib1 { get; }
		ZString PartAttrib2 { get; }
		ZString PartAttrib3 { get; }
		ZString SerialNumber { get; }
	}
}
