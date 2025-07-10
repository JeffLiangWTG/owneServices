using System.Threading;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"CIS",
	"CargoIMP Message Sender",
	"CIM",
	typeof(Enterprise.Freight.Forwarding.ServiceTasks.CargoIMP.CargoIMPSenderServiceTask),
	MinimumPeriod = "1minutes",
	MaximumPeriod = "15minutes",
	DefaultScheduleRunEvery = "15minutes",
	CanRunInAnyBranch = true,
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	"CIS",
	EDIInterchangeSchema.Constants.TableName,
	new[]
	{
		EDIInterchangeSchema.Constants.EI_Status            + "=" + EDIMessage.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit   + "=" + EDIInterchange.Direction.Transmit,
		EDIInterchangeSchema.Constants.EI_IsActive          + "=" + "Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode   + "=" + EDIInterchange.ApplicationCodes.CIM
	},
	"Cargo IMP")]

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMP
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	internal class CargoIMPSenderServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			foreach (var companyCode in DisposableEnvironment.GetActiveCompanies())
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForCompany(companyCode))
				{
					CargoIMPMessageSender.New(ServiceLogger).Process(youMustReactToThisToken);
				}
			}
		}
	}
}
