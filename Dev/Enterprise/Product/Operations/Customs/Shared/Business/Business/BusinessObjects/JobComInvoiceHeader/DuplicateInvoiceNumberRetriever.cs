using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class DuplicateInvoiceNumberRetriever
	{
		public DuplicateInvoiceNumberRetriever(BusinessObjectFactory factory, bool isDrawback)
		{
			Argument.NotNull(factory, "factory");
			this.factory = factory;
			this.isDrawback = isDrawback;
		}
		readonly BusinessObjectFactory factory;
		readonly bool isDrawback;

		internal ZString RetrieveDuplicateJobNumbers(BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoiceHeader, ZGuid organisation)
		{
			ZString result = ZString.Empty;
			if (declaration.Branch != null && !organisation.IsEmpty)
			{
				result = GetJobNumbersWithDuplicateInvoiceNumber(declaration.PK, invoiceHeader.PK, new ZString[] { declaration.JE_MessageType }, declaration.Company, invoiceHeader.JZ_InvoiceNumber, organisation);
			}
			return result;
		}

		public ZString RetrieveDuplicateJobNumbers(BaseJobDeclaration declaration, BaseJobComInvoiceHeader invoiceHeader, ZString[] messageTypes)
		{
			ZString result = ZString.Empty;
			if (declaration.Branch != null)
			{
				result = GetJobNumbersWithDuplicateInvoiceNumber(declaration.PK, invoiceHeader.PK, messageTypes, declaration.Company, invoiceHeader.JZ_InvoiceNumber, ZGuid.Empty);
			}
			return result;
		}

		public ZString GetJobNumbersWithDuplicateInvoiceNumber(ZGuid declarationPK, ZGuid invoicePK, ZString[] messageTypes, GlbCompany company, ZString invoiceNumber, ZGuid organisation)
		{
			var decQuery = new ZQuery();
			var branches = company.Branches.GetPKs();

			// company
			{
				var branchesInvoiceQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_GB, branches);
				branchesInvoiceQuery.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_JE, null);
				var branchesQuery = new ZQuery(JobDeclarationSchema.JE_GB, branches);
				branchesQuery.AddToFilter(branchesInvoiceQuery, JoinCondition.Or);
				decQuery.AddToFilter(branchesQuery);
			}

			// message type
			{
				var messageTypeInvoiceQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_StandAloneInvoiceDirection, messageTypes);
				messageTypeInvoiceQuery.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_JE, null);
				var messageTypeQuery = new ZQuery(JobDeclarationSchema.JE_MessageType, messageTypes);
				messageTypeQuery.AddToFilter(messageTypeInvoiceQuery, JoinCondition.Or);
				decQuery.AddToFilter(messageTypeQuery);
			}

			if (declarationPK.IsValid)
			{
				var notSameQuery = new ZQuery(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declarationPK);
				notSameQuery.AddToFilter(JoinCondition.Or, JobComInvoiceHeaderSchema.JZ_JE, null);
				decQuery.AddToFilter(notSameQuery);
			}

			if (invoicePK.IsValid)
			{
				decQuery.AddToFilter(JobComInvoiceHeaderSchema.PK, SQLComparisonOperator.NotEqual, invoicePK);
			}

			decQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_GroupInvoice, ZBool.False);
			decQuery.AddToFilter(JobComInvoiceHeaderSchema.JZ_InvoiceNumber, invoiceNumber);

			if (!organisation.IsEmpty)
			{
				if (isDrawback)
				{
					decQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, organisation);
				}
				else
				{
					ZQuery supplierQuery = new ZQuery(JobDeclarationSchema.JE_OH_Supplier, organisation);
					supplierQuery.AddToFilter(JoinCondition.And, JobComInvoiceHeaderSchema.JZ_OH_Supplier, null);

					ZQuery invoiceSupplierQuery = new ZQuery(JobComInvoiceHeaderSchema.JZ_OH_Supplier, organisation);
					supplierQuery.AddToFilter(invoiceSupplierQuery, JoinCondition.Or);
					decQuery.AddToFilter(supplierQuery);
				}
			}

			const string refColumnName = "DecReference";

			string sqlStatement = FormattableString.Invariant($@"
				SELECT TOP 10
					COALESCE({JobDeclarationSchema.JE_DeclarationReference.Name}, 'STANDALONE INVOICE') AS {refColumnName}
				FROM dbo.JobComInvoiceHeader
				LEFT JOIN dbo.JobDeclaration ON {JobComInvoiceHeaderSchema.JZ_JE.Name} = {JobDeclarationSchema.PK.Name}
				WHERE {decQuery.ParameterisedText.ParameterisedQueryText}
				GROUP BY {JobDeclarationSchema.JE_DeclarationReference.Name}
				ORDER BY {JobDeclarationSchema.JE_DeclarationReference.Name} DESC
			");

			var result = new List<string>();
			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(sqlStatement, decQuery.Params);
			foreach (BusinessObject bizO in collection)
			{
				result.Add(bizO[refColumnName].ToString());
			}
			return new ZStringBuilder(result).ToStringWithDelimiterBetweenAppends(", ");
		}
	}
}
