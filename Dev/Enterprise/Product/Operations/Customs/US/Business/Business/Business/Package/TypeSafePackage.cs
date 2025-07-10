using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public abstract class TypeSafePackage : AutoPackage
	{
		protected TypeSafePackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		Package package
		{
			get { return (Package)this; }
		}

		public new Bill Bill
		{
			get { return PackingGroup != null ? PackingGroup.Bill : null; }
		}

		public new PackingGroup PackingGroup
		{
			get { return (PackingGroup)base.PackingGroup; }
		}

		public new PackageValidation Validation
		{
			get { return (PackageValidation)base.Validation; }
		}

		public new PackageLookups Lookups
		{
			get { return (PackageLookups)base.Lookups; }
		}

		protected override Customs.Business.CusDecHouseContainerPackValidation GetNewValidation()
		{
			return new PackageValidation(package);
		}

		protected override Customs.Business.CusDecHouseContainerPackLookups GetNewLookups()
		{
			return new PackageLookups(package);
		}

		protected override ZString FreightPackageTypeCore
		{
			get { return ShippingOrPackingingUnitList.ToFreightPackageType(CW_PackType); }
		}
	}
}
