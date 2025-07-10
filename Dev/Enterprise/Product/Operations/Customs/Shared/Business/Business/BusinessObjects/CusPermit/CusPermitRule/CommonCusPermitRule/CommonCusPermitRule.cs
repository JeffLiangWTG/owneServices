using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class CommonCusPermitRule : AutoCusPermitRule
	{
		protected CommonCusPermitRule(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
	}
}
