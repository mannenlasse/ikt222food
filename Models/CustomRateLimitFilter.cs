﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace foodbyte.Models;

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;


public class SampleRateLimiterPolicy : IRateLimiterPolicy<string>
{
    private Func<OnRejectedContext, CancellationToken, ValueTask>? _onRejected;
    private readonly MyRateLimitOptions _options;

    public SampleRateLimiterPolicy(ILogger<SampleRateLimiterPolicy> logger,
        IOptions<MyRateLimitOptions> options)
    {
        _onRejected = (ctx, token) =>
        {
            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            logger.LogWarning($"Request rejected by {nameof(SampleRateLimiterPolicy)}");
            return ValueTask.CompletedTask;
        };
        _options = options.Value;
    }

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => _onRejected;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        return RateLimitPartition.GetSlidingWindowLimiter(string.Empty,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = _options.PermitLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _options.QueueLimit,
                Window = TimeSpan.FromSeconds(_options.Window),
                SegmentsPerWindow = _options.SegmentsPerWindow
            });
    }
}