//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccInvMsgValidation
//
//    This class should be used for overriding validation in AutoAccInvMsgValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccInvMsgValidation : AutoAccInvMsgValidation
	{
		public AccInvMsgValidation(AutoAccInvMsg parent) : base(parent)
		{
		}

		protected override void CheckA9_Code()
		{
			base.CheckA9_Code();
			MandatoryValidation.CheckEntered(Parent.A9_CodeInfo);
			if (!Parent.A9_CodeInfo.HasErrors())
			{
				ZQuery filter = new ZQuery(AccInvMsgSchema.A9_Code, Parent.A9_Code);
				filter.AddToFilter(AccInvMsgSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddToFilter(AccInvMsgSchema.A9_RN_NKCountryCode, SQLComparisonOperator.Equal, Parent.A9_RN_NKCountryCode);

				AccInvMsg invMsg = Parent.Factory.LoadTop1<AccInvMsg>(filter);
				if (invMsg != null)
				{
					Parent.A9_CodeInfo.AddError(Res.GetString("1f19d1a4-264b-43a0-8231-59dc18c92f17", "An Invoice Tax Message with this code already exists. Please enter a new Code."));
				}
			}
		}

		protected override void CheckA9_EnglishMsg()
		{
			base.CheckA9_EnglishMsg();
			MandatoryValidation.CheckEntered(Parent.A9_EnglishMsgInfo);
			TranslatableDataFieldAttribute.Validate(Parent.A9_EnglishMsgInfo);
		}

		protected override void CheckA9_TaxGroupCode()
		{
			base.CheckA9_TaxGroupCode();
			var hasRows = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Environment.Env.CurrentCompanyPK, System.Guid.Empty, System.Guid.Empty).Any();
			if (hasRows)
			{
				MandatoryValidation.CheckEntered(Parent.A9_TaxGroupCodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.A9_TaxGroupCodeInfo);
			}
		}
	}
}
