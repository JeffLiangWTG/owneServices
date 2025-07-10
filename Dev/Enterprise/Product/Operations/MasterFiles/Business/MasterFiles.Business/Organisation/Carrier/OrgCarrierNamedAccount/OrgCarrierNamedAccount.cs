using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.ONA_ForeignName)]
	[DescriptionProperty(Schema.ONA_ForeignName)]
	public class OrgCarrierNamedAccount : AutoOrgCarrierNamedAccount
	{
		public OrgCarrierNamedAccount(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.ForeignNames")]
		public override ZString ONA_ForeignName
		{
			get { return base.ONA_ForeignName; }
			set { base.ONA_ForeignName = value; }
		}

		[List("Lookups.ConsigneeOrConsignorOrControllingCustomerCollection")]
		public override ZGuid ONA_OH_Organization
		{
			get => base.ONA_OH_Organization;
			set => base.ONA_OH_Organization = value;
		}
	}
}
