using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.GUI;

public class RateLinesGridFilterStripBusinessObject : GridFilterStripBusinessObject
{
	public RateLinesGridFilterStripBusinessObject()
	{
	}

	public RateLinesGridFilterStripBusinessObject(ZGrid grid) : base(grid)
	{
		this.grid = grid;
	}

	readonly ZGrid grid;

	protected override void CreateNewCurrent(Type elementType, ref BusinessObject current)
	{
		if (elementType == typeof(RateLine))
		{
			current = grid.List.AddNew() as RateLine;
		}
	}
}
