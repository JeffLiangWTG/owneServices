using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class ContainerProvider : IContainerInfo
	{
		public ContainerProvider(Customs.Business.CusContainerInvoiceLinePivot cusContainerInvoiceLinePivot, ZDateTime effectiveAssessmentDate)
		{
			CusContainerInvoiceLinePivot = Argument.NotNull(cusContainerInvoiceLinePivot, nameof(cusContainerInvoiceLinePivot));
			EffectiveAssessmentDate = effectiveAssessmentDate;
		}
		Customs.Business.CusContainerInvoiceLinePivot CusContainerInvoiceLinePivot { get; }
		ZDateTime EffectiveAssessmentDate { get; }

		public string ContainerNo => CusContainerInvoiceLinePivot.Container != null ? CusContainerInvoiceLinePivot.Container.CO_ContainerNumber : ZString.Empty;

		public string CountryCode
		{
			get
			{
				if (CusContainerInvoiceLinePivot.Container == null || CusContainerInvoiceLinePivot.Container.OwnerCountry == null)
				{
					return ZString.Empty;
				}
				return UniversalReferenceDataHelper.MapCW1CountryCodeToCustomsCode(CusContainerInvoiceLinePivot.Factory, CusContainerInvoiceLinePivot.Container.OwnerCountry.Code, EffectiveAssessmentDate);
			}
		}
	}
}
