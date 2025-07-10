using System;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class HttpContextExtensionsFixture
	{
		[Test]
		public void SetContext()
		{
			var context = new DefaultHttpContext();
			Assert.Throws(typeof(ArgumentNullException), () => context.SetContext(null));

			var repo = new Mock<IStagingRepository>();
			var itemsCount = context.Items.Count;
			context.SetContext(repo.Object);
			Assert.AreEqual(itemsCount + 1, context.Items.Count);
			var item = (IStagingRepository)context.Items[DB_Context];
			Assert.AreEqual(repo.Object, item);
		}

		[Test]
		public void GetContext()
		{
			var repo = new Mock<IStagingRepository>();
			var context = new DefaultHttpContext();
			context.SetContext(repo.Object);
			var result = context.GetContext();
			Assert.AreEqual(repo.Object, result);
		}

		const string DB_Context = "Batch_DbContext";
	}
}
