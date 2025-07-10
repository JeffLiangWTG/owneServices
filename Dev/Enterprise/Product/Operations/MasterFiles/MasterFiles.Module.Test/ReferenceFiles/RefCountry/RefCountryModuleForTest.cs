using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	public class RefCountryModuleForTest : RefCountryModule
	{
		public RefCountryModuleForTest()
		{
		}

		public IFilterControl NewFilterControl
		{
			get { return GetNewFilterControl(); }
		}

		public IBusinessObjectCollection NewGridCollection
		{
			get { return GetNewGridCollection(); }
		}

		public FilterBusinessObject NewFilterBusinessObject
		{
			get { return GetNewFilterBusinessObject(); }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> actionMenuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, actionMenuItems).AddEntitiesMenuItem();

			return actionMenuItems.ToArray();
		}
	}
}
