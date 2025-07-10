using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NO.Business;

public class CusTransportMeans(BusinessObjectFactory factory, DataRow row) : Customs.Business.CusTransportMeans(factory, row)
{
	public CusTempStorageRegHeader ParentHeader
	{
		get => parentHeader ??= Factory.Load<CusTempStorageRegHeader>(TPM_ParentID);
		set => parentHeader = value;
	}
	CusTempStorageRegHeader parentHeader;

	[ResourceStringData("AA73505E-0EE4-420E-ABA7-9878837511EF", Caption = "Transport ID", FullDescription = "For SEA use ship name. For AIR use flight number. For other modes use the vehicle Id.")]
	public override ZString TPM_IdentificationNumber
	{
		get => base.TPM_IdentificationNumber;
		set => base.TPM_IdentificationNumber = value;
	}

	[ResourceStringData("273B182C-E66A-4EA5-9F80-F17B26B94357", Caption = "Nationality")]
	public override ZString TPM_RN_NKTransportNationality
	{
		get => base.TPM_RN_NKTransportNationality;
		set => base.TPM_RN_NKTransportNationality = value;
	}
}
