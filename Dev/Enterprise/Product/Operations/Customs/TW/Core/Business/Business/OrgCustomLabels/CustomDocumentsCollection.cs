using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class CustomDocumentsCollection : OrgCustomLabelsCollection, ISequenceNumberHeader
	{
		readonly ZString type;

		public CustomDocumentsCollection(OrgHeader parent)
			: base(parent)
		{
		}

		public CustomDocumentsCollection(ZString type, OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			this.type = type;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(JoinCondition.And, OrgCustomLabelsSchema.OT_Type, SQLComparisonOperator.Equal, type);
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is OrgCustomLabels orgCustomLabel)
			{
				orgCustomLabel.OT_Type = type;
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			return new OrgCustomLabels(this, Factory, row);
		}

		public new OrgCustomLabels AddNew()
		{
			return (OrgCustomLabels)base.AddNew();
		}

		public new OrgCustomLabels this[int index]
		{
			get { return (OrgCustomLabels)Elements[index]; }
		}

		#region ISequenceNumberHeader Members

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();

		ShortSequenceNumberGenerator positionNumberGenerator;

		internal ShortSequenceNumberGenerator PositionNumberGenerator => positionNumberGenerator ?? (positionNumberGenerator = new ShortSequenceNumberGenerator(this));

		#endregion
	}
}
