using System.Collections.Generic;

namespace Enterprise.Services.Scim.Models
{
	public class SearchScimResponse<T>
	{
		public int TotalResults { get; set; }
		public IEnumerable<T> Content { get; set; }

		public SearchScimResponse(int totalResults, IEnumerable<T> content)
		{
			TotalResults = totalResults;
			Content = content;
		}
	}
}
