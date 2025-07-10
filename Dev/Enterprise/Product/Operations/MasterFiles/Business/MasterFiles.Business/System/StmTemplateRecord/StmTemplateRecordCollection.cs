using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class StmTemplateRecordCollection : BusinessObjectCollection<StmTemplateRecord>
	{
		public StmTemplateRecordCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
