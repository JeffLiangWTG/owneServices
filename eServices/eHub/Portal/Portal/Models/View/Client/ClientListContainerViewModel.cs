using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcContrib.UI.Grid;
using MvcContrib.Pagination;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models
{
	public class ClientListContainerViewModel
	{
		public IPagination<eHubClient> PagedList { get; set; }
		public ClientFilterViewModel FilterViewModel { get; set; }
		public GridSortOptions GridSortOptions { get; set; }
	}
}