using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IConsolidatedJobDeclarationCollection<out TJobDeclaration> : IBusinessObjectCollection<TJobDeclaration>
		where TJobDeclaration : BaseJobDeclaration
	{
		new TJobDeclaration this[int index] { get; }
		bool IsCongruentOn(params Func<TJobDeclaration, IZType>[] properties);
	}

	public class ConsolidatedJobDeclarationCollection<TJobDeclaration> : BusinessObjectCollection<TJobDeclaration>, IConsolidatedJobDeclarationCollection<TJobDeclaration>
		where TJobDeclaration : BaseJobDeclaration
	{
		public ConsolidatedJobDeclarationCollection(ConsolidatedDeclaration parentBO) : base(parentBO.Factory)
		{
			Argument.NotNull(parentBO, nameof(parentBO));
			consolidatedDeclaration = parentBO;
			(this as IBusinessObjectCollection).Parent = consolidatedDeclaration;
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			if (child is TJobDeclaration jobDeclaration)
			{
				var query = new ZQuery(CusReconEntrySchema.CRE_CRD, consolidatedDeclaration.PK);
				query.AddToFilter(CusReconEntrySchema.CRE_CH_OriginalEntry, jobDeclaration.CustomsEntryHeaders.Select(_ => _.PK));
				foreach (var entry in Factory.Load<ConsolidatedDeclarationEntry>(query))
				{
					entry.Delete();
				}
				jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
				consolidatedDeclaration.HasChanges = true;
				ConsolidatedDeclaration.SetOriginalCachedConsolidatedDeclarationCore(jobDeclaration, null);
			}
			else
			{
				throw new ArgumentException($"Collection should contain only {typeof(TJobDeclaration).FullName}, but type {child.GetType().FullName} is being removed.");
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (child is TJobDeclaration jobDeclaration)
			{
				if (!IsLoading)
				{
					if (jobDeclaration.JE_EntryStatus != ConsolidatedEntryStatusList.Codes.ReadyForConsolidation)
					{
						RemoveFromElements(child);
						throw new DeveloperNotificationException($"Trying to consolidate a declaration having a status of {jobDeclaration.JE_EntryStatus}.");
					}
					else if (!jobDeclaration.ActiveEntryHeaders.Any())
					{
						RemoveFromElements(child);
						throw new DeveloperNotificationException("Trying to consolidate a declaration which has no active entry header.");
					}
					else
					{
						var reconEntry = Factory.New<ConsolidatedDeclarationEntry>();
						reconEntry.CRE_CRD = consolidatedDeclaration.PK;
						reconEntry.CRE_CH_OriginalEntry = jobDeclaration.ActiveEntryHeaders.First().PK;
						if (consolidatedDeclaration.CRD_OA_DeclarantAddress.IsValid)
						{
							reconEntry.CRE_OA_DeclarantAddress = consolidatedDeclaration.CRD_OA_DeclarantAddress;
						}
						else if (jobDeclaration.JE_OA_DeclarantAddress.IsValid)
						{
							reconEntry.CRE_OA_DeclarantAddress = jobDeclaration.JE_OA_DeclarantAddress;
						}
						else
						{
							reconEntry.CRE_OA_DeclarantAddress = consolidatedDeclaration.Branch.OrgProxy.MainAddress.PK;
						}
						jobDeclaration.ConsolidatedEntryProvider.ApplyForConsolidation();
						consolidatedDeclaration.HasChanges = true;
						ConsolidatedDeclaration.SetOriginalCachedConsolidatedDeclarationCore(jobDeclaration, consolidatedDeclaration);
					}
				}
			}
			else
			{
				RemoveFromElements(child);
				throw new ArgumentException($"Collection should contain only {typeof(TJobDeclaration).FullName}, but type {child.GetType().FullName} is added.");
			}
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZDBOnlyQuery(typeof(TJobDeclaration));
			var subQueryToCusEntryHeader = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			var subQueryToCusReconEntry = new ZDBOnlySubQuery(typeof(ConsolidatedDeclarationEntry), CusReconEntrySchema.CRE_CH_OriginalEntry);
			subQueryToCusReconEntry.AddToFilter(CusReconEntrySchema.CRE_CRD, consolidatedDeclaration.PK);
			subQueryToCusEntryHeader.AddSubQuery(subQueryToCusReconEntry, JoinCondition.And);
			query.AddSubQuery(subQueryToCusEntryHeader, JoinCondition.And);
			return query;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			if (bizOAdded is TJobDeclaration jobDeclaration && jobDeclaration.JE_EntryStatus == ConsolidatedEntryStatusList.Codes.AppliedToConsolidation)
			{
				jobDeclaration.OnAppliedToConsolidatedDeclaration();
			}
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject AddNewCore()
		{
			throw new DeveloperNotificationException("Only existing declarations can be consolidated.");
		}

		protected override BusinessObject AddNewCore(Type typeOfBizo)
		{
			throw new DeveloperNotificationException("Only existing declarations can be consolidated.");
		}

		public IEnumerator<TJobDeclaration> GetEnumerator() => Elements.Cast<TJobDeclaration>().GetEnumerator();

		public bool IsCongruentOn(params Func<TJobDeclaration, IZType>[] properties)
		{
			if (Count > 1)
			{
				var firstDeclaration = this[0];
				return !Elements.Skip(1).Cast<TJobDeclaration>().Any(el => properties.Any(prop => !prop(el).Equals(prop(firstDeclaration))));
			}
			else
			{
				return true;
			}
		}

		readonly ConsolidatedDeclaration consolidatedDeclaration;
	}
}
