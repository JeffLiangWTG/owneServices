using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateEntry : AutoTelPreDriveChecklistTemplateEntry
	{
		public TelPreDriveChecklistTemplateEntry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TelPreDriveChecklistTemplateHeader Header => Factory.Load<TelPreDriveChecklistTemplateHeader>(TTE_TTH_ChecklistTemplateHeader);

		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid TTE_TTH_ChecklistTemplateHeader
		{
			get => base.TTE_TTH_ChecklistTemplateHeader;
			set => base.TTE_TTH_ChecklistTemplateHeader = value;
		}
	}
}
