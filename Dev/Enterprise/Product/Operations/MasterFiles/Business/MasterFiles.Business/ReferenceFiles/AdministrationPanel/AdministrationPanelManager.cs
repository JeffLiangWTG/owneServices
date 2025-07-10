using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[System.CodeDom.Compiler.GeneratedCode("CargoWise.EntityFramework", "1.0")]
	public class AdministrationPanelManager : NonPersistentBusinessObject
	{
		public AdministrationPanelManager(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Dashboard
		[List("Countries")]
		public virtual ZString DB_Country { get; }

		public RefCountryCollection Countries => countries ?? (countries = new RefCountryCollection(Factory));

		RefCountryCollection countries;
		#endregion

		#region Addresses

		[List("AdminPanelAddressCollection")]
		public MDMAdminPanelAddressCollection AdminPanelAddressCollection => adminPanelAddressCollection ?? (adminPanelAddressCollection = new MDMAdminPanelAddressCollection(Factory));

		MDMAdminPanelAddressCollection adminPanelAddressCollection;

		[List("AdminPanelProcessedAddressCollection")]
		public MDMAdminPanelAddressCollection AdminPanelProcessedAddressCollection => adminPanelProcessedAddressCollection ?? (adminPanelProcessedAddressCollection = new MDMAdminPanelAddressCollection(Factory));

		MDMAdminPanelAddressCollection adminPanelProcessedAddressCollection;

		#endregion

		#region DeDuplication
		public DeduplicationOrganisationCollection DeduplicationOrganisationCollection => deduplicationOrganisationCollection ?? (deduplicationOrganisationCollection = new DeduplicationOrganisationCollection(Factory));

		DeduplicationOrganisationCollection deduplicationOrganisationCollection;

		#endregion
	}
}
