//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEDIMessagePurposeValidation
//
//    This class should be used for overriding validation in AutoEDIMessagePurposeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class EDIMessagePurposeValidation : AutoEDIMessagePurposeValidation
	{
		public EDIMessagePurposeValidation(AutoEDIMessagePurpose parent) : base(parent)
		{
		}

		protected override void CheckEMP_Code()
		{
			base.CheckEMP_Code();
			MandatoryValidation.CheckEntered(Parent.EMP_CodeInfo);
			var duplicateCodeQuery = new ZQuery(EDIMessagePurposeSchema.EMP_Code, Parent.EMP_Code);
			if (Parent.Factory.Load<EDIMessagePurpose>(duplicateCodeQuery).Length > 1)
			{
				Parent.EMP_CodeInfo.AddError(GetNonUniqueCodeError(Parent.EMP_CodeInfo));
			}
		}

		protected override void CheckEMP_Description()
		{
			base.CheckEMP_Description();
			MandatoryValidation.CheckEntered(Parent.EMP_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.EMP_DescriptionInfo);
		}

		public static string GetNonUniqueCodeError(ZPropertyInfo info) => Res.GetString("987336df-2e62-42d9-8e89-b0f18d144344", "The {0} field with the value [{1}] must be unique.", info.HumanReadableName, info.Value);
	}
}
