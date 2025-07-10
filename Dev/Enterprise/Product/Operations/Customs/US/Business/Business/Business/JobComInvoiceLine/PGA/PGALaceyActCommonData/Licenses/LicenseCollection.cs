using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class LicenseCollection : DependentCusAddInfoCollection<License, PGA>
	{
		public LicenseCollection(PGA master)
			: base(master, CusAddInfoTypeAttribute.Codes.USLaceyActLicense)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}
	}
}
