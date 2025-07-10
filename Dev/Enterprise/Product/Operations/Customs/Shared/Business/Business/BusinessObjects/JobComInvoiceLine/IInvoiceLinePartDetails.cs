using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface IInvoiceLinePartDetails
	{
		ZString CustomsCountryCode { get; }
		ZGuid PartSyncManagerActiveDeciderPK { get; }
		OrgHeader Importer { get; }
		OrgHeader Supplier { get; }
		BaseJobComInvoiceHeader Header { get; }
		bool IsForImportSectionOfDrawback { get; }
		bool IsForExportSectionOfDrawback { get; }
		bool IsDrawback { get; }
		bool IsDeleted { get; }
		bool Enabled { get; }
		ZGuid PartPK { get; set; }
		ZString PartNo { get; }
		RefCountry InvoiceCountry { get; }
		bool JustUpdatedByDataRefresh { get; }
		Type TypeOfPartUsed { get; }
		BusinessObjectFactory Factory { get; }
		void UpdateDetailsOnPartChange();
	}
}
