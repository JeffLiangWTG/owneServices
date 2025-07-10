using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentDeliveryCopyRecipient : AutoJobDocumentDeliveryCopyRecipient
	{
		public JobDocumentDeliveryCopyRecipient(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}