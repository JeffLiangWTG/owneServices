using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderValidation : WhsComponentOrderValidation
	{
		public WhsDynamicWorkOrderValidation(WhsDynamicWorkOrder parent)
			: base(parent)
		{
		}

		protected override ZString TypeInMsg => Res.GetString("defa3656-1bc8-4bc0-bec3-dbb9abdcdc3f", "Dynamic Work Order");

		protected new WhsDynamicWorkOrder Parent => (WhsDynamicWorkOrder)base.Parent;

		#region CheckWD_RequiredDate

		protected override void CheckWD_RequiredDate()
		{
			base.CheckWD_RequiredDate();
			MandatoryValidation.CheckEntered(Parent.WD_RequiredDateInfo);
		}

		#endregion

		#region CheckWD_WW_Whs

		protected override void CheckWD_WW_Whs()
		{
			base.CheckWD_WW_Whs();
			CheckWarehouseIsVirtual();
		}

		void CheckWarehouseIsVirtual()
		{
			if (!Parent.WD_WW_WhsInfo.HasErrors())
			{
				var warehouse = Parent.Warehouse;
				if (warehouse != null && !warehouse.WW_IsVirtualWarehouse)
				{
					Parent.WD_WW_WhsInfo.AddError(Res.GetString("73c058a3-7e33-44f7-81c2-34fbe16eab03", "Warehouse used for Dynamic Work Orders must be virtual."));
				}
			}
		}

		#endregion

		#region CheckWD_IsInwardsProcessingJob

		protected override void CheckWD_IsInwardsProcessingJob()
		{
			base.CheckWD_IsInwardsProcessingJob();
			if (!Parent.WD_IsInwardsProcessingJobInfo.HasErrors()
				&& !Parent.WD_IsInwardsProcessingJob)
			{
				Parent.WD_IsInwardsProcessingJobInfo.AddError(Res.GetString("1a8baef4-b628-4f79-88f5-84bb010f2ba3", "Dynamic Work Orders must be Inward Processing Jobs."));
			}
		}

		#endregion

		#region CheckWD_TotalUnits

		protected override void CheckWD_TotalUnits()
		{
			base.CheckWD_TotalUnits();
			if (Parent.IsFinalising && WarehouseDataRegistry.Instance.TotalUnitsValidation.Value)
			{
				var totalLineUnits = GetTotalLineUnits();
				if (Parent.WD_TotalUnits != totalLineUnits)
				{
					Parent.WD_TotalUnitsInfo.AddError(Parent.IsAssembly
						? TotalUnitsValidationErrorForAssemblyLine(totalLineUnits)
						: TotalUnitsValidationErrorForDisassemblyLine(totalLineUnits));
				}
			}
		}

		decimal GetTotalLineUnits()
		{
			var result = 0m;
			if (Parent.IsAssembly)
			{
				result = Parent.Lines.Cast<WhsDynamicWorkOrderLine>().Sum(line => line.WE_TransactionQuantity);
			}
			else
			{
				result = Parent.CalculateTotalUnitsForDisassembly();
			}
			return result;
		}

		string TotalUnitsValidationErrorForAssemblyLine(decimal totalLineUnits)
			=> Res.GetString(
				"fc466659-ba18-4bc4-8602-db3b0507d343",
				"Total Units {0} does not equal the total of all assembly line units (including secondary products) {1}.",
				Parent.WD_TotalUnits,
				totalLineUnits);

		string TotalUnitsValidationErrorForDisassemblyLine(decimal totalLineUnits)
			=> Res.GetString(
				"b5d88d4d-20cd-4cd3-8bdf-65470b4fb14a",
				"Total Units {0} does not equal the total of all components: {1}.",
				Parent.WD_TotalUnits,
				totalLineUnits);

		#endregion

		#region CheckConsigneeNameOrPK

		protected override void ValidateConsigneeNameOrPKCore()
		{
			// Not required for Dynamic Work Orders
		}

		protected override void CheckConsigneeNameOrPK()
		{
			// Not required for Dynamic Work Orders
		}

		#endregion

		#region CheckTransportCoNameOrPK

		protected override void ValidateTransportCoNameOrPKCore()
		{
			// Not required for Dynamic Work Orders
		}

		protected override void CheckTransportCoNameOrPK()
		{
			// Not required for Dynamic Work Orders
		}

		#endregion

		#region GetLinesForWeightAndVolumeCalculation

		protected override IEnumerable<WhsDocketLine> GetLinesForWeightAndVolumeCalculation()
		{
			IEnumerable<WhsDocketLine> lines;

			if (Parent.IsAssembly)
			{
				lines = Parent
				.AllLines
				.Cast<WhsDynamicWorkOrderLine>()
				.Where(l => !l.WE_WE_ParentDocketLine.IsEmpty && (l.ParentLine?.IsMainInwardProcessedItem ?? false));
			}
			else
			{
				lines = Parent.Lines;
			}

			return lines;
		}

		#endregion
	}
}
