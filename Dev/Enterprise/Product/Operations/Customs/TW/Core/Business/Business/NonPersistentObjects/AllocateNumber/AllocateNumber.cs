using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public abstract class AllocateNumber : AutoAllocateNumber
	{
		public AllocateNumber(BusinessObjectFactory factory, bool allowPart5NumberOutrangeWhenEntered)
			: base(factory)
		{
			this.allowPart5NumberOutrangeWhenEntered = allowPart5NumberOutrangeWhenEntered;
		}

		internal bool allowPart5NumberOutrangeWhenEntered;

		public BaseEntryNumberGenerator EntryNumberGenerator => GetEntryNumberGeneratorCore();

		protected abstract BaseEntryNumberGenerator GetEntryNumberGeneratorCore();

		public ResourceStringData FormCaption => GetFormCaptionCore();

		protected abstract ResourceStringData GetFormCaptionCore();

		public ResourceStringData FormDescription => GetFormDescriptionCore();

		protected abstract ResourceStringData GetFormDescriptionCore();

		public override ZString Part1Number => EntryNumberGenerator?.Part1 ?? ZString.Empty;

		public override ZString Part3Number => EntryNumberGenerator?.Part3 ?? ZString.Empty;

		int Part4_Length => EntryNumberGenerator?.Part4_Length ?? Schema.Part4NumberMaxLength;

		public ZString Part4Caption => Res.GetString("9fca34f2-1d07-43df-af40-475decf6f2d8", "Entry number cannot be generated because '{0}' is missing.", EntryNumberGenerator.Part4Caption);

		public ZString Number => !Part5Number.IsEmpty ? new ZString(string.Concat(Part1Number, Part2Number.PadLeft(Schema.Part2NumberMaxLength), Part3Number, Part4Number.PadLeft(Part4_Length), Part5Number)) : ZString.Empty;

		public ZString AllowedGeneratorDescriptionWhenAutoGenerate => GetAllowedGeneratorDescription(-1);

		public virtual ZString CheckBeforeAutoRegenerateEntryNumber()
		{
			return GetAllowedGeneratorDescription(-1);
		}

		ZString GetAllowedGeneratorDescription(int nextNumber)
		{
			ZString result;
			if (EntryNumberGenerator == null)
			{
				result = Res.GetString("4C6D1C7C-B278-4B6C-9D34-1DED913DDA96", "The generator rules could not be found for the job.");
			}
			else
			{
				result = EntryNumberGenerator.CheckNextNumberIsInRange(nextNumber);
				if (result.IsEmpty)
				{
					result = EntryNumberGenerator.AllowedGeneratorDescription;
				}
			}
			return result;
		}

		public ZBool IsPart5NumberOutrangeWhenAllowOutrange
		{
			get
			{
				var result = ZBool.False;
				if (allowPart5NumberOutrangeWhenEntered)
				{
					var nextNumber = EntryNumberGenerator.Sequenceformatter.FormatStringToInt(Part5Number);
					result = nextNumber > 0 && !EntryNumberGenerator.CheckNextNumberIsInRange(nextNumber).IsEmpty;
				}
				return result;
			}
		}

		public ZBool IsPart4NumberEmpty => Part4Number.IsEmpty;

		protected void SetDefaultPartNumber()
		{
			using (SuspendSettingHasChanges())
			{
				Part2Number = EntryNumberGenerator?.Part2 ?? ZString.Empty;
				Part4Number = EntryNumberGenerator?.Part4 ?? ZString.Empty;
			}
		}
	}
}
