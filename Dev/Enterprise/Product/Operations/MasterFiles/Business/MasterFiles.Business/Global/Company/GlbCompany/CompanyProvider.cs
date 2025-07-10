using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.FrameworkExtensions.Functional;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Variation of IGlbCompanyProvider which returns readonly ICompany objects.
	/// Prefer this type if you do not need to modify a company object.
	/// </summary>
	public class CompanyProvider : ICompanyProvider
	{
		public Option<ICompany> Get(ZGuid pk)
		{
			var result = (ICompany)Factory.Load<GlbCompany>(pk);
			return result.ToOption();
		}

		public BusinessObjectFactory Factory
		{
			get => factory ?? throw new InvalidOperationException("Factory has not been initialized. You must set the Factory property or use WithFactory() extension method. Sorry, using constructors is rather painful with ObjectFactory; please pass a factory.");
			set
			{
				if (factory != null)
				{
					throw new InvalidOperationException("Factory has already been initialized and cannot be overridden. If you want to provide an external Factory, it must be done before any other methods are called.");
				}
				factory = value;
			}
		}
		BusinessObjectFactory factory;
	}

	public static class CompanyOptionExtensions
	{
		/// <summary>
		/// Safely reads the country code for an ICompany.
		/// </summary>
		/// <returns>Country code or None</returns>
		public static Option<string> GetCountryCode(this Option<ICompany> company)
			=> company.Map(company => company.Country).Map(country => country.Code);
	}
}
