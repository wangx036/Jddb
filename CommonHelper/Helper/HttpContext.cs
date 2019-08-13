using Microsoft.AspNetCore.Http;

namespace CommonHelper
{
    /// <summary>
    /// HttpContext注入类
    /// </summary>
    public static class MyHttpContext
    {
        private static IHttpContextAccessor _accessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _accessor = httpContextAccessor;
        }

        public static HttpContext HttpContext => _accessor.HttpContext;
    }
}