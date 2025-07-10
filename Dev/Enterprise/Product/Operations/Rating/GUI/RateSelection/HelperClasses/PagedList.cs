using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Rating.GUI.RateSelection.HelperClasses
{
	public class PagedList
	{
		readonly List<object> DataSet = new List<object>();

		public PagedList(System.Collections.IEnumerable data, int pageSize, int currentPage = 1)
		{
			foreach (var item in data)
			{
				DataSet.Add(item);
			}

			PageSize = pageSize;
			CurrentPage = currentPage;
			TotalPageCount = (DataSet.Count > 0) ? ((int)Math.Ceiling((double)DataSet.Count / PageSize)) : 0;
		}

		public int TotalPageCount { get; private set; }
		public int PageSize { get; private set; }
		public int CurrentPage { get; private set; }

		public bool HasNextPage => CurrentPage < TotalPageCount;
		public bool HasPreviousPage => CurrentPage > 1;
		public bool IsFirstPage => CurrentPage == 1;
		public bool IsLastPage => CurrentPage >= TotalPageCount;

		public System.Collections.IEnumerable GetCurrentPage()
		{
			if (DataSet.Any())
			{
				return CurrentPage == 1 ? DataSet.Take(PageSize).ToArray() : DataSet.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToArray();
			}

			return Enumerable.Empty<object>();
		}

		public void NextPage()
		{
			if (!IsLastPage)
			{
				CurrentPage++;
			}
		}
		public void PreviousPage()
		{
			if (!IsFirstPage)
			{
				CurrentPage--;
			}
		}
		public void GotoPage(int pageNumber)
		{
			if (pageNumber >= 1 && pageNumber <= TotalPageCount)
			{
				CurrentPage = pageNumber;
			}
		}

		public void ChangePageSize(int newPageSize)
		{
			PageSize = newPageSize;
			TotalPageCount = (DataSet.Count > 0) ? ((int)Math.Ceiling((double)DataSet.Count / PageSize)) : 0;
			CurrentPage = 1;
		}
	}
}
