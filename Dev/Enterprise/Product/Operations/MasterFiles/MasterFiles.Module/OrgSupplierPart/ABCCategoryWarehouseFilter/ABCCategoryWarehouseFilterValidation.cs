using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class ABCCategoryWarehouseFilterValidation : ModuleFilterValidation
	{
		public ABCCategoryWarehouseFilterValidation(ABCCategoryWarehouseFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ABCCategoryWarehouseFilter parent;

		#region ValidateWJ_Category

		public void ValidateWJ_Category()
		{
			ValidateCalculatedProperty(parent.WJ_CategoryInfo);
		}

		protected void CheckWJ_Category()
		{
			ListValidation.WarnIfInvalidCode(parent.WJ_CategoryInfo);
		}

		#endregion

		#region ValidateWJ_WW_Warehouse

		public void ValidateWJ_WW_Warehouse()
		{
			ValidateCalculatedProperty(parent.WJ_WW_WarehouseInfo);
		}

		protected void CheckWJ_WW_Warehouse()
		{
			TypeValidation.CheckValidGuid(parent.WJ_WW_WarehouseInfo);
		}

		#endregion

		#region Overrides

		public override void ValidateAll()
		{
			ValidateWJ_Category();
			ValidateWJ_WW_Warehouse();
		}

		public override Type AutoValidationType
		{
			get { return typeof(ABCCategoryWarehouseFilterValidation); }
		}

		#endregion
	}
}
