using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ISupplyTypeSelector : IJobConfigurationSelector
	{
		ZString Incoterm { get; set; }
		ZPropertyInfo IncotermInfo { get; }
		CodeDescriptionPairList IncotermList { get; }
		ZGuid LineDepartmentPK { get; set; }
		ZPropertyInfo LineDepartmentPKInfo { get; }
		ZString SupplyType { get; set; }
		ZPropertyInfo SupplyTypeInfo { get; }
		CodeDescriptionPairList SupplyTypeList { get; }
		SupplyTypeConfigurationLookups SupplyTypeConfigurationLookups { get; }

		void ValidateIncoterm();
		void ValidateLineDepartmentPK();
		void ValidateSupplyType();
	}
}
