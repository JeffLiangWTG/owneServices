using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	internal class RefundWorkSheetDocumentWrapper : DA63DocumentWrapper, IBODocDataProvider, IDocumentWrapper
	{
		#region Constructor

		public RefundWorkSheetDocumentWrapper(CusEntryHeader cusEntryHeader)
			: base(cusEntryHeader)
		{
		}

		#endregion

		#region Fields

		public BusinessObjectCollectionWrapper<RefundWorkSheetLineDetailWrapper> RefundWorkSheetLines
		{
			get
			{
				if (refundWorkSheetLines == null)
				{
					var eligibleLines = EntryHeader.MergedLines?.OfType<CusEntryLine>()?.Select(x => new RefundWorkSheetLineDetailWrapper(x, this));
					DetermineFirstTwoOtherTaxType(eligibleLines);
					refundWorkSheetLines = new BusinessObjectCollectionWrapper<RefundWorkSheetLineDetailWrapper>(eligibleLines ?? System.Array.Empty<RefundWorkSheetLineDetailWrapper>());
				}
				return refundWorkSheetLines;
			}
		}
		BusinessObjectCollectionWrapper<RefundWorkSheetLineDetailWrapper> refundWorkSheetLines;

		void DetermineFirstTwoOtherTaxType(IEnumerable<RefundWorkSheetLineDetailWrapper> eligibleLines)
		{
			var taxTypes = eligibleLines.SelectMany(x => x.TaxTypes.Where(t => !t.IsEmpty)).Distinct().OrderBy(x => x);
			FirstOtherTaxType = taxTypes.FirstOrDefault();
			SecondOtherTaxType = taxTypes.ElementAtOrDefault(1);
		}

		public ZString FirstOtherTaxType { get; private set; }

		public ZString SecondOtherTaxType { get; private set; }

		#endregion
		#region IBODocDataProvider

		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => EntryHeader;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => this.EntryHeader.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
