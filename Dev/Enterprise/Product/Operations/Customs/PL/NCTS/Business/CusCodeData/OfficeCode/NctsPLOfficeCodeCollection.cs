using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsPLOfficeCodeCollection : EU.NCTS.Business.NctsEuOfficeCodeCollection
{
	public NctsPLOfficeCodeCollection(NctsHeader nctsHeader) : base(nctsHeader) { }

	public NctsPLOfficeCodeCollection(NctsDepartureMovementHeader departureHeader) : base(departureHeader) { }

	public new NctsPLOfficeCode this[int index] => (NctsPLOfficeCode)Elements[index];

	public new NctsPLOfficeCode AddNew() => (NctsPLOfficeCode)base.AddNew();

	public new NctsPLOfficeCode AddNew(ZString code) => (NctsPLOfficeCode)base.AddNew(code);

	protected new NctsPLOfficeCode AddNew(Type type) => (NctsPLOfficeCode)base.AddNew(type);

	protected override BusinessObject AddNewCore() => AddNew(typeof(NctsPLOfficeCode));
}
