using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class DriverCodeFindBox : ZCodeFindBox
	{
		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			var driversGroup = Factory.Load<GlbGroup>(new ZGuid(transportRegistry.TransportDriversGroup.Value));
			if (driversGroup == null)
			{
				Globals.Message.ShowError(Res.GetString("ba64855b-f594-4cce-9ba2-05d132fb7073", "A driver group needs to be set in 'Registry->Port Transport->Port Transport Drivers' to be able to select a driver."));
			}
			else if (DriverGroupIsEmpty())
			{
				Globals.Message.ShowError(Res.GetString("621cbde5-0c2d-4697-8a5a-010535a60a8d", "The driver group defined in 'Registry->Port Transport->Port Transport Drivers' has no staff set. This group needs to have staff added to be able to select a driver."));
			}
			else
			{
				base.SelectFromPopupForm(autoSelect);
			}
		}

		bool DriverGroupIsEmpty()
		{
			var query = StaffDriverCollection.GetDriversQuery(Factory, Env.CurrentBranch.Code);
			return Factory.Load<GlbStaff>(query).Length <= 0;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;
	}
}
