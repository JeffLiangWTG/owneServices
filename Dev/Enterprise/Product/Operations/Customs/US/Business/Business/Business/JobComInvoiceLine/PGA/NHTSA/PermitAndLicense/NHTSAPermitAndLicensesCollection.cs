using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSAPermitAndLicensesCollection : DependentCusAddInfoCollection<NHTSAPermitAndLicenses, NHTSADetails>
	{
		public NHTSAPermitAndLicensesCollection(NHTSADetails details)
			: base(details, CusAddInfoTypeAttribute.Codes.USNHTSAPermitAndLicense)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}
	}
}
