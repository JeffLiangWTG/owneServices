using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using static Enterprise.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Business
{
	public class OrgCustomLabels : MasterFiles.Business.OrgCustomLabels, IOrgCustomLabels, IShortSequenceNumberLine
	{
		internal CustomDocumentsCollection customDocumentsCollection;

		public OrgCustomLabels(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public OrgCustomLabels(CustomDocumentsCollection customDocumentsCollection, BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.customDocumentsCollection = customDocumentsCollection;
			customDocumentsCollection.PositionNumberGenerator.RecalculateWhenAdded(this);
		}

		[List(nameof(Lookups) + "." + nameof(OrgCustomLabelsLookups.FieldNameList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.OrgCustomLabels|OT_FieldName", Caption = "Field")]
		public override ZString OT_FieldName { get => base.OT_FieldName; set => base.OT_FieldName = value; }

		public override ZShort OT_Position
		{
			get { return base.OT_Position; }
			set
			{
				if (value > 0)
				{
					var oldValue = OT_Position;

					base.OT_Position = value;
					if (oldValue != OT_Position)
					{
						customDocumentsCollection?.PositionNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		public new OrgCustomLabelsLookups Lookups => (OrgCustomLabelsLookups)base.Lookups;

		protected override MasterFiles.Business.OrgCustomLabelsLookups GetNewLookups() => new OrgCustomLabelsLookups(this);

		protected override MasterFiles.Business.OrgCustomLabelsValidation GetNewValidation() => new OrgCustomLabelsValidation(this);

		public override void Delete()
		{
			customDocumentsCollection?.PositionNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			base.Delete();
		}

		#region ISequenceNumberLine

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => OT_Position;
			set => OT_Position = value;
		}

		ZGuid ISequenceNumberLine.FKToHeader => Header.PK;

		#endregion
	}
}
