using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(CusBrokerStaffRegistryItemUserControl))]
	public sealed class CusBrokerStaffRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new CusBrokerStaff(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CusBrokerStaffRegistryItemUserControl)control).FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "BrokerStaffCodeFindBox").ReadOnly;
	}
}
