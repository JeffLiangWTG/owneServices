using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ACIZoneInformationImporter
	{
		public ACIZoneInformationImporter(Form parentForm)
		{
			this.ParentForm = parentForm;
		}

		readonly Form ParentForm;

		public void PromptUserAndImport()
		{
			var dialog = new ZOpenFileDialog();
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				Import(dialog.OpenFile, dialog.UnmappedFileName);
			}
		}

		internal void Import(Func<Stream> openStream, string fileNameForDisplay)
		{
			try
			{
				using (Stream stream = openStream())
				{
					Import(stream, fileNameForDisplay);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(Res.GetString("7e790d56-a32a-477e-9ae9-194c9affc95e", "The file {0} could not be opened due to the error - {1}", fileNameForDisplay, ex.Message));
				return;
			}
		}

		void Import(Stream stream, string fileNameForDisplay)
		{
			bool incorrectCharacterFound = false;
			bool isFileImported = true;
			StreamReader reader = new StreamReader(stream);
			int totalLineCount = 0;
			while (!reader.EndOfStream)
			{
				ZString line = reader.ReadLine();
				if (!incorrectCharacterFound && !line.IsWesternEuropeanOrEmpty)
				{
					if (Globals.Message.Show(Res.GetString("95d1c3f9-6ff2-4eed-842f-b00a522a52fb", "Incorrect character detected in file {0} on line {1}.\r\nAnswer 'Yes' if you want the system to automatically remove this and all following incorrect characters during import.\r\nAnswer 'No' to stop import process.", fileNameForDisplay, totalLineCount + 1),
						Res.GetString("44f0a3b5-042d-4da1-95c1-c7e15d2579dc", "Incorrect Character Detected"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.No)
					{
						return;
					}

					incorrectCharacterFound = true;
				}

				totalLineCount++;
			}

			reader.BaseStream.Position = 0;
			int lineCount = 0;

			using (ProgressForm progressForm = new ProgressForm())
			{
				bool aciImportCancelled = false;
				progressForm.ShowCancelButton = true;
				progressForm.Cancelled += delegate
				{ aciImportCancelled = true; };
				ZFormModaliser.Show(progressForm, ParentForm);

				BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider();
				factoryProvider.Current.RefreshEnabled = false;

				while (!reader.EndOfStream && !aciImportCancelled)
				{
					lineCount++;

					progressForm.Status = Res.GetString("e1304513-d10b-4342-974a-8b4e709576d7", "Imported/Updated {0} of {1} records.", lineCount, totalLineCount);
					progressForm.PercentComplete = (int)((lineCount / (decimal)totalLineCount) * 100);

					ZString line = reader.ReadLine().Replace("\"", ""); // values are quoted
					if (!line.IsWesternEuropeanOrEmpty)
					{
						line = line.ConvertToWesternEuropeanCharacters();
					}

					string[] values = new OCsvLine(line).FieldValues;

					if (values.Length == 8 || values.Length == 9)
					{
						CreateOrUpdateZoneBusinessObject(factoryProvider.Current, values);
						if (lineCount % 10000 == 0)
						{
							progressForm.Status = Res.GetString("cfe20017-cbca-4999-8aeb-04658f6875b9", "Saving created/updated records...");
							factoryProvider.SaveCurrentAndCreateNew();
						}
					}
					else
					{
						Globals.Message.Show(Res.GetString("0922484F-84BD-4196-92AF-811465526D6D", "The file format is not correct. Import has been canceled. Please review the file and import again."));
						isFileImported = false;
						break;
					}
				}
				factoryProvider.SaveCurrentAndCreateNew();
			}
			if (isFileImported && !incorrectCharacterFound)
			{
				Globals.Message.Show(Res.GetString("B768E61C-557F-407E-B713-5A5D4031FE01",
					"The file has been imported successfully."));
				return;
			}
		}

		#region Implementation

		void CreateOrUpdateZoneBusinessObject(BusinessObjectFactory factory, string[] values)
		{
			string uNLOCOCode = GetUNLOCOFromIATA(factory, values[3].Trim());
			ZQuery query = new ZQuery(RefDomesticCartageZoneSchema.F1_CityTownPostCode, values[2].Trim());
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_RL_NKLoco, uNLOCOCode);
			query.AddToFilter(RefDomesticCartageZoneSchema.F1_CityTown, values[0].Trim());
			RefDomesticCartageZone zone = factory.LoadTop1<RefDomesticCartageZone>(query)
				?? factory.New<RefDomesticCartageZone>();

			zone.F1_CityTown = GetSafeValue(values[0], RefDomesticCartageZoneSchema.F1_CityTown.MaxLength);
			zone.F1_RW_NKState = GetSafeValue(values[1], RefDomesticCartageZoneSchema.F1_RW_NKState.MaxLength);
			zone.F1_CityTownPostCode = GetSafeValue(values[2], RefDomesticCartageZoneSchema.F1_CityTownPostCode.MaxLength);
			zone.F1_PortCode = GetSafeValue(values[3], RefDomesticCartageZoneSchema.F1_PortCode.MaxLength);
			zone.F1_RL_NKLoco = uNLOCOCode;
			zone.F1_AirportCity = GetSafeValue(values[4], RefDomesticCartageZoneSchema.F1_AirportCity.MaxLength);
			zone.F1_AirportPostcode = GetSafeValue(values[5], RefDomesticCartageZoneSchema.F1_AirportPostcode.MaxLength);
			zone.F1_DataSource = "ACI";

			ZDecimal distance;
			if (ZDecimal.TryParse(values[6].Trim(), out distance))
			{
				zone.F1_Distance = distance;
				zone.F1_DistanceUQ = "MI"; // Miles
			}

			zone.F1_Zone = GetSafeValue(values[7], RefDomesticCartageZoneSchema.F1_Zone.MaxLength);

			if (values.Length == 9)
			{
				if (values[8] == "1" || values[8].ToUpper() == "YES")
				{
					zone.F1_IsBeyond = true;
				}
			}
		}

		ZString GetUNLOCOFromIATA(BusinessObjectFactory factory, string iATA)
		{
			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_IATA, iATA);
			query.OrderBy = RefUNLOCOSchema.RL_Code.Name + " ASC";
			RefUNLOCO loco = factory.LoadTop1<RefUNLOCO>(query);
			return loco != null ? loco.RL_Code : ZString.Empty;
		}

		string GetSafeValue(string input, int maxLength)
		{
			string result = input.Trim();
			if (result.Length > maxLength)
			{
				result = result.Substring(0, maxLength);
			}
			return result;
		}

		#endregion
	}
}
