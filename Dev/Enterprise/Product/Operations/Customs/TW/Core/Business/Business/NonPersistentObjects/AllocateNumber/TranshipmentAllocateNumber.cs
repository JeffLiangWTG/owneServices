using System.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public class TranshipmentAllocateNumber : AllocateNumber
	{
		public TranshipmentAllocateNumber(CusInBondHeader cusInBondHeader, bool allowPart5NumberOutrangeWhenEntered = true)
			: base(cusInBondHeader.Factory, allowPart5NumberOutrangeWhenEntered)
		{
			inBondHeader = cusInBondHeader;
			SetDefaultPartNumber();
		}

		readonly CusInBondHeader inBondHeader;

		protected override BaseEntryNumberGenerator GetEntryNumberGeneratorCore()
		{
			return new TranshipmentEntryNumberGenerator(inBondHeader);
		}

		[ReadOnly(true)]
		public override ZString Part2Number { get => base.Part2Number; set => base.Part2Number = value; }

		[ReadOnly(true)]
		public override ZString Part4Number { get => base.Part4Number; set => base.Part4Number = value; }

		protected override ResourceStringData GetFormCaptionCore() => Res.GetData("2A0DD882-3698-47AD-AEE0-4350EC907C5C", "Allocate Entry Number");

		protected override ResourceStringData GetFormDescriptionCore() => Res.GetData("C6051DBB-C09C-427F-B007-B160F5665035", "Enter the number and click 'Allocate Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.");
	}
}
