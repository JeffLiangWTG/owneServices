using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeComplianceDescriptionCollection : ActiveBusinessObjectCollection<AccChargeComplianceDescription>
	{
		public AccChargeComplianceDescriptionCollection(AccChargeCode parent) : base(parent)
		{
		}
	}
}
