using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.Module;

public partial class TemporaryStorageFilterControl : EU.TemporaryStorage.Module.TemporaryStorageFilterControl
{
	public TemporaryStorageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}
}
