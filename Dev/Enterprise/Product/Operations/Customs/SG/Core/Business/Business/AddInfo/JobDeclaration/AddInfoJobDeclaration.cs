using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclaration : AddInfo
	{
		public AddInfoJobDeclaration(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public override ZString SG_US_NKPlaceOfCargoRelease
		{
			get { return base.SG_US_NKPlaceOfCargoRelease; }
			set
			{
				if (base.SG_US_NKPlaceOfCargoRelease != value)
				{
					base.SG_US_NKPlaceOfCargoRelease = value;
					MarkInvoiceHeadersAsNeedingValidation();
				}
			}
		}

		public override ZString SG_US_NKPlaceOfReceipt
		{
			get { return base.SG_US_NKPlaceOfReceipt; }
			set
			{
				if (base.SG_US_NKPlaceOfReceipt != value)
				{
					base.SG_US_NKPlaceOfReceipt = value;
					MarkInvoiceHeadersAsNeedingValidation();
				}
			}
		}

		public override ZBool SG_GoodsPreviouslyExemptedFromDuties
		{
			get { return base.SG_GoodsPreviouslyExemptedFromDuties; }
			set
			{
				if (base.SG_GoodsPreviouslyExemptedFromDuties != value)
				{
					base.SG_GoodsPreviouslyExemptedFromDuties = value;
					MarkInvoiceHeadersAsNeedingValidation();
				}
			}
		}

		public override ZString SG_Cert1Type
		{
			get { return base.SG_Cert1Type; }
			set
			{
				if (base.SG_Cert1Type != value)
				{
					base.SG_Cert1Type = value;
					SG_CertSendInvDetails = SGCertificatesCodeList.IsInvoiceDetailsAllowed(Certificate1Type);
					if (isInitialised)
					{
						MarkInvoiceHeadersAsNeedingValidation();
						foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
						{
							foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
							{
								invoiceLine.MarkAsNeedingValidation();
							}
						}
					}
				}
			}
		}

		ZZRefCusCodeListCombined Certificate1Type => SG_Cert1Type.IsEmpty ? null : SGCertificatesCodeList.GetCurrentOrMatchingCertificate(Parent.Factory, SG_Cert1Type);

		public override ZString SG_Cert2Type
		{
			get { return base.SG_Cert2Type; }
			set
			{
				if (base.SG_Cert2Type != value)
				{
					base.SG_Cert2Type = value;
					if (isInitialised)
					{
						MarkInvoiceHeadersAsNeedingValidation();
						foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
						{
							foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
							{
								invoiceLine.MarkAsNeedingValidation();
							}
						}
					}
				}
			}
		}

		public override ZString SG_ApplicationProductType
		{
			get { return base.SG_ApplicationProductType; }
			set
			{
				if (base.SG_ApplicationProductType != value)
				{
					base.SG_ApplicationProductType = value;
					if (isInitialised)
					{
						MarkInvoiceHeadersAsNeedingValidation();
						foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
						{
							foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
							{
								invoiceLine.MarkAsNeedingValidation();
							}
						}
					}

					if (!base.SG_ApplicationProductType.IsEmpty)
					{
						if (base.SG_RX_NKCertReferenceCurrency.IsEmpty)
						{
							base.SG_RX_NKCertReferenceCurrency = Core.Constants.CurrencyCodes.Singapore;
						}
					}
				}
			}
		}

		public override ZString SG_RX_NKCertReferenceCurrency
		{
			get { return base.SG_RX_NKCertReferenceCurrency; }
			set
			{
				if (value.IsEmpty && !SG_ApplicationProductType.IsEmpty)
				{
					base.SG_RX_NKCertReferenceCurrency = Core.Constants.CurrencyCodes.Singapore;
				}
				else
				{
					base.SG_RX_NKCertReferenceCurrency = value;
				}
			}
		}

		protected void MarkInvoiceHeadersAsNeedingValidation()
		{
			if (isInitialised)
			{
				foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
				{
					invoiceHeader.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString SG_OutwardTransportMode
		{
			get { return base.SG_OutwardTransportMode; }
			set
			{
				base.SG_OutwardTransportMode = value;
				if (isInitialised)
				{
					foreach (Bill bill in Declaration.Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		#region Outward Vessel

		[RelatedBusinessObject("OutwardVessel")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Vessels))]
		public override ZString SG_OutwardVesselName
		{
			get => base.SG_OutwardVesselName;
			set
			{
				bool hasChanged = base.SG_OutwardVesselName != value;
				base.SG_OutwardVesselName = value;
				if (hasChanged && !IsCopying)
				{
					Declaration.DefaultOutwardVesselDetails();
				}
			}
		}

		/// <summary>
		///
		///	For SG Declaration, the loading of the Outward RefVessel has changed to take the Lloyds/IMO into consideration:
		///		By default, the VesselName and LloydsIMO will be used - except for SG Outward Vessel where VesselName only is used & if duplicates exist, the user will need to select/enter appropriate details required.
		///		The logic should be:
		///		a)	try to load by VesselName + LloydsIMO, if a single vessel is found, then return that vessel;
		///		b)	If the LloydsIMO is not given, try to load by the VesselName only, if a single vessel is found, then return that vessel;
		///		c)	If multiple vessels are found, return null. (User will need to manually select the required vessel)
		/// 
		/// </summary>
		public RefVessel OutwardVessel => Vessels.Length == 1 ? Vessels[0] : null;

		public bool OutwardVesselHasDuplicates => Vessels.Length > 1;

		RefVessel[] Vessels => Factory.GetValue(ref fVesselsCached, () => LoadOutwardVessels());
		CachedProperty<RefVessel[]> fVesselsCached;

		protected RefVessel[] LoadOutwardVessels()
		{
			var vesselQuery = new ZQuery(RefVesselSchema.RV_Code, SG_OutwardVesselName);
			vesselQuery.IgnoreActiveFilter = true;

			return Factory.Load<RefVessel>(vesselQuery);
		}

		#endregion

		public override ZBool SG_IsInwardHandCarried
		{
			get { return base.SG_IsInwardHandCarried; }
			set
			{
				base.SG_IsInwardHandCarried = value;
				if (isInitialised)
				{
					foreach (Bill bill in Declaration.Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZBool SG_IsOutwardHandCarried
		{
			get { return base.SG_IsOutwardHandCarried; }
			set
			{
				base.SG_IsOutwardHandCarried = value;
				if (isInitialised)
				{
					foreach (Bill bill in Declaration.Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZBool SG_DutyExempt
		{
			get { return base.SG_DutyExempt; }
			set
			{
				if (base.SG_DutyExempt != value)
				{
					base.SG_DutyExempt = value;
					if (isInitialised)
					{
						MarkInvoiceHeadersAsNeedingValidation();
						foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
						{
							foreach (JobComInvoiceLine invoiceLine in invoiceHeader.JobComInvoiceLines)
							{
								invoiceLine.MarkAsNeedingValidation();
							}
						}
					}
				}
			}
		}

		protected override SchemaColumn[] ColumnsForFastSearch
		{
			get
			{
				return new SchemaColumn[] {
					SGAddInfoSchema.SG_OutwardHAWB,
					SGAddInfoSchema.SG_OutwardMAWB,
					SGAddInfoSchema.SG_OutwardVesselName,
					SGAddInfoSchema.SG_OutwardVoyageFlightNo,
					SGAddInfoSchema.SG_OutwardTransportMode
				};
			}
		}

		#region Validation/Lookups

		public new AddInfoJobDeclarationLookups Lookups
		{
			get { return (AddInfoJobDeclarationLookups)base.Lookups; }
		}

		public new AddInfoJobDeclarationValidation Validation
		{
			get { return (AddInfoJobDeclarationValidation)base.Validation; }
		}

		protected override SGAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobDeclarationLookups(this);
		}

		protected override SGAddInfoValidation GetNewValidation()
		{
			SGAddInfoValidation result = null;

			if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT)
			{
				result = new AddInfoJobDeclarationValidation_IPT(this);
			}
			else if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP)
			{
				result = new AddInfoJobDeclarationValidation_INP(this);
			}
			else if (Declaration.IsOUTDEC)
			{
				result = this.SG_ApplicationProductType != "" ? new AddInfoJobDeclarationValidation_OUTwCO(this) : new AddInfoJobDeclarationValidation_OUT(this);
			}
			else if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.TNP)
			{
				result = new AddInfoJobDeclarationValidation_TNP(this);
			}
			else if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.COO)
			{
				result = new AddInfoCOValidation(this);
			}
			else
			{
				result = new AddInfoJobDeclarationValidation(this);
			}

			return result;
		}

		#endregion
	}
}
