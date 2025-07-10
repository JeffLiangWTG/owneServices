using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.LVS.GUI
{
	public class DisclaimApplicablePGAsSectionLog : IOperationalActionSectionLog
	{
		public DisclaimApplicablePGAsSectionLog()
		{
			resultBuilder = new ZStringBuilder();
		}
		readonly ZStringBuilder resultBuilder;

		public ZString Logs
		{
			get
			{
				return resultBuilder.ToStringWithNewLineBetweenAppends();
			}
		}

		#region IOperationalActionSectionLog Members

		void IOperationalActionSectionLog.BumpSectionProgress()
		{
		}

		void IOperationalActionSectionLog.Notify(OperationalActionLogErrorLevel errorLevel, string text)
		{
			resultBuilder.Append(text);
		}

		void IOperationalActionSectionLog.NotifyFormat(OperationalActionLogErrorLevel errorLevel, string format, params object[] args)
		{
		}

		void IOperationalActionSectionLog.SetSectionProgressMax(int max)
		{
		}

		#endregion
	}
}
