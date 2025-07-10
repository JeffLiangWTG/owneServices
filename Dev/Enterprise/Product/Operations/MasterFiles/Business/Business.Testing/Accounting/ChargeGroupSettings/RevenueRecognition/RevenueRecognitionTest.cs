using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RevenueRecognition))]
	class RevenueRecognitionTest : ChargeGroupSettingTest
	{
		public void TestRevenueRecognitionForQSH()
		{
			var result = RevenueRecognition.CreateRevenueRecognitionForQSH();

			var persistentFieldNames = result.ZPropertyInfoHash.Cast<ZPropertyInfo>().Select(x => x.Name);

			var expectedColumnsAndValues = new Dictionary<string, IZType>();
			expectedColumnsAndValues.Add(result.RecognitionDateOptionCodeInfo.Name, (ZString)RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction);
			expectedColumnsAndValues.Add(result.RecognitionDateOptionDescriptionInfo.Name, (ZString)RevenueRecognitionLookups.RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
			expectedColumnsAndValues.Add(result.OffsetInfo.Name, (ZInt)0);
			expectedColumnsAndValues.Add(result.OffsetTypeInfo.Name, (ZString)"DAY");
			expectedColumnsAndValues.Add(result.BrokerCodeInfo.Name, ZString.Empty);
			expectedColumnsAndValues.Add(result.JobTypeInfo.Name, (ZString)"QSH");
			expectedColumnsAndValues.Add(result.DirectionCodeInfo.Name, (ZString)"ALL");
			expectedColumnsAndValues.Add(result.ModeInfo.Name, (ZString)"");

			AssertContainsExactElementsInAnyOrder("All Properties should be in persistent list", persistentFieldNames, expectedColumnsAndValues.Keys);
			AssertContainsExactElementsInAnyOrder("All Properties must have expected values", expectedColumnsAndValues.Keys, result.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => expectedColumnsAndValues.ContainsKey(x.Name) && x.Value.CompareTo(expectedColumnsAndValues[x.Name]) == 0).Select(x => x.Name));
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new RevenueRecognition();

			result.JobType = "SHP";
			result.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			result.Mode = Core.Constants.TransportModes.Air;
			result.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

			return result;
		}

		protected new RevenueRecognition BizObj
		{
			get { return (RevenueRecognition)base.BizObj; }
		}
	}
}
