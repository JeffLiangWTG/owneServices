using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	[ModuleID(ModuleId.CartageLeg)]
	public class CommonCartageLegCollection : ActiveBusinessObjectCollection<CommonCartageLeg>
	{
		public CommonCartageLegCollection(CommonWorkSheet runSheet)
			: base(runSheet.Factory, runSheet, null, JobContainerLegsSchema.JU_EY_RunSheet)
		{
			allowNew = false;
		}

		public CommonCartageLegCollection(CommonBookedCtgMove bookedCartageMove)
			: base(bookedCartageMove.Factory, bookedCartageMove, null, JobContainerLegsSchema.JU_EW)
		{
			allowNew = true;
		}

		public CommonCartageLegCollection(CommonBookedCtgMoveCollection bookedMoves)
			: base(bookedMoves.Factory)
		{
			this.allowNew = false;
			this.bookedMoves = bookedMoves;
			BuildCollection();

			bookedMoves.CountChanged += new EventHandler(bookedMoves_CountChanged);
		}

		void bookedMoves_CountChanged(object sender, EventArgs e)
		{
			BuildCollection();
		}

		public CommonCartageLegCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNew
		{
			get { return allowNew; }
		}
		readonly bool allowNew;

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			if (!((CommonCartageLeg)selectedBusinessObject).JU_EY_RunSheet.IsEmpty)
			{
				errors.Add(Res.GetString("500f61ba-20bc-4914-a100-d440bd890d12", "Port Transport Legs can belong to only one worksheet. This Port Transport Leg cannot be attached."));
			}
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new CommonCartageLegFindBoxListProvider(this); }
		}

		class CommonCartageLegFindBoxListProvider : FindBoxListProvider
		{
			public CommonCartageLegFindBoxListProvider(IBusinessObjectCollection collection) : base(collection)
			{
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code)
			{
				base.AddCodeEqualsFilter(query, code);
				query.IsNoResultQuery = false;

				var slashIndex = code.LastIndexOf('/');
				var cartageID = code.Substring(0, slashIndex);
				var deliverySuffix = code.Substring(slashIndex + 1);

				var mainQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
				mainQuery.AddToFilter(JobContainerLegsSchema.JU_SplitDeliverySuffix, deliverySuffix);

				var moveSub = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW);
				var cartageSub = new ZDBOnlySubQuery(typeof(CommonCartage), JobBookedCtgMoveSchema.EW_JJ);
				cartageSub.AddToFilter(JobCartageSchema.JJ_ConsignmentID, cartageID);

				moveSub.AddSubQuery(cartageSub, JoinCondition.And);
				mainQuery.AddSubQuery(moveSub, JoinCondition.And);

				query.AddToFilter(mainQuery);
			}
		}

		void BuildCollection()
		{
			if (bookedMoves.Count > 0)
			{
				AdditionalFilter = new ZQuery(JobContainerLegsSchema.JU_EW, Array.ConvertAll(bookedMoves.ToArray(), (bm) => bm.PK));
			}
			else
			{
				AdditionalFilter = ZQuery.NoResultQuery;
			}
		}
		readonly CommonBookedCtgMoveCollection bookedMoves;
	}
}
