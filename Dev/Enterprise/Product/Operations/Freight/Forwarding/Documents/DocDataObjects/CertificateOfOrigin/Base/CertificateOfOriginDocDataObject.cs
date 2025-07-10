using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Supporters.Address;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocSending;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Supporters;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CertificateOfOrigin.Base
{
	public abstract class CertificateOfOriginDocDataObject<TLineItem> : DocDataObject, IDataSourceProvider, ISupportingDocDataObject
		where TLineItem : CertificateOfOriginLineItemDocDataObject
	{
		protected CertificateOfOriginDocDataObject(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region CertificateOfOriginNumber

		public ZString CertificateOfOriginNumber
		{
			get => certificateOfOriginNumber;
			set
			{
				if (SetNonPersistentPropertyValue(CertificateOfOriginNumberInfo, ref certificateOfOriginNumber, value))
				{
					Validate(certificateOfOriginNumber);
				}
			}
		}

		ZString certificateOfOriginNumber;

		public ZPropertyInfo CertificateOfOriginNumberInfo => GetZPropertyInfo(nameof(CertificateOfOriginNumber));

		#endregion

		#region ExporterAddress

		public Address ExporterAddress
		{
			get => exporterAddress;
			set => exporterAddress = SetChild(exporterAddress, value);
		}

		Address exporterAddress;

		#endregion

		#region ProducerAddress

		public Address ProducerAddress
		{
			get => producerAddress;
			set => producerAddress = SetChild(producerAddress, value);
		}

		Address producerAddress;

		#endregion

		#region ProducerAddressState

		[List(nameof(AddressCollection))]
		[CustomFindBoxPopup(typeof(IAddressFormPopup), showDescription: false)]
		public ZString ProducerAddressStateString
		{
			get => producerAddressStateString;
			set
			{
				if (SetNonPersistentPropertyValue(ProducerAddressStateStringInfo, ref producerAddressStateString, value))
				{
					var addressState = AddressStateHelper.GetAddressStateObjectFromString(value);

					if (addressState != null)
					{
						ProducerAddressState.IsSameAsExporter = addressState.IsSameAsExporter;
						ProducerAddressState.IsUnknown = addressState.IsUnknown;
						ProducerAddressState.ExcludeFromPDF = addressState.ExcludeFromPDF;
					}

					Validate(ProducerAddressStateStringInfo);
				}
			}
		}

		ZString producerAddressStateString = AddressStateHelper.GetAddressStateString();

		public ZPropertyInfo ProducerAddressStateStringInfo => GetZPropertyInfo(nameof(ProducerAddressStateString));

		public AddressState ProducerAddressState
		{
			get => producerAddressState;
			set => producerAddressState = SetChild(producerAddressState, value);
		}

		AddressState producerAddressState = new()
		{
			IsSameAsExporter = false,
			IsUnknown = false,
			ExcludeFromPDF = false
		};

		[BusinessObjectTestExclude]
		public AddressBusinessObjectConfiguration AddressCollection { get; set; } = new(string.Empty, false);

		#endregion

		#region ImporterAddress

		public Address ImporterAddress
		{
			get => importerAddress;
			set => importerAddress = SetChild(importerAddress, value);
		}

		Address importerAddress;

		#endregion

		#region CurrentUser

		public Address CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		Address currentUser;

		#endregion

		#region ApplicantCompanyAddress

		public Address ApplicantCompanyAddress
		{
			get => applicantCompanyAddress;
			set => applicantCompanyAddress = SetChild(applicantCompanyAddress, value);
		}

		Address applicantCompanyAddress;

		public ZString ApplicantCompanyAddressError
		{
			get => applicantCompanyAddressError;
			set
			{
				if (SetNonPersistentPropertyValue(ApplicantCompanyAddressErrorInfo, ref applicantCompanyAddressError, value))
				{
					Validate(ApplicantCompanyAddressErrorInfo);
				}
			}
		}
		ZString applicantCompanyAddressError;

		public ZPropertyInfo ApplicantCompanyAddressErrorInfo => GetZPropertyInfo(nameof(ApplicantCompanyAddressError));

		#endregion

		#region CurrentUserError

		public ZString CurrentUserError
		{
			get => currentUserError;
			set
			{
				if (SetNonPersistentPropertyValue(CurrentUserErrorInfo, ref currentUserError, value))
				{
					Validate(CurrentUserErrorInfo);
				}
			}
		}
		ZString currentUserError;

		public ZPropertyInfo CurrentUserErrorInfo => GetZPropertyInfo(nameof(CurrentUserError));

		#endregion

		#region DepartureDate

		public ZDateTime DepartureDate
		{
			get => departureDate;
			set
			{
				if (SetNonPersistentPropertyValue(DepartureDateInfo, ref departureDate, value))
				{
					Validate(DepartureDateInfo);
				}
			}
		}

		ZDateTime departureDate;

		public ZPropertyInfo DepartureDateInfo => GetZPropertyInfo(nameof(DepartureDate));

		#endregion

		#region ArrivalDate

		public ZDateTime ArrivalDate
		{
			get => arrivalDate;
			set
			{
				if (SetNonPersistentPropertyValue(ArrivalDateInfo, ref arrivalDate, value))
				{
					Validate(ArrivalDateInfo);
				}
			}
		}

		ZDateTime arrivalDate;

		public ZPropertyInfo ArrivalDateInfo => GetZPropertyInfo(nameof(ArrivalDate));

		#endregion

		#region Vessel

		public Vessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}

		Vessel vessel;

		#endregion

		#region VoyageFlightNumber

		public ZString VoyageFlightNumber
		{
			get => voyageFlightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNumberInfo, ref voyageFlightNumber, value))
				{
					Validate(VoyageFlightNumberInfo);
				}
			}
		}

		ZString voyageFlightNumber;

		public ZPropertyInfo VoyageFlightNumberInfo => GetZPropertyInfo(nameof(VoyageFlightNumber));

		#endregion

		#region TransportMode

		public ICodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(transportMode, value);
		}
		ICodeDescription transportMode;

		#endregion

		#region PortOfLoading

		public IUnloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}

		IUnloco portOfLoading;

		#endregion

		#region PortOfDischarge

		public IUnloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		IUnloco portOfDischarge;

		#endregion

		#region PortOfOrigin

		public IUnloco PortOfOrigin
		{
			get => portOfOrigin;
			set => portOfOrigin = SetChild(portOfOrigin, value);
		}

		IUnloco portOfOrigin;

		#endregion

		#region PortOfDestination

		public IUnloco PortOfDestination
		{
			get => portOfDestination;
			set => portOfDestination = SetChild(portOfDestination, value);
		}

		IUnloco portOfDestination;

		#endregion

		#region Remarks

		public ZString Remarks
		{
			get => remarks;
			set
			{
				if (SetNonPersistentPropertyValue(RemarksInfo, ref remarks, value))
				{
					Validate(RemarksInfo);
				}
			}
		}
		ZString remarks;

		public ZPropertyInfo RemarksInfo => GetZPropertyInfo(nameof(Remarks));

		#endregion

		#region IsBacktobackCertificateOfOrigin

		public ZBool IsBacktobackCertificateOfOrigin
		{
			get => isBacktoBackCertificateOfOrigin;
			set
			{
				if (SetNonPersistentPropertyValue(IsBacktobackCertificateOfOriginInfo, ref isBacktoBackCertificateOfOrigin, value))
				{
				}
			}
		}

		ZBool isBacktoBackCertificateOfOrigin;

		public ZPropertyInfo IsBacktobackCertificateOfOriginInfo => GetZPropertyInfo(nameof(IsBacktobackCertificateOfOrigin));

		#endregion

		#region IsSubjectOfThirdPartyInvoice

		public ZBool IsSubjectOfThirdPartyInvoice
		{
			get => isSubjectOfThirdPartyInvoice;
			set
			{
				if (SetNonPersistentPropertyValue(IsSubjectOfThirdPartyInvoiceInfo, ref isSubjectOfThirdPartyInvoice, value))
				{
				}
			}
		}

		ZBool isSubjectOfThirdPartyInvoice;

		public ZPropertyInfo IsSubjectOfThirdPartyInvoiceInfo => GetZPropertyInfo(nameof(IsSubjectOfThirdPartyInvoice));

		#endregion

		#region IsIssuedRetroactively

		public ZBool IsIssuedRetroactively
		{
			get => isIssuedRetroactively;
			set
			{
				if (SetNonPersistentPropertyValue(IsIssuedRetroactivelyInfo, ref isIssuedRetroactively, value))
				{
				}
			}
		}

		ZBool isIssuedRetroactively;

		public ZPropertyInfo IsIssuedRetroactivelyInfo => GetZPropertyInfo(nameof(IsIssuedRetroactively));

		#endregion

		#region IsDeMinimis

		public ZBool IsDeMinimis
		{
			get => isDeMinimis;
			set
			{
				if (SetNonPersistentPropertyValue(IsDeMinimisInfo, ref isDeMinimis, value))
				{
				}
			}
		}

		ZBool isDeMinimis;

		public ZPropertyInfo IsDeMinimisInfo => GetZPropertyInfo(nameof(IsDeMinimis));

		#endregion

		#region IsAccumulation

		public ZBool IsAccumulation
		{
			get => isAccumulation;
			set
			{
				if (SetNonPersistentPropertyValue(IsAccumulationInfo, ref isAccumulation, value))
				{
				}
			}
		}

		ZBool isAccumulation;

		public ZPropertyInfo IsAccumulationInfo => GetZPropertyInfo(nameof(IsAccumulation));

		#endregion

		#region LineItems

		public IReadOnlyCollection<TLineItem> LineItems
		{
			get => lineItems;
			set => lineItems = SetChildCollection(lineItems, value);
		}

		IReadOnlyCollection<TLineItem> lineItems;

		#endregion

		#region Signature

		public SignatureDetails Signature
		{
			get => signature;
			set => signature = SetChild(signature, value);
		}
		SignatureDetails signature;

		#endregion

		#region SignatureError

		public ZString SignatureError
		{
			get => signatureError;
			set
			{
				if (SetNonPersistentPropertyValue(SignatureErrorInfo, ref signatureError, value))
				{
					Validate(SignatureErrorInfo);
				}
			}
		}
		ZString signatureError;

		public ZPropertyInfo SignatureErrorInfo => GetZPropertyInfo(nameof(SignatureError));

		#endregion

		#region AgreementInfo

		public AgreementInfo AgreementInfo
		{
			get => agreementInfo;
			set => agreementInfo = SetChild(agreementInfo, value);
		}
		AgreementInfo agreementInfo;

		#endregion

		#region DocSendingCollection

		[BusinessObjectTestExclude]
		public DocSendingBusinessObjectCollection DocSendingCollection
		{
			get => docSendingCollection;
			set => docSendingCollection = SetChild(docSendingCollection, value);
		}
		DocSendingBusinessObjectCollection docSendingCollection;

		#endregion

		#region ThirdPartyInvoiceIssuer

		public ZString ThirdPartyInvoiceIssuer
		{
			get => thirdPartyInvoiceIssuer;
			set
			{
				if (SetNonPersistentPropertyValue(ThirdPartyInvoiceIssuerInfo, ref thirdPartyInvoiceIssuer, value))
				{
					Validate(ThirdPartyInvoiceIssuerInfo);
				}
			}
		}
		ZString thirdPartyInvoiceIssuer;

		public ZPropertyInfo ThirdPartyInvoiceIssuerInfo => GetZPropertyInfo(nameof(ThirdPartyInvoiceIssuer));

		#endregion

		#region Source
		public LineItemSource Source
		{
			get;
			set;
		}
		#endregion
	}
}
