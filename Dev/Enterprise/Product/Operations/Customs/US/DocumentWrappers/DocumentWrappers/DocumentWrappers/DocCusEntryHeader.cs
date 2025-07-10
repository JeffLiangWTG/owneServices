using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			return (cusEntryHeader == null) ? null : new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
		}

		public DocDeclaration Declaration
		{
			get { return DocDeclaration.New(CusEntryHeader.Declaration, Factory); }
		}

		public DocCusEntryLineCollection EntryLines
		{
			get
			{
				if (fEntryLines == null)
				{
					fEntryLines = new DocCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
					fEntryLines.Sort("LineNumber", System.ComponentModel.ListSortDirection.Ascending);
				}
				return fEntryLines;
			}
		}
		DocCusEntryLineCollection fEntryLines;

		public Enterprise.DocumentWrappers.DocOrganisation Forwarder
		{
			get { return Enterprise.DocumentWrappers.DocOrganisation.New(CusEntryHeader.Declaration.Forwarder, Factory); }
		}

		public ZString ForwarderIdentificatioNumber
		{
			get
			{
				var forwarder = CusEntryHeader.Declaration != null ? CusEntryHeader.Declaration.Forwarder : null;

				var address = forwarder != null ? forwarder.MainAddress : null;
				return new SEDIdentificationAndNumberDecider(forwarder, address, null).IdentificationNumber;
			}
		}

		public DocUSOrganisation USPPI
		{
			get { return DocUSOrganisation.New(CusEntryHeader.USPPI, Factory); }
		}

		public DocDocAddress USPPIAddress
		{
			get
			{
				var invoicePickupAddress = CusEntryHeader.RandomHeader.SupplierPickupAddress;
				return invoicePickupAddress == null || invoicePickupAddress.E2_OA_Address.IsEmpty ? USPPI?.LocationAddress : DocDocAddress.New(invoicePickupAddress, Factory);
			}
		}

		public ZString USPPIIdentificatioNumber
		{
			get
			{
				var result = ZString.Empty;
				var usppi = CusEntryHeader.USPPI;
				if (usppi != null)
				{
					var usOrganisationDocAddress = usppi.USOrganisationDocAddress;
					if (usOrganisationDocAddress != null && usOrganisationDocAddress.E2_AddressOverride)
					{
						result = new SEDIdentificationAndNumberDecider(usOrganisationDocAddress, new string[] {
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber,
						OrgCusCode.USACodeTypes.ForeignRegistrationNumber,
						OrgCusCode.CodeTypes.DataUniversalNumberingSystem }).IdentificationNumber;
					}
					else
					{
						var usppiOrg = usppi.Organisation;
						var address = usppiOrg != null ? usppiOrg.MainAddress : null;
						result = new SEDIdentificationAndNumberDecider(usppiOrg, address, null).IdentificationNumber;
					}
				}
				return result;
			}
		}

		public DocUSOrganisation ExportUltimateConsignee
		{
			get { return DocUSOrganisation.New(CusEntryHeader.ExportUltimateConsignee, Factory); }
		}

		public DocUSOrganisation IntermediateConsignee
		{
			get { return DocUSOrganisation.New(CusEntryHeader.IntermediateConsignee, Factory); }
		}

		public ZString TransportationReferenceNumber
		{
			get { return CusEntryHeader.TransportationReferenceNumber; }
		}

		public ZString IsTransactionsRelated
		{
			get { return (CusEntryHeader.IsTransactionsRelated) ? "X" : ""; }
		}

		public ZString IsNotTransactionsRelated
		{
			get { return (CusEntryHeader.IsTransactionsRelated) ? "" : "X"; }
		}

		public ZString PointOfOriginOrForeignTradeZone
		{
			get { return (CusEntryHeader.IsForeignTradeZoneRequired) ? CusEntryHeader.ForeignTradeZone : CusEntryHeader.StateOfOrigin; }
		}

		public ZString CountryOfUltimateDestination
		{
			get { return CusEntryHeader.CountryOfUltimateDestination; }
		}

		public ZString TransportModeDescription
		{
			get { return Declaration.TransportModeDescription; }
		}

		public ZString LoadingPier
		{
			get
			{
				DocUNLOCO portOfLoading = Declaration.PortOfLoading;
				return (portOfLoading == null || !Declaration.TransportModeIsSea) ? ZString.Empty : portOfLoading.PortName;
			}
		}

		public ZString CarrierCode
		{
			get { return CusEntryHeader.CarrierCode; }
		}

		public ZString ExportingCarrier
		{
			get { return CusEntryHeader.ExportingCarrier; }
		}

		public ZString PortOfExportName
		{
			get
			{
				DocUNLOCO portOfExport = Declaration.PortOfExport;
				return (portOfExport == null) ? ZString.Empty : portOfExport.PortName;
			}
		}

		public ZString ImportEntryNumber
		{
			get { return CusEntryHeader.ImportEntryNumber; }
		}

		public ZString IsHazardousCargo
		{
			get { return (CusEntryHeader.IsHazardousCargo) ? "X" : ""; }
		}

		public ZString IsNotHazardousCargo
		{
			get { return (CusEntryHeader.IsHazardousCargo) ? "" : "X"; }
		}

		public ZString PortOfUnloadingName
		{
			get
			{
				DocUNLOCO portOfArrival = Declaration.PortOfArrival;
				return (portOfArrival != null && (Declaration.TransportModeIsAir || Declaration.TransportModeIsSea)) ? portOfArrival.PortNameAndCountryName : ZString.Empty;
			}
		}

		public ZString IsRoutedTransaction
		{
			get { return (CusEntryHeader.IsRoutedTransaction) ? "X" : ""; }
		}

		public ZString IsNotRoutedTransaction
		{
			get { return (CusEntryHeader.IsRoutedTransaction) ? "" : "X"; }
		}

		public ZString InbondType
		{
			get { return CusEntryHeader.InbondType; }
		}

		public ZString IsContainerized
		{
			get { return (CusEntryHeader.Declaration.IsContainerised) ? "X" : ""; }
		}

		public ZString IsNotContainerized
		{
			get { return (!CusEntryHeader.Declaration.IsContainerised) ? "X" : ""; }
		}

		public ZString IsShippedViaAir
		{
			get { return (CusEntryHeader.Declaration.IsAir) ? "X" : ""; }
		}

		public ZString IsShippedViaSea
		{
			get { return (CusEntryHeader.Declaration.IsSea) ? "X" : ""; }
		}

		public ZString IsShippedViaRoad
		{
			get { return (CusEntryHeader.Declaration.IsRoad) ? "X" : ""; }
		}

		public ZString IsShippedViaRail
		{
			get { return (CusEntryHeader.Declaration.IsRail) ? "X" : ""; }
		}

		public ZString IsShippedViaCourier
		{
			get { return (CusEntryHeader.Declaration.JE_TransportMode == Core.Constants.TransportModes.Courier) ? "X" : ""; }
		}

		public ZString IsDirect
		{
			get
			{
				return Declaration.Shipment != null
			  && Declaration.Shipment.Consol != null
			  && Declaration.Shipment.Consol.IsDirect ? "X" : "";
			}
		}

		public ZString IsConsolidate
		{
			get
			{
				return Declaration.Shipment != null
			  && Declaration.Shipment.Consol != null
			  && !Declaration.Shipment.Consol.IsDirect ? "X" : "";
			}
		}

		public ZString IsPrepaid
		{
			get { return Declaration.Shipment != null && Declaration.Shipment.IsPrepaid ? "X" : ""; }
		}

		public ZString IsCollect
		{
			get { return Declaration.Shipment != null && Declaration.Shipment.IsCollect ? "X" : ""; }
		}

		public ZString CODAmount
		{
			get { return Declaration.Shipment != null ? Declaration.Shipment.FreightCODAmountWithCurrency : (ZString)""; }
		}

		public ZString LicenseNumberAndLicenseException
		{
			get
			{
				if (HasMultipleLicenseDetails)
				{
					return "SEE ABOVE";
				}
				else if (EntryLines.Count > 0)
				{
					return EntryLines[0].LicenseNumberAndLicenseException;
				}
				else
				{
					return "";
				}
			}
		}

		public ZString ECCN
		{
			get
			{
				if (HasMultipleECCN)
				{
					return "SEE ABOVE";
				}
				else if (EntryLines.Count > 0)
				{
					return EntryLines[0].ECCN;
				}
				else
				{
					return "";
				}
			}
		}

		public ZBool HasMultipleECCN
		{
			get { return CusEntryHeader.HasMultipleECCN; }
		}

		public ZBool HasMultipleLicenseDetails
		{
			get { return CusEntryHeader.HasMultipleLicenseDetails; }
		}

		#region Implementation

		CusEntryHeader CusEntryHeader
		{
			get { return (CusEntryHeader)WrappedObject; }
		}

		#endregion

	}
}
