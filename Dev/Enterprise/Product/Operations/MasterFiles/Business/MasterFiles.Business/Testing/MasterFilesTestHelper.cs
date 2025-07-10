#if DEBUG
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class MasterFilesTestHelper : TestCaseWithFactory
	{
		public MasterFilesTestHelper()
		{
		}

		public MasterFilesTestHelper(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}

		public static OrgCusCode AddSCAC(OrgHeader org, ZString sCAC)
		{
			OrgCusCode customsCode = null;
			if (org != null)
			{
				MasterFilesTestHelper helper = new MasterFilesTestHelper(org.Factory);
				customsCode = helper.UpdateOrAddCustomsRegNo(org, sCAC, OrgCusCode.CodeTypes.CarrierCode, helper.UnitedStates);
			}
			return customsCode;
		}

		public OrgHeader CreateOrganisation(ZString fullName, ZString closestPort)
		{
			return CreateOrganisation(fullName, closestPort, "STREET 1", "CITY", "0987654321");
		}

		public OrgHeader CreateOrganisation(ZString fullName, ZString closestPort, ZString address1, ZString city, ZString phone)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = fullName;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_Phone = phone;
			return org;
		}

		public static void FillWithValidTestDataSoFormSaveWorks(OrgHeader org)
		{
			org.OH_FullName = "MAIORGSYD";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "123 Fake St";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_PostCode = "2000";
			org.MainAddress.OA_State = "NSW";
			org.OH_IsConsignee = true;
		}

		public OrgContact UpdateContact(OrgContact contact, ZString name)
		{
			contact.OC_ContactName = name;
			contact.OC_Phone = "0123456789";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			document.OD_DefaultContact = true;
			return contact;
		}

		static readonly Random Generator = new Random();
		public static string GetRandomString(int stringLength)
		{
			string result = "";
			for (int i = 0; i < stringLength; i++)
			{
				int index = Generator.Next(65, 90);
				result += (char)index;
			}
			return result;
		}

		#region Consignor
		public OrgHeader Consignor
		{
			get
			{
				if (fConsignor == null)
				{
					fConsignor = CreateConsignor();
				}
				return fConsignor;
			}
		}
		OrgHeader fConsignor;

		public virtual OrgHeader CreateConsignor()
		{
			OrgHeader org = CreateOrganisation("CONSIGNOR NAME", AUSYD.Code);
			org.OH_IsConsignor = true;
			return org;
		}
		#endregion

		#region ConsignorContact
		public OrgContact ConsignorContact
		{
			get
			{
				if (fConsignorContact == null)
				{
					fConsignorContact = UpdateContact(Consignor.Contacts.AddNew(), "CONSIGNOR CONTACT");
				}
				return fConsignorContact;
			}
		}
		OrgContact fConsignorContact;
		#endregion

		#region Consignee
		public OrgHeader Consignee
		{
			get
			{
				if (fConsignee == null)
				{
					fConsignee = CreateConsignee();
				}
				return fConsignee;
			}
		}
		OrgHeader fConsignee;

		public virtual OrgHeader CreateConsignee()
		{
			OrgHeader org = CreateOrganisation("CONSIGNEE NAME", USLAX.Code);
			org.OH_IsConsignee = true;
			return org;
		}
		#endregion

		#region ConsigneeContact
		public OrgContact ConsigneeContact
		{
			get
			{
				if (fConsigneeContact == null)
				{
					fConsigneeContact = UpdateContact(Consignee.Contacts.AddNew(), "CONSIGNEE CONTACT");
				}
				return fConsigneeContact;
			}
		}
		OrgContact fConsigneeContact;
		#endregion

		#region NotifyParty
		public OrgHeader NotifyParty
		{
			get
			{
				if (fNotifyParty == null)
				{
					fNotifyParty = CreateNotifyParty();
				}
				return fNotifyParty;
			}
		}
		OrgHeader fNotifyParty;

		public virtual OrgHeader CreateNotifyParty()
		{
			OrgHeader org = CreateOrganisation("NOTIFY PARTY NAME", NonUSPortWithoutUSCode.Code);
			return org;
		}
		#endregion

		#region NotifyPartyContact
		public OrgContact NotifyPartyContact
		{
			get
			{
				if (fNotifyPartyContact == null)
				{
					fNotifyPartyContact = UpdateContact(NotifyParty.Contacts.AddNew(), "NOTIFY PARTY CONTACT");
				}
				return fNotifyPartyContact;
			}
		}
		OrgContact fNotifyPartyContact;
		#endregion

		#region ImportForwarder
		public OrgHeader ImportForwarder
		{
			get
			{
				if (fImportForwarder == null)
				{
					fImportForwarder = CreateImportForwarder();
				}
				return fImportForwarder;
			}
		}
		OrgHeader fImportForwarder;

		public virtual OrgHeader CreateImportForwarder()
		{
			OrgHeader org = CreateOrganisation("IMPORT FORWARDER NAME", USPortWithoutUSCode.Code);
			org.OH_IsForwarder = true;
			return org;
		}
		#endregion

		#region ImportForwarderContact
		public OrgContact ImportForwarderContact
		{
			get
			{
				if (importForwarderContact == null)
				{
					importForwarderContact = UpdateContact(ImportForwarder.Contacts.AddNew(), "IMPORT FORWARDER CONTACT");
				}
				return importForwarderContact;
			}
		}
		OrgContact importForwarderContact;
		#endregion

		#region ExportForwarder
		public OrgHeader ExportForwarder
		{
			get
			{
				if (fExportForwarder == null)
				{
					fExportForwarder = CreateExportForwarder();
				}
				return fExportForwarder;
			}
		}
		OrgHeader fExportForwarder;

		public virtual OrgHeader CreateExportForwarder()
		{
			OrgHeader org = CreateOrganisation("EXPORT FORWARDER NAME", NZAKL.Code);
			org.OH_IsForwarder = true;
			return org;
		}
		#endregion

		#region ExportForwarderContact
		public OrgContact ExportForwarderContact
		{
			get
			{
				if (exportForwarderContact == null)
				{
					exportForwarderContact = UpdateContact(ExportForwarder.Contacts.AddNew(), "EXPORT FORWARDER CONTACT");
				}
				return exportForwarderContact;
			}
		}
		OrgContact exportForwarderContact;
		#endregion

		#region ShippingLine
		public OrgHeader ShippingLine
		{
			get
			{
				if (fShippingLine == null)
				{
					fShippingLine = CreateShippingLine();
				}
				return fShippingLine;
			}
		}
		OrgHeader fShippingLine;

		public virtual OrgHeader CreateShippingLine()
		{
			OrgHeader org = CreateOrganisation("SHIPPING LINE NAME", NZAKL.Code);
			org.OH_IsShippingLine = true;
			org.OH_IsShippingProvider = true;
			PopulateSCAC(org, ShippingLineSCAC);
			return org;
		}

		public const string ShippingLineSCAC = "VOCC";

		#endregion

		#region UnitedStates
		public RefCountry UnitedStates
		{
			get
			{
				if (fUnitedStates == null)
				{
					fUnitedStates = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates);
				}
				return fUnitedStates;
			}
		}
		RefCountry fUnitedStates;
		#endregion

		#region Australia
		public RefCountry Australia
		{
			get
			{
				if (australia == null)
				{
					australia = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
				}
				return australia;
			}
		}
		RefCountry australia;
		#endregion

		#region NewZealand
		public RefCountry NewZealand
		{
			get
			{
				if (newZealand == null)
				{
					newZealand = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.NewZealand);
				}
				return newZealand;
			}
		}
		RefCountry newZealand;
		#endregion

		#region Singapore
		public RefCountry Singapore
		{
			get
			{
				if (singapore == null)
				{
					singapore = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Singapore);
				}
				return singapore;
			}
		}
		RefCountry singapore;
		#endregion

		#region Thailand
		public RefCountry Thailand
		{
			get
			{
				if (thailand == null)
				{
					thailand = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Thailand);
				}
				return thailand;
			}
		}
		RefCountry thailand;
		#endregion

		#region Canada
		public RefCountry Canada
		{
			get { return fCanada ?? (fCanada = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Canada)); }
		}
		RefCountry fCanada;
		#endregion

		#region AUSYD
		public RefUNLOCO AUSYD
		{
			get
			{
				if (fAUSYD == null)
				{
					fAUSYD = CreateUnlocoIfNotExists("AUSYD", Australia);
					AddUSScheduleCode(Schedule.K, fAUSYD, AUSYDScheduleDOrK, ZString.Empty);
				}
				return fAUSYD;
			}
		}
		RefUNLOCO fAUSYD;
		public const string AUSYDScheduleDOrK = "60267";
		#endregion

		#region AUBNE
		public RefUNLOCO AUBNE
		{
			get
			{
				if (fAUBNE == null)
				{
					fAUBNE = CreateUnlocoIfNotExists("AUBNE", Australia);
					AddUSScheduleCode(Schedule.K, fAUBNE, AUBNEScheduleDOrK, ZString.Empty);
				}
				return fAUBNE;
			}
		}
		RefUNLOCO fAUBNE;
		public const string AUBNEScheduleDOrK = "60210";
		#endregion

		#region AUMEL
		public RefUNLOCO AUMEL
		{
			get
			{
				if (fAUMEL == null)
				{
					fAUMEL = CreateUnlocoIfNotExists("AUMEL", Australia);
					AddUSScheduleCode(Schedule.K, fAUMEL, AUMELScheduleDOrK, ZString.Empty);
				}
				return fAUMEL;
			}
		}
		RefUNLOCO fAUMEL;
		public const string AUMELScheduleDOrK = "60237";
		#endregion

		#region NZAKL
		public RefUNLOCO NZAKL
		{
			get
			{
				if (fNZAKL == null)
				{
					fNZAKL = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
					if (fNZAKL == null)
					{
						fNZAKL = Factory.New<RefUNLOCO>();
						fNZAKL.RL_Code = "NZAKL";
						fNZAKL.RL_RN_NKCountryCode = NewZealand.Code;
					}
					AddUSScheduleCode(Schedule.K, fNZAKL, NZAKLScheduleDOrK, ZString.Empty);
				}
				return fNZAKL;
			}
		}
		RefUNLOCO fNZAKL;
		public const string NZAKLScheduleDOrK = "61401";
		#endregion

		#region HongKong
		public RefCountry HongKong
		{
			get
			{
				if (hongKong == null)
				{
					hongKong = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.HongKong);
				}
				return hongKong;
			}
		}
		RefCountry hongKong;
		#endregion

		#region USLAX
		public RefUNLOCO USLAX
		{
			get
			{
				if (fUSLAX == null)
				{
					fUSLAX = CreateUnlocoIfNotExists("USLAX", UnitedStates);
					AddUSScheduleCode(Schedule.D, fUSLAX, USLAXScheduleDOrK, USLocoMapSystemUsageList.Codes.Sea);
				}
				return fUSLAX;
			}
		}
		RefUNLOCO fUSLAX;
		public const string USLAXScheduleDOrK = "2704";
		#endregion

		#region SGSIN
		public RefUNLOCO SGSIN
		{
			get
			{
				if (fSGSIN == null)
				{
					fSGSIN = CreateUnlocoIfNotExists("SGSIN", Singapore);
					AddUSScheduleCode(Schedule.K, fSGSIN, SGSINScheduleDOrK, ZString.Empty);
				}
				return fSGSIN;
			}
		}
		RefUNLOCO fSGSIN;
		public const string SGSINScheduleDOrK = "55976";
		#endregion

		#region THBKK
		public RefUNLOCO THBKK
		{
			get
			{
				if (fTHBKK == null)
				{
					fTHBKK = CreateUnlocoIfNotExists("THBKK", Thailand);
					AddUSScheduleCode(Schedule.K, fTHBKK, THBKKScheduleDOrK, ZString.Empty);
				}
				return fTHBKK;
			}
		}
		RefUNLOCO fTHBKK;
		public const string THBKKScheduleDOrK = "54901";
		#endregion

		#region USCHI
		public RefUNLOCO USCHI
		{
			get
			{
				if (fUSCHI == null)
				{
					fUSCHI = CreateUnlocoIfNotExists("USCHI", UnitedStates);
					AddUSScheduleCode(Schedule.D, fUSCHI, USCHIScheduleDOrK, USLocoMapSystemUsageList.Codes.All);
				}
				return fUSCHI;
			}
		}
		RefUNLOCO fUSCHI;
		public const string USCHIScheduleDOrK = "3901";
		#endregion

		#region CATOR
		public RefUNLOCO CATOR
		{
			get
			{
				if (fCATOR == null)
				{
					fCATOR = CreateUnlocoIfNotExists("CATOR", Canada);
					AddUSScheduleCode(Schedule.K, fCATOR, CATORScheduleDOrK, ZString.Empty);
				}
				return fCATOR;
			}
		}
		RefUNLOCO fCATOR;
		public const string CATORScheduleDOrK = "01535";
		#endregion

		#region CAVAR
		public RefUNLOCO CAVAR
		{
			get
			{
				if (fCAVAR == null)
				{
					fCAVAR = CreateUnlocoIfNotExists("CAVAR", Canada);
					AddUSScheduleCode(Schedule.K, fCAVAR, CAVARScheduleDOrK, ZString.Empty);
				}
				return fCAVAR;
			}
		}
		RefUNLOCO fCAVAR;
		public const string CAVARScheduleDOrK = "12493";
		#endregion

		#region NonUSPortWithoutUSCode
		public RefUNLOCO NonUSPortWithoutUSCode
		{
			get
			{
				if (fNonUSPortWithoutUSCode == null)
				{
					fNonUSPortWithoutUSCode = CreateUnlocoIfNotExists("AUZZZ", Australia);
				}
				return fNonUSPortWithoutUSCode;
			}
		}
		RefUNLOCO fNonUSPortWithoutUSCode;
		#endregion

		#region USPortWithoutUSCode
		public RefUNLOCO USPortWithoutUSCode
		{
			get
			{
				if (fUSPortWithoutUSCode == null)
				{
					fUSPortWithoutUSCode = CreateUnlocoIfNotExists("USZZZ", UnitedStates);
				}
				return fUSPortWithoutUSCode;
			}
		}
		RefUNLOCO fUSPortWithoutUSCode;
		#endregion

		#region Container40NO
		public RefContainer Container40NO
		{
			get
			{
				if (fContainer40NO == null)
				{
					fContainer40NO = CreateOrUpdateContainer("40NO", 40m, 8.6m, 8, "4000", "");
				}
				return fContainer40NO;
			}
		}
		RefContainer fContainer40NO;
		#endregion

		#region Container40US
		public RefContainer Container40US
		{
			get
			{
				if (fContainer40US == null)
				{
					fContainer40US = CreateOrUpdateContainer("40US", 40m, 8.6m, 8, "4000", "40");
				}
				return fContainer40US;
			}
		}
		RefContainer fContainer40US;
		#endregion

		#region Container20NO
		public RefContainer Container20NO
		{
			get
			{
				if (fContainer20NO == null)
				{
					fContainer20NO = CreateOrUpdateContainer("20NO", 20m, 8.6m, 8, "2000", "");
				}
				return fContainer20NO;
			}
		}
		RefContainer fContainer20NO;
		#endregion

		#region Container20US
		public RefContainer Container20US
		{
			get
			{
				if (fContainer20US == null)
				{
					fContainer20US = CreateOrUpdateContainer("20US", 20m, 8.6m, 8, "2000", "20");
				}
				return fContainer20US;
			}
		}
		RefContainer fContainer20US;
		#endregion

		#region VesselWithoutCountry
		public RefVessel VesselWithoutCountry
		{
			get
			{
				if (fVesselWithoutCountry == null)
				{
					fVesselWithoutCountry = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, "VESSEL WITHOUT COUNTRY");
					if (fVesselWithoutCountry == null)
					{
						fVesselWithoutCountry = Factory.New<RefVessel>();
						fVesselWithoutCountry.RV_Code = "VESSEL WITHOUT COUNTRY";
						fVesselWithoutCountry.RV_LloydsNumber = "9191917";
					}
				}
				return fVesselWithoutCountry;
			}
		}
		RefVessel fVesselWithoutCountry;
		#endregion

		#region VesselWithCountry
		public RefVessel VesselWithCountry
		{
			get
			{
				if (fVesselWithCountry == null)
				{
					fVesselWithCountry = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, "VESSEL WITH COUNTRY");
					if (fVesselWithCountry == null)
					{
						fVesselWithCountry = Factory.New<RefVessel>();
						fVesselWithCountry.RV_Code = "VESSEL WITH COUNTRY";
						fVesselWithCountry.RV_LloydsNumber = "8181812";
						fVesselWithCountry.RV_RN_NKCountryOfReg = HongKong.Code;
					}
				}
				return fVesselWithCountry;
			}
		}
		RefVessel fVesselWithCountry;
		#endregion

		#region UNDG0483
		public UNDGSubstance UNDG0483
		{
			get
			{
				if (fUNDG0483 == null)
				{
					ZQuery uNDGQuery = new ZQuery(UNDGSubstanceSchema.DG_UNNO, "0483");
					uNDGQuery.AddToFilter(UNDGSubstanceSchema.DG_Variant, "a");
					fUNDG0483 = Factory.LoadTop1<UNDGSubstance>(uNDGQuery);
					if (fUNDG0483 == null)
					{
						fUNDG0483 = Factory.New<UNDGSubstance>();
						fUNDG0483.DG_UNNO = "0483";
						fUNDG0483.DG_Class = "4.3";
						fUNDG0483.DG_PG = "II";
					}
				}
				return fUNDG0483;
			}
		}
		UNDGSubstance fUNDG0483;
		#endregion

		public RefContainer CreateOrUpdateContainer(ZString code, ZDecimal length, ZDecimal height, ZDecimal width, ZString iSOType, ZString uSContainerCode)
		{
			ZQuery codeQuery = new ZQuery(RefContainerSchema.RC_Code, code);
			RefContainer result = Factory.LoadTop1<RefContainer>(codeQuery);
			if (result == null)
			{
				result = Factory.New<RefContainer>();
				result.RC_Code = code;
			}
			result.RC_ShippingMode = "SEA";
			result.RC_Description = code + " Description";
			result.RC_Length = length;
			result.RC_Height = height;
			result.RC_Width = width;
			result.RC_ISOType = iSOType;
			result.RC_IsIso = !result.RC_ISOType.IsEmpty;
			result.SetCountrySpecificContainerCode(uSContainerCode, "US");
			result.RC_TEU = Math.Ceiling(result.RC_Length / 2);
			return result;
		}

		public void AddUSScheduleCode(Schedule schedule, RefUNLOCO unloco, ZString uSCode, string usageCode)
		{
			RefLocoMap locoMap = null;

			foreach (RefLocoMap map in unloco.RefLocoMaps.Find(USScheduleLocoMapQuery(schedule)))
			{
				locoMap = map;
				break;
			}

			if (locoMap == null)
			{
				locoMap = unloco.RefLocoMaps.AddNew();
				locoMap.RY_SystemUsage = schedule == Schedule.D ? usageCode : USLocoMapSystemUsageList.Codes.SCK;
				locoMap.RY_RN = UnitedStates.PK;
			}
			locoMap.RY_LocalPortCode = uSCode;
		}

		#region USLocoMapQuery
		protected ZQuery USScheduleLocoMapQuery(Schedule schedule)
		{
			ZQuery result = new ZQuery();
			if (schedule == Schedule.D)
			{
				string[] systemUsage = new[] { USLocoMapSystemUsageList.Codes.Air, USLocoMapSystemUsageList.Codes.Sea, USLocoMapSystemUsageList.Codes.All, USLocoMapSystemUsageList.Codes.SCD };
				result.AddToFilter(RefLocoMapSchema.RY_SystemUsage, systemUsage);
			}
			else
			{
				result.AddToFilter(RefLocoMapSchema.RY_SystemUsage, USLocoMapSystemUsageList.Codes.SCK);
			}
			result.AddToFilter(RefLocoMapSchema.RY_RN, UnitedStates.PK);
			return result;
		}
		#endregion

		public void PopulateSCAC(OrgHeader org, ZString sCAC)
		{
			if (org != null)
			{
				org.SetCustomsCode(OrgCusCode.CodeTypes.CarrierCode, UnitedStates, sCAC);
			}
		}

		public RefUNLOCO CreateUnlocoIfNotExists(ZString code, RefCountry country)
		{
			RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (unloco == null)
			{
				unloco = Factory.New<RefUNLOCO>();
				unloco.RL_Code = code;
				unloco.RL_RN_NKCountryCode = country.Code;
				ZQuery query = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, country.RN_Code);
				query.OrderBy = RefCountryStatesSchema.RW_Code.Name;
				RefCountryStates state = Factory.LoadTop1<RefCountryStates>(query);
				if (state != null)
				{
					unloco.RL_RW = state.PK;
				}
			}
			return unloco;
		}

		#region NotificationStaff
		public GlbStaff NotificationStaff
		{
			get
			{
				if (fNotificationStaff == null)
				{
					fNotificationStaff = Factory.New<GlbStaff>();
					fNotificationStaff.GS_FullName = "DUMMY STAFF";
					fNotificationStaff.GS_EmailAddress = "dummy@dummy.com";
				}
				return fNotificationStaff;
			}
		}
		GlbStaff fNotificationStaff;
		#endregion

		#region NotificationGroup
		public GlbGroup NotificationGroup
		{
			get
			{
				if (fNotificationGroup == null)
				{
					fNotificationGroup = Factory.New<GlbGroup>();
					fNotificationGroup.GG_Code = "AMS";
					fNotificationGroup.Staff.Add(NotificationStaff);
				}
				return fNotificationGroup;
			}
		}
		GlbGroup fNotificationGroup;
		#endregion

		public RefCurrency CAD
		{
			get { return cAD ?? (cAD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Canada)); }
		}
		RefCurrency cAD;

		public RefCurrency USD
		{
			get
			{
				if (uSD == null)
				{
					uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.UnitedStates);
				}
				return uSD;
			}
		}
		RefCurrency uSD;

		public virtual RefCurrency AUD
		{
			get
			{
				if (aUD == null)
				{
					aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia);
				}
				return aUD;
			}
		}
		RefCurrency aUD;

		public OrgCusCode UpdateOrAddCustomsRegNo(OrgHeader org, ZString regNo, ZString codeType, ICountry country)
		{
			OrgCusCode customsCode = null;
			if (org != null)
			{
				customsCode = org.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, country);
				if (customsCode == null)
				{
					customsCode = org.CustomsCodes.AddNew();
					customsCode.OK_CodeType = codeType;
					RefCountry refCountry = Factory.Load<RefCountry>(country.PK);
					customsCode.OK_RN_NKCodeCountry = refCountry != null ? refCountry.Code : ZString.Empty;
				}
				customsCode.OK_CustomsRegNo = regNo;
			}
			return customsCode;
		}

		public IDisposable GetUtcPlus8UserContext()
		{
			var timeZoneSet = Factory.LoadTop1<RefTimeZoneSet>(new ZQuery(RefTimeZoneSetSchema.R3_TimeZoneSetName, "Asia/Singapore"));
			AssertEquals("Precondition no daylight saving to stuff this test up", null, timeZoneSet.DaylightSavingZone);
			AssertEquals("Precondition no daylight saving to stuff this test up", 0, timeZoneSet.DaylightSavingZones.ToArray().Length);
			AssertEquals("Timezone is UTC+8", (ZShort)480, timeZoneSet.StandardZone.R2_OffsetMinutesFromUTC);
			var singaporeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SIN"));
			AssertEquals("Precondition: branch is in correct time zone", "Asia/Singapore", singaporeBranch.HomePort.TimeZoneSet.R3_TimeZoneSetName);
			return Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranch.PK.ToGuid(), Env.CurrentDepartment.PK);
		}
		public static RefDataGrouping CheckDataGroupingAndCreateIfNeeded(ZString zzzDataGrouping, BusinessObjectFactory factory)
		{
			var refDataGrouping = factory.LoadTop1<RefDataGrouping>(new ZQuery(RefDataGroupingSchema.ZZZ_DataGrouping, zzzDataGrouping));
			if (refDataGrouping == null)
			{
				refDataGrouping = factory.New<RefDataGrouping>();
				refDataGrouping.ZZZ_DataGrouping = zzzDataGrouping;
				refDataGrouping.ZZZ_Description = "Auto-created test country " + zzzDataGrouping;
			}
			return refDataGrouping;
		}

		#region PostTransaction

		public Guid PostTransaction(string ledger, string transactionType, string transactionNum, decimal localAmount, decimal gSTAmount, decimal outstandingAmount, Guid branch, ZDateTime postDate, Guid bankAccount, bool isReversed, string currency, string receiptType, Guid org, ZDateTime dueDate, Guid department)
		{
			return PostTransaction(ledger, transactionType, transactionNum, localAmount, gSTAmount, outstandingAmount, branch, postDate, bankAccount, isReversed, currency, receiptType, org, dueDate, department, ZDateTime.Empty);
		}

		public Guid PostTransaction(string ledger, string transactionType, string transactionNum, decimal localAmount, decimal gSTAmount, decimal outstandingAmount, Guid branch, ZDateTime postDate, Guid bankAccount, bool isReversed, string currency, string receiptType, Guid org, ZDateTime dueDate, Guid department, ZDateTime fullyPaidDate, string transactionCategory = "")
		{
			return PostTransaction(ledger, transactionType, transactionNum, localAmount, gSTAmount, outstandingAmount, branch, postDate, bankAccount, isReversed, currency, receiptType, org, dueDate, department, fullyPaidDate, 1, transactionCategory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Introduced in WI00864979, this code should be refactored and suppression should be removed in a future update to enhance code maintainability and clarity.")]
		public Guid PostTransaction(string ledger, string transactionType, string transactionNum, decimal localAmount, decimal gSTAmount, decimal outstandingAmount, Guid branch, ZDateTime postDate, Guid bankAccount, bool isReversed, string currency, string receiptType, Guid org, ZDateTime dueDate, Guid department, ZDateTime fullyPaidDate, Byte transactionCount, string transactionCategory = "")
		{
			Guid pK = Guid.NewGuid();
			string insertPayment = @"INSERT INTO " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + " ( " +
				AccTransactionHeaderSchema.PK.Name + " , " +
				AccTransactionHeaderSchema.AH_Ledger.Name + " , " +
				AccTransactionHeaderSchema.AH_TransactionType.Name + " , " +
				AccTransactionHeaderSchema.AH_TransactionCategory.Name + " , " +
				AccTransactionHeaderSchema.AH_TransactionNum.Name + " , " +
				AccTransactionHeaderSchema.AH_InvoiceDate.Name + " , " +
				AccTransactionHeaderSchema.AH_DueDate.Name + " , " +
				AccTransactionHeaderSchema.AH_InvoiceAmount.Name + " , " +
				AccTransactionHeaderSchema.AH_GSTAmount.Name + ", " +
				AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency.Name + ", " +
				AccTransactionHeaderSchema.AH_PostDate.Name + " , " +
				AccTransactionHeaderSchema.AH_OutstandingAmount.Name + " , " +
				AccTransactionHeaderSchema.AH_AB.Name + ", " +
				AccTransactionHeaderSchema.AH_OH.Name + " , " +
				AccTransactionHeaderSchema.AH_GC.Name + " , " +
				AccTransactionHeaderSchema.AH_GB.Name + " , " +
				AccTransactionHeaderSchema.AH_ReceiptType.Name + ", " +
				AccTransactionHeaderSchema.AH_GE.Name + ", " +
				AccTransactionHeaderSchema.AH_IsCancelled.Name + ", " +
				AccTransactionHeaderSchema.AH_FullyPaidDate.Name + ", " +
				AccTransactionHeaderSchema.AH_TransactionCount.Name + ", " +
				AccTransactionHeaderSchema.AH_SystemCreateTimeUtc.Name + ", " +
				AccTransactionHeaderSchema.AH_SystemCreateUser.Name + ", " +
				AccTransactionHeaderSchema.AH_SystemLastEditTimeUtc.Name + ", " +
				AccTransactionHeaderSchema.AH_SystemLastEditUser.Name + @")
					VALUES
					(
						@PK,
						@Ledger,
						@TransactionType,
						@TransactionCategory,
						@TransactionNum,
						@InvoiceDate,
						@DueDate,
						@InvoiceAmount,
						@GSTAmount,
						@Currency,
						@PostDate,
						@OutstandingAmount,
						@BankAccount,
						@Org,
						(SELECT TOP 1 GB_GC FROM dbo.GlbBranch WHERE GB_PK = @Branch),
						@Branch,
						@ReceiptType,
						@Department,
						@IsCancelled,
						@FullyPaidDate,
						@TransactionCount,
						GetUtcDate(),
						'~BP',
						GetUtcDate(),
						'~BP')";
			DbCommand cmd = Db.Connection.Command(insertPayment, 7200);
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pK);
			cmd.AddParameter("@Ledger", SqlDbType.Char, AccTransactionHeaderSchema.AH_Ledger.MaxLength, ledger);
			cmd.AddParameter("@TransactionType", SqlDbType.Char, AccTransactionHeaderSchema.AH_TransactionType.MaxLength, transactionType);
			cmd.AddParameter("@TransactionCategory", SqlDbType.Char, AccTransactionHeaderSchema.AH_TransactionCategory.MaxLength, transactionCategory);
			cmd.AddParameter("@TransactionNum", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength, transactionNum);
			cmd.AddParameter("@InvoiceDate", SqlDbType.SmallDateTime, ZDateTime.Now.Date.ToDateTime());
			cmd.AddParameter("@DueDate", SqlDbType.SmallDateTime, dueDate.ToDateTime());
			cmd.AddParameter("@InvoiceAmount", SqlDbType.Money, localAmount);
			cmd.AddParameter("@GSTAmount", SqlDbType.Money, gSTAmount);
			cmd.AddParameter("@Currency", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency.MaxLength, currency);
			cmd.AddParameter("@PostDate", SqlDbType.SmallDateTime, postDate.ToDateTime());
			cmd.AddParameter("@OutstandingAmount", SqlDbType.Money, outstandingAmount);
			cmd.AddParameter("@BankAccount", SqlDbType.UniqueIdentifier, bankAccount != Guid.Empty ? bankAccount : DBNull.Value);
			cmd.AddParameter("@Org", SqlDbType.UniqueIdentifier, org);
			cmd.AddParameter("@Branch", SqlDbType.UniqueIdentifier, branch);
			cmd.AddParameter("@ReceiptType", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_ReceiptType.MaxLength, receiptType);
			cmd.AddParameter("@Department", SqlDbType.UniqueIdentifier, department);
			cmd.AddParameter("@IsCancelled", SqlDbType.Bit, isReversed);
			cmd.AddParameter("@FullyPaidDate", SqlDbType.SmallDateTime, fullyPaidDate != ZDateTime.Empty ? fullyPaidDate.ToDateTime() : DBNull.Value);
			cmd.AddParameter("@TransactionCount", SqlDbType.TinyInt, transactionCount);
			cmd.ExecuteNonQuery();

			return pK;
		}

		#endregion

		#region GetNewOrganisationWithCompanyData

		public OrgHeader GetNewOrganisationWithCompanyData(bool isDebtor, ZGuid glbCompanyPK, GlbBranch defaultBranch, OrgDebtorGroup debtorGroup, ZString orgARCategory, ZString aRConsolidatedAccountingCategory, ZString aRCreditRating, ZBool aROnCreditHold, ZString oH_Code)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = oH_Code;
			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			org.CompanyData.OB_OH = org.PK;
			org.CompanyData.OB_IsDebtor = isDebtor;
			org.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
			org.CompanyData.OB_GB_ControllingBranch = defaultBranch.PK;
			org.CompanyData.OB_ARCategory = orgARCategory;
			org.CompanyData.OB_ARConsolidatedAccountingCategory = aRConsolidatedAccountingCategory;
			org.CompanyData.OB_ARCreditRating = aRCreditRating;
			org.CompanyData.OB_AROnCreditHold = aROnCreditHold;
			org.CompanyData.OB_ARCreditLimit = 100m;
			org.CompanyData.OB_GC = glbCompanyPK;

			return org;
		}

		#endregion

		#region GetNewBankAccount

		public AccBankAccount GetNewBankAccount(ZString code, ZString accountNum, ZString bSB, ZGuid gLHeaderPK, ZString currencyNK, ZGuid glbCompanyPK)
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_Code = code;
			testBank.AB_AG = gLHeaderPK;
			testBank.AB_RX_NKAccountCurrency = currencyNK;
			testBank.AB_GC = glbCompanyPK;
			testBank.AB_AccountNum = accountNum;
			testBank.AB_BSB = bSB;
			return testBank;
		}

		public AccBankAccount GetNewBankAccountWithBranch(RefCurrency currency, GlbBranch branch, ZBool isDefault)
		{
			AccBankAccount acct = Factory.New<AccBankAccount>();
			acct.AB_RX_NKAccountCurrency = currency.RX_Code;
			if (branch != null)
			{
				acct.AB_GB = branch.PK;
			}
			acct.AB_IsDefaultReceiptBankAccount = isDefault;
			return acct;
		}

		#endregion

		#region GetNewGLHeader

		public AccGLHeader GetNewGLHeader(ZString gLAccountFormat, ZString accountNum, ZString description)
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.GLAccountFormat).Returns(gLAccountFormat);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AccGLHeader testGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
				testGLHeader.AG_AccountNum = accountNum;
				testGLHeader.AG_Description = description;
				return testGLHeader;
			}
		}

		#endregion

		#region GetNewCurrency

		public RefCurrency GetNewCurrency(ZString code, ZString description)
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();
			testCurrency.RX_Code = code;
			testCurrency.RX_Desc = description;
			return testCurrency;
		}

		#endregion

		#region GetNewBranch

		public GlbBranch GetNewBranch(ZGuid glbCompanyPK)
		{
			GlbBranch defaultBranch = Factory.NewWithValidTestData<GlbBranch>();
			defaultBranch.GB_GC = glbCompanyPK;
			return defaultBranch;
		}

		#endregion

		#region GetNewOrgDebtorGroup

		public OrgDebtorGroup GetNewOrgDebtorGroup()
		{
			OrgDebtorGroup debtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			return debtorGroup;
		}

		#endregion

		#region GetNewContact

		public OrgContact GetNewContact(OrgHeader org, ZString contactName, ZString phone)
		{
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Phone = phone;
			contact.OC_OH = org.PK;
			return contact;
		}

		#endregion

		#region GetNewOrgDocument

		public OrgDocument GetNewOrgDocument(ZGuid contactPK, ZBool isDefaultContact, ZString documentGroup)
		{
			OrgDocument document = Factory.NewWithValidTestData<OrgDocument>();
			document.OD_OC = contactPK;
			document.OD_DefaultContact = isDefaultContact;
			document.OD_DocumentGroup = documentGroup;

			return document;
		}

		#endregion

		#region AddCollectionNote

		public void AddCollectionNote(OrgHeader org, ZGuid orgContactPK, ZDateTime createTime, ZDateTime callBackDateTime, ZString callDisposition, ZString callDetailNote)
		{
			OrgCollectionNote note = GetNewCollectionNote(org, orgContactPK, createTime, callBackDateTime, callDisposition, callDetailNote);
		}

		#endregion

		#region AddARTerms

		public void AddCompanyARTerms(OrgHeader org)
		{
			org.CompanyData.ARTerms.DeleteAll();

			var standardTerms = org.CompanyData.ARTerms.AddNew();
			standardTerms.PY_InvoiceClass = "ALL";
			standardTerms.PY_InvoiceTerm = "INV";
			standardTerms.PY_InvoiceDays = 10;

			var disbursementTerms = org.CompanyData.ARTerms.AddNew();
			disbursementTerms.PY_InvoiceClass = "DSB";
			disbursementTerms.PY_InvoiceTerm = "INV";
			disbursementTerms.PY_InvoiceDays = 10;

			Factory.Save();
		}

		#endregion

		#region GetNewCollectionNote

		public OrgCollectionNote GetNewCollectionNote(OrgHeader org, ZGuid orgContactPK, ZDateTime createTime, ZDateTime callBackDateTime, ZString callDisposition, ZString callDetailNote)
		{
			OrgCollectionNote collectionNote = org.CollectionNotes.AddNew();
			collectionNote.PN_SystemCreateTimeUtc = createTime;
			collectionNote.PN_CallBackDate = callBackDateTime;
			collectionNote.PN_CallDisposition = callDisposition;
			collectionNote.PN_CallDetailNote = callDetailNote;
			collectionNote.PN_OC = orgContactPK;
			collectionNote.PN_OB = org.CompanyData.PK;

			return collectionNote;
		}

		#endregion

		#region GetNewGlbCompany

		public GlbCompany GetNewGlbCompany(ZString name)
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = name;
			Factory.Save();
			return company;
		}

		#endregion

		#region GetNewGlbDepartment

		public GlbDepartment GetNewGlbDepartment()
		{
			GlbDepartment department = Factory.NewWithValidTestData<GlbDepartment>();
			return department;
		}
		#endregion

		#region OrgProxyEnv

		public static OrgProxyEnv SetupOrgProxy(BusinessObjectFactory factory, Action<OrgHeader, GlbBranch> orgSetup = null)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Email = "turtle@fish.com";

			var branch = factory.NewWithValidTestData<GlbBranch>();
			var company = factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry(Constants.CountryCodes.UnitedStates);
			company.GC_OH_OrgProxy = branch.GB_OH_OrgProxy = org.PK;
			branch.GB_GC = company.PK;
			orgSetup?.Invoke(org, branch);
			factory.Save();

			return new OrgProxyEnv(org, branch);
		}

		public sealed class OrgProxyEnv : IDisposable
		{
			public OrgProxyEnv(OrgHeader org, GlbBranch branch)
			{
				Org = org;
				Branch = branch;
				disposable = Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK);
			}

			public OrgHeader Org { get; }
			public GlbBranch Branch { get; }
			readonly IDisposable disposable;

			public void Dispose()
			{
				disposable.Dispose();
			}
		}

		public static void SetupOrgCommunicationModes(OrgHeader org, string organisationName)
		{
			var workflowDescriptorCode = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			// Email Notification
			var emailNotificationMode = org.EDICommunicationsModes.Cast<EDICommunicationsMode>().FirstOrDefault(x => x.EK_Module == workflowDescriptorCode && x.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationEmail
				&& x.EK_Destination == organisationName + "@notificationemail.cargowise.com");
			if (emailNotificationMode == null)
			{
				emailNotificationMode = org.EDICommunicationsModes.AddNew();
				emailNotificationMode.EK_Module = workflowDescriptorCode;
				emailNotificationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
				emailNotificationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				emailNotificationMode.EK_Destination = organisationName + "@notificationemail.cargowise.com";
			}

			// Some Communication mode not applicable in this case
			var anotherMode = org.EDICommunicationsModes.Cast<EDICommunicationsMode>().FirstOrDefault(x => x.EK_FileFormat == "@@@");
			if (anotherMode == null)
			{
				anotherMode = org.EDICommunicationsModes.AddNew();
				anotherMode.EK_FileFormat = "@@@";
			}
		}

		public static EDICommunicationsMode AddCommunicationMode(OrgHeader org, string format, string module = null, string transport = null)
		{
			EDICommunicationsMode xmlMode;
			{
				xmlMode = org.EDICommunicationsModes.AddNew();
				xmlMode.EK_Module = module ?? WorkflowDescriptors.DummyWorkflowDescriptorCode;
				xmlMode.EK_FileFormat = format;
				xmlMode.EK_CommunicationsTransport = transport ?? EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				xmlMode.EK_Destination = "test@test.com";
				xmlMode.EK_Filename = "HerpDerp";
			}

			return xmlMode;
		}

		#endregion

		#region GetNonPublicValue

		public static TReturn GetNonPublicPropertyValue<TReturn>(string propertyName, object objectToQuery)
		{
			PropertyInfo property = null;
			var type = objectToQuery.GetType();

			while (property == null && type != null)
			{
				property = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				if (property == null)
				{
					type = type.BaseType;
				}
			}

			if (property == null)
			{
				throw new ArgumentException("Field '" + propertyName + "' not found in object");
			}

			return (TReturn)property.GetValue(objectToQuery, null);
		}

		public static TReturn GetNonPublicValue<TReturn>(string fieldName, object objectToQuery)
		{
			FieldInfo field = null;
			var type = objectToQuery.GetType();

			while (field == null && type != null)
			{
				field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				if (field == null)
				{
					type = type.BaseType;
				}
			}

			if (field == null)
			{
				throw new ArgumentException("Field '" + fieldName + "' not found in object");
			}

			return (TReturn)field.GetValue(objectToQuery);
		}
		#endregion

		#region Task Types

		public static void AddTaskTypesToRegistry(string workflowType, params string[] taskTypes)
		{
			var dictionary = new Dictionary<string, string>(taskTypes.Length);

			foreach (var type in taskTypes)
			{
				dictionary.Add(type, null);
			}

			AddTaskTypesToRegistry(workflowType, dictionary);
		}

		public static void AddTaskTypesToRegistry(string workflowType, Dictionary<string, string> codesAndDescriptions)
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypeCategory = categorisedTaskTypes.Cast<CategorisedWorkflowTaskTypes>().SingleOrDefault(c => c.Code == workflowType) ?? categorisedTaskTypes.AddNew();
			taskTypeCategory.Code = workflowType;

			foreach (var pair in codesAndDescriptions)
			{
				var taskType = taskTypeCategory.TaskTypes.AddNew();
				taskType.Code = pair.Key;

				if (!string.IsNullOrEmpty(pair.Value))
				{
					taskType.Description = (NoResString)pair.Value;
				}
			}

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		public static void SetTaskTypeActiveStatus(string workflowType, string taskType, bool isActive)
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypeCategory = categorisedTaskTypes.Cast<CategorisedWorkflowTaskTypes>().SingleOrDefault(c => c.Code == workflowType);

			if (taskTypeCategory != null)
			{
				var workflowTaskType = taskTypeCategory.TaskTypes.Cast<WorkflowTaskType>().SingleOrDefault(t => t.Code == taskType);

				if (workflowTaskType != null)
				{
					workflowTaskType.IsActive = isActive;

					WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
				}
			}
		}

		public static void MakeCompletionStatementTaskType(string workflowType, string completionStatementTaskType)
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypeCategory = categorisedTaskTypes.Cast<CategorisedWorkflowTaskTypes>().SingleOrDefault(c => c.Code == workflowType) ?? categorisedTaskTypes.AddNew();
			taskTypeCategory.Code = workflowType;
			var taskType = taskTypeCategory.TaskTypes.AddNew();
			taskType.Code = completionStatementTaskType;
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		public static void SetUpAssistWithThisTaskInRegistry(string workflowType, string taskType, int lowEstimateMinutes, int variationFactor)
		{
			var registryItem = WorkflowDataRegistry.Instance.AssistWithThisTask.Value;

			var orgSettings = registryItem.GetTaskDetailsForWorkflowType(workflowType);
			orgSettings.TaskType = taskType;
			orgSettings.LowEstimateMinutes = lowEstimateMinutes;
			orgSettings.VariationFactor = variationFactor;

			WorkflowDataRegistry.Instance.AssistWithThisTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem);
		}

		#endregion

		#region Quality Containment Barriers

		public static void AddCompletionStatementTaskType(string type, string workflowType, string description = null)
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypes = categorisedTaskTypes.GetTaskTypesFromWorkflowCode(workflowType);
			var taskType = taskTypes.Cast<WorkflowTaskType>().FirstOrDefault(t => t.Code == type) ?? taskTypes.AddNew();
			taskType.Code = type;

			if (description != null)
			{
				taskType.Description = (NoResString)description;
			}

			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		public static void SetAsQualityContainmentBarrierTaskType(string type, string workflowType)
		{
			var collection = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypes = collection.Cast<CategorisedWorkflowTaskTypes>().FirstOrDefault(t => t.Code == workflowType) ?? collection.AddNew();
			var taskType = taskTypes.TaskTypes.Cast<WorkflowTaskType>().FirstOrDefault(t => t.Code == type) ?? taskTypes.TaskTypes.AddNew();

			taskTypes.Code = workflowType;
			taskType.Code = type;
			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			taskType.CanCloseTaskNotAssignedToSelf = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
		}

		public static void AddIterationReasonToRegistry(string workflowType, string code, string description, bool setSystemLevelValue = false)
		{
			var iterationReasonsCollection = WorkflowDataRegistry.Instance.IterationReasons.Value;

			var iterationReason = iterationReasonsCollection.GetIterationReason(workflowType, code);
			if (iterationReason != null)
			{
				return;
			}

			var iterationReasons = iterationReasonsCollection.GetIterationReasonsFromWorkflowCode(workflowType);
			if (iterationReasons.Count == 1 && iterationReasons[0].Code == "UDF")
			{
				iterationReasons.RemoveAll();
			}
			iterationReason = iterationReasons.AddNew();
			iterationReason.Code = code;
			iterationReason.Description = (NoResString)description;

			WorkflowDataRegistry.Instance.IterationReasons.SetValue(setSystemLevelValue ? Guid.Empty : Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, iterationReasonsCollection);
		}

		public static void ClearIterationReasonsFromRegistry(string workflowType, bool setSystemLevelValue = false)
		{
			var iterationReasonsCollection = WorkflowDataRegistry.Instance.IterationReasons.Value;
			if (iterationReasonsCollection.Any())
			{
				var iterationReasons = iterationReasonsCollection.GetIterationReasonsFromWorkflowCode(workflowType);
				iterationReasons.RemoveAll();

				WorkflowDataRegistry.Instance.IterationReasons.SetValue(setSystemLevelValue ? Guid.Empty : Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, iterationReasonsCollection);
			}
		}

		public static void AssertRtfText(string message, string expectedText, ZBlob rawRtf)
		{
			AssertMultilineASCIIEquals(message, expectedText, ORtfTextUtil.RtfToText(rawRtf));
		}

		#endregion

		#region Workflow

		public static bool EnableFactorySaveAlerterInTesting
		{
			get => enableFactorySaveAlerterInTesting.Value;
			set => enableFactorySaveAlerterInTesting.Value = value;
		}
		readonly static Overridable<bool> enableFactorySaveAlerterInTesting = new Overridable<bool>(false);

		public static string RunLogWalker()
		{
			return ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
		}

		public static string RunFieldChangeTriggerProcessorServiceTask()
		{
			return ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunFieldChangeTriggerProcessor();
		}

		public static string GetFldNotificationsFromLogWalker(string logWalkerOutput)
		{
			var fldNotificationsList = new List<string>();
			var inFLD = false;

			foreach (var line in logWalkerOutput.SplitByLine())
			{
				if (line.StartsWith("[WorkflowEventTrigger] [Default] Firing action: FLD"))
				{
					inFLD = true;
				}
				else if (line.StartsWith("[WorkflowEventTrigger] [Default] Action completed: FLD"))
				{
					inFLD = false;
				}
				else if (inFLD)
				{
					fldNotificationsList.Add(fldNotificationTagRemover.Match(line).Groups["notification"].Value);
				}
			}

			return string.Join("\r\n", fldNotificationsList);
		}
		static readonly Regex fldNotificationTagRemover = new Regex(@"(.*\[WorkflowEventTrigger\] \[Default\] )?(?<notification>.*)", RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public static ProcessTask CreateMilestone(IWorkflowProvider job, Event @event, string triggerCondition = null, string triggerConditionValue = null)
		{
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = @event.Code;
			milestone.TriggerConditions.TriggerCondition = triggerCondition;
			milestone.TriggerConditions.TriggerConditionValue = triggerConditionValue;
			milestone.P9_Description = @event.Description;

			return milestone;
		}

		public static ProcessTask CreateException(IWorkflowProvider job, ProcessTask milestone)
		{
			var exception = job.WorkflowItems.Exceptions.AddNew();
			exception.P9_SE_NKExceptionEvent = milestone.P9_SE_NKMilestoneEvent;

			return exception;
		}

		public static ProcessTask CreateTrigger(IWorkflowProvider job, Event @event, string triggerFieldName = null, string triggerCondition = null, string triggerConditionValue = null)
		{
			var trigger = CreateTrigger(job, @event.Code, triggerFieldName, triggerCondition, triggerConditionValue);
			trigger.P9_Description = @event.Description;

			return trigger;
		}

		public static ProcessTask CreateTrigger(IWorkflowProvider job, string eventCode, string triggerFieldName = null, string triggerCondition = null, string triggerConditionValue = null, string lineTriggerType = null, string description = null)
		{
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = eventCode;
			trigger.TriggerConditions.TriggerFieldName = triggerFieldName;
			trigger.TriggerConditions.TriggerCondition = triggerCondition;
			trigger.TriggerConditions.TriggerConditionValue = triggerConditionValue;

			trigger.P9_LineTriggerType = lineTriggerType;
			trigger.P9_Description = description;

			return trigger;
		}

		public static ProcessTaskNotification CreateTriggerAction(IBaseTrigger trigger, string triggerActionType, string recipientType = null, string emailText = null, string emailAddress = null, StmMenuItem documentMenuItem = null, string actionReference = null)
		{
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = triggerActionType;
			triggerAction.PQ_Calc_TriggerParty = recipientType;

			if (emailText != null)
			{
				if (!triggerActionType.In(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage))
				{
					triggerAction.OverrideEmail = true;
				}

				triggerAction.PQ_EmailTextFallbackToTemplate = emailText;
			}

			if (emailAddress != null)
			{
				triggerAction.PQ_EmailAddr = emailAddress;
			}

			if (documentMenuItem != null)
			{
				triggerAction.PQ_SU_Document = documentMenuItem.PK;
			}

			if (actionReference != null)
			{
				triggerAction.PQ_ActionReference = actionReference;
			}

			return triggerAction;
		}

		public static ProcessTaskNotification CreateEmailNotificationTriggerAction(IBaseTrigger trigger, string recipientType, string emailText = null, string emailAddress = null)
		{
			return CreateTriggerAction(trigger, EDICommunicationsModeFileFormatList.Codes.NotificationEmail, recipientType, emailText, emailAddress);
		}

		public static TWorkflowProvider CreateWorkflowProvider<TWorkflowProvider>(BusinessObjectFactory factory)
			where TWorkflowProvider : BusinessObject, IWorkflowProvider
		{
			return factory.NewWithValidTestData<TWorkflowProvider>();
		}

		public static ProcessTask CreateTask(IWorkflowProvider job, string assignedStaff = null, GlbCompany company = null, string status = null, string taskType = "UDF", GlbCapability requiredCapability = null, GlbGroup assignedGroup = null, int? sequence = null, string description = null)
		{
			var task = job.WorkflowItems.Tasks.AddNew();

			task.P9_Type = taskType;

			if (assignedStaff != null)
			{
				task.P9_GS_NKAssignedStaffMember = assignedStaff;
			}

			if (requiredCapability != null)
			{
				task.P9_G4_RequiredCapability = requiredCapability.PK;
			}

			if (assignedGroup != null)
			{
				task.P9_GG_AssignedGroup = assignedGroup.PK;
			}

			if (company != null)
			{
				task.P9_GC = company.PK;
			}

			if (status != null)
			{
				task.P9_Status = status;
			}

			if (sequence != null)
			{
				task.P9_Sequence = sequence.Value;
			}

			if (description != null)
			{
				task.P9_Description = description;
			}

			return task;
		}

		public static IProcessTaskIterationLink CreateIterationLink(ProcessTask containmentBarrierTask, ProcessTask iterateToTask)
		{
			var link = containmentBarrierTask.Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = containmentBarrierTask.PK;
			link.P9I_P9_IterationTask = iterateToTask.PK;

			return link;
		}

		public static void SetAllTasksStatus(IWorkflowProvider job, string newStatus)
		{
			job.WorkflowItems.Tasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = newStatus);
		}

		public static void ClearWorkflowTables()
		{
			TestCaseHelper.ClearTable(ProcessTaskNotificationSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTemplateTriggerSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTasksSchema.Constants.TableName);
			TestCaseHelper.ClearTable(ProcessTaskTemplateSchema.Constants.TableName);
		}

		public static GenCustomColumnDefinition CreateCustomField(ProcessTaskTemplate template, string name, string type = AddOnColumnDataType.Codes.String)
		{
			var customFieldDefinition = template.GenCustomColumnDefinitions.AddNew();
			customFieldDefinition.XC_Name = name;
			customFieldDefinition.XC_Type = type;

			return customFieldDefinition;
		}

		public static GenCustomAddOnRule CreateAddOnRule(BusinessObjectFactory factory)
		{
			var rule = factory.New<GenCustomAddOnRule>();
			return rule;
		}

		#endregion

		#region Universal Triggers

		public static ProcessTaskTemplate CreateUniversalTemplate(BusinessObjectFactory factory, string processType,
			string subType1 = null, string subType2 = null, string subType3 = null, string subType4 = null, string subType5 = null,
			string loadPort = null, string dischargePort = null, ZGuid? department = null, bool isGlobal = true,
			string triggerFallbackMethod = FallbackTypeList.Codes.NeverFallback, bool isSystemDefined = false, string name = null)
		{
			return CreateWorkflowTemplate(factory, processType, subType1, subType2, subType3, subType4, subType5, loadPort, dischargePort, department, isGlobal, triggerFallbackMethod, isSystemDefined: isSystemDefined, isUniversal: true, name: name);
		}

		public static ProcessTaskTemplate CreateWorkflowTemplate(BusinessObjectFactory factory, string processType,
			string subType1 = null, string subType2 = null, string subType3 = null, string subType4 = null, string subType5 = null,
			string loadPort = null, string dischargePort = null, ZGuid? department = null, bool isGlobal = true,
			string triggerFallbackMethod = FallbackTypeList.Codes.NeverFallback, bool isUniversal = false, bool isSystemDefined = false, string name = null, string description = null)
		{
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = processType;
			template.P0_TriggerFallbackMethod = triggerFallbackMethod;

			template.P0_SubType1 = subType1;
			template.P0_SubType2 = subType2;
			template.P0_SubType3 = subType3;
			template.P0_SubType4 = subType4;
			template.P0_SubType5 = subType5;

			template.P0_LoadPortCountry = loadPort;
			template.P0_DischargePortCountry = dischargePort;

			if (department != null)
			{
				template.P0_GE = department.Value;
			}

			template.GlobalTemplate = isGlobal;
			template.P0_IsUniversal = isUniversal;
			template.P0_IsSystem = isSystemDefined;

			if (string.IsNullOrEmpty(name))
			{
				template.P0_Name = string.Join(", ",
					new object[] { processType, subType1, subType2, subType3, subType4, subType5, loadPort, dischargePort, template.Department?.GE_Code, template.Company?.GC_Code, isUniversal }
					.Select(o => o?.ToString())
					.Where(s => !string.IsNullOrEmpty(s)));
			}
			else
			{
				template.P0_Name = name;
			}

			template.P0_Description = description;

			return template;
		}

		public static ITemplateTrigger CreateTemplateTrigger(ProcessTaskTemplate template, string eventCode, string triggerCondition = null, string triggerConditionValue = null)
		{
			var trigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			((BusinessObject)trigger).FillWithValidTestData();

			trigger.TriggerEventCode = eventCode;
			trigger.TriggerCondition = triggerCondition;
			trigger.TriggerConditionValue = triggerConditionValue;

			return trigger;
		}

		public static void AssertUniversalTriggerFired(ITemplateTrigger trigger, IWorkflowProvider job, bool expectingTriggerToHaveFired, ZDateTime? expectedFiringTime = null)
		{
			var jobTriggerLink = trigger.GetOrCreateJobVersionOfTrigger((IBusiness)job, createIfNotFound: false);

			if (expectingTriggerToHaveFired)
			{
				AssertNotNull("Expecting this trigger to have fired for the job, but there wasn't a job-trigger link created to store the event time", jobTriggerLink);
				AssertEquals("Expecting a valid LastFiredTime to be recorded against this trigger link", true, jobTriggerLink.LastFiredTime.IsValid);

				if (expectedFiringTime != null)
				{
					AssertEquals("LastFiredTime", expectedFiringTime, jobTriggerLink.LastFiredTime.ToDateTime());
				}
			}
			else if (jobTriggerLink != null)
			{
				AssertEquals("Expecting this trigger NOT to have fired for the job, but a job-trigger link has been created with a valid event time, indicating it has fired previously", ZDateTimeOffset.Empty, jobTriggerLink.LastFiredTime);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Staff

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, string emailAddress)
		{
			return CreateStaff(factory, "", "", emailAddress);
		}

		public static SalesTeam CreateSalesTeam(BusinessObjectFactory factory, string code, string description)
		{
			var team = factory.NewWithValidTestData<SalesTeam>();
			team.GG_Code = code;
			team.GG_Desc = description;

			return team;
		}

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, string code, string fullName, string emailAddress = "")
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			if (!string.IsNullOrEmpty(code))
			{
				staff.GS_Code = code;
			}
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = emailAddress;

			return staff;
		}

		public static GlbGroup CreateGroup(BusinessObjectFactory factory, string code, string description = null)
		{
			var group = factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = code;
			group.GG_Desc = description;

			return group;
		}

		public static GlbGroup CreateGroup(BusinessObjectFactory factory, params GlbStaff[] groupMembers)
		{
			var group = factory.NewWithValidTestData<GlbGroup>();

			group.Staff.AddRange(groupMembers);

			return group;
		}

		public static GlbGroup CreateGroup(BusinessObjectFactory factory, string code, string description = "", string category = "", params GlbStaff[] groupMembers)
		{
			var group = factory.NewWithValidTestData<GlbGroup>();
			if (!string.IsNullOrEmpty(code))
			{
				group.GG_Code = code;
			}
			group.GG_Desc = description;
			group.GG_Category = category;

			group.Staff.AddRange(groupMembers);

			return group;
		}

		public static GlbCapability CreateCapability(BusinessObjectFactory factory, string code, string description, params GlbStaff[] staffWithCapability)
		{
			var capability = factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = code;
			capability.G4_Description = description;

			capability.ResourcesWithCapability.AddRange(staffWithCapability);

			return capability;
		}

		public static GlbCapability CreateCapability(BusinessObjectFactory factory, params GlbStaff[] staffWithCapability)
		{
			return CreateCapability(factory, null, staffWithCapability);
		}

		public static GlbCapability CreateCapability(BusinessObjectFactory factory, string scope, params GlbStaff[] staffWithCapability)
		{
			var capability = factory.NewWithValidTestData<GlbCapability>();

			if (scope != null)
			{
				capability.G4_CapacityScope = scope;
			}

			capability.ResourcesWithCapability.AddRange(staffWithCapability);

			return capability;
		}

		public static void MakeStaffOwnerOfGroup(GlbGroup group, GlbStaff staff)
		{
			if (group.GG_Code.IsEmpty)
			{
				throw new ArgumentException("Group.GG_Code needs to be specified.");
			}

			staff.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner; // Oh my...
			staff.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);
		}

		#endregion

		#region RefAccTaxRate
		public static RefAccTaxRate CreateRefTaxRate(BusinessObjectFactory factory, ZString countryCode, ZString referenceType, ZInt numerator, ZInt denominator, ZDate startDtae, ZDate endDate, RefAccTaxRate refAccTaxRate = null)
		{
			refAccTaxRate = refAccTaxRate ?? factory.New<RefAccTaxRate>();
			refAccTaxRate.ZAT_RN_NKCountry = countryCode;
			refAccTaxRate.ZAT_ReferenceRateType = referenceType;
			refAccTaxRate.ZAT_RateNumerator = numerator;
			refAccTaxRate.ZAT_RateDenominator = denominator;
			refAccTaxRate.ZAT_StartDate = startDtae;
			refAccTaxRate.ZAT_EndDate = endDate;
			return refAccTaxRate;
		}

		#endregion

		#region AssertEventRaised

		public static StmALog AssertEventRaised(IStmALogParent bizo, string eventCode, string reference = null, bool deferFiringWorkflow = false)
		{
			return AssertEventRaised(string.Empty, bizo, eventCode, reference, deferFiringWorkflow);
		}

		public static StmALog AssertEventRaised(string assertionMessage, IStmALogParent bizo, string eventCode, string reference = null, bool deferFiringWorkflow = false)
		{
			var log = GetLatestLog(bizo, eventCode, reference);
			var message = new StringBuilder(assertionMessage);

			if (!string.IsNullOrEmpty(assertionMessage))
			{
				message.AppendLine();
			}

			var eventText = new Lazy<string>(() => string.Join("\r\n", bizo.Logs.GetAllLogs().Cast<StmALog>().Select(l => string.Format("• Code: {0}, Reference: [{1}], Display Text: [{2}]", l.SL_SE_NKEvent, l.SL_Reference, l.DisplayEventReference))));

			if (log == null)
			{
				message.AppendFormat("An event with code {0} and reference [{1}] should have been raised. The following events were raised:\r\n\t{2}", eventCode, reference, eventText.Value);

				Fail(message.ToString());
			}
			else if (reference != null)
			{
				message.AppendFormat("An event with code {0} was raised, but the reference was incorrect. Other events raised:\r\n\t{1}", eventCode, eventText.Value);

				AssertEquals(message.ToString(), reference, log.SL_Reference);
			}
			else
			{
				Assert(true);
			}

			if (deferFiringWorkflow)
			{
				AssertEquals("The event matched, but SL_FireWorkflow was incorrect. Firing Workflow for this log should be deferred to the Log Walker service task", true, log.SL_FireWorkflow);
			}
			else
			{
				AssertEquals("The event matched, but SL_FireWorkflow was incorrect. Firing Workflow for this log should happen immediately (SL_FireWorkflow really means \"don't fire workflow... yet.\")", false, log.SL_FireWorkflow);
			}

			return log;
		}

		public static void AssertNoEventRaised(IStmALogParent bizo, string eventCode)
		{
			AssertNoEventRaised(string.Empty, bizo, eventCode);
		}

		public static void AssertNoEventRaised(string assertionMessage, IStmALogParent bizo, string eventCode)
		{
			var log = GetLatestLog(bizo, eventCode, null);

			if (log != null)
			{
				var message = new StringBuilder(assertionMessage);

				if (!string.IsNullOrEmpty(assertionMessage))
				{
					message.AppendLine();
				}

				message.AppendFormat("An event with code {0} should NOT have been raised but it was.", eventCode);

				Fail(message.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		public static StmALog GetLatestLog(IStmALogParent bizo, string eventCode, string reference)
		{
			var logs = bizo.Logs.Find(l => l.SL_SE_NKEvent == eventCode).OrderByDescending(e => e.SL_EventTime).ThenByDescending(e => e.InstantiationTime).ToArray();

			if (logs.Length > 1 && reference != null)
			{
				return logs.FirstOrDefault(l => l.SL_Reference == reference) ?? logs.FirstOrDefault();
			}
			else
			{
				return logs.FirstOrDefault();
			}
		}

		#endregion

		#region Security

		public static ModuleTree GetFullyLoadedModuleTree()
		{
			var tree = new ModuleTree();
			var loader = ObjectFactory.Get<IModuleTreeLoader>();
			loader.Initialise(tree, Env.Security);
			loader.LoadModules();

			return tree;
		}

		#endregion

		#region Implementation

		protected override BusinessObjectFactory NewFactory() => fFactory ?? base.NewFactory();

		readonly BusinessObjectFactory fFactory;

		#endregion
	}
}
#endif
