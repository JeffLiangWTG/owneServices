using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class NHTSADetailsCollection : DependentCusAddInfoCollection<NHTSADetails, USNHTSA>
	{
		public NHTSADetailsCollection(USNHTSA header)
			: base(header, CusAddInfoTypeAttribute.Codes.USNHTSADetails)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var header = Master as NHTSAHeader;
			if (header != null && header.IsMotorVehicles)
			{
				((NHTSADetails)child).US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			}
		}
	}
}
