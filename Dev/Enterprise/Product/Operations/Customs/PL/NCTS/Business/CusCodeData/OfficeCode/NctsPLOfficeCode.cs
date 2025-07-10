using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsPLOfficeCode(BusinessObjectFactory factory, DataRow row) : NctsEuOfficeCode(factory, row), Integration.Customs.PL.INctsEuOfficeCode
{
	protected override CusCodeDataValidation GetNewPhase5Validation() => MovementHeader != null ? new NctsPLOfficeCodePhase5DepartureValidation(this) : new NctsEuOfficeCodePhase5Validation(this);
}
