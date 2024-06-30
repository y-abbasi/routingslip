using System.Collections.Immutable;

namespace Tiba.RoutingSlips.Builders;

public record CompensateLog(RoutingSlipActivity Activity, object LogData)
{
}