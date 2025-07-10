using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistTemplateHeader : AutoTelPreDriveChecklistTemplateHeader
	{
		public TelPreDriveChecklistTemplateHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		TelPreDriveChecklistTemplateEntryCollection entries;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("1DC06236-A6CA-4D84-B739-22C9F8FFDB8E", "Pre-drive declaration templates are currently static, pre-defined types. While their entries can be deleted and modified, the header cannot"); }
		}

		public override void Delete()
		{
			throw new NotSupportedException(ReasonForNotAbleToDelete);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("37E3B961-5372-4A44-B639-B9624AE02B75", "{0}", TTH_Description);

		[ChildEditable]
		public TelPreDriveChecklistTemplateEntryCollection Entries
		{
			get
			{
				if (entries == null)
				{
					entries = new TelPreDriveChecklistTemplateEntryCollection(Factory, this);
				}
				RegisterEditableChildObject(entries);
				return entries;
			}
		}
	}
}
