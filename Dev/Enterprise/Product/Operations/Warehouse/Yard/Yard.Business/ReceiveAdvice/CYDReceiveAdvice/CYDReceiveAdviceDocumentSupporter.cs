using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveAdviceDocumentSupporter : DocumentSupporter
	{
		public CYDReceiveAdviceDocumentSupporter(CYDReceiveAdvice receiveAdvice)
			: base(receiveAdvice)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CYDReceiveAdvice; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, ReceiveAdvice);

			if (genericWrappers != null)
			{
				return genericWrappers;
			}

			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(DataContext.CYDReceiveAdvice, ReceiveAdvice) };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.CYDReceiveAdvice };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion

		#region ReceiveAdvice

		protected CYDReceiveAdvice ReceiveAdvice
		{
			get { return (CYDReceiveAdvice)BusinessObject; }
		}

		#endregion
	}
}
