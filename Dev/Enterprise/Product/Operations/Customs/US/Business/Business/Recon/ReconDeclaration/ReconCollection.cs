using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ReconCollection : NonPersistentBusinessObjectCollection<ReconDeclaration>
	{
		public ReconCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new ReconDeclaration(declaration);
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new ReconCollectionFetchStrategy(this);
		}

		class ReconCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
		{
			public ReconCollectionFetchStrategy(ReconCollection collection)
				: base(collection)
			{
			}

			protected new ReconCollection Collection
			{
				get { return (ReconCollection)base.Collection; }
			}

			protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
			{
				base.FetchForViewCore(businessObjects, columns);
				var reconDeclarations = businessObjects.OfType<ReconDeclaration>();
				var factory = Collection.Factory;
				var cusEntryHeaderCusEntryNumRequiredFetchForView = columns.FirstOrDefault(x => x.ColumnName == ReconDeclaration.Schema.ReconEntryNumber) != null;
				if (cusEntryHeaderCusEntryNumRequiredFetchForView)
				{
					var entriesQuery = new ZQuery(CusEntryHeaderSchema.CH_JE, reconDeclarations.Select(x => x.JE_PK));
					var entries = factory.Load<CusEntryHeader>(entriesQuery);
					if (entries.Length > 0)
					{
						foreach (var entry in entries)
						{
							factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, entry.PK);
						}
					}
				}
				var glbStaffReqquiredFetchForView = columns.FirstOrDefault(x => x.ColumnName == ReconDeclaration.Schema.PreparerName) != null;
				if (glbStaffReqquiredFetchForView)
				{
					foreach (var reconDec in reconDeclarations)
					{
						factory.AddFetchHint(GlbStaffSchema.GS_Code, reconDec.JE_GS_NKCusAgent);
					}
				}
			}
		}
	}
}
