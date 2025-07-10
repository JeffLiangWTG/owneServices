using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.ForwardAir
{
	internal class ForwardAirRatesValueObjectDataAdapter : RatingValueObjectDataAdapter<RatingHeader>
	{
		#region Implementation

		protected override bool ShouldCheckifOwnerIsSpecified
		{
			get { return false; }
		}

		protected override RatingHeader FindBusinessObject(Xsd.Rate value, IValueObjectImportContext context)
		{
			return null; // always create new rateheader
		}

		public override RatingHeader CreateOrUpdateFromValueObject(Xsd.Rate value, IValueObjectImportContext context)
		{
			return null; // import - we only import the rateentry for the given rating header, we are not supposed to create or update the rating header here.
		}

		#endregion

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.Costing; }
		}

		protected override void ExportToValueObjectCore(RatingHeader bizObj, Xsd.Rate constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Export is not Supported");
		}
	}
}
