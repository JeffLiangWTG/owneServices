using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	class DA63DocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper
	{
		#region ctor

		public DA63DocumentWrapper(CusEntryHeader cusEntryHeader)
			: base(cusEntryHeader?.Factory ?? new BusinessObjectFactory())
		{
			Argument.NotNull(cusEntryHeader, "cusEntryHeader");
			EntryHeader = cusEntryHeader;
			Declaration = cusEntryHeader.Declaration;
			CUSDECSource = new MessageSendingObject(EntryHeader);
		}

		#endregion

		#region Related Objects

		public CusEntryHeader EntryHeader { get; private set; }
		public JobDeclaration Declaration { get; private set; }

		internal ICUSDECMessageDataProvider CUSDECSource;

		#endregion

		#region Fields

		public AddressInformationDocWrapper Exporter => exporter ??= new AddressInformationDocWrapper(CUSDECSource.Exporter);
		AddressInformationDocWrapper exporter;

		public ZString CountryOfDestination => CUSDECSource.CountryOfDestination;
		public ZString TransportMode => CUSDECSource.TransportMode;

		public ZString AgentCode => CUSDECSource?.AgentCode ?? ZString.Empty;

		public OrgHeader EffectiveAgent => effectiveAgent ??= OrgHeader.FindByOrgCusCode(Factory, OrgCusCode.CodeTypes.AgentCode, AgentCode, Core.Constants.CountryCodes.SouthAfrica);
		OrgHeader effectiveAgent;

		public BusinessObjectCollectionWrapper<DA63LineDetailWrapper> DA63EntryLines => da63EntryLines ??= PopulateDA63EntryLines();
		BusinessObjectCollectionWrapper<DA63LineDetailWrapper> da63EntryLines;

		protected virtual BusinessObjectCollectionWrapper<DA63LineDetailWrapper> PopulateDA63EntryLines()
		{
			var eligableDA63Lines = EntryHeader.MergedLines?.OfType<CusEntryLine>()?.Where(x => x.RandomLine?.IsDA63 ?? false)?.Select(x => new DA63LineDetailWrapper(x));
			da63EntryLines = new BusinessObjectCollectionWrapper<DA63LineDetailWrapper>(eligableDA63Lines ?? System.Array.Empty<DA63LineDetailWrapper>());
			return da63EntryLines;
		}

		public ZBool HasDA63EntryLines => DA63EntryLines.Count > 0;

		public ZString TotalNoOfPacks => totalNoOfPacks ??= CUSDECSource.TotalNoOfPacks;
		string totalNoOfPacks;

		public ZString MarksAndNumbers => marksAndNumbers ??= ZString.Join(System.Environment.NewLine, CUSDECSource.MarksAndNumbers.Where(x => !x.IsEmpty).ToArray());
		string marksAndNumbers;

		public GlbStaff DeclarantUser => GlbStaff.CurrentUser;

		public ZString OriginalMRN => CUSDECSource?.OriginalMRN ?? ZString.Empty;

		public ZString CustomsOfficeCode => CUSDECSource?.CustomsOfficeCode ?? ZString.Empty;

		public ZDecimal TotalAmountClaimed => DA63EntryLines.Cast<DA63LineDetailWrapper>().Sum(x => x.DA63TotalAmountClaimed);

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

		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ??= BODocDataProvider.GetDefault(this);
		IBODocDataProvider basicBODocDataProvider;

		#endregion
	}
}
