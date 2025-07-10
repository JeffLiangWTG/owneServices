using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

sealed class CusGoodsLocationValidation : EU.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(CusGoodsLocation parent) : base(parent)
	{
	}

	protected override void CheckCGL_Qualifier()
	{
		base.CheckCGL_Qualifier();
		CheckRuleR0086E(Parent.CGL_QualifierInfo, message: Res.GetString("89CF1768-4C61-4661-B9EF-56688A32AFE4", "[R0086E] You have not entered a Qualifier for Location of Goods."));
	}

	protected override void CheckCGL_Type()
	{
		base.CheckCGL_Type();
		CheckRuleR0086E(Parent.CGL_TypeInfo, message: Res.GetString("D08B044D-9F92-4630-B416-602E23644415", "[R0086E] You have not entered a Type for Location of Goods."));
	}

	void CheckRuleR0086E(ZPropertyInfo propertyInfo, string message)
	{
		var cusEntryInstruction = Parent.Parent as CusEntryInstruction;
		if (cusEntryInstruction?.InvoiceLines?.Any(x => Constants.Rules.R0086EProcedureFirstTwoNumbers.Contains(x.JI_Procedure.SubstringSafe(0, 2))
			|| Constants.Rules.R0086EProcedure.Contains(x.JI_Procedure)) ?? false)
		{
			if (propertyInfo.Value.IsEmpty)
			{
				propertyInfo.AddMessageError(message);
			}
		}
	}

	new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;
}
