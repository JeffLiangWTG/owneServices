using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusVATApplicability : AutoRefCusVATApplicability
	{
		public RefCusVATApplicability(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
