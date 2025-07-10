using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

// TODO : REmove this?
namespace Enterprise.Freight.CFS.Business
{
	public class ConstantsAndReusables
	{
		#region Query Decider Filter List Codes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public abstract class FilterTypes
		{
			public const string None = "None";
			public const string All = "All";
			public const string Common = "Common";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public abstract class PortFilterTypes : FilterTypes
		{
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public abstract class NumberFilterTypes : FilterTypes
		{
			public const string LoadList = "Load List #";
			public const string HouseBill = "HouseBill";
			public const string MasterBill = "MasterBill";
			public const string Shipment = "Shipment #";
			public const string Container = "Container #";
			public const string ContainerJob = "Container Job #";
			public const string InterimReceipt = "Interim Receipt #";
			public const string Release = "Release #";
			public const string Job = "Job #";
			public const string ClientRef = "Client Ref";
			public const string InvoiceNo = "Invoice #";
			public const string CustomsEntryNumber = "Customs Entry #";
			public const string CarrierBookingRef = "Carriers Booking Ref #";
			public const string AdditionalReferenceNumbers = "Additional Reference #";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public abstract class OrgFilterTypes : FilterTypes
		{
			public const string Client = "Client";
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";
			public const string Line = "Shipping Line";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related.")]
		public abstract class DateFilterTypes : FilterTypes
		{
			public const string ETD = "ETD";
			public const string ETA = "ETA";
			public const string Unpack = "Unpack";
			public const string Pack = "Pack";
			public const string Available = "Available";
			public const string Storage = "Storage";
			public const string Arrival = "Arrival";
			public const string Departure = "Departure";
			public const string Receipt = "Receipt";
			public const string RegisteredDate = "Registered Date";
		}

		#endregion

		public ConstantsAndReusables(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}
		readonly BusinessObjectFactory Factory;

#if DEBUG

		public JobSailing CreateNewSailing(bool import)
		{
			ZString domesticPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString foreignPort = GetAForeignPort("");

			if (import)
			{
				return CreateNewSailing(foreignPort, domesticPort, ZDateTime.Today.AddDays(-1), ZDateTime.Today);
			}
			else
			{
				return CreateNewSailing(domesticPort, foreignPort, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			}
		}

		public JobSailing CreateNewSailing()
		{
			return CreateNewSailing("SGSIN", "NZAKL", ZDateTime.Today.AddDays(-1), ZDateTime.Today);
		}

		public JobSailing CreateNewSailing(ZString portOfLoadnig, ZString portOfDischarge)
		{
			return CreateNewSailing(portOfLoadnig, portOfDischarge, ZDateTime.Today.AddDays(-1), ZDateTime.Today);
		}

		public JobSailing CreateNewSailing(ZString portOfLoadnig, ZString portOfDischarge, ZDateTime eTD, ZDateTime eTA)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = portOfLoadnig;
			origin.JA_E_DEP = eTD;
			voyage.Origins.Add(origin);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = eTA;
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		public ZString GetAForeignPort(ZString portToExclude)
		{
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode);
			filter.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, portToExclude);
			var firstForeignLoco = Factory.LoadTop1<RefUNLOCO>(filter);

			return firstForeignLoco != null ? firstForeignLoco.RL_Code : ZString.Empty;
		}

		public void EnsureCurrentCompanyMatchesCurrentBranch()
		{
			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			var loco = Factory.LoadTop1<RefUNLOCO>(filter);

			if (loco != null)
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = loco.RL_RN_NKCountryCode;
			}
		}

		public void SetCountryCode(ZString code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = code;
		}

#endif
	}
}
