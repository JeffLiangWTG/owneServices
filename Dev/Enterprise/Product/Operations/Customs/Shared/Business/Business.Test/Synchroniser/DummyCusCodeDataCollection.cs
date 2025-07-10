using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DummyCusCodeDataCollection : CusCodeDataCollection<DummyCusCodeData>
	{
		public DummyCusCodeDataCollection(BusinessObject parent)
			: base(parent, "XXX")
		{
		}

		public new DummyCusCodeData AddNew(ZString number)
		{
			return AddNew("XXX", number);
		}

		public bool ContainsNumber(string number)
		{
			return this.Cast<DummyCusCodeData>().Any(element => element.CY_Data == number);
		}
	}
}
