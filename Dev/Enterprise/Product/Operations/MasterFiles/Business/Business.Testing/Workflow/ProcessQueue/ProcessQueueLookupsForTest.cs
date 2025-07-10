using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessQueueLookupsForTest : ProcessQueueLookups
	{
		public ProcessQueueLookupsForTest(ProcessQueueForTest parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetCustomsStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (Parent.P4_CustomsQueue == "ON1")
			{
				result.AddPair("XXX", "Only 1 status");
			}
			else
			{
				result.AddPair("S1", "More than 1");
				result.AddPair("S2", "More than 1");
			}
			return result;
		}

		protected override CodeDescriptionPairList GetCommercialStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (Parent.P4_QueueName == "ON2")
			{
				result.AddPair("XXX", "Only 1 status");
			}
			else
			{
				result.AddPair("S1", "More than 1");
				result.AddPair("S2", "More than 1");
			}
			return result;
		}

		protected override CodeDescriptionPairList GetCustomsSubStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (Parent.P4_CustomsStatus == "S1")
			{
				result.AddPair("YYY", "Only 1 sub status");
			}
			else
			{
				result.AddPair("A1", "More than 1");
				result.AddPair("A2", "MEH");
			}
			return result;
		}

		protected override CodeDescriptionPairList GetCommercialSubStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			if (Parent.P4_Status == "S2")
			{
				result.AddPair("YYY", "Only 1 sub status");
			}
			else
			{
				result.AddPair("A1", "More than 1");
				result.AddPair("A2", "MEH");
			}
			return result;
		}

		protected override bool CustomsStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override bool CommercialStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override bool CustomsSubStatusListShouldBeCached
		{
			get { return false; }
		}

		protected override bool CommercialSubStatusListShouldBeCached
		{
			get { return false; }
		}

		new ProcessQueue Parent
		{
			get { return (ProcessQueue)base.Parent; }
		}
	}
}
