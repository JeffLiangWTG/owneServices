using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Module
{
	public class CusInBondHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CusInBondHeaderController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var result = new USInBondForm((CusInBondHeader)businessEntity);
			result.SkipRecentItems = skipRecentItems;
			return result;
		}
		bool? skipRecentItems;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.InBond; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.InBond; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusInBondHeader); }
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.USInBondEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.USInBondView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.USInBondNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new InBondPlugIn((ICusInBondParent)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.US.InBond.Module.Res.GetData("PlugInTabPage|USInBond", "In-Bond"); }
		}

		public IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return base.ShowLoadedForm(sourceEntity, action);
		}
	}

	/// <summary>
	/// This controller is used when an In-Bond Movement is attached to a Shipment: to show the ShipmentForm from the In-Bond module
	/// </summary>
	public class CusInBondHeaderShipmentController : JobShipmentController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.InBondPluggedIntoShipment; }
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobShipment.ToString();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.InBond; }
		}

		public override IZForm ShowNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.JobShipment);
			IZForm newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var result = new ShipmentForm(businessEntity as ForwardingShipment);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.US.InBond;
			result.SkipRecentItems = skipRecentItems;
			return result;
		}
		bool? skipRecentItems;

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			IBusiness result = null;
			inBond = factory.Load<CusInBondHeader>(sourceEntityPK);
			if (inBond != null)
			{
				result = inBond.Parent;
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
			get { return typeof(CusInBondHeader); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.US.InBond.Module.Res.GetData("PlugInTabPage|USInBond", "In-Bond"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new InBondPlugIn((ICusInBondParent)businessEntity);
		}

		public IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return base.ShowLoadedForm(sourceEntity, action);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.USInBondEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.USInBondView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.USInBondNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion

		CusInBondHeader inBond;
	}
}
