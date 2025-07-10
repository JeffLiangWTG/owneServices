using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCDataVersionModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USCDataVersion;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.HTSReferenceFilesDataVersion;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (!IsSharedDatabase())
			{
				result.Add(new ZMenuItem("Reset HTS Version", new EventHandler(ResetHTSVersion_Click)));
			}
			return result.ToArray();

			bool IsSharedDatabase()
			{
				var currentRefDbName = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Enterprise, Core.Constants.CountryCodes.UnitedStates);
				return RefDbTableNameResolver.IsSharedDatabase(currentRefDbName);
			}
		}

		protected void ResetHTSVersion_Click(object sender, EventArgs e)
		{
			if (!Env.Security.HTSReferenceFilesDataVersionReset.IsAllowed)
			{
				Env.Security.HTSReferenceFilesDataVersionReset.ShowError();
			}
			else
			{
				var message = "The HTS Version will be reset. HTS Reference Files will be updated over the next few hours. It is suggested that you only reset during hours of low business activity.";

				var result = Globals.Message.Show(message, "Reset Version?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
				if (result == DialogResult.OK)
				{
					var dataVersion = USCDataVersion.GetLastHTSAttempt(Factory);
					dataVersion.UZ_Version = 9805;
					dataVersion.UZ_Note = ExtractReferenceFilesResult.Success;

					new ReferenceFileRequester().RequestTariffByUpdate(dataVersion.UZ_Version);

					Factory.Save();
					Globals.Message.ShowInformation(@"Version Reset. 
				
Initial Harmonized Tariff Schedule Request Message sent.");
				}
			}
		}

		protected override IFilterControl GetNewFilterControl() => new USCDataVersionFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCDataVersionCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCDataVersionFilterStripBusinessObject();
	}
}
