//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobPackLineHarmonisedCodeValidation
//
//    This class should be used for overriding validation in AutoJobPackLineHarmonisedCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Freight.Business
{
	using System.Text.RegularExpressions;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;
	using Res = Enterprise.Freight.Res;

	public class JobPackLineHarmonisedCodeValidation : AutoJobPackLineHarmonisedCodeValidation
	{
		public JobPackLineHarmonisedCodeValidation(AutoJobPackLineHarmonisedCode parent) : base(parent)
		{
		}

		#region Parent

		public new JobPackLineHarmonisedCode Parent => (JobPackLineHarmonisedCode)base.Parent;

		#endregion

		protected override void CheckJLH_RN_NKCountry()
		{
			base.CheckJLH_RN_NKCountry();

			if (!Parent.IsAutoAddedItem)
			{
				MandatoryValidation.CheckEntered(Parent.JLH_RN_NKCountryInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JLH_RN_NKCountryInfo);

			if (!Parent.JLH_RN_NKCountry.IsEmpty && IsDuplicateCombination())
			{
				Parent.JLH_RN_NKCountryInfo.AddError(DuplicateError);
			}
		}

		protected override void CheckJLH_Code()
		{
			base.CheckJLH_Code();

			if (!Parent.IsAutoAddedItem)
			{
				MandatoryValidation.CheckEntered(Parent.JLH_CodeInfo);
			}

			if (!Parent.JLH_Code.IsEmpty)
			{
				if (!Regex.IsMatch(Parent.JLH_Code, @"^[0-9]+[0-9\.]{3,}$"))
				{
					var notification = Res.GetString("f8b2e12c-9a34-40f1-922d-3255a9cdc990", "Invalid Harmonized Code. Only numeric characters (minimum 4) and dots are allowed. Code is mandatory when Country/Region is entered.");

					if (Parent.IsInDatabase && !Parent.JLH_CodeInfo.HasChanges)
					{
						Parent.JLH_CodeInfo.AddWarning(notification);
					}
					else
					{
						Parent.JLH_CodeInfo.AddError(notification);
					}
				}

				if (IsDuplicateCombination())
				{
					Parent.JLH_CodeInfo.AddError(DuplicateError);
				}
			}
		}

		bool IsDuplicateCombination()
		{
			var query = new ZQuery(JobPackLineHarmonisedCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(JobPackLineHarmonisedCodeSchema.JLH_JL, Parent.JLH_JL);
			query.AddToFilter(JobPackLineHarmonisedCodeSchema.JLH_RN_NKCountry, Parent.JLH_RN_NKCountry);
			query.AddToFilter(JobPackLineHarmonisedCodeSchema.JLH_Code, Parent.JLH_Code);

			var duplicate = Parent.Factory.LoadTop1<JobPackLineHarmonisedCode>(query);
			return duplicate != null;
		}

		string DuplicateError => Res.GetString("0702ab5d-d0a1-4e84-9036-75a240107306", "The Country/Region and Code must be unique.");
	}
}
