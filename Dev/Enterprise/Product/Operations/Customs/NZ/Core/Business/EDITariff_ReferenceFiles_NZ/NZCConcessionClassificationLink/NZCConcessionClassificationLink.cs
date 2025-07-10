using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCConcessionClassificationLink : AutoNZCConcessionClassificationLink
	{
		public NZCConcessionClassificationLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
