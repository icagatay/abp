﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Tracing;

namespace Volo.Abp.AspNetCore.Tracing
{
    [Dependency(ReplaceServices = true)]
    public class AspNetCoreCorrelationIdProvider : ICorrelationIdProvider, ITransientDependency
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected AspNetCoreCorrelationIdOptions Options { get; }

        public AspNetCoreCorrelationIdProvider(
            IHttpContextAccessor httpContextAccessor,
            IOptions<AspNetCoreCorrelationIdOptions> options)
        {
            HttpContextAccessor = httpContextAccessor;
            Options = options.Value;
        }

        public virtual string Get()
        {
            if (HttpContextAccessor.HttpContext?.Request?.Headers == null)
            {
                return CreateNewCorrelationId();
            }

            lock (HttpContextAccessor.HttpContext.Request.Headers)
            {
                string correlationId = HttpContextAccessor.HttpContext.Request.Headers[Options.HeaderName];

                if (correlationId.IsNullOrEmpty())
                {
                    correlationId = CreateNewCorrelationId();
                    HttpContextAccessor.HttpContext.Request.Headers[Options.HeaderName] = correlationId;
                }

                return correlationId;
            }
        }

        protected virtual string CreateNewCorrelationId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
