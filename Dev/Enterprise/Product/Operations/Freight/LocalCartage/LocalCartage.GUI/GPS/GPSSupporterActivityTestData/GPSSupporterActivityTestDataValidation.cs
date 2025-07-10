using CargoWise.EntityFramework;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public class GPSSupporterActivityTestDataValidation : LocalCartageVehicleActivityValidation
	{
		public GPSSupporterActivityTestDataValidation(GPSSupporterActivityTestData parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateEN_ActivityType();
			ValidateClient();
		}

		GPSSupporterActivityTestData GPSActivityTestData
		{
			get { return (GPSSupporterActivityTestData)Parent; }
		}

		protected override void CheckEN_ActivityType()
		{
			base.CheckEN_ActivityType();
			ListValidation.ErrorIfInvalidCode(GPSActivityTestData.EN_ActivityTypeInfo);
		}

		public void ValidateClient()
		{
			ValidateCalculatedProperty(GPSActivityTestData.ClientPKInfo);
		}

		protected void CheckClientPK()
		{
			TypeValidation.CheckValidGuid(GPSActivityTestData.ClientPKInfo);
		}
	}
}
