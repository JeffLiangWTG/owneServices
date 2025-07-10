using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Tested in GB")]
	public class DeclarationToInvoiceToLineAddInfoTextFilter : ModuleTextFilter
	{
		public DeclarationToInvoiceToLineAddInfoTextFilter(ZString description, ZString nameOfCustomPropertyInsideJIAddInfo)
			: base(description, (c, v) => GetDeclarationToInvoiceLineAddInfoQuery(c, v, nameOfCustomPropertyInsideJIAddInfo))
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get { return AddInfoModuleTextFilter.AllowedComparisonOperatorsForAddInfo; }
		}

		static ZQuery GetDeclarationToInvoiceLineAddInfoQuery(SQLComparisonOperator comparisonOperator, ZString value, string nameOfCustomProperty)
		{
			var query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			var headerSub = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			var lineSub = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
			var addinfoQuery = AddInfoFilterRepository.GetAddInfoQuery(comparisonOperator, value, JobComInvoiceLineSchema.JI_AddInfo, nameOfCustomProperty);
			lineSub.AddToFilter(addinfoQuery);
			headerSub.AddSubQuery(lineSub, JoinCondition.And);
			query.AddSubQuery(headerSub, JoinCondition.And);
			return query;
		}

		public override bool IsExpensiveQuery
		{
			get { return true; }
		}
	}
}
