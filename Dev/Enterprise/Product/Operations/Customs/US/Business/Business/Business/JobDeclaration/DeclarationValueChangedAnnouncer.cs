using System;

namespace Enterprise.Customs.US.Business
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration.US_EnableENSInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_EnableAIIInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_TariffTypeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_InbondTypeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_EntryTypeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_IsInvoiceByRequestInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_SchDLoadingInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.JE_DateOfArrivalInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.US_CargoReleaseTypeInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public override void Dispose()
		{
			base.Dispose();

			Declaration.US_EnableENSInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_EnableAIIInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_TariffTypeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_InbondTypeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_EntryTypeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_IsInvoiceByRequestInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_SchDLoadingInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.JE_DateOfArrivalInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.US_CargoReleaseTypeInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}
	}
}
