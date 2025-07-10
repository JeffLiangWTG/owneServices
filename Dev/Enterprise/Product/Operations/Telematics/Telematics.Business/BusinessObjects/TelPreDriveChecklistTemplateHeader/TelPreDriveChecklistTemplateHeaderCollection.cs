using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateHeaderCollection : ActiveBusinessObjectCollection<TelPreDriveChecklistTemplateHeader>
	{
		public TelPreDriveChecklistTemplateHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew => false;
	}
}
