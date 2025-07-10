using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Customs.Business
{
	public class BondedWarehouseTransaction : IWhsBondedWarehouseTransaction
	{
		public BondedWarehouseTransaction(BaseJobDeclaration declaration)
		{
			this.Declaration = declaration;
		}

		// TODO - refactor interface to include both in and out entry numbers
		public bool UseEntryKeyFromEntry = true;

		// new this for correct typing in subclasses
		protected readonly BaseJobDeclaration Declaration;

		public IOrgHeader TransportCompany
		{
			get { return TransportCompanyCore; }
		}

		protected virtual OrgHeader TransportCompanyCore
		{
			get { return Declaration.DeliveryOrPickupCartageCo; }
		}

		#region LineProductionStrategies

		protected abstract class LineProductionStrategy
		{
			protected LineProductionStrategy(BondedWarehouseTransaction transaction)
			{
				this.Transaction = transaction;
			}

			public IWhsBondedWarehouseTransactionLineCollection Lines
			{
				get
				{
					if (lines == null)
					{
						lines = ObjectFactory.Get<IWhsBondedWarehouseTransactionLineCollection>();
						PopulateLines(lines);
					}
					return lines;
				}
			}
			IWhsBondedWarehouseTransactionLineCollection lines;

			protected abstract void PopulateLines(IWhsBondedWarehouseTransactionLineCollection result);

			protected bool ShouldLineBeIncluded(BaseJobComInvoiceLine invoiceLine)
			{
				return (Transaction.Declaration.IsExWarehouse && (Transaction.Declaration.IsWHSUniversalXMLActive || invoiceLine.UseBondedWarehouseAutomation)) || (Transaction.Declaration.IsImport && invoiceLine.IsGoingIntoBondedWarehouse && invoiceLine.Part != null && invoiceLine.JI_InvoiceQuantity > 0 && !invoiceLine.JI_InvoiceUQ.IsEmpty);
			}

			protected readonly BondedWarehouseTransaction Transaction;
		}

		protected class EntryLineProductionStrategy : LineProductionStrategy
		{
			public EntryLineProductionStrategy(BondedWarehouseTransaction transaction) : base(transaction) { }

			protected override void PopulateLines(IWhsBondedWarehouseTransactionLineCollection result)
			{
				foreach (CusEntryHeader header in Transaction.Declaration.CustomsEntryHeaders)
				{
					foreach (CusEntryLine line in header.MergedLines)
					{
						if (ShouldLineBeIncluded(line.RandomLine))
						{
							BondedWarehouseTransactionLine transactionLine = (BondedWarehouseTransactionLine)((IBondedWarehouseTransactionLineProvider)line).TransactionLine;
							transactionLine.UseEntryKeyFromEntry = Transaction.UseEntryKeyFromEntry;
							result.Add(transactionLine);
						}
					}
				}
			}
		}

		protected class InvoiceLineProductionStrategy : LineProductionStrategy
		{
			public InvoiceLineProductionStrategy(BondedWarehouseTransaction transaction) : base(transaction) { }

			protected override void PopulateLines(IWhsBondedWarehouseTransactionLineCollection result)
			{
				foreach (BaseJobComInvoiceLine line in Transaction.Declaration.InvoiceLines)
				{
					if (ShouldLineBeIncluded(line))
					{
						BondedWarehouseTransactionLine transactionLine = (BondedWarehouseTransactionLine)((IBondedWarehouseTransactionLineProvider)line).TransactionLine;
						transactionLine.UseEntryKeyFromEntry = Transaction.UseEntryKeyFromEntry;
						result.Add(transactionLine);
					}
				}
			}
		}

		#endregion

		protected LineProductionStrategy LineProduction
		{
			get
			{
				if (fLineProductionStrategy == null)
				{
					fLineProductionStrategy = new EntryLineProductionStrategy(this);
				}
				return fLineProductionStrategy;
			}
		}
		LineProductionStrategy fLineProductionStrategy;

		public void SetEntryLineMode()
		{
			fLineProductionStrategy = new EntryLineProductionStrategy(this);
		}

		public void SetInvoiceLineMode()
		{
			fLineProductionStrategy = new InvoiceLineProductionStrategy(this);
		}

		public IWhsBondedWarehouseTransactionLineCollection Lines
		{
			get { return LineProduction.Lines; }
		}

		IWhsWarehouseTransactionLineCollection IWhsWarehouseTransaction.Lines
		{
			get { return Lines; }
		}

		public ZDateTime Date
		{
			get
			{
				ZDateTime result = Declaration.CustomsEntryHeaders.LastEntryToClearDate;
				if (result.IsEmpty)
				{
					result = Declaration.JE_DateOfFirstArrival;
				}

				return result;
			}
		}

		public IOrgAddress Warehouse
		{
			get { return WarehouseAddressCore; }
		}

		// OBSOLETE
		protected virtual OrgAddress WarehouseAddressCore
		{
			get { return Declaration.WarehouseAddress; }
		}

		public IOrgHeader Client
		{
			get { return Declaration.Importer; }
		}

		public bool IsWarehousedByExternalAgent
		{
			get { return Declaration.IsWarehousedByExternalAgent; }
		}

		public ZString Reference
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		public ZGuid ExternalPK
		{
			get { return Declaration.PK; }
		}

		public IEnumerable<AdditionalReference> AdditionalReferences
		{
			get
			{
				ArrayList result = new ArrayList();

				ICodeDescriptionPairListWithDefaultCode docketReferences = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;

				if (docketReferences.ContainsCode("MAB"))
				{
					AddAddititionalReferenceIfNotEmpty(result, "MAB", Declaration.JE_MasterBill);
				}
				if (docketReferences.ContainsCode("VFN"))
				{
					AddAddititionalReferenceIfNotEmpty(result, "VFN", Declaration.JE_VoyageFlightNo);
				}
				if (docketReferences.ContainsCode("HSB"))
				{
					AddAddititionalReferenceIfNotEmpty(result, "HSB", Declaration.JE_HouseBill);
				}

				return (AdditionalReference[])result.ToArray(typeof(AdditionalReference));
			}
		}

		protected void AddAddititionalReferenceIfNotEmpty(ArrayList destination, ZString type, ZString reference)
		{
			if (!reference.IsEmpty)
			{
				destination.Add(new AdditionalReference(type, reference));
			}
		}

		public NotificationCollection Problems
		{
			get
			{
				if (fProblems == null)
				{
					fProblems = new NotificationCollection();
				}
				return fProblems;
			}
		}
		NotificationCollection fProblems;

		public bool HasErrors
		{
			get
			{
				bool result = Problems.HasErrors;
				if (!result && Lines != null)
				{
					foreach (IWhsBondedWarehouseTransactionLine line in Lines)
					{
						if (line.HasErrors)
						{
							result = true;
							break;
						}
					}
				}
				return result;
			}
		}

		public bool HasWarnings
		{
			get { throw new NotSupportedException("soon to be pulled out with base class"); }
		}
	}
}
