using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Business
{
	public class WeightUQCalculator
	{
		public WeightUQCalculator()
		{
		}

		public WeightUQCalculator(CusEntryHeader entryHeader)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			this.EntryHeader = entryHeader;
		}

		protected readonly CusEntryHeader EntryHeader;

		public virtual string UQ
		{
			get
			{
				CalculateWeightAndWeightUQ();
				return fWeightUQ;
			}
		}

		public virtual ZDecimal Weight
		{
			get
			{
				CalculateWeightAndWeightUQ();
				return fWeight;
			}
		}

		protected virtual ZDecimal HeaderWeight(BaseJobComInvoiceHeader header)
		{
			return header.JZ_Weight;
		}

		protected virtual ZString HeaderWeightUQ(BaseJobComInvoiceHeader header)
		{
			return header.JZ_WeightUQ;
		}

		protected virtual ZDecimal DeclarationWeight(BaseJobDeclaration declaration)
		{
			return declaration.JE_TotalWeight;
		}

		protected virtual ZString DeclarationWeightUQ(BaseJobDeclaration declaration)
		{
			return declaration.JE_TotalWeightUnit;
		}

		protected virtual int MaximumEntryHeaderCount
		{
			get { return 1; }
		}

		public ZDecimal WeightAsUnit(string weightUnit)
		{
			decimal conversionFactor = ZCusBizHelper.GetInstance().GetConversionFactor(UQ, weightUnit);
			return Weight * conversionFactor;
		}

		#region Implementation

		protected void CalculateWeightAndWeightUQ()
		{
			var result = CalculateWeightAndWeightUQCore();

			fWeightUQ = result.Unit;
			fWeight = result.Amount;
		}

		protected virtual ZWeight CalculateWeightAndWeightUQCore()
		{
			ZString wUQ = PreferredWeightUQ;
			ZDecimal w = 0m;
			if (EntryHeader != null)
			{
				foreach (BaseJobComInvoiceHeader header in EntryHeader.InvoiceHeaders)
				{
					if (!header.IsDeleted)
					{
						decimal uQConversionFactor = ZCusBizHelper.GetInstance().GetConversionFactor(HeaderWeightUQ(header), wUQ);
						w += HeaderWeight(header) * uQConversionFactor * GetSplitNatureFactor(header);
					}
				}
				if (w.IsEmpty)
				{
					BaseJobDeclaration declaration = EntryHeader.Declaration;
					if (declaration != null && declaration.CustomsEntryHeaders.Count <= MaximumEntryHeaderCount)
					{
						w = DeclarationWeight(declaration);
						wUQ = DeclarationWeightUQ(declaration);
					}
				}
			}
			return new ZWeight(w, wUQ);
		}

		protected ZString PreferredWeightUQ
		{
			get
			{
				ZString result = ZString.Empty;
				if (EntryHeader != null)
				{
					foreach (BaseJobComInvoiceHeader invoiceHeader in EntryHeader.InvoiceHeaders)
					{
						if (!invoiceHeader.IsDeleted)
						{
							if (result.IsEmpty)
							{
								result = HeaderWeightUQ(invoiceHeader);
							}
							else
							{
								if (result != HeaderWeightUQ(invoiceHeader))
								{
									result = "KG";
									break;
								}
							}
						}
					}
				}
				return result.IsEmpty ? new ZString("KG") : result;
			}
		}

		protected virtual decimal GetSplitNatureFactor(BaseJobComInvoiceHeader invoiceHeader)
		{
			return 1m;
		}

		ZString fWeightUQ;
		ZDecimal fWeight;

		#endregion
	}
}
