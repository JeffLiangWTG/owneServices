using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class PackingGroup : AutoPackingGroup
		, Integration.Customs.US.IPackingGroup
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
				if (hasChanged && Declaration != null)
				{
					Packages.MarkAsNeedingValidation();
				}
			}
		}

		public new CusContainer Container
		{
			get { return (CusContainer)base.Container; }
		}

		protected override bool ShouldCopyDeclarationTotalNoOfPacksCore
		{
			get { return true; }
		}
	}
}
