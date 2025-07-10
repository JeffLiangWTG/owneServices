using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Module
{
	public class ISFController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.ImporterSecurityFiling; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ImporterSecurityFiling; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusISFHeader); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			if (ArgsForNewForm != null)
			{
				var shipmentNumber = ArgsForNewForm.FirstOrDefault();
				if (!string.IsNullOrEmpty(shipmentNumber))
				{
					var shipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, shipmentNumber));
					if (shipment != null)
					{
						using (shipment.SuspendDeclarationForDocuments())
						{
							var creator = ISFFromShipmentCreator.GetCreatorForShipment(shipment);
							return creator.Create(Factory);
						}
					}
				}
			}

			return base.GetNewBusinessEntityInLocalFactory();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ISFForm((CusISFHeader)businessEntity);
		}

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ImporterSecurityFilingView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ImporterSecurityFilingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImporterSecurityFilingNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ImporterSecurityFilingDelete; }
		}

		#endregion
	}
}
