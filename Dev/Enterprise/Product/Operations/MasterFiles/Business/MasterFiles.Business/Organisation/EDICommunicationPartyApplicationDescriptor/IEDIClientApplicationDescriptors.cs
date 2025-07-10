using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IEDIClientApplicationDescriptors
	{
		IEnumerable<IEDIClientApplicationDescriptor> Values { get; }
		IEDIClientApplicationDescriptor GetValue(string code);
	}
}
