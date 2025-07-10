using System;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusProcedureFilterLookups : CommonFilterLookups
	{
		public ZZRefCusProcedureFilterLookups(ZZRefCusProcedureFilterStripBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		protected new ZZRefCusProcedureFilterStripBusinessObject FilterBizObj => (ZZRefCusProcedureFilterStripBusinessObject)base.FilterBizObj;

		public CodeDescriptionPairList CategoryCodeList() => GetCachedList(nameof(ZZRefCusProcedureFilterLookups.CategoryCodeList), (procedure) => procedure.ZZ6_Category, (data) => data.code);

		public CodeDescriptionPairList ProcedureCodeList() => GetCachedList(nameof(ZZRefCusProcedureFilterLookups.ProcedureCodeList), (procedure) => procedure.ZZ6_ProcedureCode, (data) => RefCusProcedure.GetProcedureDescription(Factory, data.dataGroupingCode, data.code));

		public CodeDescriptionPairList PreviousProcedureCodeList() => GetCachedList(nameof(ZZRefCusProcedureFilterLookups.PreviousProcedureCodeList), (procedure) => procedure.ZZ6_PreviousProcedureCode, (data) => RefCusProcedure.GetProcedureDescription(Factory, data.dataGroupingCode, data.code));

		public CodeDescriptionPairList ConcessionCodeList() => GetCachedList(nameof(ZZRefCusProcedureFilterLookups.ConcessionCodeList), (procedure) => procedure.ZZ6_Concession, (data) => RefCusProcedure.GetConcessionDescription(Factory, data.dataGroupingCode, data.code));

		public CodeDescriptionPairList GroupList() => GetCachedList(nameof(ZZRefCusProcedureFilterLookups.GroupList), (procedure) => procedure.ZZ6_Group, (data) => data.code);

		public CodeDescriptionPairList ShipmentTypeList() => GetCachedList(nameof(ZZRefCusProcedureFilterLookups.ShipmentTypeList), (procedure) => procedure.ZZ6_ShipmentType, (data) => data.code);

		CodeDescriptionPairList GetCachedList(string listName, Func<RefCusProcedure, ZString> getCode, Func<(string dataGroupingCode, ZString code), string> getDescription)
		{
			var date = ZDateTime.Today;
			var dataGroupingCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return Factory.GetCachedValue($"BaseZZRefCusProcedureFilterLookups|{dataGroupingCode}|{date.ToShortDateString()}|{listName}", delegate
			{
				var result = new CodeDescriptionPairList();
				var procedures = RefCusProcedureCollection.LoadDistinctCodesForCountry(Factory, dataGroupingCode, date);
				CargoWise.Common.IEnumerableExtensions.ForEach(procedures.Select(x => getCode(x)).DistinctBy(code => code).OrderBy(code => code), code => result.AddPair(code, getDescription((dataGroupingCode, code))));
				return result;
			});
		}
	}
}
