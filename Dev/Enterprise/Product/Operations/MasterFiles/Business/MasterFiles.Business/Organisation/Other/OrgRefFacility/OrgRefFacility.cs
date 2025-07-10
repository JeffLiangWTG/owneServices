using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRefFacility : AutoOrgRefFacility
	{
		public OrgRefFacility(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.ActiveAddresses")]
		public override ZGuid OFC_OA_PremisesAddress
		{
			get
			{
				return base.OFC_OA_PremisesAddress;
			}
			set
			{
				base.OFC_OA_PremisesAddress = value;
				ValidateAll();
			}
		}

		[List("Lookups.RefFacilities")]
		public override ZGuid OFC_RFT_Facility
		{
			get { return base.OFC_RFT_Facility; }
			set
			{
				base.OFC_RFT_Facility = value;
				ValidateAll();
			}
		}

		void ValidateAll()
		{
			if (!base.IsValidationSuspended)
			{
				Validation.ValidateAll();
			}
		}

		public RefFacility Facility => base.Factory.Load<RefFacility>(OFC_RFT_Facility);
	}
}
