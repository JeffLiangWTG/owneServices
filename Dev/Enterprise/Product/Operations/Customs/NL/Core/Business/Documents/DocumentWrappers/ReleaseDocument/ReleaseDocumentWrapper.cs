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

public class ReleaseDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider, IDocumentWrapper, IReleaseDocumentDataProvider
{
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;

	public ReleaseDocumentWrapper(CusEntryHeader cusEntryHeader)
		: base(cusEntryHeader?.Factory)
	{
		entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		declaration = cusEntryHeader.Declaration;
	}

	#region IReleaseDocumentDataProvider
	public IAddressDocumentInformation Exporter => new AddressInformationDocumentWrapper(declaration.ExporterDocAddress);

	public IAddressDocumentInformation Declarant => new AddressInformationDocumentWrapper(declaration.Declarant);

	public ZDateTime ReleaseDate => entryHeader.CH_EntryReleaseDate;

	public ZString ConsolID => declaration.RelevantConsol?.JK_BookingReference ?? ZString.Empty;

	public IShipmentDetails ShipmentDetails => new ReleaseShipmentDetailsWrapper(entryHeader, declaration);

	public BusinessObjectCollectionWrapper<ReleaseItemLineDetailsWrapper> ItemLineDetails
	{
		get
		{
			if (itemLineDetails == null)
			{
				itemLineDetails = new BusinessObjectCollectionWrapper<ReleaseItemLineDetailsWrapper>(entryHeader.AllEntryLines.Select(x => new ReleaseItemLineDetailsWrapper(x, x.CL_LineNumber)));
			}
			return itemLineDetails;
		}
	}
	BusinessObjectCollectionWrapper<ReleaseItemLineDetailsWrapper> itemLineDetails;

	IEnumerable<IItemLineDetails> IReleaseDocumentDataProvider.ItemLines => ItemLineDetails.Select(x => x as IItemLineDetails);
	#endregion

	#region IBODocDataProvider
	public DocWrapperCopyInfo AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;

	public string[] ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;

	BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => entryHeader;

	public BusinessObject ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;

	public IZType GetCustomField(string fieldName, string fieldType = null) => BasicBODocDataProvider.GetCustomField(fieldName, fieldType);

	public string GetCustomFieldCodeDescription(string fieldName, string fieldType = null) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, fieldType);

	public ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);

	public ZDateTime GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);

	public void SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);

	IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));

	IBODocDataProvider basicBODocDataProvider;
	#endregion
}
