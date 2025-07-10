namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public interface IProcessDataHelper
	{
		bool TryToGet<T>(out T value);
		void Update<T>(T value);
		void Reset();
	}
}
