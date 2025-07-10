using CargoWise.EntityFramework;
using Enterprise.Integration.Freight;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IContainerParentDataObjectWriter
	{
		void PopulateContainer(BusinessObject sourceBO, ICommonContainer containerOverride, ITopLevelDataObject dataObject);

		ITopLevelDataObject GetContainerParentDataObject(BusinessObject sourceBO);
	}
}
