using System;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CL054_RefCusCodeTypeProducer : RefCusCodeTypeProducer
	{
		protected override string CodeType => "CL054";

		protected override string Description => "Query Identifier";

		protected override bool ReadOnly => true;

		protected override byte MaxLength => 1;

		public override DateTime PublicationDate => new DateTime(1900, 01, 01);
	}
}
