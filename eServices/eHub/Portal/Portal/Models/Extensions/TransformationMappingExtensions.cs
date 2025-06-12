using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class TransformationMappingExtensions
	{

		public static List<TransformationMappingView> GetTransformationMappingViewList(this IeHubTransactionsContext _context, Guid setId)
		{
			var mappingList = _context.GetTransformationMappingList(setId);
			var mappingViewList = new List<TransformationMappingView>();

			for (int i = 0; i < mappingList.Count; i++)
			{
				mappingViewList.Add(new TransformationMappingView() { Mapping = mappingList[i], TransformationTypeName = UtilExtensions.FormatAsseblyName(_context.GetTransformationType(mappingList[i].TM_TT_PK).TT_TransformationType) });
			}

			return mappingViewList.ToList();
		}

		public static List<eHubTransformationMapping> GetTransformationMappingList(this IeHubTransactionsContext _context, Guid setId)
		{
			return (from m in _context.eHubTransformationMappings where m.TM_TS_PK == setId orderby m.TM_Order select m).ToList<eHubTransformationMapping>();
		}

		public static void DeleteTransformationMappingList(this IeHubTransactionsContext _context, Guid setId)
		{
			var list = (from m in _context.eHubTransformationMappings where m.TM_TS_PK == setId orderby m.TM_Order select m).ToList<eHubTransformationMapping>();
			foreach(var item in list)
			{
				_context.eHubTransformationMappings.DeleteObject(item);
			}
		}
	}
}