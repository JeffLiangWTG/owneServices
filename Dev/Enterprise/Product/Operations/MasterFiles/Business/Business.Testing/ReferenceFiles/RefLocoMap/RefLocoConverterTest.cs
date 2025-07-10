using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RefLocoConverterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestInitialiseConverter()
		{
			RefLocoConverter converter = new RefLocoConverter(Factory);
			AssertEquals(converter.Factory, Factory);
		}

		public void TestGetUNLOCOByPK()
		{
			RefLocoConverter converter = new RefLocoConverter(Factory);
			RefUNLOCO uNLOCO = converter.GetUNLOCO(PK1);
			AssertEquals("AUSYD", uNLOCO.Code);
			AssertEquals("AUSYD", converter.GetUNLOCOString(PK1));

			uNLOCO = converter.GetUNLOCO(PK2);
			AssertEquals("IDJKT", uNLOCO.Code);
			AssertEquals("IDJKT", converter.GetUNLOCOString(PK2));
		}

		public void TestGetUNLOCOByDetails()
		{
			RefLocoConverter converter = new RefLocoConverter(Factory);
			RefUNLOCO uNLOCO = converter.GetUNLOCO("_._", "8888", Country1Guid);
			AssertEquals("AUSYD", uNLOCO.Code);
			AssertEquals("AUSYD", converter.GetUNLOCOString("_._", "8888", Country1Guid));

			uNLOCO = converter.GetUNLOCO("_._", "8989", Country2Guid);
			AssertEquals("IDJKT", uNLOCO.Code);
			AssertEquals("IDJKT", converter.GetUNLOCOString("_._", "8989", Country2Guid));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Country1Guid = new ZGuid(StrCountry1);
			Country2Guid = new ZGuid(StrCountry2);
			InsertRequiredDBRecords();
		}

		protected void InsertRequiredDBRecords()
		{
			// Add the required data if not exist in the database				
			RefLocoMap locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RN = Country1Guid;
			locoMap.RY_SystemUsage = "_._";
			locoMap.RY_LocalPortCode = "8888";
			locoMap.RY_RL_NKLocoPort = UNLOCO1;

			RefLocoMap locoMap2 = Factory.New<RefLocoMap>();
			locoMap2.RY_RN = Country2Guid;
			locoMap2.RY_SystemUsage = "_._";
			locoMap2.RY_LocalPortCode = "8989";
			locoMap2.RY_RL_NKLocoPort = UNLOCO2;

			Factory.Save();

			PK1 = locoMap.PK;
			PK2 = locoMap2.PK;
		}

		protected const string UNLOCO1 = "AUSYD";
		protected const string StrCountry1 = "E4EB6E97-AA78-4C46-BF86-35B7FBB5FB0E";
		protected const string UNLOCO2 = "IDJKT";
		protected const string StrCountry2 = "CBDDBD26-C634-4947-9BEB-8558CE852772";
		protected ZGuid Country1Guid;
		protected ZGuid Country2Guid;
		protected ZGuid PK1;
		protected ZGuid PK2;

		#endregion
	}
}
