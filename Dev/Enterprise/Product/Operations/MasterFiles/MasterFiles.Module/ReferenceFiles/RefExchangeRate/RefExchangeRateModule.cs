using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefExchangeRateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ExchangeRate;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.ExchangeRate);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new RefExchangeRateFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
			=> new RefExchangeRateFilterControl(GridCollection, (RefExchangeRateFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new RefExchangeRateCollection(Factory, new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK));

		protected override Func<DataTable, DataRow> FilterOnMultiRowResult => table =>
		{
			var currentCompanyPk = Env.CurrentCompanyPK;
			return table.Rows.Cast<DataRow>().SingleOrDefault(dr => dr.Field<Guid>(RefExchangeRateSchema.RE_GC.Name) == currentCompanyPk);
		};

		#region Exchange Rate Bulk Update

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant for comparison")]
		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				List<ToolBarButton> result = new List<ToolBarButton>(base.ToolBarButtons);

				foreach (ZToolBarButton button in result)
				{
					if (button.Text == "Ex Rate Update")
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Dollar);
						break;
					}
				}

				return result.ToArray();
			}
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("MasterFiles.ExchangeRate.ExRateUpdate", "Ex Rate Update"), BulkUpdateButton_Click));
			return result.ToArray();
		}

		void BulkUpdateButton_Click(object sender, EventArgs e) => new ExchangeRateWrapperController().ShowNewForm();

		#endregion
	}
}
