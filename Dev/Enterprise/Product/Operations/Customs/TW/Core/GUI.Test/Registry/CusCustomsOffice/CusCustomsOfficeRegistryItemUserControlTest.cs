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
	[TestedType(typeof(CusCustomsOfficeRegistryItemUserControl))]
	public sealed class CusCustomsOfficeRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new CusCustomsOffice(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CusCustomsOfficeRegistryItemUserControl)control).FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "CustomsOfficeCodeFindBox").ReadOnly;
	}
}
