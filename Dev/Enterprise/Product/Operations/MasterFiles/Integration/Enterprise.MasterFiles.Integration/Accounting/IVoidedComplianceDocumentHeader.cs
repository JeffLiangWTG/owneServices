using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration.Accounting
{
	public interface IVoidComplianceDocumentHeader
	{
		void CreateVoidedComplianceDocumentHeader(BusinessObjectFactory factory, ZGuid sequenceBook, ZString subType, ZString documentNumber);
	}
}
