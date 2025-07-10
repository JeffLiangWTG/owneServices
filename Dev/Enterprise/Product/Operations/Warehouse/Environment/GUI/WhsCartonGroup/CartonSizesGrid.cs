using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.GUI
{
	public class CartonSizesGrid : ZModuleButtonGrid
	{
		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new CartonSizesGridAttacher(destinationCollection, findBoxList, moduleID, CartonGroup);
		}

		protected override IBusiness GetNewBusinessEntity(ZController controller)
		{
			var cartonSize = controller.Factory.New<WhsCartonSize>();

			var link = controller.Factory.New<WhsCartonGroupSizeLink>();
			link.WCV_WCS = cartonSize.PK;

			return link; // Base code attaches it to the group
		}

		protected override IZForm ShowFormForNewEntity(IBusiness businessEntity, ZController controller)
		{
			return base.ShowFormForNewEntity(((WhsCartonGroupSizeLink)businessEntity).CartonSize, controller);
		}

		protected override void Detach(BusinessObject selected)
		{
			selected.Delete();
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			return ((WhsCartonGroupSizeLink)selected).CartonSize;
		}

		WhsCartonGroup CartonGroup => (WhsCartonGroup)DataSource;
	}

	class CartonSizesGridAttacher : ZRecordAttacher
	{
		public CartonSizesGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, WhsCartonGroup cartonGroup)
			: base(destinationCollection, findBoxList, moduleID)
		{
			CartonGroup = Argument.NotNull(cartonGroup, nameof(cartonGroup));
		}

		WhsCartonGroup CartonGroup { get; }

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			var cartonSize = CartonGroup.Factory.Load<WhsCartonSize>(pk);

			if (cartonSize != null)
			{
				CartonGroup.CartonSizes.Add(cartonSize);
			}

			return cartonSize != null;
		}

		protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
		{
			return ((WhsCartonGroupSizeLink)bizObj).WCV_WCS;
		}
	}
}
