using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for StmTemplate.
	/// </summary>
	public class StmTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public StmTemplateController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentTemplate; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmTemplate); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ZChildForm(businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.QuotationDocumentsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.QuotationDocumentsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.QuotationDocumentsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.QuotationDocumentsView; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
