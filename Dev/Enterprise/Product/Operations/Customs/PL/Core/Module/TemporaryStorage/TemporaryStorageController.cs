using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.Customs.PL.GUI.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.Module;

public class TemporaryStorageController : EU.TemporaryStorage.Module.TemporaryStorageController
{
	public TemporaryStorageController() : base()
	{
	}

	public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageJobHeader);

	protected override IZForm GetForm(IBusiness businessEntity) => new CusTempStorageForm((CusTempStorageJobHeader)businessEntity);

	protected override IBusiness GetNewBusinessEntityInLocalFactory()
	{
		return CusTempStorageJobHeader.New(Factory);
	}
}
