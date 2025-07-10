using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProcedureCollection : ActiveBusinessObjectCollection<RefCusProcedure>, ICodeDescriptionPairList, IFindBoxListProvider
	{
		readonly ZString dataGroupingCode;

		ZString DataGroupingCode => string.IsNullOrEmpty(dataGroupingCode) ? GlbCompany.CurrentCompany.Country.Code : dataGroupingCode;

		public RefCusProcedureCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date)
			: this(factory, RefCusProcedure.Loader.GetFilter(dataGroupingCode, date))
		{
			this.dataGroupingCode = dataGroupingCode;
		}

		public RefCusProcedureCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date, ZString declarationType, ZString shipmentType)
			: this(factory, RefCusProcedure.Loader.GetFullFilter(dataGroupingCode, date, declarationType, shipmentType))
		{
			this.dataGroupingCode = dataGroupingCode;
		}

		RefCusProcedureCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{ }

		#region static

		public static RefCusProcedureCollection LoadCustomsProcedureCodesForCountryAndShipmentType(BusinessObjectFactory factory, string dataGroupingCode, string shipmentType, ZDateTime date)
		{
			var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, string.Empty);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, shipmentType);
			return new RefCusProcedureCollection(factory, query);
		}

		public static RefCusProcedureCollection LoadPreviousProceduresCodesForCountryShipmentTypeProcedureCode(BusinessObjectFactory factory, string dataGroupingCode, string shipmentType, string customsProcedureCode, ZDateTime date, bool emptyConcession = false, string category = null)
		{
			var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, customsProcedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, shipmentType);
			if (emptyConcession)
			{
				query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, SQLComparisonOperator.Equal, string.Empty);
			}
			if (category != null)
			{
				query.AddToFilter(RefCusProcedureSchema.ZZ6_Category, category);
			}

			return new RefCusProcedureCollection(factory, query);
		}

		public static RefCusProcedureCollection LoadCustomsProcedureCodesForCountry(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date)
		{
			var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, string.Empty);
			return new RefCusProcedureCollection(factory, query);
		}

		public static RefCusProcedureCollection LoadPreviousProceduresCodesForCountryProcedureCode(BusinessObjectFactory factory, ZString dataGroupingCode, ZString customsProcedureCode, ZDateTime date)
		{
			var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, customsProcedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.NotEqual, string.Empty);
			return new RefCusProcedureCollection(factory, query);
		}

		public static RefCusProcedureCollection LoadDistinctCodesForCountry(BusinessObjectFactory factory, ZString dataGroupingCode, ZDateTime date)
		{
			var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
			return new RefCusProcedureCollection(factory, query);
		}

		public static RefCusProcedureCollection LoadCustomsProcedureCodesForCountryAndShipmentTypeAndGroup(BusinessObjectFactory factory, string dataGroupingCode, string shipmentType, ZDateTime date, IEnumerable<string> groupPatterns)
		{
			var query = RefCusProcedure.Loader.GetFullFilter(dataGroupingCode, date, ZString.Empty, shipmentType);

			var subQuery = new ZQuery();
			groupPatterns.ForEach(x => subQuery.AddToFilter(new ZQuery(RefCusProcedureSchema.ZZ6_Group, SQLComparisonOperator.Contains, x), JoinCondition.Or));

			query.AddToFilter(subQuery, JoinCondition.And);
			return new RefCusProcedureCollection(factory, query);
		}

		public static RefCusProcedureCollection LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(BusinessObjectFactory factory, string dataGroupingCode, string shipmentType, string customsProcedureCode, string previousProcedureCode, ZDateTime date, string category = null)
		{
			var query = RefCusProcedure.Loader.GetFilter(dataGroupingCode, date);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, customsProcedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, SQLComparisonOperator.Equal, previousProcedureCode);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, SQLComparisonOperator.Contains, shipmentType);
			query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, SQLComparisonOperator.NotEqual, string.Empty);
			if (category != null)
			{
				query.AddToFilter(RefCusProcedureSchema.ZZ6_Category, category);
			}

			return new RefCusProcedureCollection(factory, query);
		}

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			foreach (RefCusProcedure cusProcedure in this)
			{
				if (cusProcedure.FullCodeCurrentPlusPreviousPlusConcession == code.ToString())
				{
					return true;
				}
			}

			return false;
		}

		public string GetDescriptionFromCode(string code)
		{
			var loader = new RefCusProcedure.Loader(Factory);
			var refCusProcedure = loader.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(code, DataGroupingCode, ZDateTime.Now);
			return refCusProcedure == null ? ZString.Empty : refCusProcedure.ZZ6_Description;
		}

		#endregion

		string IFindBoxListProvider.DescriptionFromCode(string code)
		{
			return (this as ICodeDescriptionPairList).GetDescriptionFromCode(code);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public static class FilterConstants
		{
			public const string ProcedureCode = "Procedure Code";
		}
	}
}
