using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.UY.Manifest.Business
{
	internal class DAEPackWrapper : IDaeBillOfLadingLine
	{
		internal DAEPackWrapper(AsycudaPack pack)
		{
			Pack = Argument.NotNull(pack, "asycudaPack cannot be null");
		}
		protected readonly AsycudaPack Pack;

		short IDaeBillOfLadingLine.LineNo => Pack.APA_LineNo;

		string IDaeBillOfLadingLine.Line_RecordType
		{
			get { return recordType; }
			set { recordType = value; }
		}
		string recordType;

		string IDaeBillOfLadingLine.Line_OperationType
		{
			get { return operationType; }
			set { operationType = value; }
		}
		string operationType;

		decimal IDaeBillOfLadingLine.Weight => Pack.APA_ArrivedWeight != 0 ? decimal.Parse(Core.Constants.Weight.ConvertSafe(Pack.APA_ArrivedWeight, Pack.APA_WeightUQ, Core.Constants.Weight.Kilograms).ToString("0.000"))
			: decimal.Parse(Core.Constants.Weight.ConvertSafe(Pack.APA_Weight, Pack.APA_WeightUQ, Core.Constants.Weight.Kilograms).ToString("0.000"));

		string IDaeBillOfLadingLine.PackUQ
		{
			get
			{
				var query = new ZQuery(RefPacksSchema.RP_CommercialPack, Pack.APA_PackUQ);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Uruguay);
				var pack = Pack.Factory.LoadTop1<CusRefPacks>(query);

				return pack?.RP_CustomsPack ?? Pack.APA_PackUQ;
			}
		}

		decimal IDaeBillOfLadingLine.PackQty => decimal.Parse(Pack.APA_PackQty.ToString(("0.000")));

		string IDaeBillOfLadingLine.MarksAndNumbers => Pack.APA_MarksAndNumbers;

		string IDaeBillOfLadingLine.GoodsDescription => Pack.APA_GoodsDescription;

		bool IDaeBillOfLadingLine.IsModifiedLine => ZBool.True;

		decimal IDaeBillOfLadingLine.LessOrMoreThanOriginal => decimal.Zero;

		bool IDaeBillOfLadingLine.IsNewPack => ZBool.True;
	}

	internal class DAEAmendPackWrapper : DAEPackWrapper, IDaeBillOfLadingLine
	{
		internal DAEAmendPackWrapper(AsycudaPack pack, ZBool isNewBill, IBillSent billSent) : base(pack)
		{
			this.isNewBill = isNewBill;
			this.billSent = billSent;
			packSent = this.billSent?.PackInfo.Where(x => x.Line == Line.LineNo)?.FirstOrDefault();
		}

		readonly ZBool isNewBill;
		readonly IBillSent billSent;
		readonly IPackSent packSent;
		IDaeBillOfLadingLine Line => this;

		bool IDaeBillOfLadingLine.IsModifiedLine
		{
			get
			{
				return Line.IsNewPack
					? ZBool.False
					: (ZBool)(packSent.Weight != Line.Weight
					|| packSent.PackUQ != Line.PackUQ
					|| packSent.PackQty != Line.PackQty
					|| packSent.MarksAndNumbers != Line.MarksAndNumbers
					|| packSent.GoodsDescription != Line.GoodsDescription);
			}
		}

		decimal IDaeBillOfLadingLine.PackQty => decimal.Parse(Pack.APA_ArrivedQuantity.ToString("0.000")) > 0
					? decimal.Parse(Pack.APA_ArrivedQuantity.ToString("0.000"))
					: decimal.Parse(Pack.APA_PackQty.ToString(("0.000")));

		decimal IDaeBillOfLadingLine.LessOrMoreThanOriginal => Line.IsNewPack || decimal.Parse(Pack.APA_ArrivedQuantity.ToString("0.000")) == 0
					? decimal.Zero
					: (Pack.APA_ArrivedQuantity - packSent.PackQty);

		bool IDaeBillOfLadingLine.IsNewPack => isNewBill || packSent == null;
	}
}
