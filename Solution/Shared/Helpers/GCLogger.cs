using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Shared.Helpers;

public static class GCLogger
{
    private static readonly string[] GenerationNames = { "Gen0", "Gen1", "Gen2", "LOH", "POH" };

    public static void Log(ILogger logger)
    {
        var gc = GC.GetGCMemoryInfo();

        var sb = new StringBuilder();

        sb.AppendLine("========== GC Memory ==========");
        sb.AppendLine($"Managed Heap : {GC.GetTotalMemory(false) / MB:F2} MB");
        sb.AppendLine($"GC Index     : {gc.Index}");
        sb.AppendLine();

        sb.AppendLine("Collection counts:");
        sb.AppendLine($"Gen0 collections: {GC.CollectionCount(0)}");
        sb.AppendLine($"Gen1 collections: {GC.CollectionCount(1)}");
        sb.AppendLine($"Gen2 collections: {GC.CollectionCount(2)}");
        sb.AppendLine();

        sb.AppendLine("Generation information from the last GC:");

        logger.LogWarning("{GcMemoryReport}", sb.ToString());
    }

    private const double MB = 1024d * 1024d;
}
