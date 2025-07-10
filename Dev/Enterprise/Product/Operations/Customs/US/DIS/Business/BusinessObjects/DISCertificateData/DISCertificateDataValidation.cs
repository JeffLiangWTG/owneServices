using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISCertificateDataValidation : AutoDISCertificateDataValidation
	{
		public DISCertificateDataValidation(AutoDISCertificateData bizObj)
			: base(bizObj)
		{
		}

		protected override void CheckCertificateNumber()
		{
			base.CheckCertificateNumber();
			MandatoryValidate(AutoDISCertificateData.Schema.CertificateNumber);
		}

		protected override void CheckExpiryDate()
		{
			base.CheckExpiryDate();
			MandatoryValidate(AutoDISCertificateData.Schema.ExpiryDate);
		}

		protected override void CheckIssueDate()
		{
			base.CheckIssueDate();
			MandatoryValidate(AutoDISCertificateData.Schema.IssueDate);
		}

		protected override void CheckGrossTonnage()
		{
			base.CheckGrossTonnage();
			MandatoryValidate(AutoDISCertificateData.Schema.GrossTonnage);
		}

		protected override void CheckNetTonnage()
		{
			base.CheckNetTonnage();
			MandatoryValidate(AutoDISCertificateData.Schema.NetTonnage);
		}

		void MandatoryValidate(string propertyName)
		{
			var disCertificateData = Parent as DISCertificateData;
			if (disCertificateData != null)
			{
				var disFormCusCode = disCertificateData.Document?.DISFormCusCode;
				if (disFormCusCode != null && disFormCusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.USDISRequiredData, propertyName))
				{
					var propertyInfo = disCertificateData.FindPropertyInfo(propertyName);
					if (propertyInfo != null)
					{
						MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
					}
				}
			}
		}
	}
}
