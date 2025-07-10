using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	/// <summary>
	/// Module Controller for JobConsol.
	/// </summary>
	public class JobConsolController : TemplateRecordZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public JobConsolController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.JobConsol;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.JobConsol; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		public void SetNewBusinessObjectToReturn(IBusiness businessObject)
		{
			BusinessObjectToReturn = businessObject;
		}

		#region Implementation

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			ForwardingConsol consol = sourceEntity as ForwardingConsol;
			if (consol != null)
			{
				return consol.Validation.GetConsolTypeSecurityCheckpoint().IsAllowed ? base.ShowEditForm(sourceEntity) : base.ShowViewForm(sourceEntity);
			}
			return base.ShowEditForm(sourceEntity);
		}

		public IZForm ShowEditForm(BusinessObject sourceEntity, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return ShowEditForm(sourceEntity);
		}

		bool? skipRecentItems;

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			var consol = businessEntity as ForwardingConsol;
			if (consol != null)
			{
				consol.IsRoot = true;
			}

			var result = GetFormCore(consol);
			result.SkipRecentItems = skipRecentItems;
			return result;
		}

		protected virtual ConsolForm GetFormCore(ForwardingConsol consol)
		{
			return new ConsolForm(consol);
		}

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			var osmgSecurityCheckpoint = GetConsolRelatedShipmentsOSMGSecurityCheckpoint(bizObject);
			if (osmgSecurityCheckpoint != null && !osmgSecurityCheckpoint.IsAllowed)
			{
				return osmgSecurityCheckpoint;
			}

			return base.GetCheckPointForView(bizObject);
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			var osmgSecurityCheckpoint = GetConsolRelatedShipmentsOSMGSecurityCheckpoint(bizObject);
			if (osmgSecurityCheckpoint != null && !osmgSecurityCheckpoint.IsAllowed)
			{
				return osmgSecurityCheckpoint;
			}

			return base.GetCheckPointForEdit(bizObject);
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			var osmgSecurityCheckpoint = GetConsolRelatedShipmentsOSMGSecurityCheckpoint(bizObject);
			if (osmgSecurityCheckpoint != null && !osmgSecurityCheckpoint.IsAllowed)
			{
				return osmgSecurityCheckpoint;
			}

			return base.GetCheckPointForDelete(bizObject);
		}

		ConsolRelatedShipmentsOSMGSecurityCheckpoint GetConsolRelatedShipmentsOSMGSecurityCheckpoint(BusinessObject bizObject)
		{
			if (bizObject == null)
			{
				return null;
			}

			if (consolRelatedShipmentsOSMGSecurityCheckpoint == null || bizObject.PK != lastBizoGuid)
			{
				consolRelatedShipmentsOSMGSecurityCheckpoint = new ConsolRelatedShipmentsOSMGSecurityCheckpoint(bizObject);
				lastBizoGuid = bizObject.PK;
			}

			return consolRelatedShipmentsOSMGSecurityCheckpoint;
		}
		ConsolRelatedShipmentsOSMGSecurityCheckpoint consolRelatedShipmentsOSMGSecurityCheckpoint;
		ZGuid lastBizoGuid = ZGuid.Empty;

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MaintainConsol; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainConsolNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.MaintainConsolEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainConsolDelete; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (BusinessObjectToReturn != null)
			{
				IBusiness result = BusinessObjectToReturn;
				BusinessObjectToReturn = null;
				return result;
			}
			else
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			ChildEditableService.SetState(factory, ChildEditableServiceStates.Consol);
		}

		#endregion

		protected IBusiness BusinessObjectToReturn;

		#endregion
	}
}
