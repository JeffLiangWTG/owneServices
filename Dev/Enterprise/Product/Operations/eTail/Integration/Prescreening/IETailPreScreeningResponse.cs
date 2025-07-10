using System.Collections.ObjectModel;

namespace Enterprise.eTail.Integration
{
	public interface IETailPreScreeningResponse
	{
		ReadOnlyCollection<IHVLVConsignmentPreScreeningResult> Results { get; }

		bool Finished { get; }

		bool Passed { get; }

		string ErrorMessage { get; }

		string ErrorMessageDetail { get; }
	}
}
