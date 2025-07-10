using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public delegate void OnSet(BusinessObject bizo, ZPropertyInfo info, ZGuid instanceIdentifier, CustomAddOnRuleArgs args);

	public interface ICustomAddOnRule : CargoWise.Workflow.ICustomAddOnRule
	{
		string Name { get; }
		bool CanBeApplied(Type type);
		Action<ZPropertyInfo> GetValidator();
		OnSet GetOnSetBehaviour();
		IEnumerable<DynamicMetaData> GetMetaData();
		bool IsUpperCase { get; }
	}
}
