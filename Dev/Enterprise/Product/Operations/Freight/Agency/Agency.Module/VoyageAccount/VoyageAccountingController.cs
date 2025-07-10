using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class VoyageAccountingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyVoyageAccounting; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(VoyageAccount); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new VoyageAccountingForm((VoyageAccount)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyVoyageAccounting;
			}
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyVoyageAccount; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyVoyageAccountNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyVoyageAccountEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyVoyageAccountDelete; }
		}

		#endregion
	}
}


