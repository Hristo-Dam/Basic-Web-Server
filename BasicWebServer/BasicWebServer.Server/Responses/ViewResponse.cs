
namespace BasicWebServer.Server.Responses
{
    public class ViewResponse : ContentResponse
    {
        private const char PATH_SEPARATOR = '/';

        public ViewResponse(string viewName, string controllerName, object model = null)
            : base("", ContentType.HTML)
        {
            if (!viewName.Contains(PATH_SEPARATOR))
            {
                viewName = controllerName + PATH_SEPARATOR + viewName;
            }

            var viewPath = Path.GetFullPath($"./Views/{viewName.TrimStart(PATH_SEPARATOR)}.cshtml");

            var viewContent = File.ReadAllText(viewPath);

            if (model != null)
            {
                viewContent = this.PopulateModel(viewContent, model);
            }

            this.Body = viewContent;
        }

        private string PopulateModel(string viewContent, object model)
        {
            var data = model
                .GetType()
                .GetProperties()
                .Select(pr => new
                {
                    pr.Name,
                    Value = pr.GetValue(model)
                });

            foreach (var entry in data)
            {
                const string OPENING_BRACKETS = "{{";
                const string CLOSING_BRACKETS = "}}";

                viewContent = viewContent
                    .Replace($"{OPENING_BRACKETS}{entry.Name}{CLOSING_BRACKETS}", entry.Value.ToString());
            }

            return viewContent;
        }
    }
}
