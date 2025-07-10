using System;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CL156_RefCusCodeTypeProducer : RefCusCodeTypeProducer
	{
		protected override string CodeType => "CL156";

		protected override string Description => "Role of Requester";

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 1;

		public override DateTime PublicationDate => new DateTime(1900, 01, 01);
	}
}
