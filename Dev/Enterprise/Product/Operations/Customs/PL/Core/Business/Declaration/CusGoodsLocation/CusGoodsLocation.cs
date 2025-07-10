using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public sealed class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.PL.ICusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override bool ContactPersonDataVisible => base.ContactPersonDataVisible && CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);
}
