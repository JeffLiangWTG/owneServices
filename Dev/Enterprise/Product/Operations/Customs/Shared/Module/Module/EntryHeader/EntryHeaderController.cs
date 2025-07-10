using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	/// <summary>
	/// Inherit from this and fill in GetForm
	/// </summary>
	public class EntryHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EntryHeader; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.EntryHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusEntryHeader); }
		}

		protected sealed override SecurityCheckpoint CheckPointForDelete
		{
			get { return null; }
		}

		protected sealed override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CustomsDeclarationEnquiryEdit; }
		}

		protected sealed override SecurityCheckpoint CheckPointForNew
		{
			get { return null; }
		}

		protected sealed override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CustomsDeclarationEnquiry; }
		}

		protected BaseJobDeclaration GetDeclaration(IBusiness bizObj)
		{
			var declaration = bizObj as BaseJobDeclaration;
			if (declaration == null)
			{
				var entry = bizObj as CusEntryHeader;
				declaration = entry?.Declaration;
			}
			return declaration;
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			IZForm result;
			var declaration = GetDeclaration(sourceEntity);
			if (declaration == null)
			{
				throw new ModuleGuiNotSupportedException("No support for CusEntryHeader not attached to declaration");
			}
			else
			{
				var shipment = declaration.Shipment;
				var entry = sourceEntity as CusEntryHeader;
				var favoriteProvider = CargoWise.Application.ObjectFactory.Get<Core.Modules.IFavoriteProvider>();
				if (shipment != null)
				{
					var shipmentController = (JobDeclarationShipmentController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
					shipmentController.ShowEditForm(shipment, true);

					favoriteProvider.AddToRecentItems(new ZArchitecture.Favorites.LinkWrapper(ModuleID.Name, entry.PK.ToGuid(), ZArchitecture.ShowEditFormUrlHandler.Instance.Create(ID, entry.PK), shipment.JS_UniqueConsignRef + " - " + entry.CH_BGMReference));

					result = shipmentController.LastShownForm;
				}
				else
				{
					var declarationController = (JobDeclarationController)ZControllerFactory.Create(JobDeclarationControllerID);
					declarationController.ShowEditForm(declaration, true);

					favoriteProvider.AddToRecentItems(new ZArchitecture.Favorites.LinkWrapper(ModuleID.Name, entry.PK.ToGuid(), ZArchitecture.ShowEditFormUrlHandler.Instance.Create(ID, entry.PK), declaration.JE_DeclarationReference + " - " + entry.CH_BGMReference));

					result = declarationController.LastShownForm;
				}
			}

			LastShownForm = result;
			OnFormShown(result, null, sourceEntity);
			return result;
		}

		protected virtual ControllerID JobDeclarationControllerID => ControllerIDs.Customs.JobDeclaration;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotImplementedException(); // All forms shown must be proxied via JobDeclarationController or JobDeclarationShipmentController
		}

		protected virtual void OnFormShown(object sender, EventArgs e, IBusiness topLevelObject)
		{
			if (topLevelObject is CusEntryHeader)
			{
				var form = (ZForm)sender;

				var customsBrokerageUserControl = form?.FindSingleOrDefault<BaseCustomsBrokerageUserControl>();
				if (customsBrokerageUserControl != null)
				{
					if (form is Freight.Forwarding.GUI.ShipmentForm)
					{
						form.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.JobDeclaration);
					}

					customsBrokerageUserControl.Focus();
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.MessagesTabPage;

					FindEntriesBoundGrid(customsBrokerageUserControl)?.SelectSingleElementByPK(topLevelObject.Identifier);
				}
			}
		}

		protected virtual ZGrid FindEntriesBoundGrid(BaseCustomsBrokerageUserControl customsBrokerageUserControl)
		{
			return customsBrokerageUserControl.FindSingleOrDefault<ImportMessageUserControl>()?.EntriesBoundGrid
					?? customsBrokerageUserControl.FindSingleOrDefault<EntriesAndEntryLinesUserControl>()?.EntriesBoundGrid;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (CheckDeclarationBelongsToThisCompany(sourceEntity))
			{
				result = base.ShowEditForm(sourceEntity);
			}
			return result;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			IZForm result = null;
			if (CheckDeclarationBelongsToThisCompany(sourceEntity))
			{
				result = base.ShowViewForm(sourceEntity);
			}
			return result;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ControllerShowDeleteFormNotSupportedException("Does not support Delete functionality");
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Does not support New functionality");
		}

		bool CheckDeclarationBelongsToThisCompany(BusinessObject sourceEntity)
		{
			bool result = true;
			var declaration = GetDeclaration(sourceEntity);
			var company = declaration?.Company;
			if (company != null && company.PK != GlbCompany.CurrentCompany.PK)
			{
				Globals.Message.ShowError(Res.GetString("{7855CCA6-D9A1-4281-ABF3-46F536477921}", "You are trying to view a declaration that belongs to a different company. Please log into the company '{0}' and try again.", company.GC_Name));
				result = false;
			}

			return result;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return Factory.New<BaseJobDeclaration>();
		}
	}
}
