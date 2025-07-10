using System;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class SailingUserControl : ZUserControl
	{
		public SailingUserControl()
		{
			InitializeComponent();
		}

		CusInBondHeader Header
		{
			get { return (CusInBondHeader)DataSource; }
		}

		void SelectSailingButton_Click(object sender, EventArgs e)
		{
			if (!Header.CanChangeSailing)
			{
				Globals.Message.Show(CannotChangeSailingMessage);
			}
			else
			{
				var helper = new SailingIFindBox(Header, (ZForm)ParentForm);
				helper.ShowModuleFromISailingParent();
			}
		}

		internal static string CannotChangeSailingMessage
		{
			get { return Res.GetString("C178F87C-BF29-4729-AACB-8DFD7C264ADE", "Once messages are sent to customs, you cannot change the sailing."); }
		}

		void EditSailingButton_Click(object sender, EventArgs e)
		{
			var sailing = Header.Sailing;
			var voyage = sailing != null ? sailing.Voyage : null;
			if (voyage != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.JobSeaVoyage);
				var editForm = controller.ShowEditForm(voyage);
				editForm.Closed += delegate
				{
					if (Header != null)
					{
						Header.ChangeSailing(Header.BH_ParentID);
					}
				};
			}
			else
			{
				Globals.Message.Show(SailingScheduleNotCreated);
			}
		}

		internal static string SailingScheduleNotCreated
		{
			get { return Res.GetString("EBF20311-2C18-4B34-92F3-9F944D2E53EA", "There is no Sailing Schedule to edit."); }
		}

		void ClearSailingButton_Click(object sender, EventArgs e)
		{
			if (!Header.CanChangeSailing)
			{
				Globals.Message.Show(CannotChangeSailingMessage);
			}
			else
			{
				Header.ChangeSailing(ZGuid.Empty);
			}
		}

		void RefreshStatisticsButton_Click(object sender, EventArgs e)
		{
			Header.RefreshSailingStatistics();
		}
	}
}

#if DEBUG
namespace Enterprise.Customs.US.AMS.GUI
{
	partial class SailingUserControl
	{
		internal IZForm LastOpenedFormForTest { get; private set; }

		public void EditSailingButton_Click_ForTest(object sender, EventArgs e)
		{
			var sailing = Header.Sailing;
			var voyage = sailing != null ? sailing.Voyage : null;
			if (voyage != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.JobSeaVoyage);
				var editForm = controller.ShowEditForm(voyage);
				editForm.Closed += delegate
				{
					if (Header != null)
					{
						Header.ChangeSailing(Header.BH_ParentID);
					}
				};
				LastOpenedFormForTest = editForm;
			}
			else
			{
				Globals.Message.Show(SailingScheduleNotCreated);
			}
		}
	}
}
#endif
