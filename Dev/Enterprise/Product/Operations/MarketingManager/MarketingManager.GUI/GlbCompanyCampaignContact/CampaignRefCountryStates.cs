using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	[DescriptionProperty(RefCountryStates.Schema.RW_Description, CanBeReferencedBy = true)]
	public class CampaignRefCountryStates : RefCountryStates
	{
		public CampaignRefCountryStates(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}
	}
}
