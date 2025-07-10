using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackingGroup : BasePackingGroup, Integration.Customs.NZ.IPackingGroup
	{
		public PackingGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid CR_CO_Container
		{
			get { return base.CR_CO_Container; }
			set
			{
				bool hasChanged = base.CR_CO_Container != value;
				base.CR_CO_Container = value;
				if (hasChanged)
				{
					Declaration.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		#region Related Business Objects

		public new PackageCollection Packages
		{
			get { return (PackageCollection)base.Packages; }
		}

		protected override BasePackageCollection CreateNewPackageCollection()
		{
			return new PackageCollection(this);
		}

		#endregion

		protected override CusDecHouseContainerPivotValidation GetNewValidation()
		{
			return new PackingGroupValidation(this);
		}

		public new CusContainer Container
		{
			get { return (CusContainer)base.Container; }
		}
	}
}
