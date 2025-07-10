using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.Rating.Integration.ResString;

namespace Enterprise.Rating.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
	public class QuantityUnit
	{
		#region Constants

		public const string KG = "KG";
		public const string M3 = "M3";

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.TransportBookings | RateType.LocalTransport | RateType.Shipping)]
		[MeasureType(MeasureType.ContainerCount)]
		public const string CN = Constants.BusinessQuantityUnit.Container;

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.Shipment)]
		public const string HB = "HB";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.LowestBill)]
		public const string LW = "LW";

		[RateType(RateType.Forwarding | RateType.Customs | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.CFS | RateType.TransportBookings | RateType.LocalTransport | RateType.Shipping)]
		[MeasureType(MeasureType.Package)]
		public const string PK = "PK";

		[RateType(RateType.Forwarding | RateType.Customs | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.TransportBookings | RateType.LocalTransport | RateType.Shipping)]
		[MeasureType(MeasureType.Unit)]
		public const string KM = Constants.Length.Kilometres;

		[RateType(RateType.Forwarding | RateType.Customs | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.TransportBookings | RateType.LocalTransport | RateType.Shipping)]
		[MeasureType(MeasureType.Unit)]
		public const string MI = Constants.Length.Miles;

		[RateType(RateType.ContainerYard)]
		[MeasureType(MeasureType.Length)]
		public const string CM = Constants.Length.Centimetres;

		[RateType(RateType.ContainerYard)]
		[MeasureType(MeasureType.Area)]
		public const string CM2 = Constants.Area.SquareCentimetre;

		[RateType(RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit)]
		[MeasureType(MeasureType.Line)]
		public const string LI = "LI";

		[RateType(RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit)]
		[MeasureType(MeasureType.PalletID)]
		public const string PI = "PI";

		[RateType(0)]
		[MeasureType(MeasureType.ChargeablePallet)]
		public const string CP = "CP";

		[RateType(0)]
		[MeasureType(MeasureType.JobUnit)]
		public const string JU = "JU";

		[RateType(0)]
		[MeasureType(MeasureType.JobWeight)]
		public const string JW = "JW";

		[RateType(0)]
		[MeasureType(MeasureType.JobVolume)]
		public const string JV = "JV";

		[RateType(0)]
		[MeasureType(MeasureType.Package)]
		public const string JP = "JP";

		[RateType(RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit)]
		[MeasureType(MeasureType.LocationPallet)]
		public const string PL = "PL";

		[RateType(RateType.Warehouse)]
		[MeasureType(MeasureType.BOMKit)]
		public const string BOM = "BOM";

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.TransportBookings | RateType.LocalTransport | RateType.ContainerYard)]
		[MeasureType(MeasureType.Time)]
		public const string HR = Constants.Time.Hours;

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.TransportBookings | RateType.LocalTransport | RateType.ShippingExportDetention | RateType.ShippingImportDetention | RateType.ContainerYard)]
		[MeasureType(MeasureType.Time)]
		public const string DY = Constants.Time.Days;

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.ShippingExportDetention | RateType.ShippingImportDetention | RateType.ContainerYard)]
		[MeasureType(MeasureType.Time)]
		public const string WK = Constants.Time.Weeks;

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.TransportBookings | RateType.LocalTransport | RateType.ContainerYard)]
		[MeasureType(MeasureType.Unidentified)]
		public const string SV = "SV";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FDALine)]
		[MeasureTypeCountrySpecific("US")]
		public const string FD = "FD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FDADisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string FDD = "FDD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.PNFDALine)]
		[MeasureTypeCountrySpecific("US")]
		public const string PN = "PN";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.OMCLine)]
		[MeasureTypeCountrySpecific("US")]
		public const string OM = "OM";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.OMCDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string OMD = "OMD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CPSCLine)]
		[MeasureTypeCountrySpecific("US")]
		public const string CS = "CS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CPSCDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string CSD = "CSD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DEALine)]
		[MeasureTypeCountrySpecific("US")]
		public const string DE = "DE";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DEADisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string DED = "DED";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FCCLine)]
		[MeasureTypeCountrySpecific("US")]
		public const string FC = "FC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FCCDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string FCD = "FCD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DOTLine)]
		[MeasureTypeCountrySpecific("US")]
		public const string DO = "DO";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DOTDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string DOD = "DOD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.LaceyLine)]
		[MeasureTypeCountrySpecific("US")]
		public const string LY = "LY";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.LaceyDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string LYD = "LYD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFS370)]
		[MeasureTypeCountrySpecific("US")]
		public const string N3 = "N3";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFS370Disclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string N3D = "N3D";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFSAMR)]
		[MeasureTypeCountrySpecific("US")]
		public const string NA = "NA";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFSAMRDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string NAD = "NAD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFSHMS)]
		[MeasureTypeCountrySpecific("US")]
		public const string NS = "NS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFSHMSDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string NSD = "NSD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFSSIM)]
		[MeasureTypeCountrySpecific("US")]
		public const string NP = "NP";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AMS)]
		[MeasureTypeCountrySpecific("US")]
		public const string AM = "AM";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NOP)]
		[MeasureTypeCountrySpecific("US")]
		public const string NO = "NO";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AMSNOPDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string NOD = "NOD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AMSDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string AMD = "AMD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.APHIS)]
		[MeasureTypeCountrySpecific("US")]
		public const string AS = "AS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.APHISDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string ASD = "ASD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ATF)]
		[MeasureTypeCountrySpecific("US")]
		public const string AF = "AF";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DDTC)]
		[MeasureTypeCountrySpecific("US")]
		public const string DC = "DC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FSIS)]
		[MeasureTypeCountrySpecific("US")]
		public const string FS = "FS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FSISDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string FSD = "FSD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FWS)]
		[MeasureTypeCountrySpecific("US")]
		public const string FW = "FW";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.FWSDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string FWD = "FWD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.PST)]
		[MeasureTypeCountrySpecific("US")]
		public const string PS = "PS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.PSTDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string PSD = "PSD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.HFC)]
		[MeasureTypeCountrySpecific("US")]
		public const string HF = "HF";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.HFCDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string HFD = "HFD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.TTB)]
		[MeasureTypeCountrySpecific("US")]
		public const string TB = "TB";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.TTBDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string TBD = "TBD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.TCC)]
		[MeasureTypeCountrySpecific("US")]
		public const string CL = "CL";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.VNE)]
		[MeasureTypeCountrySpecific("US")]
		public const string VE = "VE";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.VNEDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string VED = "VED";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ODS)]
		[MeasureTypeCountrySpecific("US")]
		public const string OD = "OD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ODSDisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string ODD = "ODD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.TSCA)]
		[MeasureTypeCountrySpecific("US")]
		public const string TS = "TS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.TSCADisclaim)]
		[MeasureTypeCountrySpecific("US")]
		public const string TSD = "TSD";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NMFSCOA)]
		[MeasureTypeCountrySpecific("US")]
		public const string NC = "NC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DeliveryOrders)]
		[MeasureTypeCountrySpecific("US")]
		public const string DOC = "DOC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.SteelLicenses)]
		[MeasureTypeCountrySpecific("US")]
		public const string L01 = "L01";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.SG_TPLCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L02 = "L02";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CA_NAFTA_TPLCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L03 = "L03";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.MX_NAFTA_TPLCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L04 = "L04";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.BeefExportCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L05 = "L05";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DiamondCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L06 = "L06";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ATPDEACertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L07 = "L07";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AU_FTA_ExportCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L08 = "L08";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.MXCementLicense)]
		[MeasureTypeCountrySpecific("US")]
		public const string L09 = "L09";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CAFTA_TPLCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L10 = "L10";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ALBCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L11 = "L11";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CottonShirtingFabricLicense)]
		[MeasureTypeCountrySpecific("US")]
		public const string L12 = "L12";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.HaitiEarnedAllowance)]
		[MeasureTypeCountrySpecific("US")]
		public const string L13 = "L13";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AgriculturalLicense)]
		[MeasureTypeCountrySpecific("US")]
		public const string L14 = "L14";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CAExportSugarCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L16 = "L16";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.WoolLicense)]
		[MeasureTypeCountrySpecific("US")]
		public const string L17 = "L17";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CBTPACertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L18 = "L18";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AGOATextileProvisionNumber)]
		[MeasureTypeCountrySpecific("US")]
		public const string L19 = "L19";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.OtherNonStandardVisa)]
		[MeasureTypeCountrySpecific("US")]
		public const string L20 = "L20";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.USDASugarCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L21 = "L21";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.OrganicProductExemptionCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L22 = "L22";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AMSCertificateOfExemption)]
		[MeasureTypeCountrySpecific("US")]
		public const string L23 = "L23";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L25 = "L25";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.MexicanSugarExportLicense)]
		[MeasureTypeCountrySpecific("US")]
		public const string L26 = "L26";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.GeneralNote15cWaiverCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L27 = "L27";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.AluminumLicenses)]
		[MeasureTypeCountrySpecific("US")]
		public const string L28 = "L28";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CanadianUSMCA_TPLCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L29 = "L29";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.MexicanUSMCA_TPLCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string L30 = "L30";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense)]
		[MeasureTypeCountrySpecific("US")]
		public const string L31 = "L31";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.KRExportSteelCertificate)]
		[MeasureTypeCountrySpecific("US")]
		public const string LKR = "LKR";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.VISANumbers)]
		[MeasureTypeCountrySpecific("US")]
		public const string VIS = "VIS";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.PGALines)]
		[MeasureTypeCountrySpecific("US","CA")]
		public const string PGA = "PGA";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.PGADisclaims)]
		[MeasureTypeCountrySpecific("US")]
		public const string PGD = "PGD";

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.Shipping | RateType.Warehouse | RateType.TransitWarehouse | RateType.TransitWarehouseTransportationUnit | RateType.TransportBookings | RateType.ContainerYard)]
		[MeasureType(MeasureType.ContainerCount)]
		public const string TU = "TU";

		[RateType(RateType.Forwarding | RateType.Customs | RateType.CFS | RateType.Shipping | RateType.LocalTransport | RateType.TransportBookings)]
		[MeasureType(MeasureType.LoadingMeters)]
		public const string LM = "LM";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CFIALine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string FI = "FI";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.NRCANLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string NR = "NR";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.SITTLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string IC = "IC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.TCLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string TC = "TC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.OtherPGALine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string OP = "OP";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.HCLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string HC = "HC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.PHACLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string PH = "PH";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.ECCCLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string EC = "EC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.DFOLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string DF = "DF";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.CNSCLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string SC = "SC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.GACLine)]
		[MeasureTypeCountrySpecific("CA")]
		public const string GC = "GC";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.HTS9902Line)]
		[MeasureTypeCountrySpecific("US")]
		public const string H92 = "H92";

		[RateType(RateType.Forwarding | RateType.Customs)]
		[MeasureType(MeasureType.HTS9903Line)]
		[MeasureTypeCountrySpecific("US")]
		public const string H93 = "H93";

		#endregion

		#region GetDescription

		public static MultilingualString GetDescription(string unit, RateType rateType, Constants.PluralState pluralState = Constants.PluralState.Plural)
		{
			switch (pluralState)
			{
				case Constants.PluralState.Plural:
				case Constants.PluralState.NonPlural:
					switch (unit)
					{
						case CN:
							return rateType == RateType.Shipping
									   ? ResString.GetMultilingualString("Rating|QuantityUnit|CN|Shipping", "Container/Pack")
									   : ResString.GetMultilingualString("Rating|QuantityUnit|CN", "Container");

						case CP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CP", "Job Pallet");
						case DO:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DO", "DOT/NHTSA Lines");
						case DY:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DY", "Day");
						case FC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FC", "FCC Lines");
						case FD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FD", "FD – FDA Lines (ALL)");
						case PN:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PN", "PN – FDA Lines (Prior Notice Only)");
						case HB:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HB", "House Bill");
						case HR:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HR", "Hour");
						case JP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JP", "Job Package");
						case JU:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JU", "Job Unit");
						case JV:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JV", "Job Volume");
						case JW:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JW", "Job Weight");
						case KM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|KM", "Kilometer");
						case CM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CM", "Centimeter");
						case CM2:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CM2", "Square Centimeter");
						case LI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LI", "Line");
						case LW:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LW", "Lowest Bill");
						case LY:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LY", "Lacey Act Lines");
						case MI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|MI", "Mile");
						case PK:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PK", "Package");
						case PL:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PL", "Location Pallet");
						case SV:
							return ResString.GetMultilingualString("Rating|QuantityUnit|SV", "Service Occurrence");
						case WK:
							return ResString.GetMultilingualString("Rating|QuantityUnit|WK", "Week");
						case TU:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TU", "Twenty foot equivalent unit");
						case LM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LM", "Loading Meter");
						case FI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CF", "CFIA Line");
						case NR:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NR", "Natural Resources Canada Line");
						case IC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|IC", "Industry Canada Line");
						case TC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TC", "Transport Canada Line");
						case OP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OP", "Other IID PGA Line");
						case HC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HC", "Health Canada Line");
						case PH:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PH", "Public Health Agency of Canada Line");
						case EC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|EC", "Environment And Climate Change Canada Line");
						case DF:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DF", "Department of Fisheries and Oceans Line");
						case SC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|SC", "Canadian Nuclear Safety Commission Line");
						case GC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|GC", "Global Affairs Canada Line");
						case N3:
							return ResString.GetMultilingualString("Rating|QuantityUnit|N3", "NMFS 370 Lines");
						case NA:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NA", "NMFS AMR Lines");
						case NC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NC", "NMFS COA Lines");
						case AM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AM", "AMS Lines");
						case AS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AS", "APHIS Lines");
						case AF:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AF", "ATF Lines");
						case DC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DC", "DDTC Lines");
						case FS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FS", "FSIS Lines");
						case FW:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FW", "FWS Lines");
						case NS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NS", "NMFS HMS Lines");
						case NP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NP", "NMFS SIM Lines");
						case PS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PS", "PST Lines");
						case HF:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HF", "HFC Lines");
						case TB:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TB", "TTB Lines");
						case CL:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CL", "TTB Cola Certificate Lines");
						case VE:
							return ResString.GetMultilingualString("Rating|QuantityUnit|VE", "VNE Lines");
						case OD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OD", "ODS Lines");
						case TS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TS", "TSCA Lines");
						case OM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OM", "OMC Lines");
						case CS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CS", "CPSC Lines");
						case DE:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DE", "DEA Lines");
						case PI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PI", "Pallet ID");
						case NO:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NO", "NOP Lines");
						case ASD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ASD", "APHIS Disclaim Unit");
						case CSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CSD", "CPSC Disclaim Unit");
						case DED:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DED", "DEA Disclaim Unit");
						case DOD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DOD", "DOT/NHTSA Disclaim Unit");
						case ODD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ODD", "ODS Disclaim Unit");
						case PSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PSD", "PST Disclaim Unit");
						case HFD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HFD", "HFC Disclaim Unit");
						case TSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TSD", "TSCA Disclaim Unit");
						case VED:
							return ResString.GetMultilingualString("Rating|QuantityUnit|VED", "VNE Disclaim Unit");
						case FCD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FCD", "FCC Disclaim Unit");
						case FDD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FDD", "FDA Disclaim Unit");
						case FWD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FWD", "FWS Disclaim Unit");
						case LYD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LYD", "Lacey Act Disclaim Unit");
						case N3D:
							return ResString.GetMultilingualString("Rating|QuantityUnit|N3D", "NMFS 370 Disclaim Unit");
						case NAD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NAD", "NMFS AMR Disclaim Unit");
						case NSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NSD", "NMFS HMS Disclaim Unit");
						case OMD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OMD", "OMC Disclaim Unit");
						case TBD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TBD", "TTB Disclaim Unit");
						case AMD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AMD", "AMS Disclaim Unit");
						case NOD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NOD", "AMS NOP Disclaim Unit");
						case FSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FSD", "FSIS Disclaim Unit");
						case DOC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DOC", "Delivery Orders Unit");
						case L01:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L01", "Steel Licenses Unit");
						case L02:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L02", "SG TPL Certificate Unit");
						case L03:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L03", "CA NAFTA TPL Certificate Unit");
						case L04:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L04", "MX NAFTA TPL Certificate Unit");
						case L05:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L05", "Beef Export Certificate Unit");
						case L06:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L06", "Diamond Certificate Unit");
						case L07:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L07", "ATPDEA Certificate Unit");
						case L08:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L08", "AU FTA Export Certificate Unit");
						case L09:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L09", "MX Cement License Unit");
						case L10:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L10", "CAFTA TPL Certificate Unit");
						case L11:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L11", "ALB Certificate Unit");
						case L12:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L12", "Cotton Shirting Fabric License Unit");
						case L13:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L13", "Haiti Earned Allowance Unit");
						case L14:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L14", "Agricultural License Unit");
						case L16:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L16", "CA Export Sugar Certificate Unit");
						case L17:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L17", "Wool License Unit");
						case L18:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L18", "CBTPA Certificate Unit");
						case L19:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L19", "AGOA Textile Provision Number Unit");
						case L20:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L20", "Other Non-Standard Visa Unit");
						case L21:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L21", "USDA Sugar Certificate Unit");
						case L22:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L22", "Organic Product Exemption Certificate Unit");
						case L23:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L23", "AMS Certificate Exemption Unit");
						case L25:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L25", "Dominican Republic Earned Allowance Program Certificate Unit");
						case L26:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L26", "Mexican Sugar Export License Unit");
						case L27:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L27", "General Note 15c Waiver Certificate Unit");
						case L28:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L28", "Aluminum Licenses Unit");
						case L29:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L29", "Canadian USMCA TPL Certificate Unit");
						case L30:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L30", "Mexican USMCA TPL Certificate Unit");
						case L31:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L31", "Argentine White Grape Juice Concentrate Export License Unit");
						case LKR:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LKR", "KR Export Steel Certificate Unit");
						case VIS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|VIS", "VISA Numbers Unit");
						case PGA:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PGA", "Total of ALL PGA Lines Unit");
						case PGD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PGD", "Total of ALL PGA Disclaims Unit");
						case BOM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|BOM", "BOM Kit");
						case H92:
							return ResString.GetMultilingualString("Rating|QuantityUnit|H92", "Total 9902 HTS Count for Declaration");
						case H93:
							return ResString.GetMultilingualString("Rating|QuantityUnit|H93", "Total 9903 HTS Count for Declaration");
						default:
							return (NoResString)"";
					}
				case Constants.PluralState.PluralOrNonPlural:
					switch (unit)
					{
						case CN:
							return rateType == RateType.Shipping
									   ? ResString.GetMultilingualString("Rating|QuantityUnit|CNPluralOrNonPlural|Shipping", "Container(s)/Pack(s)")
									   : ResString.GetMultilingualString("Rating|QuantityUnit|CNPluralOrNonPlural", "Container(s)");

						case CP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CPPluralOrNonPlural", "Job Pallet(s)");
						case DO:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DOPluralOrNonPlural", "DOT/NHTSA Line(s)");
						case DY:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DYPluralOrNonPlural", "Day(s)");
						case FC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FCPluralOrNonPlural", "FCC Line(s)");
						case FD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FDPluralOrNonPlural", "FD – FDA Line(s)(ALL)");
						case PN:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PNPluralOrNonPlural", "PN – FDA Line(s)(Prior Notice Only)");
						case HB:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HBPluralOrNonPlural", "House Bill(s)");
						case HR:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HRPluralOrNonPlural", "Hour(s)");
						case JP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JPPluralOrNonPlural", "Job Package(s)");
						case JU:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JUPluralOrNonPlural", "Job Unit(s)");
						case JV:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JVPluralOrNonPlural", "Job Volume(s)");
						case JW:
							return ResString.GetMultilingualString("Rating|QuantityUnit|JWPluralOrNonPlural", "Job Weight(s)");
						case KM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|KMPluralOrNonPlural", "Kilometer(s)");
						case CM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CMPluralOrNonPlural", "Centimeter(s)");
						case CM2:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CM2PluralOrNonPlural", "Square Centimeter(s)");
						case LI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LIPluralOrNonPlural", "Line(s)");
						case LW:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LWPluralOrNonPlural", "Lowest Bill(s)");
						case LY:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LYPluralOrNonPlural", "Lacey Act Line(s)");
						case MI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|MIPluralOrNonPlural", "Mile(s)");
						case PK:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PKPluralOrNonPlural", "Package(s)");
						case PL:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PLPluralOrNonPlural", "Location Pallet(s)");
						case SV:
							return ResString.GetMultilingualString("Rating|QuantityUnit|SVPluralOrNonPlural", "Service Occurrence(s)");
						case WK:
							return ResString.GetMultilingualString("Rating|QuantityUnit|WKPluralOrNonPlural", "Week(s)");
						case TU:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TUPluralOrNonPlural", "Twenty foot equivalent unit(s)");
						case LM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LMPluralOrNonPlural", "Loading Meter(s)");
						case FI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CFPluralOrNonPlural", "CFIA Line(s)");
						case NR:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NRPluralOrNonPlural", "NRCAN Line(s)");
						case IC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ICPluralOrNonPlural", "SITT Line(s)");
						case TC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TCPluralOrNonPlural", "TC Line(s)");
						case OP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OPPluralOrNonPlural", "Other IID PGA Line(s)");
						case HC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HCPluralOrNonPlural", "Health Canada Line(s)");
						case PH:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PHPluralOrNonPlural", "Public Health Agency of Canada Line(s)");
						case EC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ECPluralOrNonPlural", "Environment And Climate Change Canada Line(s)");
						case DF:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DFPluralOrNonPlural", "Department of Fisheries and Oceans Line(s)");
						case SC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|SCPluralOrNonPlural", "Canadian Nuclear Safety Commission Line(s)");
						case GC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|GCPluralOrNonPlural", "Global Affairs Canada Line(s)");
						case N3:
							return ResString.GetMultilingualString("Rating|QuantityUnit|N3PluralOrNonPlural", "NMFS 370 Line(s)");
						case NA:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NAPluralOrNonPlural", "NMFS AMR Line(s)");
						case NC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NCPluralOrNonPlural", "NMFS COA Line(s)");
						case AM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AMPluralOrNonPlural", "AMS Line(s)");
						case AS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ASPluralOrNonPlural", "APHIS Line(s)");
						case AF:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AFPluralOrNonPlural", "ATF Line(s)");
						case DC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DCPluralOrNonPlural", "DDTC Line(s)");
						case FS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FSPluralOrNonPlural", "FSIS Line(s)");
						case FW:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FWPluralOrNonPlural", "FWS Line(s)");
						case NS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NSPluralOrNonPlural", "NMFS HMS Line(s)");
						case NP:
							return ResString.GetMultilingualString("Rating|QuantityUnit|SIMluralOrNonPlural", "NMFS SIM Line(s)");
						case PS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PSPluralOrNonPlural", "PST Line(s)");
						case HF:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HFPluralOrNonPlural", "HFC Line(s)");
						case TB:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TBPluralOrNonPlural", "TTB Line(s)");
						case CL:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CCPluralOrNonPlural", "TTB Cola Certificate Line(s)");
						case VE:
							return ResString.GetMultilingualString("Rating|QuantityUnit|VEPluralOrNonPlural", "VNE Line(s)");
						case OD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ODPluralOrNonPlural", "ODS Line(s)");
						case TS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TSPluralOrNonPlural", "TSCA Line(s)");
						case OM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OMPluralOrNonPlural", "OMC Line(s)");
						case CS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CSPluralOrNonPlural", "CPSC Line(s)");
						case DE:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DEPluralOrNonPlural", "DEA Line(s)");
						case PI:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PIPluralOrNonPlural", "Pallet ID(s)");
						case NO:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NOPluralOrNonPlural", "NOP Line(s)");
						case ASD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ASDPluralOrNonPlural", "APHIS Disclaim Unit(s)");
						case CSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|CSDluralOrNonPlural", "CPSC Disclaim Unit(s)");
						case DED:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DEDPluralOrNonPlural", "DEA Disclaim Unit(s)");
						case DOD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DODPluralOrNonPlural", "DOT/NHTSA Disclaim Unit(s)");
						case ODD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|ODDPluralOrNonPlural", "ODS Disclaim Unit(s)");
						case PSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PSDPluralOrNonPlural", "PST Disclaim Unit(s)");
						case HFD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|HFDPluralOrNonPlural", "HFC Disclaim Unit(s)");
						case TSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TSDPluralOrNonPlural", "TSCA Disclaim Unit(s)");
						case VED:
							return ResString.GetMultilingualString("Rating|QuantityUnit|VEDPluralOrNonPlural", "VNE Disclaim Unit(s)");
						case FCD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FCDPluralOrNonPlural", "FCC Disclaim Unit(s)");
						case FDD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FDDPluralOrNonPlural", "FDA Disclaim Unit(s)");
						case FWD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FWDPluralOrNonPlural", "FWS Disclaim Unit(s)");
						case LYD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LYDPluralOrNonPlural", "Lacey Act Disclaim Unit(s)");
						case N3D:
							return ResString.GetMultilingualString("Rating|QuantityUnit|N3DPluralOrNonPlural", "NMFS 370 Disclaim Unit(s)");
						case NAD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NADPluralOrNonPlural", "NMFS AMR Disclaim Unit(s)");
						case NSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NSDPluralOrNonPlural", "NMFS HMS Disclaim Unit(s)");
						case OMD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|OMDPluralOrNonPlural", "OMC Disclaim Unit(s)");
						case TBD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|TBDPluralOrNonPlural", "TTB Disclaim Unit(s)");
						case AMD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|AMDPluralOrNonPlural", "AMS Disclaim Unit(s)");
						case NOD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|NODPluralOrNonPlural", "AMS NOP Disclaim Unit(s)");
						case FSD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|FSDPluralOrNonPlural", "FSIS Disclaim Unit(s)");
						case DOC:
							return ResString.GetMultilingualString("Rating|QuantityUnit|DOCPluralOrNonPlural", "Delivery Orders Unit(s)");
						case L01:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L01PluralOrNonPlural", "Steel Licenses Unit(s)");
						case L02:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L02PluralOrNonPlural", "SG TPL Certificate Unit(s)");
						case L03:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L03PluralOrNonPlural", "CA NAFTA TPL Certificate Unit(s)");
						case L04:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L04PluralOrNonPlural", "MX NAFTA TPL Certificate Unit(s)");
						case L05:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L05PluralOrNonPlural", "Beef Export Certificate Unit(s)");
						case L06:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L06PluralOrNonPlural", "Diamond Certificate Unit(s)");
						case L07:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L07PluralOrNonPlural", "ATPDEA Certificate Unit(s)");
						case L08:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L08PluralOrNonPlural", "AU FTA Export Certificate Unit(s)");
						case L09:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L09PluralOrNonPlural", "MX Cement License Unit(s)");
						case L10:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L10PluralOrNonPlural", "CAFTA TPL Certificate Unit(s)");
						case L11:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L11PluralOrNonPlural", "ALB Certificate Unit(s)");
						case L12:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L12PluralOrNonPlural", "Cotton Shirting Fabric License Unit(s)");
						case L13:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L13PluralOrNonPlural", "Haiti Earned Allowance Unit(s)");
						case L14:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L14PluralOrNonPlural", "Agricultural License Unit(s)");
						case L16:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L16PluralOrNonPlural", "CA Export Sugar Certificate Unit(s)");
						case L17:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L17PluralOrNonPlural", "Wool License Unit(s)");
						case L18:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L18PluralOrNonPlural", "CBTPA Certificate Unit(s)");
						case L19:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L19PluralOrNonPlural", "AGOA Textile Provision Number Unit(s)");
						case L20:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L20PluralOrNonPlural", "Other Non-Standard Visa Unit(s)");
						case L21:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L21PluralOrNonPlural", "USDA Sugar Certificate Unit(s)");
						case L22:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L22PluralOrNonPlural", "Organic Product Exemption Certificate Unit(s)");
						case L23:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L23PluralOrNonPlural", "AMS Certificate Exemption Unit(s)");
						case L25:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L25PluralOrNonPlural", "Dominican Republic Earned Allowance Program Certificate Unit(s)");
						case L26:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L26PluralOrNonPlural", "Mexican Sugar Export License Unit(s)");
						case L27:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L27PluralOrNonPlural", "General Note 15c Waiver Certificate Unit(s)");
						case L28:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L28PluralOrNonPlural", "Aluminum Licenses Unit(s)");
						case L29:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L29PluralOrNonPlural", "Canadian USMCA TPL Certificate Unit(s)");
						case L30:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L30PluralOrNonPlural", "Mexican USMCA TPL Certificate Unit(s)");
						case L31:
							return ResString.GetMultilingualString("Rating|QuantityUnit|L31PluralOrNonPlural", "Argentine White Grape Juice Concentrate Export License Unit(s)");
						case LKR:
							return ResString.GetMultilingualString("Rating|QuantityUnit|LKRPluralOrNonPlural", "KR Export Steel Certificate Unit(s)");
						case VIS:
							return ResString.GetMultilingualString("Rating|QuantityUnit|VISPluralOrNonPlural", "VISA Numbers Unit(s)");
						case PGA:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PGAPluralOrNonPlural", "Total of ALL PGA Lines Unit(s)");
						case PGD:
							return ResString.GetMultilingualString("Rating|QuantityUnit|PGDPluralOrNonPlural", "Total of ALL PGA Disclaims Unit(s)");
						case BOM:
							return ResString.GetMultilingualString("Rating|QuantityUnit|BOMPluralOrNonPlural", "BOM Kit(s)");
						case H92:
							return ResString.GetMultilingualString("Rating|QuantityUnit|H92PluralOrNonPlural", "Total 9902 HTS Count for Declaration");
						case H93:
							return ResString.GetMultilingualString("Rating|QuantityUnit|H93PluralOrNonPlural", "Total 9903 HTS Count for Declaration");
						default:
							return (NoResString)"";
					}
				default:
					return (NoResString)"";
			}
		}

		#endregion

		#region Is...

		/// <summary>
		///		Returns a value indicating whether the <paramref name="unit"/> is weight unit or volume unit or loading meter unit.
		/// </summary>
		public static bool IsVolumetric(string unit)
		{
			return IsWeight(unit) || IsVolume(unit) || IsLoadingMeter(unit);
		}

		public static bool IsWeight(string unit)
		{
			return Constants.Weight.ContainsCode(unit);
		}

		public static bool IsVolume(string unit)
		{
			return Constants.Volume.ContainsCode(unit);
		}

		public static bool IsLength(string unit)
		{
			return Constants.Length.ContainsCode(unit);
		}

		public static bool IsTime(string unit)
		{
			return unit == HR || unit == DY || unit == WK;
		}

		public static bool IsDistance(string unit)
		{
			return unit == KM || unit == MI;
		}

		public static bool IsLoadingMeter(ZString unit)
		{
			return Constants.LoadingLength.ContainsCode(unit);
		}

		#endregion

		#region Attributes

		[AttributeUsage(AttributeTargets.Field)]
		public sealed class MeasureTypeAttribute(MeasureType measureType) : Attribute
		{
			public readonly MeasureType MeasureType = measureType;
		}

		[AttributeUsage(AttributeTargets.Field)]
		public sealed class MeasureTypeCountrySpecificAttribute(params string[] countryCodes) : Attribute
		{
			public readonly string[] CountryCodes = countryCodes;
		}

		#endregion
	}
}
