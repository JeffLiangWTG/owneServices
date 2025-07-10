using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IRelatableActivity : IBusiness, IAuditDetails
	{
		ZGuid PK { get; }
		ZString ActivityType { get; }
		string TablePrefix { get; }
		IOrgHeader Client { get; }
		ZBool ClientHasChanges { get; }
		IOrgContact Contact { get; }
		ZBool ContactHasChanges { get; }
		ZString Summary { get; }

		IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection { get; }
		IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection { get; }

		ZBool ShouldIgnoreSuperAndSubActivityRelationships { get; }
		void OnRelatedActivitySaving(IRelatableActivity relatedActivity);

		ZBool SupportViewRelatedCommunications { get; }
	}

	public interface ISuperRelatableActivity : IRelatableActivity
	{
		ISubRelatableActivity GetMatchingSubActivity(IRelatableActivity activity);
	}

	public interface ISubRelatableActivity : IRelatableActivity
	{
		ISuperRelatableActivity SuperActivity { get; }
	}
}
