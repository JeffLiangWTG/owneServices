using System.Linq;
using System.Text.RegularExpressions;

namespace Enterprise.Customs.TW.Business
{
	public class EthanolPermitNumberCusSupportingValidation : Customs.Business.CusSupportingInfoValidation
	{
		public EthanolPermitNumberCusSupportingValidation(EthanolPermitNumberCusSupporting parent) : base(parent)
		{
		}

		public new EthanolPermitNumberCusSupporting Parent => (EthanolPermitNumberCusSupporting)base.Parent;

		bool IsCapitalLettersAndNumbersOnly(string val)
		{
			return Regex.IsMatch(val, @"^[A-Z0-9]*$");
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			var parent = Parent;
			var referenceNumber = parent.CSI_ReferenceNumber;
			var targetInfo = parent.CSI_ReferenceNumberInfo;
			if (!referenceNumber.IsEmpty)
			{
				var parentPK = parent.PK;
				if (!IsCapitalLettersAndNumbersOnly(referenceNumber))
				{
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.CapitalLettersAndNumbersOnly);
				}

				if (parent.Parent.EthanolPermitNumbers.Cast<EthanolPermitNumberCusSupporting>().Any(x => x.CSI_ReferenceNumber == referenceNumber && x.PK != parentPK))
				{
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.EthanolPermitNumberCannotBeDuplicated);
				}

				if (referenceNumber.Length != 14)
				{
					targetInfo.AddMessageError(Res.GetString("C7593B19-7CC3-4FAC-8C42-E47E3374C4D0", "{0} must consist 14 alphanumeric characters.", targetInfo.HumanReadableName));
				}
			}
		}
	}
}
