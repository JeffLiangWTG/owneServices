//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccSurchargeApplicationValidation
//
//    This class should be used for overriding validation in AutoAccSurchargeApplicationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccSurchargeApplicationValidation : AutoAccSurchargeApplicationValidation
	{
		public AccSurchargeApplicationValidation(AutoAccSurchargeApplication parent) : base(parent)
		{
		}

		protected new AccSurchargeApplication Parent
		{
			get { return (AccSurchargeApplication)base.Parent; }
		}

		#region ASP_JobType

		protected override void CheckASP_JobType()
		{
			base.CheckASP_JobType();
			MandatoryValidation.CheckEntered(Parent.ASP_JobTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ASP_JobTypeInfo);
			CheckItIsNotDuplicate();
		}

		#endregion

		#region ASP_SupplyType

		protected override void CheckASP_SupplyType()
		{
			base.CheckASP_SupplyType();
			ListValidation.ErrorIfInvalidCode(Parent.ASP_SupplyTypeInfo);
			CheckItIsNotDuplicate();
		}

		#endregion

		#region ASP_HomeCountryOrZone

		protected override void CheckASP_HomeCountryOrZone()
		{
			base.CheckASP_HomeCountryOrZone();
			MandatoryValidation.CheckEntered(Parent.ASP_HomeCountryOrZoneInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ASP_HomeCountryOrZoneInfo);
			CheckItIsNotDuplicate();
		}

		#endregion

		#region ASP_OrganizationCategory

		protected override void CheckASP_OrganizationCategory()
		{
			base.CheckASP_OrganizationCategory();
			MandatoryValidation.CheckEntered(Parent.ASP_OrganizationCategoryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ASP_OrganizationCategoryInfo);
			CheckItIsNotDuplicate();
		}

		#endregion

		#region ASP_PlaceOfSupply

		protected override void CheckASP_PlaceOfSupply()
		{
			base.CheckASP_PlaceOfSupply();
			if (!Parent.ASP_PlaceOfSupply.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ASP_PlaceOfSupplyInfo);
				CheckItIsNotDuplicate();
			}
		}

		#endregion

		#region ASP_ASC_NKSurchargeCode

		protected override void CheckASP_ASC_NKSurchargeCode()
		{
			base.CheckASP_ASC_NKSurchargeCode();
			MandatoryValidation.CheckEntered(Parent.ASP_ASC_NKSurchargeCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ASP_ASC_NKSurchargeCodeInfo);
			CheckItIsNotDuplicate();
		}

		#endregion

		protected override void CheckASP_AT()
		{
			base.CheckASP_AT();

			ListValidation.ErrorIfInvalidPK(Parent.ASP_ATInfo);
		}

		#region DuplicateCheck

#if DEBUG
		public
#endif
		void CheckItIsNotDuplicate()
		{
			var isDuplicateErrorString = Res.GetString("1ad0ee6c-b15f-4beb-98a3-7b07799e6c2c", "There should not be more than one row with the same conditions. (i.e. Job Type + Supply Type + Organization Country/Region Or Zone + Organization Category + SELL FPOS + Surcharge Code.)");
			Parent.RemoveRowError(isDuplicateErrorString);

			var parentCollection = Parent.Company.AccSurchargeApplications;

			if (parentCollection != null)
			{
				var hasDuplicateSetting = parentCollection.Cast<AccSurchargeApplication>()
													.Any(x => x != Parent &&
															x.ASP_JobType == Parent.ASP_JobType &&
															x.ASP_SupplyType == Parent.ASP_SupplyType &&
															x.ASP_HomeCountryOrZone == Parent.ASP_HomeCountryOrZone &&
															x.ASP_OrganizationCategory == Parent.ASP_OrganizationCategory &&
															x.ASP_PlaceOfSupply == Parent.ASP_PlaceOfSupply &&
															x.ASP_ASC_NKSurchargeCode == Parent.ASP_ASC_NKSurchargeCode);
				if (hasDuplicateSetting)
				{
					Parent.AddRowError(isDuplicateErrorString);
				}
			}
		}
		#endregion
	}
}
