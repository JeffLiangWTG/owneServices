using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsProductInfo : DataObjectInfo
	{
		#region Constructors

		public WhsProductInfo(OrgSupplierPart part, GoodsHandlingInstructionsType noteType)
			: this(part)
		{
			if (part != null)
			{
				GoodsHandlingInstructions = GetHandlingInstruction(part, noteType);
			}
		}

		public WhsProductInfo(OrgSupplierPart part)
			: this()
		{
			if (part != null)
			{
				PK = part.PK.ToGuid();
				Code = part.OP_PartNum;
				Cubic = part.OP_Cubic;
				CubicUQ = part.OP_CubicUQ;
				Description = part.OP_Desc;
				PalletSize = part.OP_StockKeepingUnitPerPallet;
				StockUnit = part.OP_StockKeepingUnit;
				Weight = part.OP_Weight;
				WeightUQ = part.OP_WeightUQ;
				Depth = part.OP_Depth;
				Width = part.OP_Width;
				Height = part.OP_Height;
				MeasureUQ = part.OP_MeasureUQ;
				DecimalPlaces = part.OP_CountDecimalPlaces;
				IsBarcoded = part.OP_IsBarcoded;
				ProductUnits = new WhsUnitRateInfoCollection(part.PartUnits);
				ProductBarcodes = new WhsProductBarcodeInfoCollection(part.PartBarcodes);
				PickFaces = new WhsPickFaceInfoCollection(WhsProduct.GetWhsProduct(part));
				var clientsForProduct = part.RelatedOrganisations.Cast<OrgPartRelation>().Where(ou => ou.IsBoth || ou.IsOwner).ToArray();
				ClientPKs = clientsForProduct.Select(c => c.OU_OH.ToGuid()).ToArray();
				ClientNames = clientsForProduct.Select(c => c.Organisation.OH_Code.ToString()).ToArray();
			}
		}

		public WhsProductInfo()
		{
			PK = Guid.Empty;
			Code = "";
			Description = "";
			StockUnit = "";
			Weight = 0;
			WeightUQ = "";
			Depth = 0;
			Width = 0;
			Height = 0;
			MeasureUQ = "";
			Cubic = 0;
			CubicUQ = "";
			PalletSize = 0;
			DecimalPlaces = 0;
			IsBarcoded = true;
			ClientPKs = Array.Empty<Guid>();
			ClientNames = Array.Empty<string>();
		}

		#endregion

		#region GetInfo

		public static WhsProductInfo GetInfo(OrgSupplierPart part)
		{
			return part != null
				? part.Factory.GetCachedValue("WhsProductInfo|GetInfo|" + part.PK, () => new WhsProductInfo(part))
				: new WhsProductInfo(null);
		}

		public static WhsProductInfo GetInfo(OrgSupplierPart part, GoodsHandlingInstructionsType noteType)
		{
			return part != null && noteType != GoodsHandlingInstructionsType.None
				? part.Factory.GetCachedValue("WhsProductInfo|GetInfo|" + part.PK + "|" + noteType, () => new WhsProductInfo(part, noteType))
				: GetInfo(part);
		}

		static ZString GetHandlingInstruction(OrgSupplierPart part, GoodsHandlingInstructionsType noteType)
		{
			var notesByPriority = new List<(StmNote Note, int Priority)>();

			if (noteType != GoodsHandlingInstructionsType.None)
			{
				var notes = part.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
				if (notes.Length > 0)
				{
					BuildNoteList(notes);
				}
			}
			return notesByPriority.Count > 0
					? notesByPriority.OrderBy(x => x.Priority).Select(x => x.Note.ST_NoteDataAsText).FirstOrDefault(text => !text.IsEmpty)
					: ZString.Empty;

			void BuildNoteList(StmNote[] notes)
			{
				const string AllModule = nameof(StmNoteContextModule.A);
				const string AllDirection = nameof(StmNoteContextDirection.A);
				const string AllFreight = nameof(StmNoteContextFreightMode.A);
				const string WarehouseModule = nameof(StmNoteContextModule.W);
				const string InboundDirection = nameof(StmNoteContextDirection.R);
				const string OutboundDirection = nameof(StmNoteContextDirection.O);
				const string ReceiveOrReleaseFreight = nameof(StmNoteContextFreightMode.R);
				const string OrderFreight = nameof(StmNoteContextFreightMode.O);

				foreach (var note in notes)
				{
					var module = note.ST_NoteContextModule.ToString();
					var direction = note.ST_NoteContextDirection.ToString();
					var freight = note.ST_NoteContextFreightMode.ToString();

					if (module == AllModule && direction == AllDirection && freight == AllFreight)
					{
						notesByPriority.Add((note, 5));
					}
					else if (module == WarehouseModule && direction == AllDirection && freight == AllFreight)
					{
						notesByPriority.Add((note, 4));
					}
					else if (noteType == GoodsHandlingInstructionsType.Unload)
					{
						if (module == WarehouseModule && direction == InboundDirection)
						{
							var priority = freight == ReceiveOrReleaseFreight ? 1 : 3;
							notesByPriority.Add((note, priority));
						}
					}
					else if (noteType == GoodsHandlingInstructionsType.OrderPicking)
					{
						if (module == WarehouseModule && direction == OutboundDirection)
						{
							int priority;

							if (freight == ReceiveOrReleaseFreight)
							{
								priority = 1;
							}
							else if (freight == OrderFreight)
							{
								priority = 2;
							}
							else if (freight == AllFreight)
							{
								priority = 3;
							}
							else
							{
								continue;
							}

							notesByPriority.Add((note, priority));
						}
					}
				}
			}
		}

		#endregion

		#region Properties

		public Guid PK
		{
			get;
			set;
		}

		public string Code
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		public string StockUnit
		{
			get;
			set;
		}

		public decimal Weight
		{
			get;
			set;
		}

		public string WeightUQ
		{
			get;
			set;
		}

		public decimal Depth
		{
			get;
			set;
		}

		public decimal Width
		{
			get;
			set;
		}

		public decimal Height
		{
			get;
			set;
		}

		public string MeasureUQ
		{
			get;
			set;
		}

		public decimal Cubic
		{
			get;
			set;
		}

		public string CubicUQ
		{
			get;
			set;
		}

		public decimal PalletSize
		{
			get;
			set;
		}

		public int DecimalPlaces
		{
			get;
			set;
		}

		public string GoodsHandlingInstructions { get; set; }

		public Guid[] ClientPKs { get; set; }

		public string[] ClientNames { get; set; }

		public bool IsBarcoded { get; set; }

		public WhsUnitRateInfoCollection ProductUnits
		{
			get { return productUnits ?? (productUnits = new WhsUnitRateInfoCollection()); }
			set { productUnits = value; }
		}

		public WhsProductBarcodeInfoCollection ProductBarcodes
		{
			get { return productBarcodes ?? (productBarcodes = new WhsProductBarcodeInfoCollection()); }
			set { productBarcodes = value; }
		}

		public WhsPickFaceInfoCollection PickFaces
		{
			get { return this.pickFaces ?? (this.pickFaces = new WhsPickFaceInfoCollection()); }
			set { this.pickFaces = value; }
		}

		#endregion

		#region Implementation

		WhsUnitRateInfoCollection productUnits;
		WhsProductBarcodeInfoCollection productBarcodes;
		WhsPickFaceInfoCollection pickFaces;

		#endregion
	}
}
