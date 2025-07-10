namespace Enterprise.Customs.US.Business
{
	public class PGAIndicators
	{
		public PGAIndicators(JobDeclaration parent)
		{
			declaration = parent;
		}
		readonly JobDeclaration declaration;
		public bool HasInvoiceLinesWithATF
		{
			get
			{
				if (!hasInvoiceLinesWithATF.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithATF.Value;
			}
		}

		bool? hasInvoiceLinesWithATF;

		public bool HasInvoiceLinesWithFDADisclaim
		{
			get
			{
				if (!hasInvoiceLinesWithFDADisclaim.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFDADisclaim.Value;
			}
		}

		public bool HasInvoiceLinesWithOGAFDA
		{
			get
			{
				if (!hasInvoiceLinesWithOGAFDA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithOGAFDA.Value;
			}
		}

		public bool HasInvoiceLinesWithPGAFDA
		{
			get
			{
				if (!hasInvoiceLinesWithPGAFDA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithPGAFDA.Value;
			}
		}

		public bool HasInvoiceLinesWithDOT
		{
			get
			{
				if (!hasInvoiceLinesWithDOT.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithDOT.Value;
			}
		}

		public bool HasInvoiceLinesWithODS
		{
			get
			{
				if (!hasInvoiceLinesWithODS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithODS.Value;
			}
		}

		public bool HasInvoiceLinesWithVNE
		{
			get
			{
				if (!hasInvoiceLinesWithVNE.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithVNE.Value;
			}
		}

		public bool HasInvoiceLinesWithFSIS
		{
			get
			{
				if (!hasInvoiceLinesWithFSIS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFSIS.Value;
			}
		}

		public bool HasInvoiceLinesWithPST
		{
			get
			{
				if (!hasInvoiceLinesWithPST.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithPST.Value;
			}
		}

		public bool HasInvoiceLinesWithHFC
		{
			get
			{
				if (!hasInvoiceLinesWithHFC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithHFC.Value;
			}
		}

		public bool HasInvoiceLinesWithNHTSARequireIOR
		{
			get
			{
				if (!hasInvoiceLinesWithNHTSARequireIOR.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNHTSARequireIOR.Value;
			}
		}

		public bool HasInvoiceLinesWithNHTSA
		{
			get
			{
				if (!hasInvoiceLinesWithNHTSA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNHTSA.Value;
			}
		}

		public bool HasInvoiceLinesWithNMFS
		{
			get
			{
				if (!hasInvoiceLinesWithNMFS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNMFS.Value;
			}
		}

		public bool HasInvoiceLinesWithAPHIS
		{
			get
			{
				if (!hasInvoiceLinesWithAPHIS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithAPHIS.Value;
			}
		}

		public bool HasInvoiceLinesWithFWS
		{
			get
			{
				if (!hasInvoiceLinesWithFWS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFWS.Value;
			}
		}

		public bool HasInvoiceLinesWithTSCA
		{
			get
			{
				if (!hasInvoiceLinesWithTSCA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithTSCA.Value;
			}
		}

		public bool HasInvoiceLinesWithAMS
		{
			get
			{
				if (!hasInvoiceLinesWithAMS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithAMS.Value;
			}
		}

		public bool HasInvoiceLinesWithNOP
		{
			get
			{
				if (!hasInvoiceLinesWithNOP.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithNOP.Value;
			}
		}

		public bool HasInvoiceLinesWithACELacey
		{
			get
			{
				if (!hasInvoiceLinesWithACELacey.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithACELacey.Value;
			}
		}

		public bool HasInvoiceLinesWithTTB
		{
			get
			{
				if (!hasInvoiceLinesWithTTB.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithTTB.Value;
			}
		}

		public bool HasInvoiceLinesWithDDTC
		{
			get
			{
				if (!hasInvoiceLinesWithDDTC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithDDTC.Value;
			}
		}

		public bool HasInvoiceLinesWithOMC
		{
			get
			{
				if (!hasInvoiceLinesWithOMC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithOMC.Value;
			}
		}

		public bool HasInvoiceLinesWithODSOrTSCAARequireIM
		{
			get
			{
				if (!hasInvoiceLinesWithODSOrTSCAARequireIM.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithODSOrTSCAARequireIM.Value;
			}
		}

		public bool HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2
		{
			get
			{
				if (!hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2.Value;
			}
		}
		bool? hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2;

		public bool HasInvoiceLinesWithCPSC
		{
			get
			{
				if (!hasInvoiceLinesWithCPSC.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithCPSC.Value;
			}
		}

		public bool HasInvoiceLinesWithFWSProcessingCodeWithEDS
		{
			get
			{
				if (!hasInvoiceLinesWithFWSProcessingCodeWithEDS.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithFWSProcessingCodeWithEDS.Value;
			}
		}

		bool? hasInvoiceLinesWithFDADisclaim;
		bool? hasInvoiceLinesWithDOT;

		bool? hasInvoiceLinesWithODS;
		bool? hasInvoiceLinesWithVNE;
		bool? hasInvoiceLinesWithFSIS;
		bool? hasInvoiceLinesWithPST;
		bool? hasInvoiceLinesWithHFC;
		bool? hasInvoiceLinesWithNHTSA;
		bool? hasInvoiceLinesWithNHTSARequireIOR;
		bool? hasInvoiceLinesWithNMFS;
		bool? hasInvoiceLinesWithAPHIS;
		bool? hasInvoiceLinesWithFWS;
		bool? hasInvoiceLinesWithTSCA;
		bool? hasInvoiceLinesWithAMS;
		bool? hasInvoiceLinesWithNOP;
		bool? hasInvoiceLinesWithACELacey;
		bool? hasInvoiceLinesWithTTB;
		bool? hasInvoiceLinesWithDDTC;
		bool? hasInvoiceLinesWithODSOrTSCAARequireIM;
		bool? hasInvoiceLinesWithOMC;
		bool? hasInvoiceLinesWithPGAFDA;
		bool? hasInvoiceLinesWithOGAFDA;
		bool? hasInvoiceLinesWithCPSC;
		bool? hasInvoiceLinesWithFWSProcessingCodeWithEDS;

		public bool HasInvoiceLinesWithDEA
		{
			get
			{
				if (!hasInvoiceLinesWithDEA.HasValue)
				{
					CalculateInvoiceLinesWithPGAIndicators();
				}
				return hasInvoiceLinesWithDEA.Value;
			}
		}
		bool? hasInvoiceLinesWithDEA;

		public void RefreshInvoiceLinesWithPGAIndicators()
		{
			hasInvoiceLinesWithFDADisclaim = null;
			hasInvoiceLinesWithPGAFDA = null;
			hasInvoiceLinesWithOGAFDA = null;
			hasInvoiceLinesWithDOT = null;

			hasInvoiceLinesWithODS = null;
			hasInvoiceLinesWithVNE = null;
			hasInvoiceLinesWithFSIS = null;
			hasInvoiceLinesWithPST = null;
			hasInvoiceLinesWithHFC = null;
			hasInvoiceLinesWithNHTSA = null;
			hasInvoiceLinesWithNHTSARequireIOR = null;
			hasInvoiceLinesWithNMFS = null;
			hasInvoiceLinesWithAPHIS = null;
			hasInvoiceLinesWithFWS = null;
			hasInvoiceLinesWithTSCA = null;
			hasInvoiceLinesWithAMS = null;
			hasInvoiceLinesWithNOP = null;
			hasInvoiceLinesWithOMC = null;
			hasInvoiceLinesWithACELacey = null;
			hasInvoiceLinesWithATF = null;
			hasInvoiceLinesWithTTB = null;
			hasInvoiceLinesWithDDTC = null;
			hasInvoiceLinesWithODSOrTSCAARequireIM = null;
			hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 = null;
			hasInvoiceLinesWithDEA = null;
			hasInvoiceLinesWithCPSC = null;
			hasInvoiceLinesWithFWSProcessingCodeWithEDS = null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void CalculateInvoiceLinesWithPGAIndicators()
		{
			declaration.LoadInvoiceLinesChildrenIfNeeded();
			hasInvoiceLinesWithFDADisclaim = false;
			hasInvoiceLinesWithPGAFDA = false;
			hasInvoiceLinesWithOGAFDA = false;
			hasInvoiceLinesWithDOT = false;

			hasInvoiceLinesWithODS = false;
			hasInvoiceLinesWithVNE = false;
			hasInvoiceLinesWithFSIS = false;
			hasInvoiceLinesWithPST = false;
			hasInvoiceLinesWithHFC = false;
			hasInvoiceLinesWithNHTSA = false;
			hasInvoiceLinesWithNHTSARequireIOR = false;
			hasInvoiceLinesWithNMFS = false;
			hasInvoiceLinesWithAPHIS = false;
			hasInvoiceLinesWithFWS = false;
			hasInvoiceLinesWithTSCA = false;
			hasInvoiceLinesWithAMS = false;
			hasInvoiceLinesWithNOP = false;
			hasInvoiceLinesWithACELacey = false;
			hasInvoiceLinesWithATF = false;
			hasInvoiceLinesWithTTB = false;
			hasInvoiceLinesWithDDTC = false;
			hasInvoiceLinesWithOMC = false;
			hasInvoiceLinesWithODSOrTSCAARequireIM = false;
			hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 = false;
			hasInvoiceLinesWithDEA = false;
			hasInvoiceLinesWithCPSC = false;
			hasInvoiceLinesWithFWSProcessingCodeWithEDS = false;

			foreach (JobComInvoiceHeader invoiceHeader in declaration.Invoices)
			{
				hasInvoiceLinesWithFDADisclaim |= invoiceHeader.HasInvoiceLinesWithFDADisclaim;
				hasInvoiceLinesWithPGAFDA |= invoiceHeader.HasInvoiceLinesWithPGAFDA;
				hasInvoiceLinesWithOGAFDA |= invoiceHeader.HasInvoiceLinesWithOGAFDA;
				hasInvoiceLinesWithDOT |= invoiceHeader.HasInvoiceLinesWithDOT;

				hasInvoiceLinesWithODS |= invoiceHeader.HasInvoiceLinesWithODS;
				hasInvoiceLinesWithVNE |= invoiceHeader.HasInvoiceLinesWithVNE;
				hasInvoiceLinesWithFSIS |= invoiceHeader.HasInvoiceLinesWithFSIS;
				hasInvoiceLinesWithPST |= invoiceHeader.HasInvoiceLinesWithPST;
				hasInvoiceLinesWithHFC |= invoiceHeader.HasInvoiceLinesWithHFC;
				hasInvoiceLinesWithNHTSA |= invoiceHeader.HasInvoiceLinesWithNHTSA;
				hasInvoiceLinesWithNHTSARequireIOR |= invoiceHeader.HasInvoiceLinesWithNHTSARequireIOR;
				hasInvoiceLinesWithNMFS |= invoiceHeader.HasInvoiceLinesWithNMFS;
				hasInvoiceLinesWithAPHIS |= invoiceHeader.HasInvoiceLinesWithAPHIS;
				hasInvoiceLinesWithFWS |= invoiceHeader.HasInvoiceLinesWithFWS;
				hasInvoiceLinesWithTSCA |= invoiceHeader.HasInvoiceLinesWithTSCA;
				hasInvoiceLinesWithAMS |= invoiceHeader.HasInvoiceLinesWithAMS;
				hasInvoiceLinesWithNOP |= invoiceHeader.HasInvoiceLinesWithNOP;
				hasInvoiceLinesWithACELacey |= invoiceHeader.HasInvoiceLinesWithACELacey;
				hasInvoiceLinesWithATF |= invoiceHeader.HasInvoiceLinesWithATF;
				hasInvoiceLinesWithTTB |= invoiceHeader.HasInvoiceLinesWithTTB;
				hasInvoiceLinesWithDDTC |= invoiceHeader.HasInvoiceLinesWithDDTC;
				hasInvoiceLinesWithOMC |= invoiceHeader.HasInvoiceLinesWithOMC;
				hasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2 |= invoiceHeader.HasInvoiceLinesWithAMSDisclaimedMO7OrMO1OrMO2;
				hasInvoiceLinesWithODSOrTSCAARequireIM |= invoiceHeader.HasInvoiceLinesWithODSOrTSCAARequireIM;
				hasInvoiceLinesWithDEA |= invoiceHeader.HasInvoiceLinesWithDEA;
				hasInvoiceLinesWithCPSC |= invoiceHeader.HasInvoiceLinesWithCPSC;
				hasInvoiceLinesWithFWSProcessingCodeWithEDS |= invoiceHeader.HasInvoiceLinesWithFWSProcessingCodeWithEDS;
			}
		}

		public bool HasPGADetailsRequiringDeclarationDate
		{
			get { return HasInvoiceLinesWithVNE || HasInvoiceLinesWithNHTSA || HasInvoiceLinesWithPST || HasInvoiceLinesWithFSIS || HasInvoiceLinesWithACELacey || HasInvoiceLinesWithFWS; }
		}

		internal bool HasInvoiceLinesWithPGA
		{
			get
			{
				return HasInvoiceLinesWithFSIS || HasInvoiceLinesWithACELacey || HasExclusivePGAData;
			}
		}

		internal bool HasExclusivePGAData
		{
			get
			{
				return HasInvoiceLinesWithPGAFDA
					|| HasInvoiceLinesWithVNE
					|| HasInvoiceLinesWithPST
					|| HasInvoiceLinesWithNMFS
					|| HasInvoiceLinesWithAPHIS
					|| HasInvoiceLinesWithFWS
					|| HasInvoiceLinesWithNHTSA
					|| HasInvoiceLinesWithAMS
					|| HasInvoiceLinesWithNOP
					|| HasInvoiceLinesWithATF
					|| HasInvoiceLinesWithDDTC
					|| HasInvoiceLinesWithTTB
					|| HasInvoiceLinesWithOMC
					|| HasInvoiceLinesWithCPSC
					|| HasInvoiceLinesWithDEA
					|| HasInvoiceLinesWithHFC;
			}
		}

		internal bool HasInvoiceLinesWithSection301Or232
		{
			get
			{
				if (!hasInvoiceLinesWithSection301Or232.HasValue)
				{
					CalculateVariousFlags();
				}
				return hasInvoiceLinesWithSection301Or232.Value;
			}
		}
		bool? hasInvoiceLinesWithSection301Or232;

		internal bool HasInvoiceLinesWithDomesticStatus
		{
			get
			{
				if (!hasInvoiceLinesWithDomesticStatus.HasValue)
				{
					CalculateVariousFlags();
				}
				return hasInvoiceLinesWithDomesticStatus.Value;
			}
		}

		internal bool HasInvoiceLinesWithADCVDCaseReported
		{
			get
			{
				if (!hasInvoiceLinesWithADCVDCaseReported.HasValue)
				{
					CalculateVariousFlags();
				}
				return hasInvoiceLinesWithADCVDCaseReported.Value;
			}
		}
		bool? hasInvoiceLinesWithADCVDCaseReported;

		internal bool AllInvoiceLinesAreFromCA
		{
			get
			{
				if (!allInvoiceLinesAreFromCA.HasValue)
				{
					CalculateVariousFlags();
				}
				return allInvoiceLinesAreFromCA.Value;
			}
		}
		bool? allInvoiceLinesAreFromCA;

		internal bool AllInvoiceLinesAreReturnedGoods
		{
			get
			{
				if (!allInvoiceLinesAreReturnedGoods.HasValue)
				{
					CalculateVariousFlags();
				}
				return allInvoiceLinesAreReturnedGoods.Value;
			}
		}
		bool? allInvoiceLinesAreReturnedGoods;

		void CalculateVariousFlags()
		{
			hasInvoiceLinesWithDomesticStatus = false;
			hasInvoiceLinesWithADCVDCaseReported = false;
			hasInvoiceLinesWithSection301Or232 = false;
			allInvoiceLinesAreFromCA = declaration.Invoices.Count > 0;
			allInvoiceLinesAreReturnedGoods = declaration.Invoices.Count > 0;

			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				hasInvoiceLinesWithDomesticStatus |= invoice.HasInvoiceLinesWithDomesticStatus;
				hasInvoiceLinesWithADCVDCaseReported |= invoice.HasInvoiceLinesWithADCVDCaseReported;
				hasInvoiceLinesWithSection301Or232 |= invoice.HasInvoiceLinesWithSection301Or232;
				allInvoiceLinesAreFromCA &= invoice.AllInvoiceLinesAreFromCA;
				allInvoiceLinesAreReturnedGoods &= invoice.AllInvoiceLinesAreReturnedGoods;
			}
		}

		public void RefreshInvoiceLinesWithSpecificColumnsChanged()
		{
			hasInvoiceLinesWithDomesticStatus = null;
			hasInvoiceLinesWithADCVDCaseReported = null;
			hasInvoiceLinesWithSection301Or232 = null;
			allInvoiceLinesAreFromCA = null;
			allInvoiceLinesAreReturnedGoods = null;
		}
		bool? hasInvoiceLinesWithDomesticStatus;
	}
}
