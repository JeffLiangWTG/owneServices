using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class NctsBillAdditionalDocument : EU.NCTS.Business.NctsBillAdditionalDocument
{
	public NctsBillAdditionalDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override int CSI_ReferenceNumberMaxLength => IsArrivalMovement ? 70 : base.CSI_ReferenceNumberMaxLength;

	protected override CusSupportingInfoLookups GetNewLookups() => new NctsBillAdditionalDocumentLookups(this);

	protected override CusSupportingInfoValidation GetNewValidation()
	=> new NctsBillAdditionalDocumentPhase5Validation(this);
}
