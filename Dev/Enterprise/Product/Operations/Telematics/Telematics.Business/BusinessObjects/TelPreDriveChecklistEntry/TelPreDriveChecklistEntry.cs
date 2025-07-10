using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistEntry : AutoTelPreDriveChecklistEntry
	{
		public TelPreDriveChecklistEntry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TelPreDriveChecklistHeader Header => Factory.Load<TelPreDriveChecklistHeader>(TPE_TPH_ChecklistHeader);

		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid TPE_TPH_ChecklistHeader
		{
			get => base.TPE_TPH_ChecklistHeader;
			set => base.TPE_TPH_ChecklistHeader = value;
		}

		[ReadOnly(true)]
		public override ZShort TPE_Index
		{
			get => base.TPE_Index;
			set => base.TPE_Index = value;
		}

		[ReadOnly(true)]
		public override ZString TPE_IsAgreed
		{
			get => base.TPE_IsAgreed;
			set => base.TPE_IsAgreed = value;
		}

		[ReadOnly(true)]
		public override ZString TPE_Description
		{
			get => base.TPE_Description;
			set => base.TPE_Description = value;
		}
	}
}
