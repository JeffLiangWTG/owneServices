using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestsSubclassesOf(typeof(GuaranteesFilterStripBusinessObject))]
	public abstract class GuaranteesFilterStripBusinessObjectAbstractTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => GetGuaranteesFilterStripBusinessObject();

		public GuaranteesFilterStripBusinessObject GetGuaranteesFilterStripBusinessObject() => new ();

		protected static void AddPermitLine(BaseCusGuaranteeHeader guaranteeHeader, ZString reference, ZDate transactionDate)
		{
			var guaranteeLine = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLine.FillWithValidTestData();
			guaranteeLine.CPL_Reference = reference;
			guaranteeLine.CPL_TransactionDate = transactionDate;
		}
	}
}
