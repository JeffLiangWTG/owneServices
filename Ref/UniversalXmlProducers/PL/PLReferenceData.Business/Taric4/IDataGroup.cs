namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

public interface IDataGroup<T>
	where T: IDataPoint
{
	T[] DataPoints { get; set; }
}
