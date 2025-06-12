using System.Collections.Generic;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
	public interface IQueueSpec
    {
        string Name { get; }
        IDictionary<string, object> Configuration { get; }
		IQueueSpec DeadLetterQueue { get; }
		IEnumerable<IBindingSpec> Bindings { get; }

        IQueueSpec WithDeadLetterQueue(IQueueSpec spec);
		IQueueSpec WithBinding(IBindingSpec spec);
    }
}
