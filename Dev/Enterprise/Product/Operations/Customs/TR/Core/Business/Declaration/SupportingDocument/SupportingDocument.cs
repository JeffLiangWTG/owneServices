using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class SupportingDocument : EU.Business.Declaration.MultiLineAddInfos.SupportingDocument
	{
		public SupportingDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new SupportingDocumentLookups Lookups => (SupportingDocumentLookups)base.Lookups;

		public new SupportingDocumentValidation Validation => (SupportingDocumentValidation)base.Validation;

		#region Implementation

		protected override Customs.Business.CusSupportingInfoLookups GetNewLookups()
		{
			return new SupportingDocumentLookups(this);
		}

		protected override Customs.Business.CusSupportingInfoValidation GetNewValidation()
		{
			return new SupportingDocumentValidation(this);
		}

		#endregion
	}
}
