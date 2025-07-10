using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefPremisesGateCodeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefPremisesGateCode; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PremisesGateCode; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefPremisesGateCode);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefPremisesGateCodeFilterControl(GridCollection, (RefPremisesGateCodeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefPremisesGateCodeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefPremisesGateCodeFilterBusinessObject();
		}

		#region Menu / Toolbar

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(FindOrganisationMenuItem);
			return result.ToArray();
		}

		MenuItem FindOrganisationMenuItem
		{
			get
			{
				return new ZMenuItem(ResString.GetMultilingualString("MasterFiles.RefPremisesGateCode.FindOrganization", "&Find/Create Organization"),
					new EventHandler(FindOrganisationHandler));
			}
		}

		#endregion

		#region FindOrganisation

		void FindOrganisationHandler(object sender, EventArgs e)
		{
			if (CurrentBusinessObjectInGrid != null)
			{
				ShowRelatedOrganisation((RefPremisesGateCode)CurrentBusinessObjectInGrid);
			}
		}

		internal void ShowRelatedOrganisation(RefPremisesGateCode selectedBO)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, selectedBO.R5_PremisesGateCode);
			query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_CodeType, selectedBO.R5_OrgRegCodeType);
			OrgCusCode registrationNumber = Factory.LoadTop1<OrgCusCode>(query);
			if (registrationNumber != null)
			{
				OrgHeader organisation = registrationNumber.Header;
				OrganisationController.ShowEditForm(organisation);
			}
			else
			{
				ZOrganisationsForm newOrgForm = (ZOrganisationsForm)OrganisationController.ShowNewForm();
				OrgCusCode newRegistrationNumber = ((OrgHeader)newOrgForm.BusinessEntity).CustomsCodes.AddNew();
				newRegistrationNumber.OK_CustomsRegNo = selectedBO.R5_PremisesGateCode;
				newRegistrationNumber.OK_CodeType = selectedBO.R5_OrgRegCodeType;
			}
		}

		#endregion

		#region Implementation

		protected ZController OrganisationController
		{
			get
			{
				if (fOrganisationController == null)
				{
					fOrganisationController = ZControllerFactory.Create(ControllerIDs.Organisation);
				}
				return fOrganisationController;
			}
		}

		ZController fOrganisationController;

		#endregion
	}
}
