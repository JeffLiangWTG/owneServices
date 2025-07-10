using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.Organisation)]
	public class OrganisationManyToManyCollection : ActiveBusinessObjectCollection<OrgHeader>, IOrgHeaderCollection
	{
		public OrganisationManyToManyCollection(RefCarrierConsortium consortium) : base(consortium, typeof(RefOrgConsortiumPivot))
		{
			ConsortiumParent = consortium;
		}

		public new OrgHeader[] Find(ZQuery sQLFilter)
		{
			return new List<OrgHeader>(base.Find(sQLFilter)).ToArray();
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		protected RefCarrierConsortium ConsortiumParent;

		#region IOrgHeaderCollection Members

		public bool AllowNewTemporaryOrganisations
		{
			get { return false; }
		}

		void IOrgHeaderCollection.SetDefaultsForNewChild(object child)
		{
			SetDefaultsForNewElementCore((OrgHeader)child);
		}

		#endregion
	}
}
