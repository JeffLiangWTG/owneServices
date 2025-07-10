using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Licensing;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.MasterFiles.Business
{
	public sealed class StandardValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public StandardValueSource()
		{
			providers = new INumberGeneratorValueProvider[]
			{
				new NumberGeneratorValueProvider(Keys.BranchCode, BranchCode),
				new NumberGeneratorValueProvider(Keys.ClientCoded1, ClientCoded),
				new NumberGeneratorValueProvider(Keys.ClientCoded2, ClientCoded),
				new NumberGeneratorValueProvider(Keys.ClientCoded3, ClientCoded),
				new NumberGeneratorValueProvider(Keys.CompanyCode, CompanyCode),
				new NumberGeneratorValueProvider(Keys.EnterpriseCode, EnterpriseCode),
				new NumberGeneratorValueProvider(Keys.MonthAs2Digits, MonthAs2Digits),
				new NumberGeneratorValueProvider(Keys.MonthAsLetter, MonthAsLetter),
				new NumberGeneratorValueProvider(Keys.ServerCode, ServerCode),
				new NumberGeneratorValueProvider(Keys.UniversalOfficeCode, UniversalOfficeCode),
				new NumberGeneratorValueProvider(Keys.YearAsDigit, YearAsDigit),
				new NumberGeneratorValueProvider(Keys.YearAsLetter, YearAsLetter),
				new NumberGeneratorValueProvider(Keys.Quarter, Quarter),
			};
		}

		#region IEnumerable<INumberGeneratorValueProvider> Members

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return ((IEnumerable<INumberGeneratorValueProvider>)providers).GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		string BranchCode(NumberGenerator generator, string detail)
		{
			if (generator.Context == null || generator.Factory == null)
			{
				return null;
			}
			else
			{
				return generator.Context.BranchValue(generator.Factory, (b) => b.GB_Code.ToString());
			}
		}

		string ClientCoded(NumberGenerator generator, string detail)
		{
			var processor = ObjectFactory.Get<ITextMacroProcessor>();
			var result = processor.Replace(detail, new[] { generator.TargetBO });

			return result;
		}

		string CompanyCode(NumberGenerator generator, string detail)
		{
			if (generator.Context == null || generator.Factory == null)
			{
				return null;
			}
			else
			{
				return generator.Context.CompanyValue(generator.Factory, (co) => co.GC_Code.ToString());
			}
		}

		string EnterpriseCode(NumberGenerator generator, string detail)
		{
			if (generator.Context == null || generator.Factory == null)
			{
				return null;
			}
			else
			{
				return ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			}
		}

		string MonthAs2Digits(NumberGenerator generator, string detail)
		{
			return ZDateTime.Now.Month.ToString("00");
		}

		string MonthAsLetter(NumberGenerator generator, string detail)
		{
			return "ABCDEFGHIJKL"[ZDateTime.Now.Month - 1].ToString();
		}

		string ServerCode(NumberGenerator generator, string detail)
		{
			if (generator.Context == null || generator.Factory == null)
			{
				return null;
			}
			else
			{
				return ObjectFactory.Get<IProductRegistration>().Key.ServerCode;
			}
		}

		string UniversalOfficeCode(NumberGenerator generator, string detail)
		{
			if (generator.Context == null || generator.Factory == null)
			{
				return null;
			}
			else
			{
				string result = generator.Context.BranchValue(generator.Factory, (b) => GetUniversalOfficeCode(b.OrgProxy));

				if (string.IsNullOrEmpty(result))
				{
					result = generator.Context.CompanyValue(generator.Factory, (c) => GetUniversalOfficeCode(c.OrgProxy));
				}

				return result;
			}
		}

		string YearAsDigit(NumberGenerator generator, string detail)
		{
			return new ZString(ZDateTime.Now.Year.ToString()).Right(new ZInt(detail));
		}

		string YearAsLetter(NumberGenerator generator, string detail)
		{
			return "BCDEFGHIJKLMNOPQRSTUVWXYZA"[ZDateTime.Now.Year % 26].ToString();
		}
		string Quarter(NumberGenerator generator, string detail)
		{
			return new ZString("Q" + (ZDate.Today.Month + 2) / 3);
		}
		static string GetUniversalOfficeCode(OrgHeader proxy)
		{
			return proxy == null ? string.Empty : proxy.CustomsCodes.GetUOC().ToString();
		}

		readonly INumberGeneratorValueProvider[] providers;
	}
}
