using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVOuterPackage : IBusiness
	{
		ZGuid PK { get; }

		ZString HVO_ContainerNumber { get; set; }
		ZString HVO_F3_NKPackageType { get; set; }
		ZGuid HVO_HVL_LoadList { get; set; }
		ZGuid HVO_JK_LoadedOnConsol { get; set; }
		ZGuid HVO_OH_LastMileCarrier { get; set; }
		ZGuid HVO_OH_Owner { get; set; }
		ZString HVO_PackageBarcode { get; set; }
		ZString HVO_PackageReference { get; set; }
		ZString HVO_Status { get; set; }
		ZDateTime HVO_SystemCreateTimeUtc { get; set; }
		ZString HVO_SystemCreateUser { get; set; }
		ZDateTime HVO_SystemLastEditTimeUtc { get; set; }
		ZString HVO_SystemLastEditUser { get; set; }

		IOrgHeader LastMileCarrier { get; }
		IHVLVOriginLoadList LoadList { get; }
		IOrgHeader Owner { get; }
		IEnumerable<IHVLVItem> ActiveItems { get; }
		IHVLVItemCollection Items { get; }

		Logs Logs { get; }
		Notes Notes { get; }
	}
}
