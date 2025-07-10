using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public abstract class ProcessManagementActivityDataObjectWriter<T> : ActivityDataObjectWriter<T>
		where T : BusinessObject, IWorkTaskRelatedItemSource, IUniversalXMLNoteParent
	{
		protected ProcessManagementActivityDataObjectWriter(IDataWritingManager writeManager, bool shouldIncludeRelatedItems)
			: base(writeManager)
		{
			this.shouldIncludeRelatedItems = shouldIncludeRelatedItems;
		}

		readonly bool shouldIncludeRelatedItems;

		protected sealed override void PopulateDataObject(T businessObject, Activity activity)
		{
			PopulateDataObjectCore(businessObject, activity);
			PopulateNotes(businessObject, activity);
			PopulateCreatedBy(businessObject, activity);
			PopulateConversation(businessObject, activity);

			if (shouldIncludeRelatedItems)
			{
				PopulateRelatedItems(businessObject, activity);
			}
		}

		void PopulateNotes(T businessObject, Activity activity)
		{
			activity.SetNoteCollection(() =>
			{
				var notes = businessObject.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);

				return ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial);
			});
		}

		protected abstract void PopulateDataObjectCore(T businessObject, Activity activity);

		void PopulateRelatedItems(T businessObject, Activity activity)
		{
			var items = businessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().OrderBy(x => x.ItemDescription).ToArray();

			if (items.Any())
			{
				var itemsByType = items.GroupBy(x => x.GetType());

				foreach (var itemsGroup in itemsByType)
				{
					var attribute = itemsGroup.First().GetAttribute<UniversalDataContextAttribute>();

					if (attribute != null)
					{
						void PopulateRelatedActivityCollection(List<Activity> collection)
						{
							var manager = ActivityDataContextManagerHelper.GetActivityDataContextManagers(attribute.DataContextType).Single();

							foreach (var item in itemsGroup.Cast<BusinessObject>())
							{
								var writer = manager.GetActivityDataObjectWriter(writeManager, shouldIncludeRelatedItems: false);
								var relatedActivity = (Activity)writer.GetDataObject(item);
								collection.Add(relatedActivity);
							}
						}

						if (activity.RelatedActivityCollection == null)
						{
							activity.SetRelatedActivityCollection(() =>
							{
								var collection = new List<Activity>();

								PopulateRelatedActivityCollection(collection);

								return collection;
							});
						}
						else
						{
							PopulateRelatedActivityCollection(activity.RelatedActivityCollection);
						}
					}
				}
			}
		}

		protected static CodeDescriptionPair GetCodeDescriptionPair(ZString code, ICodeDescriptionPairList lookups)
		{
			var description = lookups.GetDescriptionFromCode(code);

			return new CodeDescriptionPair { Code = code, Description = description };
		}

		void PopulateCreatedBy(T businessObject, Activity activity)
		{
			var staffCode = (ZString)businessObject[CreatedByColumn];

			if (!staffCode.IsEmpty)
			{
				var staff = businessObject.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);

				if (staff != null)
				{
					activity.CreatedBy = Staff.New(staff);
				}
			}
		}

		protected abstract SchemaStringColumn CreatedByColumn { get; }

		void PopulateConversation(T source, Activity destination)
		{
			var shouldIncludeInternalMessages = IsOrgProxyOnlySelected();
			UniversalEConversationHelper.PopulateConversation(source, destination, writeManager, shouldIncludeInternalMessages);
		}
	}
}
