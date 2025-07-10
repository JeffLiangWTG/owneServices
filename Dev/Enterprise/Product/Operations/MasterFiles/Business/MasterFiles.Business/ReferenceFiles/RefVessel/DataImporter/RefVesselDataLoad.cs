using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for RefVesselDataLoad.
	/// </summary>
	public class RefVesselDataLoad : DataLoad
	{
		public void ImportVesselData(string dataLocation)
		{
			ImportData(dataLocation, (NoResString)"Vessel");
		}

		#region Validation

		public override string CSVTemplateHeading
		{
			get { return string.Join("", CsvHeaders); }
		}

		protected virtual string[] CsvHeaders
		{
			get { return new[] { "NAME,", "COUNTRYOFREGO,", "LLOYDSID" }; }
		}

		protected override bool IsFileHeaderValid(OCsvLine line)
		{
			return line.FieldValues.Length == 3
				&& line.FieldValues[0].ToUpper() == "NAME"
				&& line.FieldValues[1].ToUpper() == "COUNTRYOFREGO"
				&& line.FieldValues[2].ToUpper() == "LLOYDSID";
		}
		#endregion

		#region ImportFrom .csv file

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			if (ValidLine(line))
			{
				ExtractExcelVesselData(line);
				UpdateVessel();
			}
			else
			{
				RunCounters.RecsExcluded++;
				if (!EmptyLine(line))
				{
					DisplayLogMessage(Res.GetString("fdb9f59c-b0e7-49ac-baed-235edf6aa972", "File contains inconsistent data. Row {0} contains the following data : {1}", RunCounters.CurrentRow.ToString(), line.ToString()));
				}
				else
				{
					DisplayLogMessage(Res.GetString("8c571bfb-2507-486b-964e-ccf171fef353", "Empty line is ignored and not processed. Row {0}.", RunCounters.CurrentRow.ToString()));
				}
			}
		}

		bool EmptyLine(OCsvLine line)
		{
			return line.FieldValues.Length == 0;
		}

		protected virtual bool ValidLine(OCsvLine line)
		{
			return line.FieldValues.Length == 3
				&& line.FieldValues[1].Length <= 2
				&& line.FieldValues[2].Length <= 7;
		}

		protected virtual void ExtractExcelVesselData(OCsvLine line)
		{
			VesselName = new ZString(line.FieldValues[0]).Trim().SubstringSafe(0, 35);
			VesselNationality = new ZString(line.FieldValues[1]).Trim();
			VesselLloydsNo = new ZString(line.FieldValues[2]).Trim().SubstringSafe(0, 7);
		}

		void UpdateVessel()
		{
			ZQuery filter = new ZQuery(RefVesselSchema.RV_Code, VesselName);
			RefVessel vessel = (RefVessel)Factory.LoadTop1(typeof(RefVessel), filter);
			if (vessel == null)
			{
				RefVessel newVessel = Factory.New<RefVessel>();
				vessel = newVessel;
				RunCounters.RecsCreated++;
			}

			vessel = LoadImportedVesselValues(vessel);
			if (vessel.HasErrors)
			{
				DisplayLogMessage(Res.GetString("57C00080-4183-450C-9505-CEB4BB8B6E9C", "Fix the errors in importing CSV file (Name, Country/Region) are compulsory: ", vessel.RowMessageErrors.ToString()));
				RunCounters.RecsExcluded++;
			}
			else
			{
				RunCounters.RecsUpdated++;
			}

			RunCounters.RecsToUpdate++;
			UpdateAndDisplayIfRequired(vessel.PK.ToGuid(), RefVesselSchema.Constants.TableName);
		}

		protected virtual RefVessel LoadImportedVesselValues(RefVessel vessel)
		{
			vessel.RV_Code = VesselName;
			vessel.RV_LloydsNumber = VesselLloydsNo;
			ZQuery filter = new ZQuery(RefCountrySchema.RN_Code, VesselNationality);
			RefCountry country = (RefCountry)Factory.LoadTop1(typeof(RefCountry), filter);
			vessel.RV_RN_NKCountryOfReg = country != null ? country.Code : ZString.Empty;
			return vessel;
		}

		protected virtual bool AreVesselNameDetailsValid()
		{
			bool returnValue = false;
			if (!VesselName.IsEmpty)
			{
				returnValue = true;
			}

			return returnValue;
		}

		protected virtual bool AreDataLengthValid()
		{
			bool returnValue = false;
			if (VesselName.Length <= RefVesselSchema.RV_Code.MaxLength && VesselLloydsNo.Length <= RefVesselSchema.RV_LloydsNumber.MaxLength)
			{
				returnValue = true;
			}
			return returnValue;
		}

		#endregion

		#region VesselTable

		protected internal ZString VesselName;
		protected internal ZString VesselLloydsNo;
		protected internal ZString VesselNationality;

		#endregion
	}
}
