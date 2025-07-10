using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
	[CodeAlive("Will be used in later work items")]
	public class PkgPackageOrderReference : AutoPkgPackageOrderReference
	{
		public PkgPackageOrderReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Package

		public PkgPackage Package => Factory.Load<PkgPackage>(KPO_KP_Package);

		#endregion

		#region KPO_KP_Package

		[RelatedBusinessObject(nameof(Package))]
		public override ZGuid KPO_KP_Package
		{
			get => base.KPO_KP_Package;
			set => base.KPO_KP_Package = value;
		}

		#endregion
	}
}
