using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Module for Statements
	/// </summary>
	class StatementModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public StatementModule() => Plugins.Add(ControllerIDs.OperationalActions);

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USCustomsStatement;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => StatementProcessTask.StatementWorkflow.Code;

		protected override IFilterControl GetNewFilterControl() => new StatementFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new StatementCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new StatementFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CustomsStatement);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USCustomsImportStatement;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem statementPaymentAuthorizationRequestItem = new ZMenuItem("Statement Re-Route Request");
			statementPaymentAuthorizationRequestItem.Click += new EventHandler(statementPaymentAuthorizationRequestItem_Click);
			result.Add(statementPaymentAuthorizationRequestItem);

			return result.ToArray();
		}

		void statementPaymentAuthorizationRequestItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new StatementAndACHRerouteForm(new StatementAndACHPaymentReroute()));
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new StatementOperationalActionSupporter();
	}
}
