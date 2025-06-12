using static CargoWise.eHub.Products.GBCustoms.Core.Correlation.GBCustomsCorrelation;

namespace CargoWise.eHub.Products.GBCustoms.Core.Correlation
{
    public class GBCustomsCorrelationIdentifier
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public CorrelationType Type { get; set; }
    }
}
