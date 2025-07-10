using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UniversalCopyClearCollectionOnCopy]
	public class JobDocAddressDependentCollection : DependentBusinessObjectCollection<JobDocAddress, BusinessObject>, IJobDocAddressDependentCollection
	{
		public JobDocAddressDependentCollection(IDocAddresses parent)
			: base((BusinessObject)parent)
		{
			this.Parent = parent;
		}

		readonly IDocAddresses Parent;

		#region Load

		public override void Load()
		{
			base.Load();

			var addressTypesToAdd = new List<DocAddressType>();
			foreach (DocAddressType addressType in Parent.SupportedAddressTypes)
			{
				if (!ContainsDocAddressType(addressType))
				{
					JobDocAddressRequirement requirement = Parent.GetDocAddressRequirement(addressType);
					if (requirement != null && requirement.IsMandatory)
					{
						addressTypesToAdd.Add(addressType);
					}
				}
			}
			// this saves scanning addresses that have already been created..
			foreach (var addressType in addressTypesToAdd)
			{
				AddNew(addressType);
			}
		}

		#endregion

		#region Add

		#region AddNew

		public JobDocAddress AddNew(DocAddressType docAddressType)
		{
			JobDocAddress[] docAddress = FindDocAddressesByType(docAddressType);
			int max = docAddress.Length > 0 ? docAddress[docAddress.Length - 1].E2_AddressSequence + 1 : 0;
			return AddNew(docAddressType, max);
		}

		public JobDocAddress AddNew(DocAddressType docAddressType, int sequence)
		{
			JobDocAddress result = AddNew();

			using (result.SuspendSettingHasChanges())
			{
				result.DocAddressType = docAddressType;
				result.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
			}

			return result;
		}

		public JobDocAddress AddNew(OrgAddress orgAddress)
		{
			return AddNew(orgAddress, DocAddressType.None);
		}

		public JobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			JobDocAddress result = AddNew(docAddressType);
			result.E2_OA_Address = orgAddress.PK;
			return result;
		}

		#endregion

		#region OnRemoved

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			JobDocAddress.ReinitializeAddressSequenceNumber(this.Cast<JobDocAddress>());
		}

		#endregion

		#region OnAdded

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			JobDocAddress addedAddress = (JobDocAddress)bizOAdded;
			if (IsUpdatingByDataRefreshBus)
			{
				ZQuery query = new ZQuery(JobDocAddressSchema.E2_AddressType, addedAddress.E2_AddressType);
				query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, addedAddress.E2_AddressSequence);
				query.AddToFilter(JobDocAddressSchema.PK, SQLComparisonOperator.NotEqual, addedAddress.PK);

				foreach (JobDocAddress address in Find(query))
				{
					if (!address.IsInDatabase)
					{
						address.Delete();
					}
				}
			}
			base.OnAdded(bizOAdded);
			addedAddress.ParentType = Master.GetType();
		}

		#endregion

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Find

		public bool ContainsDocAddressType(DocAddressType docAddressType)
		{
			return FindByDocAddressType(docAddressType) != null;
		}

		public JobDocAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			return FindDocAddressesByType(docAddressType).FirstOrDefault();
		}

		public JobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			if (Count > 0)
			{
				var addressType = DocAddressTypes.GetCode(Factory, docAddressType);
				var sequenceAsByte = ZByte.ParseSafe(sequence.ToString(), 0);
				foreach (JobDocAddress item in this)
				{
					if (item.E2_AddressType == addressType && item.E2_AddressSequence == sequenceAsByte)
					{
						return item;
					}
				}
			}
			return null;
		}

		public JobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return FindDocAddressesByType(docAddressType, null);
		}

		public JobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, JobDocAddress ignoreDocAddress)
		{
			return FindDocAddressesByType(new[] { docAddressType }, new[] { ignoreDocAddress });
		}

		public JobDocAddress[] FindDocAddressesByType(DocAddressType[] docAddressTypes)
		{
			return FindDocAddressesByType(docAddressTypes, null);
		}

		public JobDocAddress[] FindDocAddressesByType(DocAddressType[] docAddressTypes, JobDocAddress[] ignoreDocAddresses)
		{
			if (Count > 0 && docAddressTypes != null && docAddressTypes.Length > 0)
			{
				var excludedPKs = CreateExcludedPKs(ignoreDocAddresses);
				var inputOrderLookup = CreateInputOrderLookup(docAddressTypes);

				return this
					.Cast<JobDocAddress>()
					.Where(jobDocAddress => !jobDocAddress.IsDeleted && inputOrderLookup.ContainsKey(jobDocAddress.E2_AddressType) && !excludedPKs.Contains(jobDocAddress.PK))
					.OrderBy(jobDocAddress => inputOrderLookup[jobDocAddress.E2_AddressType])
					.ThenBy(jobDocAddress => jobDocAddress.E2_AddressSequence)
					.ToArray();
			}
			else
			{
				return System.Array.Empty<JobDocAddress>();
			}
		}

		public JobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			JobDocAddress result = FindByDocAddressType(docAddressType);
			return result ?? AddNew(docAddressType);
		}

		public JobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			JobDocAddress result = FindByDocAddressType(requirement.DefaultDocAddressType);

			if (result == null)
			{
				result = CreateWithRequirement(requirement);
			}
			else
			{
				AddRequirementToDocAddress(requirement, result);
				foreach (JobDocAddressRequirement additionalRequirement in requirement.AdditionalRequirements.Values)
				{
					FindOrCreateWithRequirement(additionalRequirement);
				}
			}

			return result;
		}

		public event System.Action<DocAddressType> DocAddressOnAddedEvent;

		public void RaiseDocAddressCollectionChanged(DocAddressType newAddressType)
		{
			DocAddressOnAddedEvent?.Invoke(newAddressType);
		}

		#region FindDocAddressesByType Implementation

		HashSet<ZGuid> CreateExcludedPKs(JobDocAddress[] ignoreDocAddresses)
		{
			return new HashSet<ZGuid>(
				(ignoreDocAddresses ?? System.Array.Empty<JobDocAddress>())
				.Where(ignoreDocAddress => ignoreDocAddress != null && !ignoreDocAddress.PK.IsEmpty)
				.Select(ignoreDocAddress => ignoreDocAddress.PK)
				.Distinct());
		}

		IReadOnlyDictionary<ZString, int> CreateInputOrderLookup(DocAddressType[] docAddressTypes)
		{
			return Enumerable.Range(0, docAddressTypes.Length)
				.Select(index => new { index, type = docAddressTypes[index] })
				.GroupBy(x => x.type)
				.Select(x => new { type = x.Key, x.OrderBy(grouping => grouping.index).First().index })
				.ToDictionary(x => DocAddressTypes.GetCode(Factory, x.type), x => x.index);
		}

		#endregion

		#endregion

		#region Create

		public JobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement)
		{
			JobDocAddress result = AddNew(requirement.DefaultDocAddressType);
			AddRequirementToDocAddress(requirement, result);
			foreach (JobDocAddressRequirement additionalRequirement in requirement.AdditionalRequirements.Values)
			{
				FindOrCreateWithRequirement(additionalRequirement);
			}

			return result;
		}

		public JobDocAddress CreateWithAddressType(DocAddressType docAddressType)
		{
			return AddNew(docAddressType);
		}

		void AddRequirementToDocAddress(JobDocAddressRequirement requirement, JobDocAddress address)
		{
			address.DefaultAddressType = requirement.DefaultAddressType;
			address.DefaultContactType = requirement.DefaultContactType;
			if (requirement.SaveEvenIfBlank)
			{
				address.MakePersistentEvenIfEmpty();
			}
			address.OverrideRequirement = requirement;
		}

		public JobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			JobDocAddress result = FindOrCreateWithDocAddressType(docAddressType);
			result.E2_AddressOverride = false;
			result.E2_OA_Address = orgAddressPK;

			return result;
		}

		public JobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
		{
			JobDocAddress result = FindByDocAddressType(requirement.DefaultDocAddressType, sequence);
			if (result == null)
			{
				result = CreateWithRequirement(requirement);
				using (result.SuspendSettingHasChanges())
				{
					result.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
				}
			}
			return result;
		}

		public JobDocAddress FindOrCreateDummyAddress(int sequence)
		{
			JobDocAddress result = FindByDocAddressType(DocAddressType.NonPersistent, sequence);
			if (result == null)
			{
				result = CreateWithAddressType(DocAddressType.NonPersistent);
				using (result.SuspendSettingHasChanges())
				{
					result.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
					result.MakeNonPersistent();
				}
			}
			return result;
		}

		#endregion

		#region FK Relationship

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobDocAddressSchema.E2_ParentID; }
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			JobDocAddress address = (JobDocAddress)dependent;
			address.ParentType = Master.GetType();
			base.SetCollectionRelationships(address);
			address.E2_ParentTableCode = Master.TablePrefix;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result;
			var baseFilter = base.CreateRelationshipFilter();
			if (Master != null)
			{
				result = new ZQuery(JobDocAddressSchema.E2_ParentTableCode, Master.TablePrefix);
				result.AddToFilter(baseFilter);
			}
			else
			{
				result = baseFilter;
			}
			return result;
		}

		#endregion
	}
}
