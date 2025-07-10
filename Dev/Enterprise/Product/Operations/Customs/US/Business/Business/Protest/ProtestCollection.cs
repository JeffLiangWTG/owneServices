using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Protest
{
	[ModuleID(ModuleId.Protest)]
	public class ProtestCollection : NonPersistentBusinessObjectCollection<Protest>
	{
		public ProtestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return new Protest(declaration);
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ProtestCollectionFetchStrategy(this);
		}

		class ProtestCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
		{
			public ProtestCollectionFetchStrategy(ProtestCollection collection)
				: base(collection)
			{
			}

			protected new ProtestCollection Collection
			{
				get { return (ProtestCollection)base.Collection; }
			}

			protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
			{
				base.FetchForViewCore(businessObjects, columns);
				var protests = businessObjects.OfType<Protest>();
				var factory = Collection.Factory;
				var glbStaffReqquiredFetchForView = columns.FirstOrDefault(x => x.ColumnName == Protest.Schema.BrokerName) != null;
				if (glbStaffReqquiredFetchForView)
				{
					foreach (var reconDec in protests)
					{
						factory.AddFetchHint(GlbStaffSchema.GS_Code, reconDec.JE_GS_NKCusAgent);
					}
				}
			}
		}

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new ProtestFindBoxListProvider(this); }
		}

		class ProtestFindBoxListProvider : FindBoxListProvider
		{
			public ProtestFindBoxListProvider(IBusinessObjectCollection collection)
				: base(collection)
			{
			}

			protected override void AddCodeStartsWithFilter(ZQuery query, string code)
			{
				AddToFilter(query, SQLComparisonOperator.StartsWith, code);
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				AddToFilter(query, SQLComparisonOperator.Equal, code);
			}

			void AddToFilter(ZQuery query, SQLComparisonOperator comparisonOperator, string code)
			{
				ZDBOnlyQuery codeQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

				ZDBOnlySubQuery entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, JobMessageTypeList.MoreCodes.Protest);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, code);
				codeQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);

				query.AddToFilter(Enterprise.ZArchitecture.Schema.JobDeclarationSchema.JE_MessageType, JobMessageTypeList.MoreCodes.Protest);
				query.AddToFilter(codeQuery);
			}
		}

		#endregion
	}
}
