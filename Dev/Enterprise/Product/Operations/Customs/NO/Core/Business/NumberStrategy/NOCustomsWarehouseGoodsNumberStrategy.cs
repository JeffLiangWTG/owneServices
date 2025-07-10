using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Business;

sealed class NOCustomsWarehouseGoodsNumberStrategy : ICustomsWarehouseGoodsNumberStrategy
{
	public NOCustomsWarehouseGoodsNumberStrategy(IDbConnected fountainConnection)
	{
		this.fountainConnection = Argument.NotNull(fountainConnection, nameof(fountainConnection));
	}

	readonly IDbConnected fountainConnection;

	public string GetCustomsWarehouseGoodsNumber(ZDateTime arrivalDate, string grantId)
	{
		if (!arrivalDate.IsValid)
		{
			throw new ArgumentException("Invalid arrival date.", nameof(arrivalDate));
		}

		using var transactionManager = fountainConnection.Connection.BeginTransactionWithManager();
		var numberFountain = GetNumberFountain(arrivalDate, grantId);
		var warehouseGoodsNumber = numberFountain.GetNextFormatted(fountainConnection);
		transactionManager.CommitTransaction();
		return warehouseGoodsNumber;
	}

	static INumberFountainProxy GetNumberFountain(ZDateTime arrivalDate, string grantId) => Env.NumberFountains.NOCustomsWarehouseGoodsNumber(arrivalDate.ToDateTime(), grantId);
}
