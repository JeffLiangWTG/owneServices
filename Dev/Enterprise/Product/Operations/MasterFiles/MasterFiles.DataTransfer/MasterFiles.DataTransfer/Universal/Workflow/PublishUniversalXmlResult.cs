using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Workflow
{
	public class PublishUniversalXmlResult : IEnumerable<UniversalEvent>
	{
		public PublishUniversalXmlResult(IList<UniversalEvent> events)
		{
			this.events = events;
		}
		readonly IList<UniversalEvent> events;
		public int Length => events.Count;
		public IEnumerator<UniversalEvent> GetEnumerator() => events.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => events.GetEnumerator();
		public UniversalEvent this[int i]
		{
			get => events[i];
		}

		Exception failureException;
		public Exception FailureException
		{
			get => failureException;
			set => failureException = value;
		}

		public bool ShouldRetry { get; internal set; }
	}
}
