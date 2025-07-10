using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	HVLVReferenceValidationServiceTask.TaskCode,
	"HVLV Reference Validation",
	"FRT",
	typeof(HVLVReferenceValidationServiceTask),
	MinimumPeriod = "5minutes",
	CanRunInAnyBranch = true,
	IsMandatory = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]

[assembly: HostedServiceBusinessObjectBinding(
	HVLVReferenceValidationServiceTask.TaskCode,
	HVLVConsignmentSchema.Constants.TableName,
	new[] { HVLVConsignmentSchema.Constants.HVC_IsValidatedForUniqueness + "=N" }, "HVLV Consignment Uniqueness check")]
[assembly: HostedServiceBusinessObjectBinding(
	HVLVReferenceValidationServiceTask.TaskCode,
	HVLVItemSchema.Constants.TableName,
	new[] { HVLVItemSchema.Constants.HVI_IsValidatedForUniqueness + "=N" }, "HVLV Item Uniqueness check")]
namespace Enterprise.eTail.ServiceTasks
{
	public class HVLVReferenceValidationServiceTask : ServiceProviderImpl
	{
		public const string TaskCode = "HRV";

		public override void RunTask(CancellationToken cancellationToken)
		{
			if (AutoGenerateConsignmentAndItemIDsDisabled)
			{
				if (ConsignmentsOrItemsToValidateForUniqueness())
				{
					var branch = GlbBranch.GetOneActiveBranchPerCompany().FirstOrDefault();
					if (branch != null)
					{
						using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
						{
							new HVLVReferenceValidator(ServiceLogger, new HVLVConsignmentReferenceInfo()).Run(cancellationToken);
							new HVLVReferenceValidator(ServiceLogger, new HVLVItemReferenceInfo()).Run(cancellationToken);
						}
					}
				}
			}
		}

		#region Implementation

		bool AutoGenerateConsignmentAndItemIDsDisabled => !HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.Value;

		bool ConsignmentsOrItemsToValidateForUniqueness()
		{
			var factory = new BusinessObjectFactory();

			var itemQuery = new ZQuery(HVLVItemSchema.HVI_IsValidatedForUniqueness, 0);
			var anyItemsToBeValidatedForUniqueness = factory.Exists(typeof(HVLVItem), itemQuery);

			var consignmentQuery = new ZQuery(HVLVConsignmentSchema.HVC_IsValidatedForUniqueness, 0);
			var anyConsignmentsToBeValidatedForUniqueness = factory.Exists(typeof(HVLVConsignment), consignmentQuery);

			return anyItemsToBeValidatedForUniqueness || anyConsignmentsToBeValidatedForUniqueness;
		}

		#endregion
	}
}
