using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.GUI
{
	public class CartonGroupParentGrid : ZModuleButtonGrid
	{
		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new CartonGroupParentAttacher(destinationCollection, findBoxList, moduleID, (WhsCartonGroup)DataSource);
		}

		protected override void Detach(BusinessObject selected)
		{
			((OrgMiscServ)selected).OM_WCG_CartonGroup = ZGuid.Empty;
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			return ((OrgMiscServ)selected).Header;
		}
	}

	class CartonGroupParentAttacher : ZRecordAttacher
	{
		public CartonGroupParentAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, WhsCartonGroup cartonGroup)
			: base(destinationCollection, findBoxList, moduleID)
		{
			CartonGroup = Argument.NotNull(cartonGroup, nameof(cartonGroup));
		}

		WhsCartonGroup CartonGroup { get; }

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			var org = CartonGroup.Factory.Load<OrgHeader>(pk);
			if (org != null)
			{
				CartonGroup.ParentOrgMiscServs.Add(org.MiscServ);
			}
			return org != null;
		}

		protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
		{
			return ((OrgMiscServ)bizObj).OM_OH;
		}
	}
}
