using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedWarehouseTransactionLine : WhsWarehouseTransactionLine, IWhsBondedWarehouseTransactionLine, ICloneable
	{
		#region Constructor

		public WhsBondedWarehouseTransactionLine()
		{
			TILV = Money.Empty;
		}

		public WhsBondedWarehouseTransactionLine(Integration.IWhsDocketLine docketLine)
			: this((WhsDocketLine)docketLine)
		{
		}

		public WhsBondedWarehouseTransactionLine(WhsDocketLine docketLine)
			: base(docketLine)
		{
			ExtractDataFromDocketLine(docketLine);
		}

		void ExtractDataFromDocketLine(WhsDocketLine docketLine)
		{
			UniqueKey = docketLine.PK;
			TILV = Money.Empty;

			if (docketLine.CustomsData != null)
			{
				var ratio = docketLine.WE_TransactionQuantity / docketLine.CustomsData.WB_BondedWhsQty;
				AddInfo = docketLine.CustomsData.WB_AddInfo;
				BondedWarehouseQuantity = Utilities.Round(docketLine.CustomsData.WB_BondedWhsQty * ratio, 4);
				BondedWarehouseQuantityUnit = docketLine.CustomsData.WB_BondedWhsUnitOfQty;
				CountryOfOrigin = docketLine.CustomsData.CountryOfOrigin;
				CustomsQuantity = Utilities.Round(docketLine.CustomsData.WB_CustomsQty * ratio, 5);
				CustomsQuantityUnit = docketLine.CustomsData.WB_CustomsUnitOfQty;
				CustomsSecondQuantity = docketLine.CustomsData.WB_CustomsSecondQuantity;
				CustomsSecondQuantityUnit = docketLine.CustomsData.WB_CustomsSecondUnitQty;
				CustomsThirdQuantity = docketLine.CustomsData.WB_CustomsThirdQuantity;
				CustomsThirdQuantityUnit = docketLine.CustomsData.WB_CustomsThirdUnitQty;
				EntryDate = docketLine.CustomsData.WB_EntryDate;
				TILV = new Money(Utilities.Round(docketLine.CustomsData.WB_TILV * ratio, 4), docketLine.CustomsData.TILVCurrency);
				ValueForDuty = Utilities.Round(docketLine.CustomsData.WB_ValueForDuty * ratio, 4);
			}
		}

		#endregion

		#region IWhsBondedWarehouseTransactionLine Members

		public ZString AddInfo
		{
			get;
			set;
		}

		public IRefCountry CountryOfOrigin
		{
			get;
			set;
		}

		public ZDateTime EntryDate
		{
			get;
			set;
		}

		public ZDecimal ValueForDuty
		{
			get;
			set;
		}

		public IMoney TILV
		{
			get;
			set;
		}

		public ZDecimal CustomsQuantity
		{
			get;
			set;
		}

		public ZString CustomsQuantityUnit
		{
			get;
			set;
		}

		public ZDecimal CustomsSecondQuantity
		{
			get;
			set;
		}

		public ZString CustomsSecondQuantityUnit
		{
			get;
			set;
		}

		public ZDecimal CustomsThirdQuantity
		{
			get;
			set;
		}

		public ZString CustomsThirdQuantityUnit
		{
			get;
			set;
		}

		public ZDecimal BondedWarehouseQuantity
		{
			get;
			set;
		}

		public ZString BondedWarehouseQuantityUnit
		{
			get;
			set;
		}

		public ZGuid UniqueKey
		{
			get;
			set;
		}

		// Below will be obselete
		public ZString OriginalEntryKey
		{
			get { return null; }
		}

		public ZShort OriginalEntryLineNumber
		{
			get { return 0; }
		}

		#endregion

		#region Comparers

		public bool CompareForBondedWhsQtyWithIsEmptyCheck(IWhsBondedWarehouseTransactionLine line)
		{
			bool result = false;
			if (EntryKey.IsEmpty || EntryKey.EqualsIgnoringCase(line.EntryKey))
			{
				if (EntryLineNumber == 0 || EntryLineNumber == line.EntryLineNumber)
				{
					if (Product == null || Product.PK == line.Product.PK)
					{
						if (Quantity == 0 || Quantity == line.Quantity)
						{
							if (PartAttrib1.IsEmpty || PartAttrib1.EqualsIgnoringCase(line.PartAttrib1))
							{
								if (PartAttrib2.IsEmpty || PartAttrib2.EqualsIgnoringCase(line.PartAttrib2))
								{
									if (PartAttrib3.IsEmpty || PartAttrib3.EqualsIgnoringCase(line.PartAttrib3))
									{
										if (SerialNumber.IsEmpty || SerialNumber.EqualsIgnoringCase(line.SerialNumber))
										{
											result = true;
										}
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		public WhsBondedWarehouseTransactionLine Clone()
		{
			return (WhsBondedWarehouseTransactionLine)MemberwiseClone();
		}

		#endregion

		#region Copy

		public static WhsBondedWarehouseTransactionLine Copy(IWhsBondedWarehouseTransactionLine line)
		{
			WhsBondedWarehouseTransactionLine copyTo = new WhsBondedWarehouseTransactionLine();
			copyTo.Warehouse = line.Warehouse;
			copyTo.AddInfo = line.AddInfo;
			copyTo.BondedWarehouseQuantity = line.BondedWarehouseQuantity;
			copyTo.BondedWarehouseQuantityUnit = line.BondedWarehouseQuantityUnit;
			copyTo.PartAttrib1 = line.PartAttrib1;
			copyTo.PartAttrib2 = line.PartAttrib2;
			copyTo.PartAttrib3 = line.PartAttrib3;
			copyTo.SerialNumber = line.SerialNumber;
			copyTo.CountryOfOrigin = line.CountryOfOrigin;
			copyTo.CustomsQuantity = line.CustomsQuantity;
			copyTo.CustomsQuantityUnit = line.CustomsQuantityUnit;
			copyTo.CustomsSecondQuantity = line.CustomsSecondQuantity;
			copyTo.CustomsSecondQuantityUnit = line.CustomsSecondQuantityUnit;
			copyTo.CustomsThirdQuantity = line.CustomsThirdQuantity;
			copyTo.CustomsThirdQuantityUnit = line.CustomsThirdQuantityUnit;
			copyTo.EntryDate = line.EntryDate;
			copyTo.EntryKey = line.EntryKey;
			copyTo.EntryLineNumber = line.EntryLineNumber;
			copyTo.Product = line.Product;
			copyTo.Quantity = line.Quantity;
			copyTo.QuantityUnit = line.QuantityUnit;
			copyTo.ValueForDuty = line.ValueForDuty;
			copyTo.TILV = line.TILV;
			copyTo.UniqueKey = line.UniqueKey;
			return copyTo;
		}

		public static WhsBondedWarehouseTransactionLine[] Copy(IWhsBondedWarehouseTransactionLine[] lines)
		{
			WhsBondedWarehouseTransactionLine[] copyTo = new WhsBondedWarehouseTransactionLine[lines.Length];
			int i = 0;
			foreach (IWhsBondedWarehouseTransactionLine line in lines)
			{
				copyTo[i++] = WhsBondedWarehouseTransactionLine.Copy(line);
			}
			return copyTo;
		}

		#endregion
	}
}
