using System;

namespace Enterprise.Customs.Business.MultiLineAddInfos.Testing
{
	sealed class AddInfoWithWrongConstructor : BaseAddInfo
	{
		public AddInfoWithWrongConstructor()
			: base(null, null)
		{
		}

		public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
		{
			get { throw new NotImplementedException(); }
		}
	}
}
