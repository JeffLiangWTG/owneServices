using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class ExemptionOfControllingAgenciesCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public ExemptionOfControllingAgenciesCusSupportingValidation(ExemptionOfControllingAgenciesCusSupporting parent) : base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent as ExemptionOfControllingAgenciesCusSupporting;
			var referenceNumber = parent.CSI_ReferenceNumber;
			if (!referenceNumber.IsEmpty)
			{
				var targetInfo = parent.CSI_ReferenceNumberInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);

				var invoiceLine = parent.Parent;
				if (invoiceLine != null)
				{
					ValidationHelper.CheckPermitNumbersPlusSpecialCodeNumber(invoiceLine, targetInfo);
					var count = invoiceLine.ExemptionOfControllingAgenciesCusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().Count(c => c.CSI_ReferenceNumber == referenceNumber);
					if (invoiceLine.ExemptionOfControllingAgenciesCusSupportings.Cast<ExemptionOfControllingAgenciesCusSupporting>().Any(x => x.CSI_ReferenceNumber == referenceNumber && x.PK != parent.PK))
					{
						targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.SpecialCodeCannotBeDuplicated);
					}
				}
			}
		}
	}
}
