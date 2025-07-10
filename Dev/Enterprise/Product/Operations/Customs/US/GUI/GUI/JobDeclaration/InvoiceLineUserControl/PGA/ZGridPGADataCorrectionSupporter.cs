using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	public class ZGridPGADataCorrectionSupporter
	{
		public ZGridPGADataCorrectionSupporter(ZGrid grid)
		{
			this.grid = grid;
			if (!grid.IsDesignMode())
			{
				releaseDateThreshold = ZDateTime.Today.AddDays(-10);
				var workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
				if (workingDays != null)
				{
					releaseDateThreshold = workingDays.GetAnotherStandardWorkingDay(ZDate.Today.ToDateTime(), -10);
				}
			}
		}

		readonly ZGrid grid;
		readonly ZDateTime releaseDateThreshold;

		public void AddPGALineEditMenu()
		{
			grid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			grid.ContextMenu.MenuItems.Add("Update PGA Line", UpdatePGALines_Clicked);
			grid.ContextMenu.MenuItems.Add("Delete PGA Line", DeletePGALines_Clicked);
			grid.ContextMenu.MenuItems.Add("Add More PGA Lines", AddMorePGALines_Clicked);
			grid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
		}

		void UpdatePGALines_Clicked(object sender, EventArgs e)
		{
			var list = grid.ListManager.List as IPGADataCorrectionCollection;
			if (list != null)
			{
				if (grid.SelectedElements.Length < 1)
				{
					Globals.Message.ShowInformation(SelectPGALineMessage);
				}
				else
				{
					var lines = grid.SelectedElements.Cast<IPGADataCorrection>().Where(x => x.CanBeChangedToBeUpdated()).ToArray();
					var count = lines.Length;
					var message = Res.GetString("BBC4A870-117F-42DB-A0AC-7B235F72407E", "Status of {0} PGA lines updated.", count);
					if (count == 0)
					{
						Globals.Message.ShowInformation(Res.GetString("A2AC189C-80AA-42A4-9C8E-E1FCD18AAD4F", "There are no PGA lines that can be updated. The current PGA Status should be either 'Lodged' or 'To Be Deleted' to be changed to 'To Be Updated'."));
					}
					else
					{
						var releaseDate = list.GetReleaseDate();
						var agencyCode = CheckCorrectionForPGAs(list);
						if (!agencyCode.IsEmpty && !releaseDate.IsEmpty)
						{
							var warningMessage = GetReleasedUpdateStatusChangeWarning(agencyCode);
							UpdateStatus(
								() => Globals.Message.Show(warningMessage, "Warning", MessageBoxButtons.YesNo, DialogResult.No),
								lines,
								PGATrackingStatusList.Codes.ToBeUpdated,
								message);
						}
						else if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
						{
							UpdateStatus(
								() => Globals.Message.Show(PGAReleasedUpdateStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No),
								lines,
								PGATrackingStatusList.Codes.ToBeUpdated,
								message);
						}
						else
						{
							UpdateStatus(
								() => Globals.Message.Show(UpdateStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No),
								lines,
								PGATrackingStatusList.Codes.ToBeUpdated,
								message);
						}
					}
				}
			}
		}

		void DeletePGALines_Clicked(object sender, EventArgs e)
		{
			var list = grid.ListManager.List as IPGADataCorrectionCollection;
			if (list != null)
			{
				if (grid.SelectedElements.Length < 1)
				{
					Globals.Message.ShowInformation(SelectPGALineMessage);
				}
				else
				{
					var lines = grid.SelectedElements.Cast<IPGADataCorrection>().Where(x => (ZString)x.TrackingStatusInfo.Value == PGATrackingStatusList.Codes.Added).ToArray();
					var count = lines.Length;
					var message = Res.GetString("82A0BD07-6308-4628-AC25-3FA5B4F14E8B", "Status of {0} PGA lines changed to 'To be deleted'.", count);
					if (count == 0)
					{
						Globals.Message.ShowInformation(Res.GetString("A2AC189C-80AA-42A4-9C8E-E1FCD18AAD4G", "There are no PGA lines that can be changed. The PGA Status should be 'Lodged' to be changed to 'To Be Deleted'."));
					}
					else
					{
						var releaseDate = list.GetReleaseDate();
						var agencyCode = CheckCorrectionForPGAs(list);
						if (!agencyCode.IsEmpty && !releaseDate.IsEmpty)
						{
							var warningMessage = GetReleasedDeleteStatusChangeWarning(agencyCode);
							UpdateStatus(
								() => Globals.Message.Show(warningMessage, "Warning", MessageBoxButtons.YesNo, DialogResult.No),
								lines,
								PGATrackingStatusList.Codes.ToBeDeleted,
								message);
						}
						else if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
						{
							UpdateStatus(
								() => Globals.Message.Show(PGAReleasedDeleteStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No),
								lines,
								PGATrackingStatusList.Codes.ToBeDeleted,
								message);
						}
						else
						{
							UpdateStatus(
								() => Globals.Message.Show(DeleteStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No),
								lines,
								PGATrackingStatusList.Codes.ToBeDeleted,
								message);
						}
					}
				}
			}
		}

		void AddMorePGALines_Clicked(object sender, EventArgs e)
		{
			var list = grid.ListManager.List as IPGADataCorrectionCollection;
			if (list != null)
			{
				var releaseDate = list.GetReleaseDate();
				var agencyCodes = CheckCorrectionForPGAs(list);
				if (!agencyCodes.IsEmpty && !releaseDate.IsEmpty)
				{
					var warningMessage = GetReleasedAddStatusChangeWarning(agencyCodes);
					if (Globals.Message.Show(warningMessage, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						list.AllowAddNewPGALines = true;
					}
				}
				else if (!releaseDate.IsEmpty && releaseDate < releaseDateThreshold)
				{
					if (Globals.Message.Show(PGAReleasedAddStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						list.AllowAddNewPGALines = true;
					}
				}
				else
				{
					if (Globals.Message.Show(AddStatusChangeWarning, "Warning", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						list.AllowAddNewPGALines = true;
					}
				}
				grid.ListManager.Refresh();
				grid.Refresh();
			}
		}

		ZString CheckCorrectionForPGAs(IPGADataCorrectionCollection list)
		{
			var result = ZString.Empty;
			if (list.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FDA))
			{
				result = ACEGovernmentAgenciesCodeList.Codes.FDA;
			}
			else if (list.HasSpecificPGALines(ACEGovernmentAgenciesCodeList.Codes.FWS))
			{
				result = ACEGovernmentAgenciesCodeList.Codes.FWS;
			}
			return result;
		}

		void UpdateStatus(Func<DialogResult> getDialogResultFunc, IPGADataCorrection[] lines, ZString status, ZString message)
		{
			if (getDialogResultFunc() == DialogResult.Yes)
			{
				foreach (var pgaLine in lines)
				{
					pgaLine.TrackingStatusInfo.Value = status;
				}
				Globals.Message.ShowInformation(message);
			}
		}

		public static string SelectPGALineMessage => Res.GetString("BED9C96B-C534-473E-8228-B433B8958C79", "Please select (highlight) at least one PGA line.");

		public static string UpdateStatusChangeWarning => Res.GetString("F8677D96-8244-48BB-B1A2-24BB5B6FC13A", "You are about to change the status of this PGA line to 'To Be Updated'. This means that after changing it you will need to send a PGA data correction message to register this at Customs. Are you sure you wish to continue?");
		public static string GetReleasedUpdateStatusChangeWarning(ZString pgaCodes)
		{
			return Res.GetString("DD589222-213C-4B43-B379-5282B37EBBD7", "You are about to change the status of this PGA line to 'To Be Updated'. The entry has already been released.  {0} will no longer accept changes to the {0} data. Are you sure you wish to continue?", pgaCodes);
		}
		public static string PGAReleasedUpdateStatusChangeWarning => Res.GetString("D16D7FB2-0B76-4B24-8CC1-971D067CCB2B", "You are about to change the status of this PGA line to 'To Be Updated'. The entry was released more than 10 days ago.  CBP will no longer accept changes to the PGA data. Are you sure you wish to continue?");

		public static string DeleteStatusChangeWarning => Res.GetString("124F6305-D41A-43A3-A885-F05DC62D578E", "You are about to change the status of this PGA line to 'To Be Deleted'. This means that after changing it you will need to send a PGA data correction message to register this at Customs. Are you sure you wish to continue?");

		public static string GetReleasedDeleteStatusChangeWarning(ZString pgaCodes)
		{
			return Res.GetString("452A3BF3-4BF7-49AD-A98D-D6BF70B96386", "You are about to change the status of this PGA line to 'To Be Deleted'. The entry has already been released.  {0} will no longer accept changes to the {0} data. Are you sure you wish to continue?", pgaCodes);
		}

		public static string PGAReleasedDeleteStatusChangeWarning => Res.GetString("CE885CE7-68F4-49F1-8E9A-0A8C7729CF81", "You are about to change the status of this PGA line to 'To Be Deleted'. The entry was released more than 10 days ago.  CBP will no longer accept changes to the PGA data. Are you sure you wish to continue?");

		public static string AddStatusChangeWarning => Res.GetString("4EBF7AEF-80FF-4936-98D4-8F05BEE15A53", "You are about to ADD a PGA line to the invoice. This means that after changing it you will need to send a PGA data correction message to register this at Customs. Are you sure you wish to continue?");

		public static string GetReleasedAddStatusChangeWarning(ZString pgaCodes)
		{
			return Res.GetString("C5EBB80C-83A8-47E1-AC65-69787A69CED7", "You are about to ADD a PGA line to the invoice. The entry has already been released. {0} will no longer accept changes to the {0} data. Are you sure you wish to continue?", pgaCodes);
		}

		public static string PGAReleasedAddStatusChangeWarning => Res.GetString("D73D994B-C442-4CBB-9AD8-55232215E20A", "You are about to ADD a PGA line to the invoice. The entry was released more than 10 days ago. CBP will no longer accept changes to the PGA data. Are you sure you wish to continue?");
	}
}
