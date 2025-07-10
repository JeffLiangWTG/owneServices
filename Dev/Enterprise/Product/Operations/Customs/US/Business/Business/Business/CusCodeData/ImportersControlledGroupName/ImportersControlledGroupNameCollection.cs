using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class ImportersControlledGroupNameCollection : Customs.Business.CusCodeDataCollection<ImportersControlledGroupName>
	{
		public ImportersControlledGroupNameCollection(OrgCountryData master)
			: base(master, CusCodeDataTypeList.Codes.ImportersControlledGroupName)
		{
		}

		new OrgCountryData Master => (OrgCountryData)base.Master;

		protected override bool AllowNewCore => base.AllowNewCore && Master.OrgHeader.OH_IsConsignee;
	}
}
