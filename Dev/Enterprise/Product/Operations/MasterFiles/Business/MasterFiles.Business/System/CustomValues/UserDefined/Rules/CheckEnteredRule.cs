using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class CheckEnteredRule : ICustomAddOnRule
	{
		public Action<ZPropertyInfo> GetValidator()
		{
			return MandatoryValidation.CheckEntered;
		}

		public IEnumerable<DynamicMetaData> GetMetaData()
		{
			return Array.Empty<DynamicMetaData>();
		}

		public bool CanBeApplied(Type type)
		{
			return !typeof(ZBool).IsAssignableFrom(type);
		}

		public OnSet GetOnSetBehaviour() => null;

		public string Code => CargoWise.Workflow.CustomAddOnRuleTypes.CheckEntered;

		public string Name
		{
			get { return Res.GetString("CustomAddOnRule.CheckEntered.Name", "Mandatory"); }
		}

		public bool IsEnabled { get; set; }

		public bool IsUpperCase => false;
	}
}
