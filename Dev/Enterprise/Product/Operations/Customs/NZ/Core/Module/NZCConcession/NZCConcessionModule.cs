using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module
{
	public class NZCConcessionModule : ZFilterGridModule
	{
		public NZCConcessionModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.NZ.Concession; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.NZ.Concession);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NZCConcessionFilterControl(GridCollection, (NZCConcessionFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new NonDependentNZCConcessionCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NZCConcessionFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NZCustomsConcession; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>();
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.NZCConcessionModule|View", "&View"), new EventHandler(HandleViewClick)));
			return menu.ToArray();
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
	}
}
