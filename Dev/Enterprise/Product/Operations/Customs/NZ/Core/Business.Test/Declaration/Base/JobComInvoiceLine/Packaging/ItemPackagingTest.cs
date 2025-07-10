using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(ItemPackaging))]
	public class ItemPackagingTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<ItemPackaging>
	{
		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return ItemPackaging;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			ItemPackaging.ItemPackages.AddNew();
			return ItemPackaging;
		}

		protected override IEnumerable<ItemPackaging> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			yield return declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ItemPackages.AddNew();
		}

		#endregion

		[ExpectNoExceptions]
		public void TestSetPackagingDefaultQtyFromInvoiceQty()
		{
			InvoiceLine.JI_InvoiceQuantity = 20;
			InvoiceLine.JI_InvoiceUQ = Constants.PkgUnit.Spool;
			var packagingLine1 = InvoiceLine.ItemPackages[0];
			AssertEquals("NZ_NumberOfPackages should default for this UQ", 20, packagingLine1.NZ_NumberOfPackages);
			AssertEquals("NZ_PackageUQ should default for this UQ", "SO", packagingLine1.NZ_PackageUQ);

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 50;
			invoiceLine2.JI_InvoiceUQ = "L";
			var packagingLine2 = invoiceLine2.ItemPackages[0];
			AssertEquals("NZ_NumberOfPackages", 50, packagingLine2.NZ_NumberOfPackages);
			AssertEquals("NZ_PackageUQ should default as Packages where there is no direct conversion - e.g. for Litres UQ", "PK", packagingLine2.NZ_PackageUQ);

			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 144;
			invoiceLine3.JI_InvoiceUQ = Constants.PkgUnit.Mix;
			var packagingLine3 = invoiceLine3.ItemPackages[0];
			AssertEquals("NZ_NumberOfPackages should default for this UQ", 144, packagingLine3.NZ_NumberOfPackages);
			AssertEquals("NZ_PackageUQ should default 'NG' for this invoice UQ", "NG", packagingLine3.NZ_PackageUQ);

			var invoiceLine4 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 10.0m;
			invoiceLine4.JI_InvoiceUQ = Constants.PkgUnit.Spool;
			var packagingLine4 = invoiceLine4.ItemPackages[0];
			AssertEquals("Pre-condition", 10, packagingLine4.NZ_NumberOfPackages);
			invoiceLine4.JI_InvoiceQuantity = (ZDecimal)int.MaxValue + 10.0m;
			AssertEquals("NZ_NumberOfPackages should not change as JI_InvoiceQuantity is oversize", 10, packagingLine4.NZ_NumberOfPackages);
		}

		public void TestSetPackagingDefaultVolumeFromInvoiceVolume()
		{
			InvoiceLine.JI_InvoiceQuantity = 20;
			InvoiceLine.JI_Volume = 200;
			InvoiceLine.JI_VolumeUQ = Core.Constants.Volume.CubicMetres;
			var packagingLine1 = InvoiceLine.ItemPackages[0];
			AssertEquals("NZ_PackageVolume should default to invoice vol for M3", 200m, packagingLine1.NZ_PackageVolume);

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 50;
			invoiceLine2.JI_VolumeUQ = Core.Constants.Volume.Litre;
			invoiceLine2.JI_Volume = 5000;
			var packagingLine2 = invoiceLine2.ItemPackages[0];
			AssertEquals("NZ_PackageVolume should convert litres to MTQ", 5m, packagingLine2.NZ_PackageVolume);

			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 144;
			invoiceLine3.JI_VolumeUQ = Core.Constants.Volume.CubicYards;
			invoiceLine3.JI_Volume = 480;
			var packagingLine3 = invoiceLine3.ItemPackages[0];
			AssertEquals("NZ_PackageVolume should convert from Y3 to MTQ", 366.986m, packagingLine3.NZ_PackageVolume);
		}

		#region Implementation

		ItemPackaging ItemPackaging
		{
			get { return itemPackaging ?? (itemPackaging = InvoiceLine.ItemPackages.AddNew()); }
		}
		ItemPackaging itemPackaging;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoiceHeader;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				}

				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
