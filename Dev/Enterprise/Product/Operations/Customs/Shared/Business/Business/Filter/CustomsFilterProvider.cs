using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CustomsFilterProvider : ICustomsFilterProvider
	{
		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobDeclaration attached to JobShipment
		/// Allows additional filtering to be added at the invoice line level
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		public ZDBOnlySubQuery GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobShipment(ZQuery additionalInvoiceLineFilter)
		{
			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.JE_JS);
			declarationSubQuery.AddSubQuery(GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(additionalInvoiceLineFilter), JoinCondition.And);
			return declarationSubQuery;
		}

		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobDeclaration attached to JobDeclaration
		/// Allows additional filtering to be added at the invoice line level
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		public ZDBOnlySubQuery GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(ZQuery additionalInvoiceLineFilter)
		{
			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddSubQuery(GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(additionalInvoiceLineFilter), JoinCondition.And);
			return declarationSubQuery;
		}

		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobComInvoiceHeader attached to JobDeclaration.
		/// Allows additional filtering to be added at the invoice line level.
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		public ZDBOnlySubQuery GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(ZQuery additionalInvoiceLineFilter)
		{
			ZDBOnlySubQuery headerSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			headerSubQuery.AddSubQuery(GetJobCommercialInvoiceQueryWithLineFilterAttachedToJobComInvoiceHeader(additionalInvoiceLineFilter), JoinCondition.And);
			return headerSubQuery;
		}

		/// <summary>
		/// Returns a DBOnlySubQuery filter starting at JobComInvoiceLine attached to JobComInvoiceHeader.
		/// Allows additional filtering to be added at the invoice line level.
		/// </summary>
		/// <param name="additionalInvoiceLineFilter"></param>
		/// <returns></returns>
		public ZDBOnlySubQuery GetJobCommercialInvoiceQueryWithLineFilterAttachedToJobComInvoiceHeader(ZQuery additionalInvoiceLineFilter)
		{
			ZDBOnlySubQuery lineSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
			lineSubQuery.AddToFilter(additionalInvoiceLineFilter);
			return lineSubQuery;
		}

		/// <summary>
		/// Returns a ZDBOnlyQuery filter starting at JobComInvoiceLine.
		/// Allows additional filtering on ProductCode at the invoice line level.
		/// </summary>
		/// <param name="operator"></param>
		/// <param name="lineProductCode"></param>
		public ZDBOnlyQuery GetInvoiceLineProductCodeQuery(SQLComparisonOperator @operator, ZString lineProductCode)
		{
			ZDBOnlyQuery partSubQuery = new ZDBOnlyQuery(typeof(BaseJobComInvoiceLine));
			partSubQuery.AddToFilter_PossiblyCommaSeparated(JobComInvoiceLineSchema.JI_PartNo, @operator, lineProductCode);
			return partSubQuery;
		}

		public ZDBOnlyQuery GetJobDeclarationFromEntryNumber(SQLComparisonOperator @operator, ZString value)
		{
			return EntryNumberQueryGenerator.GetEntryNumberQuery(@operator, value, GlbCompany.CurrentCompany.Country.Code) as ZDBOnlyQuery;
		}
	}
}
