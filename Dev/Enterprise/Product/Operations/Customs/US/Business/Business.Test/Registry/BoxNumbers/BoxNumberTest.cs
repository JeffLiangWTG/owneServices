using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BoxNumber))]
	sealed class BoxNumberTest : RegistryBusinessObjectTemplateTestCase<BoxNumber>
	{
		#region Test Validations

		public void TestValidateTransportMode()
		{
			BoxNumberCollection collection = CollectionRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			BoxNumber boxNo = collection.AddNew();
			BoxNumber boxNoDetails1 = collection[0];
			boxNoDetails1.TransportMode = BoxNoTransportModeList.Codes.ALL;
			boxNoDetails1.BoxNo = "756";
			AssertNoErrors(boxNoDetails1.TransportModeInfo);

			BoxNumber boxNo2 = collection.AddNew();
			BoxNumber boxNoDetails2 = collection[1];
			boxNoDetails2.TransportMode = "TRK";
			boxNoDetails2.BoxNo = "228";
			AssertHasErrorContaining(boxNoDetails2.TransportModeInfo, ListValidation.InvalidCodeError);

			boxNoDetails2.TransportMode = BoxNoTransportModeList.Codes.SEA;
			AssertNoErrorContaining(boxNoDetails2.TransportModeInfo, ListValidation.InvalidCodeError);

			boxNoDetails2.TransportMode = BoxNoTransportModeList.Codes.ALL;
			AssertHasErrorContaining(boxNoDetails2.TransportModeInfo, BoxNumber.DuplicateTransportMode);
		}

		public void TestValidateBoxNo()
		{
			BoxNumberCollection collection = CollectionRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			BoxNumber boxNo = collection.AddNew();
			BoxNumber boxNoDetails1 = collection[0];
			boxNoDetails1.TransportMode = "ALL";
			boxNoDetails1.BoxNo = "";
			AssertHasErrors(boxNoDetails1.BoxNoInfo);

			boxNoDetails1.BoxNo = "756";
			AssertNoErrors(boxNoDetails1.BoxNoInfo);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			BoxNumber result = new BoxNumber();
			result.TransportMode = "ALL";

			return result;
		}

		protected override BoxNumber GetBusinessObjectToClone()
		{
			return (BoxNumber)GetNewBusinessObject();
		}

		protected override BoxNumber GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		BoxNumberCollectionRegistryItem CollectionRegistryItem
		{
			get { return USCustomsDataRegistry.Instance.BoxNumbers; }
		}

		#endregion
	}
}
