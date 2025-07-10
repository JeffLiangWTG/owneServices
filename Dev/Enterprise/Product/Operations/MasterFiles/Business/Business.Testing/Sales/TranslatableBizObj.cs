using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TranslatableBizObj : DummyBusinessObject
	{
		public TranslatableBizObj(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[TranslatableDataField(Schema.TableName, Schema.Z0_Code, Schema.Z0_CodeMaxLength, Schema.Z0_Code, Type = typeof(TranslatableBizObj), Asmid = ResString.AssemblyId)]
		public override ZString Z0_Code { get => base.Z0_Code; set => base.Z0_Code = value; }
	}
}
