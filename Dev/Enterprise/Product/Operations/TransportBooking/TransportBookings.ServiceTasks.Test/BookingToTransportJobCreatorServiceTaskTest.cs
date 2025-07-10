using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.TransportBookings.ServiceTasks.Test
{
	[TestedType(typeof(BookingToTransportJobCreatorServiceTask))]
	class BookingToTransportJobCreatorServiceTaskTest : ServiceTaskTestCase<BookingToTransportJobCreatorServiceTask>
	{
		public void TestHostedServiceBindingAttribute()
		{
			AssertSingleHostedServiceAttribute<BookingToTransportJobCreatorServiceTask>(
				"TBC",
				"Transport Booking to Transport Jobs",
				"DOM",
				typeof(BookingToTransportJobCreatorServiceTask),
				"60Seconds",
				true
			);
		}

		void AssertSingleHostedServiceAttribute<T>(string expectedCode, string expectedDescription, string expectedCategory, Type expectedType, string expectedMinimumPeriod, bool expectedCanRunInAnyBranch)
			where T : ServiceProviderImpl
		{
			var serviceProvierType = typeof(T);
			var assembly = serviceProvierType.Assembly;
			var attributes = Array.ConvertAll(assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false), attribute => (HostedServiceAttribute)attribute);
			var hostedServiceAttribute = Array.FindAll(attributes, attribute => attribute.TypeName.Equals(serviceProvierType.FullName, StringComparison.Ordinal)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("Code", expectedCode, hostedServiceAttribute.Code);
				AssertEquals("Description", expectedDescription, hostedServiceAttribute.Description);
				AssertEquals("Category", expectedCategory, hostedServiceAttribute.Category);
				AssertEquals("Type", expectedType, hostedServiceAttribute.Type);
				AssertEquals("MinimumPeriod", expectedMinimumPeriod, hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", expectedCanRunInAnyBranch, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestServiceTask()
		{
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now))
			{
				var logger = new TestServiceLogger();
				var serviceTask = new BookingToTransportJobCreatorServiceTask { ServiceLogger = logger };
				serviceTask.RunTask();

				AssertEquals("logger.ToString()", "Information|Did not find any Transport Bookings to Create Transport Jobs from.\r\n", logger.ToString());
			}
		}

		public void TestDefaultSchedule()
		{
			AssertEquals("30minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestCheckEffectiveDateIsSet_HostServiceRequirementIsDefined()
		{
			var methodInfo = typeof(BookingToTransportJobCreatorServiceTask).GetMethod(nameof(BookingToTransportJobCreatorServiceTask.CheckEffectiveDateIsSet));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCheckEffectiveDateIsSet_ShouldReturnNoErrorMsg_WhenRegistryValueIsSet()
		{
			TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now);

			var message = BookingToTransportJobCreatorServiceTask.CheckEffectiveDateIsSet();
			AssertEquals(message, string.Empty);
		}

		public void TestCheckEffectiveDateIsSet_ShouldReturnErrorMsg_WhenRegistryValueIsNotSet()
		{
			var message = BookingToTransportJobCreatorServiceTask.CheckEffectiveDateIsSet();
			AssertEquals(
				string.Format(
					CultureInfo.InvariantCulture,
					"The registry setting '{0}' requires a value other than '{1}'.",
					TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.GetLocationInEnglish(),
					DateTime.MinValue.ToString(CultureInfo.InvariantCulture)),
				message);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
