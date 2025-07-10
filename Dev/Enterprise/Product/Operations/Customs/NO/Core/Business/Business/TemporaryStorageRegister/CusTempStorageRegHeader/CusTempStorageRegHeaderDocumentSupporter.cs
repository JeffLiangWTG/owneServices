using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.NO.Business;

public class CusTempStorageRegHeaderDocumentSupporter(CusTempStorageRegHeader parentBusinessObject) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderDocumentSupporter(parentBusinessObject)
{
	public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun) => false;
}
