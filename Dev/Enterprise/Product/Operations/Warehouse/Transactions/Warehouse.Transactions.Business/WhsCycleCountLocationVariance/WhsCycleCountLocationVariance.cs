using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DeferTriggerAndRunBeforeCommit("TG_WhsCycleCountLocationVariance_HasSameStatus", WhsValidationHelper.WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatus, WhsCycleCountLocationVarianceSchema.Constants.PK, typeof(IWhsCycleCountLocationVarianceHasSameStatusDeferTriggerStrategy))]
	public class WhsCycleCountLocationVariance : AutoWhsCycleCountLocationVariance
	{
		public WhsCycleCountLocationVariance(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventModifyOfKeyFieldsTriggerError = "Attempted to change key fields.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventDeleteTriggerError = "Cycle Count Variances cannot be deleted.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventVariancesWithDifferentStatus = "All variances do not have same status.";

		#region CycleCount

		public WhsCycleCountLocation CycleCount
		{
			get { return Factory.Load<WhsCycleCountLocation>(WCC_WCL_CycleCountLocation); }
		}

		[RelatedBusinessObject("CycleCount")]
		public override ZGuid WCC_WCL_CycleCountLocation { get => base.WCC_WCL_CycleCountLocation; set => base.WCC_WCL_CycleCountLocation = value; }

		#endregion

		#region Properties

		public WhsProduct WhsProduct => Product != null ? WhsProduct.GetWhsProduct(Product) : null;

		#endregion
	}
}
