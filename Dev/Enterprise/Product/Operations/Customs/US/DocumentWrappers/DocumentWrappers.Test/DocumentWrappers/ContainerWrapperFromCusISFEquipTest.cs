using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromCusISFEquip))]
	sealed class ContainerWrapperFromCusISFEquipTest : ContainerWrapperTest
	{
		public void TestFreightJob()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_JobReference = "12345";
			CusISFEquip container = header.Equipments.AddNew();
			ContainerWrapperFromCusISFEquip wrapper = new ContainerWrapperFromCusISFEquip(container, Factory);
			AssertEquals("Wrapped business object should be CusISFHeader", "12345", wrapper.FreightJob.JobNumber);
		}

		public override void TestWrapperMappingFull()
		{
			CusISFEquip container = Factory.New<CusISFEquip>();
			container.BE_ContainerNum = "AJUR9834711";

			ContainerWrapperFromCusISFEquip wrapper = new ContainerWrapperFromCusISFEquip(container, Factory);
			AssertEquals("wrapper.Hazardous", false, wrapper.IsHazardous);
			AssertEquals("wrapper.Reefer", false, wrapper.IsReefer);
			AssertEquals("wrapper.Mode.Code", ZString.Empty, wrapper.Mode.Code);
			AssertEquals("wrapper.DeliveryMode.Code", ZString.Empty, wrapper.DeliveryMode.Code);
			AssertEquals("wrapper.Type.Code", ZString.Empty, wrapper.Type.Code);
			AssertEquals("wrapper.Type.Description", ZString.Empty, wrapper.Type.Description);
			AssertEquals("wrapper.WeightTare.Value", ZDecimal.Zero, wrapper.WeightTare.Value);
			AssertEquals("wrapper.WeightTare.Unit.Code", "KG", wrapper.WeightTare.Unit.Code);
			AssertEquals("wrapper.WeightGoods.Value", ZDecimal.Zero, wrapper.WeightGoods.Value);
			AssertEquals("wrapper.WeightGoods.Unit.Code", ZString.Empty, wrapper.WeightGoods.Unit.Code);
			AssertEquals("wrapper.WeightDunnage.Value", ZDecimal.Zero, wrapper.WeightDunnage.Value);
			AssertEquals("wrapper.WeightDunnage.Unit.Code", ZString.Empty, wrapper.WeightDunnage.Unit.Code);
			AssertEquals("wrapper.WeightGross.Value", ZDecimal.Zero, wrapper.WeightGross.Value);
			AssertEquals("wrapper.WeightGross.Unit.Code", ZString.Empty, wrapper.WeightGross.Unit.Code);
			AssertEquals("wrapper.VolumeGoods.Value", ZDecimal.Zero, wrapper.VolumeGoods.Value);
			AssertEquals("wrapper.VolumeGoods.Unit.Code", ZString.Empty, wrapper.VolumeGoods.Unit.Code);
			AssertEquals("wrapper.PackCount.Value", ZDecimal.Zero, wrapper.PackCount.Value);
			AssertEquals("wrapper.PackCount.Unit.Code", ZString.Empty, wrapper.PackCount.Unit.Code);
			AssertEquals("wrapper.Commodity.Count", 0, wrapper.Commodities.Count);
			AssertEquals("wrapper.Services.Count", 0, wrapper.Services.Count);
			AssertEquals("wrapper.ContainerNo", "AJUR9834711", wrapper.ContainerNo);
			AssertEquals("wrapper.ContainerNumberOrTypeCount", "AJUR9834711", wrapper.ContainerNumberOrTypeCount);
			AssertEquals("wrapper.SealNo", ZString.Empty, wrapper.SealNo);
			AssertEquals("wrapper.SealNo2", ZString.Empty, wrapper.SealNo2);
			AssertEquals("wrapper.SealNo3", ZString.Empty, wrapper.SealNo3);
			AssertEquals("wrapper.ReleaseNumber", ZString.Empty, wrapper.ReleaseNumber);
			AssertEquals("wrapper.ArrivalReleaseNumber", ZString.Empty, wrapper.ArrivalReleaseNumber);
			AssertEquals("wrapper.EmptyReturnedBy", ZDateTime.Empty, wrapper.EmptyReturnedBy);
			AssertEquals("wrapper.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapper.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapper.BookingReference", ZString.Empty, wrapper.BookingReference);
			AssertEquals("wrapper.ArrivalEstimatedDelivery", ZDateTime.Empty, wrapper.ArrivalEstimatedDelivery);
			AssertEquals("wrapper.ArrivalSlotReference", ZString.Empty, wrapper.ArrivalSlotReference);
			AssertEquals("wrapper.DepartureSlotReference", ZString.Empty, wrapper.DepartureSlotReference);
			AssertEquals("ArrivalSlotTime", ZDateTime.Empty, wrapper.ArrivalSlotTime);
			AssertEquals("DepartureSlotTime", ZDateTime.Empty, wrapper.DepartureSlotTime);
			AssertEquals("wrapper.ExportDepotCustomsReference", ZString.Empty, wrapper.ExportDepotCustomsReference);
			AssertEquals("wrapper.EmptyReadyForReturn", ZDateTime.Empty, wrapper.EmptyReadyForReturn);
			AssertEquals("wrapper.EmptyRequired", ZDateTime.Empty, wrapper.EmptyRequired);
			AssertEquals("wrapper.WharfGateOut", ZDateTime.Empty, wrapper.WharfGateOut);
			AssertEquals("wrapper.DepartureEstimatedPickup", ZDateTime.Empty, wrapper.DepartureEstimatedPickup);
			AssertEquals("wrapper.Length", ZDecimal.Zero, wrapper.Length);
			AssertEquals("wrapper.Width", ZDecimal.Zero, wrapper.Width);
			AssertEquals("wrapper.Height", ZDecimal.Zero, wrapper.Height);
			AssertEquals("wrapper.SetPointTemperature.Value", ZDecimal.Zero, wrapper.SetPointTemperature.Value);
			AssertEquals("wrapper.SetPointTemperature.Unit.Code", ZString.Empty, wrapper.SetPointTemperature.Unit.Code);
			Assert("wrapper.Damaged", !wrapper.Damaged);
			Assert("wrapper.Frozen", !wrapper.Frozen);
			Assert("wrapper.Chilled", !wrapper.Chilled);
			Assert("wrapper.ControlledAtmosphere", !wrapper.ControlledAtmosphere);
			AssertEquals("wrapper.HumidityPercentage", ZByte.Zero, wrapper.HumidityPercentage);
			AssertEquals("wrapper.AirVentFlow.Value", ZDecimal.Zero, wrapper.AirVentFlow.Value);
			AssertEquals("wrapper.AirVentFlow.Unit.Code", ZString.Empty, wrapper.AirVentFlow.Unit.Code);
			AssertEquals("wrapper.ClipOnUnit", ZString.Empty, wrapper.ClipOnUnit);
			AssertEquals("wrapper.UNDGSubstances.Count", 0, wrapper.UNDGSubstances.Count);
			AssertEquals("wrapper.DepartureContainerYardAddress.CompanyName", ZString.Empty, wrapper.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapper.ArrivalContainerYardAddress.CompanyName", ZString.Empty, wrapper.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapper.ContainerJobID", ZString.Empty, wrapper.ContainerJobID);
			AssertEquals("wrapper.IsChargeable", "No", wrapper.IsChargeable);
			AssertEquals("wrapper.IsPalletized", "No", wrapper.IsPalletized);
			AssertEquals("wrapper.Items", "0", wrapper.Packages);
			AssertEquals("wrapper.Pallets", "0", wrapper.Pallets);
			AssertEquals("wrapper.ContainerQuality", ZString.Empty, wrapper.ContainerQuality.Code);
			AssertEquals("wrapper.PrintTACImage", ZBool.False, wrapper.PrintTACImage);
			AssertEquals("wrapper.ExportDetention.Released", ZDateTime.Empty, wrapper.ExportDetention.Released);
			AssertEquals("wrapper.ImportDetention.Released", ZDateTime.Empty, wrapper.ImportDetention.Released);
			AssertEquals("wrapper.CFSClient", null, wrapper.CFSClient);
			AssertEquals("wrapper.UnpackShed", ZString.Empty, wrapper.UnpackShed);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromCusISFEquip(null, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 
ArrivalContainerYardAddress : 
CFSClient :  is null
ContainerQuality : 
DeliveryMode : 
DepartureContainerYardAddress : 
ExportDetention : 
FreightJob :  is null
ImportDetention : 
Mode : 
OffHirePort : 
OnHirePort : 
Owner : 
PackCount : 
Registry : (No Default Field Value Available on Registry)
SetPointTemperature : 
Status : 
Type : 
VGMMethod : 
VGMVerifiedByAddress : 
VolumeGoods : 
WeightDunnage : 
WeightGoods : 
WeightGross : 
WeightTare :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CusISFEquip container = Factory.New<CusISFEquip>();
			container.BE_ContainerNum = "TURE8648321";
			return new ContainerWrapperFromCusISFEquip(container, Factory);
		}
	}
}
