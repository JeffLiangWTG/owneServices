using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	abstract class DeclarationBasherFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		protected override Customs.Business.BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(Customs.Business.BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			var indexWithinRange = (index + (invoiceIndex * 10)) % Tariffs.Count;
			var quantityFactor = index + 1m;
			var tariff = Tariffs[indexWithinRange];
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.U0_Tariff;
			invoiceLine.JI_RN_NKCountryOfExport = "US";
			invoiceLine.JI_QualifiesForPreferentialDuty = QualifiesForPreferentialDutyList.Codes.NonQualifying;
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLine.JI_LinePrice = quantityFactor * 1.37m;
			if (!invoiceLine.JI_CustomsUnitQty.IsEmpty)
			{
				invoiceLine.JI_CustomsQuantity = quantityFactor * 7.13m;
			}

			if (!invoiceLine.JI_SupplementaryUQ.IsEmpty)
			{
				invoiceLine.JI_SupplementaryQty = quantityFactor * 1.73m;
			}

			return invoiceLine;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			const int MinScreenWidthSupported = 1836;
			const int MinScreenHeightSupported = 1170;
			using (var form = new DeclarationForm(declaration))
			{
				Assert("NZ Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + MinScreenWidthSupported, form.MinimumSize.Width <= MinScreenWidthSupported);
				Assert("NZ Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + MinScreenHeightSupported, form.MinimumSize.Height <= MinScreenHeightSupported);
			}
		}

		NonDependentNZCClassificationCollection Tariffs
		{
			get
			{
				if (tariffs == null)
				{
					//select top 1000 * from nzcclassification where U0_Tariff like '%05.%' and U0_ComputerDumpDescr != '' and U0_Description != ''
					var filter = new ZQuery();
					filter.AddToFilter(NZCClassificationSchema.U0_ComputerDumpDescr, SQLComparisonOperator.NotEqual, "");
					filter.AddToFilter(NZCClassificationSchema.U0_Description, SQLComparisonOperator.NotEqual, "");
					filter.AddToFilter(NZCClassificationSchema.U0_Tariff, SQLComparisonOperator.Contains, "05.");
					filter.MaximumRows = 50;
					tariffs = new NonDependentNZCClassificationCollection(Factory);
					tariffs.Load(filter);
				}

				return tariffs;
			}
		}

		NonDependentNZCClassificationCollection tariffs;
		// NZ Results as of 7-Dec-2007
		//Passed: Create/Show 50 lines - Expected Time (Standard CPU): 15000 ms, Actual Time (Standard CPU): 2838 ms, Your CPU Performance Ratio: 1.8, Actual Time (Your CPU): 1573.6991 ms
		//Passed: Reload/Validate 50 lines - Expected Time (Standard CPU): 3000 ms, Actual Time (Standard CPU): 264 ms, Your CPU Performance Ratio: 1.8, Actual Time (Your CPU): 146.5164 ms
		//Passed: Create/Show 500 lines - Expected Time (Standard CPU): 100000 ms, Actual Time (Standard CPU): 18590 ms, Your CPU Performance Ratio: 1.8, Actual Time (Your CPU): 10319.6844 ms
		//Passed: Reload/Validate 500 lines - Expected Time (Standard CPU): 30000 ms, Actual Time (Standard CPU): 5754 ms, Your CPU Performance Ratio: 1.8, Actual Time (Your CPU): 3189.1492 ms
		//Passed: Create/Show 1000 lines - Expected Time (Standard CPU): 250000 ms, Actual Time (Standard CPU): 47050 ms, Your CPU Performance Ratio: 1.8, Actual Time (Your CPU): 26068.813 ms
		//Passed: Reload/Validate 1000 lines - Expected Time (Standard CPU): 60000 ms, Actual Time (Standard CPU): 11049 ms, Your CPU Performance Ratio: 1.8, Actual Time (Your CPU): 6124.5313 ms
		//Failed: Ratio 50/500 - 2.18 (2)
		//Passed: Ratio 500/1000 - 0.96 (2.5)
		protected override void SetUp()
		{
			base.SetUp();
			CreateNZClassificationIfDoesNotExist("Breeding fowls weighing under 185g", "For breeding", "0105.11.00.01E");
			CreateNZClassificationIfDoesNotExist("Fowls other than for breeding under 185 g", "Other", "0105.11.00.09L");
			CreateNZClassificationIfDoesNotExist("Turkeys for breeding weighing < 185 g", "For breeding", "0105.12.00.01K");
			Factory.Save();
		}

		void CreateNZClassificationIfDoesNotExist(string computerDumpDescr, string description, string tariff)
		{
			var query = new ZQuery(NZCClassificationSchema.U0_ComputerDumpDescr, computerDumpDescr);
			query.AddToFilter(NZCClassificationSchema.U0_Description, description);
			query.AddToFilter(NZCClassificationSchema.U0_Tariff, tariff);
			var classification = Factory.LoadTop1<NZCClassification>(query);
			if (classification == null)
			{
				classification = Factory.New<NZCClassification>();
				classification.U0_ComputerDumpDescr = computerDumpDescr;
				classification.U0_Description = description;
				classification.U0_Tariff = tariff;
				classification.U0_DateActiveFrom = DateTime.Today.AddYears(-1);
			}
		}
	}

	[TestedType(typeof(DeclarationForm))]
	public class DeclarationFormEmptyDeclarationFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			var result = new DeclarationBasherForm(declaration);
			result.ControllerID = ControllerIDs.Customs.JobDeclaration;
			result.Declaration.ApportionmentDirty = false;
			return result;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			const int MinScreenWidthSupported = 1836;
			const int MinScreenHeightSupported = 1170;
			using (var form = GetFormToBashCore())
			{
				Assert("NZ Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + MinScreenWidthSupported, form.MinimumSize.Width <= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported));
				Assert("NZ Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + MinScreenHeightSupported, form.MinimumSize.Height <= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported));
			}
		}
	}

	public class DeclarationBasherForm : DeclarationForm
	{
		public DeclarationBasherForm(JobDeclaration declaration)
			: base(declaration)
		{
			CustomsBrokerageUserControl.LoadInvoiceLinesTabPage();
			var levyCurrencyControl = CustomsBrokerageUserControl.InvoiceLinesTabPage.FindSingleOrDefault<ConvertToLocalCurrencyControl>("LevyCurrencyControl");
			TypeDescriptor.AddAttributes(levyCurrencyControl, new SuppressFormsLocalizedTestAttribute());
		}

		protected override bool ShouldInformVersionDifference => false;
	}
}
