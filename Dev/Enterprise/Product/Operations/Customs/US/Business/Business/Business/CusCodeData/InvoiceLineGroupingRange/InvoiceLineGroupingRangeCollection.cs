using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineGroupingRangeCollection : DependentBusinessObjectCollection<InvoiceLineGroupingRange, JobComInvoiceLine>
	{
		public InvoiceLineGroupingRangeCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public InvoiceLineGroupingRange AddNew(ZShort startSequenceNo, ZShort endSequenceNo)
		{
			InvoiceLineGroupingRange result = AddNew();
			result.US_StartSequenceNo = startSequenceNo;
			result.US_EndSequenceNo = endSequenceNo;
			return result;
		}

		public InvoiceLineGroupingRange GetRangeWithLowestSequenceNo()
		{
			InvoiceLineGroupingRange result = null;
			foreach (InvoiceLineGroupingRange range in this)
			{
				if (result == null || result.US_StartSequenceNo > range.US_StartSequenceNo)
				{
					result = range;
				}
			}
			return result;
		}

		public int TotalNoOfSequences
		{
			get
			{
				int result = 0;
				foreach (InvoiceLineGroupingRange range in this)
				{
					result += range.NoOfSequences;
				}
				return result;
			}
		}

		public void RefreshAIILines()
		{
			foreach (InvoiceLineGroupingRange range in this)
			{
				range.RefreshAIILines();
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusCodeDataSchema.CY_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.InvoiceLineNumberRange);
			result.AddToFilter(CusCodeDataSchema.CY_Code, CusCodeDataTypeList.Codes.InvoiceLineNumberRange);
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (InvoiceLineGroupingRange)dependent;
			child.CY_Type = CusCodeDataTypeList.Codes.InvoiceLineNumberRange;
			child.CY_Code = CusCodeDataTypeList.Codes.InvoiceLineNumberRange;
		}
	}
}
