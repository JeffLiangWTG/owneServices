using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Rating
{
	public interface IRatingDocRollupOrSort
	{
		ZString Module { get; set; }
		ZPropertyInfo ModuleInfo { get; }
		ZString JobType { get; set; }
		ZPropertyInfo JobTypeInfo { get; }
		ZString TransportMode { get; set; }
		ZPropertyInfo TransportModeInfo { get; }
		ZString Display { get; set; }
		ZPropertyInfo DisplayInfo { get; }
		ZString Style { get; set; }
		ZPropertyInfo StyleInfo { get; }
		IBusinessObjectCollection ParentCollection { get; }
		BusinessObjectFactory Factory { get; }
		bool IsRegistry { get; }
	}
}
