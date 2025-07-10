using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobHeaderParent : IJobHeaderParentCore, IBillingPlugin
	{
		void SetJobNumberFieldOnSaving();
		void OnJobCreating(JobHeader job);
		void OnJobCreated(JobHeader job);
		void OnJobDeleting(JobHeader job);
		void OnJobDeleted(JobHeader job);
		bool AllowInvoiceDeletion { get; }
		bool IsDeleted { get; }
	}

	public static class IJobHeaderParentExtensions
	{
		public static string TablePrefix(this IJobHeaderParent jobHeaderParent)
		{
			Argument.NotNull(jobHeaderParent, nameof(jobHeaderParent));
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(jobHeaderParent.TableName);
		}
	}
}
