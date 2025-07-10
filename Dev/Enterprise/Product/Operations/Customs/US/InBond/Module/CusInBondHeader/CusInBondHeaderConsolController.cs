using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
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

namespace Enterprise.Customs.US.InBond.Module
{
	/// <summary>
	/// This controller is used when an In-Bond Movement is attached to a Consol: to show the ConsolForm from the In-Bond module
	/// </summary>
	public class CusInBondHeaderConsolController : JobConsolController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.InBondPluggedIntoConsol; }
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.JobConsol.ToString();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.InBond; }
		}

		public override IZForm ShowNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			IZForm newForm = controller.ShowNewForm();
			LastShownForm = controller.LastShownForm;
			return newForm;
		}

		protected override ConsolForm GetFormCore(ForwardingConsol consol)
		{
			var result = new ConsolForm(consol);
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
				CommonConsol consol = factory.Load<ForwardingConsol>(sourceEntityPK);
				if (consol != null)
				{
					result = consol;
				}
			}
			return result;
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.US.InBond.Module.Res.GetData("PlugInTabPage|USInBond", "In-Bond"); } }

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusInBondHeader); }
		}

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
