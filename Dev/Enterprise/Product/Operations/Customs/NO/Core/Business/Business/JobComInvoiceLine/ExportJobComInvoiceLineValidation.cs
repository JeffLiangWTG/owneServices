using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckJI_PrimaryPreference()
		{
			base.CheckJI_PrimaryPreference();
			var parent = Parent;
			var targetInfo = parent.JI_PrimaryPreferenceInfo;

			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			if (targetInfo.Value.IsEmpty)
			{
				targetInfo.AddWarning(Res.GetString("4C08C5B5-FBF8-48D9-9743-717C9A49495E", "Preference code is required. A value will be suggested on save."));
			}
		}

		protected override void CheckJI_StateOrRegionOfOrigin()
		{
			base.CheckJI_StateOrRegionOfOrigin();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_StateOrRegionOfOriginInfo);
		}
	}
}
