namespace Enterprise.Warehouse.Cartonisation.Business
{
	public interface IRandom
	{
		int Next(int min, int max);
		int Next(int max);
	}
}