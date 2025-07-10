using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	public class PackContainerRegistrationModule : ZFilterGridModule
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PackContainerRegistration; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.CFSContainerRego }; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.CFSManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.CFSContainerRegistration; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.PackContainerRegistration);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PackContainerRegistrationFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new CFSContainerRegistrationList(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetGuiProviders(factory);
			return factory;
		}

		void SetGuiProviders(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ServicesSelectionGuiProvider.Register(factory);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PackContainerRegistrationFilterBusinessObject();
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return new SortInfo(CFSContainer.Schema.JC_ContainerJobID, ListSortDirection.Descending); }
		}

		#endregion

		#region New Actions

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("a4915742-2c10-4bd9-a08b-c6e85a47421b", "Storage Container"), ShowStorageFormForNewEntity));
			NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("97304446-c299-41b8-a58f-a92477737bbd", "CFS Container"), ShowCFSFormForNewEntity));
			return result.ToArray();
		}

		void ShowCFSFormForNewEntity(object sender, EventArgs e)
		{
			IZForm form = ShowNewForm();
			CFSContainer cFSContainer = (CFSContainer)((CFSContainerForm)form).BusinessEntity;
			if (cFSContainer.JC_Purpose != ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS)
			{
				using (cFSContainer.SuspendSettingHasChanges())
				{
					cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
				}
			}
		}

		void ShowStorageFormForNewEntity(object sender, EventArgs e)
		{
			IZForm form = ShowNewForm();
			CFSContainer cFSContainer = (CFSContainer)((CFSContainerForm)form).BusinessEntity;
			if (cFSContainer.JC_Purpose != ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage)
			{
				using (cFSContainer.SuspendSettingHasChanges())
				{
					cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
					cFSContainer.JC_OverrideFCLAvailableStorage = true;
				}
			}
		}

		#endregion
	}
}
