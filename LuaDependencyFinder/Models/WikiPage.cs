namespace LuaDependencyFinder.Models
{
    public class WikiPage
    {
        public string Page { get; }

        public DateTime TimeStamp { get; }

        public string Contents { get; }

        public WikiPage(string page, DateTime timeStamp, string contents)
        {
            Page = page;
            TimeStamp = timeStamp;
            Contents = contents;
        }
    }
}
