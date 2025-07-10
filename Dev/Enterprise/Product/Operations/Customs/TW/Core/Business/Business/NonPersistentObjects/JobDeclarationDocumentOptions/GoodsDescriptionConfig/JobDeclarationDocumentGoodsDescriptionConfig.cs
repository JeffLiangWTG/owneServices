using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentGoodsDescriptionConfig : AutoJobDeclarationDocumentGoodsDescriptionConfig, IShortSequenceNumberLine
	{
		public JobDeclarationDocumentGoodsDescriptionConfig(JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig) : base(jobDeclarationDocumentAddressConfig.Factory)
		{
			this.jobDeclarationDocumentAddressConfig = Argument.NotNull(jobDeclarationDocumentAddressConfig, nameof(jobDeclarationDocumentAddressConfig));
			jobDeclarationDocumentAddressConfig.PositionNumberGenerator.RecalculateWhenAdded(this);
		}

		readonly JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;

		public ZBool IsImport => jobDeclarationDocumentAddressConfig.Declaration.IsImport;

		public override ZShort Position
		{
			get { return base.Position; }
			set
			{
				if (value > 0)
				{
					var oldValue = Position;

					base.Position = value;
					if (oldValue != Position)
					{
						jobDeclarationDocumentAddressConfig.PositionNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationDocumentGoodsDescriptionConfigLookups.FiledsList))]
		public override ZString Field { get => base.Field; set => base.Field = value; }

		#region Lookups

		public JobDeclarationDocumentGoodsDescriptionConfigLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new JobDeclarationDocumentGoodsDescriptionConfigLookups(this);
				}

				return lookups;
			}
		}

		JobDeclarationDocumentGoodsDescriptionConfigLookups lookups;

		#endregion

		public override void Delete()
		{
			base.Delete();
			jobDeclarationDocumentAddressConfig.PositionNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		#region ISequenceNumberLine

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => Position;
			set => Position = value;
		}

		ZGuid ISequenceNumberLine.FKToHeader => jobDeclarationDocumentAddressConfig.AddressConfigPK;

		#endregion
	}
}
