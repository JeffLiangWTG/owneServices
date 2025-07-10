using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public struct RegistrationNumber
	{
		public ZString Number;
		public ZString NumberType;
	}

	public delegate RegistrationNumber RegistrationNumberReturner();

	public class RegistrationNumberResult
	{
		public RegistrationNumberResult(BusinessObjectFactory factory, bool isApplicable, RegistrationNumberReturner registrationNumberDelegate)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			IsApplicable = isApplicable;
			if (isApplicable && registrationNumberDelegate == null)
			{
				throw new ArgumentNullException(nameof(registrationNumberDelegate));
			}
			this.registrationNumberDelegate = registrationNumberDelegate;
		}

		public readonly bool IsApplicable;
		readonly RegistrationNumberReturner registrationNumberDelegate;
		readonly BusinessObjectFactory factory;

		public ZString RegistrationNumber
		{
			get
			{
				if (RegistrationNumberCached == null)
				{
					RegistrationNumberCached = new CachedProperty<ZString>(factory, () => IsApplicable ? registrationNumberDelegate().Number : ZString.Empty);
				}
				return RegistrationNumberCached.Value;
			}
		}
		CachedProperty<ZString> RegistrationNumberCached;

		public ZString RegistrationNumberType
		{
			get
			{
				if (RegistrationNumberTypeCached == null)
				{
					RegistrationNumberTypeCached = new CachedProperty<ZString>(factory, () => IsApplicable ? registrationNumberDelegate().NumberType : ZString.Empty);
				}
				return RegistrationNumberTypeCached.Value;
			}
		}
		CachedProperty<ZString> RegistrationNumberTypeCached;
	}
}
