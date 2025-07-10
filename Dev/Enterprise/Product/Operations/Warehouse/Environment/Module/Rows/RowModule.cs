using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class RowModule : ZFilterGridModule
	{
		public RowModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsConfigRow; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigRow);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RowFilterControl(GridCollection, (RowFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RowFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsRowTypedBusinessObjectCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.WarehouseManagerCoreAnd4PL; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WhsConfigLocation; }
		}

		protected override IZForm ShowNewForm()
		{
			var form = (RowEntryForm)base.ShowNewForm();
			if (form != null)
			{
				var row = (WhsRow)form.BusinessEntity;
				var filterBizO = (RowFilterBusinessObject)FilterBusinessObject;

				if (!filterBizO.WR_WW_Whs.IsEmpty)
				{
					row.WR_WW_Whs = filterBizO.WR_WW_Whs;
				}
			}

			return form;
		}
	}

	public class WhsRowTypedBusinessObjectCollection : BusinessObjectCollection<WhsRow>
	{
		public WhsRowTypedBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
