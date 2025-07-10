using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public interface IStatementLineDeclaration : ICustomsJobInfo, IControllerIDProvider
	{
		ZString JE_DeclarationReference { get; }
		ZString FormattedEntryNumber { get; }
		ZString EntryNumber { get; }
		ZString ProcessingDistrictPort { get; }
		ZString LocalCurrencyCode { get; }
		bool HasEntryBeenWithdrawn { get; }
		bool IsRemoteLocationFiling { get; }
		bool IsACE { get; }
		ZString US_PreparerDistrictPort { get; }
		ZString US_PreparerOfficeCode { get; }
		ZString BrokerReferenceNumber { get; }
		ZString EntryFilerCode { get; }
		EDIMessageCollection Messages { get; }
		OrgHeader IOR { get; }

		ZDateTime US_PSDAccepted { get; set; }
		ZString US_PaymentType { get; set; }
		ZString US_ClientBranchDesignation { get; set; }
		ZDateTime US_PaymentDate { get; set; }
		ZString US_PaperlessEntry { get; set; }
		ZString US_PeriodicStatementMM { get; set; }
		ZDateTime US_PreliminaryStatementPrintDate { get; set; }

		ZBool US_ConsolACE { get; set; }

		ZString ReleaseStatus { get; }
		ZString ReleaseStatusDescription { get; }

		Guid RegistryCompanyPK { get; }
	}
}
