using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class ExternalWarehouseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public ExternalWarehouseMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => "External Warehouse Message";

		protected override string ApplicationCodeCore => EDIInterchange.ApplicationCodes.SouthAfricanTransactionOrders;

		void LogError(EDIMessage message, ZString logMessage)
		{
			Logger.Log(Integration.LogType.Error, logMessage);
			var log = message.Logs.AddNew();
			message.Logs.AddNew(Events.ErrorReport, logMessage);
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var messageData = GetMessageData(message);

			if (messageData != null)
			{
				errors = ValidateMessage(message.Factory, messageData);

				if (!errors.Any())
				{
					Logger.Log(Integration.LogType.Information, Res.GetString("356612CF-78A2-402F-86CA-9E1E162499FF", "Successfully validated Batch {0}.", messageData.Batch));
					Logger.Log(Integration.LogType.Information, Res.GetString("E8573BCD-24B1-48B3-9D7B-F6EC3B769D3A", "Processing message {0}.", message.EM_MessageNum));
					var factory = message.Factory;
					var batch = factory.New<CusWHSOperatorTransactionBatch>();
					batch.WOB_GC_Company = message.Interchange.Branch.Company.PK;
					batch.WOB_Batch = messageData?.Batch;
					batch.WOB_OA_Warehouse = GetWarehouseAddress(factory, messageData?.OrganizationAddress);
					message.EM_LinkUniqueID = batch.PK;
					message.EM_LinkTable = CusWHSOperatorTransactionBatchSchema.Constants.TableName;

					foreach (var warehouseTransaction in messageData?.ExternalWarehouseTransactionCollection)
					{
						var transaction = factory.New<CusWHSOperatorTransaction>();
						transaction.WOT_WOB_CusWHSTransactionBatch = batch.PK;
						transaction.WOT_TransactionType = warehouseTransaction.TransactionType;
						transaction.WOT_ExportType = warehouseTransaction.ExportType;
						transaction.WOT_TransactionDate = warehouseTransaction.TransactionDate;
						transaction.WOT_OwnerReference = warehouseTransaction.OwnerReference;
						transaction.WOT_LineReference = warehouseTransaction.LineReference;
						transaction.WOT_OP_Product = GetProduct(warehouseTransaction.ProductCode, warehouseTransaction.Owner)?.PK ?? ZGuid.Empty;
						transaction.WOT_OH_ProductOwner = GetOwner(warehouseTransaction.Owner)?.PK ?? ZGuid.Empty;
						transaction.WOT_Quantity = warehouseTransaction.Quantity;
						transaction.WOT_TotalValue = warehouseTransaction.TotalValue;
						transaction.WOT_RX_NKCurrency = warehouseTransaction.Currency;
						transaction.WOT_Status = warehouseTransaction.TransactionType == WarehouseOperatorTransactionTypeList.Codes.REC ? WarehouseOperatorTransactionStatusList.Codes.QUE : WarehouseOperatorTransactionStatusList.Codes.VAL;
						ZBool.TryParse(warehouseTransaction.IsFinal, out ZBool isFinal);
						transaction.WOT_IsFinal = isFinal;
						if (ZInt.TryParse(warehouseTransaction.BatchLineno, out ZInt lineNo))
						{
							transaction.WOT_BatchLineNo = lineNo;
						}

						message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
						Logger.Log(Integration.LogType.Information, Res.GetString("AD7FD06B-F62E-4260-8662-9831D003EDDA", "Message {0} processed successfully.", message.EM_MessageNum));
					}
				}
				else
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
					errors.Distinct().ForEach(x => LogError(message, x));
					LogError(message, Res.GetString("2B449F59-8808-4FB0-BC87-026F9C9D2CF1", "Message {0} cannot be processed.", message.EM_MessageNum));
				}
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				LogError(message, Res.GetString("BE04A5A5-3C0C-44D4-A354-751FAC9796FF", "Message {0} cannot be processed. Error reading message text", message.EM_MessageNum));
			}
		}

		protected ZGuid GetWarehouseAddress(BusinessObjectFactory factory, WarehouseOrganizationAddress address)
		{
			ZGuid warehouseAddressPK = ZGuid.Empty;

			if (address != null)
			{
				var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_Code, address?.AddressShortCode);

				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, address?.OrganizationCode);

				orgAddressQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

				warehouseAddressPK = factory.Load<OrgAddress>(orgAddressQuery).FirstOrDefault()?.PK ?? ZGuid.Empty;
			}

			return warehouseAddressPK;
		}

		internal ExternalWarehouseBatch GetMessageData(EDIMessage message)
		{
			try
			{
				var externalWarehouseBatch = XElement.Parse(message.EM_MessageText);
				var batch = new ExternalWarehouseBatch();
				batch.Batch = externalWarehouseBatch.Element("Batch")?.Value ?? ZString.Empty;
				GetOrganizationAddress(externalWarehouseBatch, batch);

				var externalWarehouseTransactions = externalWarehouseBatch.Element("ExternalWarehouseTransactionCollection").Elements();
				batch.ExternalWarehouseTransactionCollection = new List<ExternalWarehouseTransaction>();

				foreach (var externalWarehouseTransaction in externalWarehouseTransactions)
				{
					var transaction = GetWarehouseTransaction(externalWarehouseTransaction);
					batch.ExternalWarehouseTransactionCollection.Add(transaction);
				}

				return batch;
			}
			catch (Exception ex)
			{
				var errorMessage = "Invalid message format. Cannot Process." + System.Environment.NewLine +
					ex.Message;
				LogError(message, errorMessage);
				return null;
			}
		}

		void GetOrganizationAddress(XElement externalWarehouseBatch, ExternalWarehouseBatch batch)
		{
			var organizationAddress = externalWarehouseBatch.Element("OrganizationAddress");
			var addressType = organizationAddress.Element("AddressType")?.Value ?? ZString.Empty;
			var addressShortCode = organizationAddress.Element("AddressShortCode")?.Value ?? ZString.Empty;
			var organizationCode = organizationAddress.Element("OrganizationCode")?.Value ?? ZString.Empty;
			batch.OrganizationAddress = new WarehouseOrganizationAddress
			{
				OrganizationCode = organizationCode,
				AddressType = addressType,
				AddressShortCode = addressShortCode
			};
		}

		ExternalWarehouseTransaction GetWarehouseTransaction(XElement externalWarehouseTransaction)
		{
			var transaction = new ExternalWarehouseTransaction();
			transaction.BatchLineno = externalWarehouseTransaction.Element("BatchLineno")?.Value ?? ZString.Empty;
			transaction.TransactionType = externalWarehouseTransaction.Element("TransactionType")?.Value ?? ZString.Empty;
			transaction.ExportType = externalWarehouseTransaction.Element("ExportType")?.Value ?? ZString.Empty;
			ZDateTime.TryParseExact(externalWarehouseTransaction.Element("TransactionDate")?.Value ?? ZString.Empty, out var transactionDate, "yyyy-MM-dd");
			transaction.TransactionDate = transactionDate.Date;
			transaction.OwnerReference = externalWarehouseTransaction.Element("OwnerReference")?.Value ?? ZString.Empty;
			transaction.LineReference = externalWarehouseTransaction.Element("LineReference")?.Value ?? ZString.Empty;
			transaction.ProductCode = externalWarehouseTransaction.Element("ProductCode")?.Value ?? ZString.Empty;
			transaction.Owner = externalWarehouseTransaction.Element("Owner")?.Value ?? ZString.Empty;
			ZInt.TryParse(externalWarehouseTransaction.Element("Quantity")?.Value ?? ZString.Empty, out var quantity);
			transaction.Quantity = quantity;
			ZDecimal.TryParse(externalWarehouseTransaction.Element("TotalValue")?.Value ?? ZString.Empty, out var totalValue);
			transaction.TotalValue = totalValue;
			transaction.Currency = externalWarehouseTransaction.Element("Currency")?.Value ?? ZString.Empty;
			transaction.CountryOrigin = externalWarehouseTransaction.Element("CountryOrigin")?.Value ?? ZString.Empty;
			transaction.IsFinal = externalWarehouseTransaction.Element("IsFinal")?.Value ?? ZString.Empty;
			return transaction;
		}

		List<ZString> ValidateMessage(BusinessObjectFactory factory, ExternalWarehouseBatch messageData)
		{
			var errors = new List<ZString>();

			ValidateBatch(factory, messageData.Batch, errors);
			ValidateWarehouse(factory, messageData.OrganizationAddress, errors);

			int lineCount = 1;
			foreach (var externalWarehouseTransaction in messageData.ExternalWarehouseTransactionCollection)
			{
				ValidateBatchLineNo(externalWarehouseTransaction.BatchLineno, lineCount, errors);
				ValidateProduct(externalWarehouseTransaction.ProductCode, externalWarehouseTransaction.Owner, errors);
				ValidateOwnerReference(externalWarehouseTransaction.OwnerReference, externalWarehouseTransaction.BatchLineno, errors);

				lineCount++;
			}
			ValidateUniqueBatchLineNo(messageData.ExternalWarehouseTransactionCollection, errors);

			return errors;
		}

		void ValidateBatch(BusinessObjectFactory factory, string batch, List<ZString> errors)
		{
			if (batch.IsNullOrEmpty())
			{
				errors.Add("Batch number is missing.");
			}
			else
			{
				var batchQuery = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
				batchQuery.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_Batch, batch);
				var batchFound = factory.Load<CusWHSOperatorTransactionBatch>(batchQuery).FirstOrDefault();

				if (batchFound != null)
				{
					errors.Add(ZString.Format("Duplicate Batch name {0} – a batch with this name has already been added.", batch));
				}
			}
		}

		void ValidateWarehouse(BusinessObjectFactory factory, WarehouseOrganizationAddress address, List<ZString> errors)
		{
			if (GetWarehouseAddress(factory, address).IsEmpty)
			{
				errors.Add(ZString.Format("Cannot resolve to a warehouse with name {0} and address {1}", address?.OrganizationCode, address?.AddressShortCode));
			}
		}

		void ValidateBatchLineNo(string batchLineno, int lineCount, List<ZString> errors)
		{
			if (batchLineno.IsNullOrEmpty())
			{
				errors.Add(ZString.Format("Batch Line No for record number {0} is missing.", lineCount));
			}
			else
			{
				ZInt.TryParse(batchLineno, out var lineNo);

				if (lineNo == 0)
				{
					errors.Add(ZString.Format("Batch Line No for record number {0} is invalid.", lineCount));
				}
			}
		}

		void ValidateOwnerReference(string ownerReference, string batchLineNo, List<ZString> errors)
		{
			if (ownerReference.IsNullOrEmpty())
			{
				errors.Add(ZString.Format("Owner Reference for Line Number {0} is missing.", batchLineNo));
			}
		}

		void ValidateProduct(string productCode, string owner, List<ZString> errors)
		{
			var product = GetProduct(productCode, owner);

			bool validProduct = product != null;

			if (!validProduct)
			{
				errors.Add(ZString.Format("Product code {0} with owner {1} cannot be found.", productCode, owner));
			}
		}

		void ValidateUniqueBatchLineNo(List<ExternalWarehouseTransaction> transactions, List<ZString> errors)
		{
			var duplicatedLineNumbers = transactions.GroupBy(tx => tx.BatchLineno)
				.Where(group => !group.Key.IsNullOrEmpty() && group.Count() > 1)
				.ToDictionary(group => group.Key);
			if (duplicatedLineNumbers.Count > 0)
			{
				var lineCount = 1;
				foreach (var tx in transactions)
				{
					if (duplicatedLineNumbers.TryGetValue(tx.BatchLineno, out var duplicatedTransactions) && tx != duplicatedTransactions.First())
					{
						errors.Add(ZString.Format("Batch Line No {0} for record number {1} is duplicated.", tx.BatchLineno, lineCount));
					}
					lineCount++;
				}
			}
		}

		OrgHeader GetOwner(string ownerCode)
		{
			var ownerQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			ownerQuery.AddToFilter(OrgHeaderSchema.OH_Code, ownerCode);
			return new BusinessObjectFactory().Load<OrgHeader>(ownerQuery).FirstOrDefault();
		}

		OrgSupplierPart GetProduct(string productCode, string owner)
		{
			var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCode);
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, true);

			var orgPartRelationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			orgPartRelationSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new ZString[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgPartRelationSchema.OU_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, owner);
			orgPartRelationSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			productQuery.AddSubQuery(orgPartRelationSubQuery, JoinCondition.And);

			var product = new BusinessObjectFactory().Load<OrgSupplierPart>(productQuery).FirstOrDefault();
			return product;
		}

		List<ZString> errors = new List<ZString>();
	}

	public class ExternalWarehouseBatch
	{
		public string Batch { get; set; }

		public WarehouseOrganizationAddress OrganizationAddress { get; set; }

		public List<ExternalWarehouseTransaction> ExternalWarehouseTransactionCollection { get; set; } = new List<ExternalWarehouseTransaction>();
	}

	public class WarehouseOrganizationAddress
	{
		public string AddressType { get; set; }

		public string AddressShortCode { get; set; }

		public string OrganizationCode { get; set; }
	}

	public class ExternalWarehouseTransaction
	{
		public string BatchLineno { get; set; }

		public string TransactionType { get; set; }

		public string ExportType { get; set; }

		public ZDate TransactionDate { get; set; }

		public string OwnerReference { get; set; }

		public string LineReference { get; set; }

		public string ProductCode { get; set; }

		public string Owner { get; set; }

		public int Quantity { get; set; }

		public decimal TotalValue { get; set; }

		public string Currency { get; set; }

		public string CountryOrigin { get; set; }

		public string IsFinal { get; set; }
	}
}
