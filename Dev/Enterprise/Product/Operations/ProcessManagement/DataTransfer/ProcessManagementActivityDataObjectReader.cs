using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer
{
	public abstract class ProcessManagementActivityDataObjectReader<T> : ActivityDataObjectReader<T>
		where T : BusinessObject, IWorkflowProviderCore, IWorkTaskRelatedItem, IWorkTaskRelatedItemSource, IUniversalXMLNoteParent
	{
		protected ProcessManagementActivityDataObjectReader(Activity dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected override T GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override IMatchingBusinessEntityFinder<T> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override void PopulateBusinessObjectCore(T businessObject)
		{
			PopulateFields(businessObject);
			PopulateBranch(businessObject);
			PopulateDepartment(businessObject);
			PopulateLocation(businessObject);
			PopulateCompany(businessObject);
			PopulateClients(businessObject);
			PopulateRelatedItems(businessObject);
			PopulateNotes(businessObject);
		}

		protected abstract void PopulateFields(T businessObject);

		#region Branch, Department, Company

		void PopulateBranch(T businessObject)
		{
			PopulateForeignObject(businessObject, GlbBranchSchema.GB_Code, BranchColumn, typeof(GlbBranch), activity => activity.Branch);
		}

		void PopulateDepartment(T businessObject)
		{
			PopulateForeignObject(businessObject, GlbDepartmentSchema.GE_Code, DepartmentColumn, typeof(GlbDepartment), activity => activity.Department);
		}

		void PopulateCompany(T businessObject)
		{
			PopulateForeignObject(businessObject, GlbCompanySchema.GC_Code, CompanyColumn, typeof(GlbCompany), activity => activity.Company);
		}

		void PopulateForeignObject(T businessObject, SchemaStringColumn codeColumn, SchemaGuidColumn foreignKeyColumn, Type foreignObjectType, Func<Activity, ICodeDataObject> elementGetter)
		{
			if (foreignKeyColumn != null)
			{
				var element = elementGetter(dataObject);

				if (element != null && element.Code.HasValue)
				{
					var foreignObject = businessObject.Factory.LoadFromNaturalKey(foreignObjectType, codeColumn, element.Code.Value);

					if (foreignObject != null)
					{
						SetValue(businessObject, foreignKeyColumn, foreignObject.PK);
					}
				}
			}
		}

		#endregion

		void PopulateLocation(T businessObject)
		{
			if (LocationColumn != null && (dataObject.Location?.Code.HasValue ?? false))
			{
				SetValue(businessObject, LocationColumn, dataObject.Location.Code.Value);
			}
		}

		protected virtual SchemaGuidColumn BranchColumn => null;
		protected virtual SchemaGuidColumn DepartmentColumn => null;
		protected virtual SchemaStringColumn LocationColumn => null;
		protected virtual SchemaGuidColumn CompanyColumn => null;
		protected virtual SchemaGuidColumn Client1Column => null;
		protected virtual SchemaGuidColumn Client2Column => null;

		void PopulateClients(T businessObject)
		{
			if (Client1Column != null && Client1AddressType != ActivityOrganizationAddressType.None)
			{
				var address = GetAddressFromDataObject(Client1AddressType);
				PopulateClient(businessObject, Client1Column, address, IsClient1Required, Client1AddressType);
			}

			if (Client2Column != null && Client2AddressType != ActivityOrganizationAddressType.None)
			{
				var address = GetAddressFromDataObject(Client2AddressType);
				PopulateClient(businessObject, Client2Column, address, IsClient2Required, Client2AddressType);
			}
		}

		OrganizationAddress GetAddressFromDataObject(ActivityOrganizationAddressType type)
		{
			return dataObject.OrganizationAddressCollection?.SingleOrDefault(x => x.AddressType.HasValue && string.Equals(x.AddressType, type.ToString(), StringComparison.OrdinalIgnoreCase));
		}

		void PopulateClient(T businessObject, SchemaGuidColumn column, OrganizationAddress addressData, bool isValueRequired, ActivityOrganizationAddressType addressType)
		{
			if (addressData != null)
			{
				var address = new OrganisationDataObjectReader(addressData, logger, factory).GetMatched();

				if (address != null)
				{
					var contacts = address.Header.Contacts.Cast<OrgContact>();
					var contact = contacts.SingleOrDefault(x => addressData.Contact.HasValue && string.Equals(x.OC_ContactName, addressData.Contact.Value, StringComparison.OrdinalIgnoreCase));

					if (contact != null)
					{
						businessObject.SetValue(column, contact.PK, null);
					}
					else
					{
						if (isValueRequired)
						{
							throw new DataObjectReadFailureException(Res.GetString("a8e63376-f5da-4503-8675-facaa71d1902", "Could not find a contact with the name '{0}' in the matched organization '{1}'. Because Client is a required field, this item cannot be imported.",
								addressData.Contact, address.Header.OH_FullName));
						}
					}

					SetAddress(businessObject, address, addressType);
				}
				else
				{
					if (isValueRequired)
					{
						throw new DataObjectReadFailureException(Res.GetString("bae8a987-7415-485c-955b-5bf9c015b70d", "Could not find an organization matching the details provided. Because Client is a required field, this item cannot be imported."));
					}
				}
			}
		}

		protected virtual void SetAddress(T businessObject, OrgAddress address, ActivityOrganizationAddressType addressType)
		{
		}

		protected virtual ActivityOrganizationAddressType Client1AddressType => ActivityOrganizationAddressType.None;
		protected virtual ActivityOrganizationAddressType Client2AddressType => ActivityOrganizationAddressType.None;
		protected virtual bool IsClient1Required => true;
		protected virtual bool IsClient2Required => true;

		void PopulateRelatedItems(T businessObject)
		{
			if (dataObject.RelatedActivityCollection != null)
			{
				var relatedActivitiesWithBadDataTargets = dataObject.RelatedActivityCollection.Where(activity => activity.DataContext?.DataTargetCollection == null || !activity.DataContext.DataTargetCollection.Any(target => target.Type.HasValue)).ToArray();

				foreach (var activity in relatedActivitiesWithBadDataTargets)
				{
					logger.LogBoth(LogType.Warning, Res.GetString("51f07a10-d28b-42e6-9b24-a1fc8272b6f4", "Could not attach related item [{0}]. Each related Activity must include a valid {1}.", activity.Summary, "DataTarget"));
				}

				var supportedModules = businessObject.SupportedRelatedItemModules.ToArray();
				var validRelatedActivities = dataObject.RelatedActivityCollection.Where(x => !relatedActivitiesWithBadDataTargets.Contains(x)).Select(x => new RelatedItemInfo(x, supportedModules)).ToArray();
				var supportedItems = validRelatedActivities.Where(x => x.ModuleInfo != null).ToArray();
				var nonSupportedItems = validRelatedActivities.Except(supportedItems).ToArray();

				if (nonSupportedItems.Any())
				{
					var supportedModulesString = GetSupportedModulesString(supportedModules);

					foreach (var item in nonSupportedItems)
					{
						logger.LogBoth(LogType.Warning, Res.GetString("e867d9d4-9dd8-44f9-a138-292a2bcd4c79",
							"Could not attach related item [{0}: {1} - {2}]. Only item types {3} can be attached to a {4}.",
							item.Type, item.Key, item.Summary, supportedModulesString, businessObject.Type));
					}
				}

				if (supportedItems.Any())
				{
					PopulateSupportedRelatedItems(businessObject, supportedItems);
				}
			}
		}

		void PopulateSupportedRelatedItems(T businessObject, RelatedItemInfo[] supportedItems)
		{
			var itemsByType = supportedItems.GroupBy(x => x.Type);
			var foundJobNumbers = new List<ZString>();
			var notFoundItems = new List<RelatedItemInfo>();

			foreach (var itemGroup in itemsByType)
			{
				var moduleInfo = itemGroup.First().ModuleInfo;
				var jobNumbersToLoad = itemGroup.Where(x => x.Key.HasValue).Select(x => x.Key.Value).ToArray();
				var items = businessObject.Factory.Load(moduleInfo.BusinessObjectType, new ZQuery(moduleInfo.JobNumberColumn, jobNumbersToLoad));
				businessObject.RelatedItems.AddRange(items);

				foundJobNumbers.AddRange(items.Cast<IWorkTaskRelatedItem>().Select(w => w.Number));
				var notFoundJobNumbers = jobNumbersToLoad.Where(x => !foundJobNumbers.Contains(x));
				notFoundItems.AddRange(itemGroup.Where(x => x.Key.HasValue && notFoundJobNumbers.Contains(x.Key.Value)));

				var newItems = CreateNewRelatedItems(itemGroup.Where(x => !x.Key.HasValue), moduleInfo.DataContextType);
				businessObject.RelatedItems.AddRange(newItems);
			}

			foreach (var item in notFoundItems)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("8a3d9012-1b73-45b9-91b4-09929d3688f4",
					"Could not attach related item [{0}: {1} - {2}]. A {0} with the specified job number was not found.",
					item.Type, item.Key, item.Summary));
			}
		}

		IEnumerable<BusinessObject> CreateNewRelatedItems(IEnumerable<RelatedItemInfo> itemInfos, DataContextType type)
		{
			var manager = ActivityDataContextManagerHelper.GetActivityDataContextManagers(type).Single();

			foreach (var itemInfo in itemInfos)
			{
				var reader = manager.GetActivityDataObjectReader(itemInfo.Activity, logger, factory);
				yield return reader.ReadIntoTopLevelBusinessObject();
			}
		}

		class RelatedItemInfo
		{
			public RelatedItemInfo(Activity activity, WorkTaskRelatedItemModuleInfo[] supportedModules)
			{
				Activity = activity;
				Target = activity.DataContext.DataTargetCollection.First(x => x.Type.HasValue);

				ModuleInfo = Enum.TryParse(Target.Type, out DataContextType contextType)
					? supportedModules.FirstOrDefault(x => x.DataContextType == contextType)
					: null;
			}

			public Activity Activity { get; }
			IDataTargetDataObject Target { get; }
			public ZString Summary => Activity.Summary.Value;
			public ZString Type => Target.Type.Value;
			public ZString? Key => Target.Key;
			public WorkTaskRelatedItemModuleInfo ModuleInfo { get; }
		}

		static string GetSupportedModulesString(WorkTaskRelatedItemModuleInfo[] supportedModules)
		{
			string GetSquareBrackettedName(string name)
			{
				return FormattableString.Invariant($"[{name}]"); // Nothing to translate, just adding some brackets
			}

			switch (supportedModules.Length)
			{
				case 0:
					return string.Empty;
				case 1:
					return GetSquareBrackettedName(supportedModules.Single().Caption);
				default:
					return string.Join(", ", supportedModules.OrderBy(x => x.Caption).Select(x => GetSquareBrackettedName(x.Caption)));
			}
		}

		void PopulateNotes(T businessObject)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, businessObject).ReadIntoCollectionRetainingUnmatchedElements();
			}
		}
	}
}
