using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module
{
	public class RateAttachmentSetController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RateAttachmentSetController()
		{
		}

		#region Standard Controller Overrides

		public override ControllerID ID
		{
			get { return ControllerIDs.RateAttachmentSet; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RateAttachmentSet); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RateAttachmentSetForm((RateAttachmentSet)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RateAttachmentSet; }
		}

		#endregion

		#region Delete Form

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.ShowError(sourceEntity.ReasonForNotAbleToDelete);
			}
			else
			{
				base.ShowDeleteForm(sourceEntity);
			}
			return LastShownForm;
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.QuotationDocumentsView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.QuotationDocumentsNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.QuotationDocumentsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.QuotationDocumentsDelete; }
		}

		#endregion
	}
}

