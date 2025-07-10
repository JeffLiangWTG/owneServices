//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusReconDeclarationValidation
//
//    This class should be used for overriding validation in AutoCusReconDeclarationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusReconDeclarationValidation : CusReconBase.CusReconDeclarationValidation
	{
		public CusReconDeclarationValidation(CusReconDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckCRD_ApplicationCode()
		{
			base.CheckCRD_ApplicationCode();

			if (Parent.CRD_ApplicationCode.Length != 3)
			{
				Parent.CRD_ApplicationCodeInfo.AddError(Res.GetString("EE359709-D7A7-4AC5-87DD-8FB30E40FFD9", "Application Code must be 3 characters."));
			}
		}

		protected override void CheckCRD_DeclarationType()
		{
			base.CheckCRD_DeclarationType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CRD_DeclarationTypeInfo);
		}

		protected override void CheckCRD_DeclarantType()
		{
			base.CheckCRD_DeclarantType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CRD_DeclarantTypeInfo);
		}
	}
}
