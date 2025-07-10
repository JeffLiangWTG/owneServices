using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ATF))]
	public class ATFTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ATF>
	{
		public void TestIsFirearms()
		{
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.I;
			Assert(ATF.IsFirearms);
		}

		public void TestIsImplementsOfWar()
		{
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.II;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.IV;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.VI;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.VII;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.VIII;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.XIV;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.XVI;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.XX;
			Assert(ATF.IsImplementsOfWar);
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.XXI;
			Assert(ATF.IsImplementsOfWar);
		}

		public void TestIsAmmunition()
		{
			ATF.US_MunitionsListCategory = MunitionsCategoryList.Codes.III;
			Assert(ATF.IsAmmunition);
		}

		public void TestPGALineReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_FDAADTA = ZDateTime.BrettsBirthday;
			declaration.US_SchDEntry = "1101";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			var atf = invoiceLine.ATFLines.AddNew();

			atf.US_CategoryCode = ATFCategoryCodeList.Codes.ESP;
			atf.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeUpdated;
			Factory.Save();
			atf.OnLoaded();
			Assert(!atf.ReadOnly);

			atf.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			Factory.Save();
			atf.OnLoaded();
			Assert(atf.ReadOnly);

			atf.US_TrackingStatus = ZString.Empty;
			Factory.Save();
			atf.OnLoaded();
			Assert(!atf.ReadOnly);

			atf.US_TrackingStatus = PGATrackingStatusList.Codes.ToBeDeleted;
			Factory.Save();
			atf.OnLoaded();
			Assert(atf.ReadOnly);

			atf.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			Factory.Save();
			atf.OnLoaded();
			Assert(atf.ReadOnly);
		}

		public void TestIATFLinesMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_FDAADTA = ZDateTime.BrettsBirthday;
			declaration.US_SchDEntry = "1101";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			var iatf = (IATFData)invoiceLine.ATFLines.AddNew();

			AssertEquals(declaration.US_FDAADTA, iatf.ArrivalDate);
			AssertEquals("1101", iatf.ArrivalLocation);
			AssertEquals("CN", iatf.ExportCountry);
		}

		public void TestManufacturerAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			orgHeader.OH_FullName = "HEADER COMPANY";
			var newAddress = orgHeader.Addresses.AddNew();
			newAddress.OA_CompanyNameOverride = "ADDRESS COMPANY";

			ATF.InvoiceLine.JI_OA_ManufacturerAddress = newAddress.PK;
			var manufacturerAddress = ((IATFData)ATF).ManufacturerAddress;
			AssertEquals("ADDRESS COMPANY", manufacturerAddress.CompanyName);

			ATF.InvoiceLine.JI_OA_ManufacturerAddress = orgHeader.MainAddress.PK;
			manufacturerAddress = ((IATFData)ATF).ManufacturerAddress;
			AssertEquals("HEADER COMPANY", manufacturerAddress.CompanyName);

			newAddress.OA_CompanyNameOverride = ZString.Empty;
			ATF.InvoiceLine.JI_OA_ManufacturerAddress = newAddress.PK;
			manufacturerAddress = ((IATFData)ATF).ManufacturerAddress;
			AssertEquals("HEADER COMPANY", manufacturerAddress.CompanyName);
		}

		public void TestClone()
		{
			var invoiceLine = ATF.InvoiceLine;

			var atfN = invoiceLine.ATFLines.AddNew();

			atfN.US_CategoryCode = ATFCategoryCodeList.Codes.ESP;
			atfN.US_Quantity = 5m;
			atfN.US_FFLNumber = "SA444";
			atfN.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._2;
			atfN.US_FELNumber = "12";
			atfN.US_FELExemptionCode = ExemptionCodesCodeList.Codes._2;
			atfN.US_PermitNumber = "TT";
			atfN.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			atfN.US_AECANumber = "12221";
			atfN.US_AECAExemptionCode = ExemptionCodesCodeList.Codes._2;
			atfN.US_Model = "2015";
			atfN.US_CaliberGaugeSize = "12";
			atfN.US_BarrelLength = 5m;
			atfN.US_OverallLength = 12m;
			atfN.US_ExtendedDescription = "DESCRIPTION";

			invoiceLine.Declaration.CopyLastPGADetailsToNewLine = true;
			Factory.Save();

			var atfNew = invoiceLine.ATFLines.AddNew();

			AssertEquals("CategoryCode must be the same", atfN.US_CategoryCode, atfNew.US_CategoryCode);
			AssertEquals("Quantity must be the same", atfN.US_Quantity, atfNew.US_Quantity);
			AssertEquals("FFLNumber must be the same", atfN.US_FFLNumber, atfNew.US_FFLNumber);
			AssertEquals("FFLExemptionCode must be the same", atfN.US_FFLExemptionCode, atfNew.US_FFLExemptionCode);
			AssertEquals("FELNumber must be the same", atfN.US_FELNumber, atfNew.US_FELNumber);
			AssertEquals("FELExemptionCode must be the same", atfN.US_FELExemptionCode, atfNew.US_FELExemptionCode);
			AssertEquals("PermitNumber must be the same", atfN.US_PermitNumber, atfNew.US_PermitNumber);
			AssertEquals("PermitExemptionCode must be the same", atfN.US_PermitExemptionCode, atfNew.US_PermitExemptionCode);
			AssertEquals("AECANumber must be the same", atfN.US_AECANumber, atfNew.US_AECANumber);
			AssertEquals("AECAExemptionCode must be the same", atfN.US_AECAExemptionCode, atfNew.US_AECAExemptionCode);
			AssertEquals("Model must be the same", atfN.US_Model, atfNew.US_Model);
			AssertEquals("CaliberGaugeSize must be the same", atfN.US_CaliberGaugeSize, atfNew.US_CaliberGaugeSize);
			AssertEquals("BarrelLength must be the same", atfN.US_BarrelLength, atfNew.US_BarrelLength);
			AssertEquals("OverallLength must be the same", atfN.US_OverallLength, atfNew.US_OverallLength);
			AssertEquals("ExtendedDescription must be the same", atfN.US_ExtendedDescription, atfNew.US_ExtendedDescription);
		}

		public void TestIAESATFMembers()
		{
			ATF.US_FFLNumber = "FFFFFFFFFF";
			ATF.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._1;
			ATF.US_PermitNumber = "PPPPPPPPPP";
			ATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			ATF.US_Quantity = 1000000m;
			ATF.US_CategoryCode = ATFCategoryCodeList.Codes.AW;
			ATF.InvoiceLine.JI_Description = "THIS IS EXPORT ATF, IT HAS A LONG DESCRIPTION";

			var aesATF = (IAESATF)ATF;
			AssertEquals("FFFFFFFFFF", aesATF.FFLNumber);
			AssertEquals(ExemptionCodesCodeList.Codes._1, aesATF.FFLExemptionCode);
			AssertEquals("PPPPPPPPPP", aesATF.PermitNumber);
			AssertEquals(ExemptionCodesCodeList.Codes._2, aesATF.PermitExemptionCode);
			AssertEquals(1000000m, aesATF.Quantity);
			AssertEquals(ATFCategoryCodeList.Codes.AW, aesATF.CategoryCode);
			AssertEquals("THIS IS EXPORT ATF, IT HAS A", aesATF.Description);
		}

		public void TestExportATF()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var atf = invoiceLine.ATFLines.AddNew();
			AssertEquals(false, atf.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, atf.IsExport);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(false, productATF.IsExport);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(true, productATF.IsExport);
		}

		public void TestUS_PermitNumberMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(AutoUSATFAddInfo.Schema.US_PermitNumberMaxLength, productATF.US_PermitNumberInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(11, productATF.US_PermitNumberInfo.MaxLength);
		}

		public void TestUS_PermitExemptionCodeMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(AutoUSATFAddInfo.Schema.US_PermitExemptionCodeMaxLength, productATF.US_PermitExemptionCodeInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(1, productATF.US_PermitExemptionCodeInfo.MaxLength);
		}

		public void TestUS_FFLNumberMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(AutoUSATFAddInfo.Schema.US_FFLNumberMaxLength, productATF.US_FFLNumberInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(20, productATF.US_FFLNumberInfo.MaxLength);
		}

		public void TestUS_FFLExemptionCodeMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(AutoUSATFAddInfo.Schema.US_FFLExemptionCodeMaxLength, productATF.US_FFLExemptionCodeInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(1, productATF.US_FFLExemptionCodeInfo.MaxLength);
		}

		public void TestUS_CategoryCodeMaxLength()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(AutoUSATFAddInfo.Schema.US_CategoryCodeMaxLength, productATF.US_CategoryCodeInfo.MaxLength);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(4, productATF.US_CategoryCodeInfo.MaxLength);
		}

		public void TestUS_QuantityDecimalPlaces()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var productATF = pivot.ATFLines.AddNew();
			AssertEquals(2, MetaData.GetDecimalPlaces(productATF, productATF.US_QuantityInfo.PropertyDescriptor));
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(0, MetaData.GetDecimalPlaces(productATF, productATF.US_QuantityInfo.PropertyDescriptor));
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return ATF;
		}
		#endregion

		#region Implementation

		protected override IEnumerable<ATF> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			yield return dec.Invoices.AddNew().InvoiceLines.AddNew().ATFLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dec = factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			return dec.Invoices.AddNew().InvoiceLines.AddNew().ATFLines.AddNew();
		}

		ATF ATF
		{
			get
			{
				if (atf == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					atf = invoiceLine.ATFLines.AddNew();
				}
				return atf;
			}
		}
		ATF atf;
		#endregion
	}
}
