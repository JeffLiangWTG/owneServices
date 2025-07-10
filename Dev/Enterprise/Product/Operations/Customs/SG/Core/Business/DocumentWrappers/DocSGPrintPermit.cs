using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocSGPrintPermit : DocumentWrapper
	{
		protected DocSGPrintPermit(PrintPermit permit, BusinessObjectFactory factoryToWrap)
			: base(permit, factoryToWrap)
		{
			this.permit = permit;
		}

		public static DocSGPrintPermit New(PrintPermit permit, BusinessObjectFactory factoryToWrap)
		{
			return permit != null ? new DocSGPrintPermit(permit, factoryToWrap) : null;
		}

		readonly PrintPermit permit;

		protected IPrintPermit inPrintPermit
		{
			get { return ((PrintPermit)WrappedObject).Permit; }
		}

		protected IPrintPermitTN41 inPrintPermitTN41
		{
			get { return inPrintPermit as IPrintPermitTN41; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Cargo Clearance Permit"; }
		}

		public override string ToString()
		{
			return "CCP";
		}

		#region IPrintPermitAmend Members

		public ZString ArrivalDate
		{
			get { return inPrintPermit.ArrivalDate.ToString("dd/MM/yyyy"); }
		}

		public DocPrintPermitConditionsCollection CAConditions
		{
			get
			{
				var result = new DocPrintPermitConditionsCollection(Factory);

				var inPrintPermitTN4 = inPrintPermit as IPrintPermitTN4;
				if (inPrintPermitTN4 != null && inPrintPermitTN4.CAConditions != null)
				{
					foreach (var printConditions in inPrintPermitTN4.CAConditions)
					{
						var boPrintPermitConditions = new PrintPermitConditions(printConditions, Factory);
						var docPrintPermitConsignment = DocPrintPermitConditions.New(boPrintPermitConditions, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public TN41DocPrintPermitConditionsCollection CAPermitConditions
		{
			get
			{
				var result = new TN41DocPrintPermitConditionsCollection(Factory);

				var inPrintPermitTN41 = inPrintPermit as IPrintPermitTN41;
				if (inPrintPermitTN41 != null && inPrintPermitTN41.CAPermitConditions != null)
				{
					foreach (var printConditions in inPrintPermitTN41.CAPermitConditions)
					{
						var boPrintPermitConditions = new TN41PrintPermitConditions(printConditions, Factory);
						var docPrintPermitConsignment = TN41DocPrintPermitConditions.New(boPrintPermitConditions, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public ZString CargoPackingType
		{
			get { return inPrintPermit.CargoPackingType; }
		}

		public ZString CertificateNb
		{
			get { return inPrintPermit.CertificateNo; }
		}

		public DocPrintPermitConsignmentDetailsCollection ConsignmentDetails
		{
			get
			{
				var result = new DocPrintPermitConsignmentDetailsCollection(Factory);
				if (inPrintPermit.ConsignmentDetails != null)
				{
					foreach (var printConsignment in inPrintPermit.ConsignmentDetails)
					{
						var boPrintPermitConsignment = new PrintPermitConsignmentDetails(printConsignment, Factory);
						var docPrintPermitConsignment = DocPrintPermitConsignmentDetails.New(boPrintPermitConsignment, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public DocPrintPermitContainersCollection ContainerIdentifiers
		{
			get
			{
				var result = new DocPrintPermitContainersCollection(Factory);
				if (inPrintPermit.ContainerIdentifiers != null)
				{
					foreach (var printContainer in inPrintPermit.ContainerIdentifiers)
					{
						var boPrintPermitContainer = new PrintPermitContainers(printContainer, Factory);
						var docPrintPermitContainers = DocPrintPermitContainers.New(boPrintPermitContainer, Factory);
						result.Add(docPrintPermitContainers);
					}
				}
				return result;
			}
		}

		public ZString CountryOfFinalDest
		{
			get { return inPrintPermit.CountryOfFinalDest; }
		}

		public DocPrintPermitConditionsCollection CustomsConditions
		{
			get
			{
				var result = new DocPrintPermitConditionsCollection(Factory);
				var inPrintPermitTN4 = inPrintPermit as IPrintPermitTN4;
				if (inPrintPermitTN4 != null && inPrintPermitTN4.CustomsConditions != null)
				{
					foreach (var printConditions in inPrintPermitTN4.CustomsConditions)
					{
						var boPrintPermitConditions = new PrintPermitConditions(printConditions, Factory);
						var docPrintPermitConsignment = DocPrintPermitConditions.New(boPrintPermitConditions, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public TN41DocPrintPermitConditionsCollection CustomsPermitConditions
		{
			get
			{
				var result = new TN41DocPrintPermitConditionsCollection(Factory);
				var inPrintPermitTN41 = inPrintPermit as IPrintPermitTN41;
				if (inPrintPermitTN41 != null && inPrintPermitTN41.CustomsPermitConditions != null)
				{
					foreach (var printConditions in inPrintPermitTN41.CustomsPermitConditions)
					{
						var boPrintPermitConditions = new TN41PrintPermitConditions(printConditions, Factory);
						var docPrintPermitConsignment = TN41DocPrintPermitConditions.New(boPrintPermitConditions, Factory);
						result.Add(docPrintPermitConsignment);
					}
				}
				return result;
			}
		}

		public ZString CustomsProcedureCodes
		{
			get { return string.Join(System.Environment.NewLine, inPrintPermit.CustomsProcedureCodes.SplitIntoArray(35, 5)); }
		}

		public ZString DeclarantCode
		{
			get { return inPrintPermit.DeclarantCode; }
		}

		public ZString DeclarantName
		{
			get { return inPrintPermit.DeclarantName; }
		}

		public ZString DeclarationType
		{
			get { return inPrintPermit.DeclarationType; }
		}

		public ZString DepartureDate
		{
			get { return inPrintPermit.DepartureDate.ToString("dd/MM/yyyy"); }
		}

		public ZString EntityIdentOfCompany
		{
			get { return inPrintPermit.EntityIdentOfCompany; }
		}

		public ZString Exporter
		{
			get { return inPrintPermit.Exporter; }
		}

		public ZString ExporterNameLine1
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.ExporterNameLine1 : ZString.Empty; }
		}

		public ZString ExporterNameLine2
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.ExporterNameLine2 : ZString.Empty; }
		}

		public ZString ExporterUEN
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.ExporterUEN : ZString.Empty; }
		}

		public ZString FinalPortOfCall
		{
			get { return inPrintPermit.FinalPortOfCall; }
		}

		public ZString HandlingAgent
		{
			get { return inPrintPermit.HandlingAgent; }
		}

		public ZString HandlingAgentNameLine1
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.HandlingAgentNameLine1 : ZString.Empty; }
		}

		public ZString HandlingAgentNameLine2
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.HandlingAgentNameLine2 : ZString.Empty; }
		}

		public ZString HandlingAgentNameLine3
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.HandlingAgentNameLine3 : ZString.Empty; }
		}

		public ZString HandlingAgentUEN
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.HandlingAgentUEN : ZString.Empty; }
		}

		public ZString Importer
		{
			get { return inPrintPermit.Importer; }
		}

		public ZString ImporterNameLine1
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.ImporterNameLine1 : ZString.Empty; }
		}

		public ZString ImporterNameLine2
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.ImporterNameLine2 : ZString.Empty; }
		}

		public ZString ImporterUEN
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.ImporterUEN : ZString.Empty; }
		}

		public ZString InOBLMawbNb
		{
			get { return inPrintPermit.InOBLMawbNb.Length < 28 ? inPrintPermit.InOBLMawbNb : ZString.Empty; }
		}

		public ZString LongInOBLMawbNb
		{
			get { return inPrintPermit.InOBLMawbNb.Length < 28 ? ZString.Empty : inPrintPermit.InOBLMawbNb; }
		}

		public ZString InVesName
		{
			get { return inPrintPermit.InVesName; }
		}

		public ZString InVoyageFlightNumber
		{
			get { return inPrintPermit.InVoyageFlightNumber; }
		}

		public ZString InwardCarrierAgent
		{
			get { return inPrintPermit.InwardCarrierAgent; }
		}

		public ZString InwardCarrierAgentNameLine1
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.InwardCarrierAgentNameLine1 : ZString.Empty; }
		}

		public ZString InwardCarrierAgentNameLine2
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.InwardCarrierAgentNameLine2 : ZString.Empty; }
		}

		public ZString InwardCarrierAgentNameLine3
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.InwardCarrierAgentNameLine3 : ZString.Empty; }
		}

		public ZString LicenceNb
		{
			get { return string.Join(System.Environment.NewLine, inPrintPermit.LicenceNo.SplitIntoArray(35, 5)); }
		}

		public ZString ManufacturerName
		{
			get { return inPrintPermit.ManufacturerName; }
		}

		public ZString MessageType
		{
			get { return inPrintPermit.MessageType; }
		}

		public ZString NameOfCompany
		{
			get { return inPrintPermit.NameOfCompany; }
		}

		public ZString NextPortOfCall
		{
			get { return inPrintPermit.NextPortOfCall; }
		}

		public ZString OutOBLMawbNb
		{
			get { return inPrintPermit.OutOBLMawbNb.Length < 25 ? inPrintPermit.OutOBLMawbNb : ZString.Empty; }
		}

		public ZString LongOutOBLMawbNb
		{
			get { return inPrintPermit.OutOBLMawbNb.Length < 25 ? ZString.Empty : inPrintPermit.OutOBLMawbNb; }
		}

		public ZString OutVesLocation
		{
			get { return inPrintPermit.OutVesLocation; }
		}

		public ZString OutVesName
		{
			get { return inPrintPermit.OutVesName; }
		}

		public ZString OutVoyageFlightNumber
		{
			get { return inPrintPermit.OutVoyageFlightNumber; }
		}

		public ZString OutwardCarrierAgent
		{
			get { return inPrintPermit.OutwardCarrierAgent; }
		}

		public ZString OutwardCarrierAgentNameLine1
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.OutwardCarrierAgentNameLine1 : ZString.Empty; }
		}

		public ZString OutwardCarrierAgentNameLine2
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.OutwardCarrierAgentNameLine2 : ZString.Empty; }
		}

		public ZString OutwardCarrierAgentNameLine3
		{
			get { return inPrintPermitTN41 != null ? inPrintPermitTN41.OutwardCarrierAgentNameLine3 : ZString.Empty; }
		}

		public ZString PermitNumber
		{
			get { return inPrintPermit.PermitNumber; }
		}

		public ZString PlaceOfReceipt
		{
			get { return inPrintPermit.PlaceOfReceipt; }
		}

		public ZString PlaceOfRelease
		{
			get { return inPrintPermit.PlaceOfRelease; }
		}

		public ZString PlaceOfReleaseName
		{
			get { return inPrintPermitTN41?.PlaceOfReleaseName ?? string.Empty; }
		}

		public ZString PlaceOfReleaseCode
		{
			get { return inPrintPermitTN41?.PlaceOfReleaseCode ?? string.Empty; }
		}

		public ZString PlaceOfReceiptName
		{
			get { return inPrintPermitTN41?.PlaceOfReceiptName ?? string.Empty; }
		}

		public ZString PlaceOfReceiptCode
		{
			get { return inPrintPermitTN41?.PlaceOfReceiptCode ?? string.Empty; }
		}

		public ZString PortOfDischarge
		{
			get { return inPrintPermit.PortOfDischarge; }
		}

		public ZString PortOfDischargeFinalPortOfCall => IsSeaStoreDeclaration ? FinalPortOfCall : PortOfDischarge;

		public ZString PortOfLoading
		{
			get { return inPrintPermit.PortOfLoading; }
		}

		public ZString PortOfLoadingPortOfCall => IsSeaStoreDeclaration ? NextPortOfCall : PortOfLoading;

		ZBool IsSeaStoreDeclaration
		{
			get
			{
				var result = false;

				var supporterPk = (permit as IVisualizerNoteSupporter)?.PK ?? ZGuid.Invalid;
				if (supporterPk.IsValid)
				{
					result = Factory.GetCachedValue<ZBool>(supporterPk + nameof(IsSeaStoreDeclaration), () =>
					{
						var customsDec = Factory.Load<CusEntryHeader>(supporterPk) as ICustomsDec;
						return customsDec != null && customsDec.IsSeaStoreDeclaration;
					});
				}

				return result;
			}
		}

		public ZString TelNb
		{
			get { return inPrintPermit.TelNb; }
		}

		public ZString TotalAmountPayable
		{
			get { return inPrintPermit.TotalAmountPayable.ToString(2); }
		}

		public ZString TotalCustomsDUTPayable
		{
			get { return inPrintPermit.TotalCustomsDUTPayable.ToString(2); }
		}

		public ZString TotalOtherTaxPayable
		{
			get { return inPrintPermit.TotalOtherTaxPayable.ToString(2); }
		}

		public ZString TotalExciseDUTPayable
		{
			get { return inPrintPermit.TotalExciseDUTPayable.ToString(2); }
		}

		public ZString TotalGrossWt
		{
			get { return inPrintPermit.TotalGrossWt; }
		}

		public ZString TotalGstAmount
		{
			get { return inPrintPermit.TotalGstAmount.ToString(2); }
		}

		public ZString TotalOuterPack
		{
			get { return inPrintPermit.TotalOuterPack; }
		}

		public ZString TowingVesselName
		{
			get { return inPrintPermit.TowingVesselName; }
		}

		public ZString TradersRemark
		{
			get
			{
				var tradersRemarkPrintString = new ZStringBuilder();

				foreach (var str in inPrintPermit.TradersRemark)
				{
					tradersRemarkPrintString.AppendLine(str);
				}

				return tradersRemarkPrintString.ToString();
			}
		}

		public ZString UniqueRef
		{
			get
			{
				ZString result;
				if (inPrintPermit.TradeNetVersion == SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne)
				{
					result = inPrintPermit.UniqueRef;
				}
				else
				{
					result = inPrintPermit.UniqueRef.Substring(0, 20) + " " + inPrintPermit.UniqueRef.Substring(20, 8) + " " + inPrintPermit.UniqueRef.Substring(28, 4);
				}

				return result;
			}
		}

		public ZString ValidityPeriodFrom
		{
			get { return inPrintPermit.ValidityPeriodFrom.ToString("dd/MM/yyyy"); }
		}

		public ZString ValidityPeriodTo
		{
			get { return inPrintPermit.ValidityPeriodTo.ToString("dd/MM/yyyy"); }
		}

		public ZString PermitNumberBarcode
		{
			get { return PermitNumber.Length > 0 ? "*" + PermitNumber + "*" : ""; }
		}

		public ZString AmendmentDate
		{
			get { return inPrintPermit.AmendDate.ToString("dd/MM/yyyy"); }
		}

		public ZString AmendmentFields
		{
			get
			{
				var result = new ZString();

				foreach (var str in inPrintPermit.AmendFields)
				{
					result = result + str + System.Environment.NewLine;
				}

				return result;
			}
		}

		public ZBool HideMawbLine
		{
			get { return inPrintPermit.HideMawbLine; }
		}

		public ZBool HideHawbLine
		{
			get { return inPrintPermit.HideHawbLine; }
		}

		public ZBool HideCifFobValue
		{
			get { return inPrintPermit.HideCifFobValue; }
		}

		public ZBool HideLspValue
		{
			get { return inPrintPermit.HideLspValue; }
		}

		public ZBool HideGstValue
		{
			get { return inPrintPermit.HideGstValue; }
		}

		public ZBool HideDutQtyWtVolValue
		{
			get { return inPrintPermit.HideDutQtyWtVolValue; }
		}

		public ZBool HideUnitPriceValue
		{
			get { return inPrintPermit.HideUnitPriceValue; }
		}

		public ZBool HideExciseValue
		{
			get { return inPrintPermit.HideExciseValue; }
		}

		public ZBool HideDutyValue
		{
			get { return inPrintPermit.HideDutyValue; }
		}

		public ZBool HideOtherTaxValue
		{
			get { return inPrintPermit.HideOtherTaxValue; }
		}

		#endregion
	}
}
