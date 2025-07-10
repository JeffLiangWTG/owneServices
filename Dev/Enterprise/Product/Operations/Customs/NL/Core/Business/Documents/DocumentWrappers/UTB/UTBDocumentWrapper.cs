using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.NL.Business;

public class UTBDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper, IUTBDocumentDataProvider
{
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	public const int ObjectionIntervalInDays = 43;

	public UTBDocumentWrapper(CusEntryHeader cusEntryHeader)
		: base(cusEntryHeader?.Factory)
	{
		Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		entryHeader = cusEntryHeader;
		declaration = cusEntryHeader.Declaration;
	}

	public ZDateTime DateOfIssue => entryHeader.CH_EntryReleaseDate;
	public ZDateTime ObjectionDateTime
	{
		get
		{
			ZDateTime result = DateOfIssue;
			if (!result.IsEmpty)
			{
				result = DateOfIssue.AddDays(ObjectionIntervalInDays);
			}
			return result;
		}
	}

	public IAddressDocumentInformation Agent => CachedValueHelper.GetValue(ref agentCached, () => new AddressInformationDocumentWrapper(declaration.ControllingAgent));
	CachedValue<IAddressDocumentInformation> agentCached;

	public IAddressDocumentInformation Declarant => new AddressInformationDocumentWrapper(declaration.Declarant?.Header);
	public ZString MovementReferenceNumber => entryHeader.EntryNumber;
	public ZDecimal TotalDutiesAndTaxes => CachedValueHelper.GetValue(ref totalDutiesAndTaxesCached, () => entryHeader.Duty + entryHeader.VAT);
	CachedValue<ZDecimal> totalDutiesAndTaxesCached;

	public BusinessObjectCollectionWrapper<UTBEntryLineDetailWrapper> CusEntryLineDetails
	{
		get
		{
			if (entryLineDetails == null)
			{
				entryLineDetails = new BusinessObjectCollectionWrapper<UTBEntryLineDetailWrapper>(entryHeader.MergedLines?.Select(x => new UTBEntryLineDetailWrapper(x, new ZGuid())));
			}
			return entryLineDetails;
		}
	}
	BusinessObjectCollectionWrapper<UTBEntryLineDetailWrapper> entryLineDetails;

	IEnumerable<IEntryLineDetails> IUTBDocumentDataProvider.EntryLineDetails => CusEntryLineDetails.Select(x => x as IEntryLineDetails);

	#region IBODocDataProvider

	public DocWrapperCopyInfo AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
	BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => entryHeader;
	void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
	public string[] ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

	public BusinessObject ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;

	public IZType GetCustomField(string fieldName, string fieldType = null) => BasicBODocDataProvider.GetCustomField(fieldName, fieldType);

	public string GetCustomFieldCodeDescription(string fieldName, string fieldType = null) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, fieldType);

	public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);

	public ZDateTime GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);

	public void SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);

	IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));

	IBODocDataProvider basicBODocDataProvider;

	public ZString UCR => declaration.JE_UCR;

	public ZString NumberOfEntryLines => CusEntryLineDetails.Count.ToString();

	public BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper> FeeDetails => CachedValueHelper.GetValue(ref feeDetailsCached, () =>
	{
		var mergedLines = entryHeader.MergedLines.OfType<CusEntryLine>().SelectMany(x => x.Fees);
		var cusEntryLineFeesList = mergedLines.OfType<CusEntryLineFee>().Select(x => new UTBEntryLineFeeWrapper(x, false));
		var lineFeeDetails = new BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper>(cusEntryLineFeesList);
		return lineFeeDetails;
	});
	CachedValue<BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper>> feeDetailsCached;
	#endregion
}
