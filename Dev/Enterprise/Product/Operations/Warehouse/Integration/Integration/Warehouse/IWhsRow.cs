using CargoWise.Types;
namespace Enterprise.Warehouse.Integration
{
	public interface IWhsRow
	{
		ZGuid PK { get; }

		ZString WR_Name { get; set; }
		ZShort WR_Columns { get; set; }
		ZShort WR_Levels { get; set; }
		ZShort WR_Trays { get; set; }
		IWhsLocationCollection Locations { get; }
		object this[string propertyName] { get; set; }

		ZGuid WR_WW_Whs { get; set; }
	}
}
