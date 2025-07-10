using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class UNDGDataItemValidation : MasterFiles.Business.UNDGDataItemValidation
	{
		public UNDGDataItemValidation(UNDGDataItem parent)
			: base(parent)
		{
		}

		public new UNDGDataItem Parent
		{
			get { return (UNDGDataItem)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				if (Parent.HasSailingLinkage && ((ISailingSynchronisationTarget<MasterFiles.Business.UNDGDataItem>)Parent).Source == null)
				{
					Parent.AddRowWarning(ValidationConstants.SailingSynchronisation.HazardousDetailMightBeIncorectlyAdded);
				}
			}
		}
	}
}
