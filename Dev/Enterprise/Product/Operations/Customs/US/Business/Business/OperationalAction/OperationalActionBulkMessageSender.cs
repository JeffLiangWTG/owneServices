using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public abstract class OperationalActionBulkMessageSender<T>
		where T : BusinessObject
	{
		public OperationalActionBulkMessageSender(T job)
		{
			this.job = job;
		}
		protected readonly T job;

		public SaveResult OperationalActionSendMessage(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			return OperationalActionSendMessageCore(sendWithMessageErrors, log);
		}
		protected abstract SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log);

		internal string MessageType
		{
			get { return MessageTypeCore; }
		}
		protected abstract string MessageTypeCore { get; }

		protected abstract LogControllerLink JobLink { get; }
	}
}
