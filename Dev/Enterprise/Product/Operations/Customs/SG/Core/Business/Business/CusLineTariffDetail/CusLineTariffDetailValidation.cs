using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusLineTariffDetailValidation : Customs.Business.CusLineTariffDetailValidation
	{
		public CusLineTariffDetailValidation(CusLineTariffDetail parent)
			: base(parent)
		{
		}

		protected JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Parent.Factory.Load<JobComInvoiceLine>(Parent.BZ_ParentID)); }
		}
		JobComInvoiceLine invoiceLine;

		public new CusLineTariffDetail Parent
		{
			get { return (CusLineTariffDetail)base.Parent; }
		}

		protected override void CheckBZ_Tariff()
		{
			if (Parent.BZ_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BZ_TariffInfo, "Commodity Code");

				if (InvoiceLine != null && InvoiceLine.UniversalTariff != null)
				{
					if (!Parent.IsGenericProductCode())
					{
						ListValidation.WarnIfInvalidCode(Parent.BZ_TariffInfo, Parent.Lookups.TariffCommodities);
					}

					InvoiceLine.Validation.ValidateJI_Tariff();
				}

				if (InvoiceLine != null && InvoiceLine.ProductCodes != null)
				{
					foreach (CusLineTariffDetail productCode in InvoiceLine.ProductCodes)
					{
						if (productCode.IsChemicalPurityRequired())
						{
							if (InvoiceLine.Declaration != null && InvoiceLine.Declaration.CALicences.Count < 1)
							{
								productCode.BZ_TariffInfo.AddMessageError("A National Authority (Chemical Weapons Convention), NA(CWC), Licence is required before Import or Export of these goods. Enter your license number in the CA License grid on the Declaration Details tab.");
							}
						}
						else if (productCode.IsHSA_CPM())
						{
							if (InvoiceLine.Declaration != null)
							{
								if (InvoiceLine.Declaration.CALicences.Count < 1 && InvoiceLine.CASCCode1s.Count < 1)
								{
									productCode.BZ_TariffInfo.AddMessageError("For Chinese proprietary medicines (CPM), the importer's licence number is to be indicated in the 'Licence Number', (on Details tab: CA Licences), and CPM product listing number is to be indicated under 'CA/SC Code 1', (on Invoice Lines Additional Details tab).");
								}
								else if (InvoiceLine.Declaration.CALicences.Count < 1)
								{
									productCode.BZ_TariffInfo.AddMessageError("For Chinese proprietary medicines (CPM), the importer's licence number needs to be indicated in the 'Licence Number', (on Details tab: CA Licences).");
								}
								else if (InvoiceLine.CASCCode1s.Count < 1)
								{
									productCode.BZ_TariffInfo.AddMessageError("For Chinese proprietary medicines (CPM), the CPM product listing number needs to be indicated under 'CA/SC Code 1', (on Invoice Lines Additional Details tab).");
								}
							}
						}
					}
				}
			}
		}

		protected override void CheckBZ_Qty1()
		{
			if (Parent.BZ_Qty1 <= 0)
			{
				Parent.BZ_Qty1Info.AddWarning("You have not entered a quantity for this product code.");
			}
		}

		protected override void CheckBZ_UQ1()
		{
			if (Parent.BZ_UQ1 != "-")
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BZ_UQ1Info, Parent.Lookups.ProductCodeUQList);
			}

			if (Parent.BZ_Qty1 > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BZ_UQ1Info, "Unit of Quanitity");
			}

			if (InvoiceLine != null && InvoiceLine.ProductCodes != null)
			{
				foreach (CusLineTariffDetail productCode in InvoiceLine.ProductCodes)
				{
					if (productCode.IsHSAProductCode && productCode.BZ_UQ1 != UnitOfQuantityCodeList.Codes.KGM)
					{
						productCode.BZ_UQ1Info.AddMessageError("Unit of Qty must be 'KGM' for all HSA Complimentary Health Product codes.");
					}
				}
			}
		}
	}
}
