using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Rating;

public class AutoRatingClassesTest : TestCase
{
	public void TestMeasureTypeDescriptions()
	{
		AssertEquals("Total of ALL HTS9902 Lines", MeasureTypeDescriptions.GetDescription(MeasureType.HTS9902Line));
		AssertEquals("Total of ALL HTS9903 Lines", MeasureTypeDescriptions.GetDescription(MeasureType.HTS9903Line));
		AssertEquals("Steel Licenses", MeasureTypeDescriptions.GetDescription(MeasureType.SteelLicenses));
		AssertEquals("SG TPL Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.SG_TPLCertificate));
		AssertEquals("CA NAFTA TPL Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.CA_NAFTA_TPLCertificate));
		AssertEquals("MX NAFTA TPL Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.MX_NAFTA_TPLCertificate));
		AssertEquals("Beef Export Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.BeefExportCertificate));
		AssertEquals("Diamond Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.DiamondCertificate));
		AssertEquals("ATPDEA Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.ATPDEACertificate));
		AssertEquals("AU FTA Export Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.AU_FTA_ExportCertificate));
		AssertEquals("MX Cement License", MeasureTypeDescriptions.GetDescription(MeasureType.MXCementLicense));
		AssertEquals("CAFTA TPL Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.CAFTA_TPLCertificate));
		AssertEquals("Atlantic Lumber Board Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.ALBCertificate));
		AssertEquals("Cotton Shirting Fabric License", MeasureTypeDescriptions.GetDescription(MeasureType.CottonShirtingFabricLicense));
		AssertEquals("Haiti Earned Allowance", MeasureTypeDescriptions.GetDescription(MeasureType.HaitiEarnedAllowance));
		AssertEquals("Agricultural License", MeasureTypeDescriptions.GetDescription(MeasureType.AgriculturalLicense));
		AssertEquals("CA Export Sugar Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.CAExportSugarCertificate));
		AssertEquals("Wool License", MeasureTypeDescriptions.GetDescription(MeasureType.WoolLicense));
		AssertEquals("CBTPA Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.CBTPACertificate));
		AssertEquals("AGOA Textile Provision Number", MeasureTypeDescriptions.GetDescription(MeasureType.AGOATextileProvisionNumber));
		AssertEquals("Other Non-Standard Visa", MeasureTypeDescriptions.GetDescription(MeasureType.OtherNonStandardVisa));
		AssertEquals("USDA Sugar Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.USDASugarCertificate));
		AssertEquals("Organic Product Exemption Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.OrganicProductExemptionCertificate));
		AssertEquals("AMS Certificate Exemption", MeasureTypeDescriptions.GetDescription(MeasureType.AMSCertificateOfExemption));
		AssertEquals("Dominican Republic Earned Allowance Program Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate));
		AssertEquals("Mexican Sugar Export License", MeasureTypeDescriptions.GetDescription(MeasureType.MexicanSugarExportLicense));
		AssertEquals("General Note 15c Waiver Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.GeneralNote15cWaiverCertificate));
		AssertEquals("Aluminum Licenses", MeasureTypeDescriptions.GetDescription(MeasureType.AluminumLicenses));
		AssertEquals("Canadian USMCA TPL Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.CanadianUSMCA_TPLCertificate));
		AssertEquals("Mexican USMCA TPL Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.MexicanUSMCA_TPLCertificate));
		AssertEquals("Argentine White Grape Juice Concentrate Export License", MeasureTypeDescriptions.GetDescription(MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense));
		AssertEquals("KR Export Steel Certificate", MeasureTypeDescriptions.GetDescription(MeasureType.KRExportSteelCertificate));
	}
}
