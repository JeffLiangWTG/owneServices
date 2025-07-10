using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class UpdateRateValidation : AutoUpdateRateValidation
	{
		public UpdateRateValidation(AutoUpdateRate parent)
			: base(parent) { }

		protected override void CheckClientPK()
		{
			base.CheckClientPK();

			MandatoryValidation.CheckEntered(Parent.ClientPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.ClientPKInfo, Parent.Lookups.Clients);

			if (Parent.ClientRate == null)
			{
				Parent.ClientPKInfo.AddError(Res.GetString("2d20ed2c-5739-4f8c-a2f8-9972dcb0ebef", "This client has no client rate in the system. You cannot send an update to them."));
			}

			if (!Parent.ClientPK.IsEmpty)
			{
				var rateCollection = GetRateCollection();

				if (rateCollection != null)
				{
					foreach (UpdateRate rate in rateCollection)
					{
						if (rate != Parent && rate.ClientPK == Parent.ClientPK)
						{
							Parent.ClientPKInfo.AddError(Res.GetString("ae1063b8-2eb3-4b49-b37c-f5c724ed9ba1", "You cannot have the same organization listed more than once."));
							break;
						}
					}
				}
			}
		}

		#region Implementation

		UpdateRateCollection GetRateCollection()
		{
			foreach (var collection in Parent.ParentCollections)
			{
				var potential = collection as UpdateRateCollection;

				if (potential != null)
				{
					return potential;
				}
			}

			return null;
		}

		public new UpdateRate Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (UpdateRate)base.Parent; }
		}

		#endregion
	}
}

