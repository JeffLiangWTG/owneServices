using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PesticideLineCollection : DependentCusAddInfoCollection<PesticideLine, Pesticide>
	{
		public PesticideLineCollection(Pesticide master)
			: base(master, CusAddInfoTypeAttribute.Codes.USPesticideLine)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}
	}
}
