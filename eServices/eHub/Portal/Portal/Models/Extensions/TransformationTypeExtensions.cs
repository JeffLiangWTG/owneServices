using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class TransformationTypeExtensions
	{
		public static eHubTransformationType GetTransformationType(this IeHubTransactionsContext _context, Guid id)
		{
			return (from t in _context.eHubTransformationTypes where t.TT_PK == id select t).First();
		}

		public static TransformationTypeView[] FindTransformationTypeViewList(this IeHubTransactionsContext _context, string text)
		{
			return (from t in _context.eHubTransformationTypes
					where t.TT_TransformationType.Contains(text)
					orderby t.TT_TransformationType
					select new TransformationTypeView { Id = t.TT_PK, Name = t.TT_TransformationType }).Distinct().ToArray();
		}

		public static TransformationTypeView[] FindTransformationTypeViewList(this IeHubTransactionsContext _context, Guid id)
		{
			var list = (from t in _context.eHubTransformationTypes
						where t.TT_DT_Source == id
						orderby t.TT_TransformationType
						select new TransformationTypeView { Id = t.TT_PK, Name = t.TT_TransformationType }).Distinct().ToArray();

			if (list.Length != 0) return list;

			list = (from t1 in _context.eHubTransformationTypes
					join t2 in _context.eHubTransformationTypes on t1.TT_DT_Source equals t2.TT_DT_Target
					where t2.TT_PK == id
					orderby t1.TT_TransformationType
					select new TransformationTypeView { Id = t1.TT_PK, Name = t1.TT_TransformationType }).Distinct().ToArray();

			return list;
		}
	}
}