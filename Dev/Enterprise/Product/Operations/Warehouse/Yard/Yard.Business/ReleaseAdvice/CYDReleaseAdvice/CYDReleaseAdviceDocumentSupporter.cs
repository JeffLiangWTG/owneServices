using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseAdviceDocumentSupporter : DocumentSupporter
	{
		public CYDReleaseAdviceDocumentSupporter(CYDReleaseAdvice releaseAdvice)
			: base(releaseAdvice)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CYDReleaseAdvice; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, ReleaseAdvice);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.CYDReleaseAdvice, ReleaseAdvice) };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.CYDReleaseAdvice };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region ReleaseAdvice

		protected CYDReleaseAdvice ReleaseAdvice
		{
			get { return (CYDReleaseAdvice)BusinessObject; }
		}

		#endregion
	}
}
