using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(CusTWControllingMessageHeader), "ProductLabelRanges")]
	public class CusTWProductLabelRange : AutoCusTWProductLabelRange
	{
		public CusTWProductLabelRange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(CusTWProductLabelRangeLookups.ProductLabelRangeStatusList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusTWProductLabelRange|TW0_Status", Caption = "Status", FullDescription = "The code for the registration status of the product label.")]
		public override ZString TW0_Status { get => base.TW0_Status; set => base.TW0_Status = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusTWProductLabelRange|TW0_EndNumber", Caption = "End Number", ShortCaption = "End No.", FullDescription = "The ending number of the serial number of product label or the specified code.")]
		public override ZString TW0_EndNumber { get => base.TW0_EndNumber; set => base.TW0_EndNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusTWProductLabelRange|TW0_StartNumber", Caption = "Start Number", ShortCaption = "Start No.", FullDescription = "The starting number of the serial number of product label or the specified code.")]
		public override ZString TW0_StartNumber { get => base.TW0_StartNumber; set => base.TW0_StartNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusTWProductLabelRange|TW0_RunNumber", Caption = "Run Number", ShortCaption = "Run No.", FullDescription = "The track of the product label.")]
		public override ZString TW0_RunNumber { get => base.TW0_RunNumber; set => base.TW0_RunNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusTWProductLabelRange|TW0_Year", Caption = "Year", FullDescription = "The year of the product label.")]
		public override ZString TW0_Year { get => base.TW0_Year; set => base.TW0_Year = value; }

		protected override ZString HumanReadableNameCore => Res.GetString("1b63426e-a754-4ea9-8495-0feb0f198d7f", "Label");
	}
}
