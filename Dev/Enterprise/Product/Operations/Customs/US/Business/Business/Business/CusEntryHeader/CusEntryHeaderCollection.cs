
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryHeaderCollection : CusEntryHeaderCollection<CusEntryHeader>
	{
		public CusEntryHeaderCollection(JobDeclaration parentBO, BusinessObjectFactory factory)
			: base(parentBO, factory)
		{
		}
	}
}
