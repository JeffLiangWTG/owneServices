using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class MessageValidationStrategy
	{
		protected MessageValidationStrategy()
		{
			IsEnabled = true;
		}

		public virtual bool IsApplicable
		{
			get
			{
				return true;
			}
		}

		public bool IsEnabled { get; set; }

		public abstract MultilingualString Name { get; }

		[System.Diagnostics.DebuggerStepThrough]
		public abstract void Register(BusinessObjectFactory factory);

		[System.Diagnostics.DebuggerStepThrough]
		public abstract void Unregister(BusinessObjectFactory factory);

		[System.Diagnostics.DebuggerStepThrough]
		public abstract bool IsRegistered(BusinessObjectFactory factory);
	}
}
