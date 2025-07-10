using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class NestedInnerPackLineCollection : ActiveBusinessObjectCollection<ForwardingPackLine>
	{
		public NestedInnerPackLineCollection(ForwardingPackLine parent)
			: base(parent.Factory, new ZQuery(JobPackLinesSchema.JL_JL_OuterPackLine, parent.PK))
		{
			this.parent = parent;
		}

		readonly ForwardingPackLine parent;

		protected override void SetDefaultsForNewElementCore(ForwardingPackLine newInnerPackLine)
		{
			base.SetDefaultsForNewElementCore(newInnerPackLine);

			newInnerPackLine.JL_FreightMode = FreightConstants.InnerPackType;
			newInnerPackLine.JL_JS = parent.Shipment.PK;
			newInnerPackLine.JL_JL_OuterPackLine = parent.PK;
			newInnerPackLine.JL_Description = parent.JL_Description;

			HookIntoValueChangesForPackLine(newInnerPackLine);
			RefreshAllBindings();
		}

		protected override void OnLoadedIntoCollectionCore(ForwardingPackLine loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			HookIntoValueChangesForPackLine(loadedObject);
		}

		void HookIntoValueChangesForPackLine(ForwardingPackLine innerPackLine)
		{
			innerPackLine.JL_ActualWeightInfo.ValueChanged += InnerPackWeightInfo_ValueChanged;
			innerPackLine.JL_ActualWeightUQInfo.ValueChanged += InnerPackWeightInfo_ValueChanged;
			innerPackLine.JL_F3_NKPackTypeInfo.ValueChanged += InnerPackTypeInfo_ValueChanged;
			innerPackLine.JL_PackageCountInfo.ValueChanged += InnerPackCountInfo_ValueChanged;
			innerPackLine.JL_ActualVolumeInfo.ValueChanged += InnerPackVolumeInfo_ValueChanged;
			innerPackLine.JL_ActualVolumeUQInfo.ValueChanged += InnerPackVolumeInfo_ValueChanged;
		}

		void RefreshAllBindings()
		{
			RefreshWeightBinding();
			RefreshVolumeBinding();
			RefreshPackCountBinding();
			RefreshPackTypeBinding();
		}

		void RefreshVolumeBinding()
		{
			parent.InnerPackTotalVolumeInfo.RefreshBinding();
		}

		void RefreshWeightBinding()
		{
			parent.InnerPackTotalWeightInfo.RefreshBinding();
		}

		void RefreshPackCountBinding()
		{
			parent.InnerPackCountInfo.RefreshBinding();
		}

		void RefreshPackTypeBinding()
		{
			parent.InnerPackTypeInfo.RefreshBinding();
		}

		void InnerPackVolumeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			RefreshVolumeBinding();
		}

		void InnerPackCountInfo_ValueChanged(object sender, System.EventArgs e)
		{
			RefreshPackCountBinding();
		}

		void InnerPackTypeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			RefreshPackTypeBinding();
		}

		void InnerPackWeightInfo_ValueChanged(object sender, System.EventArgs e)
		{
			RefreshWeightBinding();
		}

		public override void Delete(ForwardingPackLine businessObject)
		{
			base.Delete(businessObject);
			RefreshAllBindings();
		}
	}
}
