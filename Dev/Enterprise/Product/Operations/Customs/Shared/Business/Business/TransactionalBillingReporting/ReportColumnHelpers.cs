using System;
using System.Data;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.TransactionalBillingReporting
{
	abstract class ReportColumnHelper
	{
		public const int lengthOfReferenceFields = 50; // As per C : \Dev\Enterprise\Product\Core\Billing\Enterprise.Billing.Integration\Enterprise.Billing.Integration\BillingTransaction.xsd

		internal BillingTransaction GetTransaction(IDataReader reader, TransactionalBillingStatementGeneratorBillingAPI.ProductRegistrationSimple licenceKey)
		{
			var transaction = new BillingTransaction();
			PopulateSpecific(transaction, reader);
			PopulateCommon(transaction, reader, licenceKey);
			return transaction;
		}

		void PopulateCommon(BillingTransaction transaction, IDataReader reader, TransactionalBillingStatementGeneratorBillingAPI.ProductRegistrationSimple licenceKey)
		{
			transaction.ReportingSource = "ENT";
			transaction.PriceItemCode = PriceItemCode;
			transaction.BillableCount = 1;
			transaction.Category = "GBC";
			transaction.Branch = reader["Branch"].ToString();
			transaction.ClientNumber = licenceKey.SystemId;
			transaction.ClientID = licenceKey.EnterpriseCode + reader["Company"].ToString() + licenceKey.ServerCode;
			var dateString = reader[CreatedDateColumnName].ToString();
			DateTime date;
			if (DateTime.TryParse(dateString, out date))
			{
				transaction.ServiceOccuredUTC = date;
			}
		}

		protected abstract string PriceItemCode { get; }

		protected abstract void PopulateSpecific(BillingTransaction transaction, IDataReader reader);

		protected virtual string CreatedDateColumnName { get { return (NoResString)"Job Open Date"; } }
	}

	class CustomsEntriesHelper : ReportColumnHelper
	{
		protected override void PopulateSpecific(BillingTransaction transaction, IDataReader reader)
		{
			transaction.ClientStaffCode = reader["Job Opening Staff"].ToString();
			transaction.Reference1 = new ZString(reader["Job number"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference2 = new ZString(reader["Job type"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference3 = new ZString(reader["BGM Reference"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference4 = new ZString(reader["Entry Number"].ToString()).Left(lengthOfReferenceFields);
		}

		protected override string PriceItemCode
		{
			get { return "CUE"; }
		}
	}

	class AirWaybillHelper : ReportColumnHelper
	{
		protected override void PopulateSpecific(BillingTransaction transaction, IDataReader reader)
		{
			transaction.ClientStaffCode = reader["Job Opening Staff"].ToString();
			transaction.Reference1 = new ZString(reader["MAWB"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference2 = new ZString(reader["HAWB"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference3 = new ZString(reader["Application Code"].ToString()).Left(lengthOfReferenceFields);
		}

		protected override string PriceItemCode
		{
			get { return "AWB"; }
		}
	}

	class GenralHelper : ReportColumnHelper
	{
		protected override void PopulateSpecific(BillingTransaction transaction, IDataReader reader)
		{
			transaction.Reference1 = new ZString(reader["Direction"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference2 = new ZString(reader["Sender"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference3 = new ZString(reader["Recipient"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference4 = new ZString(reader["Message Purpose"].ToString()).Left(lengthOfReferenceFields);
			transaction.Reference5 = new ZString(reader["Interchange#"].ToString()).Left(lengthOfReferenceFields);
		}

		protected override string CreatedDateColumnName
		{
			get { return "CreatedTime"; }
		}

		protected override string PriceItemCode
		{
			get { return "GTM"; }
		}
	}
}
