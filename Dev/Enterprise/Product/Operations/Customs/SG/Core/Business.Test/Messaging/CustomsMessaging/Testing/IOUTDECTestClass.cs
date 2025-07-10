namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	using System.Collections.Generic;

	using CargoWise.Types;

	public class IOUTDECTestClass : SGCUSDECTestClass, IOUTDEC, ITCODEC
	{
		public IOUTDECTestClass()
			: base()
		{
		}

		#region IOUTDEC Members

		public ITCODEC CO
		{
			get { return fCO; }
			set { fCO = value; }
		}
		ITCODEC fCO;

		public ZBool IsReceiptInLicensedPremise
		{
			get { return fIsReceiptInLicensedPremise; }
			set { fIsReceiptInLicensedPremise = value; }
		}
		ZBool fIsReceiptInLicensedPremise;

		#endregion

		#region ITCODEC Members

		ZString ITCODEC.AdditionalInformation
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.ApplicationProductType
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.DonorCountryCode
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZInt ITCODEC.YearOfEntry
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZInt ITCODEC.PercCommContent1
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZBool ITCODEC.SendInvoiceDetails
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZInt ITCODEC.NumberOfCopies1
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZInt ITCODEC.NumberOfCopies2
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.CertificateType1
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.CertificateType2
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.CurrencyCode
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.AdditionalDetails1
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		ZString ITCODEC.TransportDetails1
		{
			get { throw new System.Exception("The method or operation is not implemented."); }
		}

		public IEnumerable<ICusCertItem> CertItems
		{
			get
			{
				if (fCertItems == null)
				{
					fCertItems = System.Array.Empty<ICusCertItem>();
				}
				return fCertItems;
			}
			set { fCertItems = value; }
		}
		IEnumerable<ICusCertItem> fCertItems;

		#endregion
	}

	public class IOUTUPDTestClass : IOUTDECTestClass, IOUTUPD
	{
		public IOUTUPDTestClass()
			: base()
		{
		}

		#region IOUTUPD Members

		public ZString CertificateNumber
		{
			get { return fCertificateNumber; }
			set { fCertificateNumber = value; }
		}
		ZString fCertificateNumber;

		#endregion
	}
}
