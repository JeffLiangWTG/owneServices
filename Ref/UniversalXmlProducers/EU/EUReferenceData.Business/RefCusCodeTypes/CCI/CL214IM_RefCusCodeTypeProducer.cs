using System;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CL214IM_RefCusCodeTypeProducer : RefCusCodeTypeProducer
	{
		public override DateTime PublicationDate => new DateTime(1900, 01, 01);

		protected override string CodeType => Constants.CCIPreviousDocumentType.CodeType;

		protected override string Description => Constants.CCIPreviousDocumentType.Description;

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 4;
	}
}
