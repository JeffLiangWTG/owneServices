using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	[UserDefinedValues]
	public class ForwardingPackage : PkgPackage
	{
		public ForwardingPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ForwardingPackageHandlingUnitHandlingUnitDivotCollection PackageHandlingUnitHandlingUnitDivots => (ForwardingPackageHandlingUnitHandlingUnitDivotCollection)base.PackageHandlingUnitHandlingUnitDivots;

		protected override PkgPackageHandlingUnitHandlingUnitDivotCollection GetPackageHandlingUnitHandlingUnitDivotCollection() => new ForwardingPackageHandlingUnitHandlingUnitDivotCollection(this);

		public new PackageJob.ForwardingPackageCollection Packages => (PackageJob.ForwardingPackageCollection)base.Packages;

		protected override PkgPackageCollection GetPackageCollectionCore() => new PackageJob.ForwardingPackageCollection(this);

		public new ForwardingPackageLookups Lookups => (ForwardingPackageLookups)base.Lookups;

		protected override PkgPackageLookups GetNewLookups() => new ForwardingPackageLookups(this);

		public override PkgPackageJob PackageJob => Factory.Load<ForwardingPackageJob>(KP_KJ_ParentPackageJob);

		public override ZString ScreeningMethod => this.GetUserDefinedValue<ZString>(nameof(ScreeningMethod));

		public override ZBool IsHighRisk => this.GetUserDefinedValue<ZBool>(nameof(IsHighRisk));

		public override ZString AdditionalScreeningMethod => this.GetUserDefinedValue<ZString>(nameof(AdditionalScreeningMethod));

		public void SetScreeningMethod(ZString screeningMethod)
		{
			this.SetUserDefinedValue(nameof(ScreeningMethod), screeningMethod);
		}

		public void SetIsHighRisk(ZBool isHighRisk)
		{
			this.SetUserDefinedValue(nameof(IsHighRisk), isHighRisk);
		}

		public void SetAdditionalScreeningMethod(ZString additionalScreeningMethod)
		{
			this.SetUserDefinedValue(nameof(AdditionalScreeningMethod), additionalScreeningMethod);
		}
	}
}
