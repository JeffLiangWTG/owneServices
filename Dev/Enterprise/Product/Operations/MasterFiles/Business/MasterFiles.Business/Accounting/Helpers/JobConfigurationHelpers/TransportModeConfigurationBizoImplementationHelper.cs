using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers
{
	public interface ITransportModeConfigurationHelper : IJobConfigurationHelper
	{
		void JobTypeSetterLogic();
	}

	class TransportModeConfigurationBizoImplementationHelper : ITransportModeConfigurationHelper
	{
		public TransportModeConfigurationBizoImplementationHelper(ITransportModeConfiguration parent)
		{
			Parent = parent;
		}
		ITransportModeConfiguration Parent { get; }

		CodeDescriptionPairList IJobConfigurationHelper.GetLookupList()
		{
			var transportModes = new CodeDescriptionPairList(OLookUpEditType.TransportType);
			transportModes.Insert(0, new CodeDescriptionPair(All, Res.GetString("52EBBFD9-C4F8-4BBF-B854-E80FD71507F4", "Any Transport Mode")));
			return transportModes;
		}

		void IJobConfigurationHelper.Validate()
		{
			if (!((IJobConfigurationHelper)this).GetReadOnlyStatus())
			{
				MandatoryValidation.CheckEntered(Parent.TransportModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.TransportModeInfo);
			}
		}

		bool IJobConfigurationHelper.GetReadOnlyStatus()
		{
			var jobType = JobType();
			return jobType != null && !jobType.IsTransportModeSupported;
		}

		void ITransportModeConfigurationHelper.JobTypeSetterLogic()
		{
			if (((IJobConfigurationHelper)this).GetReadOnlyStatus())
			{
				Parent.TransportModeInfo.Value = ZString.Empty;
			}
		}

		const string All = "ALL";

		JobInvoicingConsumerType JobType() => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetJobTypeHelper(Parent).GetJobType();
	}
}
