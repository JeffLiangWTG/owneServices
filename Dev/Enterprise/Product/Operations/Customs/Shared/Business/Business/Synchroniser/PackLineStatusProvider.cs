using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	class PackLineStatusProvider : IPackLineStatusProvider
	{
		public IPackLineStatus GetPackLineStatus(BusinessObjectFactory factory, ZString countryCode)
		{
			IPackLineStatus result = null;
			if (countryCode == Core.Constants.CountryCodes.Australia)
			{
				result = (IPackLineStatus)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.AU.IPackLineStatus>());
			}
			else if (countryCode == Core.Constants.CountryCodes.NewZealand)
			{
				result = (IPackLineStatus)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.NZ.IPackLineStatus>(), new object[] { factory });
			}
			return result;
		}
	}
}
