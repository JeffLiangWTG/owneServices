using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff
{
	public class NZCUSCARController : Customs.Module.JobDeclarationController
	{
		public NZCUSCARController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.CUSCAR; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new DeclarationForm((JobDeclaration)businessEntity);
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugIn((ForwardingShipment)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			JobDeclaration result = Factory.New<JobDeclaration>();
			using (result.SuspendSettingHasChanges())
			{
				result.JE_MessageSubType = NZ.Business.JobMessageSubTypeList.Codes.WriteOff;
			}
			return result;
		}

		#region Security
		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NZCustomsECIWriteoff; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.NZCustomsECIWriteoffDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.NZCustomsECIWriteoffEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NZCustomsECIWriteoffNew; }
		}
		#endregion
	}

	public class NZCUSCARShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.CUSCARPluggedIntoShipment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.NZ.CUSCAR; }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var result = new ShipmentForm(businessEntity as ForwardingShipment);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.NZ.CUSCAR;
			return result;
		}
	}
}
