using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public interface IDA63ValueRecalculationParent
	{
		ZBool DA63NeedsRecalculation { get; }
		void RecalculateDA63Values();
	}
}
