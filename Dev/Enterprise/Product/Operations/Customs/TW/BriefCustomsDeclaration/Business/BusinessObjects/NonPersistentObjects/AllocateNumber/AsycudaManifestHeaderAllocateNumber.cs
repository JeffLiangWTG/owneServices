using System.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaManifestHeaderAllocateNumber : AllocateNumber
	{
		public AsycudaManifestHeaderAllocateNumber(AsycudaManifestHeader header, bool allowPart5NumberOutrangeWhenEntered = true)
			: base(header.Factory, allowPart5NumberOutrangeWhenEntered)
		{
			this.header = header;
			SetDefaultPartNumber();
		}

		readonly AsycudaManifestHeader header;

		protected override BaseEntryNumberGenerator GetEntryNumberGeneratorCore() => new EntryNumberGeneratorCatD(header);

		[ReadOnly(true)]
		public override ZString Part2Number { get => base.Part2Number; set => base.Part2Number = value; }

		[ReadOnly(true)]
		public override ZString Part4Number { get => base.Part4Number; set => base.Part4Number = value; }

		protected override ResourceStringData GetFormCaptionCore() => Res.GetData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeaderAllocateNumber|FormCaption", "Modify Entry Number");

		protected override ResourceStringData GetFormDescriptionCore() => Res.GetData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeaderAllocateNumber|FormDescription", "Enter the number and click 'Modify Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.");
	}
}
