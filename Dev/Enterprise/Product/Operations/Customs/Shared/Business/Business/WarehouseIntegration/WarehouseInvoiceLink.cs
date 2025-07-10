using System.Collections;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class WarehouseInvoiceLink : IInvoiceLink
	{
		public WarehouseInvoiceLink(BaseJobDeclaration declaration)
		{
			fDeclaration = declaration;
		}

		readonly BaseJobDeclaration fDeclaration;

		// new for correct type in subclasses
		protected BaseJobDeclaration Declaration
		{
			get { return fDeclaration; }
		}

		public ZBool IsExWarehouse
		{
			get { return Declaration.IsExWarehouse; }
		}

		public ZGuid DeclarationPK
		{
			get { return Declaration.PK; }
		}

		public IOrgHeader Importer
		{
			get { return Declaration.Importer; }
		}

		public ZString OwnersReference
		{
			get { return Declaration.JE_OwnerRef; }
		}

		public void NotifyOfOrderCreation()
		{
		}

		public void NotifyOfOrderCancellation()
		{
		}

		public void NotifyOfEntryCreation()
		{
		}

		public void NotifyOfEntryCancellation()
		{
		}

		BaseJobComInvoiceHeader GetFirstOrCreateNewInvoiceHeader()
		{
			if (Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count > 0)
			{
				return Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0];
			}
			else
			{
				return Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			}
		}

#if DEBUG
		internal
