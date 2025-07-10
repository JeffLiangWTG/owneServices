using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOrgTimetableRegistryItem))]
	sealed class DefaultOrgTimetableRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultOrgTimetableSettingsCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultOrgTimetableSettingsCollection, DefaultOrgTimetableSettingsCollection> GetNewRegistryItem()
		{
			var defaultOrgtimetableSettings = new DefaultOrgTimetableSettingsCollection();
			defaultOrgtimetableSettings.AddNew();
			return new DefaultOrgTimetableRegistryItem(
				"DefaultOrgTimetable",
				OrganisationsDataRegistry.Categories.Organizations_DefaultValues,
				(NoResString)"Pickup & Delivery Timetable",
				(NoResString)"Pickup & Delivery Timetable",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				defaultOrgtimetableSettings
				);
		}
	}
}
