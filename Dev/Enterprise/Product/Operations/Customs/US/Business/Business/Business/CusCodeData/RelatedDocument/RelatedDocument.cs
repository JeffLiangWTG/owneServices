using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class RelatedDocument : Customs.Business.CusCodeData
	{
		public RelatedDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
			set { base.Parent = value; }
		}

		public new RelatedDocumentLookups Lookups
		{
			get { return (RelatedDocumentLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new RelatedDocumentLookups(this);
		}

		public new RelatedDocumentValidation Validation
		{
			get { return (RelatedDocumentValidation)base.Validation; }
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new RelatedDocumentValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.RelatedDocument;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceHeader)); }
		}
	}
}
