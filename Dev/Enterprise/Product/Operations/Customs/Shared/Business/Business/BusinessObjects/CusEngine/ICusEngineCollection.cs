using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business;

public interface ICusEngineCollection<out TCusEngine, out TCusEngineParent> : IDependentBusinessObjectCollection
	where TCusEngine : CusEngine
	where TCusEngineParent : ICusEngineParent
{
	new TCusEngineParent Master { get; }
	void Load();
	void RemoveAndDelete(BusinessObject elementToDelete);
	void RemoveAndDeleteAll();
	TCusEngine AddNew(Type bizOType);
	new TCusEngine AddNew();
	new TCusEngine this[int index] { get; }
	void MarkAsNeedingValidation();
	event CollectionCountChangedEventHandler CountChanged;
}
