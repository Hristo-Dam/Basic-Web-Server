namespace BasicWebServer.Server.Responses
{
    public class ViewResponse : ContentResponse
    {
        private const char PATH_SEPARATOR = '/';

        public ViewResponse(string viewName, string controllerName)
            : base("", ContentType.HTML)
        {
            if (!viewName.Contains(PATH_SEPARATOR))
            {
                viewName = controllerName + PATH_SEPARATOR + viewName;
            }

            var viewPath = Path.GetFullPath($"./Views/{viewName.TrimStart(PATH_SEPARATOR)}.cshtml");

            var viewContent = File.ReadAllText(viewPath);

            this.Body = viewContent;
        }
    }
}
