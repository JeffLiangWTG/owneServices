using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Packing.Business
{
	[SystemDefinedValues]
	public class PkgPackageSeal : AutoPkgPackageSeal
	{
		public PkgPackageSeal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Package

		public PkgPackage Package => Factory.Load<PkgPackage>(KPE_KP_Package);

		#endregion

		#region KPE_KP_Package

		[RelatedBusinessObject(nameof(Package))]
		public override ZGuid KPE_KP_Package
		{
			get => base.KPE_KP_Package;
			set => base.KPE_KP_Package = value;
		}

		#endregion
	}
}
