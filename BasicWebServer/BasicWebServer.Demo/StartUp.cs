using BasicWebServer.Server;
using BasicWebServer.Server.HTTP;
using BasicWebServer.Server.Responses;
using System.Text;
using System.Web;

namespace BasicWebServer.Demo
{
    public class StartUp
    {
        private const string HTML_FORM = @"<form action='/HTML' method='POST'>
   Name: <input type='text' name='Name'/>
   Age: <input type='number' name ='Age'/>
<input type='submit' value ='Save' />
</form>";
        private const string DOWNLOAD_FORM = @"<form action='/Content' method='POST'>
   <input type='submit' value ='Download Sites Content' /> 
</form>";
        private const string FILE_NAME = "content.txt";

        public static async Task Main()
        {
            await DownloadSitesAsTextFileAsync(StartUp.FILE_NAME,
                new string[] { "https://judge.softuni.org", "https://softuni.org" });

            HttpServer server = new HttpServer(routes => routes
                .MapGet("/", new TextResponse("Hello from the server!"))
                .MapGet("/Redirect", new RedirectResponse("https://www.softuni.com"))
                .MapGet("/HTML", new HtmlResponse(HTML_FORM))
                .MapPost("/HTML", new TextResponse("", StartUp.AddFormDataAction))
                .MapGet("/Content", new HtmlResponse(StartUp.DOWNLOAD_FORM))
                .MapPost("/Content", new TextFileResponse(StartUp.FILE_NAME))
                .MapGet("/Cookies", new HtmlResponse("", StartUp.AddCookiesAction))
            );

            await server.StartAsync();
        }
        private static async Task<string> DownloadWebSiteContentAsync(string url)
        {
            var httpClient = new HttpClient();
            using (httpClient)
            {
                var reponse = await httpClient.GetAsync(url);

                var html = await reponse.Content.ReadAsStringAsync();

                return html.Substring(0, 2000);
            }
        }
        private static async Task DownloadSitesAsTextFileAsync(string fileName, string[] urls)
        {
            var downloads = new List<Task<string>>();

            foreach (var url in urls)
            {
                downloads.Add(DownloadWebSiteContentAsync(url));
            }

            var responses  = await Task.WhenAll(downloads);

            var responsesString = string.Join(Environment.NewLine + new String('-', 100), responses);

            await File.WriteAllTextAsync(fileName, responsesString);
        }
        private static void AddFormDataAction(Request request, Response response)
        {
            response.Body = "";

            foreach (var (key, value) in request.Form)
            {
                response.Body += $"{key} - {value}";
                response.Body += Environment.NewLine;
            }
        }
        private static void AddCookiesAction(Request request, Response response)
        {
            bool requestHasCookies = request.Cookies.Any();
            string bodyText = string.Empty;

            if (requestHasCookies)
            {
                StringBuilder cookieText = new StringBuilder();

                cookieText.AppendLine("<h1>Cookies</h1>");

                cookieText.AppendLine(@"<table border='1'><tr><th>Name</th><th>Value</th></tr>");

                foreach (var cookie in request.Cookies)
                {
                    cookieText.Append("<tr>");

                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Name)}</td>");
                    cookieText.Append($"<td>{HttpUtility.HtmlEncode(cookie.Value)}</td>");

                    cookieText.Append("</tr>");
                }

                cookieText.Append("</table>");

                bodyText = cookieText.ToString();
            }
            else
            {
                bodyText = "<h1>No cookies yet!</h1>";
            }

            if (!requestHasCookies)
            {
                response.Cookies.Add("My-Cookie", "My-Value");
                response.Cookies.Add("My-Second-Cookie", "My-Second-Value");
            }

            response.Body = bodyText;
        }
    }
}
