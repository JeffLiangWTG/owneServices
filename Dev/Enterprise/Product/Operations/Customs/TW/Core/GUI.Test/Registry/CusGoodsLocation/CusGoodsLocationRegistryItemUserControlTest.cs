using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationRegistryItemUserControl))]
	public sealed class CusGoodsLocationRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new CusGoodsLocationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CusGoodsLocationRegistryItemUserControl)control).FindSingleOrDefault<ZGrid>(x => x.Name == "MainGrid").ReadOnly;
	}
}
