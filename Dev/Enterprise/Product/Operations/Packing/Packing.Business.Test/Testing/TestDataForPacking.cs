#if DEBUG

using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	public class TestDataForPacking
	{
		public TestDataForPacking(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			Factory = factory;
		}

		public class BarcodesForPacking
		{
			public BarcodesForPacking()
			{
				Dummy1And2Barcode = "1+2";
				Dummy1And2TUNBarcode = "1+2-TUN";
				Dummy1And2TUNPackType = "BOX";
				Dummy1And2TUNPackQty = 1;

				Dummy3Barcode = "3";
				Dummy3TUNBarcode = "3-TUN";
				Dummy3TUNPackType = "KEG";
				Dummy3TUNPackQty = 30;
			}

			public ZString Dummy1And2Barcode { get; }
			public ZString Dummy1And2TUNBarcode { get; }
			public ZString Dummy1And2TUNPackType { get; }
			public ZDecimal Dummy1And2TUNPackQty { get; }

			public ZString Dummy3Barcode { get; }
			public ZString Dummy3TUNBarcode { get; }
			public ZString Dummy3TUNPackType { get; }
			public ZDecimal Dummy3TUNPackQty { get; }
		}

		public void CreatePackingData()
		{
			if (Dummy == null)
			{
				Dummy = Factory.New<DummyWithPacking>();

				DummyLine1 = Dummy.Lines.AddNew();
				DummyLine1.Code = "P1";
				DummyLine1.Description = "TV";
				DummyLine1.DescriptionSupplement = "Size: 63in";
				DummyLine1.TotalQty = 100;
				DummyLine1.TotalQtyUQ = "UNT";
				DummyLine1.AutoPackQtyPerPackage = 4;
				DummyLine1.AutoPackPackageType = "PLT";

				DummyLine2 = Dummy.Lines.AddNew();
				DummyLine2.Code = "P2";
				DummyLine2.Description = "Amp";
				DummyLine2.TotalQty = 100;
				DummyLine2.TotalQtyUQ = "UNT";
				DummyLine2.AutoPackQtyPerPackage = 10;
				DummyLine2.AutoPackPackageType = "CTN";

				DummyLine3 = Dummy.Lines.AddNew();
				DummyLine3.Code = "P3";
				DummyLine3.Description = "Speakers";
				DummyLine3.DescriptionSupplement = "Sensitivity: 89dB";
				DummyLine3.TotalQty = 100;
				DummyLine3.TotalQtyUQ = "UNT";
				DummyLine3.AutoPackQtyPerPackage = 12;
				DummyLine3.AutoPackPackageType = "BOX";

				// attribs

				DummyLine1.ZD1_Code = "attr1";
				DummyLine1.ZD1_Number = 1;
				DummyLine2.ZD1_Code = "attr2";
				DummyLine2.ZD1_Number = 2;

				PackageJob = PkgPackageJob.LoadOrCreatePackageJob(Dummy);
				PackageJobDocumentSupporter = (PkgPackageJobDocumentSupporter)((IDocumentSupportable)PackageJob).DocumentSupporter;
				Container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

				// barcodes

				Barcodes = new BarcodesForPacking();

				DummyLine1.Barcode = Barcodes.Dummy1And2Barcode;
				DummyLine1.BarcodeTUN = Barcodes.Dummy1And2TUNBarcode;
				DummyLine1.BarcodeTUNPackType = Barcodes.Dummy1And2TUNPackType;
				DummyLine1.BarcodeTUNPackQty = Barcodes.Dummy1And2TUNPackQty;

				DummyLine2.Barcode = Barcodes.Dummy1And2Barcode;
				DummyLine2.BarcodeTUN = Barcodes.Dummy1And2TUNBarcode;
				DummyLine2.BarcodeTUNPackType = Barcodes.Dummy1And2TUNPackType;
				DummyLine2.BarcodeTUNPackQty = Barcodes.Dummy1And2TUNPackQty;

				DummyLine3.Barcode = Barcodes.Dummy3Barcode;
				DummyLine3.BarcodeTUN = Barcodes.Dummy3TUNBarcode;
				DummyLine3.BarcodeTUNPackType = Barcodes.Dummy3TUNPackType;
				DummyLine3.BarcodeTUNPackQty = Barcodes.Dummy3TUNPackQty;

				// undg

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "EXP";
				subs.DG_Variant = "";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var subs2 = Factory.New<UNDGSubstance>();
				subs2.DG_UNNO = "LOS";
				subs2.DG_Variant = "";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

				UndgDataItemEXP = Factory.NewWithValidTestData<UNDGDataItem>();
				UndgDataItemEXP.LinkDefault(subs);

				UndgDataItemLOS = Factory.NewWithValidTestData<UNDGDataItem>();
				UndgDataItemLOS.LinkDefault(subs2);
			}
		}

		#region Data

		// dummy (parent)
		public DummyWithPacking Dummy { get; private set; }

		// dummy lines (packable items)
		public IDummyPackableItemParent DummyLine1 { get; private set; }
		public IDummyPackableItemParent DummyLine2 { get; private set; }
		public IDummyPackableItemParent DummyLine3 { get; private set; }
		public IPackableItem DummyPackableItemOnLine1 => DummyLine1?.PackableItems.Single();

		// dummy lines wrapped
		public PackableItemParentWrapper DummyLine1Wrapper
		{
			get { return PackageJob.PackableItemParents.FindByPackableItemParent(DummyLine1); }
		}
		public PackableItemParentWrapper DummyLine2Wrapper
		{
			get { return PackageJob.PackableItemParents.FindByPackableItemParent(DummyLine2); }
		}
		public PackableItemParentWrapper DummyLine3Wrapper
		{
			get { return PackageJob.PackableItemParents.FindByPackableItemParent(DummyLine3); }
		}

		// package job
		public PkgPackageJob PackageJob { get; private set; }
		public PkgPackageJobDocumentSupporter PackageJobDocumentSupporter { get; private set; }

		// printer
		public PackageLabelAutoPrinter Printer
		{
			get { return (PackageLabelAutoPrinter)typeof(PkgPackageJob).GetProperty("AutoPrinter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(PackageJob, null); }
		}

		// 20 foot container
		public RefContainer Container20GP { get; private set; }

		// barcodes
		public BarcodesForPacking Barcodes { get; private set; }

		// undg
		public UNDGDataItem UndgDataItemEXP { get; private set; }
		public UNDGDataItem UndgDataItemLOS { get; private set; }

		#endregion

		readonly BusinessObjectFactory Factory;
	}
}

#endif
