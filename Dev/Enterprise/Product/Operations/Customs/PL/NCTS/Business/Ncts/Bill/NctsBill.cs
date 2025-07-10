using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsBill : EU.NCTS.Business.NctsBill
{
	public NctsBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override ZValidation GetJobDocAddressValidation(JobDocAddress addressToValidate) => new NctsJobDocAddressValidation(addressToValidate, Header);

	protected override INctsBillAdditionalDocumentCollection<EU.NCTS.Business.NctsBillAdditionalDocument> GetAdditionalDocuments() => new NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>(this);

	public new INctsArrivalCargoDescCollection<NctsArrivalCargoDesc> ArrivalGoodsItems => (INctsArrivalCargoDescCollection<NctsArrivalCargoDesc>)base.ArrivalGoodsItems;

	public new INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> AdditionalDocuments => (NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)base.AdditionalDocuments;

	protected override INctsArrivalCargoDescCollection<EU.NCTS.Business.NctsArrivalCargoDesc> GetNewArrivalGoodsItems()
	{
		return new NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>(this);
	}

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(NctsBillAdditionalDocument);
		return cusSupportingInfoTypes;
	}

	protected override bool IsWeightUQReadOnlyCore => false;
}
