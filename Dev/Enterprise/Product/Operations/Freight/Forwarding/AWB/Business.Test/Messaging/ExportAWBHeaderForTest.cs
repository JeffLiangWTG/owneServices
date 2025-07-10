using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	sealed class ExportAWBHeaderForTest : ExportAWBHeader
	{
		public ExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new ZString DetailedGoodsDescription { get; set; }

		public override RefCountry DestinationCountry => DestinationCountryForTest;

		public RefCountry DestinationCountryForTest { get; set; }

		public override RefCountry OriginCountry => OriginCountryForTest;
		public RefCountry OriginCountryForTest { get; set; }

		public override StringCollectionX GetAvailableHarmonisedCodes()
		{
			if (AvailableHarmonisedCodes == null)
			{
				AvailableHarmonisedCodes = new StringCollectionX();
			}

			return AvailableHarmonisedCodes;
		}

		internal StringCollectionX AvailableHarmonisedCodes { get; set; }

		public List<EntryNumber> CustomsEntryNumbersExposed { get; set; }

		protected override IEnumerable<EntryNumber> GetCustomsEntryNumbers()
		{
			return CustomsEntryNumbersExposed;
		}

		public List<MovementReferenceNumber> MovementReferenceNumbersExposed { get; set; }

		protected override IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbers()
		{
			return MovementReferenceNumbersExposed;
		}

		public List<GoodsDeclarationReferenceNumber> GoodsDeclarationReferenceNumbersExposed { get; set; }

		protected override IEnumerable<GoodsDeclarationReferenceNumber> GetGoodsDeclarationReferenceNumbers()
		{
			return GoodsDeclarationReferenceNumbersExposed;
		}
	}
}
