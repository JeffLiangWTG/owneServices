using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class DeclarationLevelPackingGroupCollection : EU.Business.Declaration.DeclarationLevelPackingGroupCollection
{
	public DeclarationLevelPackingGroupCollection(JobDeclaration declaration) : base(declaration)
	{
	}

	public new PackingGroup this[int index] => (PackingGroup)base[index];

	public new PackingGroup AddNew() => (PackingGroup)base.AddNew();

	protected new PackingGroup AddNew(Type type) => (PackingGroup)base.AddNew(type);

	protected override BusinessObject AddNewCore() => AddNew(typeof(PackingGroup));
}
