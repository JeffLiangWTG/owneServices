using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.TR.GUI
{
	public partial class StatementsStampDutyForm : ZTemplateForm
	{
		public StatementsStampDutyForm(CusStatementHeader statementHeader) : base(statementHeader)
		{
			InitializeComponent();
			AddPlugins();

			ZFormMenuStrategy.AddActionsMenuItem(this, new ZMenuItem(ResString.GetMultilingualString("71B958EF-33AB-4C60-8AFF-1EBBB30FB577", "Export R.A. Declaration File (TXT)"), ExportRADeclarationFile_Click));
		}

		public override string FormCaption => Res.GetString("58495E6F-F21C-4163-B779-E792AA8D95D5", "Statements / Stamp Duty");

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		void ExportRADeclarationFile_Click(object sender, EventArgs e)
		{
			var header = DataSource as CusStatementHeader;
			var response = Globals.Message.Show(Res.GetString("780CEC28-066A-4EA4-A6A2-09456FCB1180", "Do you want to create Revenue Administration Stamp Duty Declaration TXT file?"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, DialogResult.No);
			if (header != null && response == DialogResult.Yes)
			{
				using (var dialog = new ZSaveFileDialog())
				{
					dialog.Filter = (NoResString)"Txt Files (*.txt)|*.txt";
					dialog.DefaultExt = (NoResString)"txt";
					dialog.AddExtension = true;
					dialog.FileName = header.B2_StatementNumber;

					if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
					{
						var (success, errorMsg) = CreateStampDutyFile(dialog);

						if (success)
						{
							try
							{
								FireSaveButton();
								Globals.Message.Show(Res.GetString("1B26E36F-C2FA-4974-AB92-96872F1328AD", "Saved it successfully to '{0}'", dialog.UnmappedFileName));
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}

						else
						{
							Globals.Message.Show(Res.GetString("1EB3DF8C-D550-43CA-852B-ACB50F137C05", "Failed to create stamp duty file:\n{0}", errorMsg));
						}
					}
				}
			}

			(bool Success, ZString ErrorMsg) CreateStampDutyFile(ZSaveFileDialog dialog)
			{
				var success = true;
				var errorMessage = ZString.Empty;

				try
				{
					using (Stream toFile = dialog.OpenFile())
					{
						var writer = new StreamWriter(toFile, System.Text.Encoding.UTF8);

						var countSequence = ZInt.Zero;
						var statementLinesWithoutINA = header.StatementLines.Where(x => x.B3_Status != StatementLineStatusList.Codes.INA);
						foreach (var lines in statementLinesWithoutINA)
						{
							var charges = lines.Charges.OrderBy(c => c.B4_ReferenceNumber);
							foreach (var lineCharge in charges)
							{
								var sb = new StringBuilder();

								countSequence++;
								lines.B3_EntryStatus = StatementLineStatusList.Codes.RLS;
								sb.Append(countSequence);
								sb.Append($"\t{lines.B3_EntryDate.ToString("dd.MM.yyyy")}");
								sb.Append($"\t{lineCharge.B4_ReferenceNumber.Substring(3).TrimStart('0')}");
								sb.Append($"\t{ChargeTypeForTxtFile(lineCharge.B4_ChargeType)}");
								sb.Append($"\t{lines.B3_BrokerReference}");
								sb.Append($"\t{(lineCharge.B4_ChargeType == "89" || lineCharge.B4_ChargeType == "ABS" || lineCharge.B4_ChargeType == "GMS" || lineCharge.B4_ChargeType == "OBS" || lineCharge.B4_ChargeType == "SBS" ? "MAKTU" : lines.B3_CustomsFeesTotal.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")))}");
								sb.Append($"\t{lineCharge.B4_ChargeAmount.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR"))}");

								writer.WriteLine(sb.ToString());
								writer.Flush();
							}
						}
					}
				}
				catch (Exception ex)
				{
					success = false;
					errorMessage = $"Could not create '{dialog.UnmappedFileName}'\n{ex.Message}";
				}
				return (success, errorMessage);

				ZString ChargeTypeForTxtFile(ZString originalChargeType)
				{
					var returnValue = ZString.Empty;

					switch (originalChargeType)
					{
						case "89":
							returnValue = "410";
							break;
						case "ABS":
							returnValue = "314";
							break;
						case "GMS":
							returnValue = "327";
							break;
						case "OBS":
							returnValue = "326";
							break;
						case "SBS":
							returnValue = "313";
							break;
						default:
							break;
					}

					return returnValue;
				}
			}
		}
	}
}
