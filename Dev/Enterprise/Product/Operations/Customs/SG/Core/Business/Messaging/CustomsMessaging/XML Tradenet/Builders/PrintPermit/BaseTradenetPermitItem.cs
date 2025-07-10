using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class BaseTradeNetPermitItem : IPrintPermitConsignment
	{
		public BaseTradeNetPermitItem(Item item, Invoice[] invoices, bool bondedIntoOrReleasedFromBond)
		{
			this.item = Argument.NotNull(item, nameof(item));

			this.invoices = invoices;
			this.bondedIntoOrReleasedFromBond = bondedIntoOrReleasedFromBond;
		}

		readonly Item item;
		readonly Invoice[] invoices;
		readonly bool bondedIntoOrReleasedFromBond;

		public ZString SerialNb => item.ItemSequenceNumericSpecified ? item.ItemSequenceNumeric.ToString("00").PadLeft(5, ' ') : string.Empty;

		public ZString HSCode => item.ItemHarmonizedSystemCode;

		public ZString BrandName => item.BrandName;

		public ZString HSQuantity
		{
			get
			{
				// SG QA Nov-2011 #10672: "HS Qty & Unit field in CCP: For dutiable items, and non-dutiable items bonded into or released from bonded warehouse, specify total dutaable/non-dutiable quantity/weight/volume and unit of measurement"

				return CustomsDutyPayable > 0 || ExciseDutyPayable > 0 || bondedIntoOrReleasedFromBond
					? item.ItemQuantity?.TotalDutiableQuantity?.Value.ToString() ?? string.Empty
					: item.ItemQuantity?.HarmonizedSystemQuantity?.Value.ToString() ?? string.Empty;
			}
		}

		public ZString HSQuantityUnit
		{
			get
			{
				// SG QA Nov-2011 #10672: "HS Qty & Unit field in CCP: For dutiable items, and non-dutiable items bonded into or released from bonded warehouse, specify total dutaable/non-dutiable quantity/weight/volume and unit of measurement"

				return CustomsDutyPayable > 0 || ExciseDutyPayable > 0 || bondedIntoOrReleasedFromBond
					? item.ItemQuantity?.TotalDutiableQuantity?.unitCode ?? string.Empty
					: item.ItemQuantity?.HarmonizedSystemQuantity?.unitCode ?? string.Empty;
			}
		}

		public ZString Marking => item.LotIdentification?.Marking;

		public ZString CityOfOrigin => item.OriginCountry ?? string.Empty;

		public ZString Model => item.ModelDescription;

		public ZString InwardMawbObl => item.InMAWBOUCROBLNumber;

		public ZString InwardHawbHbl => item.InHAWBHUCRHBLNumber;

		public ZString OutwardMawbObl => item.OutMAWBOUCROBLNumber;

		public ZString OutwardHawbHbl => item.OutHAWBHUCRHBLNumber;

		public ZString GoodsDescription => item.GoodsDescription;

		public ZString ManufacturerName => string.Join(string.Empty, Invoice?.SupplierManufacturerParty?.Name ?? Array.Empty<string>());

		public ZDecimal UnitPrice => item.TransactionValue?.UnitPriceValue?.Amount?.Value ?? ZDecimal.Zero;

		public ZString UnitPriceCurrency => item.TransactionValue?.UnitPriceValue?.Amount?.currencyID;

		public ZString DutQuantity => item.ItemQuantity?.DutiableQuantity?.Value.ToString() ?? string.Empty;

		public ZString DutQuantityUnit => item.ItemQuantity?.DutiableQuantity?.unitCode ?? string.Empty;

		public ZDecimal CustomsDutyPayable
		{
			get
			{
				var result = ZDecimal.Zero;

				var duty = item.Tariff?.CustomsDuty;
				if (duty != null && duty.DutyAmountSpecified)
				{
					result = duty.DutyAmount;
				}

				return result;
			}
		}

		public ZDecimal ExciseDutyPayable
		{
			get
			{
				var result = ZDecimal.Zero;

				var duty = item.Tariff?.ExciseDuty;
				if (duty != null && duty.DutyAmountSpecified)
				{
					result = duty.DutyAmount;
				}

				return result;
			}
		}

		public ZDecimal OtherTaxPayable
		{
			get
			{
				var result = ZDecimal.Zero;

				var duty = item.Tariff?.OtherTax;
				if (duty != null && duty.DutyAmountSpecified)
				{
					result = duty.DutyAmount;
				}

				return result;
			}
		}

		public ZString CurrentLotNb => item.LotIdentification?.CurrentLotNumber;

		public ZString PreviousLotNb => item.LotIdentification?.PreviousLotNumber;

		public ZDecimal CifFobLspValue => item.TransactionValue?.ItemCIFFOBValue ?? 0m;

		public ZDecimal LspAmount => item.TransactionValue?.LastSellingPriceValue ?? 0m;

		public ZDecimal GstAmount => item.Tariff?.GoodsAndServicesTax?.GoodsAndServicesTaxAmount ?? 0m;

		public ZString CASCProductCode => string.Empty;

		public ZDecimal CASCProductQty => item.CASCProduct?.Sum(c => c.CASCProductQuantity?.Value ?? 0m) ?? 0m;

		public ZString CASCProductUQ => item.CASCProduct?.Select(c => c.CASCProductQuantity?.unitCode)?.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c)) ?? string.Empty;

		public ZString EngineNbChassisNb => string.Empty;

		public ZInt OuterPackQty => item.PackingDescription?.OuterPackQuantity?.Value ?? 0;

		public ZString OuterPackUQ => item.PackingDescription?.OuterPackQuantity?.unitCode;

		public ZInt InPackQty => item.PackingDescription?.InPackQuantity?.Value ?? 0;

		public ZString InPackUQ => item.PackingDescription?.InPackQuantity?.unitCode;

		public ZInt InnerPackQty => item.PackingDescription?.InnerPackQuantity?.Value ?? 0;

		public ZString InnerPackUQ => item.PackingDescription?.InnerPackQuantity?.unitCode;

		public ZInt InmostPackQty => item.PackingDescription?.InmostPackQuantity?.Value ?? 0;

		public ZString InmostPackUQ => item.PackingDescription?.InmostPackQuantity?.unitCode;

		public Invoice Invoice => invoice ?? (invoice = invoices?.FirstOrDefault(c => c.InvoiceNumber?.Equals(item.ItemInvoiceNumber ?? string.Empty) ?? false));
		Invoice invoice;

		#region ConsignmentLineValues

		public ZString LineValue1 => GetValue(0);

		public ZString LineValue2 => GetValue(1);

		public ZString LineValue3 => GetValue(2);

		public ZString LineValue4 => GetValue(3);

		public ZString LineValue5 => GetValue(4);

		public ZString LineValue6 => GetValue(5);

		public ZString LineValue7 => GetValue(6);

		public ZString LineValue8 => GetValue(7);

		public ZString LineUnit1 { get; set; }

		public ZString LineUnit2 { get; set; }

		public ZString LineUnit3 { get; set; }

		public ZString LineUnit4 { get; set; }

		public ZString LineUnit5 { get; set; }

		public ZString[] ValueFields
		{
			get
			{
				if (fValueFields == null)
				{
					var result = new List<ZString>();

					LineUnit1 = ZString.Empty;
					LineUnit2 = ZString.Empty;
					LineUnit3 = ZString.Empty;
					LineUnit4 = ZString.Empty;
					LineUnit5 = ZString.Empty;

					void AddValue(ZDecimal value, string unit = "")
					{
						if (value > 0)
						{
							result.Add(value.ToString(2));
						}

						if (!string.IsNullOrWhiteSpace(unit))
						{
							SetLineUnitValue(result.Count, unit);
						}
					}

					AddValue(CifFobLspValue);
					AddValue(LspAmount);
					AddValue(GstAmount);

					if (!DutQuantity.IsEmpty)
					{
						result.Add(DutQuantity);
						SetLineUnitValue(result.Count, DutQuantityUnit);
					}

					AddValue(UnitPrice, UnitPriceCurrency);
					AddValue(ExciseDutyPayable);
					AddValue(CustomsDutyPayable);
					AddValue(OtherTaxPayable);

					fValueFields = result;
				}

				return fValueFields.ToArray();
			}
		}

		List<ZString> fValueFields;

		void SetLineUnitValue(int valueCount, string unitValue)
		{
			if (valueCount == 1)
			{
				LineUnit1 = unitValue;
			}
			else if (valueCount == 2)
			{
				LineUnit2 = unitValue;
			}
			else if (valueCount == 3)
			{
				LineUnit3 = unitValue;
			}
			else if (valueCount == 4)
			{
				LineUnit4 = unitValue;
			}
			else if (valueCount == 5)
			{
				LineUnit5 = unitValue;
			}
		}

		ZString GetValue(int index)
		{
			return ValueFields.Length > index ? ValueFields[index] : ZString.Empty;
		}

		#endregion

		#region IPrintPermitConsignmentTN41

		public ICASCProductCode[] CASCProductCodes
		{
			get
			{
				if (productCodes == null)
				{
					var result = new List<ItemProductCode>();

					var sequence = 0;
					var products = item.CASCProduct;

					if (products != null)
					{
						foreach (var product in products)
						{
							sequence++;
							result.Add(new ItemProductCode(sequence, product));
						}
					}

					productCodes = result.ToArray();
				}

				return productCodes;
			}
		}
		ICASCProductCode[] productCodes;

		public IEngineOrChassisNumber[] EngineOrChassisNumbers
		{
			get
			{
				if (engineOrChassisNumbers == null)
				{
					var result = new List<ItemEngineOrChassisNumber>();

					if (item.ItemHarmonizedSystemCode.StartsWith("87"))
					{
						var sequence = ZInt.Zero;
						var additionalCASCIdentifications = item.CASCProduct?.Where(c => string.IsNullOrWhiteSpace(c.EndUseDescription?.EndUseLine) && c.AdditionalCASCIdentification != null).SelectMany(c => c.AdditionalCASCIdentification) ?? Enumerable.Empty<CASCProductAdditionalCASCIdentification>();

						foreach (var additionalCASCIdentification in additionalCASCIdentifications)
						{
							sequence++;
							result.Add(new ItemEngineOrChassisNumber(sequence, additionalCASCIdentification));
						}
					}

					engineOrChassisNumbers = result.ToArray();
				}

				return engineOrChassisNumbers;
			}
		}
		IEngineOrChassisNumber[] engineOrChassisNumbers;

		#region ItemProductCode

		public class ItemProductCode : ICASCProductCode
		{
			public ItemProductCode(int sequence, CASCProduct product)
			{
				SequenceNumber = sequence.ToString("00").PadLeft(5, ' ');
				ProductCode = product.CASCProductCode;

				var quantity = product.CASCProductQuantity;

				ProductQuantity = quantity != null
					? string.Concat(quantity.Value.ToString().PadLeft(16, ' '), "  ", (quantity.unitCode ?? string.Empty).PadLeft(3, ' '))
					: string.Empty;
			}

			public ZString SequenceNumber { get; }

			public ZString ProductCode { get; }

			public ZString ProductQuantity { get; }
		}

		#endregion

		#region ItemEngineOrChassisNumber

		public class ItemEngineOrChassisNumber : IEngineOrChassisNumber
		{
			public ItemEngineOrChassisNumber(int sequence, CASCProductAdditionalCASCIdentification identification)
			{
				SequenceNumber = sequence.ToString("00").PadLeft(5, ' ');
				Number = string.Join(" / ", new[] { identification.CASCCodeOne, identification.CASCCodeTwo });
			}

			public ZString SequenceNumber { get; }

			public ZString Number { get; }
		}

		#endregion

		#endregion
	}
}
