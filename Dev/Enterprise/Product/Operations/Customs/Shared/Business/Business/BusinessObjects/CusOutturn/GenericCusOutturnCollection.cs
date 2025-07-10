using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class GenericCusOutturnCollection<T, MasterT> : DependentBusinessObjectCollection<T, MasterT> where T : CusOutturn where MasterT : BusinessObject, IOutturnableLine
	{
		protected GenericCusOutturnCollection(MasterT outturnableLine) : base(outturnableLine)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusOutturnSchema.C5_ParentID; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);

			T outturn = (T)dependent;
			outturn.Parent = Master;
		}

		protected new MasterT Master
		{
			get { return base.Master; }
		}
	}
}
