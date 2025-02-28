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
        private const string LOGIN_FORM = @"<form action='/Login' method='POST'>
   Username: <input type='text' name='Username'/>
   Password: <input type='text' name='Password'/>
   <input type='submit' value ='Log In' /> 
</form>";
        private const string USERNAME = "user";
        private const string PASSWORD = "user123";


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
                .MapGet("/Session", new TextResponse("", StartUp.DisplaySessionInfoAction))
                .MapGet("/Login", new HtmlResponse(StartUp.LOGIN_FORM))
                .MapPost("/Login", new HtmlResponse("", StartUp.LoginAction))
                .MapGet("/Logout", new HtmlResponse("", StartUp.LogoutAction))
                .MapGet("/UserProfile", new HtmlResponse("", StartUp.GetUserDataAction))
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
            bool requestHasCookies = request.Cookies
                .Any(c => c.Name != Session.SESSION_COOKIE_NAME);

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
        private static void DisplaySessionInfoAction(Request request, Response response)
        {
            bool sessionExists = request.Session
                .ContainsKey(Session.SESSION_CURRENT_DATE_KEY);

            string bodyText = string.Empty;

            if (sessionExists)
            {
                var currentDate = request.Session[Session.SESSION_CURRENT_DATE_KEY];
                bodyText = $"Stored date: {currentDate}!";
            }
            else
            {
                bodyText = "Current date stored!";
            }

            response.Body = string.Empty;
            response.Body += bodyText;
        }
        private static void LoginAction(Request request, Response response)
        {
            request.Session.Clear();

            string bodyText = string.Empty;

            bool usernameMatches = request.Form["Username"] == StartUp.USERNAME;
            bool passwordMatches = request.Form["Password"] == StartUp.PASSWORD;

            if (usernameMatches && passwordMatches)
            {
                request.Session[Session.SESSION_USER_KEY] = "MyUserId";

                response.Cookies.Add(Session.SESSION_COOKIE_NAME, request.Session.Id);

                bodyText = "<h3>Logged successfully!</h3>";
            }
            else
            {
                bodyText = StartUp.LOGIN_FORM;
            }

            response.Body = string.Empty;
            response.Body += bodyText;
        }
        private static void LogoutAction(Request request, Response response)
        {
            request.Session.Clear();

            response.Body = string.Empty;
            response.Body = "<h3>Logged out successfully!</h3>";
        }
        private static void GetUserDataAction(Request request, Response response)
        {
                response.Body = string.Empty;
            if (request.Session.ContainsKey(Session.SESSION_USER_KEY))
            {
                response.Body += @$"<h3>Currently logged-in user
is with username '{USERNAME}'</h3>";
            }
            else
            {
                response.Body += @$"<h3>You should first log in
- <a href='/Login'>Login</a></h3>";
            }
        }
    }
}
