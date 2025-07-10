using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public class NamedAccountsOfContractFilter : ModuleGuidPivotFilter
	{
		readonly ZString parentPKColumn;
		public NamedAccountsOfContractFilter(ZString description, IBusinessObjectCollection collection, Type parentType, ZString parentPrefix, ZString parentPKColumn)
			: base(description,
				  ModuleIDs.Organisation,
				  RatingContractNamedAccountPivotSchema.RNP_OH_NamedAccount,
				  RatingContractNamedAccountPivotSchema.RNP_ParentID,
				  collection,
				  parentType,
				  typeof(RatingContractNamedAccountPivot),
				  new ZQuery(RatingContractNamedAccountPivotSchema.RNP_ParentTableCode, parentPrefix))
		{
			this.parentPKColumn = parentPKColumn;
		}

		protected override string ParentPkColumnNameForAllMatch => parentPKColumn;
	}
}
