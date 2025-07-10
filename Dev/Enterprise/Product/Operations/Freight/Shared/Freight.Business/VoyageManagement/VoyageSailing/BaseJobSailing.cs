using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[CodeProperty(BaseJobSailing.Schema.JX_JA_RL_NKPortOfLoading), DescriptionProperty(BaseJobSailing.Schema.Description)]
	[ActionFieldFollow(false)]
	[System.Diagnostics.DebuggerDisplay("Sailing (B:{JX_JA_RL_NKPortOfLoading}->{JX_JB_RL_NKPortOfDischarge})")]
	public class BaseJobSailing : AutoJobSailing
	{
		#region Schema

		public new class Schema : AutoJobSailing.Schema
		{
			public const string Description = "Description";
			public const string ExchangeRate = "ExchangeRate";
			public const string JX_JA_RL_NKPortOfLoading = "JX_JA_RL_NKPortOfLoading";
			public const string JX_JB_RL_NKPortOfDischarge = "JX_JB_RL_NKPortOfDischarge";
			public const string JX_Calc_LoadCountry = "JX_Calc_LoadCountry";
			public const string JX_Calc_DischargeCountry = "JX_Calc_DischargeCountry";
			public const string JX_JV_NKVessel = "JX_JV_NKVessel";
			public const string JX_JV_VoyageFlight = "JX_JV_VoyageFlight";
			public const string JX_ETD_From = "JX_ETD_From";
			public const string JX_ETD_To = "JX_ETD_To";
			public const string JX_ETA_From = "JX_ETA_From";
			public const string JX_ETA_To = "JX_ETA_To";
			public const string JX_ShowOnlyReceived = "JX_ShowOnlyReceived";
			public const string JX_ShowOnlyThisSailing = "JX_ShowOnlyThisSailing";
			public const string ShipStatus = "ShipStatus";
			public const string JX_PortFilterType = "JX_PortFilterType";
			public const string JX_RL_NKPort1 = "JX_RL_NKPort1";
			public const string JX_RL_NKPort2 = "JX_RL_NKPort2";
			public const string JX_RN_Country1 = "JX_RN_Country1";
			public const string JX_RN_Country2 = "JX_RN_Country2";
			public const string JX_JA_E_ARV = "JX_JA_E_ARV";
			public const string JX_JA_E_DEP = "JX_JA_E_DEP";
			public const string JX_JB_E_ARV = "JX_JB_E_ARV";
			public const string JX_JB_S_ARV = "JX_JB_S_ARV";
			public const string JX_JA_A_ARV = "JX_JA_A_ARV";
			public const string JX_JA_A_DEP = "JX_JA_A_DEP";
			public const string JX_JB_A_ARV = "JX_JB_A_ARV";
			public const string JX_JA_S_DEP = "JX_JA_S_DEP";
			public const string JX_JA_S_ARV = "JX_JA_S_ARV";
			public const string JX_ContainerNumber = "JX_ContainerNumber";
			public const string JX_JV_OH_Line = "JX_JV_OH_Line";
			public const string JX_JB_IsTranship = "JX_JB_IsTranship";
			public const string JX_TransportMode = "JX_TransportMode";
			public const string JX_JA_DepartureCTOOrg = "JX_JA_DepartureCTOOrg";
			public const string JX_JA_DepartureCTOAddress = "JX_JA_DepartureCTOAddress";
			public const string JX_JB_ArrivalCTOOrg = "JX_JB_ArrivalCTOOrg";
			public const string JX_JB_ArrivalCTOAddress = "JX_JB_ArrivalCTOAddress";
			public const string JX_JV_VoyageType = "JX_JV_VoyageType";
			public const string JX_JA_DepartureBerth = "JX_JA_DepartureBerth";
			public const string JX_JA_DepartureReference = "JX_JA_DepartureReference";
			public const string JX_JB_ArrivalBerth = "JX_JB_ArrivalBerth";
			public const string JX_JB_ArrivalReference = "JX_JB_ArrivalReference";
			public const string JX_JB_ArrivalCTOName = "JX_JB_ArrivalCTOName";
			public const string JX_JB_ArrivalCTOAddress1 = "JX_JB_ArrivalCTOAddress1";
			public const string JX_JB_ArrivalCTOAddress2 = "JX_JB_ArrivalCTOAddress2";
			public const string JX_JB_ArrivalCTOCity = "JX_JB_ArrivalCTOCity";
			public const string JX_JB_ArrivalCTOState = "JX_JB_ArrivalCTOState";
			public const string JX_JB_ArrivalCTOPostCode = "JX_JB_ArrivalCTOPostCode";
			public const string JX_JB_ArrivalCTOPhone = "JX_JB_ArrivalCTOPhone";
			public const string JX_JA_DepartureCTOName = "JX_JA_DepartureCTOName";
			public const string JX_JA_DepartureCTOAddress1 = "JX_JA_DepartureCTOAddress1";
			public const string JX_JA_DepartureCTOAddress2 = "JX_JA_DepartureCTOAddress2";
			public const string JX_JA_DepartureCTOCity = "JX_JA_DepartureCTOCity";
			public const string JX_JA_DepartureCTOState = "JX_JA_DepartureCTOState";
			public const string JX_JA_DepartureCTOPostCode = "JX_JA_DepartureCTOPostCode";
			public const string JX_JA_DepartureCTOPhone = "JX_JA_DepartureCTOPhone";
			public const string JX_JV_IsChartered = "JX_JV_IsChartered";
			public const string JX_JV_LineName = "JX_JV_LineName";
			public const string JX_JV_LineAddress1 = "JX_JV_LineAddress1";
			public const string JX_JV_LineAddress2 = "JX_JV_LineAddress2";
			public const string JX_JV_LineCity = "JX_JV_LineCity";
			public const string JX_JV_LineState = "JX_JV_LineState";
			public const string JX_JV_LinePostCode = "JX_JV_LinePostCode";
			public const string JX_JV_LinePhone = "JX_JV_LinePhone";

			public const string JX_JA_DocumentaryCutoff = "JX_JA_DocumentaryCutoff";
			public const string JX_JA_CTOReceivalCommences = "JX_JA_CTOReceivalCommences";
			public const string JX_JA_CTOCutOff = "JX_JA_CTOCutOff";
			public const string JX_JA_DGFCLReceivalCommences = "JX_JA_DGFCLReceivalCommences";
			public const string JX_JA_DGFCLCutOff = "JX_JA_DGFCLCutOff";
			public const string JX_JA_EmptyReceivalCommences = "JX_JA_EmptyReceivalCommences";
			public const string JX_JA_EmptyCutOff = "JX_JA_EmptyCutOff";
			public const string JX_JA_ReeferReceivalCommences = "JX_JA_ReeferReceivalCommences";
			public const string JX_JA_ReeferCutOff = "JX_JA_ReeferCutOff";
			public const string JX_JB_CTOAvailabilityDate = "JX_JB_CTOAvailabilityDate";
			public const string JX_JB_CTOStorageDate = "JX_JB_CTOStorageDate";
			public const string JX_JA_VGMCutOff = "JX_JA_VGMCutOff";

			public const string StowPlanMessageStatus = "StowPlanMessageStatus";
			public const string StowPlanMessageStatusDescription = "StowPlanMessageStatusDescription";
		}

		#endregion

		public BaseJobSailing(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobVoyage GetVoyageBusinessObject()
		{
			if (Voyage == null)
			{
				return Factory.New(typeof(JobVoyage)) as JobVoyage;
			}
			return Voyage;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobSailingFetchStrategy(this);
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Related Business Objects

		#region Destination

		public VoyageDestination Destination
		{
			get
			{
				var result = Factory.Load<VoyageDestination>(JX_JB);
				if (result != null && result.IsDeleted)
				{
					result = null;
				}
				return result;
			}
		}

		#endregion

		#region Origin

		public VoyageOrigin Origin
		{
			get
			{
				var result = Factory.Load<VoyageOrigin>(JX_JA);
				if (result != null && result.IsDeleted)
				{
					result = null;
				}
				return result;
			}
		}

		#endregion

		#region Voyage

		public JobVoyage Voyage
		{
			get { return (Origin != null) ? Origin.Voyage : null; }
		}

		#endregion

		#region Vessel

		public RefVessel Vessel
		{
			get { return RefVessel.LookupVesselByFK(JX_JV_NKVessel, Factory); }
		}

		#endregion

		#endregion

		#region Properties

		#region Description

		public ZString Description
		{
			get { return JX_JA_E_DEP.ToString(); }
		}

		#endregion

		#region Exchange Rate

		public ZString ExchangeRate
		{
			get
			{
				var rates = Voyage.ExRates.OfType<VoyageExRate>().ToArray();

				var loadPortRates = rates.Where(r => r.E8_RL_NKPort == JX_JA_RL_NKPortOfLoading)
					.OrderBy(r => r.E8_RX_NKExCurrency);
				var discPortRates = rates.Where(r => r.E8_RL_NKPort == JX_JB_RL_NKPortOfDischarge)
					.OrderBy(r => r.E8_RX_NKExCurrency);
				var genericRates = rates.Where(r => r.E8_RL_NKPort.IsEmpty
					&& (loadPortRates.All(lpRate => lpRate.E8_RX_NKExCurrency != r.E8_RX_NKExCurrency)
					|| discPortRates.All(dcRate => dcRate.E8_RX_NKExCurrency != r.E8_RX_NKExCurrency)))
					.OrderBy(r => r.E8_RX_NKExCurrency);

				var rateList = new List<ZString>();

				foreach (var loadPortRate in loadPortRates)
				{
					AddFormattedExRate(rateList, loadPortRate);
				}

				foreach (var discPortRate in discPortRates)
				{
					AddFormattedExRate(rateList, discPortRate);
				}

				foreach (var genericRate in genericRates)
				{
					AddFormattedExRate(rateList, genericRate);
				}

				return string.Join("; ", rateList);
			}
		}

		void AddFormattedExRate(List<ZString> rateList, VoyageExRate exRate)
		{
			if (exRate != null)
			{
				rateList.Add(exRate.E8_RL_NKPort.IsEmpty
					? Res.GetString("6708601c-20aa-477a-8987-e084c6cc3adc",
						"{0} {1:0.00}", exRate.E8_RX_NKExCurrency, exRate.E8_VoyageExchangeRate.Round(2))
					: Res.GetString("5f8bdc07-42e2-403e-828f-d2132d9e4bd8",
						"{0} {1} {2:0.00}", exRate.E8_RX_NKExCurrency, exRate.E8_RL_NKPort, exRate.E8_VoyageExchangeRate.Round(2)));
			}
		}

		#endregion

		#region Origin properties

		#region JX_Calc_LoadCountry

		public ZString JX_Calc_LoadCountry
		{
			get { return (Origin != null && Origin.PortOfLoading != null) ? Origin.PortOfLoading.RL_RN_NKCountryCode : ZString.Empty; }
		}

		#endregion

		#region JX_JA - Origin

		[RelatedBusinessObject("Origin")]
		[ReadOnly(true)]
		public override ZGuid JX_JA
		{
			get
			{
				return base.JX_JA;
			}
			set
			{
				if (JX_JA != value)
				{
					base.JX_JA = value;
				}
			}
		}

		#endregion

		#region JX_JB - Destination

		[RelatedBusinessObject("Destination")]
		[ReadOnly(true)]
		public override ZGuid JX_JB
		{
			get
			{
				return base.JX_JB;
			}
			set
			{
				if (JX_JB != value)
				{
					base.JX_JB = value;
				}
			}
		}

		#endregion

		#region JX_JA_E_ARV

		public ZDateTime JX_JA_E_ARV
		{
			get { return (Origin != null) ? Origin.JA_E_ARV : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_E_ARV); }
		}

		#endregion

		#region JX_JA_A_ARV

		public ZDateTime JX_JA_A_ARV
		{
			get { return (Origin != null) ? Origin.JA_A_ARV : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_A_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_A_ARV); }
		}

		#endregion

		#region JX_JA_E_DEP

		public ZDateTime JX_JA_E_DEP
		{
			get { return (Origin != null) ? Origin.JA_E_DEP : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_E_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_E_DEP); }
		}

		#endregion

		#region JX_JA_A_DEP

		public ZDateTime JX_JA_A_DEP
		{
			get { return (Origin != null) ? Origin.JA_A_DEP : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_A_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_A_DEP); }
		}

		#endregion

		#region JX_JA_S_DEP

		public ZDateTime JX_JA_S_DEP => Origin?.JA_S_DEP ?? ZDateTime.Empty;

		public ZPropertyInfo JX_JA_S_DEPInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_S_DEP); }
		}

		#endregion

		#region JX_JA_S_ARV

		public ZDateTime JX_JA_S_ARV => Origin?.JA_S_ARV ?? ZDateTime.Empty;

		public ZPropertyInfo JX_JA_S_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_S_ARV); }
		}

		#endregion

		#region JX_JA_RL_NKPortOfLoading

		[RelatedBusinessObject("PortOfLoading")]
		[ResourceStringData("JobSailing|JX_JA_RL_NKPortOfLoading", Caption = "Load Port")]
		public virtual ZString JX_JA_RL_NKPortOfLoading
		{
			get { return (Origin != null) ? Origin.JA_RL_NKPortOfLoading : ZString.Empty; }
		}

		public virtual RefUNLOCO PortOfLoading
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JX_JA_RL_NKPortOfLoading); }
		}

		public ZPropertyInfo JX_JA_RL_NKPortOfLoadingInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_RL_NKPortOfLoading); }
		}

		#endregion

		#region JX_JA_DepartureAddress

		#region JX_JA_DepartureCTOOrg

		[RelatedBusinessObject("DepartureCTO")]
		[List("Lookups.CTO_List")]
		public ZGuid JX_JA_DepartureCTOOrg
		{
			get { return (Origin != null) ? Origin.JA_Calc_DepartureCTOAddressOrg : ZGuid.Empty; }
		}

		public OrgHeader DepartureCTO
		{
			get { return (Origin != null && Origin.DepartureCTOAddress != null) ? Origin.DepartureCTOAddress.Header : null; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOOrg); }
		}

		#endregion

		#region JX_JA_DepartureCTOAddress

		[RelatedBusinessObject("DepartureCTOAddress")]
		public ZGuid JX_JA_DepartureCTOAddress
		{
			get { return (Origin != null) ? Origin.JA_OA_DepartureCTOAddress : ZGuid.Empty; }
		}

		public OrgAddress DepartureCTOAddress
		{
			get { return (Origin != null) ? Origin.DepartureCTOAddress : null; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOAddress); }
		}

		#endregion

		#region JX_JA_DepartureCTOName

		public ZString JX_JA_DepartureCTOName
		{
			get { return (DepartureCTO != null) ? DepartureCTO.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTONameInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOName); }
		}

		#endregion

		#region JX_JA_DepartureCTOAddress1

		public ZString JX_JA_DepartureCTOAddress1
		{
			get { return (DepartureCTOAddress != null) ? DepartureCTOAddress.OA_Address1 : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOAddress1Info
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOAddress1); }
		}

		#endregion

		#region JX_JA_DepartureCTOAddress2

		public ZString JX_JA_DepartureCTOAddress2
		{
			get { return (DepartureCTOAddress != null) ? DepartureCTOAddress.OA_Address2 : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOAddress2Info
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOAddress2); }
		}

		#endregion

		#region JX_JA_DepartureCTOCity

		public ZString JX_JA_DepartureCTOCity
		{
			get { return (DepartureCTOAddress != null) ? DepartureCTOAddress.OA_City : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOCityInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOCity); }
		}

		#endregion

		#region JX_JA_DepartureCTOState

		public ZString JX_JA_DepartureCTOState
		{
			get { return (DepartureCTOAddress != null) ? DepartureCTOAddress.OA_State : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOStateInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOState); }
		}

		#endregion

		#region JX_JA_DepartureCTOPostCode

		public ZString JX_JA_DepartureCTOPostCode
		{
			get { return (DepartureCTOAddress != null) ? DepartureCTOAddress.OA_PostCode : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOPostCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOPostCode); }
		}

		#endregion

		#region JX_JA_DepartureCTOPhone

		public ZString JX_JA_DepartureCTOPhone
		{
			get { return (DepartureCTO != null) ? DepartureCTO.MainAddress.OA_Phone : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureCTOPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureCTOPhone); }
		}

		#endregion

		#endregion

		#region JX_JA_DepartureBerth

		public ZString JX_JA_DepartureBerth
		{
			get { return (Origin != null) ? Origin.JA_Berth : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureBerthInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureBerth); }
		}

		#endregion

		#region JX_JA_DepartureReference

		public ZString JX_JA_DepartureReference
		{
			get { return (Origin != null) ? Origin.JA_DepartReference : ZString.Empty; }
		}

		public ZPropertyInfo JX_JA_DepartureReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DepartureReference); }
		}

		#endregion

		#region JX_JA_DocumentaryCutoff

		public ZDateTime JX_JA_DocumentaryCutoff
		{
			get { return Origin != null ? Origin.JA_DocumentaryCutoff : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_DocumentaryCutoffInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DocumentaryCutoff); }
		}

		#endregion

		#region JX_JA_VGMCutoff

		public ZDateTime JX_JA_VGMCutOff
		{
			get { return Origin != null ? Origin.JA_VGMCutOff : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_VGMCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_VGMCutOff); }
		}

		#endregion

		#region JX_JA_CTOReceivalCommences

		public ZDateTime JX_JA_CTOReceivalCommences
		{
			get { return Origin != null ? Origin.JA_ReceivalCommences : ZDateTime.Empty; }
		}
		public ZPropertyInfo JX_JA_CTOReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_CTOReceivalCommences); }
		}

		#endregion

		#region JX_JA_CTOCutOff

		public ZDateTime JX_JA_CTOCutOff
		{
			get { return Origin != null ? Origin.JA_CutOff : ZDateTime.Empty; }
		}
		public ZPropertyInfo JX_JA_CTOCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_CTOCutOff); }
		}

		#endregion

		#region JX_JA_DGFCLReceivalCommences

		public ZDateTime JX_JA_DGFCLReceivalCommences
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				VoyageOrigin origin;
				if ((origin = Origin) != null)
				{
					result = origin.JA_DGReceivalCommences;

					if (result.IsEmpty)
					{
						result = origin.JA_ReceivalCommences;
					}
				}

				return result;
			}
		}
		public ZPropertyInfo JX_JA_DGFCLReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DGFCLReceivalCommences); }
		}

		#endregion

		#region JX_JA_DGFCLCutOff

		public ZDateTime JX_JA_DGFCLCutOff
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				VoyageOrigin origin;
				if ((origin = Origin) != null)
				{
					result = origin.JA_DGCutOff;

					if (result.IsEmpty)
					{
						result = origin.JA_CutOff;
					}
				}

				return result;
			}
		}
		public ZPropertyInfo JX_JA_DGFCLCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_DGFCLCutOff); }
		}

		#endregion

		#region JX_JA_EmptyReceivalCommences

		public ZDateTime JX_JA_EmptyReceivalCommences
		{
			get { return Origin != null ? Origin.JA_EmptyReceivalCommences : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_EmptyReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_EmptyReceivalCommences); }
		}

		#endregion

		#region JX_JA_EmptyCutOff

		public ZDateTime JX_JA_EmptyCutOff
		{
			get { return Origin != null ? Origin.JA_EmptyCutOff : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_EmptyCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_EmptyCutOff); }
		}

		#endregion

		#region JX_JA_ReeferReceivalCommences

		public ZDateTime JX_JA_ReeferReceivalCommences
		{
			get { return Origin != null ? Origin.JA_ReeferReceivalCommences : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_ReeferReceivalCommencesInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_ReeferReceivalCommences); }
		}

		#endregion

		#region JX_JA_ReeferCutOff

		public ZDateTime JX_JA_ReeferCutOff
		{
			get { return Origin != null ? Origin.JA_ReeferCutOff : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JA_ReeferCutOffInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JA_ReeferCutOff); }
		}

		#endregion

		#endregion

		#region Destination properties

		#region JX_Calc_DischargeCountry

		public ZString JX_Calc_DischargeCountry
		{
			get { return (Destination != null && Destination.PortOfDischarge != null) ? Destination.PortOfDischarge.RL_RN_NKCountryCode : ZString.Empty; }
		}

		#endregion

		#region JX_JB_E_ARV

		public ZDateTime JX_JB_E_ARV
		{
			get { return (Destination != null) ? Destination.JB_E_ARV : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JB_E_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_E_ARV); }
		}

		#endregion

		#region JX_JB_A_ARV

		public ZDateTime JX_JB_A_ARV
		{
			get { return (Destination != null) ? Destination.JB_A_ARV : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JB_A_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_A_ARV); }
		}

		#endregion

		#region JX_JB_S_ARV

		public ZDateTime JX_JB_S_ARV
		{
			get { return (Destination != null) ? Destination.JB_S_ARV : ZDateTime.Empty; }
		}

		public ZPropertyInfo JX_JB_S_ARVInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_S_ARV); }
		}

		#endregion

		#region JX_JB_RL_NKPortOfDischarge

		[RelatedBusinessObject("PortOfDischarge")]
		[ResourceStringData("JobSailing|JX_JB_RL_NKPortOfDischarge", Caption = "Discharge Port")]
		public virtual ZString JX_JB_RL_NKPortOfDischarge
		{
			get { return (Destination != null) ? Destination.JB_RL_NKPortOfDischarge : ZString.Empty; }
		}

		public virtual RefUNLOCO PortOfDischarge
		{
			get { return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, JX_JB_RL_NKPortOfDischarge); }
		}

		public ZPropertyInfo JX_JB_RL_NKPortOfDischargeInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_RL_NKPortOfDischarge); }
		}

		#endregion

		#region JX_JB_ArrivalCTOAddress

		#region JX_JB_ArrivalCTOOrg

		[RelatedBusinessObject("ArrivalCTO")]
		[List("Lookups.CTO_List")]
		public ZGuid JX_JB_ArrivalCTOOrg
		{
			get { return (Destination != null) ? Destination.JB_Calc_ArrivalCTOAddressOrg : ZGuid.Empty; }
		}

		public OrgHeader ArrivalCTO
		{
			get { return (Destination != null && Destination.ArrivalCTOAddress != null) ? Destination.ArrivalCTOAddress.Header : null; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOOrg); }
		}

		#endregion

		#region JX_JB_ArrivalCTOName

		public ZString JX_JB_ArrivalCTOName
		{
			get { return (ArrivalCTO != null) ? ArrivalCTO.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTONameInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOName); }
		}

		#endregion

		#region JX_JB_ArrivalCTOAddress

		[RelatedBusinessObject("ArrivalCTOAddress")]
		public ZGuid JX_JB_ArrivalCTOAddress
		{
			get { return (Destination != null) ? Destination.JB_OA_ArrivalCTOAddress : ZGuid.Empty; }
		}

		public OrgAddress ArrivalCTOAddress
		{
			get { return (Destination != null) ? Destination.ArrivalCTOAddress : null; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOAddressInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOAddress); }
		}

		#endregion

		#region JX_JB_ArrivalCTOAddress1

		public ZString JX_JB_ArrivalCTOAddress1
		{
			get { return (ArrivalCTOAddress != null) ? ArrivalCTOAddress.OA_Address1 : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOAddress1Info
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOAddress1); }
		}

		#endregion

		#region JX_JB_ArrivalCTOAddress2

		public ZString JX_JB_ArrivalCTOAddress2
		{
			get { return (ArrivalCTOAddress != null) ? ArrivalCTOAddress.OA_Address2 : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOAddress2Info
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOAddress2); }
		}

		#endregion

		#region JX_JB_ArrivalCTOCity

		public ZString JX_JB_ArrivalCTOCity
		{
			get { return (ArrivalCTOAddress != null) ? ArrivalCTOAddress.OA_City : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOCityInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOCity); }
		}

		#endregion

		#region JX_JB_ArrivalCTOState

		public ZString JX_JB_ArrivalCTOState
		{
			get { return (ArrivalCTOAddress != null) ? ArrivalCTOAddress.OA_State : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOStateInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOState); }
		}

		#endregion

		#region JX_JB_ArrivalCTOPostCode

		public ZString JX_JB_ArrivalCTOPostCode
		{
			get { return (ArrivalCTOAddress != null) ? ArrivalCTOAddress.OA_PostCode : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOPostCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOPostCode); }
		}

		#endregion

		#region JX_JB_ArrivalCTOPhone

		public ZString JX_JB_ArrivalCTOPhone
		{
			get { return (ArrivalCTO != null) ? ArrivalCTO.MainAddress.OA_Phone : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalCTOPhoneInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalCTOPhone); }
		}

		#endregion

		#endregion

		#region JX_JB_ArrivalBerth

		public ZString JX_JB_ArrivalBerth
		{
			get { return (Destination != null) ? Destination.JB_Berth : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalBerthInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalBerth); }
		}

		#endregion

		#region JX_JB_ArrivalReference

		public ZString JX_JB_ArrivalReference
		{
			get { return (Destination != null) ? Destination.JB_ArrivalReference : ZString.Empty; }
		}

		public ZPropertyInfo JX_JB_ArrivalReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_ArrivalReference); }
		}

		#endregion

		#region JX_JB_IsTranship

		public ZBool JX_JB_IsTranship
		{
			get { return (Destination != null) ? Destination.JB_IsTranshipment : ZBool.False; }
		}

		public ZPropertyInfo JX_JB_IsTranshipInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_IsTranship); }
		}

		#endregion

		#region JX_JB_CTOAvailabilityDate

		public ZDateTime JX_JB_CTOAvailabilityDate
		{
			get { return Destination != null ? Destination.JB_AvailabilityDate : ZDateTime.Empty; }
		}
		public ZPropertyInfo JX_JB_CTOAvailabilityDateInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_CTOAvailabilityDate); }
		}

		#endregion

		#region JX_JB_StorageDate

		public ZDateTime JX_JB_CTOStorageDate
		{
			get { return Destination != null ? Destination.JB_StorageDate : ZDateTime.Empty; }
		}
		public ZPropertyInfo JX_JB_CTOStorageDateInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JB_CTOStorageDate); }
		}

		#endregion

		#endregion

		#region Voyage properties

		#region JX_JV_VoyageFlight

		public ZString JX_JV_VoyageFlight
		{
			get { return (Voyage != null) ? Voyage.JV_VoyageFlight : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_VoyageFlightInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_VoyageFlight); }
		}

		#endregion

		#region JX_JV_NKVessel

		public ZString JX_JV_NKVessel
		{
			get { return (Voyage != null) ? Voyage.JV_RV_NKVessel : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_NKVesselInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_NKVessel); }
		}

		#endregion

		#region JX_JV_OH_Line

		[RelatedBusinessObject("Line")]
		[List("Lookups.Carrier_List")]
		public ZGuid JX_JV_OH_Line
		{
			get { return (Voyage != null) ? Voyage.JV_OH_Line : ZGuid.Empty; }
		}

		public OrgHeader Line
		{
			get { return (Voyage != null) ? Voyage.Line : null; }
		}

		public ZPropertyInfo JX_JV_OH_LineInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_OH_Line); }
		}

		#endregion

		#region JX_JV_LineName

		public ZString JX_JV_LineName
		{
			get { return (Line != null) ? Line.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LineNameInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LineName); }
		}

		#endregion

		#region JX_JV_LineAddress1

		public ZString JX_JV_LineAddress1
		{
			get { return (Line != null) ? Line.MainAddress.OA_Address1 : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LineAddress1Info
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LineAddress1); }
		}

		#endregion

		#region JX_JV_LineAddress2

		public ZString JX_JV_LineAddress2
		{
			get { return (Line != null) ? Line.MainAddress.OA_Address2 : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LineAddress2Info
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LineAddress2); }
		}

		#endregion

		#region JX_JV_IsChartered

		public ZBool JX_JV_IsChartered
		{
			get { return (Voyage == null ? ZBool.False : Voyage.JV_IsChartered); }
		}

		public ZPropertyInfo JX_JV_IsCharteredInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_IsChartered); }
		}

		#endregion

		#region JX_JV_LineCity

		public ZString JX_JV_LineCity
		{
			get { return (Line != null) ? Line.MainAddress.OA_City : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LineCityInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LineCity); }
		}

		#endregion

		#region JX_JV_LineState

		public ZString JX_JV_LineState
		{
			get { return (Line != null) ? Line.MainAddress.OA_State : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LineStateInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LineState); }
		}

		#endregion

		#region JX_JV_LinePostCode

		public ZString JX_JV_LinePostCode
		{
			get { return (Line != null) ? Line.MainAddress.OA_PostCode : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LinePostCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LinePostCode); }
		}

		#endregion

		#region JX_JV_LinePhone

		public ZString JX_JV_LinePhone
		{
			get { return (Line != null) ? Line.MainAddress.OA_Phone : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_LinePhoneInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_LinePhone); }
		}

		#endregion

		#region JX_TransportMode

		public ZString JX_TransportMode
		{
			get { return (Voyage != null) ? Voyage.JV_AirSeaRoad : ZString.Empty; }
		}

		public ZPropertyInfo JX_TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.JX_TransportMode); }
		}

		#endregion

		#region JX_JV_VoyageType

		public ZString JX_JV_VoyageType
		{
			get { return (Voyage != null) ? Voyage.JV_VoyageType : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_VoyageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JX_JV_VoyageType); }
		}

		#endregion

		#region JX_JV_AircraftType

		public ZString JX_JV_AircraftType
		{
			get { return (Voyage != null) ? Voyage.JV_AircraftType : ZString.Empty; }
		}

		public ZPropertyInfo JX_JV_AircraftTypeInfo
		{
			get { return GetZPropertyInfo(nameof(JX_JV_AircraftType)); }
		}

		#endregion

		#endregion

		[ReadOnly(true)]
		public override ZString JX_UniqueReference
		{
			get { return base.JX_UniqueReference; }
			set { base.JX_UniqueReference = value; }
		}

		#endregion

		#region GetNewLookups

		public new BaseJobSailingLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobSailingLookups)GetNewLookups()); }
		}
		BaseJobSailingLookups lookups;

		protected override JobSailingLookups GetNewLookups()
		{
			return new BaseJobSailingLookups(this);
		}

		#endregion

		public ZString StowPlanMessageStatus
		{
			get
			{
				var result = ZString.Empty;
				var origin = Origin;
				var destination = Destination;
				if (destination != null && !destination.StowPlanMessageStatus.IsEmpty)
				{
					result = destination.StowPlanMessageStatus;
				}
				else if (origin != null)
				{
					result = origin.StowPlanMessageStatus;
				}
				return result;
			}
		}

		public ZString StowPlanMessageStatusDescription
		{
			get { return Factory.GetCachedValue<MessageStatusListSTW>().GetDescriptionFromCode(StowPlanMessageStatus); }
		}

		public ZString OnlineScheduleStatusDescription => JobConsolTransportLookups.GetFlightStatusList(Factory).GetDescriptionFromCode(JX_OnlineScheduleStatus);

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateReferenceNumberIfNeeded();
		}

		#endregion

		#region Reference Number

		void PopulateReferenceNumberIfNeeded()
		{
			if (!IsDeleted
				&& JX_UniqueReference.IsEmpty
				&& TryGenerateReferenceNumbers(Factory, 1, out var generatedNumbers)
				&& generatedNumbers.Length > 0)
			{
				JX_UniqueReference = generatedNumbers[0];
			}
		}

		public static bool TryGenerateReferenceNumbers(BusinessObjectFactory factory, int numberToGenerate, out string[] generatedNumbers)
		{
			if (factory is IDbConnected connected
				&& numberToGenerate > 0)
			{
				var fountain = Env.NumberFountains.SailingScheduleReferenceNumber(Schema.JX_UniqueReferenceMaxLength);
				generatedNumbers = fountain.GetNextsFormatted(connected.Connection, numberToGenerate);
				return true;
			}

			generatedNumbers = Array.Empty<string>();
			return false;
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (kind >= TestBusinessObjectKind.MinimumRequiredToSave
				&& Voyage == null)
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>(kind);
				var origin = voyage.Origins.AddNew();
				var destination = voyage.Destinations.AddNew();

				JX_JA = origin.PK;
				JX_JB = destination.PK;

				origin.FillWithValidTestData(kind, propertyPath);
				destination.FillWithValidTestData(kind, propertyPath);
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
