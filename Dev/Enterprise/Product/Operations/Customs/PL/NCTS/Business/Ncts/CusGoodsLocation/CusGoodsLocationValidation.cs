namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class CusGoodsLocationValidation : EU.NCTS.Business.CusGoodsLocationValidation
{
	public CusGoodsLocationValidation(EU.NCTS.Business.CusGoodsLocation parent) : base(parent)
	{ }

	protected override void CheckCGL_Qualifier()
	{
		base.CheckCGL_Qualifier();

		var parent = Parent;
		if (parent.CGL_Qualifier.IsEmpty && !parent.CGL_Type.IsEmpty)
		{
			parent.CGL_QualifierInfo.AddMessageError(Res.GetString("4607EDF5-5515-4984-AACA-F3120FF705E1", "Qualifier code is required for Location of Goods."));
		}
	}

	protected override void CheckCGL_Type()
	{
		base.CheckCGL_Type();

		var parent = Parent;
		if (parent.CGL_Type.IsEmpty && !parent.CGL_Qualifier.IsEmpty)
		{
			parent.CGL_TypeInfo.AddMessageError(Res.GetString("6D3229A0-2C32-456D-9B0E-FC88F4D69761", "Type code is required for Location of Goods."));
		}
	}
}
