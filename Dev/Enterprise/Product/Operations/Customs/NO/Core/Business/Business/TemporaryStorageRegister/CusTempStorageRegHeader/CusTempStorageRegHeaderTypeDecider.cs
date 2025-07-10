using System;

namespace Enterprise.Customs.NO.Business;

public sealed class CusTempStorageRegHeaderTypeDecider : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderTypeDecider
{
	public override Type GetTypeForBinding() => typeof(CusTempStorageRegHeader);

	public override Type GetTypeForNew() => typeof(CusTempStorageRegHeader);
}
