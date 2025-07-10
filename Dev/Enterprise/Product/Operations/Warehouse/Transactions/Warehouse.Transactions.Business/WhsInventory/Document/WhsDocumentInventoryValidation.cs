using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocumentInventoryValidation : ZValidation
	{
		public WhsDocumentInventoryValidation(WhsDocumentInventory parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly WhsDocumentInventory Parent;

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateLabelsToPrint();
		}

		#endregion

		#region ValidateLabelsToPrint

		public void ValidateLabelsToPrint()
		{
			ValidateCalculatedProperty(Parent.LabelsToPrintInfo);
		}

		protected void CheckLabelsToPrint()
		{
			if (Parent.LabelsToPrint < 0)
			{
				Parent.LabelsToPrintInfo.AddError(Res.GetString("WhsDocumentInventory|NotEnoughLabelsError", "Please enter the number of labels to print"));
			}
			else if (Parent.LabelsToPrint > Parent.TotalLabelsToPrintFrom)
			{
				Parent.LabelsToPrintInfo.AddError(Res.GetString("WhsDocumentInventory|TooManyLabelsError", "You cannot print more labels than available"));
			}
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(WhsDocumentInventoryValidation); }
		}

		#endregion
	}
}
