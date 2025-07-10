using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusLineTariffDetail : Customs.Business.CusLineTariffDetail, ICusProductCode, Integration.Customs.SG.ICusLineTariffDetail
	{
		public CusLineTariffDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BZ_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public override TariffView UniversalTariff
		{
			get
			{
				return UniversalReferenceDataHelper.LoadBestMatch(Factory, BZ_Tariff, InvoiceLine.EffectiveAssessmentDate);
			}
		}

		#region BZ_Tariff

		[List(nameof(Lookups) + "." + nameof(CusLineTariffDetailLookups.TariffCommodities))]
		[ResourceStringData("SGInvoiceLineUserControl|8c22ba94-69ec-4539-a68f-24816c30a76d", Caption = "Product Code")]
		public override ZString BZ_Tariff
		{
			get { return base.BZ_Tariff; }
			set
			{
				if (base.BZ_Tariff != value)
				{
					base.BZ_Tariff = value;

					var tariff = InvoiceLine?.UniversalTariff;
					if (tariff != null)
					{
						var tariffCommodity = tariff.GetTariffCommodity(this);
						if (tariffCommodity != null)
						{
							if (tariffCommodity.ZZ1_ZZ8_UQ1.IsEmpty || tariffCommodity.ZZ1_ZZ8_UQ1 == "-")
							{
								UpdateWithInvLineDetails();
							}
							else
							{
								BZ_UQ1 = tariffCommodity.ZZ1_ZZ8_UQ1;
								if (!BZ_UQ1.IsEmpty)
								{
									JobComInvoiceLine invLine = Factory.Load<JobComInvoiceLine>(BZ_ParentID);
									if (invLine != null && invLine.JI_CustomsUnitQty == BZ_UQ1)
									{
										BZ_Qty1 = invLine.JI_CustomsQuantity;
									}
								}
							}
						}
					}
					else if (BZ_Tariff == "MISC")
					{
						UpdateWithInvLineDetails();
					}

					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		void UpdateWithInvLineDetails()
		{
			JobComInvoiceLine invLine = Factory.Load<JobComInvoiceLine>(BZ_ParentID);
			if (invLine != null)
			{
				if (!invLine.JI_InvoiceUQ.IsEmpty)
				{
					BZ_UQ1 = invLine.JI_InvoiceUQ;
					if (!BZ_UQ1.IsEmpty)
					{
						BZ_Qty1 = invLine.JI_InvoiceQuantity;
					}
				}
				else
				{
					BZ_UQ1 = invLine.JI_CustomsUnitQty.Left(3);
					if (!BZ_UQ1.IsEmpty)
					{
						BZ_Qty1 = invLine.JI_CustomsQuantity;
					}
				}
			}
		}

		public override ZGuid BZ_ParentID
		{
			get { return base.BZ_ParentID; }
			set
			{
				if (base.BZ_ParentID != value)
				{
					base.BZ_ParentID = value;
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		public override ZString BZ_ParentTableCode
		{
			get { return base.BZ_ParentTableCode; }
			set
			{
				if (base.BZ_ParentTableCode != value)
				{
					base.BZ_ParentTableCode = value;
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		void MarkInvoiceLineAsNeedingValidation()
		{
			if (BZ_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix)
			{
				JobComInvoiceLine invLine = Factory.Load<JobComInvoiceLine>(BZ_ParentID);
				if (invLine != null)
				{
					invLine.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Lookups

		public new CusLineTariffDetailLookups Lookups
		{
			get { return (CusLineTariffDetailLookups)base.Lookups; }
		}

		protected override Customs.Business.CusLineTariffDetailLookups GetNewLookups()
		{
			return new CusLineTariffDetailLookups(this);
		}

		#endregion

		#region Validation

		protected override Customs.Business.CusLineTariffDetailValidation GetNewValidation()
		{
			return new CusLineTariffDetailValidation(this);
		}

		#endregion

		#region ICusProductCode Members

		ZString ICusProductCode.ProductCode
		{
			get { return BZ_Tariff; }
		}

		ZDecimal ICusProductCode.ProductCodeQty
		{
			get { return BZ_Qty1; }
		}

		ZString ICusProductCode.ProductCodeUnitType
		{
			get { return BZ_UQ1; }
		}

		#endregion

		public bool IsChemicalPurityRequired()
		{
			string codeToCheck = BZ_Tariff.SubstringSafe(0, 3);
			return codeToCheck == "S1A"
				|| codeToCheck == "S1B"
				|| codeToCheck == "S2A"
				|| codeToCheck == "S2B"
				|| codeToCheck == "S3A"
				|| codeToCheck == "S3C";
		}

		public bool IsGenericProductCode()
		{
			return BZ_Tariff == "MISC"
				|| BZ_Tariff == "OVR"
				|| BZ_Tariff == "MV";
		}

		public bool IsHSA_CPM()
		{
			return BZ_Tariff == "HSACHD01000"
				|| BZ_Tariff == "HSACHR01000"
				|| BZ_Tariff == "HSACHP01000"
				|| BZ_Tariff == "HSACHG01000"
				|| BZ_Tariff == "HSACHM01000";
		}

		public bool IsHSAProductCode
		{
			get { return BZ_Tariff.StartsWith("HSACH"); }
		}
	}
}
