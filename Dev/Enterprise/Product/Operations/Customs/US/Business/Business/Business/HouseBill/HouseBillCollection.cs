using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class BillCollection : Customs.Business.BillCollection<Bill, JobDeclaration>
	{
		public BillCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		public Bill FindByUS_SequenceNo(ZString sequenceNo)
		{
			var sequenceNoInt = ZInt.ParseSafe(sequenceNo, -1);
			return this.Cast<Bill>().FirstOrDefault(bill => bill.US_SequenceNo == sequenceNoInt);
		}

		public bool HasAtLeastHaveReceivedArrivedStatus => this.Cast<Bill>().Any(bill => bill.DispositionCodes.Cast<DispositionData>().Any(x => x.US_Code == CargoReleaseProcessingResultList.Codes.BillArrived));

		public Bill Find(ZString billNum, ZString billType, ZString issuerCode) => this.Cast<Bill>().FirstOrDefault(bill => bill.CU_BillNum == billNum && bill.CU_BillType == billType && bill.US_UI_NKBillIssuerSCAC == issuerCode);

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new BillCollectionFetchStrategy(this);

		class BillCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
		{
			public BillCollectionFetchStrategy(BillCollection collection)
				: base(collection)
			{
			}

			new BillCollection Collection => (BillCollection)base.Collection;

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				var factory = Collection.Factory;
				foreach (Bill bill in Collection)
				{
					factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, bill.PK);
					factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, bill.PK);
				}
			}
		}
	}
}
