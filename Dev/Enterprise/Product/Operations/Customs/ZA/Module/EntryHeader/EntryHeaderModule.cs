using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class EntryHeaderModule : Customs.Module.EntryHeaderModule
	{
		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new EntryHeaderController();

		protected override IBusinessObjectCollection GetNewGridCollection() => new Business.ModuleEntryHeaderCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
