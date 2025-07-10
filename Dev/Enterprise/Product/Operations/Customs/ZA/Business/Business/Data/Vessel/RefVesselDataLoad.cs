using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public class RefVesselDataLoad : MasterFiles.Business.RefVesselDataLoad
	{
		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length >= 3
				&& (line.FieldValues[0].Equals("IDOFMEANSOFTRANSPORT", StringComparison.OrdinalIgnoreCase)
					|| line.FieldValues[0].Equals("TRANSPORTID", StringComparison.OrdinalIgnoreCase))
				&& line.FieldValues[1].Equals("NAME", StringComparison.OrdinalIgnoreCase)
				&& line.FieldValues[2].Equals("CARRIERCODE", StringComparison.OrdinalIgnoreCase);
			//Data in the last column is ignored as it is not saved in the database
		}

		protected override string[] CsvHeaders
		{
			get { return new[] { "TRANSPORTID,", "NAME,", "CARRIERCODE,", "CARRIERNAME" }; }
		}

		protected override bool ValidLine(OCsvLine line)
		{
			return line.FieldValues.Length >= 3
				&& line.FieldValues[0].Length <= 10
				&& line.FieldValues[1].Length <= 35
				&& line.FieldValues[2].Length <= 4;
		}

		protected override void ExtractExcelVesselData(OCsvLine line)
		{
			Func<int, int, ZString> getFieldAtIndexWithMaxLength = (i, max) =>
			{
				return (line.FieldValues.Length > i)
					? new ZString(line.FieldValues[i]).Trim().SubstringSafe(0, max)
					: ZString.Empty;
			};

			vesselRadioCallSign = getFieldAtIndexWithMaxLength(0, 7);
			VesselName = getFieldAtIndexWithMaxLength(1, 35);
			vesselCarrierCode = getFieldAtIndexWithMaxLength(2, 4);
			vesselCarrierName = getFieldAtIndexWithMaxLength(3, 35);
		}

		protected bool EmptyVesselData
		{
			get { return VesselName.IsEmpty && vesselRadioCallSign.IsEmpty && vesselCarrierCode.IsEmpty && vesselCarrierName.IsEmpty; }
		}

		protected override RefVessel LoadImportedVesselValues(RefVessel vessel)
		{
			vessel.RV_RadioCallSign = vesselRadioCallSign;
			vessel.RV_Code = VesselName;
			vessel.RV_CarrierCode = vesselCarrierCode;
			return vessel;
		}

		protected bool AreVesselDetailsValid()
		{
			return !VesselName.IsEmpty;
		}

		protected string GetErrorMessageIfInvalidVesselDetails()
		{
			return "Vessel Name was empty. Data was Name: " + VesselName;
		}

		protected override bool AreDataLengthValid()
		{
			return (VesselName.Length <= RefVesselSchema.RV_Code.MaxLength && vesselRadioCallSign.Length <= RefVesselSchema.RV_RadioCallSign.MaxLength && vesselCarrierCode.Length <= RefVesselSchema.RV_CarrierCode.MaxLength);
		}

		protected string GetErrorMessageIfInvalidDataLength()
		{
			return "Vessel Name, RadioCallSign or CarrierCode are greater then maximum allowed length. Data was Name: " + VesselName + ", RadioCallSign: " + vesselRadioCallSign + ", CarrierCode: " + vesselCarrierCode;
		}

		#region Variables

		protected ZString vesselRadioCallSign;
		protected ZString vesselCarrierCode;
		protected ZString vesselCarrierName;

		#endregion

	}
}
