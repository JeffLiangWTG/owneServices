using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostBulkCreatorTest : TestCaseWithFactory
	{
		public void TestCreateAndRunLC()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var dec = (BusinessObject)Factory.New<Integration.Customs.AU.IJobDeclaration>();
				dec[JobDeclarationSchema.JE_MessageType] = "IMP";

				var invoice = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceHeader>();
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = dec.PK;
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoice[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

				var invoiceLine = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceLine>();
				invoiceLine[JobComInvoiceLineSchema.Constants.JI_JZ] = invoice.PK;
				invoiceLine[JobComInvoiceLineSchema.Constants.JI_LinePrice] = 1000m;

				var dec2 = (BusinessObject)Factory.New<Integration.Customs.AU.IJobDeclaration>();
				dec2[JobDeclarationSchema.JE_MessageType] = "EXP";

				var invoice2 = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceHeader>();
				invoice2[JobComInvoiceHeaderSchema.Constants.JZ_JE] = dec2.PK;
				invoice2[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
				invoice2[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoice2[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

				var invoiceLine2 = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceLine>();
				invoiceLine2[JobComInvoiceLineSchema.Constants.JI_JZ] = invoice2.PK;
				invoiceLine2[JobComInvoiceLineSchema.Constants.JI_LinePrice] = 1000m;

				var dec3 = (BusinessObject)Factory.New<Integration.Customs.AU.IJobDeclaration>();
				dec3[JobDeclarationSchema.JE_MessageType] = "IMP";

				var invoice3 = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceHeader>();
				invoice3[JobComInvoiceHeaderSchema.Constants.JZ_JE] = dec3.PK;
				invoice3[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
				invoice3[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoice3[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

				Factory.Save();

				IEnumerable<string> jobnumbers = new LandedCostBulkCreator().CreateAndRunLC(new ILandedCostHeader[] { (ILandedCostHeader)dec, (ILandedCostHeader)dec2, (ILandedCostHeader)dec3 });

				Assert(!jobnumbers.Contains<string>((CargoWise.Types.ZString)dec[JobDeclarationSchema.JE_DeclarationReference]));

				Assert("for exp, LC is not supported", jobnumbers.Contains<string>((CargoWise.Types.ZString)dec2[JobDeclarationSchema.JE_DeclarationReference]));

				Assert("no invoice lines exist", jobnumbers.Contains<string>((CargoWise.Types.ZString)dec3[JobDeclarationSchema.JE_DeclarationReference]));

				AssertNotNull("LandedCostHeader for dec is created", Factory.LoadTop1<LandedCostHeader>(new LandedCostHeaderFilter((ILandedCostHeader)dec)));
			}
		}

		public void TestBulkLandedCostingSynchronisesAll()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var pref = importer.LandedCostingPreferences.AddNew();
			pref.O9_DistributeCostBy = "VAV";
			pref.O9_LandedCostGroup = 1;
			pref.O9_LandedCostGroupName = "freight";

			var group = pref.Charges.AddNew();
			group.O0_ChargeGroup = "FRT";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var dec = (BusinessObject)Factory.New<Integration.Customs.AU.IJobDeclaration>();
			dec[JobDeclarationSchema.JE_MessageType] = "IMP";
			dec[JobDeclarationSchema.JE_OH_Importer] = importer.PK;

			var groupInvoice = (BusinessObject)Factory.LoadTop1<Integration.Customs.AU.IJobComInvoiceGroupHeader>(new ZQuery(JobComInvoiceHeaderSchema.JZ_JE, dec.PK));

			var groupInvoiceCharge = (BusinessObject)Factory.New<Integration.Customs.AU.IGroupInvoiceCharge>();
			groupInvoiceCharge[JobComInvHeaderChargeSchema.J7_ParentID] = groupInvoice.PK;
			groupInvoiceCharge[JobComInvHeaderChargeSchema.J7_ParentTableCode] = "JZ";
			groupInvoiceCharge[JobComInvHeaderChargeSchema.J7_ChargeType] = "OFT";
			groupInvoiceCharge[JobComInvHeaderChargeSchema.J7_Amount] = 500m;
			groupInvoiceCharge[JobComInvHeaderChargeSchema.J7_RX_NKCurrency] = "AUD";
			groupInvoiceCharge[JobComInvHeaderChargeSchema.J7_IsIncludedInITOT] = false;

			var invoice = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = dec.PK;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceAmount] = 1000m;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_RX_NKInvoice_Currency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_IncoTerm] = "FOB";

			var invoiceLine = (BusinessObject)Factory.New<Integration.Customs.AU.IJobComInvoiceLine>();
			invoiceLine[JobComInvoiceLineSchema.Constants.JI_JZ] = invoice.PK;
			invoiceLine[JobComInvoiceLineSchema.Constants.JI_LinePrice] = 1000m;

			Factory.Save();

			IEnumerable<string> jobnumbers = new LandedCostBulkCreator().CreateAndRunLC(new ILandedCostHeader[] { (ILandedCostHeader)dec });
			Assert(jobnumbers.Contains<string>((CargoWise.Types.ZString)dec[JobDeclarationSchema.JE_DeclarationReference]));

			var lcHeader = Factory.LoadTop1<LandedCostHeader>(new ZQuery(LandedCostHeaderSchema.LT_ParentID, dec.PK));
			AssertNotNull(lcHeader);
			AssertEquals("LC cost input should have been defaulted", 1, lcHeader.CostInputs.Count);
		}
	}
}
