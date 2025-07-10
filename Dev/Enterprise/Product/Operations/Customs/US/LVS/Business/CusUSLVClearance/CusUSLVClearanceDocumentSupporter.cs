using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVClearanceDocumentSupporter : DocumentSupporter
	{
		public CusUSLVClearanceDocumentSupporter(CusUSLVClearance clearance) : base(clearance) { }

		protected CusUSLVClearance Clearance => (CusUSLVClearance)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.INVALID;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.USLVClearanceCustomiseDocuments;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			return Enumerable.Empty<DataContextValue>().ToList();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => null;

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			return null;
		}

		protected override DataContext[] GetSupportedDataContexts() => Array.Empty<DataContext>();
	}
}
