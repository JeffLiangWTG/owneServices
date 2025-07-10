using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class BaseFreightTest : TestCaseWithFactory
	{
		#region Notes

		protected bool PredefinedNoteTypeExists(PredefinedNoteType noteTypeToCheck, NoteTypeCollection noteTypes)
		{
			foreach (PredefinedNoteType noteType in noteTypes)
			{
				if (noteTypeToCheck == noteType)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Voyages

		const string TestVoyage1VoyageNumber = "24";
		protected JobVoyage TestVoyage1
		{
			get
			{
				if (fVoyage == null)
				{
					fVoyage = CreateVoyage();
				}
				return fVoyage;
			}
		}
		JobVoyage fVoyage;

		#endregion

		#region Sailings

		protected JobSailing ExportSailing1
		{
			get
			{
				if (fExportSailing1 == null)
				{
					fExportSailing1 = CreateNewSailing(HomePort, OverseasPort, ZDateTime.Today, ZDateTime.Today.AddMonths(1));
				}
				return fExportSailing1;
			}
		}
		JobSailing fExportSailing1;

		protected JobSailing ImportSailing1
		{
			get
			{
				if (fImportSailing1 == null)
				{
					fImportSailing1 = CreateNewSailing(OverseasPort, HomePort, ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1));
				}
				return fImportSailing1;
			}
		}
		JobSailing fImportSailing1;

		protected JobSailing OffshoreSailing1
		{
			get
			{
				if (fOffshoreSailing1 == null)
				{
					fOffshoreSailing1 = CreateNewSailing(OverseasPort, OverseasPort3, ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1));
				}
				return fOffshoreSailing1;
			}
		}
		JobSailing fOffshoreSailing1;

		#endregion

		#region Test MasterFiles

		#region ContainerTypes

		#region RC_20GP_PK

		ZGuid fRC_20GP_PK;
		protected ZGuid RC_20GP_PK
		{
			get
			{
				if (!fRC_20GP_PK.IsValid)
				{
					fRC_20GP_PK = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP")).PK;
				}
				return fRC_20GP_PK;
			}
		}

		#endregion

		#region RC_40GP_PK

		ZGuid fRC_40GP_PK;
		protected ZGuid RC_40GP_PK
		{
			get
			{
				if (!fRC_40GP_PK.IsValid)
				{
					fRC_40GP_PK = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP")).PK;
				}
				return fRC_40GP_PK;
			}
		}

		#endregion

		#region RC_20RE_PK

		ZGuid fRC_20RE_PK;
		protected ZGuid RC_20RE_PK
		{
			get
			{
				if (!fRC_20RE_PK.IsValid)
				{
					fRC_20RE_PK = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE")).PK;
				}
				return fRC_20RE_PK;
			}
		}

		#endregion

		#region RC_40RE_PK

		ZGuid fRC_40RE_PK;
		protected ZGuid RC_40RE_PK
		{
			get
			{
				if (!fRC_40RE_PK.IsValid)
				{
					fRC_40RE_PK = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE")).PK;
				}
				return fRC_40RE_PK;
			}
		}

		#endregion

		#endregion

		#region Locations

		protected static ZString HomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}
				return "AUSYD";
			}
		}

		protected static ZString USPort
		{
			get { return "USLAX"; }
		}

		protected static ZString USPortAlt
		{
			get { return "USSFO"; }
		}

		protected static ZString AlternateHomePort
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					ZString uNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					ZString countryCode = uNLOCO.Substring(0, 2);
					return GetPortInCountryExcluding(countryCode, uNLOCO);
				}
				return "AUPER";
			}
		}

		protected static ZString AlternateHomePort2
		{
			get
			{
				if (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty)
				{
					ZString uNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					ZString countryCode = uNLOCO.Substring(0, 2);
					return GetPortInCountryExcluding(countryCode, uNLOCO, AlternateHomePort);
				}
				return "AUPER";
			}
		}

		protected static ZString OverseasPort
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN")
				{
					return "SGSIN";
				}
				else
				{
					return "USLAX";
				}
			}
		}

		protected static ZString OverseasPort2
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "HKHKG")
				{
					return "HKHKG";
				}
				else
				{
					return "THBKK";
				}
			}
		}

		protected static ZString OverseasPort3
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "DEHAM")
				{
					return "DEHAM";
				}
				else
				{
					return "CNSHA";
				}
			}
		}

		protected static ZString OverseasPort4
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "TTTEM")
				{
					return "TTTEM";
				}
				else
				{
					return "JPTYO";
				}
			}
		}

		protected static ZString GetPortInCountryExcluding(string countryCode, params string[] uNLOCOs)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, countryCode);
			foreach (string uNLOCO in uNLOCOs)
			{
				filter.AddToFilter(JoinCondition.And, RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, uNLOCO);
			}
			var resultPort = factory.LoadTop1<RefUNLOCO>(filter);
			return resultPort.RL_Code;
		}

		#endregion

		#region Vessels

		#region TestVessel1

		const string TestVessel1Name = "APL EMERALD";
		protected RefVessel TestVessel1
		{
			get
			{
				if (fTestVessel1 == null)
				{
					fTestVessel1 = LoadOrCreateVessel(TestVessel1Name, "8610033");
				}
				return fTestVessel1;
			}
		}
		RefVessel fTestVessel1;

		#endregion

		#region TestVessel2

		const string TestVessel2Name = "The Black Pearl";
		protected RefVessel TestVessel2
		{
			get
			{
				if (fTestVessel2 == null)
				{
					fTestVessel2 = LoadOrCreateVessel(TestVessel2Name, "8610033");
				}
				return fTestVessel2;
			}
		}
		RefVessel fTestVessel2;

		#endregion

		RefVessel LoadOrCreateVessel(string name, string lloydsNumber)
		{
			var vessel = RefVessel.LookupVesselByName(name, Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_LloydsNumber = lloydsNumber;
				vessel.RV_Name = name;
			}

			return vessel;
		}

		#endregion

		#region RefEquipment

		#region Truck1

		RefEquipment fTruck1;
		protected RefEquipment Truck1
		{
			get
			{
				if (fTruck1 == null)
				{
					RefContainer container = Factory.New<RefContainer>();
					container.RC_Code = "HELO";
					container.RC_ShippingMode = "ROA";

					fTruck1 = Factory.New<RefEquipment>();
					fTruck1.RQ_RC_RoadContainerType = container.PK;
					fTruck1.RQ_CapacityInTEU = 2;
					fTruck1.RQ_CubicCapacity = 30;
					fTruck1.RQ_ShortCode = "ABC 543";
					fTruck1.RQ_IsVehicle = true;
					fTruck1.RQ_GS_NKPreferredDriver = Driver1.GS_Code;
				}
				return fTruck1;
			}
		}

		#endregion

		#endregion

		#region GlbStaff

		GlbStaff fDriver1;
		protected GlbStaff Driver1
		{
			get
			{
				if (fDriver1 == null)
				{
					fDriver1 = Factory.New<GlbStaff>();
					fDriver1.GS_FullName = "Test Person";
					fDriver1.GS_Code = "TPN";
				}
				return fDriver1;
			}
		}

		#endregion

		#endregion

		#region Test Organisations

		protected static int GetNextOrganisationNumber()
		{
			return fNextOrganisationNumber++;
		}

		static int fNextOrganisationNumber;

		#region Consignees

		public OrgHeader[] LoadLocalConsignees(int topCount, BusinessObjectFactory factory)
		{
			ZQuery localConsigneeFilter = new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, HomePort);
			localConsigneeFilter.AddToFilter(OrgHeaderSchema.OH_IsConsignee, ZBool.True);
			localConsigneeFilter.MaximumRows = topCount;

			OrgHeader[] result = (OrgHeader[])factory.Load(typeof(OrgHeader), localConsigneeFilter);
			if (result.Length > 0)
			{
				return result;
			}
			else
			{
				return new OrgHeader[] { CreateTestLocalConsignee() };
			}
		}

		OrgHeader fLocalConsignee;
		public OrgHeader LocalConsignee
		{
			get
			{
				if (fLocalConsignee == null)
				{
					int orgNumber = GetNextOrganisationNumber();
					fLocalConsignee = Factory.New<OrgHeader>();
					fLocalConsignee.OH_Code = "LCLCNE" + orgNumber;
					fLocalConsignee.OH_IsConsignee = true;
					fLocalConsignee.OH_FullName = "Local Consignee " + orgNumber;
					fLocalConsignee.MainAddress.OA_Address1 = "Test Address Line";
					fLocalConsignee.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalConsignee;
			}
		}

		public OrgHeader CreateTestLocalConsignee()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			int orgNumber = GetNextOrganisationNumber();
			consignee.OH_FullName = "Local Consignee " + orgNumber;
			consignee.OH_Code = "LOCCON" + orgNumber;
			consignee.MainAddress.OA_Address1 = "Test Address Line";
			consignee.OH_RL_NKClosestPort = HomePort;
			return consignee;
		}

		OrgHeader fOverseasConsignee;
		public OrgHeader OverseasConsignee
		{
			get
			{
				if (fOverseasConsignee == null)
				{
					fOverseasConsignee = Factory.New<OrgHeader>();
					fOverseasConsignee.OH_Code = "OVSCNE";
					fOverseasConsignee.OH_IsConsignee = true;
					fOverseasConsignee.OH_FullName = "Overseas Consignee " + GetNextOrganisationNumber();
					fOverseasConsignee.MainAddress.OA_Address1 = "Test Address Line";
					fOverseasConsignee.OH_RL_NKClosestPort = OverseasPort;
				}
				return fOverseasConsignee;
			}
		}

		public OrgHeader CreateTestOverseasConsignee()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_FullName = "Overseas Consignee " + GetNextOrganisationNumber();
			consignee.MainAddress.OA_Address1 = "Test Address Line";
			consignee.OH_RL_NKClosestPort = OverseasPort;
			return consignee;
		}

		#endregion

		#region Consignors

		public OrgHeader[] LoadOverseasConsignors(int topCount, BusinessObjectFactory factory)
		{
			ZQuery overseasConsignorsFilter = new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, OverseasPort);
			overseasConsignorsFilter.AddToFilter(OrgHeaderSchema.OH_IsConsignor, ZBool.True);
			overseasConsignorsFilter.MaximumRows = topCount;

			OrgHeader[] result = (OrgHeader[])factory.Load(typeof(OrgHeader), overseasConsignorsFilter);
			if (result.Length > 0)
			{
				return result;
			}
			else
			{
				return new OrgHeader[] { CreateTestOverseasConsignor() };
			}
		}

		OrgHeader fLocalConsignor;
		public OrgHeader LocalConsignor
		{
			get
			{
				if (fLocalConsignor == null)
				{
					fLocalConsignor = Factory.New<OrgHeader>();
					fLocalConsignor.OH_Code = "LCLCNE";
					fLocalConsignor.OH_IsConsignor = true;
					fLocalConsignor.OH_FullName = "Local Consignor " + GetNextOrganisationNumber();
					fLocalConsignor.MainAddress.OA_Address1 = "Test Address Line";
					fLocalConsignor.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalConsignor;
			}
		}

		OrgHeader fOverseasConsignor;
		public OrgHeader OverseasConsignor
		{
			get
			{
				if (fOverseasConsignor == null)
				{
					fOverseasConsignor = Factory.New<OrgHeader>();
					fOverseasConsignor.OH_Code = "OVSCNE";
					fOverseasConsignor.OH_IsConsignor = true;
					fOverseasConsignor.OH_FullName = "Overseas Consignor " + GetNextOrganisationNumber();
					fOverseasConsignor.MainAddress.OA_Address1 = "Test Address Line";
					fOverseasConsignor.OH_RL_NKClosestPort = OverseasPort;
				}
				return fOverseasConsignor;
			}
		}

		public OrgHeader CreateTestLocalConsignor()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "Local Consignor " + GetNextOrganisationNumber();
			consignor.MainAddress.OA_Address1 = "Test Address Line";
			consignor.OH_RL_NKClosestPort = HomePort;
			return consignor;
		}

		public OrgHeader CreateTestOverseasConsignor()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_FullName = "Overseas Consignor " + GetNextOrganisationNumber();
			consignor.MainAddress.OA_Address1 = "Test Address Line";
			consignor.OH_RL_NKClosestPort = OverseasPort;
			return consignor;
		}

		#endregion

		#region Forwarders

		public class TestLocalSendingForwarder : OrgHeader
		{
			public TestLocalSendingForwarder(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				OH_IsForwarder = true;
				OH_FullName = "Local Sending Forwarder " + GetNextOrganisationNumber();
				MainAddress.OA_Address1 = "Test Address Line";
				OH_RL_NKClosestPort = HomePort;
			}
		}

		OrgHeader fLocalForwarder;
		public OrgHeader LocalForwarder
		{
			get
			{
				if (fLocalForwarder == null)
				{
					fLocalForwarder = Factory.New<OrgHeader>();
					fLocalForwarder.OH_Code = "LCLCFS";
					fLocalForwarder.OH_IsForwarder = true;
					fLocalForwarder.OH_FullName = "Local Forwarder " + GetNextOrganisationNumber();
					fLocalForwarder.MainAddress.OA_Address1 = "Test Address Line";
					fLocalForwarder.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalForwarder;
			}
		}

		public OrgHeader CreateOrgHeader()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_Code = "-" + result.PK.ToString().Substring(0, 5) + "-";
			result.OH_FullName = "Test OrgHeader";
			result.MainAddress.OA_Address1 = "Organisation Address";
			return result;
		}

		#endregion

		#region Depots

		public OrgHeader LocalDepot
		{
			get
			{
				if (fLocalDepot == null)
				{
					fLocalDepot = Factory.New<OrgHeader>();
					fLocalDepot.OH_Code = "LCLCFS";
					fLocalDepot.OH_IsMiscFreightServices = true;
					fLocalDepot.OH_IsPackDepot = true;
					fLocalDepot.OH_IsUnpackDepot = true;
					fLocalDepot.OH_FullName = "Local Depot " + GetNextOrganisationNumber();
					fLocalDepot.MainAddress.OA_Address1 = "Test Address Line";
					fLocalDepot.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalDepot;
			}
		}
		OrgHeader fLocalDepot;

		public OrgHeader AlternateLocalDepot
		{
			get
			{
				if (fAlternateLocalDepot == null)
				{
					fAlternateLocalDepot = Factory.New<OrgHeader>();
					fAlternateLocalDepot.OH_Code = "LCLCFS";
					fAlternateLocalDepot.OH_IsMiscFreightServices = true;
					fAlternateLocalDepot.OH_IsPackDepot = true;
					fAlternateLocalDepot.OH_IsUnpackDepot = true;
					fAlternateLocalDepot.OH_FullName = "Local Depot " + GetNextOrganisationNumber();
					fAlternateLocalDepot.MainAddress.OA_Address1 = "Test Address Line";
					fAlternateLocalDepot.OH_RL_NKClosestPort = HomePort;
				}
				return fAlternateLocalDepot;
			}
		}
		OrgHeader fAlternateLocalDepot;

		OrgHeader fOverseasDepot;
		public OrgHeader OverseasDepot
		{
			get
			{
				if (fOverseasDepot == null)
				{
					fOverseasDepot = Factory.New<OrgHeader>();
					fOverseasDepot.OH_Code = "OVSCFS";
					fOverseasDepot.OH_IsMiscFreightServices = true;
					fOverseasDepot.OH_IsPackDepot = true;
					fOverseasDepot.OH_IsUnpackDepot = true;
					fOverseasDepot.OH_FullName = "Overseas Depot " + GetNextOrganisationNumber();
					fOverseasDepot.MainAddress.OA_Address1 = "Test Address Line";
					fOverseasDepot.OH_RL_NKClosestPort = OverseasPort;
				}
				return fOverseasDepot;
			}
		}

		#endregion

		#region CTOs

		public OrgHeader LocalCTO
		{
			get
			{
				if (fLocalCTO == null)
				{
					fLocalCTO = Factory.New<OrgHeader>();
					fLocalCTO.OH_Code = "LCLCTO";
					fLocalCTO.OH_IsMiscFreightServices = true;
					fLocalCTO.OH_IsAirCTO = true;
					fLocalCTO.OH_IsSeaCTO = true;
					fLocalCTO.OH_FullName = "Local CTO " + GetNextOrganisationNumber();
					fLocalCTO.MainAddress.OA_Address1 = "Test Address Line";
					fLocalCTO.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalCTO;
			}
		}
		OrgHeader fLocalCTO;

		public OrgHeader AlternateLocalCTO
		{
			get
			{
				if (fAlternateLocalCTO == null)
				{
					fAlternateLocalCTO = Factory.New<OrgHeader>();
					fAlternateLocalCTO.OH_Code = "LCLCTO";
					fAlternateLocalCTO.OH_IsMiscFreightServices = true;
					fAlternateLocalCTO.OH_IsAirCTO = true;
					fAlternateLocalCTO.OH_IsSeaCTO = true;
					fAlternateLocalCTO.OH_FullName = "Local CTO " + GetNextOrganisationNumber();
					fAlternateLocalCTO.MainAddress.OA_Address1 = "Test Address Line";
					fAlternateLocalCTO.OH_RL_NKClosestPort = HomePort;
				}
				return fAlternateLocalCTO;
			}
		}
		OrgHeader fAlternateLocalCTO;

		public OrgHeader AlternateLocalCTO2
		{
			get
			{
				if (fAlternateLocalCTO2 == null)
				{
					fAlternateLocalCTO2 = Factory.New<OrgHeader>();
					fAlternateLocalCTO2.OH_Code = "LCLCTO";
					fAlternateLocalCTO2.OH_IsMiscFreightServices = true;
					fAlternateLocalCTO2.OH_IsAirCTO = true;
					fAlternateLocalCTO2.OH_IsSeaCTO = true;
					fAlternateLocalCTO2.OH_FullName = "Local CTO " + GetNextOrganisationNumber();
					fAlternateLocalCTO2.MainAddress.OA_Address1 = "Test Address Line";
					fAlternateLocalCTO2.OH_RL_NKClosestPort = HomePort;
				}
				return fAlternateLocalCTO2;
			}
		}
		OrgHeader fAlternateLocalCTO2;

		OrgHeader fOverseasCTO;
		public OrgHeader OverseasCTO
		{
			get
			{
				if (fOverseasCTO == null)
				{
					fOverseasCTO = Factory.New<OrgHeader>();
					fOverseasCTO.OH_Code = "OVSCTO";
					fOverseasCTO.OH_IsMiscFreightServices = true;
					fOverseasCTO.OH_IsAirCTO = true;
					fOverseasCTO.OH_IsSeaCTO = true;
					fOverseasCTO.OH_FullName = "Overseas CTO " + GetNextOrganisationNumber();
					fOverseasCTO.MainAddress.OA_Address1 = "Test Address Line";
					fOverseasCTO.OH_RL_NKClosestPort = OverseasPort;
				}
				return fOverseasCTO;
			}
		}

		#endregion

		#region ContainerYards

		public class TestLocalContainerYard : OrgHeader
		{
			public TestLocalContainerYard(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				OH_IsMiscFreightServices = true;
				OH_IsContainerYard = true;
				OH_FullName = "Local ContainerYard " + GetNextOrganisationNumber();
				MainAddress.OA_Address1 = "Test Address Line";
				OH_RL_NKClosestPort = HomePort;
			}
		}

		OrgHeader fLocalContainerYard;
		public OrgHeader LocalContainerYard
		{
			get
			{
				if (fLocalContainerYard == null)
				{
					fLocalContainerYard = Factory.New<OrgHeader>();
					fLocalContainerYard.OH_Code = "LCLCPK";
					fLocalContainerYard.OH_IsMiscFreightServices = true;
					fLocalContainerYard.OH_IsContainerYard = true;
					fLocalContainerYard.OH_FullName = "Local ContainerYard " + GetNextOrganisationNumber();
					fLocalContainerYard.MainAddress.OA_Address1 = "Test Address Line";
					fLocalContainerYard.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalContainerYard;
			}
		}

		public OrgHeader AlternateLocalContainerYard
		{
			get
			{
				if (fLocalContainerYard2 == null)
				{
					fLocalContainerYard2 = Factory.New<OrgHeader>();
					fLocalContainerYard2.OH_Code = "LCLCPK";
					fLocalContainerYard2.OH_IsMiscFreightServices = true;
					fLocalContainerYard2.OH_IsContainerYard = true;
					fLocalContainerYard2.OH_FullName = "Local ContainerYard " + GetNextOrganisationNumber();
					fLocalContainerYard2.MainAddress.OA_Address1 = "Test Address Line";
					fLocalContainerYard2.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalContainerYard2;
			}
		}
		OrgHeader fLocalContainerYard2;

		public OrgHeader AlternateLocalContainerYard2
		{
			get
			{
				if (fLocalContainerYard3 == null)
				{
					fLocalContainerYard3 = Factory.New<OrgHeader>();
					fLocalContainerYard3.OH_Code = "LCLCPK";
					fLocalContainerYard3.OH_IsMiscFreightServices = true;
					fLocalContainerYard3.OH_IsContainerYard = true;
					fLocalContainerYard3.OH_FullName = "Local ContainerYard " + GetNextOrganisationNumber();
					fLocalContainerYard3.MainAddress.OA_Address1 = "Test Address Line";
					fLocalContainerYard3.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalContainerYard3;
			}
		}
		OrgHeader fLocalContainerYard3;

		OrgHeader fOverseasContainerYard;
		public OrgHeader OverseasContainerYard
		{
			get
			{
				if (fOverseasContainerYard == null)
				{
					fOverseasContainerYard = Factory.New<OrgHeader>();
					fOverseasContainerYard.OH_Code = "OVSCPK";
					fOverseasContainerYard.OH_IsMiscFreightServices = true;
					fOverseasContainerYard.OH_IsContainerYard = true;
					fOverseasContainerYard.OH_FullName = "Overseas ContainerYard " + GetNextOrganisationNumber();
					fOverseasContainerYard.MainAddress.OA_Address1 = "Test Address Line";
					fOverseasContainerYard.OH_RL_NKClosestPort = OverseasPort;
				}
				return fOverseasContainerYard;
			}
		}

		#endregion

		#region Carriers

		OrgHeader fLocalLocalTransportCo;
		public OrgHeader LocalLocalTransportCo
		{
			get
			{
				if (fLocalLocalTransportCo == null)
				{
					fLocalLocalTransportCo = Factory.New<OrgHeader>();
					fLocalLocalTransportCo.OH_IsShippingProvider = true;
					fLocalLocalTransportCo.OH_IsLocalTransport = true;
					fLocalLocalTransportCo.OH_FullName = "Local LocalTransportCo " + GetNextOrganisationNumber();
					fLocalLocalTransportCo.MainAddress.OA_Address1 = "Test Address Line";
					fLocalLocalTransportCo.OH_RL_NKClosestPort = HomePort;
				}
				return fLocalLocalTransportCo;
			}
		}

		OrgHeader fOverseasLocalTransportCo;
		public OrgHeader OverseasLocalTransportCo
		{
			get
			{
				if (fOverseasLocalTransportCo == null)
				{
					fOverseasLocalTransportCo = Factory.New<OrgHeader>();
					fOverseasLocalTransportCo.OH_IsShippingProvider = true;
					fOverseasLocalTransportCo.OH_IsLocalTransport = true;
					fOverseasLocalTransportCo.OH_FullName = "Overseas LocalTransportCo " + GetNextOrganisationNumber();
					fOverseasLocalTransportCo.MainAddress.OA_Address1 = "Test Address Line";
					fOverseasLocalTransportCo.OH_RL_NKClosestPort = OverseasPort;
				}
				return fOverseasLocalTransportCo;
			}
		}

		OrgHeader fShippingCompany1;
		public OrgHeader ShippingCompany1
		{
			get
			{
				if (fShippingCompany1 == null)
				{
					fShippingCompany1 = Factory.New<OrgHeader>();
					fShippingCompany1.OH_IsShippingLine = true;
					fShippingCompany1.OH_IsShippingProvider = true;
					fShippingCompany1.OH_IsCreditor = true;
					fShippingCompany1.OH_FullName = "Shipping Company " + GetNextOrganisationNumber();
					fShippingCompany1.MainAddress.OA_Address1 = "Test Address Line";
					fShippingCompany1.OH_RL_NKClosestPort = HomePort;
				}
				return fShippingCompany1;
			}
		}

		OrgHeader fForwardingCompany1;
		public OrgHeader ForwardingCompany1
		{
			get
			{
				if (fForwardingCompany1 == null)
				{
					fForwardingCompany1 = Factory.New<OrgHeader>();
					fForwardingCompany1.OH_IsForwarder = true;
					fForwardingCompany1.OH_IsCreditor = true;
					fForwardingCompany1.OH_FullName = "Forwarding Company " + GetNextOrganisationNumber();
					fForwardingCompany1.MainAddress.OA_Address1 = "Test Address Line";
					fForwardingCompany1.OH_RL_NKClosestPort = HomePort;
				}
				return fForwardingCompany1;
			}
		}

		OrgHeader fShippingCompany2;
		public OrgHeader ShippingCompany2
		{
			get
			{
				if (fShippingCompany2 == null)
				{
					fShippingCompany2 = Factory.New<OrgHeader>();
					fShippingCompany2.OH_IsShippingLine = true;
					fShippingCompany2.OH_IsCreditor = true;
					fShippingCompany2.OH_FullName = "Shipping Company " + GetNextOrganisationNumber();
					fShippingCompany2.MainAddress.OA_Address1 = "Test Address Line";
					fShippingCompany2.OH_RL_NKClosestPort = HomePort;
				}
				return fShippingCompany2;
			}
		}

		OrgHeader fShippingCompany3;
		public OrgHeader ShippingCompany3
		{
			get
			{
				if (fShippingCompany3 == null)
				{
					fShippingCompany3 = Factory.New<OrgHeader>();
					fShippingCompany3.OH_IsShippingLine = true;
					fShippingCompany3.OH_IsCreditor = true;
					fShippingCompany3.OH_FullName = "Shipping Company " + GetNextOrganisationNumber();
					fShippingCompany3.MainAddress.OA_Address1 = "Test Address Line";
					fShippingCompany3.OH_RL_NKClosestPort = HomePort;
				}
				return fShippingCompany3;
			}
		}

		OrgHeader fShippingCompanyNonCreditor;
		public OrgHeader ShippingCompanyNonCreditor
		{
			get
			{
				if (fShippingCompanyNonCreditor == null)
				{
					fShippingCompanyNonCreditor = Factory.New<OrgHeader>();
					fShippingCompanyNonCreditor.OH_IsShippingLine = true;
					fShippingCompanyNonCreditor.OH_IsCreditor = false;
					fShippingCompanyNonCreditor.OH_FullName = "Shipping Company N AR" + GetNextOrganisationNumber();
					fShippingCompanyNonCreditor.MainAddress.OA_Address1 = "Test Address Line";
					fShippingCompanyNonCreditor.OH_RL_NKClosestPort = HomePort;
				}
				return fShippingCompanyNonCreditor;
			}
		}

		#endregion

		#region Sending/Receiving Forwarder Agent Addresses

		protected static ZString AgentStatusPublished
		{
			get { return AgentStatusList.Codes.Published; }
		}
		protected static ZString AgentStatusAppointed
		{
			get { return AgentStatusList.Codes.Appointed; }
		}
		protected static ZString AgentStatusHandles
		{
			get { return AgentStatusList.Codes.Handles; }
		}
		protected static ZString AgentStatusGatewayAgent
		{
			get { return AgentStatusList.Codes.GatewayAgent; }
		}
		protected static ZString AgentStatusGatewayAgentWithTariff
		{
			get { return AgentStatusList.Codes.GatewayAgentWithTariff; }
		}

		protected static ZString AgentDirectionBoth
		{
			get { return AgentDirectionList.Codes.Both; }
		}
		protected static ZString AgentDirectionExport
		{
			get { return AgentDirectionList.Codes.Export; }
		}
		protected static ZString AgentDirectionImport
		{
			get { return AgentDirectionList.Codes.Import; }
		}

		int addressCount;

		protected OrgHeader GetForwarder()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader result = newFactory.New<OrgHeader>();
			result.OH_Code = string.Format("FWD{0}", ++addressCount);
			result.OH_RL_NKClosestPort = HomePort;
			result.MainAddress.OA_Address1 = string.Format("{0} main address", result.OH_Code);
			result.OH_IsForwarder = true;

			return result;
		}

		protected OrgAddress GetForwarderAgentAddress(ZString agentStatus, ZString agentPortOrCountry, ZString agentDirection, ZString agentTransportMode)
		{
			OrgHeader forwarder = GetForwarder();
			OrgAddress address = AddForwarderAgentAddress(forwarder, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);

			return address;
		}

		protected OrgAddress AddForwarderAgentAddress(OrgHeader forwarder, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgAddress address = forwarder.Addresses.AddNew();
			address.OA_Code = string.Format("{0}_{1}_{2}_{3}_{4}", agentPortOrCountry, agentTransportMode, agentDirection, agentStatus, ++addressCount);
			address.OA_Address1 = address.OA_Code;

			return AddForwarderAgentAddress(forwarder, address, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);
		}

		protected OrgAddress AddForwarderAgentAddress(OrgHeader forwarder, OrgAddress address, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgAppointedAgentPorts newAppAgent;

			if (agentStatus == AgentStatusGatewayAgent
				|| agentStatus == AgentStatusGatewayAgentWithTariff)
			{
				newAppAgent = forwarder.AppointedGatewayAgentPorts.AddNew();
			}
			else
			{
				newAppAgent = forwarder.AppointedAgentPorts.AddNew();
			}

			newAppAgent.O5_PortOrCountry = agentPortOrCountry;
			newAppAgent.O5_OA_AgentOfficeAddress = address.PK;
			newAppAgent.O5_AgentDirection = agentDirection;

			switch (agentTransportMode)
			{
				case Constants.TransportModes.Air:
					newAppAgent.O5_AirAgentStatus = agentStatus;
					break;

				case Constants.TransportModes.Rail:
					newAppAgent.O5_RailAgentStatus = agentStatus;
					break;

				case Constants.TransportModes.Road:
					newAppAgent.O5_RoadAgentStatus = agentStatus;
					break;

				case Constants.TransportModes.Sea:
					newAppAgent.O5_SeaAgentStatus = agentStatus;
					break;
			}

			forwarder.Factory.Save();

			return address;
		}

		#endregion

		#endregion

		#region Test Branches

		protected GlbBranch LocalBranch;
		protected GlbBranch OverseasBranch;

		protected void SetupCurrentBranchAsDepot()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsMiscFreightServices = true;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsPackDepot = true;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;

			Factory.Save();
		}

		protected void SetupLocalBranchAsDepot()
		{
			LocalDepot.Factory.Save();
			SetupLocalBranch(LocalDepot, "CFS");
		}

		protected void SetupLocalBranchAsLocalCartage()
		{
			if (LocalCartageBranch == null)
			{
				LocalLocalTransportCo.Factory.Save();
				LocalCartageBranch = SetupLocalBranch(LocalLocalTransportCo, "LBH");
			}
		}
		GlbBranch LocalCartageBranch;

		protected GlbBranch SetupLocalBranch(OrgHeader orgProxy, string branchCode)
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			LocalBranch = currentCompany.Branches.AddNew();
			LocalBranch.GB_Code = branchCode;
			LocalBranch.GB_BranchName = "Local Branch";
			LocalBranch.GB_RL_NKHomePort = orgProxy.MainAddress.OA_RL_NKRelatedPortCode;
			LocalBranch.GB_Phone = "2342341";
			LocalBranch.GB_Fax = "345234523";
			LocalBranch.GB_OH_OrgProxy = orgProxy.PK;

			Factory.Save();

			return LocalBranch;
		}

		protected void SetupOverseasBranch()
		{
			OverseasLocalTransportCo.Factory.Save();
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			OverseasBranch = currentCompany.Branches.AddNew();
			OverseasBranch.GB_Code = "OBC";
			OverseasBranch.GB_BranchName = "Overseas Branch";
			OverseasBranch.GB_RL_NKHomePort = OverseasLocalTransportCo.MainAddress.OA_RL_NKRelatedPortCode;
			OverseasBranch.GB_Phone = "2342341";
			OverseasBranch.GB_Fax = "345234523";
			OverseasBranch.GB_OH_OrgProxy = OverseasLocalTransportCo.PK;

			Factory.Save();
		}

		#endregion

		#region Test Business Objects

		#region Forwarding
		//
		//		ForwardingShipment Shipment = GetExportShipment(typeof(ForwardingShipment));
		//
		public CommonShipment GetExportShipment(Type shipmentType)
		{
			CommonShipment result = (CommonShipment)Factory.New(shipmentType);

			result.JS_RL_NKOrigin = HomePort;
			result.JS_RL_NKDestination = OverseasPort;
			result.ConsignorPK = CreateTestLocalConsignor().PK;
			result.ConsigneePK = CreateTestOverseasConsignee().PK;

			return result;
		}

		public CommonShipment GetImportShipment(Type shipmentType)
		{
			return GetImportShipment(shipmentType, Factory);
		}

		public CommonShipment GetImportShipment(Type shipmentType, BusinessObjectFactory factory)
		{
			CommonShipment result = (CommonShipment)factory.New(shipmentType);
			result.JS_RL_NKOrigin = OverseasPort;
			result.JS_RL_NKDestination = HomePort;
			result.ConsignorPK = CreateTestOverseasConsignor().PK;
			result.ConsigneePK = CreateTestLocalConsignee().PK;
			return result;
		}

		public CommonConsol GetExportConsol(Type consolType)
		{
			CommonConsol consol = (CommonConsol)Factory.New(consolType);
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "TESVOY";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_ETD = ZDateTime.Today.AddDays(2);

			return consol;
		}

		public CommonConsol GetImportConsol(Type consolType)
		{
			return GetImportConsol(consolType, Factory);
		}
		public CommonConsol GetImportConsol(Type consolType, BusinessObjectFactory factory)
		{
			CommonConsol consol = (CommonConsol)factory.New(consolType);
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = "TESVOY";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_ETA = ZDateTime.Today.AddDays(2);
			return consol;
		}

		#endregion

		#region Sailings

		#region Sea Voyages

		protected void SetUpVoyage()
		{
			fSeaVoyage = Factory.New<JobVoyage>();
			fSeaVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			fSeaVoyage.JV_VoyageFlight = "12";
			fSeaVoyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First().RV_FK;
			fSeaVoyage.JV_OH_Line = ShippingCompany1.PK;

			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;

			o1.JA_RL_NKPortOfLoading = AlternateHomePort;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5);
			fSeaVoyage.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = HomePort;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(8);
			fSeaVoyage.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = OverseasPort;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(35);
			fSeaVoyage.Origins.Add(o3);

			d1.JB_RL_NKPortOfDischarge = HomePort;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(7);
			fSeaVoyage.Destinations.Add(d1);

			d2.JB_RL_NKPortOfDischarge = OverseasPort;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(37);
			fSeaVoyage.Destinations.Add(d2);

			d3.JB_RL_NKPortOfDischarge = AlternateHomePort;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(55);
			fSeaVoyage.Destinations.Add(d3);

			fSeaVoyage.GenerateSailings();
		}

		public JobVoyage SeaVoyage
		{
			get
			{
				if (fSeaVoyage == null)
				{
					SetUpVoyage();
				}
				return fSeaVoyage;
			}
		}

		public JobSailing ExportSailing
		{
			get
			{
				if (fExportSailing == null)
				{
					fExportSailing = GetSailingFromPortPair(SeaVoyage, HomePort, OverseasPort);
				}
				return fExportSailing;
			}
		}

		public JobSailing DomesticSailing
		{
			get
			{
				if (fDomesticSailing == null)
				{
					fDomesticSailing = GetSailingFromPortPair(SeaVoyage, AlternateHomePort, HomePort);
				}
				return fDomesticSailing;
			}
		}

		public JobSailing ImportSailing
		{
			get
			{
				if (fImportSailing == null)
				{
					fImportSailing = GetSailingFromPortPair(SeaVoyage, OverseasPort, AlternateHomePort);
				}
				return fImportSailing;
			}
		}

		public VoyageOrigin OverseasPortOrigin
		{
			get
			{
				if (fOverseasPortOrigin == null)
				{
					fOverseasPortOrigin = GetOriginFromLoad(SeaVoyage, OverseasPort);
				}
				return fOverseasPortOrigin;
			}
		}

		public VoyageOrigin AlternateHomePortOrigin
		{
			get
			{
				if (fAlternateHomePortOrigin == null)
				{
					fAlternateHomePortOrigin = GetOriginFromLoad(SeaVoyage, AlternateHomePort);
				}
				return fAlternateHomePortOrigin;
			}
		}

		public VoyageDestination OverseasPortDestination
		{
			get
			{
				if (fOverseasPortDestination == null)
				{
					fOverseasPortDestination = GetDestinationFromDischarge(SeaVoyage, OverseasPort);
				}
				return fOverseasPortDestination;
			}
		}

		public VoyageDestination AlternateHomePortDestination
		{
			get
			{
				if (fAlternateHomePortDestination == null)
				{
					fAlternateHomePortDestination = GetDestinationFromDischarge(SeaVoyage, AlternateHomePort);
				}
				return fAlternateHomePortDestination;
			}
		}

		#endregion

		#region Air Flights

		protected void SetUpFlight()
		{
			fFlight = Factory.New<JobVoyage>();
			fFlight.JV_AirSeaRoad = Constants.TransportModes.Air;
			fFlight.JV_VoyageFlight = "QF12";
			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			//RefUNLOCO R1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCO.Schema.RL_Code, AlternateHomePort) as RefUNLOCO;
			//RefUNLOCO R2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCO.Schema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			//RefUNLOCO R4 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCO.Schema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r5 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "USLAX") as RefUNLOCO;
			//RefUNLOCO R6 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCO.Schema.RL_Code, AlternateHomePort) as RefUNLOCO;

			o1.JA_RL_NKPortOfLoading = AlternateHomePort;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5).AddHours(10);
			fFlight.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = HomePort;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(5).AddHours(13);
			fFlight.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(6).AddHours(10);
			fFlight.Origins.Add(o3);

			d1.JB_RL_NKPortOfDischarge = HomePort;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(5).AddHours(11);
			fFlight.Destinations.Add(d1);

			d2.JB_RL_NKPortOfDischarge = r5.RL_Code;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(6).AddHours(6);
			fFlight.Destinations.Add(d2);

			d3.JB_RL_NKPortOfDischarge = AlternateHomePort;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(6).AddHours(23);
			fFlight.Destinations.Add(d3);

			fFlight.GenerateSailings();
		}

		public JobVoyage Flight
		{
			get
			{
				if (fFlight == null)
				{
					SetUpFlight();
				}
				return fFlight;
			}
		}

		public JobSailing SydLaxFlightLeg
		{
			get
			{
				if (fSydLaxFlightLeg == null)
				{
					fSydLaxFlightLeg = GetSailingFromPortPair(Flight, HomePort, "USLAX");
				}
				return fSydLaxFlightLeg;
			}
		}

		public JobSailing MelSydFlightLeg
		{
			get
			{
				if (fMelSydFlightLeg == null)
				{
					fMelSydFlightLeg = GetSailingFromPortPair(Flight, AlternateHomePort, HomePort);
				}
				return fMelSydFlightLeg;
			}
		}

		public JobSailing LaxMelFlightLeg
		{
			get
			{
				if (fLaxMelFlightLeg == null)
				{
					fLaxMelFlightLeg = GetSailingFromPortPair(Flight, "USLAX", AlternateHomePort);
				}
				return fLaxMelFlightLeg;
			}
		}

		public VoyageOrigin MelFlightOrigin
		{
			get
			{
				if (fMelFlightOrigin == null)
				{
					fMelFlightOrigin = GetOriginFromLoad(Flight, AlternateHomePort);
				}
				return fMelFlightOrigin;
			}
		}

		public VoyageDestination LaxFlightDestination
		{
			get
			{
				if (fLaxFlightDestination == null)
				{
					fLaxFlightDestination = GetDestinationFromDischarge(Flight, OverseasPort);
				}
				return fLaxFlightDestination;
			}
		}

		#endregion

		#region Rail Journey

		protected void SetUpJourney()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "GABRIELA";

			fJourney = Factory.New<JobVoyage>();
			fJourney.JV_AirSeaRoad = Constants.TransportModes.Rail;
			fJourney.JV_VoyageFlight = "1";
			fJourney.JV_RV_NKVessel = vessel.RV_FK;
			VoyageOrigin o1 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o2 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageOrigin o3 = Factory.New(typeof(VoyageOrigin)) as VoyageOrigin;
			VoyageDestination d1 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d2 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;
			VoyageDestination d3 = Factory.New(typeof(VoyageDestination)) as VoyageDestination;

			var overseasPort = OverseasPort;
			var alternateHomePort = AlternateHomePort;

			RefUNLOCO r1 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, alternateHomePort) as RefUNLOCO;
			RefUNLOCO r2 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r3 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, overseasPort) as RefUNLOCO;
			RefUNLOCO r4 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			RefUNLOCO r5 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, overseasPort) as RefUNLOCO;
			RefUNLOCO r6 = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, alternateHomePort) as RefUNLOCO;

			o1.JA_RL_NKPortOfLoading = r1.RL_Code;
			o1.JA_E_DEP = ZDateTime.Today.AddDays(5);
			fJourney.Origins.Add(o1);

			o2.JA_RL_NKPortOfLoading = r2.RL_Code;
			o2.JA_E_DEP = ZDateTime.Today.AddDays(8);
			fJourney.Origins.Add(o2);

			o3.JA_RL_NKPortOfLoading = r3.RL_Code;
			o3.JA_E_DEP = ZDateTime.Today.AddDays(35);
			fJourney.Origins.Add(o3);

			d1.JB_RL_NKPortOfDischarge = r4.RL_Code;
			d1.JB_E_ARV = ZDateTime.Today.AddDays(7);
			fJourney.Destinations.Add(d1);

			d2.JB_RL_NKPortOfDischarge = r5.RL_Code;
			d2.JB_E_ARV = ZDateTime.Today.AddDays(37);
			fJourney.Destinations.Add(d2);

			d3.JB_RL_NKPortOfDischarge = r6.RL_Code;
			d3.JB_E_ARV = ZDateTime.Today.AddDays(55);
			fJourney.Destinations.Add(d3);

			fJourney.GenerateSailings();
		}

		public JobVoyage Journey
		{
			get
			{
				if (fJourney == null)
				{
					SetUpJourney();
				}
				return fJourney;
			}
		}

		public JobSailing SydLaxSector
		{
			get
			{
				if (fSydLaxSector == null)
				{
					fSydLaxSector = GetSailingFromPortPair(Journey, "AUSYD", OverseasPort);
				}
				return fSydLaxSector;
			}
		}

		public JobSailing MelSydSector
		{
			get
			{
				if (fMelSydSector == null)
				{
					fMelSydSector = GetSailingFromPortPair(Journey, AlternateHomePort, "AUSYD");
				}
				return fMelSydSector;
			}
		}

		public JobSailing LaxMelSector
		{
			get
			{
				if (fLaxMelSector == null)
				{
					fLaxMelSector = GetSailingFromPortPair(Journey, OverseasPort, AlternateHomePort);
				}
				return fLaxMelSector;
			}
		}

		public VoyageOrigin MelJourOrigin
		{
			get
			{
				if (fMelJourOrigin == null)
				{
					fMelJourOrigin = GetOriginFromLoad(Journey, AlternateHomePort);
				}
				return fMelJourOrigin;
			}
		}

		public VoyageDestination LaxJourDestination
		{
			get
			{
				if (fLaxJourDestination == null)
				{
					fLaxJourDestination = GetDestinationFromDischarge(Journey, OverseasPort);
				}
				return fLaxJourDestination;
			}
		}

		#endregion

		#region Implementation

		protected JobVoyage fSeaVoyage;
		protected VoyageOrigin fOverseasPortOrigin;
		protected VoyageOrigin fAlternateHomePortOrigin;
		protected VoyageDestination fOverseasPortDestination;
		protected VoyageDestination fAlternateHomePortDestination;
		protected JobSailing fExportSailing;
		protected JobSailing fDomesticSailing;
		protected JobSailing fImportSailing;

		protected JobVoyage fFlight;
		protected VoyageOrigin fMelFlightOrigin;
		protected VoyageDestination fLaxFlightDestination;
		protected JobSailing fSydLaxFlightLeg;
		protected JobSailing fMelSydFlightLeg;
		protected JobSailing fLaxMelFlightLeg;

		protected JobVoyage fJourney;
		protected VoyageOrigin fMelJourOrigin;
		protected VoyageDestination fLaxJourDestination;
		protected JobSailing fSydLaxSector;
		protected JobSailing fMelSydSector;
		protected JobSailing fLaxMelSector;

		//protected BusinessObjectFactory Factory;

		public JobSailing GetSailingFromPortPair(JobVoyage voyageToSearch, ZString load, ZString discharge)
		{
			JobSailing result = null;
			foreach (JobSailing sailing in voyageToSearch.Sailings)
			{
				if ((sailing.Origin.JA_RL_NKPortOfLoading == load)
					&& (sailing.Destination.JB_RL_NKPortOfDischarge == discharge))
				{
					result = sailing;
					break;
				}
			}
			return result;
		}

		public VoyageOrigin GetOriginFromLoad(JobVoyage voyageToSearch, ZString load)
		{
			VoyageOrigin result = null;
			foreach (VoyageOrigin origin in voyageToSearch.Origins)
			{
				if (origin.JA_RL_NKPortOfLoading == load)
				{
					result = origin;
					break;
				}
			}
			return result;
		}

		public VoyageDestination GetDestinationFromDischarge(JobVoyage voyageToSearch, ZString discharge)
		{
			VoyageDestination result = null;
			foreach (VoyageDestination destination in voyageToSearch.Destinations)
			{
				if (destination.JB_RL_NKPortOfDischarge == discharge)
				{
					result = destination;
					break;
				}
			}
			return result;
		}

		#endregion

		#endregion

		#endregion

		#region Implementation

		JobVoyage CreateVoyage()
		{
			JobVoyage result = Factory.New<JobVoyage>();
			result.JV_RV_NKVessel = TestVessel1Name;
			result.JV_VoyageFlight = TestVoyage1VoyageNumber;
			return result;
		}

		JobSailing CreateNewSailing(ZString loadPort, ZString dischargePort, ZDateTime departureTime, ZDateTime arrivalTime)
		{
			VoyageOrigin origin = null;
			VoyageOrigin[] origins = (VoyageOrigin[])TestVoyage1.Origins.Find(new ZQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, loadPort));
			if (origins.Length > 0)
			{
				origin = origins[0];
			}
			if (origin == null)
			{
				origin = TestVoyage1.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = loadPort;
			}
			origin.JA_E_DEP = departureTime;

			VoyageDestination destination = null;
			VoyageDestination[] destinations = (VoyageDestination[])TestVoyage1.Destinations.Find(new ZQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, dischargePort));
			if (destinations.Length > 0)
			{
				destination = destinations[0];
			}
			if (destination == null)
			{
				destination = TestVoyage1.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = dischargePort;
			}
			destination.JB_E_ARV = arrivalTime;
			TestVoyage1.GenerateSailings();
			ZQuery sailingFilter = new ZQuery(JobSailingSchema.JX_JA, origin.PK);
			sailingFilter.AddToFilter(JobSailingSchema.JX_JB, destination.PK);
			var result = Factory.LoadTop1<JobSailing>(sailingFilter);
			if (result == null)
			{
				result = TestVoyage1.Sailings.AddNew();
				result.JX_JB = destination.PK;
				result.JX_JA = origin.PK;
			}
			return result;
		}

		protected virtual string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}
		#endregion
	}
}
