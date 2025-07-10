//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationValidation
//
//    This class should be used for overriding validation in AutoGlbAccreditationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationValidation : AutoGlbAccreditationValidation
	{
		public GlbAccreditationValidation(AutoGlbAccreditation parent) : base(parent)
		{
		}

		public new GlbAccreditation Parent
		{
			get { return (GlbAccreditation)base.Parent; }
		}

		protected override void CheckHAC_MustCompleteInDays()
		{
			base.CheckHAC_MustCompleteInDays();
			MandatoryValidation.CheckNotNegative(Parent.HAC_MustCompleteInDaysInfo);
		}

		GlbAccreditationCollection Accreditations
		{
			get
			{
				if (accreditations == null)
				{
					accreditations = new GlbAccreditationCollection(Parent.Factory);
				}

				return accreditations;
			}
		}

		GlbAccreditationCollection accreditations;

		protected override void CheckHAC_Code()
		{
			base.CheckHAC_Code();
			MandatoryValidation.CheckEntered(Parent.HAC_CodeInfo);

			if (Accreditations.Any(a => a.PK != Parent.PK && a.HAC_Code.EqualsIgnoringCase(Parent.HAC_Code)))
			{
				Parent.HAC_CodeInfo.AddError(Res.GetString("FF914200-0324-4611-A155-349FBACC44AC", "Code is not unique. Please enter a unique code."));
			}
		}

		protected override void CheckHAC_CertificateCode()
		{
			MandatoryValidation.CheckEntered(Parent.HAC_CertificateCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.HAC_CertificateCodeInfo);
		}

		protected override void CheckHAC_IsRefresher()
		{
			var otherAccreditationsWithMatchingCertificate = Accreditations.Where(x => x.PK != Parent.PK && x.HAC_CertificateCode == Parent.HAC_CertificateCode).ToArray();
			var isOtherMainAccreditation = otherAccreditationsWithMatchingCertificate.Any(x => !x.HAC_IsRefresher);
			var isOtherRefresherAccreditation = otherAccreditationsWithMatchingCertificate.Any(x => x.HAC_IsRefresher);

			if (Parent.HAC_IsRefresher)
			{
				if (isOtherRefresherAccreditation)
				{
					Parent.HAC_IsRefresherInfo.AddError(Res.GetString("ebf07f67-9292-488e-a3f4-a46a85f30dcf", "There can only be one refresher Accreditation for certificate code {0}", Parent.HAC_CertificateCode));
				}
				else if (!isOtherMainAccreditation)
				{
					Parent.HAC_IsRefresherInfo.AddError(Res.GetString("fc5b7d82-14d5-4e16-9400-7bc6b66efc55", "There is only a refresher Accreditation for certificate code {0}. Please add main Accreditation.", Parent.HAC_CertificateCode));
				}
			}
			else if (isOtherMainAccreditation)
			{
				Parent.HAC_IsRefresherInfo.AddError(Res.GetString("79a02acb-a911-4055-94a1-f935dc5f4e41", "There can only be one main Accreditation for certificate code {0}", Parent.HAC_CertificateCode));
			}
		}

		protected override void CheckHAC_RefresherCertificateExpiryType()
		{
			if (Parent.HAC_IsRefresher)
			{
				MandatoryValidation.CheckEntered(Parent.HAC_RefresherCertificateExpiryTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.HAC_RefresherCertificateExpiryTypeInfo);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.HAC_RefresherCertificateExpiryTypeInfo);
			}
		}
	}
}
