using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickShortLineValidation : AutoWhsPickShortLineValidation
	{
		public WhsPickShortLineValidation(AutoWhsPickShortLine parent)
			: base(parent)
		{
		}

		protected new WhsPickShortLine Parent
		{
			get { return (WhsPickShortLine)base.Parent; }
		}

		#region CheckWZS_GS_NKShortedBy

		protected override void CheckWZS_GS_NKShortedBy()
		{
			base.CheckWZS_GS_NKShortedBy();
			MandatoryValidation.CheckEntered(Parent.WZS_GS_NKShortedByInfo);
		}

		#endregion

		#region CheckWZS_ShortUnits

		protected override void CheckWZS_ShortUnits()
		{
			base.CheckWZS_ShortUnits();

			if (Parent.WZS_ShortUnits <= 0m)
			{
				Parent.WZS_ShortUnitsInfo.AddError(Res.GetString("c179dd9d-7ed4-421b-8274-531b98e1952d", "Pick Short Line Units must be greater than zero."));
			}
			else
			{
				var inventoryLine = Parent.InventoryLine;
				if (inventoryLine != null && Parent.WZS_ShortUnits > inventoryLine.WE_StockOnHand)
				{
					Parent.WZS_ShortUnitsInfo.AddError(Res.GetString("2a2750d4-e948-41f0-ae44-661b87bf4e67", "Pick Short Line Units must not exceed Available Stock on related Inventory Line record."));
				}
			}
		}

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> !FKsToNotValidateForCancelledRecords.Contains(info.Name) && base.ShouldValidateFKToCancelledRecord(info);

		static readonly ImmutableHashSet<string> FKsToNotValidateForCancelledRecords
			= ImmutableHashSet.Create(WhsPickShortLineSchema.Constants.WZS_WE_InventoryLine, WhsPickShortLineSchema.Constants.WZS_WE_TransactionLine);

		#endregion
	}
}
