namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public interface ILocalFileStorage
	{
		string FileLocation { get; }

		void ClearData();
		string Load();
		T Load<T>();
		void Save(string data);
		void Save<T>(T data);
	}
}