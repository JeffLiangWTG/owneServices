using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ISupportChangeOthersSecurity
	{
		IFindBoxListProvider CompleteGroupList { get; }
		IFindBoxListProvider CompleteStaffList { get; }
		BusinessObjectFactory Factory { get; }
		SchemaGuidColumn GlbSecurityRightHolderColumn { get; }
		ZGuid PK { get; }
		GlbSecurityChangeOthersView SecurityChangeOthersView { get; }
		GlbSecurityChangeOthersView GroupOwnersForGroupView { get; }
	}
}
