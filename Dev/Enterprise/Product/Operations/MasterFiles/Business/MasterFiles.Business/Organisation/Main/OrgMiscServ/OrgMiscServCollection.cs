using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgMiscServCollection : DependentBusinessObjectCollection<OrgMiscServ, OrgHeader>
	{
		public OrgMiscServCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgMiscServCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public new OrgHeader Master
		{
			get { return base.Master; }
		}

		#region Delete

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			// When deleting OrgMiscServ objects we need a reference to OrgHeader as we have many
			// wrapped property infos for OrgCompanyData. When we remove these wrapped infos,
			// we can remove this hack.
			((OrgMiscServ)elementToDelete).Header = Master;
			base.RemoveAndDelete(elementToDelete);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgHeader org = Master;
			OrgMiscServ miscServ = (OrgMiscServ)child;

			if (org.UNLOCO?.Country != null)
			{
				miscServ.OM_RN_NKEXDefaultCntryOfOrigin = org.UNLOCO.RL_RN_NKCountryCode;
			}

			#endregion

		}
	}
}
