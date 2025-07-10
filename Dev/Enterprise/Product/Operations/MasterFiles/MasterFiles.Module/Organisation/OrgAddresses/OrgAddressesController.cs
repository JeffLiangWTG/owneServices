using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for OrgAddresses.
	/// </summary>
	public class OrgAddressesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public OrgAddressesController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgAddresses; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrgAddresses; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgAddress); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			SetInitialTabPageNameToSelectWhenAFormIsShown(OrganisationTabPages.Address.Name);
			OrgAddress address = (OrgAddress)businessEntity;
			lastSavedPK = address.PK;
			return new ZOrganisationsForm(address.Header);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return org.MainAddress;
		}

		public override ZGuid LastSavedPK
		{
			get { return lastSavedPK; }
		}
		ZGuid lastSavedPK;
	}
}
