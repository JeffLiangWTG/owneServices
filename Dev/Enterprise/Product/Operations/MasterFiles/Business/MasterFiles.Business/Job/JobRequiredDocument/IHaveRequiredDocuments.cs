using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IHaveRequiredDocuments : IBusiness
	{
		ZString UniqueConsignRef { get; }
		ZString HouseBill { get; }
		ZString MasterBill { get; }
		OrgHeader ExportBroker { get; }
		ZString TableCode { get; }
		ZGuid PK { get; }
		JobRequiredDocumentDependentCollection RequiredDocuments { get; }
		BusinessObject UltimateDocumentParent { get; }
		Logs Logs { get; }
		IReadOnlyList<ZString> AdditionalRefTypes { get; }

		void PreLogAllDocumentsReceivedEvents();
	}

	public interface IHaveRequiredDocumentsWithAttributes : IHaveRequiredDocuments
	{
	}
}
