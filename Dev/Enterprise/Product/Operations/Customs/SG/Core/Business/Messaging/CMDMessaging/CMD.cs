using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.AWB;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMD : CargoIMP
	{
		public enum ActionCodes { Add, Modify, Delete }

		#region Constants

		public abstract class Constants
		{
			public const string CMD = "CMD";
			public const string MAWB = "MWB";
			public const string HAWB = "HWB";
			public const string Flight = "FLT";
			public const string Shipper = "SHP";
			public const string Consignee = "CNE";
			public const string ImportMAWB = "IMW";
			public const string Exemption = "EXP";
			public const string DUI = "DUI";
			public const string Sender = "SND";

			public abstract class Permit
			{
				public const string Identifier = "TPT";
				public const string TDB = "TDB";
				public const string Customs = "CED";
			}
		}

		#endregion

		public CMD(ForwardingConsol consol, CMDData cMDData)
		{
			fConsol = consol;
			fCMDData = cMDData;
			Validate();
		}

		#region Overrides

		protected override ZString MessageTypeVersionNumber
		{
			get { return "2"; }
		}

		public override ZString StandardMessageIdentifier
		{
			get { return Constants.CMD; }
		}

		#endregion

		#region Implementation

		#region Message Construction

		protected override void ConstructMessage()
		{
			AddMessageIdentification();
			AddMAWBConsignmentDetails();
			AddFlightDetails();
			AddHAWBSummaryDetails();
			AddMAWBShipperDetails();
			AddMAWBConsigneeDetails();
			AddPermitDetails();
			AddImportMAWBDetails();
			AddExemptionDetails();
			AddDeliveryUndeliveryInformation();
			AddSenderInformation();
		}

		void AddMessageIdentification()
		{
			ElementList list = new ElementList();
			list.AddValue(new Format(1, CharType.Alpha), nameof(ActionCodes.Add));   // Action Code
			list.AddSlant();
			list.AddValue(new Format(1, CharType.Alpha), ZBool.False.ToString());       // Loadlist Indicator (Not used, always False)
			list.AddSlant();
			list.AddValue(new Format(1, CharType.Alpha), IsLate.ToString());            // Late Indicator
			list.AddCRLF();
			Elements.AddHeader(list);
		}

		void AddMAWBConsignmentDetails()
		{
			ElementList list = new ElementList();
			list.AddLineIdentifier(Constants.MAWB);
			list.AddSlant();

			// AWB Identification
			list.AddHeader(GetAWBIdentification(fConsol));

			// AWB Origin and Destination
			list.AddHeader(GetAWBOriginAndDest(fConsol.LoadPort, fConsol.DischargePort));

			// AWB Quantity Details
			list.AddSlant();
			list.AddColumnIdentifier("T");
			ZInt noOfPieces = GetTotalPackageCount(fConsol);
			ZString weightUnit = GetWeightUnit(fConsol);
			ZDecimal grossWeight = GetTotalWeight(fConsol);
			list.AddHeader(GetAWBRateLineTotals(noOfPieces, weightUnit, grossWeight, false));

			list.AddCRLF();

			// Nature of Goods
			list.AddHeader(GetNatureOfGoods(fConsol));

			Elements.AddHeader(list);
		}

		void AddFlightDetails()
		{
			ElementList list = new ElementList();
			list.AddLineIdentifier(Constants.Flight);
			list.AddSlant();

			ZString carrier = ZString.Empty;
			ZString flightNo = ZString.Empty;
			ZDateTime flightDate = ZDateTime.Empty;
			if (DepartureFlight != null)
			{
				carrier = DepartureFlight.JW_VoyageFlight.Left(2);
				flightNo = DepartureFlight.JW_VoyageFlight.SubstringSafe(2, 5);
				if (!flightNo.IsEmpty && flightNo.Length < 3)
				{
					flightNo = flightNo.PadLeft(3, '0');
				}
				flightDate = DepartureFlight.JW_ETD;
			}

			list.AddValue(new Format(2, CharType.AlphaNumeric), carrier);
			list.AddValue(new Format(5, CharType.AlphaNumeric), flightNo);
			list.AddSlant();
			list.AddValue(new Format(2, CharType.Numeric), flightDate.ToString("dd"));
			list.AddValue(new Format(3, CharType.Alpha), flightDate.ToString("MMM"));
			list.AddCRLF();
			Elements.AddHeader(list);
		}

		void AddHAWBSummaryDetails()
		{
			if (!fConsol.IsDirect)
			{
				ElementList list = new ElementList();
				list.AddLineIdentifier(Constants.HAWB);
				list.AddSlant();
				list.AddValue(new Format(17, 1, CharType.AlphaNumeric), fCMDData.HAWBSerialNo);
				list.AddSlant();
				// AWB Quantity Details				
				list.AddHeader(GetAWBRateLineTotals(fCMDData.HAWBNoOfPieces, fCMDData.HAWBWeightCode, fCMDData.HAWBGrossWeight, true));
				list.AddCRLF();
				// Nature of Goods
				list.AddHeader(GetNatureOfGoods(fCMDData.HAWBNatureOfGoods));
				Elements.AddConditionalHeader(list);
			}
		}

		void AddMAWBShipperDetails()
		{
			if (fConsol.SendingForwarder != null)
			{
				ElementList list = new ElementList();
				list.AddLineIdentifier(Constants.Shipper);
				list.AddSlant();
				list.AddValue(new Format(60, 0, CharType.Text), fConsol.SendingForwarder.OH_FullNameTruncated);
				list.AddCRLF();
				list.AddSlant();
				list.AddValue(new Format(65, 0, CharType.Text), string.Concat(fConsol.SendingForwarder.MainAddress.OA_Address1, ' ', fConsol.SendingForwarder.MainAddress.OA_Address2));
				list.AddCRLF();
				Elements.AddOptionalHeader(list);
			}
		}

		void AddMAWBConsigneeDetails()
		{
			if (fConsol.ReceivingForwarder != null)
			{
				ElementList list = new ElementList();
				list.AddLineIdentifier(Constants.Consignee);
				list.AddSlant();
				list.AddValue(new Format(60, 0, CharType.Text), fConsol.ReceivingForwarder.OH_FullNameTruncated);
				list.AddCRLF();
				list.AddSlant();
				list.AddValue(new Format(65, 0, CharType.Text), string.Concat(fConsol.ReceivingForwarder.MainAddress.OA_Address1, ' ', fConsol.ReceivingForwarder.MainAddress.OA_Address2));
				list.AddCRLF();
				Elements.AddOptionalHeader(list);
			}
		}

		void AddPermitDetails()
		{
			ElementList list = new ElementList();
			list.AddLineIdentifier(Constants.Permit.Identifier);
			list.AddSlant();

			// TDB Permits
			// CED Permits are not required anymore, only TDB permits are included
			if (fCMDData.TDBPermitNos.Length > 0)
			{
				ElementList tDBPermits = new ElementList();
				tDBPermits.AddLineIdentifier(Constants.Permit.TDB);
				foreach (ZString permitNo in fCMDData.TDBPermitNos)
				{
					tDBPermits.AddSlant();
					tDBPermits.AddValue(new Format(11, CharType.Text), permitNo.PadRight(11, ' '));
					tDBPermits.AddCRLF();
				}

				list.AddConditionalHeader(tDBPermits);
			}
			else
			{
				list.AddCRLF();
			}

			Elements.AddHeader(list);
		}

		void AddImportMAWBDetails()
		{
			if (IsExport && fCMDData.ImportConsol != null)
			{
				ElementList list = new ElementList();
				list.AddLineIdentifier(Constants.ImportMAWB);
				list.AddSlant();
				list.AddHeader(GetAWBIdentification(fCMDData.ImportConsol));
				list.AddCRLF();
				Elements.AddConditionalHeader(list);
			}
		}

		void AddExemptionDetails()
		{
			ElementList list = new ElementList();
			list.AddLineIdentifier(Constants.Exemption);
			list.AddSlant();
			list.AddValue(new Format(2, CharType.Alpha), fCMDData.ExemptionCode);
			if (!fCMDData.ExemptionRemarks.IsEmpty || fCMDData.ExemptionCode == "ZZ")
			{
				ElementList remarks = new ElementList();
				remarks.AddSlant();
				remarks.AddValue(new Format(25, 0, CharType.Text), fCMDData.ExemptionRemarks);
				list.AddConditionalHeader(remarks);
			}
			list.AddCRLF();
			Elements.AddOptionalHeader(list);
		}

		void AddDeliveryUndeliveryInformation()
		{
			if (IsImport)
			{
				ElementList list = new ElementList();
				list.AddLineIdentifier(Constants.DUI);
				list.AddSlant();

				list.AddValue(new Format(1, CharType.Alpha), fCMDData.GoodsDelivered.ToString());
				list.AddCRLF();
				Elements.AddConditionalHeader(list);
			}
		}

		void AddSenderInformation()
		{
			OrgHeader orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			ZString companyUEN = orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber);

			ElementList list = new ElementList();
			list.AddLineIdentifier(Constants.Sender);
			list.AddSlant();
			list.AddValue(new Format(30, 0, CharType.Text), GlbStaff.CurrentUser.GS_FullName);
			list.AddSlant();
			list.AddValue(new Format(30, 0, CharType.Text), GlbCompany.CurrentCompany.GC_Name);
			list.AddCRLF();
			list.AddSlant();
			list.AddValue(new Format(20, CharType.Text), companyUEN.PadRight(20, ' '));
			list.AddSlant();
			list.AddValue(new Format(6, 0, CharType.AlphaNumeric), SGCustomsDataRegistry.Instance.CargoAgentCode.Value);
			list.AddSlant();
			list.AddValue(new Format(7, CharType.AlphaNumeric), SGCustomsDataRegistry.Instance.CargoAgentRef.Value);
			list.AddCRLF();
			Elements.AddHeader(list);
		}

		ElementList GetAWBIdentification(ForwardingConsol consol)
		{
			ElementList aWBIdentification = new ElementList();

			aWBIdentification.AddValue(new Format(3, CharType.Numeric), consol.MasterBillAirlinePrefix);
			aWBIdentification.AddHyphen();
			if (consol.JK_TransportMode != TransportModeCodeList.Codes.TransportMode_3_Road)
			{
				aWBIdentification.AddValue(new Format(8, CharType.Numeric), consol.MasterBillMAWB);
			}
			else
			{
				var awbNo = consol.JK_MasterBillNum.KeepNumericCharacters();
				var awbMAWB = awbNo.SubstringSafe(3, consol.MasterBillMAWBInfo.MaxLength);
				aWBIdentification.AddValue(new Format(8, CharType.Numeric), awbMAWB);
			}

			return aWBIdentification;
		}

		ElementList GetAWBOriginAndDest(RefUNLOCO origin, RefUNLOCO dest)
		{
			ElementList list = new ElementList();
			list.AddValue(new Format(3, CharType.Alpha), origin.RL_IATA);
			list.AddValue(new Format(3, CharType.Alpha), dest.RL_IATA);
			return list;
		}

		ElementList GetAWBRateLineTotals(ZInt noOfPieces, ZString weightCode, ZDecimal weight, bool addSlant)
		{
			ElementList list = new ElementList();

			list.AddValue(new Format(4, 0, CharType.Numeric), noOfPieces);
			if (addSlant)
			{
				list.AddSlant();
			}

			list.AddValue(new Format(1, CharType.Alpha), weightCode);
			list.AddValue(new Format(7, 0, CharType.NumericWithDecimal), weight);

			return list;
		}

		ElementList GetNatureOfGoods(ForwardingConsol consol)
		{
			Goods[] freightGoods = !consol.IsDirect ? new Goods[] { new Goods("AS PER MANIFEST", "") } : fCMDData.HAWBNatureOfGoods;

			return GetNatureOfGoods(freightGoods);
		}

		ElementList GetNatureOfGoods(Goods[] freightGoods)
		{
			ElementList list = new ElementList();

			foreach (Goods currentGoods in freightGoods)
			{
				list.AddSlant();
				list.AddValue(new Format(20, 0, CharType.Text), currentGoods.ManifestDescription);

				if (!currentGoods.HarmonisedCode.IsEmpty)
				{
					list.AddSlant(StatusType.Optional);
					list.AddOptionalValue(new Format(20, 0, CharType.Text), currentGoods.HarmonisedCode);
				}

				list.AddCRLF();
			}

			return list;
		}

		#endregion

		protected ZBool IsLate
		{
			get { return (LatestSubmissionDate.IsEmpty) ? ZBool.True : (ZBool)(Now > LatestSubmissionDate); }
		}

		protected virtual bool IsExport
		{
			get { return fConsol.IsExport(); }
		}

		protected virtual bool IsImport
		{
			get { return fConsol.IsImport(); }
		}

		protected virtual ZDateTime Now
		{
			get { return ZDateTime.Now; }
		}

		ZDateTime LatestSubmissionDate
		{
			get { return (IsExport) ? ExportLatestSubmissionDate : ImportLatestSubmissionDate; }
		}

		ZDateTime ExportLatestSubmissionDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!fConsol.JK_JX_JA_A_DEP.IsEmpty)
				{
					result = fConsol.JK_JX_JA_A_DEP.AddDays(7).EndOfDay();
				}
				else if (!fConsol.JK_JX_JA_E_DEP.IsEmpty)
				{
					result = fConsol.JK_JX_JA_E_DEP.AddDays(7).EndOfDay();
				}
				return result;
			}
		}

		ZDateTime ImportLatestSubmissionDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (!fConsol.JK_JX_JB_A_ARV.IsEmpty)
				{
					result = fConsol.JK_JX_JB_A_ARV.AddDays(10).EndOfDay();
				}
				else if (!fConsol.JK_JX_JB_E_ARV.IsEmpty)
				{
					result = fConsol.JK_JX_JB_E_ARV.AddDays(10).EndOfDay();
				}
				return result;
			}
		}

		Transport DepartureFlight
		{
			get
			{
				foreach (Transport transport in fConsol.Transports)
				{
					if (transport.JW_TransportType == Core.Constants.TransportPlanningType.Flight1)
					{
						return transport;
					}
				}
				return null;
			}
		}

		void Validate()
		{
			if (fConsol == null || fConsol.IsDeleted)
			{
				throw new OdysseyException("Consol cannot be null or marked as deleted");
			}

			if (fCMDData == null)
			{
				throw new OdysseyException("CMDData cannot be null");
			}
		}

		protected virtual ZInt GetTotalPackageCount(ForwardingConsol consol)
		{
			ZInt totalCount = 0;
			foreach (CommonShipment shipment in consol.Shipments)
			{
				totalCount += (shipment.JS_TotalPackageCount > 0) ? shipment.JS_TotalPackageCount : shipment.JS_OuterPacks;
			}
			return totalCount;
		}

		protected virtual ZString GetWeightUnit(ForwardingConsol consol)
		{
			return (consol.JK_TotalShipmentWeightUnit == Core.Constants.Weight.Pounds) ? "L" : "K";
		}

		protected virtual ZDecimal GetTotalWeight(ForwardingConsol consol)
		{
			return consol.GetTotalShipmentWeightForDoc(Env.Registry.Freight.AirWaybill.AirWaybillMAWBWeightAndVolumeDisplay);
		}

		readonly ForwardingConsol fConsol;
		readonly CMDData fCMDData;

		#endregion
	}
}
