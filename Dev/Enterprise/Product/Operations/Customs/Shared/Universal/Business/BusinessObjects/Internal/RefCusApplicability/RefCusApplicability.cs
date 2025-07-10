using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal.Internal
{
	internal sealed class RefCusApplicability : AutoRefCusApplicability
	{
		public RefCusApplicability(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
	}
}
