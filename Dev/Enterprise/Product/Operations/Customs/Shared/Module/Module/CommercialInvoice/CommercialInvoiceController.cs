using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	/// <summary>
	/// Inherit from this and fill in GetForm
	/// </summary>
	public class CommercialInvoiceController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CommercialInvoiceController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CommercialInvoice; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CommercialInvoice; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BaseJobComInvoiceHeader); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CustomsCommercialInvoice; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsCommercialInvoice; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CustomsCommercialInvoice; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsCommercialInvoice; }
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			var invoiceHeader = sourceEntity as BaseJobComInvoiceHeader;
			var declaration = invoiceHeader != null && invoiceHeader.IsAttachedToPersistentDeclaration ? invoiceHeader.JobDeclaration : null;
			if (declaration == null)
			{
				return base.ShowLoadedForm(sourceEntity, action);
			}

			IZForm result;
			var shipment = declaration.Shipment;
			if (shipment != null)
			{
				var shipmentController = (JobDeclarationShipmentController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				shipmentController.ShowEditForm(shipment, true);
				AddToRecentItems(sourceEntity, invoiceHeader.HumanReadableShortcutName + " (" + shipment.JS_UniqueConsignRef + ")");
				result = shipmentController.LastShownForm;
				SelectInvoice(result, invoiceHeader);
			}
			else
			{
				var declarationController = (JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
				declarationController.ShowEditForm(declaration, true);
				AddToRecentItems(sourceEntity, invoiceHeader.HumanReadableShortcutName + " (" + declaration.JE_DeclarationReference + ")");
				result = declarationController.LastShownForm;
				SelectInvoice(result, invoiceHeader);
			}

			LastShownForm = result;

			return result;
		}

		void SelectInvoice(IZForm form, BaseJobComInvoiceHeader invoice)
		{
			var zForm = form as ZForm;
			var brokerageControl = zForm?.FindSingleOrDefault<BaseCustomsBrokerageUserControl>();
			if (brokerageControl == null)
			{
				return;
			}

			brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;

			var invoicesControl = brokerageControl.InvoicesTabPage?.FindSingleOrDefault<BaseCustomsSupplierHeaderUserControl>();
			var invoiceHeadersBoundGrid = invoicesControl?.InvoiceHeadersBoundGrid;
			if (invoice != null && invoiceHeadersBoundGrid != null && invoiceHeadersBoundGrid.List != null)
			{
				invoiceHeadersBoundGrid.SelectSingleElement(invoice);
			}
		}

		void AddToRecentItems(IBusiness sourceEntity, string description)
		{
			var favoriteProvider = CargoWise.Application.ObjectFactory.Get<Core.Modules.IFavoriteProvider>();
			favoriteProvider.AddToRecentItems(new ZArchitecture.Favorites.LinkWrapper(
				ModuleID.Name,
				sourceEntity.Identifier.ToGuid(),
				ZArchitecture.ShowEditFormUrlHandler.Instance.Create(ID, sourceEntity.Identifier),
				description
			));
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CommercialInvoiceForm((BaseJobComInvoiceHeader)businessEntity);
		}
	}
}
