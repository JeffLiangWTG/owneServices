using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public static class SailingBillImportHelper
	{
		public static void SynchroniseJobDocAddressIfNotEmpty(JobDocAddress destination, JobDocAddress source)
		{
			if (source != null && !source.IsEmpty)
			{
				var args = new BusinessObjectCloneArgs(new string[] { JobDocAddress.Schema.E2_ParentTableCode, JobDocAddress.Schema.E2_ParentID,
					JobDocAddress.Schema.E2_AddressSequence, JobDocAddress.Schema.E2_AddressType, JobDocAddress.Schema.E2_AddressOverride });
				destination.E2_AddressOverride = source.E2_AddressOverride;
				destination.CopyPersistentValuesFrom(source, args);
			}
		}
	}
}
