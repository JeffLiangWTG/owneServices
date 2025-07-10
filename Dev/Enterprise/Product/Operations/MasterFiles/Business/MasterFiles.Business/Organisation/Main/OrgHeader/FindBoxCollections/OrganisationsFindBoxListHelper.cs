using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class OrganisationsFindBoxListHelper
	{
		public static void SetFilterBusinessObjectFromUnmatchOrgRecord(IBusinessObjectCollection orgCollection)
		{
			var organisationsFindBoxCollection = ((OrganisationsFindBoxCollection)orgCollection);
			var defaultForNewChild = organisationsFindBoxCollection.DefaultsForNewChild;
			var noteParent = orgCollection.Parent as IStmNoteParent ?? NoteParentFromPluginCollection(orgCollection);

			if (noteParent != null)
			{
				OrganisationTypes organisationType;
				var organisationSubType = string.Empty;
				var docAddressType = string.Empty;
				var organisationListPropertyDescriptor = orgCollection.ListPropertyDescriptor;
				if (organisationListPropertyDescriptor != null)
				{
					var organisationDefaultProviderAttr = (OrganisationDefaultProviderAttribute)organisationListPropertyDescriptor.Attributes[typeof(OrganisationDefaultProviderAttribute)];
					if (organisationDefaultProviderAttr != null)
					{
						organisationType = organisationDefaultProviderAttr.OrganisationType;
						docAddressType = organisationDefaultProviderAttr.DocAddressType;

						if (!string.IsNullOrEmpty(organisationDefaultProviderAttr.OrganisationSubType))
						{
							var organisationsSubTypeList = new OrganisationsSubTypeList();
							organisationSubType = organisationsSubTypeList[organisationDefaultProviderAttr.OrganisationSubType, StringComparison.OrdinalIgnoreCase].Description;
						}
					}
					else
					{
						organisationType = organisationsFindBoxCollection.OrganisationType;
						organisationSubType = organisationsFindBoxCollection.OrganisationSubType;
						docAddressType = organisationsFindBoxCollection.DocAddressType;
					}

					defaultForNewChild.RemoveDefaultsFromUnmatchedNote();

					var unmatchOrgCriterial = new UnmatchOrgRecordCriteria { OrganisationType = organisationType, DocAddressType = docAddressType };
					unmatchOrgCriterial.OrganisationSubType = !string.IsNullOrEmpty(organisationSubType) ? organisationSubType : organisationType.ToString();
					new UnmatchOrgRecords(noteParent).SetFilterBusinessObjectFromUnmatchOrgRecord(unmatchOrgCriterial, defaultForNewChild);
				}
			}
		}

		static IStmNoteParent NoteParentFromPluginCollection(IBusinessObjectCollection orgCollection)
		{
			var collectionForPlugin = orgCollection.Parent as JobDocAddressCollectionForPlugin;
			IStmNoteParent result = null;
			if (collectionForPlugin != null)
			{
				result = collectionForPlugin.HostParentBizo as IStmNoteParent;
			}

			return result;
		}
	}
}
