using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DeniedPartyScreeningWorker
	{
		public static async Task<List<DpsResponseWithScreeningParty>> SubmitRequestWithResponse(Form parentForm, ScreeningParty[] screeningParties)
		{
			Argument.NotNull(parentForm, nameof(parentForm), "Should provide proper parent form");
			Argument.NotNull(screeningParties, nameof(screeningParties), "Should provide proper screening request parties");

			var cancelledRequest = false;
			var countScreenedParties = 0;
			var countScreeningParties = screeningParties.Count(party => !party.IsCurrentScreeningStatusValid);
			var response = new List<DpsResponseWithScreeningParty>();

			if (countScreeningParties > 0)
			{
				var progressForm = new ProgressForm();
				try
				{
					progressForm.DoNotCloseWhenClickCancel = true;
					progressForm.Cancelled += (obj, args) =>
					{
						cancelledRequest = true;
						progressForm.SetStatusAndPercentComplete(ShowProgressMessage(countScreenedParties, countScreeningParties, cancelledRequest), countScreenedParties * 100 / countScreeningParties);
					};

					progressForm.ShowModalTo(parentForm);
					progressForm.SetStatusAndPercentComplete(ShowProgressMessage(countScreenedParties, countScreeningParties, cancelledRequest), 0);

					response = await DeniedPartyScreenerAsync.Screen(() =>
					{
						countScreenedParties++;
						progressForm.SetStatusAndPercentComplete(ShowProgressMessage(countScreenedParties, countScreeningParties, cancelledRequest), countScreenedParties * 100 / countScreeningParties);
						return cancelledRequest;
					}, screeningParties);

					progressForm.DoNotCloseWhenClickCancel = false;
					progressForm.Hide();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					DpsExceptionHandler.Process(ex, false);
				}
				finally
				{
					progressForm.Close();
				}
			}

			return response;
		}

		internal static string ShowProgressMessage(int screenedParties, int requestParties, bool hasCancelled)
		{
			var extraMessage = hasCancelled ? (NoResString)"\r\n" + (NoResString)"Please wait until current party is screened." : string.Empty;
			return Res.GetString("80D2D511-0EFF-42B1-A2D4-38BD91118A36", @"Screening... {0} of {1} parties screened.{2}", screenedParties, requestParties, extraMessage);
		}

		internal static (List<DpsSourceWithParties> SourceWithParties, List<ScreeningParty> ScreeningParties) GetSourceWithParties(BusinessObject[] bizObjects)
		{
			var sourceWithParties = new List<DpsSourceWithParties>();
			var screeningParties = new List<ScreeningParty>();

			using (new ZWaitCursorChanger())
			{
				foreach (var bizO in bizObjects)
				{
					DpsSourceWithParties dpsSourceWithParties = null;

					if (bizO is RefCountry country)
					{
						dpsSourceWithParties = AddSourceWithParties(country);
					}
					else if (bizO is IScreeningPartyProvider provider)
					{
						dpsSourceWithParties = AddSourceWithParties(bizO, provider);
					}

					if (dpsSourceWithParties != null)
					{
						sourceWithParties.Add(dpsSourceWithParties);
						screeningParties.AddRange(dpsSourceWithParties.ScreenParties);
					}
				}
			}

			return (sourceWithParties, screeningParties);
		}

		public static ScreeningParty[] ExcludePermanentClearParties(ScreeningParty[] screeningParties)
		{
			return screeningParties.Where(party => party.CurrentScreeningStatus != ScreeningStatusesList.Codes.PermanentClear).ToArray();
		}

		static DpsSourceWithParties AddSourceWithParties(RefCountry country) => new DpsSourceWithParties(country, new[] { new ScreeningParty(country, country.HumanReadableName, country) });

		static DpsSourceWithParties AddSourceWithParties(BusinessObject bizO, IScreeningPartyProvider provider) => new DpsSourceWithParties(bizO, provider.ScreeningParties);
	}
}
