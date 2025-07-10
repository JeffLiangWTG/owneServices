using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using WTG.StaticAnalysis.Annotation;
using IncoTerms = Enterprise.Core.Constants.IncoTerms;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	public class IncotermValidation : IIncotermValidation
	{
		readonly static Lazy<IncotermValidation> instance = new Lazy<IncotermValidation>(() => new IncotermValidation(), true);
		public static IncotermValidation Instance => instance.Value;

		readonly Lazy<ReadOnlyCollection<string>> incotermsExpiringIn2011 = new Lazy<ReadOnlyCollection<string>>(() => Array.AsReadOnly(IncoTerms.Incoterms2000.Except(IncoTerms.Incoterms2010).ToArray()), true);
		public ReadOnlyCollection<string> IncotermsExpiringIn2011 => incotermsExpiringIn2011.Value;

		readonly Lazy<ReadOnlyCollection<string>> incotermsExpiringIn2020 = new Lazy<ReadOnlyCollection<string>>(() => Array.AsReadOnly(IncoTerms.Incoterms2010.Except(IncoTerms.Incoterms2020).ToArray()), true);
		public ReadOnlyCollection<string> IncotermsExpiringIn2020 => incotermsExpiringIn2020.Value;

		public void WarningIfExpired(ZPropertyInfo propertyInfo)
		{
			ActionIfExpired(propertyInfo, (message) => propertyInfo.AddWarning(message));
		}

		public void ErrorIfExpired(ZPropertyInfo propertyInfo)
		{
			ActionIfExpired(propertyInfo, (message) => propertyInfo.AddError(message));
		}

		void ActionIfExpired(ZPropertyInfo propertyInfo, Action<string> action)
		{
			Argument.NotNull(propertyInfo, "propertyInfo");

			var incoterm = (ZString)propertyInfo.Value;

			if (!propertyInfo.HasNotifications() && incoterm != ZString.Empty)
			{
				if (ZDateTime.Now >= IncoTerms.Incoterms2010EffectiveDate && IncotermsExpiringIn2011.Contains(incoterm))
				{
					action(Res.GetString("a094d034-b02a-48a3-b275-373a1f0c9600", "This Incoterm is obsolete from 1 January 2011 according to the Incoterms 2010 rules."));
				}
				else if (ZDateTime.Now >= IncoTerms.Incoterms2020EffectiveDate && IncotermsExpiringIn2020.Contains(incoterm))
				{
					action(Res.GetString("72a77097-19fb-4e49-9f4e-af39a35991e4", "This Incoterm is obsolete from 1 January 2020 according to the International Chamber of Commerce rules."));
				}
			}
		}
	}
}
