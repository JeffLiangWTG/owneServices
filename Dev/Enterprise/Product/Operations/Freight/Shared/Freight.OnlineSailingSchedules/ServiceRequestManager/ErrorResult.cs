using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Freight.OnlineSailingSchedules.ServiceRequestManager
{
	public class ErrorResult
	{
		public string Type { get; set; }

		public string Title { get; set; }

		public string Status { get; set; }

		public string TraceId { get; set; }

		public string Detail { get; set; }

		public Dictionary<string, IList<string>> Errors { get; set; }

		public string ErrorMessageFull
		{
			get
			{
				var errorMessageFull = Detail;

				if (string.IsNullOrEmpty(errorMessageFull) && Errors != null)
				{
					var allErrorMessages = Errors.SelectMany(e => e.Value);
					errorMessageFull = string.Join(System.Environment.NewLine, allErrorMessages);
				}

				return errorMessageFull;
			}
		}

		public IReadOnlyList<string> ProblemMessages
		{
			get
			{
				if (!string.IsNullOrEmpty(Detail))
				{
					return new[] { Detail };
				}

				return Errors?.SelectMany(e => e.Value).ToList();
			}
		}
	}
}
