using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class OrgHeaderCollection : BusinessObjectCollection<OrgHeader>, IOrgHeaderCollection
	{
		public OrgHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgHeaderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		/// <summary>
		/// Returns whether new temporary organisations are allowed to be created and added to this collection
		/// </summary>
		public virtual bool AllowNewTemporaryOrganisations
		{
			get { return false; }
		}

		#region IOrgHeaderCollection Members

		void IOrgHeaderCollection.SetDefaultsForNewChild(object child)
		{
			SetDefaultsForNewChild((OrgHeader)child);
		}

		#endregion
	}
}
