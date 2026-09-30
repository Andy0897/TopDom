using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using TopDom.Filters;

namespace TopDom.Tests;

public class AnonymousOnlyAttributeTests
{
    private static ActionExecutingContext CreateContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            controller: new object());
    }

    [Fact]
    public void OnActionExecuting_AuthenticatedUser_RedirectsToHome()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Иван") }, "TestAuth");
        var context = CreateContext(new ClaimsPrincipal(identity));

        new AnonymousOnlyAttribute().OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Home", redirect.ControllerName);
    }

    [Fact]
    public void OnActionExecuting_AnonymousUser_DoesNotRedirect()
    {
        var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity()));

        new AnonymousOnlyAttribute().OnActionExecuting(context);

        Assert.Null(context.Result);
    }
}