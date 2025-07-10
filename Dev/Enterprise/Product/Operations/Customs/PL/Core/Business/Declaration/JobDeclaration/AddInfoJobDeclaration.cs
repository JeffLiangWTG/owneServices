using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration;

public partial class JobDeclaration
{
	public override ZString ZG_SpecificCircumstanceIndicator
	{
		get => base.ZG_SpecificCircumstanceIndicator;
		set
		{
			base.ZG_SpecificCircumstanceIndicator = value;
			InvoiceLines.MarkAsNeedingValidation();
		}
	}

	[MaxLength(2)]
	public override ZString JE_BorderTransportMeans { get => base.JE_BorderTransportMeans; set => base.JE_BorderTransportMeans = value; }
}
