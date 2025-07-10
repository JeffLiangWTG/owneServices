using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public sealed class NumberGenerator : ICustomizableNumberFormatter
	{
		public NumberGenerator()
		{
			ValueProviders = new NumberGeneratorValueProviderCollection();
			AdditionalTargets = new List<NumberGeneratorTarget>();
		}

		public delegate INumberFountainProxy FountainGetterDelegate(string fountainKey);
		public delegate INumberFountainProxy CompanyFountainGetterDelegate(string fountainKey, Guid companyPK);

		public void Generate()
		{
			ValidateGeneratorComponentsAreSet();

			if (PrimaryTarget == null)
			{
				throw new InvalidOperationException("PrimaryTarget not set yet");
			}

			ResetGeneratorTargetValues(PrimaryTarget, Context);
			var fountainKey = GetFountainKey(PrimaryTarget.NumberCustomisation);
			var fountain = GetFountainProxy(fountainKey);
			var seed = fountain.GetNext(Factory);
			var prefix = BaseFountain.Prefix;
			GeneratePrimaryTarget(seed, fountain, prefix);

			GenerateForAdditionTarget(seed, fountainKey, prefix);
		}

		void GeneratePrimaryTarget(long seed, INumberFountainProxy fountain, string prefix)
		{
			PrimaryTarget.FountainUsedForGeneration = fountain;
			Generate(PrimaryTarget, seed, prefix);

			if (PrimaryTarget.Value == prefix)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "PrimaryTarget.Value was set to prefix '{0}' only", prefix));
			}
		}

		void GenerateForAdditionTarget(long seed, string fountainKey, string prefix)
		{
			foreach (var target in AdditionalTargets)
			{
				ResetGeneratorTargetValues(target, Context);
				var additionalSeed = seed;
				target.FountainUsedForGeneration = PrimaryTarget.FountainUsedForGeneration;
				if (target.NumberCustomisation == null)
				{
					continue;
				}

				if (!target.NumberCustomisation.UseShipmentSequenceNumber)
				{
					var newFountainKey = GetFountainKey(target.NumberCustomisation);
					if (newFountainKey != fountainKey)
					{
						target.FountainUsedForGeneration = GetAdditionalTargetFountainProxy(newFountainKey);
						additionalSeed = target.FountainUsedForGeneration.GetNext(Factory);
					}
				}

				Generate(target, additionalSeed, prefix);
			}
		}

		INumberFountainProxy GetFountainProxy(string fountainKey)
		{
			INumberFountainProxy fountain;
			if (string.IsNullOrEmpty(fountainKey))
			{
				fountain = BaseFountain;
			}
			else
			{
				if (CompanyFountainGetter != null)
				{
					fountain = CompanyFountainGetter(fountainKey, GlbCompany.CurrentCompany.PK.ToGuid());
				}
				else
				{
					fountain = FountainGetter(fountainKey);
				}
			}

			return fountain;
		}

		INumberFountainProxy GetAdditionalTargetFountainProxy(string fountainKey)
		{
			if (string.IsNullOrEmpty(fountainKey))
			{
				return BaseFountain;
			}
			
			return FountainGetter(fountainKey);
		}

		static void ResetGeneratorTargetValues(NumberGeneratorTarget target, NumberGeneratorContext context)
		{
			target.Context = context;
			target.Value = ZString.Empty;
			target.FountainValue = ZString.Empty;
			target.ValuePrefix = ZString.Empty;
			target.ValueSuffix = ZString.Empty;
		}

		void ValidateGeneratorComponentsAreSet()
		{
			if (Factory == null)
			{
				throw new InvalidOperationException("Factory not set yet");
			}

			if (Context == null)
			{
				throw new InvalidOperationException("Context not set yet");
			}

			if (BaseFountain == null)
			{
				throw new InvalidOperationException("BaseFountain not set yet");
			}

			if (FountainGetter == null && CompanyFountainGetter == null)
			{
				throw new InvalidOperationException("FountainGetter not set yet");
			}
		}

		public void EnforceMaxLengths()
		{
			EnforceMaxLength(PrimaryTarget.NumberCustomisationLocation, PrimaryTarget.Name, PrimaryTarget.Value, PrimaryTarget.MaxLength);
			foreach (var target in AdditionalTargets)
			{
				EnforceMaxLength(target.NumberCustomisationLocation, target.Name, target.Value, target.MaxLength);
			}
		}

		string GetFountainKey(BillOfLadingNumberCustomisation customisation)
		{
			customisation.Elements.Sort(BillOfLadingNumberCustomisationElement.Schema.Key);

			var builder = new StringBuilder();
			foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
			{
				if (element.Fountain)
				{
					builder.Append(GetValue(element, 0));
				}
			}

			return builder.ToString();
		}

		void Generate(NumberGeneratorTarget target, long seed, string prefix)
		{
			var value = GenerateNumber(target.NumberCustomisation, seed, out var generatedPrefix, out var sequenceNumber, out var generatedSuffix);
			target.FountainValue = sequenceNumber;
			target.ValueSuffix = generatedSuffix;
			if (target.NumberCustomisation.RemoveFountainPrefix)
			{
				target.Value = value;
				target.ValuePrefix = generatedPrefix;
			}
			else
			{
				target.Value = prefix + value;
				target.ValuePrefix = prefix + generatedPrefix;
			}
			if (string.IsNullOrEmpty(value))
			{
				var message = System.FormattableString.Invariant($@"Context:
Company=[{target.Context.CompanyValue(Factory, (c) => c.GC_Code)}='{target.Context.CompanyValue(Factory, (c) => c.PK)}']
Branch=[{target.Context.BranchValue(Factory, (c) => c.GB_Code)}='{target.Context.BranchValue(Factory, (c) => c.PK)}']
Department=[{target.Context.DepartmentValue(Factory, (c) => c.GE_Code)}='{target.Context.DepartmentValue(Factory, (c) => c.PK)}']

Value='{target.Value}'
Name='{target.Name}'
MaxLength='{target.MaxLength}'
NumberCustomisationLocation='{target.NumberCustomisationLocation}'
FountainUsedForGeneration='{target.FountainUsedForGeneration.ToString()}'
Prefix='{prefix}'
Seed='{seed}'
ValuePrefix='{target.ValuePrefix}'
ValueSuffix='{target.ValueSuffix}'
FountainValue='{target.FountainValue}'
Elements.Categories='{target.NumberCustomisation.Categories}'
");
				ErrorReporter.ReportOnce(System.FormattableString.Invariant($"Invalid NumberGeneratorTarget ({target.Name}, {target.MaxLength}, {target.FountainValue})"), message);
			}
		}

		string GenerateNumber(BillOfLadingNumberCustomisation customisation, long seed, out string prefix, out string sequenceNumber, out string suffix)
		{
			customisation.Elements.Sort(BillOfLadingNumberCustomisationElement.Schema.Order);
			prefix = string.Empty;
			sequenceNumber = string.Empty;
			suffix = string.Empty;

			var builder = new StringBuilder();
			var checkDigitBuilder = new StringBuilder();
			var suffixBuilder = new StringBuilder();
			var buildSuffix = false;
			foreach (BillOfLadingNumberCustomisationElement element in customisation.Elements)
			{
				if (element.Include)
				{
					var value = GetValue(element, seed);

					if (buildSuffix)
					{
						suffixBuilder.Append(value);
					}

					if (element.Key == BillOfLadingNumberCustomisationElement.Keys.SequenceNumber)
					{
						sequenceNumber = value;
						prefix = builder.ToString();
						buildSuffix = true;
					}

					builder.Append(value);
					if (element.CheckDigit)
					{
						checkDigitBuilder.Append(value);
					}
				}
			}

			var expectedCheck = NumberFountain.CheckDigitHelper.CalculateCheckDigit(customisation.CheckDigitAlgorithm, checkDigitBuilder.ToString());
			builder.Append(expectedCheck);

			suffix = suffixBuilder.ToString();
			return builder.ToString();
		}

		string GetValue(BillOfLadingNumberCustomisationElement element, long seed)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (element.Key == BillOfLadingNumberCustomisationElement.Keys.SequenceNumber)
			{
				return seed.ToString("D" + element.Detail, CultureInfo.InvariantCulture);
			}
			else
			{
				var provider = ValueProviders[element.Key]
					?? throw new ArgumentOutOfRangeException("element.Key", element.Key, "unknown key");

				return provider.GetValue(this, element.Detail);
			}
		}

		public IEnumerable<ZString> GenerateNumbers(NumberGeneratorTarget target, int quantity, bool enforceMaxLength = false)
		{
			Argument.NotNull(target, nameof(target));
			ValidateGeneratorComponentsAreSet();
			ResetGeneratorTargetValues(target, Context);
			var fountainKey = GetFountainKey(target.NumberCustomisation);

			var prefix = target.NumberCustomisation.RemoveFountainPrefix ? string.Empty : BaseFountain.Prefix;
			var fountain = GetFountainProxy(fountainKey);

			var seeds = fountain.GetNexts(Factory, quantity);
			if (seeds.Length != quantity)
			{
				throw new InvalidOperationException("Generated seeds count is not the same as requested quantity.");
			}

			target.FountainUsedForGeneration = fountain;

			var generatedNumbers = new HashSet<ZString>();
			foreach (var seed in seeds)
			{
				var value = GenerateNumber(target.NumberCustomisation, seed, out var generatedPrefix, out var sequenceNumber, out var generatedSuffix);
				if (string.IsNullOrEmpty(value))
				{
					throw new InvalidOperationException("Generated number is empty.");
				}

				var generatedNumber = prefix + value;
				if (enforceMaxLength)
				{
					EnforceMaxLength(target.NumberCustomisationLocation, target.Name, generatedNumber, target.MaxLength);
				}

				generatedNumbers.Add(generatedNumber);
			}

			return generatedNumbers;
		}

		#region Static Methods

		static void EnforceMaxLength(string location, string name, string value, int maxLength)
		{
			var overLengthTemplate = Res.GetString("684f5cae-6b3e-4783-b3b8-9d4b0f3d0a44", "Generated a {0} that is too big to fit in the available space.\r\n('{1}')\r\n\r\nTo fix this you need to change the following registry option to generate shorter numbers.\r\n{2}", name, value, location);

			if (value.Length > maxLength)
			{
				throw new GeneratedOverLengthCodeException(overLengthTemplate);
			}
		}

		#endregion

		#region ICustomizableNumberFormatter

		INumberFountainProxy ICustomizableNumberFormatter.GetFountainProxy()
		{
			if (PrimaryTarget == null)
			{
				throw new InvalidOperationException("PrimaryTarget not set yet");
			}

			if (AdditionalTargets.Count > 0)
			{
				throw new InvalidOperationException("AdditionalTarget invalid for Customisable Number Fountain.");
			}

			ResetGeneratorTargetValues(PrimaryTarget, Context);
			ValidateGeneratorComponentsAreSet();

			var fountainKey = GetFountainKey(PrimaryTarget.NumberCustomisation);
			return GetFountainProxy(fountainKey);
		}

		string ICustomizableNumberFormatter.GetFormattedNumber(long seed, INumberFountainProxy fountain)
		{
			if (PrimaryTarget == null)
			{
				throw new InvalidOperationException("PrimaryTarget not set yet");
			}

			if (AdditionalTargets.Count > 0)
			{
				throw new InvalidOperationException("AdditionalTarget invalid for Customisable Number Fountain.");
			}

			ResetGeneratorTargetValues(PrimaryTarget, Context);
			GeneratePrimaryTarget(seed, fountain, BaseFountain.Prefix);

			EnforceMaxLength(PrimaryTarget.NumberCustomisationLocation, PrimaryTarget.Name, PrimaryTarget.Value, PrimaryTarget.MaxLength);

			return PrimaryTarget.Value;
		}

		#endregion

		public FountainGetterDelegate FountainGetter { get; set; }
		public CompanyFountainGetterDelegate CompanyFountainGetter { get; set; }
		public BusinessObjectFactory Factory { get; set; }
		public INumberFountainProxy BaseFountain { get; set; }
		public NumberGeneratorContext Context { get; set; }

		public BusinessObject TargetBO { get; set; }

		public NumberGeneratorTarget PrimaryTarget { get; set; }
		public NumberGeneratorValueProviderCollection ValueProviders { get; private set; }
		public IList<NumberGeneratorTarget> AdditionalTargets { get; private set; }
	}
}
