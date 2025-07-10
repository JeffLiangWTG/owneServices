using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NCTSWarehouseToOpenProvider : INCTSWarehouseToOpen
	{
		public NCTSWarehouseToOpenProvider(NctsPreviousDocument previousDocument)
		{
			this.previousDocument = Argument.NotNull(previousDocument, nameof(NctsPreviousDocument));
		}
		protected readonly NctsPreviousDocument previousDocument;
		ZDateTime EffectiveDate => ((previousDocument.GoodsItem as NctsDepartureCargoDesc)?.MoveHeader is NctsDepartureMovementHeader header && !header.BM_ValuationDate.IsEmpty) ? header.BM_ValuationDate : ZDateTime.Today;

		public string DeclarationNo => previousDocument.CSI_ReferenceNumber;
		public int DeclarationItemNo => previousDocument.CSI_LineNo;
		public int ItemNoOfGoodsItems => previousDocument.GoodsItem.BY_LineNo;
		public decimal Quantity => previousDocument.CSI_Quantity;
		public string Explanation => previousDocument.CSI_Description;
		public decimal Value => previousDocument.CSI_Value;
		public string CurrencyCode => previousDocument.CSI_RX_NKCurrency;
		public string IncotermCode => previousDocument.Incoterm;
		public string MethodOfPaymentCode => previousDocument.CSI_SubType;
		public string ProcedureCode => previousDocument.CSI_Procedure;
		public string CountryCode => ZZRefCusMapCombined.MapCW1CodeToCustomsCode(previousDocument.GoodsItem.Header.Factory, Core.Constants.CountryCodes.Turkey, NCTSMessageProviderConstants.NCTSHeader.CountryMapType, previousDocument.CSI_RN_NKCountryCode, EffectiveDate);
	}
}
