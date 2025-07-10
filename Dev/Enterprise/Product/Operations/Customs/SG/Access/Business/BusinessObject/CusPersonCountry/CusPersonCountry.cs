using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.Business
{
	public class CusPersonCountry : ASYCUDA.Business.CusPersonCountry
		, Integration.Customs.ASYCUDA.SGAccess.ICusPersonCountry
	{
		public CusPersonCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusPerson ParentPerson => (CusPerson)base.ParentPerson;
	}
}
