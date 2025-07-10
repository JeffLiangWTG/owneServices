using CargoWise.Types;

namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	public class AdditionalReference
	{
		public AdditionalReference(ZString type, ZString value)
		{
			this.Type = type;
			this.Value = value;
		}

		public readonly ZString Type;
		public readonly ZString Value;
	}
}
