using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class USCustomsPluginTest : SqlBillingTransactionsPluginTest<Plugins.USCustoms.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "CC_ID", "AM_ApplicationCode", "AM_RecipientMessageRaw", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"MTRMTLHST", "USI", "H4sIAAAAAAAEAK1TbWvCMBD+K9kPGN5dXoxQClEzNqizS6tliB+Uln2zMLcP+/eLLV2zITi0D4Qjl+R57h5y0dNhX38eykV1PO7eqjh6rHZl9R5Hd5vZ3ORmY1gIROAo2A3YbuNo1IlM6/Krl5oiCgQstHLuFokOrqNjQIJLmihAgnsYT0AKDiAAQCJxsVgmK7+YfZ5bs17+iINSRd637MQaJZAG4FKyU9RcCgJiNPa3hE8j8rACaIFp4jeNSvGS9/XJltuTapCpSa1LbJZd32/AR+RsYk1mmbfWslXahHNo3/gmLvnHCc75B6gG9O93Hf/2Tw3snxrCv9fgP5/a0FdX1qIZnXZkHur6I5zS5qhLjv7M9DcxVuQh5AMAAA==", new Guid("3918b567-6d63-4eaa-9bfd-4534605f6564"), DateTime.Parse("2014-11-03 20:08:53.000"), DateTime.Parse("2014-11-03 20:09:53.000")},
					new object[] {"AWLCHIORD", "USI", "H4sIAAAAAAAEALPxzEvKL81L8U0tLk5MT7Wz8UhNTEktsrNRjHZ2cQxxjHZUQAaGhgbGhiYGhnCmoamBhZGhAvEgNtbORh9miVN+SiXCKicDQ2NLA0NfD5PwIBJMBAHHcB9nD0//IJd4IwtLcxOIYLiBhYm5uaWBhamBIci1BiYIDX7+IQr+fgpunj6uCv5BCq4RAZ5Bri74LYlUUIC7zwAISPE2NgAOCkgQuOXnlyCHOlgKJqiPFkcAKm3JibQBAAA=", new Guid("4e08de2a-87f7-47b0-968d-1e20debfa3a2"), DateTime.Parse("2014-11-03 20:08:22.000"), DateTime.Parse("2014-11-03 20:09:22.000")},
					new object[] {"IJSUSAXLX", "USE", "H4sIAAAAAAAEAK1Ry2rDMBD8le0XRCv5CcawlhVb1JZbWW1jQg4JNr3F0LSH/n3dGJMSKM2he5xhd3ZmEn08jB/Hvh5Op/3rkCblsO+HtzS528qcHG0JpkEUPOYiRElW5VptHGfoITLxWDQZgIGbZ7dLk9Uiko3950VqOiQCwaIg9H3G1GWn0JmlypEFWapaS6rgpbH37R9SrUSDrMh0RbJ6ahmLeRBE0UITuYqM0xJkY55V19gZ5+Ah91FEjM8ukUH3TagW49CDKZG21A+1Mg4oz1X++webJSb0Ax7GV2z3z37P0c6Rrsfx/WeLZ2oBV1edfwEtN/3SBAIAAA==", new Guid("e7c9fe55-4c37-4848-8b61-e8cd4da4ebf8"), DateTime.Parse("2014-11-03 20:12:31.000"), DateTime.Parse("2014-11-03 20:13:31.000")},
					new object[] {"MA3SGFSGF", "USI", "H4sIAAAAAAAEAJWR0QqCMBiFX2U9gRvTRBjC75xlKcWSIkTCcHTnIOuit28amngR+bGL/Wdwzs8Zi+urftZVqpqmvCmfrVVZqbvPFjkPIYMc0BhCMCU2JsOVOGby0P8Uhc+sPiTQ1esbFRgnD2OQx00yw7ElBXpYReZciOtQh3ZiiInxWmJsU4+6rSDhFADfIp5AnCLgXOwzESIh5U6iSArxM+SM0LAfNpCZS07pqvhUEGn9GLfePfWiNfmjN42aaZW0AQAA", new Guid("308363d9-4ddc-47de-9ddf-d0310bff48a2"), DateTime.Parse("2014-11-03 20:40:22.000"), DateTime.Parse("2014-11-03 20:41:22.000")},
					new object[] {"UNIPIEFOA", "USI", "H4sIAAAAAAAEAJ1SXUvDQBD8K+sPkM7e5fIBIXCXD3o2XqomllKKVBJ8a8Dqg//eGA0WESHO4+6wMzu7sT0+9q/H9ro7nQ5PXRIvu0PbPSfxxS7NdK13ms7BDMkemP6N/T6JF5OI6du3bykD5hBsVpubeubUxtm1zYtKP3ie8L/sMbQvfBVARojUyFo5IaEEWEkgUJIlAu9ShJFSIQByf4lIaBpI7O50Y64AWd4vG7CcYxQTBo+RghACZGxZkk7TfF3nGRXVLVlnKpfNzIC2w8pTfh8Ss4z9gvFUnycq+v7l/CvG1lRc/Pihd0o3bgxUAgAA", new Guid("637b7690-190f-47ce-b360-2799c8256933"), DateTime.Parse("2014-11-03 20:41:59.000"), DateTime.Parse("2014-11-03 20:42:59.000")},
					new object[] {"FFOATLATL", "USI", "H4sIAAAAAAAEAM2UUWuDMBDHv0r2AUbvkosmIEK0lrqlOLQMutKHDqVvFdrtYd9+USd1Mkpp+7B/Xi4J97v8Ey5Bun+vP/flojoet7sqDObVtqwOYfCwjqdmadaGDYUIAqkNi4xdo80mDCZ9kaguv06lIsf3gczs6Ur2SEWG8AN0ZI1KKSEYIIpHoaUQhABx+neu5JKYMwtIeOJxiHMWmaWl1ABIt3/b+QgWTcC1zz2SHDy6lTdvAhJvzppy49JUaMQBX577CWmvyCR0D45c+lpGqbXM5Hn6mkzPwVaxsb8u0L1Bf75/5VfL+/vFoV9vyFN5YhNTnGc16nIAx+urO/dH24pdC87q+mPY9e1WvzgZ/RHfWs6LxjQEAAA=", new Guid("5af9d528-ef4e-437f-875b-3049aa04e5c2"), DateTime.Parse("2014-11-03 20:57:39.000"), DateTime.Parse("2014-11-03 20:58:39.000")},
					new object[] {"ABCDEFXYZ", "AMS", "H4sIAAAAAAAEALPxzEvKL81L8U0tLk5MT7Wz8UhNTEktsrNRjHZ2cQxxjA53VXDx91MPUQhwDAp2VXD0DVbwdQ0OdnR3DY6NtbPRh6l3yk+pJF4XRLVbfn4Jsl1gKZigPprLAKtAtLSqAAAA", new Guid("5e3a47bd-f20b-47ba-87e8-f7155315a8e1"), DateTime.Parse("2014-11-03 20:55:11.000"), DateTime.Parse("2014-11-03 20:56:11.000")},
					new object[] {"PROAGCHKG", "USI", "H4sIAAAAAAAEAJVR2wqCQBD9le0LnFXbDBZhXdcL5YXFHkIkDKU3F7Ie+vtM0STodp7mcpgzc4aGzVFdmyqq27Y81TYN6rKqzzZd5NxlGcsZmgMDWNgEPIXYMomB0e8oCptqo4ijqttTygG8NACvo510vg/SrZ7aBV2SyoT5PNj4Bx2AmAND9gSdAFmBCY+C4EnMw23IsjCJEeNcpJlwkZAykciTQnxU3CM07Qcd/jn7rRWDBZ5Sl7nrfWssai8/ugPrh+S/tAEAAA==", new Guid("51c5437e-75fd-43ed-9e04-f62fb7b0e700"), DateTime.Parse("2014-10-08 22:46:33.077"), DateTime.Parse("2014-10-08 22:47:33.077")},
					new object[] {"ACUORDHST", "USI", "H4sIAAAAAAAEAJVR0QqCQBD8le0L2r0uVDiEs4iCIil7iJCw3CIIL0576O+zLAvpIedpmYGZYVZNsp25ZumM8zw5sq/GnKRsfdXZDIY60hsN3yBEhyRSfQoSQrjwP+LYV913SGDS2ycqQOp5SF44C6ctHB/Qg9V8MRwvo62UPexXZFj5kaQ+onQDLOEJR7xUawrOCziczqfsCMl+z5eCU2BrjYWDZW6GrAHqfg8valny5xTVBCNTlvla/Sm9yW7jR3fZMCnftAEAAA==", new Guid("6fa3f45f-8691-4666-aaaf-945cf1b2ec17"), DateTime.Parse("2014-10-08 01:22:30.750"), DateTime.Parse("2014-10-08 01:23:30.750")},
					new object[] {"M1AM1AENT", "USI", "H4sIAAAAAAAEAKWSUWvCQAzHv8r5AcQkzd2uUArX84oF6wZXHzaRobT4ZmFOYd9+rVtdJ7oh/riHI7n8k1wSZdt1vd+WebXbrTZVHE2qVVm9xdFgYcemMAsj+gATogI8XRGQQIoLHG7mkkrDchlHo66spC4/fopLAKUEdNOpn12JvkaOpjluVrwiqBDpaPRp0w+gsYXLBAZDphCYNUAvENt0w4CJSepQsbSPfn72FlB3ejLJvX1xGtp/owe+sczf+JQgT0SbUSEFijVp+D/sD70QoG0986kw1rqnwo3v0HsW4jQPaLiv2+/Rf408rev3/l4eXZ1xdLbFn9reOFjWAgAA", new Guid("f3c861d3-558e-408b-8e33-a511d93e79b3"), DateTime.Parse("2016-04-21 21:00:26.883"), DateTime.Parse("2016-04-21 21:00:26.883") },
					new object[] {"PBSPHLPHL", "MAN", "H4sIAAAAAAAEAH1S22rDMAz9le0pD2JUvjROTCg4TnpjcbPEYaWlDx0NYzCa0W4P+/u5Cellg+nBts6RjmXJ0Wz/0nztd1l9PG5f61E0rbe7+jCK7tc6UVatN5tRNOjBuNl9X6jKxFCZhZIcdJw/KJ3K1QoC/VyedooUUTAiTwcIewNwcV5lJqCrskjL69RhWMz/S60MJBJZ7NKnV3AnJFtKVsaLJxkwDvm8tE4QkWDIuJfYDAijshcnAUHnsI7w2ZmgSIKWGNslqNkTAGTKEBEi92xigRAAZCCljG37WpgBSCLomW2NcN+FkGUuEh4uUSWCc5/73uNCA+VAh+hLITyjEtAKENFHQYaUS1eulxY5ELdqKA2yoCtFTZ1sUb/X22N9Zw9vH64P7kJ2aYUDUiA3/urGb6fZTXHcNJ9/Bt2Dg1/f4gex+CLdJwIAAA==", new Guid("82768C9E-06D4-4555-B7CA-4CDA9B180DCD"), DateTime.Parse("2016-04-21 21:00:26.883"), DateTime.Parse("2016-04-21 21:00:26.883") },
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(11));
			// Message 1
			AssertTransaction(transactions[0], DateTime.Parse("2014-11-03 20:09:53.000"), 1, "MTRMTLHST", null, null, "USC", "URR", "W86 02435296", "4101", "3918b567-6d63-4eaa-9bfd-4534605f6564", null, null, "HUB", DateTime.Parse("2014-11-03 20:08:53.000"), "3918b567-6d63-4eaa-9bfd-4534605f6564");
			AssertTransaction(transactions[1], DateTime.Parse("2014-11-03 20:09:53.000"), 1, "MTRMTLHST", null, null, "USC", "URR", "W86 02435320", "4101", "3918b567-6d63-4eaa-9bfd-4534605f6564", null, null, "HUB", DateTime.Parse("2014-11-03 20:08:53.000"), "3918b567-6d63-4eaa-9bfd-4534605f6564");
			// Message 2
			// Message 3
			AssertTransaction(transactions[2], DateTime.Parse("2014-11-03 20:13:31.000"), 1, "IJSUSAXLX", null, null, "USC", "USE", "e7c9fe55-4c37-4848-8b61-e8cd4da4ebf8", null, null, null, null, "HUB", DateTime.Parse("2014-11-03 20:12:31.000"), "e7c9fe55-4c37-4848-8b61-e8cd4da4ebf8");
			AssertTransaction(transactions[3], DateTime.Parse("2014-11-03 20:13:31.000"), 1, "IJSUSAXLX", null, null, "USC", "UXT", "X20141103156279", null, "e7c9fe55-4c37-4848-8b61-e8cd4da4ebf8", null, null, "HUB", DateTime.Parse("2014-11-03 20:12:31.000"), "e7c9fe55-4c37-4848-8b61-e8cd4da4ebf8");
			// Message 4
			AssertTransaction(transactions[4], DateTime.Parse("2014-11-03 20:41:22.000"), 1, "MA3SGFSGF", null, null, "USC", "UJL", "ARV60043937", null, "308363d9-4ddc-47de-9ddf-d0310bff48a2", null, null, "HUB", DateTime.Parse("2014-11-03 20:40:22.000"), "308363d9-4ddc-47de-9ddf-d0310bff48a2");
			// Message 5
			AssertTransaction(transactions[5], DateTime.Parse("2014-11-03 20:42:59.000"), 1, "UNIPIEFOA", null, null, "USC", "UQT", "657039095   ", null, "637b7690-190f-47ce-b360-2799c8256933", null, null, "HUB", DateTime.Parse("2014-11-03 20:41:59.000"), "637b7690-190f-47ce-b360-2799c8256933");
			// Message 6
			AssertTransaction(transactions[6], DateTime.Parse("2014-11-03 20:58:39.000"), 1, "FFOATLATL", null, null, "USC", "USO", "AFJ19188833", "1704", "5af9d528-ef4e-437f-875b-3049aa04e5c2", null, null, "HUB", DateTime.Parse("2014-11-03 20:57:39.000"), "5af9d528-ef4e-437f-875b-3049aa04e5c2");
			// Message 7
			AssertTransaction(transactions[7], DateTime.Parse("2014-11-03 20:56:11.000"), 1, "ABCDEFXYZ", null, null, "USC", "USA", "5e3a47bd-f20b-47ba-87e8-f7155315a8e1", null, null, null, null, "HUB", DateTime.Parse("2014-11-03 20:55:11.000"), "5e3a47bd-f20b-47ba-87e8-f7155315a8e1");
			// Message 8
			AssertTransaction(transactions[8], DateTime.Parse("2014-10-08 22:47:33.077"), 1, "PROAGCHKG", null, null, "USC", "URB", "9MU26067040", null, "51c5437e-75fd-43ed-9e04-f62fb7b0e700", null, null, "HUB", DateTime.Parse("2014-10-08 22:46:33.077"), "51c5437e-75fd-43ed-9e04-f62fb7b0e700");
			// Message 9
			AssertTransaction(transactions[9], DateTime.Parse("2014-10-08 01:23:30.750"), 1, "ACUORDHST", null, null, "USC", "UPL", "390114150048", null, "6fa3f45f-8691-4666-aaaf-945cf1b2ec17", null, null, "HUB", DateTime.Parse("2014-10-08 01:22:30.750"), "6fa3f45f-8691-4666-aaaf-945cf1b2ec17");
			// Message 10
			AssertTransaction(transactions[10], DateTime.Parse("2016-04-21 21:00:26.883"), 1, "M1AM1AENT", null, null, "USC", "ISF", "5501", "ELL-34242589645", "f3c861d3-558e-408b-8e33-a511d93e79b3", null, null, "HUB", DateTime.Parse("2016-04-21 21:00:26.883"), "f3c861d3-558e-408b-8e33-a511d93e79b3");
		}

		protected override void SetUpInternal()
		{
			base.SetUpInternal();
			mockPlugin.Object.UpdateSettings(new PluginSettings
			{
				Parameters = new[] {
				new PluginParameter("ConnectionString", "Test Connection String"),
				new PluginParameter("LogTransactionExceptions", "False")
				}
			});
		}

	}
}
