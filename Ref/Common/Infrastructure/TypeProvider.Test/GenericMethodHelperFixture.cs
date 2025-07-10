using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.TypeProvider.Test
{
	[TestFixture]
	class GenericMethodHelperFixture
	{
		[Test]
		public void InvokeGenericMethod()
		{
			Assert.AreEqual(0, new Dummy().InvokeGenericMethod(nameof(Dummy.GetDefault), typeof(int)));
			Assert.IsNull(new Dummy().InvokeGenericMethod(nameof(Dummy.GetDefault), typeof(string)));
		}

		[Test]
		public void InvokeStaticGenericMethod()
		{
			Assert.AreEqual(0, typeof(Dummy).InvokeStaticGenericMethod(nameof(Dummy.GetStaticDefault), typeof(int)));
			Assert.IsNull(typeof(Dummy).InvokeStaticGenericMethod(nameof(Dummy.GetStaticDefault), typeof(string)));
		}

		[Test]
		public void TryParseExact()
		{
			DateTime parsedDateTime;
			bool success = "Some Shit (c) David James".TryParseExact(out parsedDateTime, "ddMMyy");
			Assert.AreEqual(false, success);
			Assert.AreEqual(DateTime.MinValue, parsedDateTime);

			success = "2004-12-17T09:30:47".TryParseExact(out parsedDateTime, "adfdasfsafasdfa");
			Assert.AreEqual(false, success);
			Assert.AreEqual(DateTime.MinValue, parsedDateTime);

			success = "250198".TryParseExact(out parsedDateTime, "ddMMyy");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "982501".TryParseExact(out parsedDateTime, "yyddMM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "25/01/98".TryParseExact(out parsedDateTime, "dd/MM/yy");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "25/98/01".TryParseExact(out parsedDateTime, "dd/yy/MM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "98/25/01".TryParseExact(out parsedDateTime, "yy/dd/MM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "25/01/2098".TryParseExact(out parsedDateTime, "dd/MM/yyyy");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(2098, 01, 25), parsedDateTime);

			success = "25/2098/01".TryParseExact(out parsedDateTime, "dd/yyyy/MM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(2098, 01, 25), parsedDateTime);

			success = "2098/25/01".TryParseExact(out parsedDateTime, "yyyy/dd/MM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(2098, 01, 25), parsedDateTime);

			success = "25/01/98".TryParseExact(out parsedDateTime, "dd/MM/yy");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "25-98-01".TryParseExact(out parsedDateTime, "dd-yy-MM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "98-25-01".TryParseExact(out parsedDateTime, "yy-dd-MM");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(1998, 01, 25), parsedDateTime);

			success = "2004-12-17".TryParseExact(out parsedDateTime, "yyyy-MM-dd");
			Assert.AreEqual(true, success);
			Assert.AreEqual(new DateTime(2004, 12, 17), parsedDateTime);

			success = "".TryParseExact(out parsedDateTime, "yyyy-MM-dd");
			Assert.AreEqual(true, success);
			Assert.AreEqual(DateTime.MinValue, parsedDateTime);
		}
	}
}
