using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortManifestPort))]
	sealed class PortManifestPortTest : RegistryBusinessObjectTemplateTestCase<PortManifestPort>
	{
		public void TestValidatePort()
		{
			Port1.Port = "";
			AssertHasError("Empty Port", Port1.PortInfo, "Please enter a Port.");
			Port1.Port = "XXX";
			AssertHasError("Invalid Port", Port1.PortInfo, "Enter a valid Port.");
			Port1.Port = "NZLYT";
			AssertNoErrors("Valid Port", Port1.PortInfo);
		}

		public void TestUniquePortAndPrincipal()
		{
			var errorMessage = "The combination of Port and Principal may only appear once in this list.";

			var principal1 = ZGuid.NewZGuid();
			var principal2 = ZGuid.NewZGuid();

			Port1.Port = "NZLYT";
			Port1.PrincipalPK = principal1;
			Port2.Port = "NZLYT";
			Port2.PrincipalPK = principal2;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid Port", Port1.PortInfo);
			AssertNoErrors("Valid Principal", Port1.PrincipalPKInfo);
			AssertNoErrors("Valid Port", Port2.PortInfo);
			AssertNoErrors("Valid Principal", Port2.PrincipalPKInfo);

			Port2.PrincipalPK = principal1;
			Collection.RunPreSaveValidation();
			AssertHasError("Duplicate Port And Principal", Port1.PortInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port1.PrincipalPKInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port2.PortInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port2.PrincipalPKInfo, errorMessage);

			Port1.Port = "NZAKL";
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid Port", Port1.PortInfo);
			AssertNoErrors("Valid Principal", Port1.PrincipalPKInfo);
			AssertNoErrors("Valid Port", Port2.PortInfo);
			AssertNoErrors("Valid Principal", Port2.PrincipalPKInfo);

			Port2.Port = "NZAKL";
			Collection.RunPreSaveValidation();
			AssertHasError("Duplicate Port And Principal", Port1.PortInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port1.PrincipalPKInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port2.PortInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port2.PrincipalPKInfo, errorMessage);

			Port1.PrincipalPK = ZGuid.Empty;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid Port", Port1.PortInfo);
			AssertNoErrors("Valid Principal", Port1.PrincipalPKInfo);
			AssertNoErrors("Valid Port", Port2.PortInfo);
			AssertNoErrors("Valid Principal", Port2.PrincipalPKInfo);

			Port2.PrincipalPK = ZGuid.Empty;
			Collection.RunPreSaveValidation();
			AssertHasError("Duplicate Port And Principal", Port1.PortInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port1.PrincipalPKInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port2.PortInfo, errorMessage);
			AssertHasError("Duplicate Port And Principal", Port2.PrincipalPKInfo, errorMessage);
		}

		public void TestValidateSenderID()
		{
			Port1.Enabled = true;
			Port1.SenderID = "";
			AssertHasError("Empty Port", Port1.SenderIDInfo, "Please enter a Sender ID.");
			Port1.SenderID = "SenderID_123";
			AssertNoErrors("Valid Port", Port1.SenderIDInfo);

			Port1.Enabled = false;
			Port1.SenderID = "";
			AssertNoErrors("Valid Port", Port1.SenderIDInfo);
			Port1.SenderID = "SenderID_123";
			AssertNoErrors("Valid Port", Port1.SenderIDInfo);
		}

		public void TestDefaultValue()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UPS"))
			{
				var portManifestPort = new PortManifestPort();

				AssertEquals("UPS", portManifestPort.SenderID);
			}
		}

		#region Implementation
		PortManifestPortCollection Collection
		{
			get
			{
				return collection ?? (collection = new PortManifestPortCollection());
			}
		}
		PortManifestPortCollection collection;

		PortManifestPort Port1
		{
			get
			{
				if (port1 == null)
				{
					port1 = Collection.AddNew();
					port1.CurrentFallbackLevel = new FallbackLevel(CompanyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return port1;
			}
		}
		PortManifestPort port1;

		PortManifestPort Port2
		{
			get
			{
				if (port2 == null)
				{
					port2 = Collection.AddNew();
					port2.CurrentFallbackLevel = new FallbackLevel(companyNZ.PK.ToGuid(), Guid.Empty, Guid.Empty);
				}
				return port2;
			}
		}
		PortManifestPort port2;

		GlbCompany CompanyNZ
		{
			get { return companyNZ ?? (companyNZ = GetTestCompany("NZAKL")); }
		}
		GlbCompany companyNZ;

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortManifestPort GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PortManifestPort GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(PortManifestPort originalBusinessObject, PortManifestPort newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("Port", originalBusinessObject.Port, newBusinessObject.Port);
			AssertEquals("Principal", originalBusinessObject.PrincipalPK, newBusinessObject.PrincipalPK);
			AssertEquals("SenderID", originalBusinessObject.SenderID, newBusinessObject.SenderID);
			AssertEquals("Enabled", originalBusinessObject.Enabled, newBusinessObject.Enabled);
		}

		PortManifestPort GetNewPopulatedBusinessObject()
		{
			PortManifestPort port = new PortManifestPort();
			port.Port = "NZLYT";
			port.PrincipalPK = ZGuid.Empty;
			port.SenderID = "SenderID_123";
			port.Enabled = true;
			return port;
		}

		GlbCompany GetTestCompany(string homePort)
		{
			var countryCode = homePort.Substring(0, 2);
			var orgProxy = CreateOrgHeader(countryCode + "PROXY", true, true, homePort);
			var company = CreateNewCompany("C" + countryCode, countryCode, orgProxy);
			company.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;

			var branche = company.Branches.AddNew();
			branche.GB_Code = "B" + countryCode;
			branche.GB_OH_OrgProxy = orgProxy.PK;
			branche.GB_RL_NKHomePort = homePort;

			Factory.Save();

			return company;
		}

		public OrgHeader CreateOrgHeader(string code, bool creditor, bool debtor, string closestPort = null, bool enableDataRefreshBus = true)
		{
			var header = Factory.New<OrgHeader>();
			header.OH_Code = "Z" + code;
			header.OH_FullName = "Test Company Name";
			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			if (!string.IsNullOrEmpty(closestPort))
			{
				header.OH_RL_NKClosestPort = closestPort;
			}
			header.OH_IsDebtor = debtor;
			header.OH_IsCreditor = creditor;
			if (debtor)
			{
				header.CompanyData.SetARTaxApplicable(true);
				header.MiscServ.OM_ARWHTApplicable = true;
			}
			if (creditor)
			{
				header.CompanyData.SetAPTaxApplicable(true);
				header.MiscServ.OM_APWHTApplicable = true;
			}

			Factory.Save();

			return Factory.Load<OrgHeader>(header.PK);
		}

		public GlbCompany CreateNewCompany(ZString companyCode, ZString countryCode, OrgHeader orgPorxy = null)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
			if (orgPorxy != null)
			{
				company.GC_OH_OrgProxy = orgPorxy.PK;
			}

			company.Factory.Save();

			return Factory.Load<GlbCompany>(company.PK);
		}

		#endregion
	}
}
