using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerDetentionController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AgencyContainerDetention; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyContainerDetention; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ContainerDetention); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ContainerDetentionForm((ContainerDetention)businessEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			ContainerDetention detention = (ContainerDetention)sourceEntity;

			if (detention == null || detention.NC_GC == GlbCompany.CurrentCompany.PK)
			{
				return base.ShowEditForm(sourceEntity);
			}
			else if (Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed)
			{
				return ShowViewForm(sourceEntity);
			}
			else
			{
				Globals.Message.Show(Res.GetString("44ca127c-8f6d-455d-9fd9-791924f1597d", "You are not authorized to view the detention jobs of other companies."));
				return null;
			}
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AgencyContainerDetention; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AgencyContainerDetentionNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AgencyContainerDetentionEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AgencyContainerDetentionDelete; }
		}

		#endregion
	}
}


