using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusPerson), "Countries")]
	public class CusPersonCountry : AutoCusPersonCountry
	{
		public CusPersonCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
