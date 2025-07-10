using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class RestrictedCodeCollection : CusCodeDataCollection<RestrictedCode>
	{
		public RestrictedCodeCollection(OrgCountryData master, ZString cY_Code)
			: base(master, CusCodeDataTypeList.Codes.IORBusinessRules)
		{
			this.cY_Code = cY_Code;
		}
		readonly ZString cY_Code;

		public void CheckRestrictedCode(ZPropertyInfo info)
		{
			var value = (ZString)info.Value;

			if (!value.IsEmpty)
			{
				var isStartWithComparison = cY_Code == RestrictedCodeTypeList.Codes.RestrictedTariff;
				var matchingDelegate = isStartWithComparison ? new Func<BusinessObject, bool>(l => value.StartsWith(((CusCodeData)l).CY_Data))
					: new Func<BusinessObject, bool>(l => ((CusCodeData)l).CY_Data == value);
				if (this.Any(matchingDelegate))
				{
					info.AddMessageError(GetRestrictedValueMessage(info));
				}
			}
		}

		internal static string GetRestrictedValueMessage(ZPropertyInfo info)
		{
			return string.Format("{0} '{1}' is not allowed for Importer of Record. For more details please refer to Importer of Record > Details > Config > US Defaults",
				info.HumanReadableName, info.Value);
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (CusCodeData)dependent;
			child.CY_Code = cY_Code;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusCodeDataSchema.CY_Code, SQLComparisonOperator.Equal, cY_Code);
			return result;
		}
	}
}
