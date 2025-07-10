namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	using CargoWise.Types;

	public class INPDECTestClass : SGCUSDECTestClass, IINPDEC
	{
		public INPDECTestClass()
			: base()
		{
		}

		#region IINPDEC Members

		public ZBool Is2bStoredBWCY
		{
			get { return fIs2bStoredBWCY; }
			set { fIs2bStoredBWCY = value; }
		}
		ZBool fIs2bStoredBWCY;

		public ZBool Is2bStoredCfw
		{
			get { return isStoredInCfw; }
			set { isStoredInCfw = value; }
		}
		ZBool isStoredInCfw;

		public ZBool Is2bStoredC2y
		{
			get { return isStoredInC2y; }
			set { isStoredInC2y = value; }
		}
		ZBool isStoredInC2y;

		public ZBool IsTemporaryConsignment
		{
			get { return fIsTemporaryConsignment; }
			set { fIsTemporaryConsignment = value; }
		}
		ZBool fIsTemporaryConsignment;

		public ZDate StartDateOfTemporaryImport
		{
			get { return fStartDateOfTemporaryImport; }
			set { fStartDateOfTemporaryImport = value; }
		}
		ZDate fStartDateOfTemporaryImport;

		public ZDate EndDateOfTemporaryImport
		{
			get { return fEndDateOfTemporaryImport; }
			set { fEndDateOfTemporaryImport = value; }
		}
		ZDate fEndDateOfTemporaryImport;

		public ZString VesselType
		{
			get { return fVesselType; }
			set { fVesselType = value; }
		}
		ZString fVesselType;

		public ZString AirCraftRegNumber
		{
			get { return fAirCraftRegNumber; }
			set { fAirCraftRegNumber = value; }
		}
		ZString fAirCraftRegNumber;

		public ZString VoyageNumber
		{
			get { return fVoyageNumber; }
			set { fVoyageNumber = value; }
		}
		ZString fVoyageNumber;

		public ZString VesselIdentification
		{
			get { return fVesselIdentification; }
			set { fVesselIdentification = value; }
		}
		ZString fVesselIdentification;

		public ZString ConsigneeAddress
		{
			get { return fConsigneeAddress; }
			set { fConsigneeAddress = value; }
		}
		ZString fConsigneeAddress;

		public ZBool GoodsImportedUnderMESorBWS
		{
			get { return fGoodsImportedUnderMESorBWS; }
			set { fGoodsImportedUnderMESorBWS = value; }
		}
		ZBool fGoodsImportedUnderMESorBWS;

		#endregion
	}

	public class IINPUPDTestClass : INPDECTestClass, IINPUPD
	{
		#region IINPUPD Members

		public IINPUPDTestClass()
		{
		}

		public ZString ValidityExtForTemp
		{
			get { return fValidityExtForTemp; }
			set { fValidityExtForTemp = value; }
		}
		ZString fValidityExtForTemp;

		#endregion
	}
}
