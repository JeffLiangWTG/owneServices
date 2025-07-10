using System;
using Enterprise.Integration;

namespace Enterprise.Customs.TR.Business
{
	public interface ITRCustomsDataRegistryActionHandler
	{
		RegistryUpdateActionDelegate OnUpdateAction { get; }

		Action OnAllValuesSaved { get; }
	}
}
