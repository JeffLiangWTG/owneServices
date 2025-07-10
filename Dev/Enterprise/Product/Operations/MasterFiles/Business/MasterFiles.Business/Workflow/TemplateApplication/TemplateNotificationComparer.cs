using System;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	class TemplateNotificationComparer : IComparer<ProcessTaskNotification>
	{
		public int Compare(ProcessTaskNotification x, ProcessTaskNotification y)
		{
			using (x.SuppressIsSetFieldForSetters())
			using (y.SuppressIsSetFieldForSetters())
			{
				int result = 0;
				using (var compareItr = GetComparers(x, y).GetEnumerator())
				{
					while (result == 0 && compareItr.MoveNext())
					{
						result = compareItr.Current();
					}
				}

				return result;
			}
		}

		static IEnumerable<Func<int>> GetComparers(ProcessTaskNotification x, ProcessTaskNotification y)
		{
			yield return () => x.PQ_TriggerType.CompareTo(y.PQ_TriggerType);
			yield return () => x.PQ_SU_Document.CompareTo(y.PQ_SU_Document);
			yield return () => x.PQ_Calc_TriggerParty.CompareTo(y.PQ_Calc_TriggerParty);
			yield return () => x.PQ_TriggerParty.CompareTo(y.PQ_TriggerParty);
			yield return () => x.PQ_OH_Recipient.CompareTo(y.PQ_OH_Recipient);
			yield return () => x.PQ_MessagePurpose.CompareTo(y.PQ_MessagePurpose);

			yield return () =>
			{
				if (x.PQ_Calc_TriggerParty == y.PQ_Calc_TriggerParty && x.IsPersonRelatedEmailTriggerParty)
				{
					return 0;
				}
				return x.PQ_EmailAddr.CompareTo(y.PQ_EmailAddr);
			};

			yield return () => x.PQ_SQ.CompareTo(y.PQ_SQ);
		}
	}
}
