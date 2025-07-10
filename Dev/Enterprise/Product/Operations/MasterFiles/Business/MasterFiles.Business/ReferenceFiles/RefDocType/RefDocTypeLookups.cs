using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RefDocTypeLookups : AutoRefDocTypeLookups
	{
		public RefDocTypeLookups(AutoRefDocType parent) : base(parent)
		{
		}

		public static class Codes
		{
			public const string ElectronicHouseBill = "SEHB";
		}

		public new CodeDescriptionPairList DocumentReceivedEvents
		{
			get
			{
				if (documentReceivedEvents == null)
				{
					var isPWEnabled = DataRegistry.Instance.ProductivityWiseModeEnabled;
					documentReceivedEvents = new CodeDescriptionPairList();
					documentReceivedEvents.AddRange(Events.All.Cast<Event>().
						Where(ev => !Events.ChangeLogs.Contains(ev) &&
							(!isPWEnabled || Events.ProductivityWiseEvents.Contains(ev))).
						Select(ev => new CodeDescriptionPair(ev.Code, ev.Description)).ToList());
				}
				return documentReceivedEvents;
			}
		}
		CodeDescriptionPairList documentReceivedEvents;
	}
}
