using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	[Serializable]
	public class GenerateEntryNumberException : Exception
	{
		public GenerateEntryNumberException(string message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected GenerateEntryNumberException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public static string GenerateError => Res.GetString("4E918601-F2B1-4C66-A670-72A4A9E5C5AF", "Generate entry number error.");
	}

	public abstract class BaseEntryNumberGenerator
	{
		protected BaseEntryNumberGenerator(IEntryNumberGeneratorProvider provider)
		{
			Provider = provider;
		}

		protected IEntryNumberGeneratorProvider Provider { get; }

		public const string CustomsStmNumsType = "CUS";

		public abstract ZString CustomsBrokerageBoxNumber { get; }

		public abstract ZString EntryNumberType { get; }

		protected abstract BusinessObjectFactory Factory { get; }

		protected abstract ZString Category { get; }

		protected abstract ZString ShipmentType { get; }

		public abstract ZString Part1 { get; }

		protected int Part1_Length => 2;

		public abstract ZString Part2 { get; }

		protected int Part2_Length => 2;

		public abstract ZString Part3 { get; }

		public abstract ZString Part4 { get; }

		public abstract ZString Part4Caption { get; }

		public abstract int Part4_Length { get; }

		protected abstract GlbCompany Company { get; }

		public virtual bool IsAutoGenerateEntryNumberAllowed => true;

		public virtual ZString CannotAutoGenerateEntryNumberMessage { get; }

		public ZString GenerateEntryNumber()
		{
			var result = ZString.Empty;
			var prefix = GetEntryNumberPrefix();
			if (!prefix.IsEmpty)
			{
				var providerSequenceNumber = Provider.SequenceNumber;
				if (providerSequenceNumber.IsEmpty)
				{
					(var gotresult, var entrynumber) = GetRangedNumber();
					if (gotresult)
					{
						result = GenerateEntryNumber(entrynumber, prefix);
					}
				}
				else
				{
					result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", prefix, FormatSequenceNumber(providerSequenceNumber));
				}
			}
			return result;
		}

		ZString GenerateEntryNumber(string entrynumber, ZString prefix)
		{
			if (prefix.IsEmpty)
			{
				prefix = GetEntryNumberPrefix();
			}
			return !prefix.IsEmpty ? string.Format(CultureInfo.InvariantCulture, "{0}{1}", prefix, FormatIntToString(ZInt.ParseSafe(entrynumber, 0))) : string.Empty;
		}

		public ZString GetEntryNumberPrefix()
		{
			bool lengthCheck = Part1.Length == Part1_Length && Part2.Length == Part2_Length && Part4.Length == Part4_Length;
			var result = ZString.Empty;
			if (lengthCheck)
			{
				result = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", Part1, Part2, Part3, Part4);
			}
			return result;
		}

		public bool DoesEntryNumberFallIntoThisCategory(ZString number)
		{
			return Sequenceformatter.DoesEntryNumberFallIntoThisCategory(number);
		}

		public bool IsSequenceNumberMatchEntryNumber(ZString entryNumber, ZString providerSequenceNumber)
		{
			return Sequenceformatter.IsSequenceNumberMatchEntryNumber(entryNumber, providerSequenceNumber);
		}

		public ZString FountainName => ZString.Format("TWEntryNum_{0}_{1}", ShipmentType, Category);

		(bool, string) GetRangedNumber()
		{
			var company = Company;
			var provider = company.CustomsNumberProvider;

			var wrapper = GetustomsNumberViewStmNumsWrapper(provider);
			if (provider != null && wrapper == null)
			{
				wrapper = AddDefaultNumberRange(provider, company.PK, 1L, this.Sequenceformatter.MaximumValue);
			}
			if (wrapper != null)
			{
				wrapper.EntryNumberGenerator = this;
			}
			var result = provider.TryGetNextCustomsNumber(company.Factory, new[] { wrapper }, out var resultNumber);
			return (result, resultNumber);
		}

		TWCustomsNumberViewStmNumsWrapper AddDefaultNumberRange(CustomsNumberViewStmNumsBusinessProvider provider, ZGuid ownerPK, ZLong minimumValue, ZLong maximumValue)
		{
			var range = AddRange(ownerPK, minimumValue, maximumValue, provider);
			var stmNums = provider.CustomsNumbers.OfType<CustomsNumberViewStmNums>().FirstOrDefault(x => x.SN_Name == range.StmNums.SN_Name && x.SN_Owner == range.StmNums.SN_Owner);
			if (stmNums != null)
			{
				provider.CustomsNumberWrappers.Add(stmNums);
			}
			return (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
		}

		TWCustomsNumberViewStmNumsWrapper GetustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNumsBusinessProvider provider)
		{
			return provider?.CustomsNumberWrappers?.OfType<TWCustomsNumberViewStmNumsWrapper>().FirstOrDefault(x => x.SN_Type == CustomsStmNumsType && x.SN_FountainName == FountainName);
		}

		TWCustomsNumberViewStmNumsWrapper AddRange(ZGuid ownerPK, ZLong minimumValue, ZLong maximumValue, CustomsNumberViewStmNumsBusinessProvider currentProvider)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var provider = newFactory.Load<GlbCompany>(ownerPK).CustomsNumberProvider;
			var stmNums = provider.CustomsNumbers.AddNew();
			provider.CustomsNumberWrappers.Add(stmNums);
			var wrapper = (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.SN_Type = CustomsStmNumsType;
			wrapper.SN_FountainName = this.FountainName;
			stmNums.SN_Owner = ownerPK;
			stmNums.SN_MinimumValue = minimumValue;
			stmNums.SN_MaximumValue = maximumValue;
			try
			{
				stmNums.Factory.Save();
				currentProvider.CustomsNumbers.RefreshFromDb();
				currentProvider.NumberRanges.RefreshFromDb();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			return wrapper;
		}

		protected string FormatIntToString(int nextNumber)
		{
			return Sequenceformatter.FormatIntToString(nextNumber);
		}

		protected string FormatSequenceNumber(ZString sequenceNumber)
		{
			return Sequenceformatter.FormatSequenceNumber(sequenceNumber);
		}

		public ITWSequenceformatter Sequenceformatter => sequenceformatter ?? (sequenceformatter = GetSequenceformatter());

		ITWSequenceformatter sequenceformatter;

		protected abstract ITWSequenceformatter GetSequenceformatter();

		internal bool ExistingEntry(string nextNumber) => ExistingEntry(GenerateEntryNumber(nextNumber, ZString.Empty));

		internal bool ExistingEntry(ZString entryNumber)
		{
			return !entryNumber.IsEmpty && (CusEntryNumber.Load(Factory, EntryNumberType, entryNumber, Core.Constants.CountryCodes.Taiwan, true)?.Any() ?? false);
		}

		internal ZString CheckNextNumberIsInRange(int nextNumber, bool allowOutrange = false)
		{
			var wrapper = GetustomsNumberViewStmNumsWrapper(Company?.CustomsNumberProvider);
			var result = ZString.Empty;
			ZLong minimumValue = 1L;
			ZLong maximumValue = Sequenceformatter.MaximumValue;
			ZLong currentValue = 1L;
			if (wrapper != null)
			{
				currentValue = wrapper.StmNums.SN_Value;
				minimumValue = wrapper.SN_MinimumValue;
				maximumValue = wrapper.SN_MaximumValue;
			}
			if (nextNumber < 0)
			{
				if (currentValue == maximumValue)
				{
					result = Res.GetString("25BA0B2A-8360-407D-9C05-EAED56618CDE", "All sequential numbers for the current entry number range have been used up. Please visit Maintain > User Admin > Companies > {0} - {1} > Number Ranges to maintain the number ranges.", Company.GC_Code, Company.CompanyName);
				}
			}
			else if (!allowOutrange && (nextNumber < minimumValue || nextNumber > maximumValue))
			{
				result = Res.GetString("17344733-8EB0-432E-A76E-6C288DCA8027", "The number is not in the range (Owner: '{0}', Range Type: {1}, Message Type: {2})", wrapper?.SN_OwnerForDisplay ?? ZString.Empty, Category, ShipmentType);
			}
			return result;
		}

		public IEnumerable<ZPropertyInfo> ValidationPropertyInfos => GetValidationPropertyInfosCore().WhereNotNull();

		protected virtual IEnumerable<ZPropertyInfo> GetValidationPropertyInfosCore() => Enumerable.Empty<ZPropertyInfo>();

		public ZString AllowedGeneratorDescription
		{
			get
			{
				var prefix = GetEntryNumberPrefix();
				var result = ZString.Empty;
				var infos = ValidationPropertyInfos;
				if (prefix.IsEmpty && infos != null)
				{
					var errorMessages = new ZStringBuilder();
					foreach (var info in infos)
					{
						if (info.HasMessageErrors())
						{
							info.GetMessageErrors().ToList().ForEach(x => errorMessages.Append(ZString.Format("{0}:{1}", info.HumanReadableName, x.Message)));
						}
					}
					result = errorMessages.Length > 0 ? errorMessages.ToStringWithNewLineBetweenAppends() : string.Empty;
				}
				return result;
			}
		}
	}
}
