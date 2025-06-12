using System;

namespace CargoWise.eServices.Billing.WcfService.CircuitBreaker
{
	public abstract class CircuitBreakerState
	{
		protected readonly CircuitBreaker circuitBreaker;

		protected CircuitBreakerState(CircuitBreaker circuitBreaker)
		{
			this.circuitBreaker = circuitBreaker;
		}

		public virtual CircuitBreaker ProtectedCodeIsAboutToBeCalled()
		{
			return this.circuitBreaker;
		}
		public virtual void ProtectedCodeHasBeenCalled() { }
		public virtual void ActUponException(Exception e) { circuitBreaker.IncreaseFailureCount(); }

		public virtual CircuitBreakerState Update()
		{
			return this;
		}
	}

	public class OpenState : CircuitBreakerState
	{
		private readonly DateTime openDateTime;
		public OpenState(CircuitBreaker circuitBreaker)
			: base(circuitBreaker)
		{
			openDateTime = DateTime.UtcNow;
		}

		public override CircuitBreaker ProtectedCodeIsAboutToBeCalled()
		{
			base.ProtectedCodeIsAboutToBeCalled();
			this.Update();
			return base.circuitBreaker;
		}

		public override CircuitBreakerState Update()
		{
			base.Update();
			if (DateTime.UtcNow >= openDateTime + base.circuitBreaker.Timeout)
			{
				return circuitBreaker.MoveToHalfOpenState();
			}
			return this;
		}
	}

	public class HalfOpenState : CircuitBreakerState
	{
		public HalfOpenState(CircuitBreaker circuitBreaker) : base(circuitBreaker) { }

		public override void ActUponException(Exception e)
		{
			base.ActUponException(e);
			circuitBreaker.IncreaseRetryCount();
			circuitBreaker.MoveToOpenState();
		}

		public override void ProtectedCodeHasBeenCalled()
		{
			base.ProtectedCodeHasBeenCalled();
			circuitBreaker.MoveToClosedState();
		}
	}

	public class ClosedState : CircuitBreakerState
	{
		public ClosedState(CircuitBreaker circuitBreaker)
			: base(circuitBreaker)
		{
			circuitBreaker.ResetFailureCount();
		}

		public override void ActUponException(Exception e)
		{
			base.ActUponException(e);
			if (circuitBreaker.IsThresholdReached())
			{
				circuitBreaker.MoveToOpenState();
			}
		}
	}
}