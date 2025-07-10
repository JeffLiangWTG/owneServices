using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusAUNexdocECMCode : AutoRefCusAUNexdocECMCode
	{
		public RefCusAUNexdocECMCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
