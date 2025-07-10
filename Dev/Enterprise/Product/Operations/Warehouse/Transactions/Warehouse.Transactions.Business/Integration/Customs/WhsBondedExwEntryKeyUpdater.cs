using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedExwEntryKeyUpdater
	{
		#region Constructors

		public WhsBondedExwEntryKeyUpdater(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			this.Factory = factory;
		}

		#endregion

		#region Update

		public void Update(ZGuid declarationPk, IWhsBondedWarehouseTransaction whsBondedWarehouseTransaction)
		{
			if (declarationPk.IsEmpty)
			{
				throw new ArgumentException("declarationPk should not be empty.", nameof(declarationPk));
			}

			transaction = whsBondedWarehouseTransaction ?? throw new ArgumentNullException(nameof(whsBondedWarehouseTransaction));
			declarationPK = declarationPk;

			UpdateCore();
		}

		protected virtual void UpdateCore()
		{
			WhsOrder[] orders = Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_ExWhsJobGuid, declarationPK));

			foreach (WhsOrder order in orders)
			{
				foreach (WhsOrderLine orderLine in order.Lines)
				{
					IWhsBondedWarehouseTransactionLine line = FindLine(orderLine);
					if (line != null)
					{
						UpdateOrderLine(line, orderLine);
					}
				}
			}
		}

		void UpdateOrderLine(IWhsBondedWarehouseTransactionLine line, WhsOrderLine orderLine)
		{
			orderLine.CustomsData.WB_EntryKey = line.EntryKey.ToUpper();
			orderLine.CustomsData.WB_EntryLineNo = line.EntryLineNumber;
			orderLine.CustomsData.WB_EntryDate = line.EntryDate;
			orderLine.CustomsData.WB_DeclarationReference = transaction.Reference.ToUpper();
		}

		#endregion

		#region Line Matching

		IWhsBondedWarehouseTransactionLine FindLine(WhsOrderLine orderLine)
		{
			IWhsBondedWarehouseTransactionLine result = null;

			foreach (IWhsBondedWarehouseTransactionLine line in transaction.Lines)
			{
				if (!line.UniqueKey.IsEmpty)
				{
					if (line.UniqueKey == orderLine.PK)
					{
						result = line;
						break;
					}
				}
				else
				{
					// this is for old data structure
					if (!MatchedLines.Contains(line.GetHashCode()))
					{
						if (OldDataMatchFound(line, orderLine))
						{
							result = line;
							MatchedLines.Add(line.GetHashCode(), line);
							break;
						}
					}
				}
			}

			return result;
		}

		bool OldDataMatchFound(IWhsBondedWarehouseTransactionLine line, WhsOrderLine orderLine)
		{
			// TODO: Have to ask Richard to make sure the columns have the right amount of decimal places
			// Craig Here: I structured the if statement like this so it was easy to see where the match stopped
			bool result = false;

			if (line != null)
			{
				if (orderLine != null)
				{
					if (transaction != null)
					{
						if (orderLine.Docket != null)
						{
							if (orderLine.Docket.Warehouse != null)
							{
								if (line.Product != null)
								{
									if (orderLine.CustomsData != null)
									{
										if (line.TILV != null)
										{
											if (orderLine.Docket.Client == transaction.Client)
											{
												if (orderLine.Docket.Warehouse.WarehouseAddress == line.Warehouse)
												{
													if (orderLine.WE_OP == line.Product.PK)
													{
														if (orderLine.WE_PartAttrib1.EqualsIgnoringCase(line.PartAttrib1))
														{
															if (orderLine.WE_PartAttrib2.EqualsIgnoringCase(line.PartAttrib2))
															{
																if (orderLine.WE_PartAttrib3.EqualsIgnoringCase(line.PartAttrib3))
																{
																	if (orderLine.WE_TransactionQuantity == line.Quantity)
																	{
																		if (Utilities.Round(orderLine.CustomsData.WB_CustomsQty, 1) == Utilities.Round(line.CustomsQuantity, 1))
																		{
																			if (Utilities.Round(orderLine.CustomsData.WB_ValueForDuty, 1) == Utilities.Round(line.ValueForDuty, 1))
																			{
																				if (Utilities.Round(orderLine.CustomsData.WB_TILV, 1) == Utilities.Round(line.TILV.Amount, 1))
																				{
																					if (orderLine.CustomsData.WB_RX_NKTILVCurrency == (line.TILV.Currency == null ? string.Empty : line.TILV.Currency.Code))
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
													}
												}
											}
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

		HybridDictionary MatchedLines
		{
			get
			{
				if (matchedLines == null)
				{
					matchedLines = new HybridDictionary(transaction.Lines.Count);
				}
				return matchedLines;
			}
		}

		#endregion

		#region Implementation

		ZGuid declarationPK;
		readonly BusinessObjectFactory Factory;
		HybridDictionary matchedLines;
		IWhsBondedWarehouseTransaction transaction;

		#endregion
	}
}
