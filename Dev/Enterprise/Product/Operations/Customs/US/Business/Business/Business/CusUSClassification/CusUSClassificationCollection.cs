using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class CusUSClassificationCollection : DependentBusinessObjectCollection<CusUSClassification, BusinessObject>
	{
		public CusUSClassificationCollection(BusinessObject parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusUSClassificationSchema.CD_ParentID; }
		}
	}
}
