using BasicWebServer.Server.Common;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;

namespace BasicWebServer.Server.Routing
{
    public class RoutingTable : IRoutingTable
    {
        private readonly Dictionary<Method, Dictionary<string, Func<Request, Response>>> _routes;

        public RoutingTable()
        {
            this._routes = new Dictionary<Method, Dictionary<string, Func<Request, Response>>>()
            {
                [Method.GET] = new(),
                [Method.POST] = new(),
                [Method.PUT] = new(),
                [Method.DELETE] = new()
            };
        }

        public IRoutingTable Map(string path, Method method, Func<Request, Response> responseFunction)
        {
            Guard.AgainstNull(path, nameof(path));
            Guard.AgainstNull(responseFunction, nameof(responseFunction));

            this._routes[method][path] = responseFunction;

            return this;
        }

        public IRoutingTable MapGet(string path, Func<Request, Response> responseFunction)
            => this.Map(path, Method.GET, responseFunction);

        public IRoutingTable MapPost(string path, Func<Request, Response> responseFunction)
            => this.Map(path, Method.POST, responseFunction);

        public Response MatchRequest(Request request)
        {
            var requestMethod = request.Method;
            var requestUrl = request.Url;

            if (!this._routes.ContainsKey(requestMethod) || !this._routes[requestMethod].ContainsKey(requestUrl))
            {
                return new NotFoundResponse(); // 404
            }

            var responseFunction = this._routes[requestMethod][requestUrl];

            return responseFunction(request);
        }
    }
}
