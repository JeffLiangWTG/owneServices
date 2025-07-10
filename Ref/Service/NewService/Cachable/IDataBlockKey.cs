namespace CargoWise.RefDbRepo.NewService
{
	public interface IDataBlockKey<T>
	{
		string GetKey();
		IDataBlockKey<T> CaculateNextKey(string checkpoint);
	}
}