#endif
		class InvoiceLineMapper
		{
			public InvoiceLineMapper(IWhsBondedWarehouseTransactionLineCollection warehouseLines, BaseJobDeclaration declaration)
			{
				this.warehouseLines = warehouseLines;
				this.declaration = declaration;
				invoiceLines = declaration.InvoiceLines.ToArray<BaseJobComInvoiceLine>();
			}

			readonly BaseJobDeclaration declaration;
			readonly BaseJobComInvoiceLine[] invoiceLines;
			readonly IWhsBondedWarehouseTransactionLineCollection warehouseLines;

			public Hashtable MapByWarehouseLineToInvoiceLine
			{
				get
				{
					if (mapByWarehouseLine == null)
					{
						mapByWarehouseLine = new Hashtable();
						BuildMap();
					}
					return mapByWarehouseLine;
				}
			}
			Hashtable mapByWarehouseLine;

			protected void BuildMap()
			{
				Hashtable mapByKey = new Hashtable();
				foreach (IWhsBondedWarehouseTransactionLine warehouseLine in warehouseLines)
				{
					mapByKey[GetKey(warehouseLine)] = warehouseLine;
				}

				foreach (BaseJobComInvoiceLine invoiceLine in invoiceLines)
				{
					IWhsBondedWarehouseTransactionLine warehouseLine = (IWhsBondedWarehouseTransactionLine)mapByKey[GetKey(invoiceLine)];
					if (warehouseLine != null)
					{
						mapByWarehouseLine[warehouseLine] = invoiceLine;
					}
				}
			}

			string GetKey(BaseJobComInvoiceLine line)
			{
				return GetKey(((IBondedWarehouseTransactionLineProvider)line).TransactionLine);
			}

#if DEBUG
			internal
#endif
			string GetKey(IWhsBondedWarehouseTransactionLine line)
			{
				StringBuilder result = new StringBuilder();
				if (line.Product != null)
				{
					result.Append(line.Product.PK.ToStringKey());
				}

				result.Append(line.EntryKey);
				result.Append(line.EntryLineNumber);
				result.Append(line.Quantity);
				result.Append(line.QuantityUnit);
				if (line.Warehouse != null)
				{
					result.Append(line.Warehouse.PK.ToStringKey());
				}

				result.Append(line.PartAttrib1);
				result.Append(line.PartAttrib2);
				result.Append(line.PartAttrib3);
				result.Append(line.SerialNumber);
				result.Append(line.ValueForDuty.Round(2));
				return result.ToString();
			}

			Hashtable MapByInvoiceLineToTransactionLine
			{
				get
				{
					if (mapByInvoiceLine == null)
					{
						mapByInvoiceLine = new Hashtable();
						foreach (DictionaryEntry mapping in MapByWarehouseLineToInvoiceLine)
						{
							mapByInvoiceLine[mapping.Value] = mapping.Key;
						}
					}
					return mapByInvoiceLine;
				}
			}
			Hashtable mapByInvoiceLine;

			public void DeleteAutomationInvoiceLinesNotInMap()
			{
				var useBondedWarehouseAutomation = declaration.IsExBondAutomationEnabled;
				var isWHSUniversalXMLActive = declaration.IsWHSUniversalXMLActive;
				foreach (BaseJobComInvoiceLine line in invoiceLines)
				{
					if (((isWHSUniversalXMLActive && useBondedWarehouseAutomation) || (!isWHSUniversalXMLActive && line.UseBondedWarehouseAutomation)) && MapByInvoiceLineToTransactionLine[line] == null)
					{
						line.Delete();
					}
				}
			}
		}

		public void CreateOrUpdateInvoices(IWhsBondedWarehouseTransaction transaction)
		{
			if (transaction != null && transaction.HasErrors)
			{
				ProcessErrorsFromWHSSide(transaction);
			}
			else
			{
				if (transaction != null)
				{
					//IBondedWarehouseTransactionLine[] Lines = (IBondedWarehouseTransactionLine[])Transaction.Lines;
					InvoiceLineMapper mapper = new InvoiceLineMapper(transaction.Lines, Declaration);
					BaseJobComInvoiceHeader header = GetFirstOrCreateNewInvoiceHeader();

					IOrgAddress warehouseAddress = null;
					foreach (IWhsBondedWarehouseTransactionLine whsLine in transaction.Lines.OfType<IWhsBondedWarehouseTransactionLine>().OrderBy(x => x.EntryDate.ToShortDateString() + x.EntryKey + x.EntryLineNumber))
					{
						if (whsLine.Quantity > 0)
						{
							BaseJobComInvoiceLine line = (BaseJobComInvoiceLine)mapper.MapByWarehouseLineToInvoiceLine[whsLine];
							if (line == null)
							{
								line = header.JobComInvoiceLines.AddNew();
							}
							else
							{
								line.PartSyncManager.Refresh();
							}
							SetupInvoiceLineFromTransactionLine(whsLine, line);
							if (warehouseAddress == null)
							{
								warehouseAddress = whsLine.Warehouse;
							}
							AddErrorsAndWarnings(whsLine, line);
						}
					}

					mapper.DeleteAutomationInvoiceLinesNotInMap();

					header.JZ_InvoiceAmount = header.JZ_Calc_LinesEntered;
					header.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
					if (warehouseAddress != null && Declaration.IsWHSUniversalXMLActive && Declaration.WarehouseDocAddress.IsEmpty)
					{
						Declaration.WarehouseDocAddress.E2_OA_Address = warehouseAddress.PK;
					}

					if (transaction.Problems.HasWarnings)
					{
						Declaration.MessageInitiator.WarnUserAboutSomething(Res.GetString("5f6f6bf6-2c5d-4a62-b694-878f76f455ed", "Stock has been released from the bonded warehouse. However, there are some warnings/recommendations:\r\n{0}", transaction.Problems.WarningList.ToString()), Res.GetString("624a05c4-24cb-4802-9be9-fa8aeecf02ec", "There are warnings from the Bonded Warehouse..."));
					}
				}
			}
		}

		void ProcessErrorsFromWHSSide(IWhsBondedWarehouseTransaction transaction)
		{
			for (int i = 0; i < transaction.Lines.Count && i < Declaration.InvoiceLines.Count; i++)
			{
				AddErrorsAndWarnings(transaction.Lines[i], Declaration.InvoiceLines[i]);
			}
			if (transaction.Problems.ErrorList.Count > 0)
			{
				Declaration.MessageInitiator.WarnUserAboutSomething(Res.GetString("4e549a7e-85e9-475b-aa36-26b9e9e8d3c3", "Cannot release stock from the bonded warehouse as:\r\n{0}", transaction.Problems.ErrorList.ToString()), Res.GetString("165c167f-6dfe-4b59-abe2-ac610db355e9", "Release from bonded warehouse unsuccessful..."));
			}
			else
			{
				Declaration.MessageInitiator.WarnUserAboutSomething(Res.GetString("aff30ca3-ed03-4ab1-ae2e-c96f56d749c2", "Cannot release stock from the bonded warehouse. Check the errors on lines for details."), Res.GetString("39dc8367-57b5-4a66-a142-4c6c8193372e", "Release from bonded warehouse unsuccessful..."));
			}
		}

		protected virtual ZPropertyInfo GetEntryKeyPropertyInfo(BaseJobComInvoiceLine line)
		{
			return line.JI_AddInfoInfo; // override for more specific field
		}

		protected virtual ZPropertyInfo GetWarehousePropertyInfo(BaseJobComInvoiceLine line)
		{
			return line.JI_AddInfoInfo; // override for more specific field
		}

		protected virtual void AddErrorsAndWarnings(IWhsBondedWarehouseTransactionLine transactionLine, BaseJobComInvoiceLine invoiceLine)
		{
#if DEBUG
			using (invoiceLine.SuspendValidationTesting())
#endif
			{
				AddErrorsAndWarnings(transactionLine.PartAttrib1Problems, invoiceLine.JI_PartAttrib1Info);
				AddErrorsAndWarnings(transactionLine.PartAttrib2Problems, invoiceLine.JI_PartAttrib2Info);
				AddErrorsAndWarnings(transactionLine.PartAttrib3Problems, invoiceLine.JI_PartAttrib3Info);
				AddErrorsAndWarnings(transactionLine.SerialNumberProblems, invoiceLine.JI_SerialNumberInfo);
				AddErrorsAndWarnings(transactionLine.EntryKeyProblems, GetEntryKeyPropertyInfo(invoiceLine));
				AddErrorsAndWarnings(transactionLine.QuantityProblems, invoiceLine.JI_InvoiceQuantityInfo);
				AddErrorsAndWarnings(transactionLine.WarehouseProblems, GetWarehousePropertyInfo(invoiceLine));
				AddErrorsAndWarnings(transactionLine.BondedWarehouseQuantityProblems, invoiceLine.JI_InvoiceQuantityInfo);
			}
		}

		void AddErrorsAndWarnings(NotificationCollection notifications, ZPropertyInfo info)
		{
			if (notifications != null && notifications.ErrorList != null)
			{
				foreach (string error in notifications.ErrorList)
				{
					if (!info.HasError(error))
					{
						info.AddError(error);
					}
				}
			}

			if (notifications != null && notifications.WarningList != null)
			{
				foreach (string warning in notifications.WarningList)
				{
					if (!info.HasWarning(warning))
					{
						info.AddWarning(warning);
					}
				}
			}
		}

		protected virtual void SetupInvoiceLineFromTransactionLine(IWhsBondedWarehouseTransactionLine transactionLine, BaseJobComInvoiceLine invoiceLine)
		{
			if (transactionLine.Product != null)
			{
				invoiceLine.JI_PartNo = transactionLine.Product.OP_PartNum;
			}

			using (new CopyingOperation(invoiceLine))
			{
				invoiceLine.JI_LinePrice = transactionLine.ValueForDuty;
				if (!transactionLine.BondedWarehouseQuantity.IsEmpty && transactionLine.Quantity.IsEmpty)
				{
					invoiceLine.JI_InvoiceQuantity = transactionLine.BondedWarehouseQuantity;
					invoiceLine.JI_InvoiceUQ = transactionLine.BondedWarehouseQuantityUnit;
				}
				else
				{
					invoiceLine.JI_InvoiceQuantity = transactionLine.Quantity;
					invoiceLine.JI_InvoiceUQ = transactionLine.QuantityUnit;
				}
				invoiceLine.JI_CustomsQuantity = transactionLine.CustomsQuantity;
				invoiceLine.JI_CustomsUnitQty = transactionLine.CustomsQuantityUnit;
				invoiceLine.JI_PartAttrib1 = transactionLine.PartAttrib1;
				invoiceLine.JI_PartAttrib2 = transactionLine.PartAttrib2;
				invoiceLine.JI_PartAttrib3 = transactionLine.PartAttrib3;
				invoiceLine.JI_SerialNumber = transactionLine.SerialNumber;

				if (transactionLine.CountryOfOrigin != null)
				{
					invoiceLine.JI_CountryOfOrigin = transactionLine.CountryOfOrigin.RN_Code;
				}
			}
		}

		public ZString EntryKeyTitle
		{
			get { return EntryKeyTitleCore; }
		}

		protected virtual ZString EntryKeyTitleCore
		{
			get { return (NoResString)"Entry Key Reference"; }
		}

		internal void NotifyChangedToFromExWarehousing()
		{
			if (IsExWarehouseChanged != null)
			{
				IsExWarehouseChanged();
			}
		}
		public event IsExWarehouseChangedEvent IsExWarehouseChanged;
	}
}
