using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.TaxDateDefaultingOptionLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxDateDefaultingOptionRegistryItem))]
	sealed class TaxDateDefaultingOptionRegistryItemTest : StronglyTypedRegistryItemTestCase<TaxDateDefaultingOptionCollection>
	{
		protected override StronglyTypedRegistryItem<TaxDateDefaultingOptionCollection, TaxDateDefaultingOptionCollection> GetNewRegistryItem()
		{
			return new TaxDateDefaultingOptionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new TaxDateDefaultingOptionCollection());
		}
	}

	[TestedType(typeof(TaxDateDefaultingOptionRegistryDataType))]
	sealed class TaxDateDefaultingOptionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TaxDateDefaultingOptionRegistryDataType>
	{
		#region Implementation

		protected override TaxDateDefaultingOptionRegistryDataType GetNewDataType()
		{
			return new TaxDateDefaultingOptionRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "TaxDateDefaultingOptionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new TaxDateDefaultingOptionCollection();
			var copy = collection.AddNew();

			copy.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			copy.Mode = Constants.TransportModes.Rail;
			copy.TaxDateOption = TaxDateDefaultingOption.Code.Today;
			copy.Ledger = LedgerTypeAdditionalCodes.All;

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,84,0,97,0,120,0,68,0,97,0,116,0,101,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,105,0,110,0,103,0,79,0,112,
0,116,0,105,0,111,0,110,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,
0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,
0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,
0,110,0,99,0,101,0,34,0,62,0,60,0,84,0,97,0,120,0,68,0,97,0,116,0,101,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,105,0,110,0,103,0,79,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,74,0,111,
0,98,0,84,0,121,0,112,0,101,0,62,0,83,0,72,0,80,0,60,0,47,0,74,0,111,0,98,0,84,0,121,0,112,0,101,0,62,0,60,0,68,0,105,0,114,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,
0,62,0,65,0,76,0,76,0,60,0,47,0,68,0,105,0,114,0,101,0,99,0,116,0,105,0,111,0,110,0,67,0,111,0,100,0,101,0,62,0,60,0,77,0,111,0,100,0,101,0,62,0,82,0,65,0,73,0,60,0,47,0,77,0,111,
0,100,0,101,0,62,0,60,0,84,0,97,0,120,0,68,0,97,0,116,0,101,0,79,0,112,0,116,0,105,0,111,0,110,0,62,0,84,0,68,0,89,0,60,0,47,0,84,0,97,0,120,0,68,0,97,0,116,0,101,0,79,0,112,0,116,
0,105,0,111,0,110,0,62,0,60,0,76,0,101,0,100,0,103,0,101,0,114,0,62,0,65,0,76,0,76,0,60,0,47,0,76,0,101,0,100,0,103,0,101,0,114,0,62,0,60,0,47,0,84,0,97,0,120,0,68,0,97,0,116,0,101,
0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,105,0,110,0,103,0,79,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,84,0,97,0,120,0,68,0,97,0,116,0,101,
0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,105,0,110,0,103,0,79,0,112,0,116,0,105,0,111,0,110,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
