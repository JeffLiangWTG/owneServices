using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateEntryCollection : ActiveBusinessObjectCollection<TelPreDriveChecklistTemplateEntry>
	{
		public TelPreDriveChecklistTemplateEntryCollection(BusinessObjectFactory factory, TelPreDriveChecklistTemplateHeader header)
			: base(factory, header, null, TelPreDriveChecklistTemplateEntrySchema.TTE_TTH_ChecklistTemplateHeader)
		{
		}
	}
}
