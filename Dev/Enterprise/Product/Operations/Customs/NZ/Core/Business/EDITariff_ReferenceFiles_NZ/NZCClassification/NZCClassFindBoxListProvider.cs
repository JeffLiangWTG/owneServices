using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCClassFindBoxListProvider : IFindBoxListProvider
	{
#if DEBUG
		protected
#endif
 BusinessObjectFactory fFactory;
		protected BusinessObjectFactory Factory
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		#region IFindBoxListProvider Members

		(string, bool) IFindBoxListProvider.NearestMatch(string code, bool explicitAutoComplete, int cursor)
		{
			return ((IFindBoxListProvider)this).NearestMatchCore(code, explicitAutoComplete);
		}

		(string, bool) IFindBoxListProvider.NearestMatchCore(string code, bool explicitAutoComplete)
		{
			string result = "";
			var success = false;
			if (!string.IsNullOrEmpty(code))
			{
				ZQuery sQLFilter = new ZQuery();
				sQLFilter.AddToFilter(NZCClassificationSchema.U0_Tariff, SQLComparisonOperator.StartsWith, code.Trim());
				sQLFilter.AddFilterAndZSQLParameterCollection("len(" + NZCClassification.Schema.U0_Tariff + ") = 14", null);
				sQLFilter.AddToFilter(NZCClassificationSchema.U0_DateActiveTo, SQLComparisonOperator.Equal, null);
				sQLFilter.OrderBy = NZCClassificationSchema.U0_Tariff.Name + OrderByClause.Ascending;

				var match = Factory.LoadTop1<NZCClassification>(sQLFilter);
				if (match != null)
				{
					result = match.U0_Tariff;
					success = true;
				}
			}
			return (result, success);
		}

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			return NZCClassification.GetDescriptionForCompleteCode(Factory, code);
		}

		IBusinessObjectCollection IFindBoxListProvider.List
		{
			get { throw new ApplicationException("IFindBoxListProvider.List should not be referred to"); }
		}

		string IFindBoxListProvider.CodeFromPrimaryKey(ZGuid pK)
		{
			throw new NotSupportedException();
		}

		string IFindBoxListProvider.DescriptionFromPrimaryKey(ZGuid pK)
		{
			throw new NotSupportedException();
		}

		ZGuid IFindBoxListProvider.PrimaryKeyFromCode(string code)
		{
			throw new NotSupportedException();
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCode(string code)
		{
			throw new NotSupportedException();
		}

		BusinessObject IFindBoxListProvider.GetBusinessObjectFromCodeWithoutFilter(string code)
		{
			throw new NotSupportedException();
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCode(string code)
		{
			throw new NotSupportedException();
		}

		IEnumerable<BusinessObject> IFindBoxListProvider.GetBusinessObjectsFromCodeWithoutFilter(string code)
		{
			throw new NotSupportedException();
		}

		ICodeDescription IFindBoxListProvider.GetCustomCodeDescription(BusinessObject bizo)
			=> bizo;

		bool IFindBoxListProvider.AutoCompleteOnCommit
		{
			get { return false; }
		}

		#endregion
	}
}
