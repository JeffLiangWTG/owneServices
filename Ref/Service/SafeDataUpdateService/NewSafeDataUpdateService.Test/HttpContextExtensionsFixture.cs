using System;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class HttpContextExtensionsFixture
	{
		[Test]
		public void ShouldReportIssue()
		{
			var context = new DefaultHttpContext();
			Assert.False(context.ShouldReportIssue());

			context.Request.Headers["ReportIssue"] = "true";
			Assert.True(context.ShouldReportIssue());

			context.Request.Headers["ReportIssue"] = "false";
			Assert.False(context.ShouldReportIssue());
		}

		[Test]
		public void SetContext()
		{
			var context = new DefaultHttpContext();
			Assert.Throws(typeof(ArgumentNullException), () => context.SetContext(null, false));

			var repo = new Mock<IReferenceDataRepository>();
			var itemsCount = context.Items.Count;
			context.SetContext(repo.Object, true);
			Assert.AreEqual(itemsCount + 1, context.Items.Count);
			var item = (Tuple<IReferenceDataRepository, bool>)context.Items[DB_Context];
			Assert.AreEqual(repo.Object, item.Item1);
			Assert.True(item.Item2);
		}

		[Test]
		public void GetContext()
		{
			var context = new DefaultHttpContext();
			var repo = new Mock<IReferenceDataRepository>();
			context.SetContext(repo.Object, true);
			var result = context.GetContext();
			Assert.AreEqual(repo.Object, result.Item1);
			Assert.True(result.Item2);

			context.Items.Remove(DB_Context);
			Assert.Throws(typeof(ArgumentNullException), () => context.GetContext());

			var serviceProvider = new Mock<IServiceProvider>();
			var repo1 = new Mock<IReferenceDataRepository>();
			serviceProvider.Setup(x => x.GetService(typeof(IReferenceDataRepository))).Returns(repo1.Object);
			context.RequestServices = serviceProvider.Object;
			result = context.GetContext();
			Assert.AreEqual(repo1.Object, result.Item1);
			Assert.False(result.Item2);
		}

		const string DB_Context = "Batch_DbContext";
	}
}
