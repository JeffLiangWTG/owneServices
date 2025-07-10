using System;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketLabelLineValidation : ZValidation
	{
		public WhsDocketLabelLineValidation(WhsDocketLabelLine parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly WhsDocketLabelLine Parent;

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateNumberOfLabelsToPrint();
		}

		#endregion

		#region ValidateNumberOfLabelsToPrint

		public void ValidateNumberOfLabelsToPrint()
		{
			ValidateCalculatedProperty(Parent.NumberOfLabelsToPrintInfo);
		}

		protected void CheckNumberOfLabelsToPrint()
		{
			if (Parent.NumberOfLabelsToPrint < 0)
			{
				Parent.NumberOfLabelsToPrintInfo.AddError(Res.GetString("096E61BB-00FA-4573-AE73-530B2F73D75F", "Please enter the number of labels to print.  It can be 0 or greater."));
			}
			if (Parent.NumberOfLabelsToPrint > Parent.TotalNumberOfLabels)
			{
				Parent.NumberOfLabelsToPrintInfo.AddError(Res.GetString("54699486-1CE4-44E7-94A3-AC167A4B1FB8", "You cannot print more labels than available."));
			}
		}

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(WhsDocketLabelLineValidation); }
		}

		#endregion
	}
}
