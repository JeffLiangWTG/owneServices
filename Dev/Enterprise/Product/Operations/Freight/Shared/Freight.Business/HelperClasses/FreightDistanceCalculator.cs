using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.DistanceCalculation.Business;
using Enterprise.Freight.DistanceCalculation.Integration;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Business
{
	public class FreightDistanceCalculator
	{
		public FreightDistanceCalculator(IDistanceCalculationConsumer consumer, INotifications notification)
		{
			Argument.NotNull(consumer, "consumer");
			Argument.NotNull(notification, "notification");

			Consumer = consumer;
			Notification = notification;
		}

		public void SetCalculatedDistance()
		{
			if (Consumer.OriginAddress != null && Consumer.DestinationAddress != null)
			{
				DistanceCalculationManager manager = new DistanceCalculationManager();

				DistanceCalculationResult result = manager.Calculate(Consumer.Checkpoint, Guid.NewGuid(), DistanceCalculationConfig, Consumer.OriginAddress, Consumer.DestinationAddress);

				if (string.IsNullOrEmpty(result.StatusMessage))
				{
					ZString resultUnit = result.DistanceUnit == DistanceCalculationConstants.UnitsForCalculation.Miles ? Constants.Length.Miles : Constants.Length.Kilometres;
					bool isConsumerUnitValid = Constants.Length.ContainsCode(Consumer.DistanceUnit);

					if (isConsumerUnitValid)
					{
						Consumer.Distance = Constants.Length.Convert((ZDecimal)result.Distance, resultUnit, Consumer.DistanceUnit);
					}
					else
					{
						Consumer.Distance = result.Distance;
						Consumer.DistanceUnit = resultUnit;
					}
				}
				else
				{
					Notification.Notify(new WarningNotification(result.StatusMessage));
				}
			}
		}

		#region Implementation

		IDistanceCalculationConsumer Consumer
		{ get; set; }

		INotifications Notification
		{ get; set; }

		DistanceCalculationConfiguration DistanceCalculationConfig
		{
			get
			{
				DistanceCalculationConfiguration result = Consumer.DistanceCalculationConfig;

				if (!Consumer.DistanceUnit.IsEmpty)
				{
					result.UnitsForCalculation = (Consumer.DistanceUnit == Constants.Length.Miles) ? DistanceCalculationConstants.UnitsForCalculation.Miles : DistanceCalculationConstants.UnitsForCalculation.Kilometres;
				}
				else
				{
					result.UnitsForCalculation = DistanceCalculationConstants.UnitsForCalculation.Default;
				}

				return result;
			}
		}

		#endregion
	}
}

#region Testing
#if DEBUG

namespace Enterprise.Freight.Business
{
	using Enterprise.Security;

	class Consumer : IDistanceCalculationConsumer
	{
		#region IDistanceCalculationConsumer Members

		SecurityCheckpoint IDistanceCalculationConsumer.Checkpoint
		{
			get { throw new NotImplementedException(); }
		}

		ZDecimal IDistanceCalculationConsumer.Distance
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		ZString IDistanceCalculationConsumer.DistanceUnit
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		DistanceCalculationConfiguration IDistanceCalculationConsumer.DistanceCalculationConfig
		{
			get { throw new NotImplementedException(); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.OriginAddress
		{
			get { throw new NotImplementedException(); }
		}

		DistanceCalculationAddress IDistanceCalculationConsumer.DestinationAddress
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}

#endif
#endregion
