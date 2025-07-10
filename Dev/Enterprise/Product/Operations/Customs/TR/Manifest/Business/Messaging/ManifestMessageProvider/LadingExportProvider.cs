using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class LadingExportProvider : ILadingExports
	{
		public LadingExportProvider(RelatedDeclarationForExport decExport)
		{
			this.decExport = decExport;
		}

		readonly RelatedDeclarationForExport decExport;

		public ZDecimal GrossWeight => decExport.CSI_Quantity2;
		public ZInt BoxQuantity => decimal.ToInt32(decExport.CSI_Quantity);
		public ZString ReferenceNumber => decExport.CSI_ReferenceNumber;
		public ZString IsSubType => decExport.CSI_SubType == SubTypeList.Codes.Yes ? TurkishConstants.AnswerYes : TurkishConstants.AnswerNo;
		public ZString IsProcedure => decExport.CSI_Procedure;
	}
}
