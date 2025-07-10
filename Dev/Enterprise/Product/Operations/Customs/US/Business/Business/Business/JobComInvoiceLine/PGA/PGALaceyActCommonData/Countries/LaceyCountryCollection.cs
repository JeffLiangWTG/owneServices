using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class LaceyCountryCollection : DependentCusAddInfoCollection<LaceyCountry, PGA>
	{
		public LaceyCountryCollection(PGA master)
			: base(master, CusAddInfoTypeAttribute.Codes.USLaceyCountries)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}
	}
}
