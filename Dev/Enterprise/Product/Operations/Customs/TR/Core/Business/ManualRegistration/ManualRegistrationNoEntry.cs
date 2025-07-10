using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TR.Business
{
	public class ManualRegistrationNoEntry : NonPersistentBusinessObject
	{
		public ManualRegistrationNoEntry(ZString registrationNumber, ZDateTime registrationDate)
		{
			this.fRegistrationDate = registrationDate;
			this.fRegistrationNumber = registrationNumber;
		}

		[ResourceStringData("676AFF4D-C9C3-4CCF-B1F0-30856D9BCA2F", Caption = "Registration Date", ShortCaption = "Date")]
		public ZDateTime RegistrationDate
		{
			get { return fRegistrationDate; }
			set { fRegistrationDate = value; }
		}
		ZDateTime fRegistrationDate;

		[ResourceStringData("7CFDC863-C5A8-4D70-BA53-3C0CF3A4FEE6", Caption = "Registration Number", ShortCaption = "Reg. Number")]
		[CargoWise.ComponentModel.MaxLength(18)]
		public ZString RegistrationNumber
		{
			get { return fRegistrationNumber; }
			set
			{
				if (fRegistrationNumber != value)
				{
					CheckMaximumLength(RegistrationNumberInfo, value);
					SetNonPersistentPropertyValue(RegistrationNumberInfo, ref fRegistrationNumber, value);
				}
			}
		}
		ZString fRegistrationNumber;

		public ZPropertyInfo RegistrationNumberInfo
		{
			get { return GetZPropertyInfo(nameof(RegistrationNumber)); }
		}
	}
}
