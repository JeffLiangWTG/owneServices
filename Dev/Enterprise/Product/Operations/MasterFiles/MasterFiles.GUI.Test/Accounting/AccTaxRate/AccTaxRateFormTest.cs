using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccTaxRateForm))]
	sealed class AccTaxRateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			return new AccTaxRateForm(taxRate);
		}

		public void TestExtraRateVisibility()
		{
			AccTaxRate nonGstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate gstAndQst = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			using (AccTaxRateForm form = new AccTaxRateForm(nonGstAndQst))
			{
				form.Show();
				AssertEquals("Non-GSTANDQST: edit control shouldn't be visible", false, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("Non-GSTANDQST: type control shouldn't be visible", false, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			using (AccTaxRateForm form = new AccTaxRateForm(gstAndQst))
			{
				form.Show();
				AssertEquals("GSTANDQST: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("GSTANDQST: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate gstAndEdu = Factory.NewWithValidTestData<AccTaxRate>();
			gstAndEdu.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			using (AccTaxRateForm form = new AccTaxRateForm(gstAndEdu))
			{
				form.Show();
				AssertEquals("GSTANDEDU: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("GSTANDEDU: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate inp7 = Factory.NewWithValidTestData<AccTaxRate>();
			inp7.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATClaimed;

			using (AccTaxRateForm form = new AccTaxRateForm(inp7))
			{
				form.Show();
				AssertEquals("ChinaInputVATClaimed: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("ChinaInputVATClaimed: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate oto6 = Factory.NewWithValidTestData<AccTaxRate>();
			oto6.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ChinaInputVATOffsetAgainstOutputTax;

			using (AccTaxRateForm form = new AccTaxRateForm(oto6))
			{
				form.Show();
				AssertEquals("ChinaInputVATOffsetAgainstOutputTax: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("ChinaInputVATOffsetAgainstOutputTax: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate ret = Factory.NewWithValidTestData<AccTaxRate>();
			ret.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			using (AccTaxRateForm form = new AccTaxRateForm(ret))
			{
				form.Show();
				AssertEquals("RET VAT Withholding: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("RET VAT Withholding: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate qct = Factory.NewWithValidTestData<AccTaxRate>();
			qct.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			using (AccTaxRateForm form = new AccTaxRateForm(qct))
			{
				form.Show();
				AssertEquals("QCT: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("QCT: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate igic = Factory.NewWithValidTestData<AccTaxRate>();
			igic.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.RegionalTax;

			using (AccTaxRateForm form = new AccTaxRateForm(igic))
			{
				form.Show();
				AssertEquals("IGIC: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("IGIC: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			AccTaxRate spv = Factory.NewWithValidTestData<AccTaxRate>();
			spv.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;

			using (AccTaxRateForm form = new AccTaxRateForm(spv))
			{
				form.Show();
				AssertEquals("Split Payment VAT: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("Split Payment VAT: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AccTaxRate serviceTax = Factory.NewWithValidTestData<AccTaxRate>();
				serviceTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.ServiceTax;

				using (AccTaxRateForm form = new AccTaxRateForm(serviceTax))
				{
					form.Show();
					AssertEquals("Service Tax VAT: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
					AssertEquals("Service Tax VAT: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
				}
			}

			AccTaxRate stateTax = Factory.NewWithValidTestData<AccTaxRate>();
			stateTax.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.StateGST;

			using (AccTaxRateForm form = new AccTaxRateForm(stateTax))
			{
				form.Show();
				AssertEquals("State Tax VAT: edit control should be visible", true, form.AT_ExtraRateBoundCalcEdit.Visible);
				AssertEquals("State Tax VAT: type control should be visible", true, form.AuxiliaryTaxTypeDropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestGUIFieldsReadOnlyDependsOnSupportUser()
		{
			var taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			using (Env.SetTemporaryUserContext("CWPostMaster", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Current user is not Support User", false, GlbStaff.CurrentUser.IsSupportUser);

				using (AccTaxRateForm form = new AccTaxRateForm(taxRate))
				{
					form.Show();
					AssertEquals("AT_RateBoundCalcEdit should be read only", true, form.AT_RateBoundCalcEdit.ReadOnly);
					AssertEquals("AT_ExtraRateBoundCalcEdit should be read only", true, form.AT_ExtraRateBoundCalcEdit.ReadOnly);
					AssertEquals("PostingGroupEdit should be read only", true, form.PostingGroupEdit.ReadOnly);
				}
			}

			using (Env.SetTemporaryUserContext("CWSupport", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Current user is Support User", true, GlbStaff.CurrentUser.IsSupportUser);

				using (AccTaxRateForm form = new AccTaxRateForm(taxRate))
				{
					form.Show();
					AssertEquals("AT_RateBoundCalcEdit should not be read only", true, form.AT_RateBoundCalcEdit.ReadOnly);
					AssertEquals("AT_ExtraRateBoundCalcEdit should be read only", true, form.AT_ExtraRateBoundCalcEdit.ReadOnly);
					AssertEquals("PostingGroupEdit should not be read only", false, form.PostingGroupEdit.ReadOnly);
				}
			}
		}

		public void TestDeactivateTaxIdVerb()
		{
			AccTaxRate rate = Factory.NewWithValidTestData<AccTaxRate>();
			using (AccTaxRateForm form = new AccTaxRateForm(rate))
			{
				form.DisplayMode = ODisplayMode.Delete;
				AssertEquals(form.FormVerb, "Deactivate");
			}
		}

		[RequiresSTA]
		public void TestRefGroupBoxVisibility()
		{
			//Old rate
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = "AU";
			AssertControlVisibility(taxRate);

			//No data in RefDB
			taxRate.AT_ReferenceRateType = "STD";
			AssertControlVisibility(taxRate, isRefDataEmptyLabelVisible: true);

			//Ref DB data for reference rate only
			SetupRateInRefDB("STD");
			taxRate.AT_ReferenceRateType = "STD";
			Factory.Save();
			var newFactory1 = new BusinessObjectFactory();
			var taxRate1 = newFactory1.Load<AccTaxRate>(taxRate.PK);
			AssertControlVisibility(taxRate1, true);

			//Ref DB data for both, reference and extra reference rate
			SetupRateInRefDB("MID");
			taxRate1.AT_ReferenceRateType = "STD";
			taxRate1.AT_ReferenceExtraRateType = "MID";
			newFactory1.Save();
			var newFactory2 = new BusinessObjectFactory();
			var taxRate2 = newFactory2.Load<AccTaxRate>(taxRate.PK);
			AssertControlVisibility(taxRate2, true, true);

			void AssertControlVisibility(AccTaxRate rate, bool isRefTaxRateGroupBoxVisible = false, bool isRefExtraTaxRateGroupBox = false, bool isRefDataEmptyLabelVisible = false)
			{
				using (var form = new AccTaxRateForm(rate))
				{
					form.Show();
					AssertEquals("Test TaxRateGroupBox's visibility", isRefTaxRateGroupBoxVisible, form.RefTaxRateGroupBox.Visible);
					AssertEquals("Test ExtraTaxRateGroupBox's visibility", isRefExtraTaxRateGroupBox, form.RefExtraTaxRateGroupBox.Visible);
					AssertEquals("Test RefDataEmptyLabel's visibility", isRefDataEmptyLabelVisible, form.RefDataEmptyLabel.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestSupportOnlyColumnVisibility()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = "AU";
			SetupRateInRefDB("STD");
			SetupRateInRefDB("MID");
			taxRate.AT_ReferenceRateType = "STD";
			taxRate.AT_ReferenceExtraRateType = "MID";

			var user = Factory.New<GlbStaff>();
			user.GS_Code = "TST";
			user.GS_LoginName = "tst";
			user.GS_FullName = "Test User";
			user.GS_IsDeveloper = false;
			Factory.Save();

			AssertControlVisibility(true);

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertControlVisibility(false);
			}

			AssertControlVisibility(true);

			void AssertControlVisibility(bool isColumnVisible)
			{
				using (var form = new AccTaxRateForm(taxRate))
				{
					form.Show();
					var grid = form.Controls.Find("TaxRateGrid", true).FirstOrDefault();
					CheckVisibility();

					grid = form.Controls.Find("ExtraTaxRateGrid", true).FirstOrDefault();
					CheckVisibility();

					void CheckVisibility()
					{
						AssertNotNull(grid);
						var referenceColumn = (grid as ZGrid).Columns.FirstOrDefault(x => x.ColumnName == "ZAT_ReferenceRateType");
						if (isColumnVisible)
						{
							AssertNotNull(referenceColumn);
							AssertEquals("Test TaxRateGrid column's visibility", true, referenceColumn.IsVisible);
						}
						else
						{
							AssertNull(referenceColumn);
						}
					}
				}
			}
		}

		public void TestFormSize()
		{
			var originalClientHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 395, true).Height;
			var refDataEmptyLabelHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 25, true).Height;
			var rateGroupBoxHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 100, true).Height;
			var extraTaxRateGroupBoxHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 100, true).Height;

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			using (var form = new AccTaxRateForm(taxRate))
			{
				form.Show();
				var labelControl = form.Controls["RefDataEmptyLabel"];
				Assert("Postcondition: labelControl.Visible", !labelControl.Visible);
				var expectedHeight = originalClientHeight - refDataEmptyLabelHeight - rateGroupBoxHeight - extraTaxRateGroupBoxHeight;
				AssertEquals("Old code", expectedHeight, form.ClientSize.Height);
			}

			taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.AT_ReferenceRateType = "STD";
			Factory.Save();
			using (var form = new AccTaxRateForm(taxRate))
			{
				form.Show();
				var labelControl = form.Controls["RefDataEmptyLabel"];
				Assert("Postcondition: labelControl.Visible", labelControl.Visible);
				var expectedHeight = originalClientHeight - refDataEmptyLabelHeight;
				AssertEquals("System code without ref date", expectedHeight, form.ClientSize.Height);
			}

			SetupRateInRefDB("STD");
			SetupRateInRefDB("MID");
			taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.AT_ReferenceRateType = "STD";
			Factory.Save();
			using (var form = new AccTaxRateForm(taxRate))
			{
				form.Show();
				var labelControl = form.Controls["RefDataEmptyLabel"];
				Assert("Postcondition: labelControl.Visible", !labelControl.Visible);
				var expectedHeight = originalClientHeight - refDataEmptyLabelHeight - extraTaxRateGroupBoxHeight;
				AssertEquals("System code with ref date", expectedHeight, form.ClientSize.Height);
			}

			taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.AT_ReferenceRateType = "STD";
			taxRate.AT_ReferenceExtraRateType = "MID";
			Factory.Save();
			using (var form = new AccTaxRateForm(taxRate))
			{
				form.Show();
				var labelControl = form.Controls["RefDataEmptyLabel"];
				Assert("Postcondition: labelControl.Visible", !labelControl.Visible);
				var expectedHeight = originalClientHeight - refDataEmptyLabelHeight;
				AssertEquals("System code with extra rate", expectedHeight, form.ClientSize.Height);
			}
		}

		public void TestRefDataEmptyLabelPosition()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_ReferenceRateType = "STD";
			using (var form = new AccTaxRateForm(taxRate))
			{
				form.Show();
				var labelControl = form.Controls["RefDataEmptyLabel"];
				Assert("Postcondition: labelControl.Visible", labelControl.Visible);
				var taxMessageControl = form.Controls["AT_A9_DefaultVatClassBoundGuidFindBox"];
				var buttonsControl = form.Controls["ButtonsUserControl"];
				AssertGreaterThan(labelControl.Location.Y, taxMessageControl.Location.Y);
				AssertLessThanOrEqualTo(labelControl.Location.Y + labelControl.Height, buttonsControl.Location.Y);
				AssertGreaterThanOrEqualTo(labelControl.Location.X, form.ClientRectangle.X);
				AssertLessThanOrEqualTo(labelControl.Right, form.ClientRectangle.Right);
			}
		}

		void SetupRateInRefDB(string referenceRateType)
		{
			var refTaxRate = Factory.NewWithValidTestData<RefAccTaxRate>();
			refTaxRate.ZAT_ReferenceRateType = referenceRateType;
			refTaxRate.ZAT_StartDate = ZDateTime.Now.AddYears(-1).Date;
			refTaxRate.ZAT_EndDate = ZDateTime.Now.AddMonths(2).Date;
			refTaxRate.ZAT_RateNumerator = 12;
			refTaxRate.ZAT_RateDenominator = 1;
			refTaxRate.ZAT_RN_NKCountry = "AU";
			Factory.Save();
		}
	}
}
