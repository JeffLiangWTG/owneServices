using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportDocumentSupporter<T> : DocumentSupporter
		where T : DtbTransport
	{
		protected DtbTransportDocumentSupporter(T transport)
			: base(transport)
		{
		}

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericFreightJob && commandBeingRun != null)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Transport);
			}

			return result;
		}

		#endregion

		#region ShowReasonForNotPrinting

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion

		#region GetSupportedDataContexts

		protected sealed override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob, Constants.DataContext.GenericFreightJobServices }.Concat(GetSupportedDataContextsCore()).ToArray();
		}

		protected virtual Constants.DataContext[] GetSupportedDataContextsCore()
		{
			return System.Array.Empty<Constants.DataContext>();
		}

		#endregion

		#region Transport

		protected T Transport
		{
			get { return (T)BusinessObject; }
		}

		#endregion
	}
}
