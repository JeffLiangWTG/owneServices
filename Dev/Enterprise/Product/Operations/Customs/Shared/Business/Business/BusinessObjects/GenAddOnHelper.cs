using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class GenAddOnHelper
	{
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]  // I "considered a design that does not require that 'genAddOn' be an out parameter".  Then I considered make the analyser STFU. And the latter won.
		public static void FindOrMakeNewAddOn(string addOnCode, BusinessObject parentBizO, out GenAddOnColumn genAddOn)
		{
			var addOnStatusQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentBizO.PK);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, parentBizO.TablePrefix);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, addOnCode);
			addOnStatusQuery.FetchOnlyFromLocalCache = !parentBizO.IsInDatabase;
			genAddOn = parentBizO.Factory.LoadTop1<GenAddOnColumn>(addOnStatusQuery);
			if (genAddOn == null)
			{
				genAddOn = parentBizO.Factory.New<GenAddOnColumn>();
				genAddOn.XA_Name = addOnCode;
				((IAddOnColumn)genAddOn).Parent = parentBizO;
			}
			else
			{
				SetParentIfNeeded(genAddOn, parentBizO);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]  // I "considered a design that does not require that 'genAddOn' be an out parameter".  Then I considered make the analyser STFU. And the latter won.
		public static void Find(string addOnCode, BusinessObject parentBizO, out GenAddOnColumn genAddOn)
		{
			var addOnStatusQuery = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentBizO.PK);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, parentBizO.TablePrefix);
			addOnStatusQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, addOnCode);
			addOnStatusQuery.FetchOnlyFromLocalCache = !parentBizO.IsInDatabase;
			genAddOn = parentBizO.Factory.LoadTop1<GenAddOnColumn>(addOnStatusQuery);
			SetParentIfNeeded(genAddOn, parentBizO);
		}

		static void SetParentIfNeeded(IAddOnColumn genAddOn, BusinessObject parentBizO)
		{
			if (genAddOn != null)
			{
				genAddOn.Parent = parentBizO;
			}
		}

		public const string PlaceOfExitCode = "EXI";
		public const string CustomsActionTextAddOnTypeCode = "CAT";
		public const string HandlingInformationCodeType = "HND";
		public const string ValueOfGoodsCodeType = "VOG";
		public const string CurrencyCodeType = "CUR";
		public const string Trailer1Code = "TR1";
		public const string Trailer2Code = "TR2";
		public const string CourierConsignmentType = "COU";
		public const string NatSimplificatorInd = "NSI";
		public const string DeclarantAddressPK = "DAPK";
		public const string DeclEmailAddr = "DEA";
		public const string IsVehicles = "VEH";
		public const string ChargePaymentOrDestinationIDType = "PDI";
		public const string IsTransshipment = "TRS";
		public const string IsBroken = "BRK";
		public const string BillOfLadingItem = "BLI";
		public const string MaxMin = "MXMN";
		public const string ArrivedQuantity = "AQY";
		public const string ArrivedWeight = "AWT";
		public const string AMA_IsTramp = "AMA_IsTramp";
		public const string TotalAmount = "TAT";
		public const string PackageNetWeight = "PkgNW";
		public const string PackageNetWeightUQ = "PkgNWUQ";
		public const string LastForeignPort = "LFP";
		public const string ASY_PortOfExit = "ASY_PortOfExit";
		public const string ASY_LocalReferenceNumber = "ASY_LocalReferenceNumber";
		public const string IncludeRoutingSecurityData = "ES_IncludeRoutingSecurityData";
		public const string ENSReuse = "ENS";
		public const string TransportType = "TransportType";
		public const string TypeOfBillDocument = "TypeOfBill";
		public const string MultipleMRNIndicator = "MultipleMRNIndicator";
		public const string TankerStatus = "TankerStatus";
		public const string CommunicationLanguage = "CommunicationLanguage";
		public const string EdecOriginalTraderUID = "EdecOriginalTraderUID";
		public const string NextProcedure = "NextProcedure";
	}
}
