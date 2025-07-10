using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGACollection : DependentBusinessObjectCollection<CusUSLVItemPGA, CusUSLVItem>
	{
		public CusUSLVItemPGACollection(CusUSLVItem cusUSLVItem)
			: base(cusUSLVItem)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusUSLVItemPGASchema.ULP_ULI; }
		}
	}
}
