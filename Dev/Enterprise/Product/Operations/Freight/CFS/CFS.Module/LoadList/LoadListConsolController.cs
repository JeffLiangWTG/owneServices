using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	/// <summary>
	/// Module Controller for LoadListConsol.
	/// </summary>
	public class LoadListConsolController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LoadListConsolController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.LoadListConsol;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.LoadListConsol; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CFSLoadListConsol); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CFSLoadListConsolForm((CFSLoadListConsol)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CFSLoadList; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CFSLoadListModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CFSLoadListModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CFSLoadListModify; }
		}

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
		}

		#endregion
	}
}
