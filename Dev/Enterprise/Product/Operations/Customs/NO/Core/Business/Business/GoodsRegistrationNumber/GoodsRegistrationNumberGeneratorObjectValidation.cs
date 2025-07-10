using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business;

public class GoodsRegistrationNumberGeneratorObjectValidation(GoodsRegistrationNumberGeneratorObject parent) : ZValidation(parent)
{
	public override Type AutoValidationType => typeof(GoodsRegistrationNumberGeneratorObjectValidation);

	public override void ValidateAll()
	{
		ValidateGoodsRegistrationDate();
		ValidateWarehouseAuthorisationId();
	}

	internal void ValidateGoodsRegistrationDate()
	{
		ValidateCalculatedProperty(parent.GoodsRegistrationDateInfo);
	}

	protected void CheckGoodsRegistrationDate()
	{
		MandatoryValidation.MessageErrorIfNotEntered(parent.GoodsRegistrationDateInfo);
	}

	internal void ValidateWarehouseAuthorisationId()
	{
		ValidateCalculatedProperty(parent.WarehouseAuthorisationIdInfo);
	}

	protected void CheckWarehouseAuthorisationId()
	{
		MandatoryValidation.MessageErrorIfNotEntered(parent.WarehouseAuthorisationIdInfo);
		ListValidation.MessageErrorIfInvalidCode(parent.WarehouseAuthorisationIdInfo);
	}

	readonly GoodsRegistrationNumberGeneratorObject parent = Argument.NotNull(parent, nameof(parent));
}
