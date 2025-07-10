using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.JobRequiredDocumentAddInfo)]
	public class JobRequiredDocumentAddInfoCollection : BusinessObjectCollection<JobRequiredDocumentAddInfo>
	{
		public JobRequiredDocumentAddInfoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobRequiredDocumentAddInfoCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
