using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	[SingleObjectAroundARow]
	[CodeProperty(EDIMessagePurposeSchema.Constants.EMP_Code), DescriptionProperty(EDIMessagePurposeSchema.Constants.EMP_Description)]
	public class EDIMessagePurpose : AutoEDIMessagePurpose, IEDIMessagePurpose
	{
		public EDIMessagePurpose(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[TranslatableDataField(Schema.TableName, Schema.EMP_Description, MaxLength = Schema.EMP_DescriptionMaxLength, Type = typeof(EDIMessagePurpose), Asmid = ResString.AssemblyId)]
		public override ZString EMP_Description
		{
			get => base.EMP_Description;
			set => base.EMP_Description = value;
		}

		public MultilingualString EMP_DescriptionMultilingual => GetMultilingual(EMP_DescriptionInfo);

		[List("Lookups.Filters")]
		public override ZGuid EMP_ECF_Filter
		{
			get => base.EMP_ECF_Filter;
			set => base.EMP_ECF_Filter = value;
		}

		[ReadOnly(true)]
		public override ZDateTime EMP_SystemCreateTimeUtc
		{
			get => base.EMP_SystemCreateTimeUtc;
			set => base.EMP_SystemCreateTimeUtc = value;
		}

		[ReadOnly(true)]
		public override ZString EMP_SystemCreateUser
		{
			get => base.EMP_SystemCreateUser;
			set => base.EMP_SystemCreateUser = value;
		}

		[ReadOnly(true)]
		public override ZDateTime EMP_SystemLastEditTimeUtc
		{
			get => base.EMP_SystemLastEditTimeUtc;
			set => base.EMP_SystemLastEditTimeUtc = value;
		}

		[ReadOnly(true)]
		public override ZString EMP_SystemLastEditUser
		{
			get => base.EMP_SystemLastEditUser;
			set => base.EMP_SystemLastEditUser = value;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("423165ab-73b9-49be-8b51-b9e067138ec7", "Purpose {0}", EMP_Code);

		#region Utils

		public static IEDIMessagePurpose Load(IFactory factory, ZString purposeCode)
		{
			return (EDIMessagePurpose)factory.LoadTop1(typeof(EDIMessagePurpose), new ZQuery(EDIMessagePurposeSchema.EMP_Code, purposeCode));
		}

		#endregion
	}
}
