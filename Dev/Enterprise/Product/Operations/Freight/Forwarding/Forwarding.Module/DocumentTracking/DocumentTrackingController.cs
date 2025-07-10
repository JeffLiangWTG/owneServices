using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	/// <summary>
	/// Module Controller for DocumentTracking.
	/// </summary>
	public class DocumentTrackingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public DocumentTrackingController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocumentTracking; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DocumentTracking; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobRequiredDocument); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ForwardingShipment shipment = GetShipmentFromDoc((JobRequiredDocument)businessEntity);
			return new ShipmentForm(shipment);
		}

		public override IZForm ShowNewForm()
		{
			throw new ModuleGuiNotSupportedException("Document Tracking does not allow creation of new records");
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ForwardingDocumentTracking; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ForwardingDocumentTracking; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ForwardingDocumentTracking; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ForwardingDocumentTracking; }
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			ZForm form = (ZForm)base.ShowLoadedForm(sourceEntity, action);
			if (form != null)
			{
				form.DisableNewAction();
			}

			return form;
		}

		#region Implementation

		ForwardingShipment GetShipmentFromDoc(JobRequiredDocument doc)
		{
			doc.ParentType = typeof(ForwardingDocsAndCartage);
			IHaveRequiredDocuments docParent = doc.Parent;
			ChildEditableService.SetState(docParent.Factory, ChildEditableServiceStates.Shipment);
			return Factory.Load<ForwardingShipment>(docParent.UltimateDocumentParent.PK);
		}

		#endregion

	}
}
