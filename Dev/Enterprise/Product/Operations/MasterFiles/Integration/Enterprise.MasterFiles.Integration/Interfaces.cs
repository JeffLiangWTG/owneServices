using CargoWise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	// If you add members to these types, create a new file for them.
	public interface ICreditControlledBusinessObject { }
	public interface IJobHeaderStatusList { }
	public interface ILocationCollection { }
	public interface IShipsAgencyPrincipalCollection { }
	public interface IJobInvoicingConsumerTypes { }
	public interface IJobService { }
	public interface IFreightServiceTypes { }
	public interface IProcessTaskCollection { }
	public interface ITemplateProcessTask { }
	public interface IOpportunityProcessTasks { }
	public interface IDummyProcessTask { }
	public interface IUnmatchOrgDetailRecords { }
	public interface IZOrganisationGridFindBox { }
	public interface ILocalTransportCompanyCollection { }

	public interface IMasterFilesListProvider
	{
		ICodeDescriptionPairList PackingSlipOrderByList();
		ICodeDescriptionPairList FailureReasons();
	}

	// Don't ADD any more empty public interfaces
	// Please group them into functional units as above
	// All existing empty interfaces should be migrated into this pattern
}
