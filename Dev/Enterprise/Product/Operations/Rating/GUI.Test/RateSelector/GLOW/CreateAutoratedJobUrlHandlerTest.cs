using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.CarrierConnect;

namespace Enterprise.Rating.GUI.Testing;

public class CreateAutoratedJobUrlHandlerTest : TestCaseWithDummy
{
	public void Test_DoesNotHandle_WhenInvalidQueryString() =>
		Assert(!CreateAutoratedJobUrlHandler.Instance.Handle(new QueryString { { "Invalid", "blah" } }));

	public void Test_DoesNotHandle_WhenNoRateResult() =>
		Assert(!CreateAutoratedJobUrlHandler.Instance.Handle(new QueryString { { "Id", Guid.NewGuid().ToString() } }));

	public void Test_DoesNotHandle_WhenInvalidDto()
	{
		// Arrange
		var factory = new BusinessObjectFactory();
		var result = factory.New<RateSearchResult>();
		result.StoreDto(new { Something = Guid.NewGuid().ToString() });

		// Assert
		Assert(!CreateAutoratedJobUrlHandler.Instance.Handle(new QueryString { { "Id",  result.PK.ToString() } }));
	}
}
