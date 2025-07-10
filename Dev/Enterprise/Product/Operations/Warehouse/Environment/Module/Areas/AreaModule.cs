using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
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
	public class AreaModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsConfigArea;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigArea);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AreaFilterControl(GridCollection, (AreaFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AreaFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsAreaTypedBusinessObjectCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigArea;

		protected override IZForm ShowNewForm()
		{
			var form = (AreaEntryForm)base.ShowNewForm();
			if (form != null)
			{
				var area = (WhsArea)form.BusinessEntity;
				var filterBizO = (AreaFilterBusinessObject)FilterBusinessObject;

				if (area != null && filterBizO != null)
				{
					if (!filterBizO.WA_WW_Whs.IsEmpty)
					{
						area.WA_WW_Whs = filterBizO.WA_WW_Whs;
					}
				}
			}
			return form;
		}
	}

	public class WhsAreaTypedBusinessObjectCollection : BusinessObjectCollection<WhsArea>
	{
		public WhsAreaTypedBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer result = null;

			if (property.Name == WhsArea.Schema.WA_CalcTransitClientDescription)
			{
				result = new WhsAreaComparer(WhsArea.Schema.WA_CalcTransitClientDescription, direction);
			}

			return result ?? base.GetComparerForSort(property, direction);
		}

		#region WhsAreaComparer

		class WhsAreaComparer : IComparer<WhsArea>, IComparer
		{
			public WhsAreaComparer(ZString columnName, ListSortDirection direction)
			{
				this.columnName = columnName;
				this.direction = direction;
			}

			int Compare(WhsArea x, WhsArea y)
			{
				int result = 0;
				if (columnName == WhsArea.Schema.WA_CalcTransitClientDescription)
				{
					result = CompareCore(x.WA_CalcTransitClientDescription, y.WA_CalcTransitClientDescription);
				}
				return direction == ListSortDirection.Ascending ? result : -result;
			}

			int CompareCore(ZString x, ZString y)
			{
				var result = string.Compare(x.ToString(), y.ToString(), System.StringComparison.CurrentCulture);

				if (result != 0)
				{
					var ascending = direction == ListSortDirection.Ascending;
					if (x == WhsArea.naString)
					{
						result = ascending ? 1 : -1;
					}
					if (y == WhsArea.naString)
					{
						result = ascending ? -1 : 1;
					}
				}

				return result;
			}

			int IComparer<WhsArea>.Compare(WhsArea x, WhsArea y)
			{
				return Compare(x, y);
			}

			int IComparer.Compare(object x, object y)
			{
				return Compare((WhsArea)x, (WhsArea)y);
			}

			readonly ZString columnName;
			readonly ListSortDirection direction;
		}

		#endregion
	}
}
