using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Customs.Business
{
	public class BondedWarehouseTransactionLine : BondedWarehouseLineProblemProvider, IWhsBondedWarehouseTransactionLine
	{
		public BondedWarehouseTransactionLine(CusEntryLine entryLine)
		{
			fCusEntryLine = entryLine;
			fInvoiceLine = fCusEntryLine.RandomLine;
		}

		public BondedWarehouseTransactionLine(BaseJobComInvoiceLine invoiceLine)
		{
			fInvoiceLine = invoiceLine;
			fCusEntryLine = invoiceLine.CusEntryLine ?? invoiceLine.Factory.GetNull<CusEntryLine>();
		}

		// new for correct type in subclasses
		protected CusEntryLine EntryLine
		{
			get { return fCusEntryLine; }
		}
		readonly CusEntryLine fCusEntryLine;

		// new for correct type in subclasses
		protected BaseJobComInvoiceLine InvoiceLine
		{
			get { return fInvoiceLine; }
		}
		readonly BaseJobComInvoiceLine fInvoiceLine;

		// new for correct type in subclasses
		protected BaseJobDeclaration Declaration
		{
			get { return fInvoiceLine.Declaration; }
		}

		public ZGuid UniqueKey
		{
			get { return InvoiceLine.JI_BondedWarehouseLineKey; }
		}

		public IMoney TILV
		{
			get { return GetTILVCore(); }
		}

		protected virtual Money GetTILVCore()
		{
			return Money.Empty;
		}

		public IRefCountry CountryOfOrigin
		{
			get { return CountryOfOriginCore; }
		}

		protected virtual RefCountry CountryOfOriginCore
		{
			get { return InvoiceLine.CountryOfOrigin; }
		}

		public ZDecimal Quantity
		{
			get { return InvoiceLine.JI_InvoiceQuantity; }
		}

		public ZString QuantityUnit
		{
			get { return InvoiceLine.JI_InvoiceUQ; }
		}

		public ZDecimal CustomsQuantity
		{
			get { return GetCustomsQuantityCore(); }
		}

		protected virtual ZDecimal GetCustomsQuantityCore()
		{
			return InvoiceLine.JI_CustomsQuantity;
		}

		public ZString CustomsQuantityUnit
		{
			get { return GetCustomsQuantityUnitCore(); }
		}

		protected virtual ZString GetCustomsQuantityUnitCore()
		{
			return InvoiceLine.JI_CustomsUnitQty.Left(3);
		}

		public ZDecimal CustomsSecondQuantity
		{
			get { return GetCustomsSecondQuantityCore(); }
		}

		protected virtual ZDecimal GetCustomsSecondQuantityCore()
		{
			return InvoiceLine.JI_CustomsSecondQuantity;
		}

		public ZString CustomsSecondQuantityUnit
		{
			get { return GetCustomsSecondQuantityUnitCore(); }
		}

		protected virtual ZString GetCustomsSecondQuantityUnitCore()
		{
			return InvoiceLine.JI_CustomsSecondUnitQty.Left(4);
		}

		public ZDecimal CustomsThirdQuantity
		{
			get { return GetCustomsThirdQuantityCore(); }
		}

		protected virtual ZDecimal GetCustomsThirdQuantityCore()
		{
			return InvoiceLine.JI_CustomsThirdQuantity;
		}

		public ZString CustomsThirdQuantityUnit
		{
			get { return GetCustomsThirdQuantityUnitCore(); }
		}

		protected virtual ZString GetCustomsThirdQuantityUnitCore()
		{
			return InvoiceLine.JI_CustomsThirdUnitQty.Left(4);
		}

		public ZDecimal BondedWarehouseQuantity
		{
			get { return InvoiceLine.JI_InvoiceQuantity; }
		}

		public ZString BondedWarehouseQuantityUnit
		{
			get { return InvoiceLine.JI_InvoiceUQ; }
		}

		public ZString PartAttrib1
		{
			get { return InvoiceLine.JI_PartAttrib1; }
		}

		public ZString PartAttrib2
		{
			get { return InvoiceLine.JI_PartAttrib2; }
		}

		public ZString PartAttrib3
		{
			get { return InvoiceLine.JI_PartAttrib3; }
		}

		public ZString SerialNumber
		{
			get { return InvoiceLine.JI_SerialNumber; }
		}

		public IOrgSupplierPart Product
		{
			get { return InvoiceLine.Part; }
		}

		public bool UseEntryKeyFromEntry = true;

		public ZShort EntryLineNumber
		{
			get
			{
				if (Declaration.IsWarehousedByExternalAgent || !UseEntryKeyFromEntry)
				{
					return GetEntryLineNumberFromInvoiceLine();
				}
				else
				{
					return EntryLine.CL_LineNumber;
				}
			}
		}

		protected virtual ZShort GetEntryLineNumberFromInvoiceLine()
		{
			return ZShort.Zero;
		}

		public ZDecimal ValueForDuty
		{
			get { return InvoiceLine.JI_CustomsValue; }
		}

		public ZString EntryKey
		{
			get
			{
				if (Declaration.IsWarehousedByExternalAgent || !UseEntryKeyFromEntry)
				{
					return GetEntryKeyFromInvoiceLine();
				}
				else
				{
					return EntryKeyCore;
				}
			}
		}

		protected virtual ZString EntryKeyCore
		{
			get { return EntryLine.Header.EntryNumber; }
		}

		protected virtual ZString GetEntryKeyFromInvoiceLine()
		{
			return "";
		}

		public ZDateTime EntryDate
		{
			get
			{
				if (Declaration != null && Declaration.IsWarehousedByExternalAgent)
				{
					return GetEntryDateForWEA();
				}
				else
				{
					return EntryLine.Header.ClearanceDate;
				}
			}
		}

		protected virtual ZDateTime GetEntryDateForWEA()
		{
			throw new NotSupportedException("Override GetEntryDateForWEA()");
		}

		ZString IWhsBondedWarehouseTransactionLine.AddInfo
		{
			get { return AddInfoString; }
		}

		protected virtual ZString AddInfoString
		{
			get { return ""; }
		}

		public ZShort OriginalEntryLineNumber
		{
			get { return ZShort.Zero; } // TODO remove
		}

		public ZString OriginalEntryKey
		{
			get { return ZString.Empty; } // TODO remove
		}

		public IOrgAddress Warehouse
		{
			get { return GetWarehouseCore(); }
		}

		protected virtual OrgAddress GetWarehouseCore()
		{
			return Declaration.WarehouseDocAddress.Address;
		}
	}
}
