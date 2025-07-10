using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusFiscalReference : EU.Business.Declaration.CusFiscalReference
{
	public CusFiscalReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	[ResourceStringData("8D2D6D33-58BC-4DD7-BD96-E0E53D938F6D", Caption = "Tax Number (TIN)")]
	public override ZString CFR_Reference
	{
		get => base.CFR_Reference;
		set => base.CFR_Reference = value;
	}
}
