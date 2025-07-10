using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class FTZConcurrenceForTesting : IFTZConcurrence
	{
		public FTZConcurrenceForTesting(BusinessObjectFactory factory)
		{
			Factory = factory;
			changeLog = new ZStringBuilder();
		}

		public ZDecimal FTZConcurrenceQty
		{
			get => ftzConcurrenceQty;
			set
			{
				if (ftzConcurrenceQty != 0m)
				{
					changeLog.AppendLine($"{ftzConcurrenceQty} => {value}");
				}
				ftzConcurrenceQty = value;
			}
		}
		ZDecimal ftzConcurrenceQty;

		public void ClearChangeLog()
		{
			changeLog = new ZStringBuilder();
		}

		public ZString GetChangeLog() => changeLog.ToString();

		public BusinessObjectFactory Factory { get; }

		ZStringBuilder changeLog;
	}
}
