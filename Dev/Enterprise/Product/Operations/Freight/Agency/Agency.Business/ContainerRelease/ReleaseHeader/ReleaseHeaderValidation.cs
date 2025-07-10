using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public class ReleaseHeaderValidation : AutoReleaseHeaderValidation
	{
		public ReleaseHeaderValidation(AutoReleaseHeader parent)
			: base(parent) { }

		#region ReleaseNumber

		protected override void CheckReleaseNumber()
		{
			base.CheckReleaseNumber();

			if (Parent.IsReplacement)
			{
				MandatoryValidation.CheckEntered(Parent.ReleaseNumberInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ReleaseNumberInfo, Parent.Lookups.ReleaseNumbers);
			}
		}

		#endregion

		#region IncludeMessage

		protected override void CheckIncludeMessage()
		{
			base.CheckIncludeMessage();

			if (Parent.IncludeMessage && !(Parent.SupportsMessageSending && Parent.IncludeMessageEnabled))
			{
				Parent.IncludeMessageInfo.AddError(Res.GetString("4776DD4B-54D8-4EBA-9AAE-E60921367C08", "Principal is not configured for sending Export Pre-Advice message to {0}", Parent.Shipment.JS_NKLoadPort));
			}
		}

		#endregion

		#region Implementation

		public new ReleaseHeader Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReleaseHeader)base.Parent; }
		}

		#endregion
	}
}


