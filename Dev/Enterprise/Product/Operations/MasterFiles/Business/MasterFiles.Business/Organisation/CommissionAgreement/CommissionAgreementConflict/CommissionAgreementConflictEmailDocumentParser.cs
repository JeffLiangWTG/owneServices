using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.MasterFiles.Business
{
	public class CommissionAgreementConflictEmailDocumentParser : DocumentParser<CommissionAgreementConflictEmailCreator>
	{
		public CommissionAgreementConflictEmailDocumentParser(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocCommissionAgreementConflictEmailCreator); }
		}
	}
}
