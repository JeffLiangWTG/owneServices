using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class RelatedBusinessData : AutoRelatedBusinessData, IHugeSequenceNumberLine
	{
		public RelatedBusinessData(BusinessObjectFactory factory, OrgAddressMessageData messageData)
			: base(factory)
		{
			this.messageData = messageData;
		}
		internal readonly OrgAddressMessageData messageData;

		[MaxLength(2)]
		[ReadOnly(true)]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set { base.US_LineNo = value; }
		}

		[List(nameof(Lookups) + "." + nameof(RelatedBusinessDataLookups.RelatedBusinessTypes))]
		public override ZString US_RelatedBusiness
		{
			get { return base.US_RelatedBusiness; }
			set { base.US_RelatedBusiness = value; }
		}

		public RelatedBusinessDataLookups Lookups
		{
			get { return new RelatedBusinessDataLookups(this); }
		}

		public override void Delete()
		{
			base.Delete();
			messageData.RelatedBusinessSequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		#region IHugeSequenceNumberLine Members

		ZGuid ISequenceNumberLine.FKToHeader => messageData.wrapper.CountryData.PK;

		ZInt ISequenceNumberLine<ZInt>.SequenceNumber
		{
			get => US_LineNo;
			set => US_LineNo = value;
		}

		#endregion
	}
}
