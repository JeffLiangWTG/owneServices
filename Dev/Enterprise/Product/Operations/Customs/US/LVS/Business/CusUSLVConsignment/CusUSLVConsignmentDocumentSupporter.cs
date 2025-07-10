using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.LVS.Business
{
	class CusUSLVConsignmentDocumentSupporter : DocumentSupporter
	{
		public CusUSLVConsignmentDocumentSupporter(CusUSLVConsignment parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		protected CusUSLVConsignment Consignment => (CusUSLVConsignment)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.USLowValueBill;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.USLVClearanceCustomiseDocuments;

		protected override List<DataContextValue> GetSupportedBODataSources() => new List<DataContextValue>();

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => null;

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun) => null;

		protected override DataContext[] GetSupportedDataContexts() => Array.Empty<DataContext>();

		public override bool ShowDocumentsInDynamicMenu => false;
	}
}
