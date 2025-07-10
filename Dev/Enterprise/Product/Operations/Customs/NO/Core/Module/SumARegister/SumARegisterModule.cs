using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.NO.Business; 
using Enterprise.ZArchitecture.Modules;
using CusTempStorageRegHeader = Enterprise.Customs.NO.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.NO.Module;

public sealed class SumARegisterModule : EFTA.TemporaryStorageRegister.Module.SumARegisterModule
{
	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.NO.TemporaryStorageRegister);

	protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, TemporaryStorageApplicationCodeList.Codes.SBW);
}
