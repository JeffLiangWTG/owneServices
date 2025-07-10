using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class ReleaseShipmentDetailsWrapper : NonPersistentBusinessObject, IShipmentDetails
{
	readonly JobDeclaration declaration;
	readonly CusEntryHeader entryHeader;

	public ReleaseShipmentDetailsWrapper(CusEntryHeader entryHeader, JobDeclaration declaration) : base((entryHeader as BusinessObject)?.Factory ?? new BusinessObjectFactory())
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	public ZString ID => declaration.Shipment?.JS_BookingReference ?? ZString.Empty;

	public IAddressDocumentInformation ShipperSupplier => declaration.SupplierAddress == null ? new AddressInformationDocumentWrapper(declaration.SupplierDocumentaryAddress) : new AddressInformationDocumentWrapper(declaration.SupplierAddress);

	public IAddressDocumentInformation Consignee => declaration.ImporterAddress == null ? new AddressInformationDocumentWrapper(declaration.ImporterDocumentaryAddress) : new AddressInformationDocumentWrapper(declaration.ImporterAddress);

	public ZInt Packages => entryHeader.AllEntryLines.SelectMany(x => x.PackagingDetails).Cast<Customs.Business.InvoiceLinePackagePivot>().Sum((Customs.Business.InvoiceLinePackagePivot x) => x.CHC_NumberOfPacks);

	public ZDecimal Weight => declaration.JE_TotalWeight;

	public ZString Origin => declaration.JE_RL_NKOrigin;

	public ZString Destination => declaration.JE_RL_NKFinalDestination;

	public ZString Master => entryHeader.Declaration.JE_MasterBill;

	public ZString House => entryHeader.Declaration.JE_HouseBill;

	public ZString Containers
	{
		get
		{
			var result = new ZStringBuilder();
			declaration.CusContainers.ContainerNumbers.ForEach(x => result.Append(x));
			return result.ToStringWithDelimiterBetweenAppends(",").TrimEnd(',');
		}
	}

	public ZString Mrn => entryHeader.EntryNumber;

	public IDeclarationGoodsLocation GoodsLocation
	{
		get
		{
			var goodsLocation = entryHeader.EntryInstruction is CusEntryInstruction instruction && instruction.GoodsLocation is CusGoodsLocation location && !location.CGL_Qualifier.IsEmpty ? location : declaration.GoodsLocation as CusGoodsLocation;
			return new ReleaseDeclarationGoodsLocationWrapper(goodsLocation);
		}
	}
}
