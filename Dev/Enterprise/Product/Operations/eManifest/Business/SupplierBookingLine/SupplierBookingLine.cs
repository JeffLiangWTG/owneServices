using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.eManifest.Integration;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.eManifest.Business
{
	[UniversalDataContext(DataContextType.eManifestLine)]
	[DependentBusinessObject(typeof(SupplierBookingHeader), "BookingLines")]
	[SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplification hides desired base class")]
	public partial class SupplierBookingLine : AutoSupplierBookingLine, ISupplierBookingLine, IJobNumber, ICusCodeDataTypeSupporter, IDocumentSupportable
	{
		public SupplierBookingLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (CreatingSupplierBookingLineBizOsPreventer.IsCreatingSupplierBookingLineBizOsSuspended(factory))
			{
				throw new InvalidOperationException("Loading HVLV Lines as BOs after their rows have been modified can lead to problems, for example when creating HVLV Bookings.");
			}
		}

		#region Properties

		#region DL_CubicUQ

		[List("Lookups.DL_CubicUQ_List")]
		public override ZString DL_CubicUQ
		{
			get { return base.DL_CubicUQ; }
			set { base.DL_CubicUQ = value; }
		}

		#endregion

		#region DL_GrossWeightUQ

		[List("Lookups.DL_GrossWeightUQ_List")]
		public override ZString DL_GrossWeightUQ
		{
			get { return base.DL_GrossWeightUQ; }
			set { base.DL_GrossWeightUQ = value; }
		}

		#endregion

		#region DL_JS_ApprovedShipment

		public override ZGuid DL_JS_ApprovedShipment
		{
			get
			{
				return base.DL_JS_ApprovedShipment;
			}
			set
			{
				base.DL_JS_ApprovedShipment = value;

				shipment = null;
			}
		}

		#endregion

		#region DL_SystemCreateUser

		[ReadOnly(true)]
		public override ZString DL_SystemCreateUser
		{
			get { return base.DL_SystemCreateUser; }
			set { base.DL_SystemCreateUser = value; }
		}

		#endregion

		#region DL_SystemCreateTimeUtc

		[ReadOnly(true)]
		public override ZDateTime DL_SystemCreateTimeUtc
		{
			get { return base.DL_SystemCreateTimeUtc; }
			set { base.DL_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region DL_SystemLastEditUser

		[ReadOnly(true)]
		public override ZString DL_SystemLastEditUser
		{
			get { return base.DL_SystemLastEditUser; }
			set { base.DL_SystemLastEditUser = value; }
		}

		#endregion

		#region DL_SystemLastEditTimeUtc

		[ReadOnly(true)]
		public override ZDateTime DL_SystemLastEditTimeUtc
		{
			get { return base.DL_SystemLastEditTimeUtc; }
			set { base.DL_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region DL_KM_LastMileTransportBooking

		public override ZGuid DL_KM_LastMileTransportBooking
		{
			get { return base.DL_KM_LastMileTransportBooking; }
			set
			{
				if (DL_KM_LastMileTransportBooking != value)
				{
					base.DL_KM_LastMileTransportBooking = value;
					lastMileTransportBooking = null;
				}
			}
		}

		#endregion

		#region DL_ConsigneeState

		[List("Lookups.ConsigneeStateList")]
		public override ZString DL_ConsigneeState
		{
			get { return base.DL_ConsigneeState; }
			set { base.DL_ConsigneeState = value; }
		}

		#endregion

		#region DL_ConsignorState

		[List("Lookups.ConsignorStateList")]
		public override ZString DL_ConsignorState
		{
			get { return base.DL_ConsignorState; }
			set { base.DL_ConsignorState = value; }
		}

		#endregion

		#region BookingHeader

		public SupplierBookingHeader BookingHeader
		{
			get
			{
				return Factory.Load<SupplierBookingHeader>(DL_DH_BookingHeader);
			}
		}

		#endregion

		#region Shipment

		public Forwarding.IForwardingShipment Shipment
		{
			get
			{
				if (shipment == null && !DL_JS_ApprovedShipment.IsEmpty)
				{
					shipment = Factory.Load<Forwarding.IForwardingShipment>(DL_JS_ApprovedShipment);
				}

				return shipment;
			}
		}

		Forwarding.IForwardingShipment shipment;

		#endregion

		#region LoadList

		public ELoadList LoadList
		{
			get
			{
				return Factory.Load<ELoadList>(DL_DO_LoadList);
			}
		}

		#endregion

		#region JobNumber

		public string JobNumber
		{
			get
			{
				if (Shipment == null || Shipment.JS_UniqueConsignRef.IsEmpty)
				{
					return string.Empty;
				}

				return Shipment.JS_UniqueConsignRef + " / " + DL_ConsigneeReference;
			}
		}

		#endregion

		#region Transport Company Label

		public ZString LocalTransportCompanyLabel
		{
			get
			{
				var localTransportCompanies = DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.Value;

				foreach (LocalTransportCompanyBranding localTransportCompany in localTransportCompanies)
				{
					if (localTransportCompany.LocalTransportCompanyPK == DL_OH_LastMileCarrier)
					{
						return localTransportCompany.LabelName;
					}
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region LastMileTransportBooking

		public IDtbBooking LastMileTransportBooking
		{
			get
			{
				if (lastMileTransportBooking == null && !DL_KM_LastMileTransportBooking.IsEmpty)
				{
					lastMileTransportBooking = Factory.Load<IDtbBooking>(DL_KM_LastMileTransportBooking);
				}

				return lastMileTransportBooking;
			}
		}

		IDtbBooking lastMileTransportBooking;

		#endregion

		#region TransportBookingNumberForBinding

		public ZString TransportBookingNumberForBinding
		{
			get { return LastMileTransportBooking != null ? LastMileTransportBooking.KM_JobID : ZString.Empty; }
		}

		#endregion

		#region GS1 Prefix

		ZString GS1Prefix
		{
			get { return BookingHeader != null ? BookingHeader.GS1Prefix : ZString.Empty; }
		}

		public bool HasValidGS1Prefix
		{
			get { return SSCCBarCodeChecker.IsSSCCBarCodePrefix(GS1Prefix); }
		}

		#endregion

		#region ValidationSection

		public AddressValidationSection ValidationSection => AddressValidationSection.SupplierBookingLine;

		#endregion

		#region IsPackageIdValidSSCCBarCode

		public bool IsPackageIdValidSSCCBarCode
		{
			get
			{
				const string zeroPrefix = "00";
				const int barCodeLength = 20;

				var reference = DL_ConsigneeReference;

				if (reference.SubstringSafe(0, 2) == zeroPrefix && reference.Length == barCodeLength)
				{
					reference = DL_ConsigneeReference.SubstringSafe(2);
				}

				return SSCCBarCodeChecker.IsSSCCBarCode(reference);
			}
		}

		#endregion

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Generated SSCC Log

		internal void LogGeneratedSSCC()
		{
			var logReference = string.Format((NoResString)"SSCC '{0}' has been generated.", DL_ConsigneeReference);
			ssccGeneratedLog = Logs.CreateRecreateOrUpdateEventLog(Events.EditedARecord, EstimateActual.Actual, ZDateTimeOffset.Now, logReference);
		}

		StmALog ssccGeneratedLog;

		#endregion

		#endregion

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				var cusRelatedBusinessObjects = CusRelatedEventsFinder.GetRelatedBizOs(Shipment, DL_ConsigneeReference);
				if (cusRelatedBusinessObjects != null)
				{
					result.AddRange(cusRelatedBusinessObjects);
				}

				var commonShipment = Shipment as CommonShipment;
				if (commonShipment != null)
				{
					result.Add(commonShipment);
					result.AddRange(commonShipment.Consols);
					result.AddRange(commonShipment.TransportsIncludingRelated);
				}

				var bookingHeader = this.BookingHeader;
				if (bookingHeader != null)
				{
					result.Add(bookingHeader);
				}

				return result.ToArray();
			}
		}

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter
		{
			get { return new SupplierBookingLineDocumentSupporter(this); }
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusCodeDataTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CustomsManifestLineSequence, ObjectFactory.GetType<AU.ICustomsManifestLineSequence>()); // TODO: Delete SupplierBookingLineTest.TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed when ICustomsManifestLineSequence is removed
			return result;
		}

		#endregion

		#region IEManifestLine Members

		ZGuid IEManifestLine.PK => PK;

		ZString IEManifestLine.Reference => DL_ConsigneeReference;

		ZInt IEManifestLine.PackCount => DL_PiecesManifested;

		ZString IEManifestLine.CountryOfDestination => DL_RN_NKConsigneeCountryCode;

		ZString IEManifestLine.GoodsOwner => DL_ConsignorName;

		ZString IEManifestLine.GoodsDescription => DL_GoodsDescription;

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("988954ae-d4ea-4d69-b172-235276b1b0e7", "Supplier Booking Line {0}(Consignee='{1}')", DL_ConsigneeReference, DL_ConsigneeName);
			}
		}

		#region Saving

		public override void OnSaving()
		{
			TryGeneratingConsignmentReferenceOnSaving();

			base.OnSaving();
		}

		public override void OnSaved(bool hasSaveSucceeded)
		{
			if (!hasSaveSucceeded && !IsInDatabase && hasGeneratedSSCC)
			{
				DL_ConsigneeReference = ZString.Empty;
				hasGeneratedSSCC = false;

				if (ssccGeneratedLog != null)
				{
					ssccGeneratedLog.Delete();
				}
			}

			base.OnSaved(hasSaveSucceeded);
		}

		#region TryGeneratingConsignmentReference

		void TryGeneratingConsignmentReferenceOnSaving()
		{
			if (!IsInDatabase && DL_ConsigneeReference.IsEmpty && BookingHeader != null && HasValidGS1Prefix)
			{
				ZString possibleReference;

				do
				{
					possibleReference = applicationIdentifierPrefix + BookingHeader.GS1Fountain.GetNextFormatted(Factory);
					if (possibleReference.Length > SupplierBookingLineSchema.DL_ConsigneeReference.MaxLength)
					{
						return;
					}
				} while (BookingHeader.BookingLines.Cast<SupplierBookingLine>().Any(line => line.DL_ConsigneeReference == possibleReference));

				if (!possibleReference.IsEmpty)
				{
					DL_ConsigneeReference = possibleReference;
					LogGeneratedSSCC();
					hasGeneratedSSCC = true;
				}
			}
		}

		const string applicationIdentifierPrefix = "00";

		bool hasGeneratedSSCC;

		#endregion

		#endregion

		#endregion

		#region CarrierServiceLevel

		// Test Failure : DefaultClientServiceLevelFromParent
		// LoadFromNaturalKey was used on a table + field that does not have a unique index
		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get { return Factory.LoadTop1<OrgCarrierServiceLevel>(new ZQuery(OrgCarrierServiceLevelSchema.PL_Code, DL_PL_NKCarrierServiceLevel)); }
		}

		#endregion
	}
}
