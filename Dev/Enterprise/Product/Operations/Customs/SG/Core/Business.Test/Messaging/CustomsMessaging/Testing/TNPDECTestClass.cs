namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	using CargoWise.Types;

	public class TNPDECTestClass : SGCUSDECTestClass, ITNPDEC, ITNPUPD
	{
		public TNPDECTestClass()
			: base()
		{
		}

		#region ITNPDEC Members

		public ZDate StartDateOfCargoRemoval
		{
			get { return fStartDateOfCargoRemoval; }
			set { fStartDateOfCargoRemoval = value; }
		}
		ZDate fStartDateOfCargoRemoval;

		public ZBool Is2bStoredBWCY
		{
			get { return fIs2bStoredBWCY; }
			set { fIs2bStoredBWCY = value; }
		}
		ZBool fIs2bStoredBWCY;

		#endregion
	}

	public class ITNPUPDTestClass : TNPDECTestClass, ITNPUPD
	{
		#region ITNPUPD Members

		public ITNPUPDTestClass()
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
