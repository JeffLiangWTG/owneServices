using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsFilterProviderTest : TestCaseWithFactory
	{
		public void TestGetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobShipment()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery subQuery = FilterProvider.GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobShipment(PartQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			AssertNull(Factory.LoadTop1<ForwardingShipment>(query));
			invoiceLine.JI_PartNo = "33";
			Factory.Save();
			AssertEquals(shipment, Factory.LoadTop1<ForwardingShipment>(query));

			secondInvoiceHeader = secondDec.Invoices.AddNew();
			_ = secondInvoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			AssertEquals(shipment, Factory.LoadTop1<ForwardingShipment>(query));

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobShipment", JobDeclarationFromCommercialInvoiceQueryAttachedToJobShipmentLiteral, subQuery.LiteralTextADO);
			}
		}

		public void TestGetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			ZDBOnlySubQuery subQuery = FilterProvider.GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(PartQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			AssertNull(Factory.LoadTop1<BaseJobDeclaration>(query));
			invoiceLine.JI_PartNo = "33";
			Factory.Save();
			AssertEquals(dec, Factory.LoadTop1<BaseJobDeclaration>(query));

			secondInvoiceHeader = secondDec.Invoices.AddNew();
			_ = secondInvoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			AssertEquals(dec, Factory.LoadTop1<BaseJobDeclaration>(query));

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				AssertEquals("GetJobDeclarationFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration", JobDeclarationFromCommercialInvoiceQueryAttachedToJobDeclarationLiteral, subQuery.LiteralTextADO);
			}
		}

		public void TestGetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			ZDBOnlySubQuery subQuery = FilterProvider.GetJobComInvoiceHeaderFromCommercialInvoiceQueryWithLineFilterAttachedToJobDeclaration(PartQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			AssertNull(Factory.LoadTop1<BaseJobDeclaration>(query));
			invoiceLine.JI_PartNo = "33";
			Factory.Save();
			AssertEquals(dec, Factory.LoadTop1<BaseJobDeclaration>(query));

			secondInvoiceHeader = secondDec.Invoices.AddNew();
			_ = secondInvoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			AssertEquals(dec, Factory.LoadTop1<BaseJobDeclaration>(query));
		}

		public void TestGetJobCommercialInvoiceQueryWithLineFilterAttachedToJobComInvoiceHeader()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(BaseJobComInvoiceHeader));
			ZDBOnlySubQuery subQuery = FilterProvider.GetJobCommercialInvoiceQueryWithLineFilterAttachedToJobComInvoiceHeader(PartQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);
			AssertNull(Factory.LoadTop1<BaseJobComInvoiceHeader>(query));
			invoiceLine.JI_PartNo = "33";
			Factory.Save();
			AssertEquals(invoiceHeader, Factory.LoadTop1<BaseJobComInvoiceHeader>(query));

			secondInvoiceHeader = secondDec.Invoices.AddNew();
			_ = secondInvoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			AssertEquals(invoiceHeader, Factory.LoadTop1<BaseJobComInvoiceHeader>(query));
		}

		public void TestGetInvoiceLineProductCodeQuery()
		{
			ZDBOnlyQuery query = PartQuery;
			AssertNull(Factory.LoadTop1<BaseJobComInvoiceLine>(query));
			invoiceLine.JI_PartNo = "33";
			Factory.Save();
			AssertEquals(invoiceLine, Factory.LoadTop1<BaseJobComInvoiceLine>(PartQuery));

			secondInvoiceHeader = secondDec.Invoices.AddNew();
			_ = secondInvoiceHeader.JobComInvoiceLines.AddNew();
			Factory.Save();
			AssertEquals(invoiceLine, Factory.LoadTop1<BaseJobComInvoiceLine>(PartQuery));
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetShipment();
			SetDec();
			SetInvoiceHeader();
			SetInvoiceLine();
			SetSecondShipment();
			SetSecondDec();
			SetSecondInvoiceHeader();
			SetSecondInvoiceLine();
			Factory.Save();
		}

		void SetShipment()
		{
			shipment = Factory.New<ForwardingShipment>();
		}
		ForwardingShipment shipment;

		void SetSecondShipment()
		{
			secondShipment = Factory.New<ForwardingShipment>();
		}
		ForwardingShipment secondShipment;

		void SetDec()
		{
			dec = Factory.New<BaseJobDeclaration>();
			dec.JE_JS = shipment.PK;
		}
		BaseJobDeclaration dec;

		void SetSecondDec()
		{
			secondDec = Factory.New<BaseJobDeclaration>();
			secondDec.JE_JS = secondShipment.PK;
		}
		BaseJobDeclaration secondDec;

		void SetInvoiceHeader()
		{
			invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader = dec.Invoices.AddNew();
		}
		BaseJobComInvoiceHeader invoiceHeader;

		void SetSecondInvoiceHeader()
		{
			secondInvoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			secondInvoiceHeader = secondDec.Invoices.AddNew();
		}
		BaseJobComInvoiceHeader secondInvoiceHeader;

		void SetInvoiceLine()
		{
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		}
		BaseJobComInvoiceLine invoiceLine;

		void SetSecondInvoiceLine()
		{
			_ = secondInvoiceHeader.JobComInvoiceLines.AddNew();
		}

		static string JobDeclarationFromCommercialInvoiceQueryAttachedToJobShipmentLiteral
		{
			get
			{
				return string.Format(jobDeclarationFromCommercialInvoiceQueryLiteral,
					string.Format(jobComInvoiceHeaderFromCommercialInvoiceHeaderQueryLiteral, JobComInvoiceHeaderSchema.Constants.JZ_JE,
					string.Format(jobCommercialInvoiceQueryLiteral, JobComInvoiceLineSchema.Constants.JI_JZ, invoiceLineProductCodeQueryLiteral)));
			}
		}

		static string JobDeclarationFromCommercialInvoiceQueryAttachedToJobDeclarationLiteral
		{
			get
			{
				return string.Format(jobDeclarationFromCommercialInvoiceQueryLiteral,
					string.Format(jobComInvoiceHeaderFromCommercialInvoiceHeaderQueryLiteral, JobComInvoiceHeaderSchema.Constants.JZ_JE,
					string.Format(jobCommercialInvoiceQueryLiteral, JobComInvoiceLineSchema.Constants.JI_JZ, invoiceLineProductCodeQueryLiteral)));
			}
		}
		static readonly string jobDeclarationFromCommercialInvoiceQueryLiteral = $" IN (SELECT  FROM {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName} WHERE {JobDeclarationSchema.Constants.PK}{{0}})";

		static readonly string jobComInvoiceHeaderFromCommercialInvoiceHeaderQueryLiteral = $" IN (SELECT {{0}} FROM {JobComInvoiceHeaderSchema.Constants.SqlSchemaName}.{JobComInvoiceHeaderSchema.Constants.TableName} WHERE JZ_JE IS NOT NULL AND {JobComInvoiceHeaderSchema.Constants.PK}{{1}})";

		static readonly string jobCommercialInvoiceQueryLiteral = $" IN (SELECT {{0}} FROM {JobComInvoiceLineSchema.Constants.SqlSchemaName}.{JobComInvoiceLineSchema.Constants.TableName} WHERE {{1}})";

		static readonly string invoiceLineProductCodeQueryLiteral = JobComInvoiceLineSchema.Constants.JI_PartNo + " like '33%'";

		ZDBOnlyQuery PartQuery
		{
			get { return partQuery ?? (partQuery = FilterProvider.GetInvoiceLineProductCodeQuery(SQLComparisonOperator.StartsWith, new ZString("33"))); }
		}
		ZDBOnlyQuery partQuery;

		static CustomsFilterProvider FilterProvider
		{
			get { return filterProvider ?? (filterProvider = new CustomsFilterProvider()); }
		}
		[ThreadStatic]
		static CustomsFilterProvider filterProvider;
	}
}
