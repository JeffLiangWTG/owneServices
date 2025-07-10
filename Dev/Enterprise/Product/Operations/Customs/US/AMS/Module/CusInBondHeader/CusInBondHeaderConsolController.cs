using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Customs.US.AMS.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
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

namespace Enterprise.Customs.US.AMS.Module
{
	/// <summary>
	/// This controller is used when a AMS is attached to consol to show ConsolForm from AMS module
	/// </summary>
	class CusInBondHeaderConsolController : JobConsolController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.AMSPluggedIntoConsol; }
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.AMS; }
		}

		public override IZForm ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			var newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		protected override ConsolForm GetFormCore(ForwardingConsol businessEntity)
		{
			var result = new ConsolForm(businessEntity);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.US.AMS;
			return result;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			IBusiness result = null;
			var aMS = factory.Load<CusInBondHeader>(sourceEntityPK);
			if (aMS != null)
			{
				result = aMS.Consol;
			}
			else
			{
				var consol = factory.Load<ForwardingConsol>(sourceEntityPK);
				if (consol != null)
				{
					result = consol;
				}
			}
			return result;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|USAMS", "AMS"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new USAMSPlugIn((ForwardingConsol)businessEntity);
		}

		#region Security

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject) => CombineCheckpoint(bizObject, base.GetCheckPointForEdit);

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject) => CombineCheckpoint(bizObject, base.GetCheckPointForView);

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject) => CombineCheckpoint(bizObject, base.GetCheckPointForNew);

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject) => CombineCheckpoint(bizObject, base.GetCheckPointForDelete);

		static SecurityCheckpoint CombineCheckpoint(BusinessObject bizObject, Func<BusinessObject, SecurityCheckpoint> getBaseCheckPoint)
		{
			SecurityCheckpoint checkPoint;
			if (bizObject is CusInBondHeader header)
			{
				checkPoint = Env.Security.ConsolAMSReporting;
				if (checkPoint.IsAllowed && header.Consol is ForwardingConsol consol)
				{
					checkPoint = getBaseCheckPoint(consol);
				}
			}
			else
			{
				checkPoint = getBaseCheckPoint(bizObject);
			}

			return checkPoint;
		}

		#endregion
	}
}
