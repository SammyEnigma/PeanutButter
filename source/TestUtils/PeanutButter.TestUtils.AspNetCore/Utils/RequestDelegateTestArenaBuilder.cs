﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Builders;
using Imported.PeanutButter.Utils;

namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Builds a RequestDelegateTestArena,
/// which makes testing middleware
/// a lot easier
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class RequestDelegateTestArenaBuilder
{
    /// <summary>
    /// Create the arena for fluent syntax usage
    /// </summary>
    /// <returns></returns>
    public static RequestDelegateTestArenaBuilder Create()
    {
        return new RequestDelegateTestArenaBuilder();
    }

    /// <summary>
    /// Builds the default RequestDelegateTestArena
    /// </summary>
    /// <returns></returns>
    public static RequestDelegateTestArena BuildDefault()
    {
        return Create().Build();
    }

    private Func<HttpContext, Task> _logic = NoOp;
    private readonly List<Action<HttpContextBuilder>> _contextMutators = new();
    private HttpContext _httpContext;

    private static Task NoOp(HttpContext ctx)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Build the arena
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public RequestDelegateTestArena Build()
    {
        return _httpContext is null
            ? new RequestDelegateTestArena(
                _logic,
                RunMutators
            )
            : new RequestDelegateTestArena(_logic, _httpContext);
    }


    private void RunMutators(HttpContextBuilder builder)
    {
        foreach (var mutator in _contextMutators)
        {
            mutator(builder);
        }
    }

    /// <summary>
    /// Generate the request delegate arena for a random OPTIONS request
    /// </summary>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder ForOptionsRequest()
    {
        return WithContextMutator(
            builder => builder.WithRequestMutator(
                r => r.Method = "OPTIONS"
            )
        );
    }

    /// <summary>
    /// Fluent mechanism for adding an http context mutation
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithContextMutator(
        Action<HttpContextBuilder> mutator
    )
    {
        if (mutator is not null)
        {
            _contextMutators.Add(mutator);
        }

        return this;
    }

    /// <summary>
    /// Add a mutation on the request for the context
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithRequestMutator(
        Action<HttpRequest> mutator
    )
    {
        if (mutator is not null)
        {
            _contextMutators.Add(
                builder =>
                    builder.WithRequestMutator(mutator)
            );
        }

        return this;
    }

    /// <summary>
    /// Add a mutation on the response for the context
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithResponseMutator(
        Action<HttpResponse> mutator
    )
    {
        if (mutator is not null)
        {
            _contextMutators.Add(
                builder => builder.WithResponseMutator(
                    mutator
                )
            );
        }

        return this;
    }

    /// <summary>
    /// Set the entire response for the context
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithResponse(
        HttpResponse response
    )
    {
        _contextMutators.Add(
            builder => builder.WithResponse(response)
        );
        return this;
    }

    /// <summary>
    /// Set the entire request for the context
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithRequest(
        HttpRequest request
    )
    {
        _contextMutators.Add(
            builder => builder.WithRequest(request)
        );
        return this;
    }

    /// <summary>
    /// Fluent mechanism for setting the delegate logic (overrides any existing logic)
    /// </summary>
    /// <param name="logic"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithDelegateLogic(
        Action<HttpContext> logic
    )
    {
        return WithDelegateLogic(RequestDelegateTestArena.WrapSynchronousLogic(logic));
    }

    private RequestDelegateTestArenaBuilder WithDelegateLogic(
        Func<HttpContext, Task> logic
    )
    {
        _logic = logic;
        return this;
    }

    /// <summary>
    /// Fluent mechanism for setting the HttpContext
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithContext(
        HttpContext context
    )
    {
        _httpContext = context;
        return this;
    }

    /// <summary>
    /// Sets the origin header on the request to be the root
    /// of the request url
    /// </summary>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithOriginHeader()
    {
        return WithRequestMutator(
            req => req.Headers["Origin"] = req.FullUrl().ToString().UriRoot()
        );
    }

    /// <summary>
    /// Sets the Origin header on the request to the provided value
    /// </summary>
    /// <param name="origin"></param>
    /// <returns></returns>
    public RequestDelegateTestArenaBuilder WithOriginHeader(
        string origin
    )
    {
        return WithRequestMutator(
            req => req.Headers["Origin"] = origin
        );
    }
}