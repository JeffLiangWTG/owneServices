using System;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CL104IM_RefCusCodeTypeProducer : RefCusCodeTypeProducer
	{
		public override DateTime PublicationDate => new DateTime(1900, 01, 01);

		protected override string CodeType => Constants.CCIMethodOfPayment.CodeType;

		protected override string Description => Constants.CCIMethodOfPayment.Description;

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 1;
	}
}
