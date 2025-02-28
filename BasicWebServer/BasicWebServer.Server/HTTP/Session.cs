using BasicWebServer.Server.Common;

namespace BasicWebServer.Server.HTTP
{
    public class Session
    {
        public const string SESSION_COOKIE_NAME = "MyWebServerSID";
        public const string SESSION_CURRENT_DATE_KEY = "CurrentDate";

        private Dictionary<string, string> _data;

        public Session(string id)
        {
            Guard.AgainstNull(id, nameof(id));

            this.Id = id;
            this._data = new Dictionary<string, string>();
        }

        public string Id { get; init; } // TODO: Should be Guid?
        public string this[string key]
        {
            get => this._data[key];
            set => this._data[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return this._data.ContainsKey(key);
        }
    }
}
