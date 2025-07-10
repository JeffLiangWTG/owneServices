using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class NZCClassFindBoxTest : TariffFindBoxTest
	{
		#region TestPuttingInAValidOldCodeGetsADescription
		public void TestPuttingInAValidOldCodeGetsADescription()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2003, 6, 30);
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6115.19.01.00K";
			using (TestFormTakingInvoiceLine form = new TestFormTakingInvoiceLine(invoiceLine))
			{
				form.Show();
				UserIdleWorker.Flush();
				AssertEquals("FindBox Should have a description for 6115.19.01.00K", true, !string.IsNullOrEmpty(((NZCClassFindBox_ForTest)form.FindBox).Description));
			}
		}

		#endregion
		#region TestNZCClassFindBoxListProvider
		public void TestNZCClassFindBoxListProvider()
		{
			using (NZCClassFindBox_ForTest findBox = new NZCClassFindBox_ForTest())
			{
				AssertEquals("FindBox.ListProvider.GetType()", typeof(NZCClassFindBoxListProvider), findBox.ListProvider.GetType());
			}
		}

		#endregion
		#region Implementation
		protected override Type ExpectedFormTypeWhenBorderWiseNotEnabled
		{
			get
			{
				return typeof(NZCClassForm);
			}
		}

		protected override Common.GUI.TariffFindBox GetConcreteFindBox()
		{
			var findBox = new NZCClassFindBox_ForTest();
			return findBox;
		}

		protected override string CorrectBindToListFromInvoiceLine
		{
			get
			{
				return "Lookups.TariffList";
			}
		}

		protected override string IncorrectBindToListFromInvoiceLine
		{
			get
			{
				return "Lookups.ClassificationList";
			}
		}
		#endregion

		class NZCClassFindBox_ForTest : NZCClassFindBox
		{
			public new IFindBoxListProvider ListProvider => base.ListProvider;

			public new string Description => base.Description;
		}
	}
}
