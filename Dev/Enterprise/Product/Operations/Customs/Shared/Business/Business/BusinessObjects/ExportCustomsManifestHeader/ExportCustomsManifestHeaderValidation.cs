using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportCustomsManifestHeaderValidation
//
//    This class should be used for overriding validation in AutoExportCustomsManifestHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class ExportCustomsManifestHeaderValidation : AutoExportCustomsManifestHeaderValidation
	{
		public ExportCustomsManifestHeaderValidation(AutoExportCustomsManifestHeader parent) : base(parent)
		{
		}

		protected new ExportCustomsManifestHeader Parent => (ExportCustomsManifestHeader)base.Parent;

		protected virtual INotificationType ED_VesselNameNotificationSeverity => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override void CheckED_VesselName()
		{
			base.CheckED_VesselName();

			if (Parent.IsSea && !Parent.ED_VesselName.IsEmpty)
			{
				if (Parent.Vessel == null)   // This will / (can) occur once removing the unique constraint on Vessel RV_Code(Name) is implemented and duplicate vessel names can be created in the reference table.
				{
					if (Parent.VesselHasDuplicates)
					{
						Parent.ED_VesselNameInfo.AddNotification(
							ED_VesselNameNotificationSeverity,
							Res.GetString("C4485A2D-DDC4-49AE-A38B-F2407EDFCE79", "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel."));
					}
				}
				else
				{
					var lloydsValidation = new LloydsNumberValidation();
					lloydsValidation.Validate(Parent.Vessel.RV_LloydsNumber);
					if (!lloydsValidation.IsValid)
					{
						Parent.ED_VesselNameInfo.AddNotification(ED_VesselNameNotificationSeverity, lloydsValidation.ErrorText);
					}
				}
			}
		}
	}
}
