using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DpsResultStatus
	{
		public DpsResultStatus(BusinessObject screeningEntity, string status)
		{
			ScreeningEntity = screeningEntity;
			Status = status;
		}

		public BusinessObject ScreeningEntity { get; }

		public string Status { get; set; }
	}
}
