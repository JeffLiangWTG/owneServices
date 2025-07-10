using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TNPAAccountNumber))]
	sealed class TNPAAccountNumberTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidatePort()
		{
			BizObj.RunPreSaveValidation();
			BizObj.Port = "";
			AssertHasErrors(BizObj.PortInfo);
			AssertHasErrorContaining(BizObj.PortInfo, "Port Code is required.");

			BizObj.Port = "XX";
			AssertHasErrors(BizObj.PortInfo);
			AssertHasErrorContaining(BizObj.PortInfo, "Enter a valid selection.");

			BizObj.Port = "AUSYD";
			AssertHasErrors(BizObj.PortInfo);
			AssertHasErrorContaining(BizObj.PortInfo, "Port must be in South Africa.");

			BizObj.Port = "ZACPT";
			AssertNoErrors(BizObj.PortInfo);
		}

		public void TestPortList()
		{
			var unlocoCN = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "CNTST")) ?? Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoCN.RL_RN_NKCountryCode = Constants.CountryCodes.China;
			unlocoCN.RL_Code = "CNTST";

			var unlocoZA = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "ZATST")) ?? Factory.NewWithValidTestData<RefUNLOCO>();
			unlocoZA.RL_RN_NKCountryCode = Constants.CountryCodes.SouthAfrica;
			unlocoZA.RL_Code = "ZATST";

			Factory.Save();

			var postList = new TNPAAccountNumber().PortList;

			CombineAssertions(() =>
			{
				AssertNotNull(postList.FirstOrDefault(port => port.RL_Code == "ZATST"));
				AssertNull(postList.FirstOrDefault(port => port.RL_Code == "CNTST"));

				Assert(postList.All(port => port.RL_Code.StartsWith(Constants.CountryCodes.SouthAfrica)));
			});
		}

		public void TestCheckNumbers()
		{
			BizObj.ImportNumber = "";
			BizObj.ExportNumber = "";
			BizObj.CoastwiseNumber = "";
			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.ImportNumberInfo);
			AssertHasErrors(BizObj.ExportNumberInfo);
			AssertHasErrors(BizObj.CoastwiseNumberInfo);
			AssertHasErrorContaining(BizObj.ImportNumberInfo, "Either Import Number, Export Number or Coastwise Number is required.");
			AssertHasErrorContaining(BizObj.ExportNumberInfo, "Either Import Number, Export Number or Coastwise Number is required.");
			AssertHasErrorContaining(BizObj.CoastwiseNumberInfo, "Either Import Number, Export Number or Coastwise Number is required.");

			BizObj.ImportNumber = "I001";
			BizObj.ExportNumber = "";
			BizObj.CoastwiseNumber = "";
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.ImportNumberInfo);
			AssertNoErrors(BizObj.ExportNumberInfo);
			AssertNoErrors(BizObj.CoastwiseNumberInfo);

			BizObj.ImportNumber = "";
			BizObj.ExportNumber = "E001";
			BizObj.CoastwiseNumber = "";
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.ImportNumberInfo);
			AssertNoErrors(BizObj.ExportNumberInfo);
			AssertNoErrors(BizObj.CoastwiseNumberInfo);

			BizObj.ImportNumber = "";
			BizObj.ExportNumber = "";
			BizObj.CoastwiseNumber = "C001";
			BizObj.RunPreSaveValidation();
			AssertNoErrors(BizObj.ImportNumberInfo);
			AssertNoErrors(BizObj.ExportNumberInfo);
			AssertNoErrors(BizObj.CoastwiseNumberInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new TNPAAccountNumber();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new TNPAAccountNumber BizObj
		{
			get { return (TNPAAccountNumber)base.BizObj; }
		}

		#endregion
	}
}
