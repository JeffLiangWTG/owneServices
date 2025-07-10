using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.GUI;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	/// <summary>
	/// Module for Statements
	/// </summary>
	class CusStatementModule : ZFilterGridModule
	{
		public CusStatementModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ZAModuleIDs.CustomsStatement; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowView
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override SortInfo DefaultSortOrder => new SortInfo(CusStatementLineCharge.Schema.ProcessDate, System.ComponentModel.ListSortDirection.Descending);

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusStatementFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusStatementLineChargeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusStatementFilterStripBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(CustomsControllerIDs.CustomsStatement);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ImportBroker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZACustomsStatement); }
		}

		public override bool SupportsWorkflow
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (result.Count > 0 && result[result.Count - 1].Text != "-")
			{
				result.Add(new ZMenuItem("-"));
			}

			var statacREQDOC = new ZMenuItem(ResString.GetMultilingualString("f16ff7e0-43f1-4f62-abc7-6727a992d7fe", "Customs Statement Request (REQDOC)"), delegate
			{
				Form parentForm = this.LocateMainForm();
				STATACREQDOCSendingObjectParent parent = new STATACREQDOCSendingObjectParent(Factory);
				using (var form = new STATACREQDOCSendingForm(parent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form, parentForm) == DialogResult.OK)
					{
						new STATACREQDOCMessageManager(parent, new MessageNotificationCollector()).SendMessages();
					}
				}
			});
			result.Add(statacREQDOC);
			return result.ToArray();
		}
	}
}
