using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface IJobTypeConfigurationHelper : IJobConfigurationHelper
	{
		JobInvoicingConsumerType GetJobType();
	}

	class JobTypeConfigurationBizoImplementationHelper : IJobTypeConfigurationHelper
	{
		public JobTypeConfigurationBizoImplementationHelper(IJobTypeConfiguration parent)
		{
			Parent = parent;
		}
		IJobTypeConfiguration Parent { get; }

		CodeDescriptionPairList IJobConfigurationHelper.GetLookupList()
		{
			var jobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
			jobTypes.Insert(0, new AllJobsConsumerType());

			return jobTypes;
		}

		JobInvoicingConsumerType IJobTypeConfigurationHelper.GetJobType()
		{
			JobInvoicingConsumerType resultJobType = null;
			if (!Parent.JobTypeCode.IsEmpty)
			{
				resultJobType = ((IJobConfigurationHelper)this).GetLookupList()[Parent.JobTypeCode] as JobInvoicingConsumerType;
			}

			return resultJobType;
		}

		void IJobConfigurationHelper.Validate()
		{
			MandatoryValidation.CheckEntered(Parent.JobTypeCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JobTypeCodeInfo);
		}

		bool IJobConfigurationHelper.GetReadOnlyStatus() => false;
	}
}
