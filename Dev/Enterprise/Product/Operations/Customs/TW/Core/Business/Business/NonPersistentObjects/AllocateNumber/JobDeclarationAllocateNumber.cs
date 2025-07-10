using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationAllocateNumber : AllocateNumber
	{
		public JobDeclarationAllocateNumber(JobDeclaration declaration, bool allowPart5NumberOutrangeWhenEntered = true)
			: base(declaration.Factory, allowPart5NumberOutrangeWhenEntered)
		{
			jobDeclaration = declaration;
			Part5Number = CommonHelper.GetEntryNumberPart5(declaration.DefaultEntryNumber);
			SetDefaultPartNumber();
		}

		readonly JobDeclaration jobDeclaration;

		protected override BaseEntryNumberGenerator GetEntryNumberGeneratorCore()
		{
			return TW.Business.EntryNumberGenerator.New(jobDeclaration);
		}

		public override ZString CheckBeforeAutoRegenerateEntryNumber()
		{
			ZString result;
			var entryNumberGenerator = EntryNumberGenerator;
			if (entryNumberGenerator != null && !entryNumberGenerator.IsAutoGenerateEntryNumberAllowed)
			{
				result = entryNumberGenerator.CannotAutoGenerateEntryNumberMessage;
			}
			else
			{
				result = base.CheckBeforeAutoRegenerateEntryNumber();
			}
			return result;
		}

		protected override AllocateNumberValidation GetNewValidation()
		{
			return new JobDeclarationAllocateNumberValidation(this);
		}

		protected override ResourceStringData GetFormCaptionCore() => Res.GetData("570ED397-BB7B-46D5-87FD-C2A850BD23D8", "Modify Entry Number");

		protected override ResourceStringData GetFormDescriptionCore() => Res.GetData("FE7EEA4C-A77B-41BE-84F8-5B7BC196E548", "Enter the number and click 'Modify Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.");
	}
}
