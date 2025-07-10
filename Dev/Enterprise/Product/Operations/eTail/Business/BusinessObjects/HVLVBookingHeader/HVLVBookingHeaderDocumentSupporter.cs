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

namespace Enterprise.eTail.Business
{
	public class HVLVBookingHeaderDocumentSupporter : DocumentSupporter
	{
		public HVLVBookingHeaderDocumentSupporter(HVLVBookingHeader bookingHeader) : base(bookingHeader) { }

		public override BusinessContext BusinessContext => BusinessContext.HVLVBookingHeader;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.HVLVBookingHeaderCustomiseDocuments;

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun) => null;

		protected HVLVBookingHeader BookingHeader => (HVLVBookingHeader)BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources() => Enumerable.Empty<DataContextValue>().ToList();

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => null;

		protected override DataContext[] GetSupportedDataContexts() => Array.Empty<DataContext>();
	}
}
