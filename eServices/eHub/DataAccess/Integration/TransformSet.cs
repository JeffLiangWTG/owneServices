using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.eHub.DataAccess.Integration
{
  public class TransformSet : ICloneable
  {
    public TransformSet(List<TransformDetail> transforms, string xpathPredicate)
    {
      Transforms = transforms;
      XpathPredicate = xpathPredicate;
    }

    public readonly List<TransformDetail> Transforms;
    public readonly string XpathPredicate;

    public object Clone()
    {
      var transformsClone = this.Transforms.ToList();
      return new TransformSet(transformsClone, this.XpathPredicate);
    }

	public static implicit operator TransformSet(eServices.eHubDataAccess.Integration.TransformSet transformSet)
		=> transformSet is null ? null : new TransformSet(transformSet.Transforms?.Select(d => (TransformDetail)d).ToList(), transformSet.XpathPredicate);
	}
}
