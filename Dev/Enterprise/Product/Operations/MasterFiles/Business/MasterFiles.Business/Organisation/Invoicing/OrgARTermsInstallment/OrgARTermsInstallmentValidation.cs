//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgARTermsInstallmentValidation
//
//    This class should be used for overriding validation in AutoOrgARTermsInstallmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;

	public class OrgARTermsInstallmentValidation : AutoOrgARTermsInstallmentValidation
	{
		public OrgARTermsInstallmentValidation(AutoOrgARTermsInstallment parent) : base(parent)
		{
		}

		protected override void CheckML_SequenceNumber()
		{
			base.CheckML_SequenceNumber();
			if (Parent.Terms != null)
			{
				if (Parent.Terms.ARTermsInstallments.Cast<OrgARTermsInstallment>().Any(x => x.ML_SequenceNumber == Parent.ML_SequenceNumber
																						&& x.ML_PY_Terms == Parent.ML_PY_Terms
																						&& x.PK != Parent.PK))
				{
					Parent.ML_SequenceNumberInfo.AddError(Res.GetString("79EE03FB-C8A3-499E-B625-A2AA67A7269F", "Sequence number already exists."));
				}
			}
		}

		protected override void CheckML_SplitPercentage()
		{
			base.CheckML_SplitPercentage();
			if (!IsTotalPercentageValid())
			{
				if (!Parent.RowNotifications.Contains(PercentageNotValid))
				{
					Parent.AddRowError(PercentageNotValid);
				}
			}
			else
			{
				var terms = Parent.Terms;
				if (terms != null && !terms.ARTermsInstallments.IsNullOrEmpty())
				{
					foreach (AutoOrgARTermsInstallment installment in terms.ARTermsInstallments)
					{
						installment.RemoveRowError(PercentageNotValid);
					}
				}
			}

			if (Parent.ML_SplitPercentage <= 0)
			{
				Parent.ML_SplitPercentageInfo.AddError(Res.GetString("0ABF4EF4-893C-4AA1-965F-3A61D4FE066B", "Percentage must be greater than zero."));
			}
		}

		protected bool IsTotalPercentageValid()
		{
			var terms = Parent.Terms;
			if (terms != null && !terms.ARTermsInstallments.IsNullOrEmpty())
			{
				var sum = Parent.Terms.ARTermsInstallments.Cast<OrgARTermsInstallment>().Sum(x => x.ML_SplitPercentage);
				return sum == 100;
			}
			return true;
		}

		protected override void CheckML_AgreedPaymentMethod()
		{
			base.CheckML_AgreedPaymentMethod();
			ListValidation.ErrorIfInvalidCode(Parent.ML_AgreedPaymentMethodInfo);
		}

		protected static string PercentageNotValid => Res.GetString("FF7DDBA3-7C1E-42FA-BE5A-9486C1EB241A", "Please check the percentage, to continue saving the sum of percentages present in rows must be equal to 100%.");
	}
}
