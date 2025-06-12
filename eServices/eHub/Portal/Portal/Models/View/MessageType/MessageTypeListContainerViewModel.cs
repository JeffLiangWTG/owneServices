using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcContrib.UI.Grid;
using MvcContrib.Pagination;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models
{
	public class MessageTypeListContainerViewModel
	{
		public IPagination<eHubMessageType> PagedList { get; set; }
		public MessageTypeFilterViewModel FilterViewModel { get; set; }
		public GridSortOptions GridSortOptions { get; set; }
	}
}