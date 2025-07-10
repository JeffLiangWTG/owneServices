using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CombinedEquipmentNeededListTesting : TestCase
	{
		public void TestParameterlessConstructor()
		{
			CodeDescriptionPairList list = new CombinedEquipmentNeededList();
			CodeDescriptionPairList listFCL = new FCLEquipmentNeededList(false);
			CodeDescriptionPairList listAIR = new LCLAIREquipmentNeededList(true);

			AssertEquals("List.Count is less than sum of FCL and LCL list because they both contain ANY drop mode", listFCL.Count + listAIR.Count - 1, list.Count);
		}
	}
}
