using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	public class NZShipmentDeclarationGeneratorTest : ShipmentDeclarationGeneratorTest
	{
		public override void TestInvoiceGenerator()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			AssertEquals("Invoice generator type", ExpecteInvoiceGeneratorType, (new NZShipmentDeclarationGeneratorForTest()).GetNewInvoiceGenerator(jobDec).GetType());
		}

		protected override IShipmentDeclarationGenerator DeclarationGenerator
		{
			get
			{
				if (fDeclarationGenerator == null)
				{
					fDeclarationGenerator = new NZShipmentDeclarationGenerator();
				}

				return fDeclarationGenerator;
			}
		}

		IShipmentDeclarationGenerator fDeclarationGenerator;

		protected override Type ExpecteInvoiceGeneratorType
		{
			get { return typeof(NZInvoicesGeneratorFromXSD); }
		}

		protected override Type TypeOfDeclaration
		{
			get { return typeof(JobDeclaration); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
		}

		class NZShipmentDeclarationGeneratorForTest : NZShipmentDeclarationGenerator
		{
			public new InvoicesGeneratorFromXSD GetNewInvoiceGenerator(BaseJobDeclaration declaration)
			{
				return base.GetNewInvoiceGenerator(declaration);
			}
		}
	}
}
