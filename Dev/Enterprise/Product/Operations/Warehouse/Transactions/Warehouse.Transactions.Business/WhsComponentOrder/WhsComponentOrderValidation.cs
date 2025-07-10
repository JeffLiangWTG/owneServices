using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsComponentOrderValidation : WhsPickableDocketValidation
	{
		protected WhsComponentOrderValidation(WhsComponentOrder parent)
			: base(parent)
		{
		}

		#region Validate_FinalizePickWhenWorkOrderIsFinalized

		public void Validate_FinalizePickWhenWorkOrderIsFinalized()
		{
			if (Parent.IsFinalised)
			{
				var pick = Parent.Pick;
				if (pick != null && !pick.IsFinalised && !pick.IsFinalising)
				{
					var msg = Res.GetString("7a007bb7-dda9-45a7-905c-5e89200b6c96", "Job was finalized but Pick was not finalized.");
					if (pick.HasErrors())
					{
						msg += string.Format(Culture.Current, "\r\n{0}", pick.GetErrors().ToUniqueMessageListString());
					}
					Parent.AddRowError(msg);
				}
			}
		}

		#endregion
	}
}
