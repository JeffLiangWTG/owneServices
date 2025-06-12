//#if DEBUG

//using System;
//using System.Collections.Generic;
//using System.Web;

//using Enterprise.Freight.DistanceCalculation.Integration;

//namespace Enterprise.Freight.DistanceCalculation.Service
//{
//    public class DummyDistanceCalculationService
//    {
//        public DistanceCalculationResult Process(DistanceCalculationConfiguration DistanceCalculationConfig, DistanceCalculationAddress OriginAddress, DistanceCalculationAddress DestinationAddress)
//        {
//            DistanceCalculationResult result = new DistanceCalculationResult();

//            string fakeDistance = OriginAddress.Address1 + OriginAddress.Address2 + OriginAddress.City + OriginAddress.Country + OriginAddress.PostCode + OriginAddress.State;
//            fakeDistance += DestinationAddress.Address1 + DestinationAddress.Address2 + DestinationAddress.City + DestinationAddress.Country + DestinationAddress.PostCode + DestinationAddress.State;
			
//            result.Distance = fakeDistance.Length;
//            result.DistanceUnit = string.IsNullOrEmpty(DistanceCalculationConfig.UnitsForCalculation) ? DistanceCalculationConstants.UnitsForCalculation.Miles : DistanceCalculationConfig.UnitsForCalculation;
//            result.TravelTime = 0;
//            result.StatusMessage = "";

//            return result;
//        }
//    }
//}

//#endif