#if NETFRAMEWORK
using System.Net.Http;
using System.Web.Http;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.NetCore;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class CustomsLookupControllerTest : TestCase
	{
		public void TestGetCustomsContainerModes()
		{
			var response = GetController().GetCustomsFullContainerModes().GetResult();
			AssertEquals((int)HttpStatusCode.OK, response.GetStatusCode());
			var message = response.GetMessage(HttpStatusCode.OK, isJson: true);

			var actualList = ConvertJsonTextToList(message);

			var expectedlist = new List<LookupListItem>()
				{
					new LookupListItem() { Code = "AIR" },
					new LookupListItem() { Code = "BBK" },
					new LookupListItem() { Code = "BCN" },
					new LookupListItem() { Code = "BLK" },
					new LookupListItem() { Code = "CNT" },
					new LookupListItem() { Code = "COM" },
					new LookupListItem() { Code = "CON" },
					new LookupListItem() { Code = "EMP" },
					new LookupListItem() { Code = "FAK" },
					new LookupListItem() { Code = "FCL" },
					new LookupListItem() { Code = "FCX" },
					new LookupListItem() { Code = "FTL" },
					new LookupListItem() { Code = "GRP" },
					new LookupListItem() { Code = "LCL" },
					new LookupListItem() { Code = "LQD" },
					new LookupListItem() { Code = "LSE" },
					new LookupListItem() { Code = "LTL" },
					new LookupListItem() { Code = "MAI" },
					new LookupListItem() { Code = "NCT" },
					new LookupListItem() { Code = "OBC" },
					new LookupListItem() { Code = "OTH" },
					new LookupListItem() { Code = "ROR" },
					new LookupListItem() { Code = "SCN" },
					new LookupListItem() { Code = "ULD" },
					new LookupListItem() { Code = "UNA" },
				};

			AssertEquals(expectedlist.Count, actualList.Count);

			var difference = expectedlist.Except(actualList, new LookupListItemComparer());
			AssertEquals(0, difference.Count());
		}

		public void TestGetCustomsContainerModesAU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var response = GetController().GetCustomsContainerModesByCurrentyCountry().GetResult();
				AssertEquals((int)HttpStatusCode.OK, response.GetStatusCode());
				var message = response.GetMessage(HttpStatusCode.OK, isJson: true);

				var actualList = ConvertJsonTextToList(message);

				var expectedlist = new List<LookupListItem>()
				{
					new LookupListItem() { Code = "CNT" },
					new LookupListItem() { Code = "NCT" },
					new LookupListItem() { Code = "COM" },
					new LookupListItem() { Code = "BLK" },
					new LookupListItem() { Code = "LQD" },
				};

				AssertEquals(expectedlist.Count, actualList.Count);

				var difference = expectedlist.Except(actualList, new LookupListItemComparer());
				AssertEquals(0, difference.Count());
			}
		}

		public void TestGetCustomsContainerModesUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var response = GetController().GetCustomsContainerModesByCurrentyCountry().GetResult();
				AssertEquals((int)HttpStatusCode.OK, response.GetStatusCode());
				var message = response.GetMessage(HttpStatusCode.OK, isJson: true);

				var actualList = ConvertJsonTextToList(message);

				var expectedlist = new List<LookupListItem>()
				{
					new LookupListItem() { Code = "LCL" },
					new LookupListItem() { Code = "FCL" },
					new LookupListItem() { Code = "LSE" },
					new LookupListItem() { Code = "ULD" },
					new LookupListItem() { Code = "BBK" },
					new LookupListItem() { Code = "BLK" },
					new LookupListItem() { Code = "LQD" },
					new LookupListItem() { Code = "ROR" },
					new LookupListItem() { Code = "LTL" },
					new LookupListItem() { Code = "FTL" },
					new LookupListItem() { Code = "OBC" },
					new LookupListItem() { Code = "UNA" },
					new LookupListItem() { Code = "CNT" },
					new LookupListItem() { Code = "NCT" },
				};

				AssertEquals(expectedlist.Count, actualList.Count);

				var difference = expectedlist.Except(actualList, new LookupListItemComparer());
				AssertEquals(0, difference.Count());
			}
		}

		public void TestGetCustomsContainerModesUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var response = GetController().GetCustomsContainerModesByCurrentyCountry().GetResult();
				AssertEquals((int)HttpStatusCode.OK, response.GetStatusCode());
				var message = response.GetMessage(HttpStatusCode.OK, isJson: true);

				var actualList = ConvertJsonTextToList(message);

				var expectedlist = new List<LookupListItem>()
				{
					new LookupListItem() { Code = "BBK" },
					new LookupListItem() { Code = "BLK" },
					new LookupListItem() { Code = "CNT" },
					new LookupListItem() { Code = "LQD" },
					new LookupListItem() { Code = "NCT" },
				};

				AssertEquals(expectedlist.Count, actualList.Count);

				var difference = expectedlist.Except(actualList, new LookupListItemComparer());
				AssertEquals(0, difference.Count());
			}
		}

		#region Implementation
		CustomsLookupController GetController()
		{
			var controller = new CustomsLookupController();
#if NETFRAMEWORK
			controller.Request = new HttpRequestMessage();
			controller.Configuration = new HttpConfiguration();
#else
			controller.ControllerContext = new ControllerContext()
			{
				HttpContext = new DefaultHttpContext()
			};
#endif
			return controller;
		}

		class LookupListItem
		{
			public string Code { get; set; }
		}

		List<LookupListItem> ConvertJsonTextToList(string jsonText)
		{
			var jsonSerializer = new JsonSerializer();
			var stringReader = new StringReader(jsonText);
			var jsonReader = new JsonTextReader(stringReader);
			var list = jsonSerializer.Deserialize<List<LookupListItem>>(jsonReader);
			return list;
		}

		class LookupListItemComparer : IEqualityComparer<LookupListItem>
		{
			public bool Equals(LookupListItem item1, LookupListItem item2)
			{
				if (item1 == null && item2 == null)
				{
					return true;
				}
				else if ((item1 != null && item2 == null) ||
						(item1 == null && item2 != null))
				{
					return false;
				}
				else
				{
					return string.Equals(item1.Code, item2.Code);
				}
			}

			public int GetHashCode(LookupListItem item)
			{
				var hash = 0;
				if ((item != null) && (item.Code != null))
				{
					hash = item.Code.GetHashCode();
				}
				return hash;
			}
		}
#endregion
	}
}
