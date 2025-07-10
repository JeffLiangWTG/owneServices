#define CODE_ANALYSIS
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public void ShowNewExWarehouseForm()
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetNewBusinessEntityInLocalFactory();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			ShowFormForNewEntity(declaration);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.JobDeclaration; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BaseJobDeclaration); }
		}

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			var result = GetFormCore(businessEntity);

			var jobDeclarationForm = result as BaseJobDeclarationForm;
			if (jobDeclarationForm != null && skipRecentItems != null)
			{
				jobDeclarationForm.SkipRecentItems = skipRecentItems;
			}

			return result;
		}

		protected virtual IZForm GetFormCore(IBusiness topLevelObject)
		{
			return new BaseJobDeclarationForm((BaseJobDeclaration)topLevelObject);
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

		bool? skipRecentItems;

		public IZForm ShowEditForm(BusinessObject sourceEntity, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return base.ShowEditForm(sourceEntity);
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

		bool CheckDeclarationBelongsToThisCompany(BusinessObject sourceEntity)
		{
			bool result = true;
			var declaration = sourceEntity as BaseJobDeclaration;

			if (declaration != null && declaration.Company != null && declaration.Company.PK != GlbCompany.CurrentCompany.PK)
			{
				Globals.Message.ShowError(Res.GetString("d62dcf76-26f6-4564-accb-cebbf3616a09", "You are trying to view a declaration that belongs to a different company. Please log into the company '{0}' and try again.", declaration.Company.GC_Name));
				result = false;
			}

			return result;
		}

		protected override sealed IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			BaseJobDeclaration result = null;

			ImportJobDeclaration decToImport = sourceEntity as ImportJobDeclaration;
			if (decToImport != null)
			{
				result = decToImport.CreateNewStandAloneDeclaration(Factory);
			}
			else
			{
				result = (BaseJobDeclaration)base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}
			return result;
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.Module.Res.GetData("PlugInTabPage|JobDeclaration", "Brokerage"); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugInOneToOne((ForwardingShipment)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsDeclarationEnquiry;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsDeclarationEnquiryEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsDeclarationEnquiryNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsDeclarationEnquiryDelete;

		#region CRM Security

		protected virtual JobDeclarationCRMSecurityProvider CRMSecurityProvider => new JobDeclarationCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as BaseJobDeclaration, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as BaseJobDeclaration, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as BaseJobDeclaration, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}

	/// <summary>
	/// This controller is used when a declaration is attached to shipment to show ShipmentForm from Declaration module
	/// </summary>
	public class JobDeclarationShipmentController : JobShipmentController, IPluginControllerBusinessObjectProvider
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.JobDeclarationPluggedIntoShipment; }
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobShipment.ToString();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.JobDeclaration; }
		}

		public override ModuleIdentifier ModuleIDForDocumentSecurity
		{
			get
			{
				return ModuleIDs.JobShipment;
			}
		}

		public override IZForm ShowNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
			IZForm newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		bool? skipRecentItems;

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var result = new ShipmentForm(businessEntity as ForwardingShipment);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.JobDeclaration;
			result.SkipRecentItems = skipRecentItems;
			return result;
		}

		public IZForm ShowEditForm(BusinessObject sourceEntity, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return base.ShowEditForm(sourceEntity);
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			CommonShipment result = (CommonShipment)LoadBusinessEntity(Factory, sourceEntity.Identifier);
			if (result == null)
			{
				DeclarationFromShipmentPuller puller = sourceEntity as DeclarationFromShipmentPuller;
				if (puller != null)
				{
					BaseJobDeclaration createdDeclaration = puller.CreateOrLoadDeclarationForShipment(Factory);
					result = createdDeclaration != null ? createdDeclaration.Shipment : null;
				}
			}
			return result;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			IBusiness result = null;
			BaseJobDeclaration declaration = factory.Load<BaseJobDeclaration>(sourceEntityPK);
			if (declaration != null)
			{
				result = declaration.Shipment;
			}
			else
			{
				CommonShipment shipment = factory.Load<ForwardingShipment>(sourceEntityPK);
				if (shipment != null)
				{
					result = shipment;
				}
			}
			return result;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(BaseJobDeclaration); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugInOneToOne((ForwardingShipment)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsDeclarationEnquiry;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsDeclarationEnquiryEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsDeclarationEnquiryNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsDeclarationEnquiryDelete;

		public override ZGuid LastSavedPK
		{
			get
			{
				ZForm form = LastShownForm as ZForm;
				IIdentified businessObject = form == null ? null : form.DataSource as IIdentified;
				if (businessObject != null)
				{
					ForwardingShipment shipment = businessObject as ForwardingShipment;
					if (shipment != null)
					{
						BusinessObject declaration = shipment.GetDeclaration();
						if (declaration != null)
						{
							return declaration.PK;
						}
					}
				}

				return base.LastSavedPK;
			}
		}

		#region IPluginControllerBusinessObjectProvider Members

		IBusiness IPluginControllerBusinessObjectProvider.LoadBusinessEntityForPlugIn(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var result = (IBusiness)factory.Load<BaseJobDeclaration>(sourceEntityPK);
			if (result == null && factory.Load<ForwardingShipment>(sourceEntityPK) is ForwardingShipment shipment
				&& shipment.GetDeclaration() is IBusiness declaration)
			{
				result = declaration;
			}

			return result;
		}

		#endregion

		#region CRM Security

		BusinessObject GetBusinessObjectForSecurityCheckPoint(BusinessObject bizObject)
		{
			return bizObject is BaseJobDeclaration declaration && declaration.Shipment is ForwardingShipment shipment
				? shipment
				: bizObject;
		}

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return base.GetCheckPointForView(GetBusinessObjectForSecurityCheckPoint(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return base.GetCheckPointForEdit(GetBusinessObjectForSecurityCheckPoint(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return base.GetCheckPointForDelete(GetBusinessObjectForSecurityCheckPoint(bizObject));
		}

		#endregion
	}
}
