using System;
using CargoWise.Common;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class DocPackMenuItem : ZMenuItem
	{
		public DocPackMenuItem(WhsPick pick)
			: base(ResString.GetMultilingualString("812bc52d-c4ab-4020-9c83-1073b371afcf", "Reprint Pick Documents Pack"))
		{
			Pick = Argument.NotNull(pick, "Pick");
		}

		readonly WhsPick Pick;

		#region Overrides

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (Pick.WP_PickStatus == PickStatus.Codes.Building)
			{
				Globals.Message.ShowWarning(Res.GetString("6c940a73-984b-4599-b6a0-83b2ba8e7d05", "Pick documents cannot be printed while pick is being built"));
			}
			else
			{
				var buffer = new NotificationBuffer();
				new WhsPickDocumentsAutoPrinter(Pick, buffer, true).PrintDocument();

				if (buffer.HasErrors)
				{
					Globals.Message.ShowError(buffer.AsString);
				}
			}
		}

		#endregion
	}
}
