using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IDataModelSupporter
	{
		void PopulateDataModelIfNeeded();

		ZString DataModel { get; set; }

		bool IsInDatabase { get; }

		bool IsDeleted { get; }
	}
}
