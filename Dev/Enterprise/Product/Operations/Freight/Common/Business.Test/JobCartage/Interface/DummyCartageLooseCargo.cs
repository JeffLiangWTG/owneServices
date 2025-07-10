using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Common.Business.Testing
{
	public sealed class DummyCartageLooseCargo : NonPersistentBusinessObject, ICartageLooseCargo
	{
		public DummyCartageLooseCargo(BusinessObjectFactory factory) : base(factory) { }

		public DummyCartageLooseCargo(BusinessObjectFactory factory, int bookedPackages) : base(factory) { this.bookedPackages = bookedPackages; }

		public void SetBooked(ZInt packs, ZString packType, ZDecimal weight, ZString wUnit, ZDecimal volume, ZString vUnit)
		{
			bookedPackages = packs;
			bookedPackType = packType;
			bookedWeight = weight;
			bookedWeightUnit = wUnit;
			bookedVolume = volume;
			bookedVolumeUnit = vUnit;
		}

		public void SetDG(UNDGSubstance substance, ZDecimal dgFlashPoint, ZGuid dgContact)
		{
			dgItem = Factory.New<UNDGDataItem>();
			dgItem.DI_DG = substance.PK;
			dgItem.DI_DGFlashPoint = dgFlashPoint;
			dgItem.DI_OC_DGContact = dgContact;
		}

		public void SetDimensions(ZDecimal height, ZDecimal length, ZDecimal width, ZString dUnit)
		{
			bookedHeight = height;
			bookedLength = length;
			bookedWidth = width;
			bookedDimensionUnit = dUnit;
		}

		#region ICartageLooseCargo Members

		ZInt ICartageLooseCargo.BookedPackages
		{
			get { return bookedPackages; }
		}
		ZInt bookedPackages;

		ZString ICartageLooseCargo.BookedPackType
		{
			get { return bookedPackType; }
		}
		ZString bookedPackType;

		ZDecimal ICartageLooseCargo.BookedWeight
		{
			get { return bookedWeight; }
		}
		ZDecimal bookedWeight;

		ZString ICartageLooseCargo.BookedWeightUnit
		{
			get { return bookedWeightUnit; }
		}
		ZString bookedWeightUnit;

		ZDecimal ICartageLooseCargo.BookedVolume
		{
			get { return bookedVolume; }
		}
		ZDecimal bookedVolume;

		ZString ICartageLooseCargo.BookedVolumeUnit
		{
			get { return bookedVolumeUnit; }
		}
		ZString bookedVolumeUnit;

		ZDecimal ICartageLooseCargo.BookedHeight
		{
			get { return bookedHeight; }
		}
		ZDecimal bookedHeight;

		ZDecimal ICartageLooseCargo.BookedWidth
		{
			get { return bookedWidth; }
		}
		ZDecimal bookedWidth;

		ZDecimal ICartageLooseCargo.BookedLength
		{
			get { return bookedLength; }
		}
		ZDecimal bookedLength;

		ZString ICartageLooseCargo.BookedDimensionUnit
		{
			get { return bookedDimensionUnit; }
		}
		ZString bookedDimensionUnit;

		IReadOnlyCollection<UNDGDataItem> ICartageLooseCargo.DangerousGoods
		{
			get { return dgItem != null ? new[] { dgItem } : System.Array.Empty<UNDGDataItem>(); }
		}
		UNDGDataItem dgItem;

		#endregion
	}
}
