using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ProductionRules.Business
{
	public class ProductionRulesFactTypeView : AutoProductionRulesFactTypeView
	{
		public ProductionRulesFactTypeView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override void Delete()
		{
			throw new NotSupportedException($"Deletion of the {nameof(ProductionRulesFactTypeView)} is not supported.");
		}

		public override bool ReadOnly
		{
			get => true;
			set => base.ReadOnly = value;
		}

		#endregion
	}
}
