//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefNMFCValidation
//
//    This class should be used for overriding validation in AutoRefNMFCValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefNMFCValidation : AutoRefNMFCValidation
	{
		public RefNMFCValidation(AutoRefNMFC parent) : base(parent)
		{
		}

		#region FN_ItemNo

		protected override void CheckFN_ItemNo()
		{
			base.CheckFN_ItemNo();
			MandatoryValidation.CheckEntered(Parent.FN_ItemNoInfo);
			if (!Parent.FN_ItemNo.IsNumbersOnlyOrEmpty)
			{
				Parent.FN_ItemNoInfo.AddError(Res.GetString("05ed5323-fedd-4a03-9d55-062324298c5b", "An Item No must consist of only numbers."));
			}
		}

		#endregion

		#region FN_Class

		protected override void CheckFN_Class()
		{
			base.CheckFN_Class();
			MandatoryValidation.CheckEntered(Parent.FN_ClassInfo);
			if (!ZDecimal.CanParse(Parent.FN_Class))
			{
				Parent.FN_ClassInfo.AddError(Res.GetString("5878C7B9-1DBB-4679-BE1D-B24ECC4E5170", "A Class must be a numeric value."));
			}
		}

		#endregion

		#region FN_Code

		protected override void CheckFN_Code()
		{
			base.CheckFN_Code();
			ZQuery filter = new ZQuery(RefNMFCSchema.FN_Code, Parent.FN_Code);
			filter.AddToFilter(RefNMFCSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			RefNMFC nmfc = Parent.Factory.LoadTop1<RefNMFC>(filter);
			if (nmfc != null)
			{
				Parent.FN_CodeInfo.AddError(Res.GetString("6a69b554-5c00-4603-b6db-e86686e8b302", "This article with corresponding Item No and Class already exists."));
			}

			MandatoryValidation.CheckEntered(Parent.FN_CodeInfo);
			if (Parent.FN_Code != Parent.FN_ItemNo + "|" + Parent.FN_Class)
			{
				Parent.FN_CodeInfo.AddError(Res.GetString("83cb0c89-e517-4089-96ce-90a7fa3ba893", "The Article Code needs to be created as Item No|Class."));
			}
		}

		#endregion
	}
}
